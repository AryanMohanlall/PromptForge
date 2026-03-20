using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace ABPGroup.Git
{
    /// <summary>
    /// Represents a single commit pushed to a project repository,
    /// optionally linked to a specific prompt version for traceability.
    /// </summary>
    public class RepositoryCommit : Entity<long>
    {
        public long ProjectRepositoryId { get; set; }

        public ProjectRepository ProjectRepository { get; set; }

        [Required]
        [MaxLength(40)]
        public string Sha { get; set; }

        [MaxLength(256)]
        public string Branch { get; set; }

        [MaxLength(1000)]
        public string Message { get; set; }

        /// <summary>
        /// The prompt version that triggered this commit, if applicable.
        /// </summary>
        public int? PromptVersion { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
