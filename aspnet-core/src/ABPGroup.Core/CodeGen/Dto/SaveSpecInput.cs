using System.ComponentModel.DataAnnotations;

namespace ABPGroup.CodeGen.Dto;

public class SaveSpecInput
{
    [Required]
    public string SessionId { get; set; }

    [Required]
    public AppSpecDto Spec { get; set; }
}
