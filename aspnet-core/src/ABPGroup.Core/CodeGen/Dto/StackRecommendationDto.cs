using System.Collections.Generic;

namespace ABPGroup.CodeGen.Dto;

public class StackRecommendationDto
{
    public string Framework { get; set; }
    public string Language { get; set; }
    public string Styling { get; set; }
    public string Database { get; set; }
    public string Orm { get; set; }
    public string Auth { get; set; }
    public Dictionary<string, string> Reasoning { get; set; } = new();
}
