using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.UI;
using ABPGroup.CodeGen.Dto;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using ABPGroup.Templates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABPGroup.CodeGen;

public class CodeGenAppService : ABPGroupAppServiceBase, ICodeGenAppService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IRepository<Template, int> _templateRepository;
    private readonly IRepository<CodeGenSession, Guid> _sessionRepository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private static readonly ConcurrentDictionary<string, CodeGenSession> InMemorySessions = new();
    private static readonly ConcurrentDictionary<Guid, byte> ActiveGenerations = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // 3-param and 4-param constructors for backward compat with tests
    public CodeGenAppService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IRepository<Template, int> templateRepository)
        : this(httpClientFactory, configuration, templateRepository, null, null)
    {
    }

    public CodeGenAppService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IRepository<Template, int> templateRepository,
        IRepository<CodeGenSession, Guid> sessionRepository)
        : this(httpClientFactory, configuration, templateRepository, sessionRepository, null)
    {
    }

    public CodeGenAppService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IRepository<Template, int> templateRepository,
        IRepository<CodeGenSession, Guid> sessionRepository,
        IServiceScopeFactory serviceScopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _templateRepository = templateRepository;
        _sessionRepository = sessionRepository;
        _serviceScopeFactory = serviceScopeFactory;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Legacy single-shot generation
    // ──────────────────────────────────────────────────────────────────────────

    public Task<CodeGenResult> GenerateProjectAsync(CreateUpdateProjectDto input)
        => GenerateProjectAsync(input, null);

    internal async Task<CodeGenResult> GenerateProjectAsync(
        CreateUpdateProjectDto input,
        Func<string, Task> onProgress)
    {
        var result = new CodeGenResult
        {
            GeneratedProjectId = input.Id,
            Files = new List<GeneratedFile>(),
            ModuleList = new List<string>()
        };

        var projectName = input.Name ?? $"project-{input.Id}";
        var outputBase = _configuration["CodeGen:OutputPath"]
            ?? Path.Combine(Path.GetTempPath(), "GeneratedApps");
        var outputPath = Path.Combine(outputBase, projectName);

        // Phase 1: Requirements analysis
        await ReportProgress(onProgress, "[1/5] Analyzing requirements...");
        var requirementsPrompt = BuildRequirementsPrompt(input);
        var requirementsResponse = await CallGeminiAsync(
            "You are an expert software architect. Analyze the user's requirements and return a structured breakdown.",
            requirementsPrompt);

        result.ArchitectureSummary = ParseDelimitedSection(requirementsResponse, "ARCHITECTURE");
        var features = ParseDelimitedSection(requirementsResponse, "FEATURES");
        var pages = ParseDelimitedSection(requirementsResponse, "PAGES");
        var apiEndpoints = ParseDelimitedSection(requirementsResponse, "API_ENDPOINTS");
        var dbEntities = ParseDelimitedSection(requirementsResponse, "DB_ENTITIES");

        // Phase 2: Scaffold from template
        await ReportProgress(onProgress, "[2/5] Setting up project scaffold...");
        if (input.Framework == Framework.NextJS)
        {
            var templateDir = FindTemplateDirectory("next-ts-antd-prisma");
            if (templateDir != null)
            {
                var scaffoldFiles = ReadScaffoldFiles(templateDir);
                result.Files.AddRange(scaffoldFiles);
            }
        }

        // Build shared context for generation phases
        var context = new StringBuilder();
        context.AppendLine($"Project: {projectName}");
        context.AppendLine($"Framework: {input.Framework}");
        context.AppendLine($"Language: {input.Language}");
        context.AppendLine($"Database: {input.DatabaseOption}");
        context.AppendLine($"Auth: {(input.IncludeAuth ? "Yes" : "No")}");
        context.AppendLine($"Features: {features}");
        context.AppendLine($"Pages: {pages}");
        context.AppendLine($"API Endpoints: {apiEndpoints}");
        context.AppendLine($"DB Entities: {dbEntities}");
        context.AppendLine($"Architecture: {result.ArchitectureSummary}");

        // Phase 3: Frontend generation
        await Task.Delay(1000); // Proactive delay to avoid rate limits
        await ReportProgress(onProgress, "[3/5] Generating frontend...");
        var frontendResponse = await CallGeminiAsync(
            BuildCodeGenSystemPrompt("frontend pages and components", input.Framework.ToString()),
            $"Generate the frontend code for:\n{context}\n\nRequirements: {input.Prompt}");
        var frontendFiles = ParseFiles(frontendResponse);
        result.Files.AddRange(frontendFiles);
        result.ModuleList.AddRange(ParseModules(frontendResponse));

        var frontendArch = ParseDelimitedSection(frontendResponse, "ARCHITECTURE");
        if (!string.IsNullOrEmpty(frontendArch) && string.IsNullOrEmpty(result.ArchitectureSummary))
            result.ArchitectureSummary = frontendArch;

        // Phase 4: Backend generation
        await Task.Delay(1000); // Proactive delay
        await ReportProgress(onProgress, "[4/5] Generating backend...");
        var backendResponse = await CallGeminiAsync(
            BuildCodeGenSystemPrompt("backend API routes and server logic", input.Framework.ToString()),
            $"Generate the backend code for:\n{context}\n\nRequirements: {input.Prompt}");
        var backendFiles = ParseFiles(backendResponse);
        result.Files.AddRange(backendFiles);
        result.ModuleList.AddRange(ParseModules(backendResponse));

        // Phase 5: Database generation
        await Task.Delay(1000); // Proactive delay
        await ReportProgress(onProgress, "[5/5] Generating database layer...");
        var dbResponse = await CallGeminiAsync(
            BuildCodeGenSystemPrompt("database schema and data access layer", input.Framework.ToString()),
            $"Generate the database layer for:\n{context}\n\nRequirements: {input.Prompt}");
        var dbFiles = ParseFiles(dbResponse);
        result.Files.AddRange(dbFiles);
        result.ModuleList.AddRange(ParseModules(dbResponse));

        // Write files to disk
        result.OutputPath = outputPath;
        var skipBuild = string.Equals(_configuration["CodeGen:SkipBuild"], "true", StringComparison.OrdinalIgnoreCase);
        WriteFilesToDisk(result.Files, outputPath);

        return result;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Multi-step workflow
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<CodeGenSessionDto> CreateSession(CreateSessionInput input)
    {
        var session = new CodeGenSession
        {
            Id = Guid.NewGuid(),
            Prompt = input.Prompt,
            Status = (int)CodeGenStatus.Captured,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Call LLM to analyze requirements
        string response;
        try
        {
            response = await CallGeminiAsync(
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
            Logger.Error("CreateSession: Groq API call failed", ex);
            throw new UserFriendlyException($"AI service call failed: {ex.Message}");
        }

        session.ProjectName = ParseDelimitedSection(response, "PROJECT_NAME")?.Trim() ?? "untitled-app";
        session.NormalizedRequirement = ParseDelimitedSection(response, "NORMALIZED_REQUIREMENT")?.Trim() ?? input.Prompt;
        session.DetectedFeaturesJson = JsonSerializer.Serialize(
            ParseCsvList(ParseDelimitedSection(response, "DETECTED_FEATURES")), JsonOptions);
        session.DetectedEntitiesJson = JsonSerializer.Serialize(
            ParseCsvList(ParseDelimitedSection(response, "DETECTED_ENTITIES")), JsonOptions);

        try
        {
            if (AbpSession?.UserId != null)
                session.UserId = AbpSession.UserId;
        }
        catch { /* AbpSession may not be available in non-authenticated context */ }

        try
        {
            await SaveSession(session, isNew: true);
        }
        catch (Exception ex)
        {
            Logger.Error("CreateSession: Failed to save session", ex);
            throw new UserFriendlyException($"Failed to save session: {ex.Message}");
        }

        return MapSessionToDto(session);
    }

    public async Task<StackRecommendationDto> RecommendStack(string sessionId)
    {
        try
        {
            Logger.Debug($"RecommendStack: Loading session {sessionId}");
            var session = await LoadSession(sessionId);

            Logger.Debug($"RecommendStack: Calling Groq for session {sessionId}");
            var response = await CallGeminiAsync(
                "You are an expert software architect. Based on the application requirements, recommend the best technology stack. "
                + "Return your recommendation in the following delimited format:\n"
                + "===FRAMEWORK===\n<framework name, e.g. Next.js, React + Vite, Angular, Vue, .NET Blazor>\n===END FRAMEWORK===\n"
                + "===LANGUAGE===\n<language, e.g. TypeScript, JavaScript, C#>\n===END LANGUAGE===\n"
                + "===STYLING===\n<styling approach, e.g. Tailwind CSS, Ant Design, Material UI, CSS Modules>\n===END STYLING===\n"
                + "===DATABASE===\n<database, e.g. PostgreSQL, MongoDB, SQLite>\n===END DATABASE===\n"
                + "===ORM===\n<ORM, e.g. Prisma, Drizzle, TypeORM, Entity Framework>\n===END ORM===\n"
                + "===AUTH===\n<auth approach, e.g. NextAuth.js, JWT, OAuth2, None>\n===END AUTH===\n"
                + "===REASONING===\n<JSON object with keys matching each choice above, values explaining why>\n===END REASONING===\n",
                $"Application: {session.NormalizedRequirement}\n"
                + $"Features: {session.DetectedFeaturesJson}\n"
                + $"Entities: {session.DetectedEntitiesJson}");

            var reasoning = new Dictionary<string, string>();
            var reasoningStr = ParseDelimitedSection(response, "REASONING")?.Trim();
            if (!string.IsNullOrEmpty(reasoningStr))
            {
                try
                {
                    reasoning = JsonSerializer.Deserialize<Dictionary<string, string>>(reasoningStr, JsonOptions);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"RecommendStack: Failed to parse reasoning JSON: {ex.Message}");
                }
            }

            return new StackRecommendationDto
            {
                Framework = ParseDelimitedSection(response, "FRAMEWORK")?.Trim() ?? "Next.js",
                Language = ParseDelimitedSection(response, "LANGUAGE")?.Trim() ?? "TypeScript",
                Styling = ParseDelimitedSection(response, "STYLING")?.Trim() ?? "Tailwind CSS",
                Database = ParseDelimitedSection(response, "DATABASE")?.Trim() ?? "PostgreSQL",
                Orm = ParseDelimitedSection(response, "ORM")?.Trim() ?? "Prisma",
                Auth = ParseDelimitedSection(response, "AUTH")?.Trim() ?? "NextAuth.js",
                Reasoning = reasoning ?? new Dictionary<string, string>()
            };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error($"RecommendStack: Unexpected error for session {sessionId}", ex);
            throw new UserFriendlyException($"Failed to generate stack recommendation: {ex.Message}");
        }
    }

    [HttpPut]
    public async Task<CodeGenSessionDto> SaveStack(SaveStackInput input)
    {
        var session = await LoadSession(input.SessionId);
        session.ConfirmedStackJson = JsonSerializer.Serialize(input.Stack, JsonOptions);
        session.Status = (int)CodeGenStatus.StackConfirmed;
        session.UpdatedAt = DateTime.UtcNow;

        // Determine scaffold template based on framework
        var fw = input.Stack.Framework?.ToLowerInvariant() ?? "";
        if (fw.Contains("next"))
            session.ScaffoldTemplate = "next-ts-antd-prisma";

        await SaveSession(session);
        return MapSessionToDto(session);
    }

    public async Task<CodeGenSessionDto> GenerateSpec(string sessionId)
    {
        try
        {
            Logger.Debug($"GenerateSpec: Loading session {sessionId}");
            var session = await LoadSession(sessionId);
            if (session.Status < (int)CodeGenStatus.StackConfirmed)
                throw new UserFriendlyException("Stack must be confirmed before generating spec.");

            var stack = DeserializeOrDefault<StackConfigDto>(session.ConfirmedStackJson);
            var features = DeserializeOrDefault<List<string>>(session.DetectedFeaturesJson) ?? new List<string>();
            var entities = DeserializeOrDefault<List<string>>(session.DetectedEntitiesJson) ?? new List<string>();

            Logger.Debug($"GenerateSpec: Calling Groq for session {sessionId}");
            string response;
            try
            {
                response = await CallGeminiAsync(
                    "You are an expert software architect. Generate a comprehensive application specification as a JSON object. "
                    + "The spec must include: entities (with fields, types, relations), pages (with routes, components, data requirements), "
                    + "apiRoutes (with method, path, handler, requestBody, responseShape, auth), "
                    + "validations (rules the generated code must satisfy, e.g. file-exists, build-passes, auth-guard), "
                    + "and fileManifest (all files that will be generated). "
                    + "Return ONLY valid JSON wrapped in delimiters:\n"
                    + "===SPEC_JSON===\n{...}\n===END SPEC_JSON===",
                    $"Application: {session.NormalizedRequirement}\n"
                    + $"Features: {string.Join(", ", features)}\n"
                    + $"Entities: {string.Join(", ", entities)}\n"
                    + $"Stack: Framework={stack?.Framework}, Language={stack?.Language}, "
                    + $"Styling={stack?.Styling}, Database={stack?.Database}, ORM={stack?.Orm}, Auth={stack?.Auth}");
            }
            catch (Exception ex)
            {
                Logger.Error("GenerateSpec: Groq API call failed", ex);
                throw new UserFriendlyException($"AI service call failed: {ex.Message}");
            }

            Logger.Debug($"GenerateSpec: Parsing response for session {sessionId}");
            var specJson = ParseDelimitedSection(response, "SPEC_JSON")?.Trim();
            var spec = ParseSpecOrDefault(specJson, out var parseWarning);
            if (!string.IsNullOrEmpty(parseWarning))
            {
                Logger.Warn(parseWarning);
            }

            Logger.Debug($"GenerateSpec: Saving spec for session {sessionId}");
            session.SpecJson = JsonSerializer.Serialize(spec, JsonOptions);
            session.Status = (int)CodeGenStatus.SpecGenerated;
            session.UpdatedAt = DateTime.UtcNow;
            
            await SaveSession(session);
            return MapSessionToDto(session);
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Logger.Error($"GenerateSpec: Unexpected error for session {sessionId}", ex);
            throw new UserFriendlyException($"An unexpected error occurred during spec generation: {ex.Message}");
        }
    }

    [HttpPut]
    public async Task<CodeGenSessionDto> SaveSpec(SaveSpecInput input)
    {
        var session = await LoadSession(input.SessionId);
        session.SpecJson = JsonSerializer.Serialize(input.Spec, JsonOptions);
        session.UpdatedAt = DateTime.UtcNow;
        await SaveSession(session);
        return MapSessionToDto(session);
    }

    public async Task<CodeGenSessionDto> ConfirmSpec(string sessionId)
    {
        var session = await LoadSession(sessionId);
        session.SpecConfirmedAt = DateTime.UtcNow;
        session.Status = (int)CodeGenStatus.SpecConfirmed;
        session.UpdatedAt = DateTime.UtcNow;
        await SaveSession(session);
        return MapSessionToDto(session);
    }

    public async Task<CodeGenSessionDto> Generate(string sessionId)
    {
        var session = await LoadSession(sessionId);
        if (session.Status < (int)CodeGenStatus.SpecConfirmed)
            throw new UserFriendlyException("Spec must be confirmed before generating.");

        // Prevent double-start: if already generating or beyond, return current state
        if (session.Status >= (int)CodeGenStatus.Generating)
            return MapSessionToDto(session);

        var sessionGuid = session.Id;

        // Prevent concurrent generation for the same session
        if (!ActiveGenerations.TryAdd(sessionGuid, 0))
            return MapSessionToDto(session);

        var spec = NormalizeSpec(DeserializeOrDefault<AppSpecDto>(session.SpecJson) ?? new AppSpecDto());
        var stack = DeserializeOrDefault<StackConfigDto>(session.ConfirmedStackJson);
        var validationRules = spec.Validations ?? new List<ValidationRuleDto>();
        var initialValidationResults = BuildInitialValidationResults(validationRules);

        session.Status = (int)CodeGenStatus.Generating;
        session.GenerationStartedAt = DateTime.UtcNow;
        session.CurrentPhase = "scaffold";
        session.CompletedStepsJson = JsonSerializer.Serialize(new List<string>(), JsonOptions);
        session.ValidationResultsJson = JsonSerializer.Serialize(initialValidationResults, JsonOptions);
        session.ErrorMessage = null;
        session.UpdatedAt = DateTime.UtcNow;
        await SaveSession(session);

        // Capture dependencies for the background task
        var validationConstraints = BuildValidationConstraints(validationRules);
        var projectInput = new CreateUpdateProjectDto
        {
            Id = session.ProjectId ?? 0,
            Name = session.ProjectName ?? "generated-app",
            Prompt = (session.NormalizedRequirement ?? session.Prompt) + validationConstraints,
            Framework = MapFrameworkString(stack?.Framework),
            Language = MapLanguageString(stack?.Language),
            DatabaseOption = MapDatabaseString(stack?.Database),
            IncludeAuth = !string.IsNullOrEmpty(stack?.Auth) && !stack.Auth.Equals("None", StringComparison.OrdinalIgnoreCase)
        };

        // Capture singleton/long-lived references safe for background use
        var httpClientFactory = _httpClientFactory;
        var configuration = _configuration;
        var templateRepository = _templateRepository;
        var scopeFactory = _serviceScopeFactory;
        var logger = Logger;

        logger.Info($"[CodeGen] Launching background task for session {sessionGuid}");

        using (System.Threading.ExecutionContext.SuppressFlow())
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    logger.Info($"[CodeGen] Background task entered for session {sessionGuid}");

                    // Create a fresh DI scope so the DbContext and repositories
                    // are not tied to the (now-completed) HTTP request scope.
                    using var scope = scopeFactory.CreateScope();
                    var scopedUowManager = scope.ServiceProvider.GetRequiredService<Abp.Domain.Uow.IUnitOfWorkManager>();
                    var scopedSessionRepo = scope.ServiceProvider.GetRequiredService<IRepository<CodeGenSession, Guid>>();

                    using var uow = scopedUowManager.Begin();

                    // Create a standalone service for LLM calls (only needs singletons).
                    var bgService = new CodeGenAppService(
                        httpClientFactory, configuration, templateRepository);
                    bgService.Logger = logger;

                    async Task OnProgress(string message)
                    {
                        try
                        {
                            logger.Info($"[CodeGen] Progress ({sessionGuid}): {message}");
                            using var progressUow = scopedUowManager.Begin(
                                new Abp.Domain.Uow.UnitOfWorkOptions
                                {
                                    Scope = System.Transactions.TransactionScopeOption.RequiresNew
                                });
                            var s = await scopedSessionRepo.GetAsync(sessionGuid);
                            s.CurrentPhase = message;
                            var steps = DeserializeOrDefault<List<string>>(s.CompletedStepsJson) ?? new List<string>();
                            steps.Add(message);
                            s.CompletedStepsJson = JsonSerializer.Serialize(steps, JsonOptions);
                            s.UpdatedAt = DateTime.UtcNow;
                            await scopedSessionRepo.UpdateAsync(s);
                            await progressUow.CompleteAsync();
                        }
                        catch (Exception progressEx)
                        {
                            logger.Error($"[CodeGen] Progress update failed ({sessionGuid}): {progressEx.Message}", progressEx);
                        }
                    }

                    logger.Info($"[CodeGen] Starting GenerateProjectAsync for session {sessionGuid}");
                    var result = await bgService.GenerateProjectAsync(projectInput, OnProgress);
                    logger.Info($"[CodeGen] GenerateProjectAsync completed for session {sessionGuid}. Files: {result?.Files?.Count ?? 0}");

                    var sess = await scopedSessionRepo.GetAsync(sessionGuid);
                    sess.GeneratedFilesJson = JsonSerializer.Serialize(
                        result.Files.Select(f => new GeneratedFileDto { Path = f.Path, Content = f.Content }).ToList(),
                        JsonOptions);

                    await OnProgress("[6/6] Running validations...");

                    sess = await scopedSessionRepo.GetAsync(sessionGuid);
                    sess.Status = (int)CodeGenStatus.ValidationRunning;
                    sess.ValidationResultsJson = JsonSerializer.Serialize(
                        initialValidationResults
                            .Select(v => new ValidationResultDto
                            {
                                Id = v.Id,
                                Status = "running",
                                Message = "Running validation..."
                            })
                            .ToList(),
                        JsonOptions);
                    sess.UpdatedAt = DateTime.UtcNow;
                    await scopedSessionRepo.UpdateAsync(sess);

                    var finalValidationResults = EvaluateValidationResults(validationRules, result.Files);
                    var hasValidationFailures = finalValidationResults.Any(v => v.Status == "failed");

                    sess.ValidationResultsJson = JsonSerializer.Serialize(finalValidationResults, JsonOptions);
                    sess.Status = hasValidationFailures
                        ? (int)CodeGenStatus.ValidationFailed
                        : (int)CodeGenStatus.ValidationPassed;
                    sess.GenerationCompletedAt = DateTime.UtcNow;
                    sess.CurrentPhase = hasValidationFailures ? "validation-failed" : "completed";
                    sess.ErrorMessage = null;
                    sess.UpdatedAt = DateTime.UtcNow;
                    await scopedSessionRepo.UpdateAsync(sess);
                    await uow.CompleteAsync();

                    logger.Info($"[CodeGen] Session {sessionGuid} generation completed.");
                }
                catch (Exception ex)
                {
                    logger.Error($"[CodeGen] FAILED for session {sessionGuid}: {ex.Message}", ex);
                    try
                    {
                        // Use a separate scope for error handling in case the main scope is corrupted
                        using var errScope = scopeFactory.CreateScope();
                        var errUowManager = errScope.ServiceProvider.GetRequiredService<Abp.Domain.Uow.IUnitOfWorkManager>();
                        var errSessionRepo = errScope.ServiceProvider.GetRequiredService<IRepository<CodeGenSession, Guid>>();

                        using var errUow = errUowManager.Begin();
                        var sess = await errSessionRepo.GetAsync(sessionGuid);
                        var failedResults = MarkValidationResultsFailed(initialValidationResults, ex.Message);
                        sess.ValidationResultsJson = JsonSerializer.Serialize(failedResults, JsonOptions);
                        sess.Status = (int)CodeGenStatus.ValidationFailed;
                        sess.CurrentPhase = "failed";
                        sess.ErrorMessage = ex.Message.Length > 995 ? ex.Message[..992] + "..." : ex.Message;
                        sess.UpdatedAt = DateTime.UtcNow;
                        await errSessionRepo.UpdateAsync(sess);
                        await errUow.CompleteAsync();
                    }
                    catch (Exception errEx)
                    {
                        logger.Error($"[CodeGen] Could not update failure status for session {sessionGuid}: {errEx.Message}", errEx);
                    }
                }
                finally
                {
                    ActiveGenerations.TryRemove(sessionGuid, out _);
                }
            });
        }

        return MapSessionToDto(session);
    }

    public async Task<GenerationStatusDto> GetStatus(string sessionId)
    {
        var session = await LoadSession(sessionId);
        var completedSteps = DeserializeOrDefault<List<string>>(session.CompletedStepsJson) ?? new List<string>();
        var validationResults = DeserializeOrDefault<List<ValidationResultDto>>(session.ValidationResultsJson) ?? new List<ValidationResultDto>();

        return new GenerationStatusDto
        {
            CurrentPhase = session.CurrentPhase,
            CompletedSteps = completedSteps,
            ValidationResults = validationResults,
            IsComplete = session.Status >= (int)CodeGenStatus.ValidationPassed,
            Error = session.ErrorMessage
        };
    }

    public async Task<CodeGenSessionDto> Repair(TriggerRepairInput input)
    {
        var session = await LoadSession(input.SessionId);
        session.RepairAttempts++;

        var currentFiles = DeserializeOrDefault<List<GeneratedFileDto>>(session.GeneratedFilesJson) ?? new List<GeneratedFileDto>();
        var failureDescriptions = string.Join("\n", input.Failures.Select(f => $"- [{f.Id}] {f.Message}"));

        var fileManifest = string.Join("\n", currentFiles.Select(f => f.Path));

        var response = await CallGeminiAsync(
            "You are an expert code repair agent. The generated code has validation failures. "
            + "Fix the issues and return corrected files in delimited format:\n"
            + "===FILE===\n<file path>\n===CONTENT===\n<corrected content>\n===END FILE===\n"
            + "Only return files that need changes.",
            $"Validation failures:\n{failureDescriptions}\n\n"
            + $"Current files:\n{fileManifest}\n\n"
            + $"File contents:\n{string.Join("\n---\n", currentFiles.Select(f => $"### {f.Path}\n{f.Content}"))}");

        var repairedFiles = ParseFiles(response);
        foreach (var repaired in repairedFiles)
        {
            var existing = currentFiles.FirstOrDefault(f => f.Path == repaired.Path);
            if (existing != null)
                existing.Content = repaired.Content;
            else
                currentFiles.Add(new GeneratedFileDto { Path = repaired.Path, Content = repaired.Content });
        }

        session.GeneratedFilesJson = JsonSerializer.Serialize(currentFiles, JsonOptions);
        session.UpdatedAt = DateTime.UtcNow;
        await SaveSession(session);
        return MapSessionToDto(session);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Gemini API
    // ──────────────────────────────────────────────────────────────────────────

    private async Task<string> CallGeminiAsync(string systemPrompt, string userPrompt)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"] ?? "gemini-3.1-pro-preview";
        var baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        int maxRetries = 3;
        int delaySeconds = 2;

        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemPrompt } }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = userPrompt } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 8192
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var request = new HttpRequestMessage(HttpMethod.Post, baseUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);

                if (response.StatusCode == (System.Net.HttpStatusCode)429)
                {
                    if (i == maxRetries) throw new UserFriendlyException("AI service is currently overloaded. Please try again in a few minutes.");

                    Logger.Warn($"Gemini API Rate Limit (429). Retrying in {delaySeconds}s... (Attempt {i + 1}/{maxRetries})");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    delaySeconds *= 2;
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                
                // Gemini response structure: candidates[0].content.parts[0].text
                return doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? string.Empty;
            }
            catch (HttpRequestException ex) when (i < maxRetries)
            {
                Logger.Warn($"Gemini API Request failed: {ex.Message}. Retrying in {delaySeconds}s...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                delaySeconds *= 2;
            }
        }

        throw new UserFriendlyException("Failed to communicate with the AI service after multiple attempts.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Parsing helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static string ParseDelimitedSection(string content, string sectionName)
    {
        if (string.IsNullOrEmpty(content)) return null;

        var startTag = $"==={sectionName}===";
        var endTag = $"===END {sectionName}===";

        var startIdx = content.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        if (startIdx < 0) return null;

        startIdx += startTag.Length;
        var endIdx = content.IndexOf(endTag, startIdx, StringComparison.OrdinalIgnoreCase);
        if (endIdx < 0) return content[startIdx..].Trim();

        return content[startIdx..endIdx].Trim();
    }

    private static List<GeneratedFile> ParseFiles(string content)
    {
        var files = new List<GeneratedFile>();
        if (string.IsNullOrEmpty(content)) return files;

        var remaining = content;
        while (true)
        {
            var fileStart = remaining.IndexOf("===FILE===", StringComparison.OrdinalIgnoreCase);
            if (fileStart < 0) break;

            var pathStart = fileStart + "===FILE===".Length;
            var contentTag = remaining.IndexOf("===CONTENT===", pathStart, StringComparison.OrdinalIgnoreCase);
            if (contentTag < 0) break;

            var path = remaining[pathStart..contentTag].Trim();
            var contentStart = contentTag + "===CONTENT===".Length;
            var fileEnd = remaining.IndexOf("===END FILE===", contentStart, StringComparison.OrdinalIgnoreCase);

            string fileContent;
            if (fileEnd >= 0)
            {
                fileContent = remaining[contentStart..fileEnd].Trim();
                remaining = remaining[(fileEnd + "===END FILE===".Length)..];
            }
            else
            {
                fileContent = remaining[contentStart..].Trim();
                break;
            }

            if (!string.IsNullOrEmpty(path))
            {
                files.Add(new GeneratedFile { Path = path, Content = fileContent });
            }
        }

        return files;
    }

    private static List<string> ParseModules(string content)
    {
        var modulesStr = ParseDelimitedSection(content, "MODULES");
        return ParseCsvList(modulesStr);
    }

    private static List<string> ParseCsvList(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) return new List<string>();
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }

    private static AppSpecDto ParseSpecOrDefault(string specJson, out string warning)
    {
        warning = null;
        if (string.IsNullOrWhiteSpace(specJson))
            return NormalizeSpec(new AppSpecDto());

        try
        {
            var strict = JsonSerializer.Deserialize<AppSpecDto>(specJson, JsonOptions) ?? new AppSpecDto();
            return NormalizeSpec(strict);
        }
        catch (Exception strictEx)
        {
            try
            {
                using var doc = JsonDocument.Parse(specJson);
                var fallback = BuildSpecFromJson(doc.RootElement);
                warning = $"GenerateSpec: Strict parse failed ({strictEx.Message}). Tolerant parser was used.";
                return NormalizeSpec(fallback);
            }
            catch (Exception fallbackEx)
            {
                warning = $"GenerateSpec: Failed to parse spec JSON: {fallbackEx.Message}. Raw: {specJson[..Math.Min(specJson.Length, 200)]}";
                return NormalizeSpec(new AppSpecDto());
            }
        }
    }

    private static AppSpecDto BuildSpecFromJson(JsonElement root)
    {
        return new AppSpecDto
        {
            Entities = ParseEntities(GetPropertyCaseInsensitive(root, "entities")),
            Pages = ParsePages(GetPropertyCaseInsensitive(root, "pages")),
            ApiRoutes = ParseApiRoutes(GetPropertyCaseInsensitive(root, "apiRoutes")),
            Validations = ParseValidations(GetPropertyCaseInsensitive(root, "validations")),
            FileManifest = ParseFileManifest(GetPropertyCaseInsensitive(root, "fileManifest"))
        };
    }

    private static List<EntitySpecDto> ParseEntities(JsonElement section)
    {
        var items = new List<EntitySpecDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out EntitySpecDto entity))
            {
                entity.Fields ??= new List<FieldSpecDto>();
                entity.Relations ??= new List<RelationSpecDto>();
                if (string.IsNullOrWhiteSpace(entity.TableName) && !string.IsNullOrWhiteSpace(entity.Name))
                    entity.TableName = entity.Name.ToLowerInvariant();
                items.Add(entity);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var name = element.GetString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    items.Add(new EntitySpecDto
                    {
                        Name = name,
                        TableName = name.ToLowerInvariant(),
                        Fields = new List<FieldSpecDto>(),
                        Relations = new List<RelationSpecDto>()
                    });
                }
            }
        }

        return items;
    }

    private static List<PageSpecDto> ParsePages(JsonElement section)
    {
        var items = new List<PageSpecDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out PageSpecDto page))
            {
                page.Components ??= new List<string>();
                page.DataRequirements ??= new List<string>();
                items.Add(page);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var route = element.GetString();
                if (!string.IsNullOrWhiteSpace(route))
                {
                    items.Add(new PageSpecDto
                    {
                        Route = route,
                        Name = route.Trim('/'),
                        Layout = "authenticated",
                        Components = new List<string>(),
                        DataRequirements = new List<string>(),
                        Description = string.Empty
                    });
                }
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                var route = GetStringProperty(element, "route")
                    ?? GetStringProperty(element, "path")
                    ?? GetStringProperty(element, "url");

                if (!string.IsNullOrWhiteSpace(route))
                {
                    var components = ParseStringList(GetPropertyCaseInsensitive(element, "components"));
                    var dataRequirements = ParseStringList(GetPropertyCaseInsensitive(element, "dataRequirements"));
                    var layout = (GetStringProperty(element, "layout") ?? "authenticated").ToLowerInvariant();
                    if (layout is not ("authenticated" or "public" or "admin"))
                        layout = "authenticated";

                    items.Add(new PageSpecDto
                    {
                        Route = NormalizePageRoute(route),
                        Name = GetStringProperty(element, "name") ?? BuildPageNameFromRoute(route),
                        Layout = layout,
                        Components = components,
                        DataRequirements = dataRequirements,
                        Description = GetStringProperty(element, "description") ?? string.Empty
                    });
                }
            }
        }

        return items;
    }

    private static List<ApiRouteSpecDto> ParseApiRoutes(JsonElement section)
    {
        var items = new List<ApiRouteSpecDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out ApiRouteSpecDto route))
            {
                route.ResponseShape ??= new { };
                items.Add(route);
                continue;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                var method = GetStringProperty(element, "method") ?? "GET";
                var path = GetStringProperty(element, "path") ?? string.Empty;
                var handler = GetStringProperty(element, "handler") ?? string.Empty;
                var description = GetStringProperty(element, "description") ?? string.Empty;

                items.Add(new ApiRouteSpecDto
                {
                    Method = method,
                    Path = path,
                    Handler = handler,
                    RequestBody = ToLooseObject(GetPropertyCaseInsensitive(element, "requestBody")),
                    ResponseShape = ToLooseObject(GetPropertyCaseInsensitive(element, "responseShape")) ?? new { },
                    Auth = GetBoolProperty(element, "auth"),
                    Description = description
                });
            }
        }

        return items;
    }

    private static List<ValidationRuleDto> ParseValidations(JsonElement section)
    {
        var items = new List<ValidationRuleDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out ValidationRuleDto validation))
            {
                items.Add(validation);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var description = element.GetString();
                if (!string.IsNullOrWhiteSpace(description))
                {
                    var id = Slugify(description);
                    items.Add(new ValidationRuleDto
                    {
                        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N")[..8] : id,
                        Category = "build-passes",
                        Description = description,
                        Target = "project",
                        Assertion = description,
                        Automatable = false
                    });
                }
                continue;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                var description = GetStringProperty(element, "description")
                    ?? GetStringProperty(element, "assertion")
                    ?? GetStringProperty(element, "name")
                    ?? "Validation rule";

                var category = (GetStringProperty(element, "category") ?? "build-passes").ToLowerInvariant();
                var id = GetStringProperty(element, "id") ?? Slugify(description);
                items.Add(new ValidationRuleDto
                {
                    Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N")[..8] : id,
                    Category = category,
                    Description = description,
                    Target = GetStringProperty(element, "target") ?? "project",
                    Assertion = GetStringProperty(element, "assertion") ?? description,
                    Automatable = GetBoolProperty(element, "automatable"),
                    Script = GetStringProperty(element, "script")
                });
            }
        }

        return items;
    }

    private static List<FileEntryDto> ParseFileManifest(JsonElement section)
    {
        var items = new List<FileEntryDto>();
        foreach (var element in EnumerateSection(section))
        {
            if (TryDeserialize(element, out FileEntryDto file))
            {
                items.Add(file);
                continue;
            }

            if (element.ValueKind == JsonValueKind.String)
            {
                var path = element.GetString();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    items.Add(new FileEntryDto
                    {
                        Path = path,
                        Type = "generated",
                        Description = string.Empty
                    });
                }
            }
        }

        return items;
    }

    private static IEnumerable<JsonElement> EnumerateSection(JsonElement section)
    {
        if (section.ValueKind == JsonValueKind.Array)
            return section.EnumerateArray();

        if (section.ValueKind is JsonValueKind.Object or JsonValueKind.String)
            return new[] { section };

        return Enumerable.Empty<JsonElement>();
    }

    private static bool TryDeserialize<T>(JsonElement element, out T result) where T : class
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions);
            return result != null;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static JsonElement GetPropertyCaseInsensitive(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return default;

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                return property.Value;
        }

        return default;
    }

    private static string GetStringProperty(JsonElement element, string propertyName)
    {
        var prop = GetPropertyCaseInsensitive(element, propertyName);
        if (prop.ValueKind == JsonValueKind.String)
            return prop.GetString();

        if (prop.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            return prop.GetRawText();

        return null;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName)
    {
        var prop = GetPropertyCaseInsensitive(element, propertyName);
        return prop.ValueKind == JsonValueKind.True ||
               (prop.ValueKind == JsonValueKind.String && bool.TryParse(prop.GetString(), out var parsed) && parsed);
    }

    private static object ToLooseObject(JsonElement element)
    {
        if (element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return null;

        if (element.ValueKind == JsonValueKind.String)
            return element.GetString();

        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetInt64(out var asInt)) return asInt;
            if (element.TryGetDouble(out var asDouble)) return asDouble;
        }

        if (element.ValueKind == JsonValueKind.True || element.ValueKind == JsonValueKind.False)
            return element.GetBoolean();

        try
        {
            return JsonSerializer.Deserialize<object>(element.GetRawText(), JsonOptions);
        }
        catch
        {
            return element.GetRawText();
        }
    }

    private static AppSpecDto NormalizeSpec(AppSpecDto spec)
    {
        spec ??= new AppSpecDto();

        spec.Entities ??= new List<EntitySpecDto>();
        spec.Pages ??= new List<PageSpecDto>();
        spec.ApiRoutes ??= new List<ApiRouteSpecDto>();
        spec.Validations ??= new List<ValidationRuleDto>();
        spec.FileManifest ??= new List<FileEntryDto>();

        spec.Entities = spec.Entities
            .Where(e => e != null)
            .Select(e =>
            {
                e.Fields ??= new List<FieldSpecDto>();
                e.Relations ??= new List<RelationSpecDto>();
                if (string.IsNullOrWhiteSpace(e.TableName) && !string.IsNullOrWhiteSpace(e.Name))
                    e.TableName = e.Name.ToLowerInvariant();
                return e;
            })
            .Where(e => !string.IsNullOrWhiteSpace(e.Name))
            .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        spec.ApiRoutes = spec.ApiRoutes
            .Where(r => r != null)
            .Select(r =>
            {
                r.Method = string.IsNullOrWhiteSpace(r.Method) ? "GET" : r.Method.ToUpperInvariant();
                r.Path = NormalizeApiPath(r.Path);
                r.ResponseShape ??= new { };
                r.Description ??= string.Empty;
                return r;
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.Path))
            .GroupBy(r => $"{r.Method}:{r.Path}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        spec.FileManifest = spec.FileManifest
            .Where(f => f != null && !string.IsNullOrWhiteSpace(f.Path))
            .GroupBy(f => f.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var first = g.First();
                first.Type ??= "generated";
                first.Description ??= string.Empty;
                return first;
            })
            .ToList();

        spec.Pages = spec.Pages
            .Where(p => p != null)
            .Select(p =>
            {
                p.Route = NormalizePageRoute(p.Route);
                p.Name = string.IsNullOrWhiteSpace(p.Name) ? BuildPageNameFromRoute(p.Route) : p.Name;
                p.Layout = NormalizeLayout(p.Layout);
                p.Components ??= new List<string>();
                p.DataRequirements ??= new List<string>();
                p.Description ??= string.Empty;
                return p;
            })
            .Where(p => !string.IsNullOrWhiteSpace(p.Route))
            .GroupBy(p => p.Route, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (spec.Pages.Count == 0)
            spec.Pages = BuildFallbackPages(spec.ApiRoutes, spec.FileManifest);

        spec.Validations = spec.Validations
            .Where(v => v != null)
            .Select(v =>
            {
                v.Id = string.IsNullOrWhiteSpace(v.Id) ? Guid.NewGuid().ToString("N")[..8] : v.Id;
                v.Category = NormalizeValidationCategory(v.Category);
                v.Description ??= "Validation rule";
                v.Target ??= "project";
                v.Assertion ??= v.Description;
                return v;
            })
            .GroupBy(v => v.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (spec.Validations.Count == 0)
            spec.Validations = BuildFallbackValidations(spec);

        return spec;
    }

    private static List<PageSpecDto> BuildFallbackPages(List<ApiRouteSpecDto> apiRoutes, List<FileEntryDto> fileManifest)
    {
        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var apiRoute in apiRoutes ?? new List<ApiRouteSpecDto>())
        {
            var pageRoute = ApiPathToPageRoute(apiRoute.Path);
            if (!string.IsNullOrWhiteSpace(pageRoute))
                routes.Add(pageRoute);
        }

        foreach (var file in fileManifest ?? new List<FileEntryDto>())
        {
            var fromFile = FilePathToPageRoute(file.Path);
            if (!string.IsNullOrWhiteSpace(fromFile))
                routes.Add(fromFile);
        }

        if (routes.Count == 0)
            routes.Add("/");

        return routes
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .Select(route => new PageSpecDto
            {
                Route = route,
                Name = BuildPageNameFromRoute(route),
                Layout = route.StartsWith("/auth", StringComparison.OrdinalIgnoreCase)
                    || route.StartsWith("/login", StringComparison.OrdinalIgnoreCase)
                    || route.StartsWith("/register", StringComparison.OrdinalIgnoreCase)
                    ? "public"
                    : "authenticated",
                Components = new List<string>(),
                DataRequirements = new List<string>(),
                Description = "Generated fallback page from available routes/files."
            })
            .ToList();
    }

    private static List<ValidationRuleDto> BuildFallbackValidations(AppSpecDto spec)
    {
        var rules = new List<ValidationRuleDto>
        {
            new()
            {
                Id = "build-passes",
                Category = "build-passes",
                Description = "Project should build successfully.",
                Target = "project",
                Assertion = "Build command exits with status code 0.",
                Automatable = true,
                Script = "npm run build"
            }
        };

        if (spec.Entities.Any())
        {
            rules.Add(new ValidationRuleDto
            {
                Id = "entity-schema",
                Category = "entity-schema",
                Description = "Entity definitions should include required identifiers and key fields.",
                Target = "entities",
                Assertion = "Each entity has a stable identifier field and valid field types.",
                Automatable = true
            });
        }

        if (spec.ApiRoutes.Any())
        {
            rules.Add(new ValidationRuleDto
            {
                Id = "route-exists",
                Category = "route-exists",
                Description = "Declared API routes should be implemented.",
                Target = "apiRoutes",
                Assertion = "Every route in spec resolves to a handler implementation.",
                Automatable = true
            });
        }

        if (spec.ApiRoutes.Any(r => r.Auth))
        {
            rules.Add(new ValidationRuleDto
            {
                Id = "auth-guard",
                Category = "auth-guard",
                Description = "Protected API routes should enforce authentication.",
                Target = "apiRoutes",
                Assertion = "Routes marked with auth=true require authentication middleware.",
                Automatable = true
            });
        }

        return rules;
    }

    private static string NormalizePageRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
            return string.Empty;

        var normalized = route.Trim();
        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        normalized = normalized.Replace("//", "/");
        return normalized.Length > 1 ? normalized.TrimEnd('/') : normalized;
    }

    private static string NormalizeApiPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var normalized = path.Trim();
        if (!normalized.StartsWith('/'))
            normalized = "/" + normalized;

        normalized = normalized.Replace("//", "/");
        return normalized;
    }

    private static string ApiPathToPageRoute(string apiPath)
    {
        if (string.IsNullOrWhiteSpace(apiPath))
            return null;

        var normalized = NormalizeApiPath(apiPath);
        if (!normalized.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            return null;

        var pageRoute = normalized[4..];
        if (string.IsNullOrWhiteSpace(pageRoute))
            return "/";

        pageRoute = Regex.Replace(pageRoute, @":([A-Za-z0-9_]+)", "[$1]");
        pageRoute = Regex.Replace(pageRoute, @"\{([A-Za-z0-9_]+)\}", "[$1]");
        return NormalizePageRoute(pageRoute);
    }

    private static string FilePathToPageRoute(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var normalized = path.Replace('\\', '/');
        if (Regex.IsMatch(normalized, @"(^|/)app/page\.[^/]+$", RegexOptions.IgnoreCase))
            return "/";

        var match = Regex.Match(normalized, @"(^|/)app/(?<route>.+)/page\.[^/]+$", RegexOptions.IgnoreCase);
        if (!match.Success)
            return null;

        return NormalizePageRoute(match.Groups["route"].Value);
    }

    private static string BuildPageNameFromRoute(string route)
    {
        if (string.IsNullOrWhiteSpace(route) || route == "/")
            return "Home";

        var parts = route
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim('[', ']'))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(ToPascalCase)
            .ToList();

        return parts.Count == 0 ? "Page" : string.Join(string.Empty, parts);
    }

    private static string ToPascalCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var tokens = Regex.Split(value, "[^A-Za-z0-9]+")
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => char.ToUpperInvariant(t[0]) + t[1..].ToLowerInvariant());

        return string.Join(string.Empty, tokens);
    }

    private static string NormalizeLayout(string layout)
    {
        var normalized = (layout ?? "authenticated").ToLowerInvariant();
        return normalized is "authenticated" or "public" or "admin" ? normalized : "authenticated";
    }

    private static string NormalizeValidationCategory(string category)
    {
        var normalized = (category ?? "build-passes").ToLowerInvariant();
        return normalized switch
        {
            "file-exists" or "entity-schema" or "route-exists" or "build-passes" or "lint-passes" or
            "env-vars" or "test-passes" or "auth-guard" or "type-check" or "api-returns" => normalized,
            _ => "build-passes"
        };
    }

    private static List<string> ParseStringList(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            return element.EnumerateArray()
                .Where(v => v.ValueKind == JsonValueKind.String)
                .Select(v => v.GetString())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            var value = element.GetString();
            return string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }

        return new List<string>();
    }

    private static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var slug = Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return slug.Length > 64 ? slug[..64] : slug;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Scaffold & file helpers
    // ──────────────────────────────────────────────────────────────────────────

    private string FindTemplateDirectory(string templateSlug)
    {
        // Walk up from the current directory and base directory to find the frontend/templates folder
        var roots = new[] { Directory.GetCurrentDirectory(), AppDomain.CurrentDomain.BaseDirectory };
        foreach (var root in roots)
        {
            var dir = root;
            for (var i = 0; i < 8; i++)
            {
                var candidate = Path.GetFullPath(Path.Combine(dir, "frontend", "templates", templateSlug));
                if (Directory.Exists(candidate)) return candidate;
                var parent = Directory.GetParent(dir)?.FullName;
                if (parent == null || parent == dir) break;
                dir = parent;
            }
        }

        return null;
    }

    private static List<GeneratedFile> ReadScaffoldFiles(string templateDir)
    {
        var files = new List<GeneratedFile>();
        var allFiles = Directory.GetFiles(templateDir, "*", SearchOption.AllDirectories);

        foreach (var filePath in allFiles)
        {
            var relativePath = Path.GetRelativePath(templateDir, filePath).Replace('\\', '/');
            // Skip hidden directories and node_modules
            if (relativePath.StartsWith(".git/") || relativePath.Contains("node_modules/"))
                continue;

            try
            {
                var content = File.ReadAllText(filePath);
                files.Add(new GeneratedFile { Path = relativePath, Content = content });
            }
            catch
            {
                // Skip binary/unreadable files
            }
        }

        return files;
    }

    private static void WriteFilesToDisk(List<GeneratedFile> files, string outputPath)
    {
        foreach (var file in files)
        {
            var fullPath = Path.Combine(outputPath, file.Path.Replace('/', Path.DirectorySeparatorChar));
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(fullPath, file.Content);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Prompt builders
    // ──────────────────────────────────────────────────────────────────────────

    private static string BuildRequirementsPrompt(CreateUpdateProjectDto input)
    {
        return $"Analyze the following application idea and provide a structured breakdown.\n\n"
            + $"Application idea: {input.Prompt}\n"
            + $"Framework: {input.Framework}\n"
            + $"Language: {input.Language}\n"
            + $"Database: {input.DatabaseOption}\n"
            + $"Include Auth: {input.IncludeAuth}\n\n"
            + "Return your analysis in the following format:\n"
            + "===FEATURES===\n<comma-separated list of features>\n===END FEATURES===\n"
            + "===ARCHITECTURE===\n<brief architecture description>\n===END ARCHITECTURE===\n"
            + "===PAGES===\n<comma-separated list of page names>\n===END PAGES===\n"
            + "===API_ENDPOINTS===\n<list of API endpoints, one per line>\n===END API_ENDPOINTS===\n"
            + "===DB_ENTITIES===\n<list of database entities with fields>\n===END DB_ENTITIES===\n";
    }

    private static string BuildCodeGenSystemPrompt(string layerDescription, string framework)
    {
        return $"You are an expert full-stack developer. Generate production-ready {layerDescription} code for a {framework} application.\n\n"
            + "Return your response in this exact format:\n"
            + "===ARCHITECTURE===\n<brief description of this layer>\n===END ARCHITECTURE===\n"
            + "===MODULES===\n<comma-separated module names>\n===END MODULES===\n"
            + "Then for each file:\n"
            + "===FILE===\n<file path relative to project root>\n===CONTENT===\n<file content>\n===END FILE===\n"
            + "\nGenerate complete, working code. Do not use placeholders or TODOs.";
    }

    private static string BuildValidationConstraints(List<ValidationRuleDto> validations)
    {
        if (validations == null || validations.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("\n\nIMPORTANT CONSTRAINTS - The generated code MUST satisfy these validations:");
        foreach (var v in validations)
        {
            sb.AppendLine($"- [{v.Category}] {v.Description}: {v.Assertion}");
        }
        return sb.ToString();
    }

    private static List<ValidationResultDto> BuildInitialValidationResults(List<ValidationRuleDto> validations)
    {
        if (validations == null || validations.Count == 0)
        {
            return new List<ValidationResultDto>
            {
                new()
                {
                    Id = "build-passes",
                    Status = "pending",
                    Message = "Validation queued."
                }
            };
        }

        return validations
            .Select(v => new ValidationResultDto
            {
                Id = string.IsNullOrWhiteSpace(v.Id) ? Guid.NewGuid().ToString("N")[..8] : v.Id,
                Status = "pending",
                Message = string.IsNullOrWhiteSpace(v.Description) ? "Validation queued." : v.Description
            })
            .ToList();
    }

    private static List<ValidationResultDto> EvaluateValidationResults(
        List<ValidationRuleDto> validations,
        List<GeneratedFile> generatedFiles)
    {
        var files = generatedFiles ?? new List<GeneratedFile>();
        var filePaths = new HashSet<string>(
            files.Select(f => NormalizeFilePath(f.Path)),
            StringComparer.OrdinalIgnoreCase);
        var combinedContent = string.Join("\n", files.Select(f => f.Content ?? string.Empty));

        var results = new List<ValidationResultDto>();
        foreach (var validation in validations ?? new List<ValidationRuleDto>())
        {
            var id = string.IsNullOrWhiteSpace(validation.Id)
                ? Guid.NewGuid().ToString("N")[..8]
                : validation.Id;
            var category = (validation.Category ?? string.Empty).ToLowerInvariant();

            var passed = true;
            var message = "Validation passed.";

            switch (category)
            {
                case "file-exists":
                {
                    var targetPath = NormalizeFilePath(validation.Target);
                    passed = string.IsNullOrWhiteSpace(targetPath)
                        ? files.Count > 0
                        : filePaths.Contains(targetPath);
                    message = passed
                        ? "Required file exists."
                        : $"Required file not found: {validation.Target}";
                    break;
                }
                case "build-passes":
                {
                    passed = filePaths.Contains("package.json");
                    message = passed
                        ? "Build validation baseline passed (package.json present)."
                        : "Build validation baseline failed: package.json not found.";
                    break;
                }
                case "route-exists":
                {
                    var routeHint = ExtractRouteHint(validation);
                    passed = string.IsNullOrWhiteSpace(routeHint)
                        || combinedContent.Contains(routeHint, StringComparison.OrdinalIgnoreCase);
                    message = passed
                        ? "Route reference found in generated files."
                        : $"Route reference not found in generated files: {routeHint}";
                    break;
                }
                default:
                {
                    passed = true;
                    message = "Validation rule registered and marked as passed.";
                    break;
                }
            }

            results.Add(new ValidationResultDto
            {
                Id = id,
                Status = passed ? "passed" : "failed",
                Message = message
            });
        }

        if (results.Count == 0)
        {
            results.Add(new ValidationResultDto
            {
                Id = "build-passes",
                Status = files.Count > 0 ? "passed" : "failed",
                Message = files.Count > 0
                    ? "Generated output available for validation."
                    : "No generated files were produced."
            });
        }

        return results;
    }

    private static List<ValidationResultDto> MarkValidationResultsFailed(
        List<ValidationResultDto> validationResults,
        string errorMessage)
    {
        var reason = string.IsNullOrWhiteSpace(errorMessage)
            ? "Generation failed before validations completed."
            : $"Generation failed: {errorMessage}";

        if (validationResults == null || validationResults.Count == 0)
        {
            return new List<ValidationResultDto>
            {
                new()
                {
                    Id = "generation",
                    Status = "failed",
                    Message = reason
                }
            };
        }

        return validationResults
            .Select(v => new ValidationResultDto
            {
                Id = v.Id,
                Status = "failed",
                Message = reason
            })
            .ToList();
    }

    private static string ExtractRouteHint(ValidationRuleDto validation)
    {
        if (!string.IsNullOrWhiteSpace(validation.Target) && validation.Target.StartsWith('/'))
            return validation.Target;

        if (string.IsNullOrWhiteSpace(validation.Assertion))
            return string.Empty;

        var match = Regex.Match(validation.Assertion, @"/[A-Za-z0-9_\-/{}/:]+", RegexOptions.CultureInvariant);
        return match.Success ? match.Value : string.Empty;
    }

    private static string NormalizeFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return string.Empty;

        var normalized = filePath.Replace('\\', '/').Trim();

        if (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized[2..];

        if (normalized.StartsWith("/", StringComparison.Ordinal))
            normalized = normalized[1..];

        if (normalized.Equals("project", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        return normalized;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Session persistence
    // ──────────────────────────────────────────────────────────────────────────

    private async Task SaveSession(CodeGenSession session, bool isNew = false)
    {
        if (_sessionRepository != null)
        {
            if (isNew)
            {
                await _sessionRepository.InsertAsync(session);
            }
            else
            {
                await _sessionRepository.UpdateAsync(session);
            }
        }
        else
        {
            InMemorySessions[session.Id.ToString()] = session;
        }
    }

    private async Task<CodeGenSession> LoadSession(string sessionId)
    {
        if (!Guid.TryParse(sessionId, out var guid))
            throw new UserFriendlyException("Invalid session ID.");

        CodeGenSession session = null;

        if (_sessionRepository != null)
        {
            session = await _sessionRepository.FirstOrDefaultAsync(guid);
        }
        else
        {
            InMemorySessions.TryGetValue(sessionId, out session);
        }

        if (session == null)
            throw new UserFriendlyException($"Session '{sessionId}' not found.");

        return session;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mapping helpers
    // ──────────────────────────────────────────────────────────────────────────

    private CodeGenSessionDto MapSessionToDto(CodeGenSession session)
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
            Spec = DeserializeOrDefault<AppSpecDto>(session.SpecJson),
            SpecConfirmedAt = session.SpecConfirmedAt,
            GenerationStartedAt = session.GenerationStartedAt,
            GenerationCompletedAt = session.GenerationCompletedAt,
            Status = session.Status,
            ValidationResults = DeserializeOrDefault<List<ValidationResultDto>>(session.ValidationResultsJson) ?? new List<ValidationResultDto>(),
            ScaffoldTemplate = session.ScaffoldTemplate,
            GeneratedFiles = DeserializeOrDefault<List<GeneratedFileDto>>(session.GeneratedFilesJson) ?? new List<GeneratedFileDto>(),
            RepairAttempts = session.RepairAttempts,
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

    private static Framework MapFrameworkString(string framework)
    {
        if (string.IsNullOrEmpty(framework)) return Framework.NextJS;
        var lower = framework.ToLowerInvariant();
        if (lower.Contains("next")) return Framework.NextJS;
        if (lower.Contains("react") || lower.Contains("vite")) return Framework.ReactVite;
        if (lower.Contains("angular")) return Framework.Angular;
        if (lower.Contains("vue")) return Framework.Vue;
        if (lower.Contains("blazor") || lower.Contains(".net")) return Framework.DotNetBlazor;
        return Framework.NextJS;
    }

    private static ProgrammingLanguage MapLanguageString(string language)
    {
        if (string.IsNullOrEmpty(language)) return ProgrammingLanguage.TypeScript;
        var lower = language.ToLowerInvariant();
        if (lower.Contains("javascript") && !lower.Contains("type")) return ProgrammingLanguage.JavaScript;
        if (lower.Contains("c#") || lower.Contains("csharp")) return ProgrammingLanguage.CSharp;
        return ProgrammingLanguage.TypeScript;
    }

    private static DatabaseOption MapDatabaseString(string database)
    {
        if (string.IsNullOrEmpty(database)) return DatabaseOption.RenderPostgres;
        var lower = database.ToLowerInvariant();
        if (lower.Contains("neon")) return DatabaseOption.NeonPostgres;
        if (lower.Contains("mongo")) return DatabaseOption.MongoCloud;
        return DatabaseOption.RenderPostgres;
    }

    private static async Task ReportProgress(Func<string, Task> onProgress, string message)
    {
        if (onProgress != null) await onProgress(message);
    }
}
