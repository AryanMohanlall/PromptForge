using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace ABPGroup.Deployments
{
    /// <summary>
    /// A single log entry associated with a deployment.
    /// </summary>
    public class DeploymentLog : Entity<long>
    {
        public long DeploymentId { get; set; }

        public Deployment Deployment { get; set; }

        public LogLevel Level { get; set; }

        [Required]
        [MaxLength(4000)]
        public string Message { get; set; }

        public DateTime Timestamp { get; set; }

        [MaxLength(256)]
        public string Source { get; set; }

        /// <summary>
        /// Free-form JSON blob for structured log data.
        /// </summary>
        public string MetadataJson { get; set; }
    }
}
