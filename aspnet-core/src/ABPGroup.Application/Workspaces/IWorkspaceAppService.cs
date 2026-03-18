using Abp.Application.Services;
using ABPGroup.Workspaces.Dto;

namespace ABPGroup.Workspaces;

public interface IWorkspaceAppService : IAsyncCrudAppService<WorkspaceDto, int, PagedWorkspaceResultRequestDto, CreateWorkspaceDto, WorkspaceDto>
{
}
