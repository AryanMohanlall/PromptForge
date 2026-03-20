using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Git;

namespace ABPGroup.Git.Dto
{
    [AutoMapFrom(typeof(RepositoryCommit))]
    public class RepositoryCommitDto : EntityDto<long>
    {
        public long ProjectRepositoryId { get; set; }
        public string Sha { get; set; }
        public string Branch { get; set; }
        public string Message { get; set; }
        public int? PromptVersion { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
