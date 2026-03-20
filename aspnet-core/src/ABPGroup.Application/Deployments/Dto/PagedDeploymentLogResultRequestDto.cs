using Abp.Application.Services.Dto;

namespace ABPGroup.Deployments.Dto
{
    public class PagedDeploymentLogResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
