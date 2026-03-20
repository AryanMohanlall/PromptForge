using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Git;

namespace ABPGroup.Git.Dto
{
    [AutoMapFrom(typeof(ProjectRepository))]
    public class ProjectRepositoryDto : EntityDto<long>
    {
        public long ProjectId { get; set; }
        public GitProvider Provider { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string DefaultBranch { get; set; }
        public RepositoryVisibility Visibility { get; set; }
        public string HtmlUrl { get; set; }
        public string ExternalRepositoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
