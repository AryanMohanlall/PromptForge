using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Domain.Services;
using Abp.UI;
using ABPGroup.CodeGen.Dto;
using ABPGroup.CodeGen.PromptTemplates;
using Microsoft.Extensions.Logging;

namespace ABPGroup.CodeGen;

public class CodeGenRefiner : DomainService, ICodeGenRefiner
{
    private readonly ICodeGenAiService _aiService;
    private readonly ICodeGenSessionManager _sessionManager;
    private readonly ICodeGenValidator _validator;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CodeGenRefiner(
        ICodeGenAiService aiService,
        ICodeGenSessionManager sessionManager,
        ICodeGenValidator validator)
    {
        _aiService = aiService;
        _sessionManager = sessionManager;
        _validator = validator;
    }

    public async Task<CodeGenSessionDto> RepairAsync(TriggerRepairInput input)
    {
        var session = await _sessionManager.GetSessionAsync(input.SessionId);
        session.RepairAttempts++;

        var currentFiles = DeserializeOrDefault<List<GeneratedFileDto>>(session.GeneratedFilesJson) ?? new List<GeneratedFileDto>();
        var spec = LoadStoredSpec(session.SpecJson) ?? new AppSpecDto();
        var stack = DeserializeOrDefault<StackConfigDto>(session.ConfirmedStackJson);
        var repairFailures = input.Failures?.Where(f => f?.Status == "failed").ToList() ?? new List<ValidationResultDto>();
        var affectedPaths = BuildRepairAffectedPaths(repairFailures, spec, stack);

        var repairPrompt = RepairPrompts.BuildRepairPrompt(repairFailures, spec, currentFiles, affectedPaths);

        var response = await _aiService.CallAiAsync(
            "You are an expert code repair agent specializing in targeted, minimal diffs.",
            repairPrompt);

        var repairedFiles = ParseFilesToDtos(response);
        var deletedFiles = ParseDeletedFiles(response);
        var mergedFiles = MergeRefinementResults(currentFiles, repairedFiles, deletedFiles);
        
        var validationRules = spec.Validations ?? new List<ValidationRuleDto>();
        var validationResults = _validator.EvaluateValidationResults(validationRules, ConvertToFiles(mergedFiles), stack);
        var hasFailures = validationResults.Any(v => v.Status == "failed");

        session.GeneratedFilesJson = JsonSerializer.Serialize(mergedFiles, JsonOptions);
        session.ValidationResultsJson = JsonSerializer.Serialize(validationResults, JsonOptions);
        session.GenerationMode = "repair";
        session.Status = hasFailures
            ? (int)CodeGenStatus.ValidationFailed
            : (int)CodeGenStatus.ValidationPassed;
        
        // session.CurrentPhase = hasFailures ? "repair-validation-failed" : "repair-complete"; // CurrentPhase is not in CodeGenSession yet?
        session.ErrorMessage = null;
        session.UpdatedAt = DateTime.UtcNow;
        
        await _sessionManager.SaveSessionAsync(session);
        return _sessionManager.MapToDto(session);
    }

    public async Task<RefinementResultDto> RefineSessionAsync(RefinementInputDto input)
    {
        var session = await _sessionManager.GetSessionAsync(input.SessionId);
        if (session.Status < (int)CodeGenStatus.ValidationPassed)
            throw new UserFriendlyException("Session must have completed generation before refinement.");

        var currentFileDtos = DeserializeOrDefault<List<GeneratedFileDto>>(session.GeneratedFilesJson) ?? new List<GeneratedFileDto>();
        var spec = LoadStoredSpec(session.SpecJson);

        var prompt = RefinementPrompts.BuildDiffPrompt(
            input.ChangeRequest,
            spec,
            currentFileDtos,
            input.AffectedFiles);

        string response;
        try
        {
            response = await _aiService.CallAiAsync(
                "You are an expert code refactoring agent specializing in targeted, minimal changes.",
                prompt);
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"AI service call failed: {ex.Message}");
        }

        var summary = CodeGenHelpers.ParseDelimitedSection(response, "SUMMARY")?.Trim() ?? "Refinement applied.";
        var changedFiles = ParseFilesToDtos(response);
        var deletedFiles = ParseDeletedFiles(response);
        var mergedFiles = MergeRefinementResults(currentFileDtos, changedFiles, deletedFiles);

        session.GeneratedFilesJson = JsonSerializer.Serialize(mergedFiles, JsonOptions);

