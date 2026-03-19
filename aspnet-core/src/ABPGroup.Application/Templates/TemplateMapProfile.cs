using AutoMapper;
using ABPGroup.Templates.Dto;

namespace ABPGroup.Templates;

public class TemplateMapProfile : Profile
{
    public TemplateMapProfile()
    {
        CreateMap<Template, TemplateDto>();
        CreateMap<CreateUpdateTemplateDto, Template>()
            .ForMember(x => x.Id, opt => opt.Ignore());
    }
}
