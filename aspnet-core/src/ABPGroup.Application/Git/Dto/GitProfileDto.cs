using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Git;

namespace ABPGroup.Git.Dto
{
    [AutoMapFrom(typeof(GitProfile))]
    public class GitProfileDto : EntityDto<long>
    {
        public long UserId { get; set; }
        public GitProvider Provider { get; set; }
        public string ProviderUserId { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsConnected { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
