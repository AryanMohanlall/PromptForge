using Abp.Application.Services;
using ABPGroup.Deployments.Dto;

namespace ABPGroup.Deployments
{
    public interface IDeploymentAppService
        : IAsyncCrudAppService<DeploymentDto, long, PagedDeploymentResultRequestDto, CreateUpdateDeploymentDto, CreateUpdateDeploymentDto>
    {
    }
}
