using Abp.Application.Services;
using Abp.Application.Services.Dto;
using ABPGroup.Templates.Dto;
using System.Threading.Tasks;

namespace ABPGroup.Templates;

public interface ITemplateAppService : IApplicationService
{
    // Tenant-facing — read only
    Task<PagedResultDto<TemplateDto>> GetListAsync(TemplateListInput input);
    Task<TemplateDto>                 GetAsync(int id);

    // PlatformAdministrator only
    Task<TemplateDto> CreateAsync(CreateUpdateTemplateDto input);
    Task<TemplateDto> UpdateAsync(int id, CreateUpdateTemplateDto input);
    Task              DeleteAsync(int id);
    Task<TemplateDto> PublishAsync(int id);    // Draft → Active
    Task<TemplateDto> DeprecateAsync(int id);  // Active → Deprecated
    Task<TemplateDto> SetFeaturedAsync(int id, bool featured);
    Task              ToggleFavoriteAsync(int id);
}
