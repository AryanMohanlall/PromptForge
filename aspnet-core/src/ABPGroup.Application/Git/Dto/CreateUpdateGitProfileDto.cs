using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace ABPGroup.Git.Dto
{
    public class CreateUpdateGitProfileDto : EntityDto<long>
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
