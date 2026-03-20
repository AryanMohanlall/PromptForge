using Abp.Domain.Entities;
using ABPGroup.Builds;
using ABPGroup.Deployments;
using ABPGroup.Git;
using ABPGroup.MultiTenancy;
using System;
using System.Collections.Generic;

namespace ABPGroup.Projects;

/// <summary>
/// Main aggregate root for a generated application.
/// </summary>
public class Project : Entity<long>
{
    public int WorkspaceId { get; set; }

    public Tenant Workspace { get; set; }

    public string Name { get; set; }

    public long? PromptId { get; set; }

    public Prompt PromptEntity { get; set; }

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

    public int? TemplateId { get; set; }

    public string ArchitectureSummary { get; set; }

    public string GeneratedModules { get; set; }

    public string StatusMessage { get; set; }

    public ICollection<Prompt> Prompts { get; set; }

    /// <summary>
    /// The Git repository backing this project (1:1).
    /// </summary>
    public ProjectRepository Repository { get; set; }

    public ICollection<BuildJob> BuildJobs { get; set; } = new List<BuildJob>();

    public ICollection<GeneratedArtifact> GeneratedArtifacts { get; set; } = new List<GeneratedArtifact>();

    public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();
}
