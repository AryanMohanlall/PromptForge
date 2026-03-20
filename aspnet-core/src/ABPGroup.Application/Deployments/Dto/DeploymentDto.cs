using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Deployments;

namespace ABPGroup.Deployments.Dto
{
    [AutoMapFrom(typeof(Deployment))]
    public class DeploymentDto : EntityDto<long>
    {
        public long ProjectId { get; set; }
        public long ProjectRepositoryId { get; set; }
        public DeploymentTarget Target { get; set; }
        public string EnvironmentName { get; set; }
        public DeploymentStatus Status { get; set; }
        public string Url { get; set; }
        public string ProviderDeploymentId { get; set; }
        public DateTime? TriggeredAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
    }
}
