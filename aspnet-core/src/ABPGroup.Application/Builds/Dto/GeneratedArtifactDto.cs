using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Builds;

namespace ABPGroup.Builds.Dto
{
    [AutoMapFrom(typeof(GeneratedArtifact))]
    public class GeneratedArtifactDto : EntityDto<long>
    {
        public long ProjectId { get; set; }
        public int PromptVersion { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public ArtifactType ArtifactType { get; set; }
        public string ContentHash { get; set; }
        public long? SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
