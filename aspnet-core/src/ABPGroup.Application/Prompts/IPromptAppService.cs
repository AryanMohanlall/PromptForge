using Abp.Application.Services;
using ABPGroup.Prompts.Dto;

namespace ABPGroup.Prompts;

public interface IPromptAppService : IAsyncCrudAppService<PromptDto, long, PagedPromptResultRequestDto, CreateUpdatePromptDto, CreateUpdatePromptDto>
{
}
