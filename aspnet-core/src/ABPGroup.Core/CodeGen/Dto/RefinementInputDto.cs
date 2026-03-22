using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.CodeGen.Dto;

public class RefinementInputDto
{
    [Required]
    public string SessionId { get; set; }

    [Required]
    public string ChangeRequest { get; set; }

    public List<string> AffectedFiles { get; set; } = new();
}
