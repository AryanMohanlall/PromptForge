using System;
using Abp.Application.Services.Dto;

namespace ABPGroup.Git.Dto
{
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
