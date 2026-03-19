using System.Collections.Generic;

namespace ABPGroup.CodeGen.Dto;

public class GenerationStatusDto
{
    public string CurrentPhase { get; set; }
    public List<string> CompletedSteps { get; set; } = new();
    public List<ValidationResultDto> ValidationResults { get; set; } = new();
    public bool IsComplete { get; set; }
    public string Error { get; set; }
}
