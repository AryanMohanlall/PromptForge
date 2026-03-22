using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Services;
using Abp.UI;
using ABPGroup.CodeGen.Dto;
using ABPGroup.CodeGen.PromptTemplates;
using Microsoft.Extensions.Logging;

namespace ABPGroup.CodeGen;

public class CodeGenPlanner : DomainService, ICodeGenPlanner
{
    private readonly ICodeGenAiService _aiService;
    private readonly ICodeGenSessionManager _sessionManager;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CodeGenPlanner(
        ICodeGenAiService aiService,
        ICodeGenSessionManager sessionManager)
    {
        _aiService = aiService;
        _sessionManager = sessionManager;
    }

    public async Task<RequirementsSnapshot> LoadRequirementsSnapshotAsync(
        CodeGenEngineInput input,
        AppSpecDto approvedPlan,
        string approvedReadme,
        Func<string, Task> onProgress)
    {
        if (HasUsableSpec(approvedPlan))
        {
            if (onProgress != null) await onProgress("[1/5] Using approved README and implementation plan...");
            return BuildRequirementsSnapshotFromPlan(approvedPlan, approvedReadme);
        }

        return await AnalyzeRequirementsAsync(input, onProgress);
    }

    public async Task<RequirementsSnapshot> AnalyzeRequirementsAsync(
        CodeGenEngineInput input,
        Func<string, Task> onProgress)
    {
        if (onProgress != null) await onProgress("[1/5] Analyzing requirements...");
        
        var requirementsPrompt = BuildRequirementsPrompt(input);
        var requirementsResponse = await _aiService.CallAiAsync(
            "You are an expert software architect. Analyze the user's requirements and return a structured breakdown.",
            requirementsPrompt);

        return new RequirementsSnapshot
        {
            ArchitectureSummary = CodeGenHelpers.ParseDelimitedSection(requirementsResponse, "ARCHITECTURE"),
            Features = CodeGenHelpers.ParseDelimitedSection(requirementsResponse, "FEATURES"),
            Pages = CodeGenHelpers.ParseDelimitedSection(requirementsResponse, "PAGES"),
            ApiEndpoints = CodeGenHelpers.ParseDelimitedSection(requirementsResponse, "API_ENDPOINTS"),
            DbEntities = CodeGenHelpers.ParseDelimitedSection(requirementsResponse, "DB_ENTITIES")
        };
    }

    public async Task<ReadmeResultDto> GenerateReadmeAsync(string sessionId)
    {
        var session = await _sessionManager.GetSessionAsync(sessionId);
        var stack = _sessionManager.MapToDto(session).ConfirmedStack ?? new StackConfigDto();
        var features = JsonSerializer.Deserialize<List<string>>(session.DetectedFeaturesJson ?? "[]") ?? new List<string>();
        var entities = JsonSerializer.Deserialize<List<string>>(session.DetectedEntitiesJson ?? "[]") ?? new List<string>();

        var prompt = PlannerPrompts.BuildReadmePrompt(
            session.ProjectName,
            session.NormalizedRequirement,
            stack,
            features,
            entities);

        var response = await _aiService.CallAiAsync(
            "You are an expert technical writer and software architect. Create a comprehensive README.md for the following project.",
            prompt);

        var readmeMarkdown = CodeGenHelpers.ParseDelimitedSection(response, "README");
        var summary = CodeGenHelpers.ParseDelimitedSection(response, "SUMMARY") ?? "Strategic implementation plan for your application.";

        var plan = await GeneratePlanFromReadmeAsync(readmeMarkdown, stack, session, features, entities);

        var result = new ReadmeResultDto
        {
            ReadmeMarkdown = readmeMarkdown,
            Summary = summary,
            Plan = plan
        };

        // Persist the combined package
        session.SpecJson = JsonSerializer.Serialize(result, JsonOptions);
        session.UpdatedAt = DateTime.UtcNow;
        await _sessionManager.SaveSessionAsync(session);

        return result;
    }

