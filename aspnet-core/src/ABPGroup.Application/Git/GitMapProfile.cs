using AutoMapper;
using ABPGroup.Git.Dto;

namespace ABPGroup.Git
{
    /// <summary>
    /// AutoMapper profile for all Git-related entities.
    /// </summary>
    public class GitMapProfile : Profile
    {
        public GitMapProfile()
        {
            CreateMap<GitProfile, GitProfileDto>();
            CreateMap<CreateUpdateGitProfileDto, GitProfile>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<ProjectRepository, ProjectRepositoryDto>();
            CreateMap<CreateUpdateProjectRepositoryDto, ProjectRepository>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<RepositoryCommit, RepositoryCommitDto>();
        }
    }
}
