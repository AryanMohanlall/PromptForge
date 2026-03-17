using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using ABPGroup.MultiTenancy;
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

    public ProjectAppService(
        IRepository<Project, long> repository,
        IRepository<Prompt, long> promptRepository,
        IRepository<Tenant, int> tenantRepository,
        TenantManager tenantManager)
        : base(repository)
    {
        _promptRepository = promptRepository;
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        GetPermissionName = null;
        GetAllPermissionName = null;
        CreatePermissionName = null;
        UpdatePermissionName = null;
        DeletePermissionName = null;
    }

    public override async Task<ProjectDto> CreateAsync(CreateUpdateProjectDto input)
    {
        input.WorkspaceId = await ResolveWorkspaceIdAsync(input);

        var entity = MapToEntity(input);
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = entity.CreatedAt;
        if (input.PromptSubmittedAt.HasValue)
        {
            entity.Status = ProjectStatus.PromptSubmitted;
        }

        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        var prompt = await CreatePromptRecordAsync(entity.Id, input);
        entity.PromptId = prompt.Id;
        await Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEntityDto(entity);
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
