using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using ABPGroup.Deployments;
using ABPGroup.Projects;

namespace ABPGroup.Git
{
    /// <summary>
    /// Represents the Git repository backing a project.
    /// One-to-one with Project.
    /// </summary>
    public class ProjectRepository : Entity<long>
    {
        public long ProjectId { get; set; }

        public Project Project { get; set; }

        public GitProvider Provider { get; set; }

        [Required]
        [MaxLength(256)]
        public string Owner { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [Required]
        [MaxLength(512)]
        public string FullName { get; set; }

        [MaxLength(128)]
        public string DefaultBranch { get; set; }

        public RepositoryVisibility Visibility { get; set; }

        [MaxLength(1024)]
        public string HtmlUrl { get; set; }

        [MaxLength(256)]
        public string ExternalRepositoryId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<RepositoryCommit> Commits { get; set; } = new List<RepositoryCommit>();

        public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();
    }
}
