using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using ABPGroup.CodeGen.Dto;
namespace ABPGroup.CodeGen;

public interface ICodeGenPlanner : ITransientDependency
{
    Task<RequirementsSnapshot> LoadRequirementsSnapshotAsync(
        CodeGenEngineInput input,
        AppSpecDto approvedPlan,
        string approvedReadme,
        Func<string, Task> onProgress);

    Task<RequirementsSnapshot> AnalyzeRequirementsAsync(
        CodeGenEngineInput input,
        Func<string, Task> onProgress);

    Task<ReadmeResultDto> GenerateReadmeAsync(string sessionId);
    
    Task<AppSpecDto> GeneratePlanFromReadmeAsync(
        string readmeMarkdown,
        StackConfigDto stack,
        CodeGenSession session,
        List<string> features,
        List<string> entities);

    RequirementsSnapshot BuildRequirementsSnapshotFromPlan(
        AppSpecDto approvedPlan,
        string approvedReadme);
}
