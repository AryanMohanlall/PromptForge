using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using ABPGroup.MultiTenancy;
using ABPGroup.CodeGen;
using ABPGroup.Projects.Dto;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ABPGroup.Projects;

public class ProjectAppService : AsyncCrudAppService<Project, ProjectDto, long, PagedProjectResultRequestDto, CreateUpdateProjectDto, CreateUpdateProjectDto>, IProjectAppService
{
    private readonly IRepository<Prompt, long> _promptRepository;
    private readonly IRepository<Tenant, int> _tenantRepository;
    private readonly TenantManager _tenantManager;
    private readonly ICodeGenAppService _codeGenAppService;

    public ProjectAppService(
        IRepository<Project, long> repository,
        IRepository<Prompt, long> promptRepository,
        IRepository<Tenant, int> tenantRepository,
        TenantManager tenantManager,
        ICodeGenAppService codeGenAppService)
        : base(repository)
    {
        _promptRepository = promptRepository;
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _codeGenAppService = codeGenAppService;
        GetPermissionName = null;
        GetAllPermissionName = null;
        CreatePermissionName = null;
        UpdatePermissionName = null;
        DeletePermissionName = null;
    }

    public override async Task<PagedResultDto<ProjectDto>> GetAllAsync(PagedProjectResultRequestDto input)
    {
        if (AbpSession.TenantId.HasValue && AbpSession.TenantId.Value > 0)
        {
            input.WorkspaceId = AbpSession.TenantId.Value;
        }

        return await base.GetAllAsync(input);
    }

    public override async Task<ProjectDto> CreateAsync(CreateUpdateProjectDto input)
    {
        Logger.Info($"Creating project with name: {input.Name}");
        if (input.PromptId.HasValue && input.PromptId.Value <= 0)
        {
            input.PromptId = null;
        }

        if (input.WorkspaceId.HasValue && input.WorkspaceId.Value <= 0)
        {
            input.WorkspaceId = null;
        }

        if (input.PromptVersion <= 0)
        {
            input.PromptVersion = 1;
        }

        Logger.Info($"Resolved WorkspaceId: {input.WorkspaceId}, PromptId: {input.PromptId}, PromptVersion: {input.PromptVersion}");
        input.WorkspaceId = await ResolveWorkspaceIdAsync(input);

        var entity = MapToEntity(input);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        if (input.PromptSubmittedAt.HasValue)
        {
            entity.Status = ProjectStatus.PromptSubmitted;
        }

        Logger.Info($"Inserting project entity into database. Name: {entity.Name}, WorkspaceId: {entity.WorkspaceId}, PromptVersion: {input.PromptVersion}");
        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        var prompt = await CreatePromptRecordAsync(entity.Id, input);
        entity.PromptId = prompt.Id;
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        input.Id = entity.Id;
        input.WorkspaceId = entity.WorkspaceId;
        input.PromptId = entity.PromptId;
        input.PromptSubmittedAt = entity.PromptSubmittedAt;
        input.Status = entity.Status;
        input.PromptVersion = prompt.Version;

        // Only start codegen when the prompt was actually submitted
        if (entity.Status == ProjectStatus.PromptSubmitted)
        {
            entity.Status = ProjectStatus.CodeGenerationInProgress;
            entity.UpdatedAt = DateTime.UtcNow;
            await Repository.UpdateAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();

            Logger.Info($"Queuing background code generation for project: {input.Name} ({entity.Id}).");
            StartCodeGenInBackground(entity.Id, input);
        }

        return MapToEntityDto(entity);
    }

