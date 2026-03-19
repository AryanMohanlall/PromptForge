using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using ABPGroup.Authorization;
using ABPGroup.Templates.Dto;
using System.Linq;
using System.Threading.Tasks;

namespace ABPGroup.Templates;

public class TemplateAppService
    : AsyncCrudAppService<Template, TemplateDto, int, TemplateListInput, CreateUpdateTemplateDto, CreateUpdateTemplateDto, EntityDto<int>, EntityDto<int>>,
      ITemplateAppService
{
    public TemplateAppService(IRepository<Template, int> repository) : base(repository)
    {
        CreatePermissionName = PermissionNames.Pages_Templates_Create;
        UpdatePermissionName = PermissionNames.Pages_Templates_Edit;
        DeletePermissionName = PermissionNames.Pages_Templates_Delete;
    }

    // Tenant-facing — read only
    public async Task<PagedResultDto<TemplateDto>> GetListAsync(TemplateListInput input)
    {
        return await base.GetAllAsync(input);
    }

    public async Task<TemplateDto> GetAsync(int id)
    {
        return await base.GetAsync(new EntityDto<int>(id));
    }

    // PlatformAdministrator only
    public async Task<TemplateDto> UpdateAsync(int id, CreateUpdateTemplateDto input)
    {
        input.Id = id;
        return await base.UpdateAsync(input);
    }

    public async Task DeleteAsync(int id)
    {
        await base.DeleteAsync(new EntityDto<int>(id));
    }

    protected override IQueryable<Template> CreateFilteredQuery(TemplateListInput input)
    {
        return Repository.GetAll()
            .WhereIf(input.Category.HasValue, x => x.Category == input.Category.Value)
            .WhereIf(input.Framework.HasValue, x => x.Framework == input.Framework.Value)
            .WhereIf(input.Database.HasValue, x => x.Database == input.Database.Value)
            .WhereIf(input.IncludesAuth.HasValue, x => x.IncludesAuth == input.IncludesAuth.Value)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(input.IsFeatured.HasValue, x => x.IsFeatured == input.IsFeatured.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.SearchTerm),
                x =>
                    x.Name.Contains(input.SearchTerm) ||
                    x.Description.Contains(input.SearchTerm) ||
                    x.Author.Contains(input.SearchTerm) ||
                    x.Tags.Contains(input.SearchTerm));
    }

    protected override IQueryable<Template> ApplySorting(IQueryable<Template> query, TemplateListInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Sorting))
        {
            return query.OrderByDescending(x => x.IsFeatured)
                        .ThenByDescending(x => x.CreationTime);
        }

        return base.ApplySorting(query, input);
    }

    public async Task<TemplateDto> PublishAsync(int id)
    {
        var template = await Repository.GetAsync(id);
        if (template.Status == TemplateStatus.Draft)
        {
            template.Status = TemplateStatus.Active;
        }
        return MapToEntityDto(template);
    }

    public async Task<TemplateDto> DeprecateAsync(int id)
    {
        var template = await Repository.GetAsync(id);
        if (template.Status == TemplateStatus.Active)
        {
            template.Status = TemplateStatus.Deprecated;
        }
        return MapToEntityDto(template);
    }

    public async Task<TemplateDto> SetFeaturedAsync(int id, bool featured)
    {
        var template = await Repository.GetAsync(id);
        template.IsFeatured = featured;
        return MapToEntityDto(template);
    }
}
