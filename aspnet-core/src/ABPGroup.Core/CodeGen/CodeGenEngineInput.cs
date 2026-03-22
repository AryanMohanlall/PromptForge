using ABPGroup.Projects;

namespace ABPGroup.CodeGen;

public class CodeGenEngineInput
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Prompt { get; set; }
    public Framework Framework { get; set; }
    public ProgrammingLanguage Language { get; set; }
    public DatabaseOption DatabaseOption { get; set; }
    public bool IncludeAuth { get; set; }
}
