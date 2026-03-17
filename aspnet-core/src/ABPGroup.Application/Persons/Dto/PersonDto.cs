using Abp.Application.Services.Dto;
using ABPGroup.Persons;

namespace ABPGroup.Persons.Dto
{
    public class PersonDto : EntityDto<long>
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public PersonRole Role { get; set; }
        public string GitHubUsername { get; set; }
        public string GitHubAccessToken { get; set; }
        public string AvatarUrl { get; set; }
    }
}
