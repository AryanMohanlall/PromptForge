using Abp.Application.Services.Dto;

namespace ABPGroup.Git.Dto
{
    public class PagedProjectRepositoryResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
