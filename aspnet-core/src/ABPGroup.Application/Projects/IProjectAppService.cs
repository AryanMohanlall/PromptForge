using Abp.Application.Services;
using ABPGroup.Projects.Dto;

namespace ABPGroup.Projects;

public interface IProjectAppService : IAsyncCrudAppService<ProjectDto, long, PagedProjectResultRequestDto, CreateUpdateProjectDto, CreateUpdateProjectDto>
{
}
