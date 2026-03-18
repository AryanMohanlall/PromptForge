using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using ABPGroup.Authorization;
using ABPGroup.Templates.Dto;
using System.Linq;

namespace ABPGroup.Templates;

[AbpAuthorize(PermissionNames.Pages_Templates)]
public class TemplateAppService
    : AsyncCrudAppService<Template, TemplateDto, int, PagedTemplateResultRequestDto, CreateUpdateTemplateDto, CreateUpdateTemplateDto>,
      ITemplateAppService
{
    public TemplateAppService(IRepository<Template, int> repository) : base(repository)
    {
        CreatePermissionName = PermissionNames.Pages_Templates_Create;
        UpdatePermissionName = PermissionNames.Pages_Templates_Edit;
        DeletePermissionName = PermissionNames.Pages_Templates_Delete;
    }

    protected override IQueryable<Template> CreateFilteredQuery(PagedTemplateResultRequestDto input)
    {
        return Repository.GetAll()
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                x =>
                    (x.Name != null && x.Name.Contains(input.Keyword)) ||
                    (x.Slug != null && x.Slug.Contains(input.Keyword)) ||
                    (x.Description != null && x.Description.Contains(input.Keyword)) ||
                    (x.Category != null && x.Category.Contains(input.Keyword)) ||
                    (x.Tags != null && x.Tags.Contains(input.Keyword)) ||
                    (x.Author != null && x.Author.Contains(input.Keyword)));
    }

    protected override IQueryable<Template> ApplySorting(IQueryable<Template> query, PagedTemplateResultRequestDto input)
    {
        return query.OrderByDescending(x => x.CreationTime);
    }
}
