using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Services;
using ABPGroup.CodeGen.Dto;
using ABPGroup.CodeGen.PromptTemplates;
using ABPGroup.Projects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABPGroup.CodeGen;

public class CodeGenEngine : DomainService, ICodeGenEngine
{
    private readonly ICodeGenAiService _aiService;
    private readonly ICodeGenPlanner _planner;
    private readonly ICodeGenScaffolder _scaffolder;
    private readonly IConfiguration _configuration;

    public CodeGenEngine(
        ICodeGenAiService aiService,
        ICodeGenPlanner planner,
        ICodeGenScaffolder scaffolder,
        IConfiguration configuration)
    {
        _aiService = aiService;
        _planner = planner;
        _scaffolder = scaffolder;
        _configuration = configuration;
    }

    public async Task<CodeGenResult> GenerateProjectAsync(
        CodeGenEngineInput input,
        Func<string, Task> onProgress,
        string currentDir = null,
        AppSpecDto approvedPlan = null,
        string approvedReadme = null)
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
        
        var requirements = await _planner.LoadRequirementsSnapshotAsync(input, rawApprovedPlan, approvedReadme, onProgress);
        var normalizedPlan = CodeGenHelpers.NormalizeSpec(rawApprovedPlan);

        result.ArchitectureSummary = requirements.ArchitectureSummary;

        if (onProgress != null) await onProgress("[2/5] Setting up project scaffold...");
        _scaffolder.AddScaffoldFiles(result.Files, input.Framework, currentDir);
        _scaffolder.AddApprovedReadmeFile(result.Files, approvedReadme);

        var scaffoldBaseline = BuildScaffoldBaseline(result.Files);
        var context = BuildGenerationContext(projectName, input, requirements, approvedReadme);

        var frontendResponse = await GenerateLayerAsync(
            "[3/5] Generating frontend...",
            "frontend pages and components",
            "Generate the frontend code",
            input,
            context,
            normalizedPlan,
            scaffoldBaseline,
            approvedReadme,
            onProgress);
        MergeLayerResponse(result, frontendResponse, true);

        var backendResponse = await GenerateLayerAsync(
            "[4/5] Generating backend...",
            "backend API routes and server logic",
            "Generate the backend code",
            input,
            context,
            normalizedPlan,
            scaffoldBaseline,
            approvedReadme,
            onProgress);
        MergeLayerResponse(result, backendResponse, false);

        var databaseResponse = await GenerateLayerAsync(
            "[5/5] Generating database layer...",
            "database schema and data access layer",
            "Generate the database layer",
            input,
            context,
            normalizedPlan,
            scaffoldBaseline,
            approvedReadme,
            onProgress);
        MergeLayerResponse(result, databaseResponse, false);

        result.OutputPath = outputPath;
        _scaffolder.WriteFilesToDisk(result.Files, outputPath);

        return result;
    }

    public async Task<string> GenerateLayerAsync(
        string progressLabel,
        string layerDescription,
        string userInstruction,
        CodeGenEngineInput input,
        System.Text.StringBuilder context,
        AppSpecDto approvedPlan,
        string scaffoldBaseline,
        string approvedReadme,
        Func<string, Task> onProgress)
    {
        await Task.Delay(CodeGenConstants.GenerationPhaseDelayMilliseconds);
        if (onProgress != null) await onProgress(progressLabel);

        return await _aiService.CallAiAsync(
            GeneratorPrompts.BuildCodeGenSystemPrompt(layerDescription, approvedPlan, input.Framework.ToString(), scaffoldBaseline, approvedReadme),
            GeneratorPrompts.BuildLayerUserPrompt(userInstruction, context, input.Prompt, approvedReadme));
    }

    public string BuildScaffoldBaseline(List<GeneratedFile> files)
    {
        if (files == null || files.Count == 0)
            return string.Empty;

        var baseline = new StringBuilder();

        foreach (var file in files
            .OrderBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
            .Take(CodeGenConstants.ScaffoldBaselineFileLimit))
        {
            if (baseline.Length >= CodeGenConstants.ScaffoldBaselineTotalLength)
                break;

            var snippet = file.Content ?? string.Empty;
            if (snippet.Length > CodeGenConstants.ScaffoldBaselineSnippetLength)
                snippet = snippet[..CodeGenConstants.ScaffoldBaselineSnippetLength];

            baseline.AppendLine($"FILE: {file.Path}");
            baseline.AppendLine(snippet);
            baseline.AppendLine("===END BASELINE FILE===");
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

    public void MergeLayerResponse(CodeGenResult result, string layerResponse, bool allowArchitectureOverride)
    {
        var files = CodeGenHelpers.ParseFiles(layerResponse);
        foreach (var file in files)
        {
            result.Files.Add(new GeneratedFile { Path = file.Path, Content = file.Content });
        }
        
        result.ModuleList.AddRange(CodeGenHelpers.ParseModules(layerResponse));

        if (!allowArchitectureOverride || !string.IsNullOrWhiteSpace(result.ArchitectureSummary))
            return;

        var architecture = CodeGenHelpers.ParseDelimitedSection(layerResponse, "ARCHITECTURE");
        if (!string.IsNullOrWhiteSpace(architecture))
            result.ArchitectureSummary = architecture;
    }

    private string BuildOutputPath(string projectName)
    {
        var outputBase = _configuration["CodeGen:OutputPath"]
            ?? Path.Combine(Path.GetTempPath(), "GeneratedApps");
        return Path.Combine(outputBase, projectName);
    }
}
