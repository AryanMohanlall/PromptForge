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
    /// Read-only service for viewing build job history.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_BuildJobs)]
    public class BuildJobAppService
        : AsyncCrudAppService<BuildJob, BuildJobDto, long, PagedBuildJobResultRequestDto>,
          IBuildJobAppService
    {
        public BuildJobAppService(IRepository<BuildJob, long> repository) : base(repository)
        {
            CreatePermissionName = null;
            UpdatePermissionName = null;
            DeletePermissionName = null;
        }

        protected override IQueryable<BuildJob> CreateFilteredQuery(PagedBuildJobResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.CurrentStep.Contains(input.Keyword));
        }

        protected override IQueryable<BuildJob> ApplySorting(IQueryable<BuildJob> query, PagedBuildJobResultRequestDto input)
        {
            return query.OrderByDescending(x => x.StartedAt);
        }
    }
}
