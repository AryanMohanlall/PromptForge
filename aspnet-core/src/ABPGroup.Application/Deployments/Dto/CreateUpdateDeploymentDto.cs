using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace ABPGroup.Deployments.Dto
{
    public class CreateUpdateDeploymentDto : EntityDto<long>
    {
        public long ProjectId { get; set; }

        public long ProjectRepositoryId { get; set; }

        public DeploymentTarget Target { get; set; }

        [StringLength(128)]
        public string EnvironmentName { get; set; }

        public DeploymentStatus Status { get; set; }

        [StringLength(1024)]
        public string Url { get; set; }

        [StringLength(256)]
        public string ProviderDeploymentId { get; set; }
    }
}
