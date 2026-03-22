using System.ComponentModel.DataAnnotations;

namespace ABPGroup.CodeGen.Dto;

public class SaveStackInput
{
    [Required]
    public string SessionId { get; set; }

    [Required]
    public StackConfigDto Stack { get; set; }
}
