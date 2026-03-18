using Abp.Application.Services.Dto;

namespace ABPGroup.Persons.Dto
{
    public class PagedPersonResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}
