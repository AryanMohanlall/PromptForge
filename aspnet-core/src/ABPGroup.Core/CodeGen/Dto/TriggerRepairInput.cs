using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.CodeGen.Dto;

public class TriggerRepairInput
{
    [Required]
    public string SessionId { get; set; }

    public List<ValidationResultDto> Failures { get; set; }
}
