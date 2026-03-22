using System.Collections.Generic;

namespace ABPGroup.CodeGen.Dto;

public class RefinementResultDto
{
    public List<GeneratedFileDto> ChangedFiles { get; set; } = new();
    public List<string> DeletedFiles { get; set; } = new();
    public string Summary { get; set; }
    public List<ValidationResultDto> ValidationResults { get; set; } = new();
}
