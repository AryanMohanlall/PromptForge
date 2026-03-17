using AutoMapper;
using ABPGroup.Authorization.Users;
using ABPGroup.Persons.Dto;

namespace ABPGroup.Persons
{
    public class PersonMapProfile : Profile
    {
        public PersonMapProfile()
        {
            CreateMap<User, PersonDto>();
            CreateMap<CreateUpdatePersonDto, User>()
                .ForMember(x => x.Id, opt => opt.Ignore());
        }
    }
}
