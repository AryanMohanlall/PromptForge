using System.ComponentModel.DataAnnotations;
using ABPGroup.Persons;

namespace ABPGroup.Invitations.Dto;

public class SendInvitationInput
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public PersonRole Role { get; set; }
}
