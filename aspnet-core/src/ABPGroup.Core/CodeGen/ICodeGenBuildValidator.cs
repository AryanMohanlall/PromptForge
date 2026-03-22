using System.Collections.Generic;
using System.Threading.Tasks;
using ABPGroup.Projects;

namespace ABPGroup.CodeGen;

public interface ICodeGenBuildValidator
{
    Task<BuildValidationResult> ValidateBuildAsync(string outputPath, Framework framework);
}

public class BuildValidationResult
{
    public bool Success { get; set; }
    public string Logs { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
