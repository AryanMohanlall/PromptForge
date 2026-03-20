using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using ABPGroup.Authorization;
using ABPGroup.Builds.Dto;
using System.Linq;

namespace ABPGroup.Builds
{
    /// <summary>
    /// Read-only service for viewing generated artifacts.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_GeneratedArtifacts)]
    public class GeneratedArtifactAppService
        : AsyncCrudAppService<GeneratedArtifact, GeneratedArtifactDto, long, PagedGeneratedArtifactResultRequestDto>,
          IGeneratedArtifactAppService
    {
        public GeneratedArtifactAppService(IRepository<GeneratedArtifact, long> repository) : base(repository)
        {
            CreatePermissionName = null;
            UpdatePermissionName = null;
            DeletePermissionName = null;
        }

        protected override IQueryable<GeneratedArtifact> CreateFilteredQuery(PagedGeneratedArtifactResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.FileName.Contains(input.Keyword) || x.Path.Contains(input.Keyword));
        }

        protected override IQueryable<GeneratedArtifact> ApplySorting(IQueryable<GeneratedArtifact> query, PagedGeneratedArtifactResultRequestDto input)
        {
            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
}