        var history = DeserializeOrDefault<List<RefinementHistoryEntry>>(session.RefinementHistoryJson) ?? new List<RefinementHistoryEntry>();
        history.Add(new RefinementHistoryEntry
        {
            Timestamp = DateTime.UtcNow,
            ChangeRequest = input.ChangeRequest,
            ChangedFiles = changedFiles.Select(f => f.Path).ToList(),
            DeletedFiles = deletedFiles
        });
        session.RefinementHistoryJson = JsonSerializer.Serialize(history, JsonOptions);
        session.GenerationMode = "refinement";
        session.UpdatedAt = DateTime.UtcNow;

        await _sessionManager.SaveSessionAsync(session);

        var specValidations = spec.Validations ?? new List<ValidationRuleDto>();
        var mergedFilesForValidation = ConvertToFiles(mergedFiles);
        var stack = DeserializeOrDefault<StackConfigDto>(session.ConfirmedStackJson);
        var validationResults = _validator.EvaluateValidationResults(specValidations, mergedFilesForValidation, stack);

        return new RefinementResultDto
        {
            ChangedFiles = changedFiles,
            DeletedFiles = deletedFiles,
            Summary = summary,
            ValidationResults = validationResults
        };
    }

    private static List<GeneratedFileDto> MergeRefinementResults(
        List<GeneratedFileDto> original,
        List<GeneratedFileDto> changed,
        List<string> deleted)
    {
        var result = new List<GeneratedFileDto>(original);

        foreach (var deletedPath in deleted)
        {
            result.RemoveAll(f => string.Equals(CodeGenHelpers.NormalizeFilePath(f.Path), CodeGenHelpers.NormalizeFilePath(deletedPath), StringComparison.OrdinalIgnoreCase));
        }

        foreach (var changedFile in changed)
        {
            var existing = result.FirstOrDefault(f => string.Equals(CodeGenHelpers.NormalizeFilePath(f.Path), CodeGenHelpers.NormalizeFilePath(changedFile.Path), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.Content = changedFile.Content;
            }
            else
            {
                result.Add(changedFile);
            }
        }

        return result;
    }

    private static List<GeneratedFileDto> ParseFilesToDtos(string response)
    {
        var files = CodeGenHelpers.ParseFiles(response);
        return files.Select(f => new GeneratedFileDto { Path = f.Path, Content = f.Content }).ToList();
    }

    private static List<string> ParseDeletedFiles(string response)
    {
        var deletedSection = CodeGenHelpers.ParseDelimitedSection(response, "DELETED");
        return CodeGenHelpers.ParseCsvList(deletedSection);
    }

    private static List<GeneratedFile> ConvertToFiles(List<GeneratedFileDto> dtos)
    {
        return dtos.Select(d => new GeneratedFile { Path = d.Path, Content = d.Content }).ToList();
    }

    private static List<string> BuildRepairAffectedPaths(List<ValidationResultDto> failures, AppSpecDto spec, StackConfigDto stack)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var specValidations = spec?.Validations ?? new List<ValidationRuleDto>();

        foreach (var failure in failures)
        {
            var rule = specValidations.FirstOrDefault(v => v.Id == failure.Id);
            
            // First, try to use the specific Target from the rule
            if (rule != null && !string.IsNullOrWhiteSpace(rule.Target))
            {
                paths.Add(rule.Target);
            }
            
            // Next, extract a route hint based on the rule's original assertion, or the failure message
            var assertionToCheck = rule?.Assertion ?? failure.Message;
            var hint = CodeGenHelpers.ExtractRouteHint(new ValidationRuleDto { Id = failure.Id, Target = rule?.Target, Assertion = assertionToCheck });
            if (!string.IsNullOrWhiteSpace(hint)) paths.Add(hint);
        }
        return paths.ToList();
    }

    private static T DeserializeOrDefault<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
        catch { return null; }
    }

    private static AppSpecDto LoadStoredSpec(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        var readmePackage = DeserializeOrDefault<ReadmeResultDto>(json);
        if (!string.IsNullOrWhiteSpace(readmePackage?.ReadmeMarkdown))
        {
            return CodeGenHelpers.NormalizeSpec(readmePackage.Plan ?? new AppSpecDto());
        }
        return CodeGenHelpers.NormalizeSpec(DeserializeOrDefault<AppSpecDto>(json) ?? new AppSpecDto());
    }
}
