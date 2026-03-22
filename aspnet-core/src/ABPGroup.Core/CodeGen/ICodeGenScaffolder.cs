using System.Collections.Generic;
using Abp.Dependency;
using ABPGroup.Projects;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen;

public interface ICodeGenScaffolder : ITransientDependency
{
    string FindTemplateDirectory(string templateSlug, string currentDir = null);
    List<GeneratedFile> ReadScaffoldFiles(string templateDir);
    void WriteFilesToDisk(List<GeneratedFile> files, string outputPath);
    void AddScaffoldFiles(List<GeneratedFile> files, Framework framework, string currentDir);
    void AddApprovedReadmeFile(List<GeneratedFile> files, string approvedReadme);
}

public class GenerationBlueprintDto
{
    public AppSpecDto Spec { get; set; }
    public string ReadmeMarkdown { get; set; }
}
