using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Dependency;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen;

public interface ICodeGenRefiner : ITransientDependency
{
    Task<CodeGenSessionDto> RepairAsync(TriggerRepairInput input);
    Task<RefinementResultDto> RefineSessionAsync(RefinementInputDto input);
}
