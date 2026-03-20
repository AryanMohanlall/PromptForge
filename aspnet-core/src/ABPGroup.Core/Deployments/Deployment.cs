using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using ABPGroup.Git;
using ABPGroup.Projects;

namespace ABPGroup.Deployments
{
    /// <summary>
    /// Represents a deployment of a project repository to a hosting target.
    /// </summary>
    public class Deployment : Entity<long>
    {
        public long ProjectId { get; set; }

        public Project Project { get; set; }

        public long ProjectRepositoryId { get; set; }

        public ProjectRepository ProjectRepository { get; set; }

        public DeploymentTarget Target { get; set; }

        [MaxLength(128)]
        public string EnvironmentName { get; set; }

        public DeploymentStatus Status { get; set; }

        [MaxLength(1024)]
        public string Url { get; set; }

        [MaxLength(256)]
        public string ProviderDeploymentId { get; set; }

        public DateTime? TriggeredAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [MaxLength(2000)]
        public string ErrorMessage { get; set; }

        public ICollection<DeploymentLog> Logs { get; set; } = new List<DeploymentLog>();
    }
}
