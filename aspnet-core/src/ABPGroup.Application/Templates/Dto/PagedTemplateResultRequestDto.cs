using Abp.Application.Services.Dto;
using ABPGroup.Projects;

namespace ABPGroup.Templates.Dto;

public class TemplateListInput : PagedAndSortedResultRequestDto
{
    public TemplateCategory?    Category     { get; set; }
    public Framework?           Framework    { get; set; }
    public DatabaseOption?      Database     { get; set; }
    public bool?                IncludesAuth { get; set; }
    public TemplateStatus?      Status       { get; set; }
    public string               SearchTerm   { get; set; }
    public bool?                IsFeatured   { get; set; }
    public bool?                IsMyTemplates { get; set; }
}
