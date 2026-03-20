using Abp.Application.Services.Dto;

namespace ABPGroup.Builds.Dto
{
    public class PagedGeneratedArtifactResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
