using Abp.Application.Services;
using ABPGroup.Git.Dto;

namespace ABPGroup.Git
{
    public interface IProjectRepositoryAppService
        : IAsyncCrudAppService<ProjectRepositoryDto, long, PagedProjectRepositoryResultRequestDto, CreateUpdateProjectRepositoryDto, CreateUpdateProjectRepositoryDto>
    {
    }
}
