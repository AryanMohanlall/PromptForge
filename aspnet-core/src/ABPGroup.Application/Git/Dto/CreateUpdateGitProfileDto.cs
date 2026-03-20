using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using ABPGroup.Git;

namespace ABPGroup.Git.Dto
{
    [AutoMapTo(typeof(GitProfile))]
    public class CreateUpdateGitProfileDto
    {
        public GitProvider Provider { get; set; }

        [Required]
        [StringLength(256)]
        public string ProviderUserId { get; set; }

        [Required]
        [StringLength(256)]
        public string Username { get; set; }

        [StringLength(1024)]
        public string AvatarUrl { get; set; }

        [StringLength(2048)]
        public string AccessToken { get; set; }

        public bool IsConnected { get; set; }
    }
}
