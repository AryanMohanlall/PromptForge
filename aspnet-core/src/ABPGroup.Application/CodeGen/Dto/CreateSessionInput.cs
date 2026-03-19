using System.ComponentModel.DataAnnotations;

namespace ABPGroup.CodeGen.Dto;

public class CreateSessionInput
{
    [Required]
    public string Prompt { get; set; }
}
