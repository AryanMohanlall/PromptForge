using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using ABPGroup.Authorization;
using ABPGroup.Deployments.Dto;
using System.Linq;

namespace ABPGroup.Deployments
{
    /// <summary>
    /// Read-only service for viewing deployment logs.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_DeploymentLogs)]
    public class DeploymentLogAppService
        : AsyncCrudAppService<DeploymentLog, DeploymentLogDto, long, PagedDeploymentLogResultRequestDto>,
          IDeploymentLogAppService
    {
        public DeploymentLogAppService(IRepository<DeploymentLog, long> repository) : base(repository)
        {
            CreatePermissionName = null;
            UpdatePermissionName = null;
            DeletePermissionName = null;
        }

        protected override IQueryable<DeploymentLog> CreateFilteredQuery(PagedDeploymentLogResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.Message.Contains(input.Keyword) || x.Source.Contains(input.Keyword));
        }

        protected override IQueryable<DeploymentLog> ApplySorting(IQueryable<DeploymentLog> query, PagedDeploymentLogResultRequestDto input)
        {
            return query.OrderByDescending(x => x.Timestamp);
        }
    }
}
