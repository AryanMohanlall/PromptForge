using Abp.Domain.Entities.Auditing;
using System;

namespace ABPGroup.Templates;

public class Template : FullAuditedEntity<int>
{
    public string Name { get; set; }

    public string Slug { get; set; }

    public string Description { get; set; }

    public string PreviewImageUrl { get; set; }

    public string Category { get; set; }

    public string Tags { get; set; }

    public string Author { get; set; }

    public string SourceUrl { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public int? LikeCount { get; set; }

    public int? ViewCount { get; set; }
}
