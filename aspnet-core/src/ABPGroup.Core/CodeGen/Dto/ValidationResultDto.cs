namespace ABPGroup.CodeGen.Dto;

public class ValidationResultDto
{
    public string Id { get; set; }
    public string Status { get; set; } // pending | running | passed | failed
    public string Message { get; set; }
}
