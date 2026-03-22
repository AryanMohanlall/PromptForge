using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.UI;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen;

public class CodeGenSessionManager : DomainService, ICodeGenSessionManager
{
    private readonly IRepository<CodeGenSession, Guid> _sessionRepository;
    private readonly ICodeGenAiService _aiService;
    private readonly IUnitOfWorkManager _uowManager;
    
    private static readonly ConcurrentDictionary<string, CodeGenSession> InMemorySessions = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CodeGenSessionManager(
        IRepository<CodeGenSession, Guid> sessionRepository,
        ICodeGenAiService aiService,
        IUnitOfWorkManager uowManager)
    {
        _sessionRepository = sessionRepository;
        _aiService = aiService;
        _uowManager = uowManager;
    }

    public async Task<CodeGenSession> GetSessionAsync(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var guid))
            throw new UserFriendlyException("Invalid session ID.");

        CodeGenSession session = null;

        if (_sessionRepository != null)
        {
            using (var uow = _uowManager.Begin())
            {
                session = await _sessionRepository.FirstOrDefaultAsync(guid);
                await uow.CompleteAsync();
            }
        }
        else
        {
            InMemorySessions.TryGetValue(sessionId, out session);
        }

        if (session == null)
            throw new UserFriendlyException($"Session '{sessionId}' not found.");

        return session;
    }

    public async Task<CodeGenSessionDto> GetSessionDtoAsync(string sessionId)
    {
        var session = await GetSessionAsync(sessionId);
        return MapToDto(session);
    }

    public async Task SaveSessionAsync(CodeGenSession session, bool isNew = false)
    {
        if (_sessionRepository != null)
        {
            using (var uow = _uowManager.Begin())
            {
                if (isNew)
                {
                    await _sessionRepository.InsertAsync(session);
                }
                else
                {
                    await _sessionRepository.UpdateAsync(session);
                }
                await uow.CompleteAsync();
            }
        }
        else
        {
            InMemorySessions[session.Id.ToString()] = session;
        }
    }

    public System.Linq.IQueryable<CodeGenSession> GetSessionQuery()
    {
        return _sessionRepository.GetAll();
    }

    public async Task<CodeGenSessionDto> CreateSessionAsync(CreateSessionInput input, long? userId = null)
    {
        var session = new CodeGenSession
        {
            Id = Guid.NewGuid(),
            Prompt = input.Prompt,
            Status = (int)CodeGenStatus.Captured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UserId = userId
        };

        // Call LLM to analyze requirements
        string response;
        try
        {
            response = await _aiService.CallAiAsync(
                "You are an expert software architect. Analyze the user's application idea and extract structured information. "
                + "Return your analysis in the following delimited format:\n"
                + "===PROJECT_NAME===\n<suggested project name>\n===END PROJECT_NAME===\n"
                + "===NORMALIZED_REQUIREMENT===\n<clear, detailed restatement of the requirement>\n===END NORMALIZED_REQUIREMENT===\n"
                + "===DETECTED_FEATURES===\n<comma-separated list of features>\n===END DETECTED_FEATURES===\n"
                + "===DETECTED_ENTITIES===\n<comma-separated list of data entities>\n===END DETECTED_ENTITIES===\n",
                input.Prompt);
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"AI service call failed: {ex.Message}");
        }

        session.ProjectName = CodeGenHelpers.ParseDelimitedSection(response, "PROJECT_NAME")?.Trim() ?? "untitled-app";
        session.NormalizedRequirement = CodeGenHelpers.ParseDelimitedSection(response, "NORMALIZED_REQUIREMENT")?.Trim() ?? input.Prompt;
        session.DetectedFeaturesJson = JsonSerializer.Serialize(
            CodeGenHelpers.ParseCsvList(CodeGenHelpers.ParseDelimitedSection(response, "DETECTED_FEATURES")), JsonOptions);
        session.DetectedEntitiesJson = JsonSerializer.Serialize(
            CodeGenHelpers.ParseCsvList(CodeGenHelpers.ParseDelimitedSection(response, "DETECTED_ENTITIES")), JsonOptions);

        await SaveSessionAsync(session, isNew: true);

        return MapToDto(session);
    }

    public CodeGenSessionDto MapToDto(CodeGenSession session)
    {
        return new CodeGenSessionDto
        {
            Id = session.Id.ToString(),
            UserId = session.UserId ?? 0,
            ProjectId = session.ProjectId,
            ProjectName = session.ProjectName,
            Prompt = session.Prompt,
            NormalizedRequirement = session.NormalizedRequirement,
            DetectedFeatures = DeserializeOrDefault<List<string>>(session.DetectedFeaturesJson) ?? new List<string>(),
            DetectedEntities = DeserializeOrDefault<List<string>>(session.DetectedEntitiesJson) ?? new List<string>(),
            ConfirmedStack = DeserializeOrDefault<StackConfigDto>(session.ConfirmedStackJson),
            Spec = LoadStoredSpec(session.SpecJson),
            SpecConfirmedAt = session.SpecConfirmedAt,
            GenerationStartedAt = session.GenerationStartedAt,
            GenerationCompletedAt = session.GenerationCompletedAt,
            Status = session.Status,
            ValidationResults = DeserializeOrDefault<List<ValidationResultDto>>(session.ValidationResultsJson) ?? new List<ValidationResultDto>(),
            ScaffoldTemplate = session.ScaffoldTemplate,
            GeneratedFiles = DeserializeOrDefault<List<GeneratedFileDto>>(session.GeneratedFilesJson) ?? new List<GeneratedFileDto>(),
            RepairAttempts = session.RepairAttempts,
            IsPublic = session.IsPublic,
            GenerationMode = session.GenerationMode,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt
        };
    }

    private static T DeserializeOrDefault<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<T>(json, JsonOptions); }
        catch { return null; }
    }

    private static AppSpecDto LoadStoredSpec(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        var blueprint = LoadGenerationBlueprint(json);
        return CodeGenHelpers.NormalizeSpec(blueprint.Spec ?? new AppSpecDto());
    }

    private static GenerationBlueprint LoadGenerationBlueprint(string json)
    {
        var readmePackage = DeserializeOrDefault<ReadmeResultDto>(json);
        if (!string.IsNullOrWhiteSpace(readmePackage?.ReadmeMarkdown))
        {
            return new GenerationBlueprint
            {
                Spec = readmePackage.Plan ?? new AppSpecDto(),
                ReadmeMarkdown = readmePackage.ReadmeMarkdown
            };
        }

        return new GenerationBlueprint
        {
            Spec = DeserializeOrDefault<AppSpecDto>(json) ?? new AppSpecDto(),
            ReadmeMarkdown = null
        };
    }

    private class GenerationBlueprint
    {
        public AppSpecDto Spec { get; set; }
        public string ReadmeMarkdown { get; set; }
    }
}
