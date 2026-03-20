using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using ABPGroup.Projects;

namespace ABPGroup.Templates;

public class Template : FullAuditedEntity<int>, IMayHaveTenant
{
    public int? TenantId { get; set; }
    public const int MaxNameLength         = 128;
    public const int MaxDescriptionLength  = 1000;
    public const int MaxTagLength          = 500;
    public const int MaxThumbnailUrlLength = 500;
    public const int MaxPreviewUrlLength   = 500;
    public const int MaxAuthorLength       = 128;
    public const int MaxVersionLength      = 20;
    public const int MaxScaffoldLength     = 8000;

    // Identity
    public string Name         { get; set; }
    public string Description  { get; set; }
    public string Author       { get; set; }

    // Classification
    public TemplateCategory    Category     { get; set; }
    public Framework           Framework    { get; set; }
    public ProgrammingLanguage Language     { get; set; }
    public DatabaseOption      Database     { get; set; }
    public bool                IncludesAuth { get; set; }

    // Discoverability — comma-separated, max 10 tags
    public string Tags { get; set; }

    // Presentation
    public string ThumbnailUrl { get; set; }
    public string PreviewUrl   { get; set; }

    // Lifecycle
    public TemplateStatus Status     { get; set; }
    public string         Version    { get; set; }
    public bool           IsFeatured { get; set; }
    public int            ForkCount  { get; set; }

    // Generation scaffold — injected into AI system prompt at generation time.
    // JSON blob containing: folder structure hints, default dependencies,
    // required pages, layout instructions, and prompt additions.
    // Never exposed to tenants.
    public string ScaffoldConfig { get; set; }
}
