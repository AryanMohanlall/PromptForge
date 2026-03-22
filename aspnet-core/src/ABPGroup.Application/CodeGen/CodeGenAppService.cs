using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using ABPGroup.CodeGen.Dto;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using ABPGroup.Templates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ABPGroup.CodeGen;

/// <summary>
/// Facade service for code generation operations.
/// All core logic is delegated to specialized domain services to adhere to the 350-line class rule.
/// </summary>
public class CodeGenAppService : ABPGroupAppServiceBase, ICodeGenAppService
{
    private readonly ICodeGenSessionManager _sessionManager;
    private readonly ICodeGenEngine _engine;
    private readonly ICodeGenPlanner _planner;
    private readonly ICodeGenRefiner _refiner;
    private readonly ICodeGenAiService _aiService;
    private readonly IRepository<Template, int> _templateRepository;
    private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;

    private static readonly ConcurrentDictionary<Guid, byte> ActiveGenerations = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public CodeGenAppService(
        ICodeGenSessionManager sessionManager,
        ICodeGenEngine engine,
        ICodeGenPlanner planner,
        ICodeGenRefiner refiner,
        ICodeGenAiService aiService,
        IRepository<Template, int> templateRepository,
        Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
    {
        _sessionManager = sessionManager;
        _engine = engine;
        _planner = planner;
        _refiner = refiner;
        _aiService = aiService;
        _templateRepository = templateRepository;
        _scopeFactory = scopeFactory;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Core API Methods (ICodeGenAppService)
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CodeGenSessionDto> CreateSession(CreateSessionInput input)
    {
        return await _sessionManager.CreateSessionAsync(input, AbpSession.UserId);
    }

    public async Task<CodeGenSessionDto> GetSession(string sessionId)
    {
        return await _sessionManager.GetSessionDtoAsync(sessionId);
    }

    public async Task<StackRecommendationDto> RecommendStack(string sessionId)
    {
        var session = await _sessionManager.GetSessionAsync(sessionId);
        var prompt = $"Analyze the project '{session.ProjectName}' and its requirement: '{session.NormalizedRequirement}'.\n"
                   + "Suggest a technical stack (Framework, Language, Styling, Database, Orm, Auth).";

        var response = await _aiService.CallAiAsync(
            "You are a tech stack advisor. Suggest the best stack for the given project.",
            prompt);

        var explanation = CodeGenHelpers.ParseDelimitedSection(response, "EXPLANATION") ?? "Optimized for speed and development experience.";

        return new StackRecommendationDto
        {
            Framework = CodeGenHelpers.ParseDelimitedSection(response, "FRAMEWORK") ?? "Next.js",
            Language = CodeGenHelpers.ParseDelimitedSection(response, "LANGUAGE") ?? "TypeScript",
            Styling = CodeGenHelpers.ParseDelimitedSection(response, "STYLING") ?? "Ant Design / Tailwind",
            Database = CodeGenHelpers.ParseDelimitedSection(response, "DATABASE") ?? "PostgreSQL",
            Orm = CodeGenHelpers.ParseDelimitedSection(response, "ORM") ?? "Prisma",
            Auth = CodeGenHelpers.ParseDelimitedSection(response, "AUTH") ?? "NextAuth.js",
            Reasoning = new Dictionary<string, string>
            {
                { "framework", explanation },
                { "language", explanation },
                { "styling", explanation },
                { "database", explanation },
                { "orm", explanation },
                { "auth", explanation }
            }
        };
    }

    [HttpPut]
    public async Task<CodeGenSessionDto> SaveStack(SaveStackInput input)
    {
        var session = await _sessionManager.GetSessionAsync(input.SessionId);
        session.ConfirmedStackJson = JsonSerializer.Serialize(input.Stack, JsonOptions);
        session.Status = (int)CodeGenStatus.StackConfirmed;
        session.UpdatedAt = DateTime.UtcNow;
        await _sessionManager.SaveSessionAsync(session);
        return _sessionManager.MapToDto(session);
    }

    public async Task<CodeGenSessionDto> GenerateSpec(string sessionId)
    {
        // This usually triggers the AI to build the AppSpecDto.
        // For simplicity, we reuse GenerateReadme logic or a specific planning call.
        var readmeResult = await _planner.GenerateReadmeAsync(sessionId);
        var session = await _sessionManager.GetSessionAsync(sessionId);
        return _sessionManager.MapToDto(session);
    }

    [HttpPut]
    public async Task<CodeGenSessionDto> SaveSpec(SaveSpecInput input)
    {
        var session = await _sessionManager.GetSessionAsync(input.SessionId);
        session.SpecJson = JsonSerializer.Serialize(input.Spec, JsonOptions);
        session.SpecConfirmedAt = DateTime.UtcNow;
        session.Status = (int)CodeGenStatus.SpecConfirmed;
        session.UpdatedAt = DateTime.UtcNow;
        await _sessionManager.SaveSessionAsync(session);
        return _sessionManager.MapToDto(session);
    }

    public async Task<CodeGenSessionDto> ConfirmSpec(string sessionId)
    {
        var session = await _sessionManager.GetSessionAsync(sessionId);
        session.SpecConfirmedAt = DateTime.UtcNow;
        session.Status = (int)CodeGenStatus.SpecConfirmed;
        session.UpdatedAt = DateTime.UtcNow;
        await _sessionManager.SaveSessionAsync(session);
        return _sessionManager.MapToDto(session);
    }

    public async Task<CodeGenSessionDto> Generate(string sessionId)
    {
        Logger.Info($"Generate endpoint called for session {sessionId}");
        await TriggerGeneration(sessionId);
        return await _sessionManager.GetSessionDtoAsync(sessionId);
    }

    public async Task<GenerationStatusDto> GetStatus(string sessionId)
    {
        // Use a projection to avoid loading GeneratedFilesJson and other large blobs
        var guid = Guid.Parse(sessionId);
        var sessionData = await _sessionManager.GetSessionQuery()
            .Where(s => s.Id == guid)
            .Select(s => new {
                s.Status,
                s.ErrorMessage,
                s.CurrentPhase,
                s.CompletedStepsJson,
                s.ValidationResultsJson,
                s.Id
            })
            .FirstOrDefaultAsync();

        if (sessionData == null)
            throw new Abp.UI.UserFriendlyException($"Session {sessionId} not found.");

        var completedSteps = !string.IsNullOrEmpty(sessionData.CompletedStepsJson)
            ? JsonSerializer.Deserialize<string[]>(sessionData.CompletedStepsJson, JsonOptions)
            : Array.Empty<string>();
 
        var validationResults = !string.IsNullOrEmpty(sessionData.ValidationResultsJson)
            ? JsonSerializer.Deserialize<ValidationResultDto[]>(sessionData.ValidationResultsJson, JsonOptions)
            : Array.Empty<ValidationResultDto>();
 
        bool isRunning = ActiveGenerations.ContainsKey(sessionData.Id);
        int status = sessionData.Status;
        string errorMessage = sessionData.ErrorMessage;
        string currentPhase = sessionData.CurrentPhase ?? "Working...";

        // If the status is Generating but it's not actually running in our ActiveGenerations dictionary,
        // it means the session was likely lost due to an application restart or a fatal crash in the background task.
        // Reset the session status back to SpecConfirmed so that retrying will properly re-trigger generation.
        if (status == (int)CodeGenStatus.Generating && !isRunning)
        {
            // Reset session status to allow retry
            var resetScope = _scopeFactory.CreateScope();
            var resetSessionManager = resetScope.ServiceProvider.GetRequiredService<ICodeGenSessionManager>();
            var resetSession = await resetSessionManager.GetSessionAsync(sessionId);
            resetSession.Status = (int)CodeGenStatus.SpecConfirmed;
            resetSession.ErrorMessage = null;
            resetSession.CurrentPhase = null;
            resetSession.UpdatedAt = DateTime.UtcNow;
            await resetSessionManager.SaveSessionAsync(resetSession);

            status = (int)CodeGenStatus.Failed;
            errorMessage = "Generation was interrupted by an application restart. Please try triggering generation again.";
            currentPhase = "Generation interrupted";
        }

        return new GenerationStatusDto
        {
            Status = status,
            IsRunning = isRunning,
            ErrorMessage = errorMessage,
            CurrentPhase = currentPhase,
            CompletedSteps = completedSteps,
            ValidationResults = validationResults,
            IsComplete = status == (int)CodeGenStatus.GenerationCompleted || status == (int)CodeGenStatus.Failed
        };
    }

    public async Task<CodeGenSessionDto> Repair(TriggerRepairInput input)
    {
        return await _refiner.RepairAsync(input);
    }

    public async Task<RefinementResultDto> RefineSession(RefinementInputDto input)
    {
        return await _refiner.RefineSessionAsync(input);
    }

    public async Task<ReadmeResultDto> GenerateReadme(string sessionId)
    {
        return await _planner.GenerateReadmeAsync(sessionId);
    }

    public async Task<CodeGenSessionDto> ConfirmReadme(string sessionId)
    {
        return await ConfirmSpec(sessionId);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Background Processing
    // ──────────────────────────────────────────────────────────────────────────

    public async Task TriggerGeneration(string sessionId)
    {
        var session = await _sessionManager.GetSessionAsync(sessionId);
        if (session.Status < (int)CodeGenStatus.SpecConfirmed)
            throw new Abp.UI.UserFriendlyException("Spec must be confirmed before generation.");

        // Validate required fields before starting background generation
        if (string.IsNullOrWhiteSpace(session.NormalizedRequirement))
            throw new Abp.UI.UserFriendlyException("Session must have a normalized requirement before generation. Please complete the spec review step.");

        if (string.IsNullOrWhiteSpace(session.SpecJson))
            throw new Abp.UI.UserFriendlyException("Session must have a confirmed spec before generation. Please confirm the spec first.");

        if (string.IsNullOrWhiteSpace(session.ProjectName))
            throw new Abp.UI.UserFriendlyException("Session must have a project name before generation.");

        if (!ActiveGenerations.TryAdd(session.Id, 0))
            return;

        // Set status to Generating immediately so polling shows it as active
        session.Status = (int)CodeGenStatus.Generating;
        session.CurrentPhase = "Starting background task...";
        session.UpdatedAt = DateTime.UtcNow;
        await _sessionManager.SaveSessionAsync(session);

        using (System.Threading.ExecutionContext.SuppressFlow())
        {
            _ = Task.Run(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var sessionManager = scope.ServiceProvider.GetRequiredService<ICodeGenSessionManager>();
                var engine = scope.ServiceProvider.GetRequiredService<ICodeGenEngine>();
                
                try
                {
                    await BackgroundGenerate(sessionId, sessionManager, engine);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Fatal error in background generation for session {sessionId}", ex);
                    
                    // Update session status to Failed on fatal error
                    try
                    {
                        var failSession = await sessionManager.GetSessionAsync(sessionId);
                        failSession.Status = (int)CodeGenStatus.Failed;
                        failSession.ErrorMessage = $"Fatal startup error: {ex.Message}";
                        failSession.UpdatedAt = DateTime.UtcNow;
                        await sessionManager.SaveSessionAsync(failSession);
                    }
                    catch { /* ignore nested error */ }
                }
                finally
                {
                    ActiveGenerations.TryRemove(session.Id, out _);
                }
            });
        }
    }

    private async Task BackgroundGenerate(string sessionId, ICodeGenSessionManager sessionManager, ICodeGenEngine engine)
    {
        Logger.Info($"BackgroundGenerate: Starting for session {sessionId}");
        
        var session = await sessionManager.GetSessionAsync(sessionId);
        // session.Status is already Generating (set in TriggerGeneration)
        session.CurrentPhase = "Initializing generation pipeline...";
        session.CompletedStepsJson = "[]";
        session.GenerationStartedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        await sessionManager.SaveSessionAsync(session);
        Logger.Info($"BackgroundGenerate: Session initialized for {sessionId}");

        var completedSteps = new List<string>();

        try
        {
            Logger.Info($"BackgroundGenerate: Parsing spec for {sessionId}");
            var planJson = session.SpecJson;
            var plan = CodeGenHelpers.ParseSpecOrDefault(planJson, out var parseWarning);
            if (!string.IsNullOrWhiteSpace(parseWarning))
            {
                Logger.Warn($"BackgroundGenerate: Spec parse warning for {sessionId}: {parseWarning}");
            }
            
            var stack = JsonSerializer.Deserialize<StackConfigDto>(session.ConfirmedStackJson ?? "{}", JsonOptions);
            if (stack == null)
            {
                Logger.Warn($"BackgroundGenerate: Stack config is null for {sessionId}, using defaults");
            }
            
            var engineInput = new CodeGenEngineInput
            {
                Id = 1,
                Name = session.ProjectName ?? "untitled-app",
                Prompt = session.NormalizedRequirement ?? session.Prompt ?? string.Empty,
                Framework = MapFramework(stack?.Framework),
                Language = ProgrammingLanguage.TypeScript,
                DatabaseOption = DatabaseOption.RenderPostgres,
                IncludeAuth = true
            };

            if (string.IsNullOrWhiteSpace(engineInput.Prompt))
            {
                throw new InvalidOperationException("Engine input prompt is empty. Cannot generate project without requirements.");
            }

            Logger.Info($"BackgroundGenerate: Calling GenerateProjectAsync for {sessionId} with framework={engineInput.Framework}, prompt length={engineInput.Prompt.Length}");
            var result = await engine.GenerateProjectAsync(
                engineInput,
                async msg =>
                {
                    Logger.Info($"BackgroundGenerate: Progress update for {sessionId}: {msg}");
                    var innerScope = _scopeFactory.CreateScope();
                    var innerSessionManager = innerScope.ServiceProvider.GetRequiredService<ICodeGenSessionManager>();
                    var innerSession = await innerSessionManager.GetSessionAsync(sessionId);
                    
                    innerSession.CurrentPhase = msg;
                    if (string.IsNullOrEmpty(innerSession.CompletedStepsJson) || innerSession.CompletedStepsJson == "[]")
                    {
                        innerSession.CompletedStepsJson = "[]";
                    }
                    
                    var steps = JsonSerializer.Deserialize<List<string>>(innerSession.CompletedStepsJson, JsonOptions) ?? new List<string>();
                    if (!steps.Contains(msg))
                    {
                        steps.Add(msg);
                    }
                    innerSession.CompletedStepsJson = JsonSerializer.Serialize(steps, JsonOptions);
                    innerSession.UpdatedAt = DateTime.UtcNow;
                    await innerSessionManager.SaveSessionAsync(innerSession);
                },
                null,
                plan);

            Logger.Info($"BackgroundGenerate: Generation completed for {sessionId}. Saving files...");
            
            // Reload one last time for the final result
            var finalScope = _scopeFactory.CreateScope();
            var finalSessionManager = finalScope.ServiceProvider.GetRequiredService<ICodeGenSessionManager>();
            var finalSession = await finalSessionManager.GetSessionAsync(sessionId);

            finalSession.GeneratedFilesJson = JsonSerializer.Serialize(
                result.Files.Select(f => new GeneratedFileDto { Path = f.Path, Content = f.Content }), JsonOptions);
            finalSession.Status = (int)CodeGenStatus.GenerationCompleted;
            finalSession.GenerationMode = "full";
            finalSession.GenerationCompletedAt = DateTime.UtcNow;
            finalSession.CurrentPhase = "Generation completed successfully";
            finalSession.UpdatedAt = DateTime.UtcNow;
            
            await finalSessionManager.SaveSessionAsync(finalSession);
            Logger.Info($"BackgroundGenerate: Session finalized for {sessionId}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Generation failed for session {sessionId}: {ex.Message}", ex);
            
            var errorScope = _scopeFactory.CreateScope();
            var errorSessionManager = errorScope.ServiceProvider.GetRequiredService<ICodeGenSessionManager>();
            var errorSession = await errorSessionManager.GetSessionAsync(sessionId);
            
            errorSession.Status = (int)CodeGenStatus.Failed;
            errorSession.ErrorMessage = ex.Message;
            errorSession.CurrentPhase = "Generation failed";
            errorSession.UpdatedAt = DateTime.UtcNow;
            
            await errorSessionManager.SaveSessionAsync(errorSession);
        }
    }

    private static Framework MapFramework(string framework)
    {
        if (string.IsNullOrEmpty(framework)) return Framework.NextJS;
        var lower = framework.ToLowerInvariant();
        if (lower.Contains("next")) return Framework.NextJS;
        if (lower.Contains("react") || lower.Contains("vite")) return Framework.ReactVite;
        return Framework.NextJS;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Legacy Methods
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CodeGenResult> GenerateProjectAsync(CreateUpdateProjectDto input, Func<string, Task> onProgress = null)
    {
        var engineInput = new CodeGenEngineInput
        {
            Id = input.Id,
            Name = input.Name,
            Prompt = input.Prompt,
            Framework = input.Framework,
            Language = input.Language,
            DatabaseOption = input.DatabaseOption,
            IncludeAuth = input.IncludeAuth
        };
        return await _engine.GenerateProjectAsync(engineInput, onProgress);
    }
}