    public async Task<AppSpecDto> GeneratePlanFromReadmeAsync(
        string readmeMarkdown,
        StackConfigDto stack,
        CodeGenSession session,
        List<string> features,
        List<string> entities)
    {
        try
        {
            var response = await _aiService.CallAiAsync(
                "You are an expert software architect. Convert the approved README into a concrete implementation plan.",
                PlannerPrompts.BuildPlanFromReadmePrompt(
                    readmeMarkdown,
                    stack,
                    session?.NormalizedRequirement ?? session?.Prompt ?? string.Empty,
                    features,
                    entities));

            var specJson = CodeGenHelpers.ParseDelimitedSection(response, "SPEC_JSON")?.Trim();
            var plan = CodeGenHelpers.ParseSpecOrDefault(specJson, out var parseWarning);
            
            if (!string.IsNullOrWhiteSpace(parseWarning))
                Logger.Warn(parseWarning);

            return EnrichReadmePlan(
                CodeGenHelpers.NormalizeSpec(plan),
                readmeMarkdown,
                session,
                features,
                entities);
        }
        catch (Exception ex)
        {
            Logger.Error($"GeneratePlanFromReadme: Failed to derive implementation plan", ex);
            throw new UserFriendlyException($"Failed to derive an implementation plan from the README: {ex.Message}");
        }
    }

    public RequirementsSnapshot BuildRequirementsSnapshotFromPlan(
        AppSpecDto approvedPlan,
        string approvedReadme)
    {
        var normalizedPlan = CodeGenHelpers.NormalizeSpec(approvedPlan ?? new AppSpecDto());

        var architectureSummary = !string.IsNullOrWhiteSpace(normalizedPlan.ArchitectureNotes)
            ? normalizedPlan.ArchitectureNotes
            : "Use the approved README and reviewed implementation plan as the source of truth.";

        if (!string.IsNullOrWhiteSpace(approvedReadme))
        {
            architectureSummary = architectureSummary + "\nReviewed README is approved for scaffolding.";
        }

        return new RequirementsSnapshot
        {
            ArchitectureSummary = architectureSummary,
            Features = BuildFeatureSummary(normalizedPlan),
            Pages = string.Join(", ", normalizedPlan.Pages.Select(p => p.Route)),
            ApiEndpoints = string.Join(", ", normalizedPlan.ApiRoutes.Select(r => $"{r.Method} {r.Path}")),
            DbEntities = string.Join(", ", normalizedPlan.Entities.Select(BuildEntitySummary))
        };
    }

    private static string BuildRequirementsPrompt(CodeGenEngineInput input)
    {
        return $"Project: {input.Name}\n" +
               $"Framework: {input.Framework}\n" +
               $"Language: {input.Language}\n" +
               $"Database: {input.DatabaseOption}\n" +
               $"Requirements: {input.Prompt}";
    }

    private static string BuildFeatureSummary(AppSpecDto spec)
    {
        var sb = new StringBuilder();
        if (spec.Entities.Count > 0) sb.Append($"{spec.Entities.Count} entities, ");
        if (spec.Pages.Count > 0) sb.Append($"{spec.Pages.Count} pages, ");
        if (spec.ApiRoutes.Count > 0) sb.Append($"{spec.ApiRoutes.Count} API routes");
        return sb.ToString().TrimEnd(' ', ',');
    }

    private static string BuildEntitySummary(EntitySpecDto entity)
    {
        var fields = string.Join(", ", entity.Fields.Select(f => f.Name));
        return $"{entity.Name}({fields})";
    }

    private static bool HasUsableSpec(AppSpecDto spec)
    {
        return spec?.Entities?.Count > 0
            || spec?.ApiRoutes?.Count > 0
            || spec?.Pages?.Count > 0;
    }

    private AppSpecDto EnrichReadmePlan(
        AppSpecDto plan,
        string readmeMarkdown,
        CodeGenSession session,
        List<string> features,
        List<string> entities)
    {
        if (plan.Entities.Count == 0 && (entities?.Count > 0 || !string.IsNullOrWhiteSpace(session?.NormalizedRequirement)))
        {
            var detectedEntities = entities ?? new List<string>();
            if (detectedEntities.Count == 0)
            {
                var inferred = CodeGenHelpers.InferTodoEntityName(session?.NormalizedRequirement);
                if (!string.IsNullOrWhiteSpace(inferred)) detectedEntities.Add(inferred);
            }

            foreach (var entityName in detectedEntities)
            {
                plan.Entities.Add(CodeGenHelpers.BuildFallbackEntitySpec(entityName));
            }
        }

        if (plan.Validations.Count == 0)
        {
            plan.Validations = CodeGenHelpers.BuildFallbackValidations(plan);
        }

        CodeGenHelpers.EnsureHomePage(plan.Pages);

        return plan;
    }
}
