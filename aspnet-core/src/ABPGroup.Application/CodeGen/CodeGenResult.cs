using System.Collections.Generic;

namespace ABPGroup.CodeGen;

public class CodeGenResult
{
    public long GeneratedProjectId { get; set; }
    public string OutputPath { get; set; }
    public List<GeneratedFile> Files { get; set; } = new();
    public string ArchitectureSummary { get; set; }
    public List<string> ModuleList { get; set; } = new();
}

public class GeneratedFile
{
    public string Path { get; set; }
    public string Content { get; set; }
}