    private void StartCodeGenInBackground(long projectId, CreateUpdateProjectDto input)
    {
        var unitOfWorkManager = UnitOfWorkManager;
        var codeGenAppService = _codeGenAppService;
        var repository = Repository;

        // SuppressFlow so the background task does not inherit the ambient UoW from the HTTP request.
        using (System.Threading.ExecutionContext.SuppressFlow())
        {
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                Console.WriteLine($"[CodeGen] Background task started for project {projectId}");
                using var uow = unitOfWorkManager.Begin();
                try
                {
                    async Task OnProgress(string message)
                    {
                        try
                        {
                            Console.WriteLine($"[CodeGen] Progress ({projectId}): {message}");
                            using var progressUow = unitOfWorkManager.Begin(
                                new Abp.Domain.Uow.UnitOfWorkOptions
                                {
                                    Scope = System.Transactions.TransactionScopeOption.RequiresNew
                                });
                            var p = await repository.GetAsync(projectId);
                            p.StatusMessage = message?.Length > 195 ? message.Substring(0, 192) + "..." : message;
                            p.UpdatedAt = DateTime.UtcNow;
                            await repository.UpdateAsync(p);
                            await progressUow.CompleteAsync();
                        }
                        catch (Exception progressEx)
                        {
                            Console.WriteLine($"[CodeGen] Progress update failed ({projectId}): {progressEx.Message}");
                        }
                    }

                    Console.WriteLine($"[CodeGen] Starting GenerateProjectAsync for project {projectId}");
                    var codeGenResult = await codeGenAppService.GenerateProjectAsync(input, OnProgress);
                    Console.WriteLine($"[CodeGen] GenerateProjectAsync completed for project {projectId}. Files: {codeGenResult?.Files?.Count ?? 0}");

                    var project = await repository.GetAsync(projectId);
                    if (codeGenResult != null)
                    {
                        project.ArchitectureSummary = codeGenResult.ArchitectureSummary;
                        if (codeGenResult.ModuleList?.Count > 0)
                        {
                            var modules = string.Join(",", codeGenResult.ModuleList);
                            project.GeneratedModules = modules.Length > 500 ? modules[..497] + "..." : modules;
                        }
                    }
                    project.Status = ProjectStatus.CodeGenerationCompleted;
                    project.StatusMessage = "Code generation completed";
                    project.UpdatedAt = DateTime.UtcNow;
                    await repository.UpdateAsync(project);
                    await uow.CompleteAsync();
                    Console.WriteLine($"[CodeGen] Project {projectId} marked as completed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CodeGen] FAILED for project {projectId}: {ex}");
                    Logger.Error("Background code generation failed for project " + projectId, ex);
                    try
                    {
                        using var errUow = unitOfWorkManager.Begin();
                        var project = await repository.GetAsync(projectId);
                        project.Status = ProjectStatus.Failed;
                        project.StatusMessage = $"Failed: {(ex.Message.Length > 180 ? ex.Message.Substring(0, 177) + "..." : ex.Message)}";
                        project.UpdatedAt = DateTime.UtcNow;
                        await repository.UpdateAsync(project);
                        await errUow.CompleteAsync();
                    }
                    catch (Exception errEx)
                    {
                        Console.WriteLine($"[CodeGen] Could not update failure status for project {projectId}: {errEx.Message}");
                    }
                }
            });
        }
    }

    public override async Task<ProjectDto> UpdateAsync(CreateUpdateProjectDto input)
    {
        input.WorkspaceId = await ResolveWorkspaceIdAsync(input, createIfMissing: false);

        var entity = await GetEntityByIdAsync(input.Id);
        MapToEntity(input, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        var prompt = await CreatePromptRecordAsync(entity.Id, input);
        entity.PromptId = prompt.Id;

        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEntityDto(entity);
    }

    protected override IQueryable<Project> CreateFilteredQuery(PagedProjectResultRequestDto input)
    {
        return Repository.GetAll()
            .WhereIf(input.WorkspaceId.HasValue, x => x.WorkspaceId == input.WorkspaceId.Value)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(),
                x => x.Name.Contains(input.Keyword) || x.Prompt.Contains(input.Keyword));
    }

    protected override IQueryable<Project> ApplySorting(IQueryable<Project> query, PagedProjectResultRequestDto input)
    {
        if (!input.Sorting.IsNullOrWhiteSpace())
        {
            return query.OrderBy(input.Sorting);
        }

        return query.OrderByDescending(x => x.UpdatedAt);
    }

    private async Task<Prompt> CreatePromptRecordAsync(long projectId, CreateUpdateProjectDto input)
    {
        var version = input.PromptVersion > 0 ? input.PromptVersion : 1;

        var existingPrompt = await _promptRepository.FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Version == version);
        if (existingPrompt != null)
        {
            existingPrompt.Content = input.Prompt;
            existingPrompt.SubmittedAt = input.PromptSubmittedAt;
            await _promptRepository.UpdateAsync(existingPrompt);
            return existingPrompt;
        }

        var prompt = new Prompt
        {
            ProjectId = projectId,
            Content = input.Prompt,
            Version = version,
            SubmittedAt = input.PromptSubmittedAt,
            CreatedAt = DateTime.UtcNow
        };

        await _promptRepository.InsertAsync(prompt);
        await CurrentUnitOfWork.SaveChangesAsync();
        return prompt;
    }

    private async Task<int> ResolveWorkspaceIdAsync(CreateUpdateProjectDto input, bool createIfMissing = true)
    {
        // In tenant context, always bind project workspace to the logged-in tenant.
        if (AbpSession.TenantId.HasValue
            && AbpSession.TenantId.Value > 0
            && await _tenantManager.FindByIdAsync(AbpSession.TenantId.Value) != null)
        {
            return AbpSession.TenantId.Value;
        }

        if (input.WorkspaceId.HasValue && _tenantRepository.GetAll().Any(x => x.Id == input.WorkspaceId.Value))
        {
            return input.WorkspaceId.Value;
        }

        var existingWorkspace = _tenantRepository.GetAll().OrderBy(x => x.Id).FirstOrDefault();
        if (existingWorkspace != null)
        {
            return existingWorkspace.Id;
        }

        if (!createIfMissing)
        {
            throw new Abp.UI.UserFriendlyException("WorkspaceId is invalid. Provide an existing workspace for updates.");
        }

        var baseName = input.Name.IsNullOrWhiteSpace() ? "workspace" : input.Name;
        var tenancyName = Regex.Replace(baseName.ToLowerInvariant(), "[^a-z0-9_-]", string.Empty);
        if (tenancyName.IsNullOrWhiteSpace())
        {
            tenancyName = "workspace";
        }

        if (!Regex.IsMatch(tenancyName, Abp.MultiTenancy.AbpTenantBase.TenancyNameRegex))
        {
            tenancyName = $"w{tenancyName}";
        }

        var suffix = 1;
        var uniqueTenancyName = tenancyName;
        while (await _tenantManager.FindByTenancyNameAsync(uniqueTenancyName) != null)
        {
            uniqueTenancyName = $"{tenancyName}{suffix++}";
        }

        var tenant = new Tenant(uniqueTenancyName, $"Workspace {input.Name}")
        {
            IsActive = true
        };

        await _tenantRepository.InsertAsync(tenant);
        await CurrentUnitOfWork.SaveChangesAsync();

        return tenant.Id;
    }
}
