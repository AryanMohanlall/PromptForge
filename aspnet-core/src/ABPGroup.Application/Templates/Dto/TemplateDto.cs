using Abp.Application.Services.Dto;
using ABPGroup.Projects;
using System;

namespace ABPGroup.Templates.Dto;

public class TemplateDto : EntityDto<int>
{
    public string              Name         { get; set; }
    public string              Description  { get; set; }
    public string              Author       { get; set; }
    public TemplateCategory    Category     { get; set; }
    public string              CategoryName { get; set; }
    public Framework           Framework    { get; set; }
    public ProgrammingLanguage Language     { get; set; }
    public DatabaseOption      Database     { get; set; }
    public bool                IncludesAuth { get; set; }
    public string[]            Tags         { get; set; }
    public string              ThumbnailUrl { get; set; }
    public string              PreviewUrl   { get; set; }
    public TemplateStatus      Status       { get; set; }
    public string              Version      { get; set; }
    public bool                IsFeatured   { get; set; }
    public int                 ForkCount    { get; set; }
    public bool                IsFavorite   { get; set; }
    public DateTime            CreatedAt    { get; set; }
}
