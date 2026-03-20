using Abp.Application.Services.Dto;

namespace ABPGroup.Git.Dto
{
    public class PagedGitProfileResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
