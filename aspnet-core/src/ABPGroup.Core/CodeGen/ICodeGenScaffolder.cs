using System.Collections.Generic;
using Abp.Dependency;
using ABPGroup.Projects;

namespace ABPGroup.CodeGen;

public interface ICodeGenScaffolder : ITransientDependency
{
    string FindTemplateDirectory(string templateSlug, string currentDir = null);
    List<GeneratedFile> ReadScaffoldFiles(string templateDir);
    void WriteFilesToDisk(List<GeneratedFile> files, string outputPath);
    void AddScaffoldFiles(List<GeneratedFile> files, Framework framework, string currentDir);
    void AddApprovedReadmeFile(List<GeneratedFile> files, string approvedReadme);
}
