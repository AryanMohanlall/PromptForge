using Abp.Application.Services.Dto;

namespace ABPGroup.Builds.Dto
{
    public class PagedBuildJobResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
