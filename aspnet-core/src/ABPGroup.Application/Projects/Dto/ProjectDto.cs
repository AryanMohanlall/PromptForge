using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;

namespace ABPGroup.Projects.Dto;

[AutoMapFrom(typeof(Project))]
public class ProjectDto : EntityDto<long>
{
    public int WorkspaceId { get; set; }

    public long? PromptId { get; set; }

    public string Name { get; set; }

    public string Prompt { get; set; }

    public int PromptVersion { get; set; }

    public DateTime? PromptSubmittedAt { get; set; }

    public Framework Framework { get; set; }

    public ProgrammingLanguage Language { get; set; }

    public DatabaseOption DatabaseOption { get; set; }

    public bool IncludeAuth { get; set; }

    public ProjectStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
