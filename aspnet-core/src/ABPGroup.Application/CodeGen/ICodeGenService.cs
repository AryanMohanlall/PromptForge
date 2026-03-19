using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using ABPGroup.CodeGen.Dto;
using ABPGroup.Projects.Dto;

namespace ABPGroup.CodeGen;

public interface ICodeGenAppService : IApplicationService
{
    // Legacy single-shot generation (backward compat with ProjectAppService)
    Task<CodeGenResult> GenerateProjectAsync(CreateUpdateProjectDto input);
    Task<CodeGenResult> GenerateProjectAsync(CreateUpdateProjectDto input, Func<string, Task> onProgress);

    // Multi-step workflow
    Task<CodeGenSessionDto> CreateSession(CreateSessionInput input);
    Task<StackRecommendationDto> RecommendStack(string sessionId);
    Task<CodeGenSessionDto> SaveStack(SaveStackInput input);
    Task<CodeGenSessionDto> GenerateSpec(string sessionId);
    Task<CodeGenSessionDto> SaveSpec(SaveSpecInput input);
    Task<CodeGenSessionDto> ConfirmSpec(string sessionId);
    Task<CodeGenSessionDto> Generate(string sessionId);
    Task<GenerationStatusDto> GetStatus(string sessionId);
    Task<CodeGenSessionDto> Repair(TriggerRepairInput input);
}
