using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using ABPGroup.Authorization;
using ABPGroup.Authorization.Users;
using ABPGroup.Templates.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ABPGroup.Templates;

// [AbpAuthorize(PermissionNames.Pages_Templates)]
public class TemplateAppService
    : AsyncCrudAppService<Template, TemplateDto, int, TemplateListInput, CreateUpdateTemplateDto, CreateUpdateTemplateDto, EntityDto<int>, EntityDto<int>>,
      ITemplateAppService
{
    private readonly IRepository<UserFavoriteTemplate, long> _favoriteRepository;
    private readonly UserManager _userManager;

    public TemplateAppService(
        IRepository<Template, int> repository,
        IRepository<UserFavoriteTemplate, long> favoriteRepository,
        UserManager userManager)
        : base(repository)
    {
        _favoriteRepository = favoriteRepository;
        _userManager = userManager;

        CreatePermissionName = PermissionNames.Pages_Templates_Create;
        UpdatePermissionName = PermissionNames.Pages_Templates_Edit;
        DeletePermissionName = PermissionNames.Pages_Templates_Delete;
    }

    public override async Task<TemplateDto> CreateAsync(CreateUpdateTemplateDto input)
    {
        var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
        var template = MapToEntity(input);

        template.Author = user.FullName;
        template.TenantId = AbpSession.TenantId;
        template.Status = TemplateStatus.Active; // Make active by default when user posts

        await Repository.InsertAsync(template);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEntityDto(template);
    }

    public async Task<PagedResultDto<TemplateDto>> GetListAsync(TemplateListInput input)
    {
        var query = CreateFilteredQuery(input);

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        var entities = await query.ToListAsync();
        var dtos = entities.Select(MapToEntityDto).ToList();


        // Fill IsFavorite
        if (AbpSession.UserId.HasValue)
        {
            var favoriteTemplateIds = await _favoriteRepository.GetAll()
                .Where(x => x.UserId == AbpSession.UserId.Value)
                .Select(x => x.TemplateId)
                .ToListAsync();

            foreach (var dto in dtos)
            {
                dto.IsFavorite = favoriteTemplateIds.Contains(dto.Id);
            }
        }

        return new PagedResultDto<TemplateDto>(totalCount, dtos);
    }

    public override async Task<TemplateDto> GetAsync(EntityDto<int> input)
    {
        var entity = await Repository.GetAsync(input.Id);
        var dto = MapToEntityDto(entity);

        if (AbpSession.UserId.HasValue)
        {
            dto.IsFavorite = await _favoriteRepository.GetAll()
                .AnyAsync(x => x.UserId == AbpSession.UserId.Value && x.TemplateId == input.Id);
        }

        return dto;
    }

    public override async Task<TemplateDto> UpdateAsync(CreateUpdateTemplateDto input)
    {
        return await base.UpdateAsync(input);
    }

    public override async Task DeleteAsync(EntityDto<int> input)
    {
        await base.DeleteAsync(input);
    }

    public async Task ToggleFavoriteAsync(int id)
    {
        var userId = AbpSession.GetUserId();
        var favorite = await _favoriteRepository.GetAll()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TemplateId == id);

        if (favorite == null)
        {
            await _favoriteRepository.InsertAsync(new UserFavoriteTemplate
            {
                UserId = userId,
                TemplateId = id,
                TenantId = AbpSession.TenantId
            });
        }
        else
        {
            await _favoriteRepository.DeleteAsync(favorite);
        }
    }

    protected override IQueryable<Template> CreateFilteredQuery(TemplateListInput input)
    {
        var query = Repository.GetAll();

        if (input.IsMyTemplates == true && AbpSession.TenantId.HasValue)
        {
            // Only templates created by current tenant
            query = query.Where(x => x.TenantId == AbpSession.TenantId.Value);
        }
        else
        {
            // Marketplace: All active templates (can be system templates or from other tenants)
            query = query.Where(x => x.Status == TemplateStatus.Active);
        }

        return query
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
