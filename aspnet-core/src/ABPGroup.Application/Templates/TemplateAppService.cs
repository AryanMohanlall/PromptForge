using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using ABPGroup.Authorization.Users;
using ABPGroup.Templates.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ABPGroup.Templates;

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

        // Everyone should be able to browse/create templates once authenticated.
        // Permissions are intentionally disabled for this app service.
        GetPermissionName = null;
        GetAllPermissionName = null;
        CreatePermissionName = null;
        UpdatePermissionName = null;
        DeletePermissionName = null;
    }

    public override async Task<TemplateDto> CreateAsync(CreateUpdateTemplateDto input)
    {
        var entity = ObjectMapper.Map<Template>(input);

        // Set TenantId from current session
        entity.TenantId = AbpSession.TenantId;

        // If Author is not provided, try to get the current user's name
        if (string.IsNullOrWhiteSpace(entity.Author) && AbpSession.UserId.HasValue)
        {
            var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            entity.Author = user?.FullName ?? user?.UserName;
        }

        // Set status to Active by default so templates appear in marketplace
        if (entity.Status == TemplateStatus.Draft)
        {
            entity.Status = TemplateStatus.Active;
        }

        await Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEntityDto(entity);
    }


    public async Task<PagedResultDto<TemplateDto>> GetListAsync(TemplateListInput input)
    {
        // Marketplace queries must cross tenant boundaries — disable the MayHaveTenant
        // filter so active templates from all tenants are visible.
        // My Templates keeps the filter enabled and scopes by TenantId in the query.
        using (input.IsMyTemplates != true
            ? CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant)
            : null)
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
    }

    // ABP's AsyncCrudAppService exposes both GetListAsync and GetAllAsync.
    // Our frontend uses GetAll, so keep the behavior consistent between them.
    public override async Task<PagedResultDto<TemplateDto>> GetAllAsync(TemplateListInput input)
    {
        return await GetListAsync(input);
    }

    public async Task<TemplateDto> GetAsync(int id)
    {
        var entity = await Repository.GetAsync(id);
        var dto = MapToEntityDto(entity);

        if (AbpSession.UserId.HasValue)
        {
            dto.IsFavorite = await _favoriteRepository.GetAll()
                .AnyAsync(x => x.UserId == AbpSession.UserId.Value && x.TemplateId == id);
        }

        return dto;
    }

    public async Task<TemplateDto> UpdateAsync(int id, CreateUpdateTemplateDto input)
    {
        input.Id = id;
        return await base.UpdateAsync(input);
    }

    public async Task DeleteAsync(int id)
    {
        await base.DeleteAsync(new EntityDto<int>(id));
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

    protected override IQueryable<Template> CreateFilteredQuery(TemplateListInput input)
    {
        var query = Repository.GetAll();

        if (input.IsMyTemplates == true && AbpSession.TenantId.HasValue)
        {
            // Only templates created by the current tenant
            query = query.Where(x => x.TenantId == AbpSession.TenantId.Value);
        }
        else
        {
            // Marketplace: only publish-ready templates across all tenants.
            // MayHaveTenant filter is disabled by the caller for this branch.
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
}