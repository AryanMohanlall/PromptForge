using System;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using ABPGroup.Authorization.Users;

namespace ABPGroup.Git
{
    /// <summary>
    /// Represents a user's connection to an external Git hosting provider.
    /// A user may have multiple profiles (e.g. GitHub + GitLab).
    /// </summary>
    public class GitProfile : Entity<long>
    {
        public long UserId { get; set; }

        public User User { get; set; }

        public GitProvider Provider { get; set; }

        [Required]
        [MaxLength(256)]
        public string ProviderUserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Username { get; set; }

        [MaxLength(1024)]
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Encrypted or token-vault reference; never returned in DTOs.
        /// </summary>
        [MaxLength(2048)]
        public string AccessToken { get; set; }

        public bool IsConnected { get; set; }

        public DateTime? LastSyncedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
