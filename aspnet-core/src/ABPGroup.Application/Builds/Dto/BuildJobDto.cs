using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Builds;

namespace ABPGroup.Builds.Dto
{
    [AutoMapFrom(typeof(BuildJob))]
    public class BuildJobDto : EntityDto<long>
    {
        public long ProjectId { get; set; }
        public int PromptVersion { get; set; }
        public BuildJobType Type { get; set; }
        public BuildJobStatus Status { get; set; }
        public string CurrentStep { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
        public string MetadataJson { get; set; }
        public long? DeploymentId { get; set; }
    }
}
