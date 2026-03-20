using Abp.Application.Services.Dto;

namespace ABPGroup.Git.Dto
{
    public class PagedRepositoryCommitResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
