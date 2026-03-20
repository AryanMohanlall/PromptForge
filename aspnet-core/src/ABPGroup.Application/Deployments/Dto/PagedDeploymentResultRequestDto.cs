using Abp.Application.Services.Dto;

namespace ABPGroup.Deployments.Dto
{
    public class PagedDeploymentResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
