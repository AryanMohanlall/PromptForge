using Abp.Application.Services.Dto;
using ABPGroup.Projects;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Templates.Dto;

public class CreateUpdateTemplateDto : EntityDto<int>
{
    [Required]
    [MaxLength(128)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    [MaxLength(128)]
    public string Author { get; set; }

    [Required]
    public TemplateCategory Category { get; set; }

    [Required]
    public Framework Framework { get; set; }

    [Required]
    public ProgrammingLanguage Language { get; set; }

    [Required]
    public DatabaseOption Database { get; set; }

    public bool IncludesAuth { get; set; }

    // Comma-separated, max 10 tags
    public string Tags { get; set; }

    [MaxLength(500)]
    public string ThumbnailUrl { get; set; }

    [MaxLength(500)]
    public string PreviewUrl { get; set; }

    [Required]
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;

    public string Version { get; set; } = "1.0.0";

    public bool IsFeatured { get; set; }

    // JSON scaffold config — injected into AI system prompt at generation time
    [MaxLength(8000)]
    public string ScaffoldConfig { get; set; }
}
