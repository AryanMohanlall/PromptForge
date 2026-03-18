using Abp.Application.Services;
using ABPGroup.Templates.Dto;

namespace ABPGroup.Templates;

public interface ITemplateAppService
    : IAsyncCrudAppService<TemplateDto, int, PagedTemplateResultRequestDto, CreateUpdateTemplateDto, CreateUpdateTemplateDto>
{
}
