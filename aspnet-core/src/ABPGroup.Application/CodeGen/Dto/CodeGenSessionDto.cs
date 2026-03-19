using System;
using System.Collections.Generic;

namespace ABPGroup.CodeGen.Dto;

public class CodeGenSessionDto
{
    public string Id { get; set; }
    public long UserId { get; set; }
    public long? ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string Prompt { get; set; }
    public string NormalizedRequirement { get; set; }
    public List<string> DetectedFeatures { get; set; } = new();
    public List<string> DetectedEntities { get; set; } = new();
    public StackConfigDto ConfirmedStack { get; set; }
    public AppSpecDto Spec { get; set; }
    public DateTime? SpecConfirmedAt { get; set; }
    public DateTime? GenerationStartedAt { get; set; }
    public DateTime? GenerationCompletedAt { get; set; }
    public int Status { get; set; }
    public List<ValidationResultDto> ValidationResults { get; set; } = new();
    public string ScaffoldTemplate { get; set; }
    public List<GeneratedFileDto> GeneratedFiles { get; set; } = new();
    public int RepairAttempts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
