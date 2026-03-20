using Abp.Application.Services;
using ABPGroup.Git.Dto;

namespace ABPGroup.Git
{
    public interface IGitProfileAppService
        : IAsyncCrudAppService<GitProfileDto, long, PagedGitProfileResultRequestDto, CreateUpdateGitProfileDto, CreateUpdateGitProfileDto>
    {
    }
}
