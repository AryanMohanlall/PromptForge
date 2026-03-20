using AutoMapper;
using ABPGroup.Builds.Dto;

namespace ABPGroup.Builds
{
    /// <summary>
    /// AutoMapper profile for build-related entities.
    /// </summary>
    public class BuildMapProfile : Profile
    {
        public BuildMapProfile()
        {
            CreateMap<BuildJob, BuildJobDto>();
            CreateMap<GeneratedArtifact, GeneratedArtifactDto>();
        }
    }
}
