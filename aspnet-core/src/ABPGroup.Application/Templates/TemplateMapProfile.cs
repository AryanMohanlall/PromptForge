using AutoMapper;
using ABPGroup.Templates.Dto;
using System;

namespace ABPGroup.Templates;

public class TemplateMapProfile : Profile
{
    public TemplateMapProfile()
    {
        CreateMap<Template, TemplateDto>()
            .ForMember(dest => dest.Tags,
                opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.Tags)
                        ? new string[0]
                        : src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)))
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(src => src.CreationTime));

        CreateMap<CreateUpdateTemplateDto, Template>()
            .ForMember(dest => dest.Tags,
                opt => opt.MapFrom(src =>
                    src.Tags == null ? null : src.Tags.Trim()));
    }
}
