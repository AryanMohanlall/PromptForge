namespace ABPGroup.CodeGen.Dto;

public class GenerationStatusDto
{
    public int Status { get; set; }
    public bool IsRunning { get; set; }
    public string ErrorMessage { get; set; }
    public string CurrentPhase { get; set; }
    public string[] CompletedSteps { get; set; }
    public ValidationResultDto[] ValidationResults { get; set; }
    public bool IsComplete { get; set; }
}
