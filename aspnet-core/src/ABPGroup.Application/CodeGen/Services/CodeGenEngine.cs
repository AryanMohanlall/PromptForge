using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Services;
using ABPGroup.CodeGen.Dto;
using ABPGroup.CodeGen.PromptTemplates;
using ABPGroup.Projects;
using Microsoft.Extensions.Configuration;

namespace ABPGroup.CodeGen;

public class CodeGenEngine : DomainService, ICodeGenEngine
{
    private readonly ICodeGenAiService _aiService;
    private readonly ICodeGenPlanner _planner;
    private readonly ICodeGenScaffolder _scaffolder;
    private readonly ICodeGenBuildValidator _buildValidator;
    private readonly ICodeGenSessionManager _sessionManager;
    private readonly IConfiguration _configuration;

    public CodeGenEngine(
        ICodeGenAiService aiService,
        ICodeGenPlanner planner,
        ICodeGenScaffolder scaffolder,
        ICodeGenBuildValidator buildValidator,
        ICodeGenSessionManager sessionManager,
        IConfiguration configuration)
    {
        _aiService = aiService;
        _planner = planner;
        _scaffolder = scaffolder;
        _buildValidator = buildValidator;
        _sessionManager = sessionManager;
        _configuration = configuration;
    }

    public async Task<CodeGenResult> GenerateProjectAsync(
        CodeGenEngineInput input,
        StackConfigDto stack,
        Func<string, Task> onProgress,
        string currentDir = null,
        AppSpecDto approvedPlan = null,
        string approvedReadme = null,
        string sessionId = null)
    {
        var result = new CodeGenResult
        {
            GeneratedProjectId = input.Id,
            Files = new List<GeneratedFile>(),
            ModuleList = new List<string>()
        };

        var projectName = input.Name ?? $"project-{input.Id}";
        var outputPath = BuildOutputPath(projectName);
        var rawApprovedPlan = approvedPlan ?? new AppSpecDto();
        
        // Persist OutputPath to session if available
        if (!string.IsNullOrEmpty(sessionId))
        {
            try
            {
                var session = await _sessionManager.GetSessionAsync(sessionId);
                session.OutputPath = outputPath;
                await _sessionManager.SaveSessionAsync(session);
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to save OutputPath to session {sessionId}: {ex.Message}");
            }
        }

        var requirements = await _planner.LoadRequirementsSnapshotAsync(input, rawApprovedPlan, approvedReadme, onProgress);
        var normalizedPlan = CodeGenHelpers.NormalizeSpec(rawApprovedPlan);

        result.ArchitectureSummary = requirements.ArchitectureSummary;

        if (onProgress != null) await onProgress("[2/5] Setting up project scaffold...");
        _scaffolder.AddScaffoldFiles(result.Files, input.Framework, currentDir);
        _scaffolder.AddApprovedReadmeFile(result.Files, approvedReadme);

        var scaffoldBaseline = BuildScaffoldBaseline(result.Files);
        var context = BuildGenerationContext(projectName, input, requirements, approvedReadme);

        string previousLayerContext = "";

        // 1. Database/Schema Layer
        var databaseResponse = await GenerateLayerAsync(
            "[3/5] Generating database layer...",
            "database schema and data access layer",
            "Generate the database layer",
            input,
            stack,
            context,
            normalizedPlan,
            scaffoldBaseline,
            approvedReadme,
            onProgress,
            previousLayerContext);
        MergeLayerResponse(result, databaseResponse, false);
        previousLayerContext = CodeGenHelpers.ExtractLayerContracts(databaseResponse);

        // 2. Backend API Layer (sees full DB contracts: schema, types, lib)
        var backendResponse = await GenerateLayerAsync(
            "[4/5] Generating backend...",
            "backend API routes and server logic",
            "Generate the backend code",
            input,
            stack,
            context,
            normalizedPlan,
            scaffoldBaseline,
            approvedReadme,
            onProgress,
            previousLayerContext);
        MergeLayerResponse(result, backendResponse, false);
        previousLayerContext += "\n" + CodeGenHelpers.ExtractLayerContracts(backendResponse);

        // 3. Frontend UI Layer (sees DB + Backend contracts: schemas, API routes, types)
        var frontendResponse = await GenerateLayerAsync(
            "[5/5] Generating frontend...",
            "frontend pages and components",
            "Generate the frontend code",
            input,
            stack,
            context,
            normalizedPlan,
            scaffoldBaseline,
            approvedReadme,
            onProgress,
            previousLayerContext);
        MergeLayerResponse(result, frontendResponse, true);

        result.OutputPath = outputPath;
        _scaffolder.WriteFilesToDisk(result.Files, outputPath);

        if (onProgress != null) await onProgress("[6/6] Validating Build & Integrity...");
        var buildResult = await _buildValidator.ValidateBuildAsync(outputPath, input.Framework);

        result.ValidationResults.Add(new ValidationResultDto
        {
            Id = "build-passes",
            Status = buildResult.Success ? "passed" : "failed",
            Message = buildResult.Success ? "Build validation succeeded on the server." : "Build validation failed on the server. Check logs for details.",
            Logs = buildResult.Logs,
            Errors = buildResult.Errors
        });

        if (!buildResult.Success)
        {
            Logger.Warn($"Build validation failed for project {input.Id}. Errors: {string.Join(", ", buildResult.Errors)}");
            
            // Hard Validation: Update session status to failed
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    var session = await _sessionManager.GetSessionAsync(sessionId);
                    session.Status = (int)CodeGenStatus.ValidationFailed;
                    session.ValidationResultsJson = JsonSerializer.Serialize(result.ValidationResults);
                    await _sessionManager.SaveSessionAsync(session);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to update session status on build failure: {ex.Message}");
                }
            }
        }
        else
        {
            Logger.Info($"Build validation succeeded for project {input.Id}.");
        }

        return result;
    }

    public async Task<string> GenerateLayerAsync(
        string progressLabel,
        string layerDescription,
        string userInstruction,
        CodeGenEngineInput input,
        StackConfigDto stack,
        System.Text.StringBuilder context,
        AppSpecDto approvedPlan,
        string scaffoldBaseline,
        string approvedReadme,
        Func<string, Task> onProgress,
        string existingLayerMetadata = null)
    {
        await Task.Delay(CodeGenConstants.GenerationPhaseDelayMilliseconds);
        if (onProgress != null) await onProgress(progressLabel);

        return await _aiService.CallAiAsync(
            GeneratorPrompts.BuildCodeGenSystemPrompt(layerDescription, approvedPlan, stack, scaffoldBaseline, approvedReadme, existingLayerMetadata),
            GeneratorPrompts.BuildLayerUserPrompt(userInstruction, context, input.Prompt, approvedReadme));
    }

    public string BuildScaffoldBaseline(List<GeneratedFile> files)
    {
        if (files == null || files.Count == 0)
            return string.Empty;

        var baseline = new StringBuilder();
        var currentLength = 0;

        // Implementation of Structural Pruning:
        // 1. Always keep the first few critical files in full (e.g. README, package.json, main layout).
        // 2. If the baseline exceeds the threshold, switch to "Structural Summary" for remaining files.
        
        var criticalFiles = files
            .Where(f => f.Path.Contains("package.json", StringComparison.OrdinalIgnoreCase) ||
                        f.Path.Contains("tsconfig.json", StringComparison.OrdinalIgnoreCase) ||
                        f.Path.StartsWith("next.config", StringComparison.OrdinalIgnoreCase) ||
                        f.Path.Contains("layout.tsx", StringComparison.OrdinalIgnoreCase) ||
                        f.Path.Contains("README.md", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var otherFiles = files.Except(criticalFiles).OrderBy(f => f.Path, StringComparer.OrdinalIgnoreCase).ToList();
        var sortedFiles = criticalFiles.Concat(otherFiles).ToList();

        foreach (var file in sortedFiles.Take(CodeGenConstants.ScaffoldBaselineFileLimit))
        {
            if (currentLength >= CodeGenConstants.ScaffoldBaselineTotalLength)
            {
                // Threshold exceeded: Switch to structural summary for remaining files
                var remainingFiles = sortedFiles.SkipWhile(f => f != file).ToList();
                baseline.AppendLine("--- BASELINE THRESHOLD EXCEEDED: REMAINING FILES SUMMARIZED ---");
                baseline.AppendLine(CodeGenHelpers.SummarizeStructuralFiles(remainingFiles));
                break;
            }

            var snippet = file.Content ?? string.Empty;
            if (snippet.Length > CodeGenConstants.ScaffoldBaselineSnippetLength)
                snippet = snippet[..CodeGenConstants.ScaffoldBaselineSnippetLength];

            baseline.AppendLine($"FILE: {file.Path}");
            baseline.AppendLine(snippet);
            baseline.AppendLine("===END BASELINE FILE===");
            
            currentLength += snippet.Length;
        }

        return baseline.ToString();
    }

    public System.Text.StringBuilder BuildGenerationContext(
        string projectName,
        CodeGenEngineInput input,
        RequirementsSnapshot requirements,
        string approvedReadme)
    {
        var context = new StringBuilder();
        context.AppendLine($"Project: {projectName}");
        context.AppendLine($"Framework: {input.Framework}");
        context.AppendLine($"Language: {input.Language}");
        context.AppendLine($"Database: {input.DatabaseOption}");
        context.AppendLine($"Auth: {(input.IncludeAuth ? "Yes" : "No")}");
        context.AppendLine($"Features: {requirements.Features}");
        context.AppendLine($"Pages: {requirements.Pages}");
        context.AppendLine($"API Endpoints: {requirements.ApiEndpoints}");
        context.AppendLine($"DB Entities: {requirements.DbEntities}");
        context.AppendLine($"Architecture: {requirements.ArchitectureSummary}");

        if (!string.IsNullOrWhiteSpace(approvedReadme))
        {
            context.AppendLine("Approved README:");
            context.AppendLine(approvedReadme);
        }

        return context;
    }

    /// <summary>
    /// Scaffold config files that the AI must not overwrite — the scaffold versions are authoritative.
    /// </summary>
    private static readonly HashSet<string> ProtectedScaffoldFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "tsconfig.json",
        "next.config.mjs",
        "next.config.js",
        "postcss.config.mjs",
        "eslint.config.mjs",
        ".gitignore"
    };

    public void MergeLayerResponse(CodeGenResult result, string layerResponse, bool allowArchitectureOverride)
    {
        // Parse using new JSON envelope format (with legacy fallback)
        var output = CodeGenHelpers.ParseGeneratorOutput(layerResponse);

        // Merge files
        foreach (var file in output.Files)
        {
            var normalizedPath = CodeGenHelpers.NormalizeFilePath(file.Path);
            var existing = result.Files.FirstOrDefault(f =>
                string.Equals(CodeGenHelpers.NormalizeFilePath(f.Path), normalizedPath, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                // Never let AI-generated output overwrite protected scaffold config files
                // (tsconfig.json, next.config.mjs, etc.) — the scaffold versions are authoritative
                if (ProtectedScaffoldFiles.Contains(normalizedPath))
                {
                    Logger.Info($"Skipping AI override of protected scaffold file: {file.Path}");
                    continue;
                }
                existing.Content = file.Content;
            }
            else
            {
                result.Files.Add(new GeneratedFile { Path = file.Path, Content = file.Content });
            }
        }

        // Merge modules
        if (output.Modules != null && output.Modules.Count > 0)
        {
            result.ModuleList.AddRange(output.Modules);
        }

        // Store self-check results
        if (output.SelfCheck != null)
        {
            var failedChecks = CodeGenHelpers.GetFailedChecks(output.SelfCheck);
            if (failedChecks.Count > 0)
            {
                Logger.Warn($"Self-check reported {failedChecks.Count} failing rules: {string.Join(", ", failedChecks.Select(c => c.Rule))}");
                foreach (var check in failedChecks)
                {
                    result.ValidationResults.Add(new ValidationResultDto
                    {
                        Id = $"self-check-{check.Rule}",
                        Status = "failed",
                        Message = $"[Self-Check] {check.Rule}: {check.Notes}"
                    });
                }
            }

            // Check for missing required files
            var generatedPaths = output.Files.Select(f => f.Path).ToList();
            var missingFiles = CodeGenHelpers.GetMissingRequiredFiles(output.RequiredFiles, generatedPaths);
            if (missingFiles.Count > 0)
            {
                Logger.Warn($"AI committed to generating files that are missing: {string.Join(", ", missingFiles)}");
                foreach (var missing in missingFiles)
                {
                    result.ValidationResults.Add(new ValidationResultDto
                    {
                        Id = $"missing-required-{CodeGenHelpers.Slugify(missing)}",
                        Status = "failed",
                        Message = $"Required file missing from AI output: {missing}"
                    });
                }
            }
        }

        // Architecture summary (from JSON or legacy)
        if (!allowArchitectureOverride || !string.IsNullOrWhiteSpace(result.ArchitectureSummary))
            return;

        if (!string.IsNullOrWhiteSpace(output.Architecture))
        {
            result.ArchitectureSummary = output.Architecture;
        }
        else
        {
            var architecture = CodeGenHelpers.ParseDelimitedSection(layerResponse, "ARCHITECTURE");
            if (!string.IsNullOrWhiteSpace(architecture))
                result.ArchitectureSummary = architecture;
        }
    }

    /// <summary>
    /// Loads a few-shot example from the configured path.
    /// </summary>
    public string LoadFewShotExample(string layerDescription = null)
    {
        try
        {
            var fewShotDir = _configuration["CodeGen:FewShotExamplesPath"];
            if (string.IsNullOrWhiteSpace(fewShotDir) || !Directory.Exists(fewShotDir))
                return null;

            // Try to find a matching example file
            var files = Directory.GetFiles(fewShotDir, "*.json");
            if (files.Length == 0)
                return null;

            // Use first available example (could be enhanced to match by layer type)
            var examplePath = files[0];
            var json = File.ReadAllText(examplePath);
            
            // Validate it's parseable
            using var doc = JsonDocument.Parse(json);
            return json;
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load few-shot example: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Loads known failures from session history for a given project.
    /// </summary>
    public string LoadKnownFailures(string sessionId = null)
    {
        if (string.IsNullOrEmpty(sessionId))
            return null;

        try
        {
            var session = _sessionManager.GetSessionAsync(sessionId).GetAwaiter().GetResult();
            if (session?.ValidationResultsJson == null)
                return null;

            var results = System.Text.Json.JsonSerializer.Deserialize<List<ValidationResultDto>>(
                session.ValidationResultsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (results == null)
                return null;

            var failures = results.Where(r => r.Status == "failed").ToList();
            if (failures.Count == 0)
                return null;

            var sb = new StringBuilder();
            foreach (var failure in failures)
            {
                sb.AppendLine($"- [{failure.Id}] {failure.Message}");
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load known failures: {ex.Message}");
            return null;
        }
    }

    private string BuildOutputPath(string projectName)
    {
        var outputBase = _configuration["CodeGen:OutputPath"]
            ?? Path.Combine(Path.GetTempPath(), "GeneratedApps");
        
        var sanitized = CodeGenHelpers.SanitizeDirName(projectName);
        if (sanitized.Length > 32)
        {
            sanitized = sanitized[..24] + "-" + CodeGenHelpers.Hash(projectName)[..7];
        }
        
        return Path.Combine(outputBase, sanitized);
    }
}
