using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using ABPGroup.Projects;

namespace ABPGroup.Builds
{
    /// <summary>
    /// Represents a single file or asset produced during code generation.
    /// </summary>
    public class GeneratedArtifact : Entity<long>
    {
        public long ProjectId { get; set; }

        public Project Project { get; set; }

        /// <summary>
        /// The prompt version that produced this artifact.
        /// </summary>
        public int PromptVersion { get; set; }

        [Required]
        [MaxLength(1024)]
        public string Path { get; set; }

        [Required]
        [MaxLength(256)]
        public string FileName { get; set; }

        public ArtifactType ArtifactType { get; set; }

        [MaxLength(64)]
        public string ContentHash { get; set; }

        public long? SizeBytes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
