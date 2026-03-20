using AutoMapper;
using ABPGroup.Deployments.Dto;

namespace ABPGroup.Deployments
{
    /// <summary>
    /// AutoMapper profile for deployment-related entities.
    /// </summary>
    public class DeploymentMapProfile : Profile
    {
        public DeploymentMapProfile()
        {
            CreateMap<Deployment, DeploymentDto>();
            CreateMap<CreateUpdateDeploymentDto, Deployment>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<DeploymentLog, DeploymentLogDto>();
        }
    }
}
