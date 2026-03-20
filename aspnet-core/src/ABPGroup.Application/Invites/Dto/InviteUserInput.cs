using System.ComponentModel.DataAnnotations;
using ABPGroup.Persons;

namespace ABPGroup.Invites.Dto
{
    public class InviteUserInput
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public PersonRole Role { get; set; }
    }
}