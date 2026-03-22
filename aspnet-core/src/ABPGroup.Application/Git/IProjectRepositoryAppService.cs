using Abp.Application.Services;
using ABPGroup.Git.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABPGroup.Git
{
    public interface IProjectRepositoryAppService
        : IAsyncCrudAppService<ProjectRepositoryDto, long, PagedProjectRepositoryResultRequestDto, CreateUpdateProjectRepositoryDto, CreateUpdateProjectRepositoryDto>
    {
        Task<List<ProjectRepositoryDto>> GetByProjectId(long projectId);
        Task<ProjectRepositoryDto> GetByExternalIdAsync(string externalId);
    }
}
