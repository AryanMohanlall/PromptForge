using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Projects.Dto;

[AutoMapTo(typeof(Project))]
public class CreateUpdateProjectDto : EntityDto<long>
{
    public int? WorkspaceId { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    public long? PromptId { get; set; }

    [Required]
    public string Prompt { get; set; }

    public int PromptVersion { get; set; } = 1;

    public DateTime? PromptSubmittedAt { get; set; }

    public Framework Framework { get; set; }

    public ProgrammingLanguage Language { get; set; }

    public DatabaseOption DatabaseOption { get; set; }

    public bool IncludeAuth { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
}
