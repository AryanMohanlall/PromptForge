using Abp.Application.Services;
using ABPGroup.Deployments.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABPGroup.Deployments
{
    public interface IDeploymentAppService
        : IAsyncCrudAppService<DeploymentDto, long, PagedDeploymentResultRequestDto, CreateUpdateDeploymentDto, CreateUpdateDeploymentDto>
    {
        Task<List<DeploymentDto>> GetByProjectId(long projectId);
    }
}
