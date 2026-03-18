using Abp.Application.Services;
using ABPGroup.Persons.Dto;

namespace ABPGroup.Persons
{
    public interface IPersonAppService
        : IAsyncCrudAppService<PersonDto, long, PagedPersonResultRequestDto, CreateUpdatePersonDto, CreateUpdatePersonDto>
    {
    }
}
