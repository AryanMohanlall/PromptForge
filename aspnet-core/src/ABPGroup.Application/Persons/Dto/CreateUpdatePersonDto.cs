using Abp.Application.Services.Dto;
using ABPGroup.Persons;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Persons.Dto
{
    public class CreateUpdatePersonDto : EntityDto<long>
    {
        [Required]
        [MaxLength(256)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(256)]
        public string EmailAddress { get; set; }

        [MaxLength(256)]
        public string DisplayName { get; set; }

        public PersonRole Role { get; set; } = PersonRole.Admin;

        [MaxLength(256)]
        public string GitHubUsername { get; set; }

        [MaxLength(1024)]
        public string GitHubAccessToken { get; set; }

        [MaxLength(1024)]
        public string AvatarUrl { get; set; }
    }
}
