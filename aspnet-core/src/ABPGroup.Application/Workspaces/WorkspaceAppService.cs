using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Abp.UI;
using ABPGroup.MultiTenancy;
using ABPGroup.Workspaces.Dto;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ABPGroup.Workspaces;

public class WorkspaceAppService : AsyncCrudAppService<Tenant, WorkspaceDto, int, PagedWorkspaceResultRequestDto, CreateWorkspaceDto, WorkspaceDto>, IWorkspaceAppService
{
    private readonly TenantManager _tenantManager;

    public WorkspaceAppService(IRepository<Tenant, int> repository, TenantManager tenantManager)
        : base(repository)
    {
        _tenantManager = tenantManager;
        GetPermissionName = null;
        GetAllPermissionName = null;
        CreatePermissionName = null;
        UpdatePermissionName = null;
        DeletePermissionName = null;
    }

    public override async Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input)
    {
        var tenancyName = await BuildUniqueTenancyNameAsync(input.TenancyName, input.Name);

        var tenant = new Tenant(tenancyName, input.Name)
        {
            IsActive = input.IsActive
        };

        await Repository.InsertAsync(tenant);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEntityDto(tenant);
    }

    public override async Task<WorkspaceDto> GetAsync(EntityDto<int> input)
    {
        if (input.Id <= 0)
        {
            if (AbpSession.TenantId.HasValue && AbpSession.TenantId.Value > 0)
            {
                input.Id = AbpSession.TenantId.Value;
            }
            else
            {
                throw new UserFriendlyException("Workspace id is required.");
            }
        }

        return await base.GetAsync(input);
    }

    protected override IQueryable<Tenant> CreateFilteredQuery(PagedWorkspaceResultRequestDto input)
    {
        return Repository.GetAll()
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(),
                x => x.Name.Contains(input.Keyword) || x.TenancyName.Contains(input.Keyword));
    }

    protected override IQueryable<Tenant> ApplySorting(IQueryable<Tenant> query, PagedWorkspaceResultRequestDto input)
    {
        if (!input.Sorting.IsNullOrWhiteSpace())
        {
            return query.OrderBy(input.Sorting);
        }

        return query.OrderByDescending(x => x.CreationTime);
    }

    protected override void MapToEntity(WorkspaceDto updateInput, Tenant entity)
    {
        entity.Name = updateInput.Name;
        entity.TenancyName = updateInput.TenancyName;
        entity.IsActive = updateInput.IsActive;
    }

    private async Task<string> BuildUniqueTenancyNameAsync(string tenancyName, string name)
    {
        var candidate = tenancyName.IsNullOrWhiteSpace() ? name : tenancyName;
        candidate = Regex.Replace(candidate, "[^a-zA-Z0-9_-]", string.Empty);

        if (candidate.IsNullOrWhiteSpace())
        {
            candidate = "workspace";
        }

        if (!Regex.IsMatch(candidate, AbpTenantBase.TenancyNameRegex))
        {
            candidate = $"w{candidate}";
        }

        var normalized = candidate;
        var suffix = 1;
        while (await _tenantManager.FindByTenancyNameAsync(normalized) != null)
        {
            normalized = $"{candidate}{suffix++}";
        }

        if (!Regex.IsMatch(normalized, AbpTenantBase.TenancyNameRegex))
        {
            throw new UserFriendlyException("Unable to generate a valid unique workspace tenancy name.");
        }

        return normalized;
    }
}
