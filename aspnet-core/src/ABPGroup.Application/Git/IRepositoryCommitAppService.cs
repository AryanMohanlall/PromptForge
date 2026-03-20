using Abp.Application.Services;
using ABPGroup.Git.Dto;

namespace ABPGroup.Git
{
    /// <summary>
    /// Read-only service for repository commit history.
    /// </summary>
    public interface IRepositoryCommitAppService
        : IAsyncCrudAppService<RepositoryCommitDto, long, PagedRepositoryCommitResultRequestDto>
    {
    }
}
