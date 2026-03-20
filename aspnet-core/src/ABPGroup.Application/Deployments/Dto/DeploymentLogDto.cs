using System;
using Abp.Application.Services.Dto;

namespace ABPGroup.Deployments.Dto
{
    public class DeploymentLogDto : EntityDto<long>
    {
        public long DeploymentId { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public string MetadataJson { get; set; }
    }
}
