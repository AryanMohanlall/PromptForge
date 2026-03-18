using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Templates.Dto;

public class CreateUpdateTemplateDto : EntityDto<int>
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [Required]
    [StringLength(200)]
    public string Slug { get; set; }

    [StringLength(2000)]
    public string Description { get; set; }

    [StringLength(500)]
    public string PreviewImageUrl { get; set; }

    [Required]
    [StringLength(100)]
    public string Category { get; set; }

    [StringLength(500)]
    public string Tags { get; set; }

    [StringLength(120)]
    public string Author { get; set; }

    [StringLength(500)]
    public string SourceUrl { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    [Range(0, int.MaxValue)]
    public int? LikeCount { get; set; }

    [Range(0, int.MaxValue)]
    public int? ViewCount { get; set; }
}
