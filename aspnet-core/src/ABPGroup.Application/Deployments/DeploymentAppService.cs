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
    /// Manages deployments of project repositories to hosting targets.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_Deployments)]
    public class DeploymentAppService
        : AsyncCrudAppService<Deployment, DeploymentDto, long, PagedDeploymentResultRequestDto, CreateUpdateDeploymentDto, CreateUpdateDeploymentDto>,
          IDeploymentAppService
    {
        public DeploymentAppService(IRepository<Deployment, long> repository) : base(repository)
        {
            CreatePermissionName = PermissionNames.Pages_Deployments_Create;
            UpdatePermissionName = PermissionNames.Pages_Deployments_Edit;
            DeletePermissionName = PermissionNames.Pages_Deployments_Delete;
        }

        protected override IQueryable<Deployment> CreateFilteredQuery(PagedDeploymentResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.EnvironmentName.Contains(input.Keyword) || x.Url.Contains(input.Keyword));
        }

        protected override IQueryable<Deployment> ApplySorting(IQueryable<Deployment> query, PagedDeploymentResultRequestDto input)
        {
            return query.OrderByDescending(x => x.TriggeredAt);
        }
    }
}
