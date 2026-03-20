using Abp.Application.Services;
using ABPGroup.Deployments.Dto;

namespace ABPGroup.Deployments
{
    /// <summary>
    /// Read-only service for viewing deployment logs.
    /// </summary>
    public interface IDeploymentLogAppService
        : IAsyncCrudAppService<DeploymentLogDto, long, PagedDeploymentLogResultRequestDto>
    {
    }
}
