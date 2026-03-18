using Abp.Application.Services.Dto;

namespace ABPGroup.Templates.Dto;

public class PagedTemplateResultRequestDto : PagedResultRequestDto
{
    public string Keyword { get; set; }
}
