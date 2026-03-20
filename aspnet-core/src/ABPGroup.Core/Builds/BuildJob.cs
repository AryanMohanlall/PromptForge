using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using ABPGroup.Projects;

namespace ABPGroup.Builds
{
    /// <summary>
    /// Represents a discrete build or code-generation job for a project.
    /// Each run is traceable to a prompt version.
    /// </summary>
    public class BuildJob : Entity<long>
    {
        public long ProjectId { get; set; }

        public Project Project { get; set; }

        /// <summary>
        /// The prompt version that triggered this build, for traceability.
        /// </summary>
        public int PromptVersion { get; set; }

        public BuildJobType Type { get; set; }

        public BuildJobStatus Status { get; set; }

        [MaxLength(256)]
        public string CurrentStep { get; set; }

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(2000)]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Free-form JSON blob for provider-specific metadata.
        /// </summary>
        public string MetadataJson { get; set; }

        /// <summary>
        /// Optional link to the deployment triggered by this build.
        /// </summary>
        public long? DeploymentId { get; set; }
    }
}
