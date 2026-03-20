using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace ABPGroup.Git.Dto
{
    public class CreateUpdateProjectRepositoryDto : EntityDto<long>
    {
        public long ProjectId { get; set; }

        public GitProvider Provider { get; set; }

        [Required]
        [StringLength(256)]
        public string Owner { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [Required]
        [StringLength(512)]
        public string FullName { get; set; }

        [StringLength(128)]
        public string DefaultBranch { get; set; }

        public RepositoryVisibility Visibility { get; set; }

        [StringLength(1024)]
        public string HtmlUrl { get; set; }

        [StringLength(256)]
        public string ExternalRepositoryId { get; set; }
    }
}
