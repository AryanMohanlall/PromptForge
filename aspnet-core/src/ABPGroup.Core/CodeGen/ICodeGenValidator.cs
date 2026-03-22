using System.Collections.Generic;
using Abp.Dependency;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen;

public interface ICodeGenValidator : ITransientDependency
{
    List<ValidationResultDto> EvaluateValidationResults(
        List<ValidationRuleDto> validations,
        List<GeneratedFile> generatedFiles,
        StackConfigDto stack);

    List<ValidationResultDto> BuildInitialValidationResults(
        List<ValidationRuleDto> validations,
        StackConfigDto stack);

    List<ValidationResultDto> BuildShellValidationResults(
        StackConfigDto stack,
        List<GeneratedFile> files,
        HashSet<string> filePaths);

    List<ValidationResultDto> BuildShellValidationPlaceholders(StackConfigDto stack);
}
