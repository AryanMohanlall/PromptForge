using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen;

public interface ICodeGenEngine : ITransientDependency
{
    Task<CodeGenResult> GenerateProjectAsync(
        CodeGenEngineInput input,
        Func<string, Task> onProgress,
        string currentDir = null,
        AppSpecDto approvedPlan = null,
        string approvedReadme = null);

    Task<string> GenerateLayerAsync(
        string progressLabel,
        string layerDescription,
        string userInstruction,
        CodeGenEngineInput input,
        System.Text.StringBuilder context,
        AppSpecDto approvedPlan,
        string scaffoldBaseline,
        string approvedReadme,
        Func<string, Task> onProgress);

    string BuildScaffoldBaseline(List<GeneratedFile> files);
    
    System.Text.StringBuilder BuildGenerationContext(
        string projectName,
        CodeGenEngineInput input,
        RequirementsSnapshot requirements,
        string approvedReadme);

    void MergeLayerResponse(CodeGenResult result, string layerResponse, bool allowArchitectureOverride);
}
