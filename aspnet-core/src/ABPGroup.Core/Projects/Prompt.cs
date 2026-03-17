using Abp.Domain.Entities;
using System;

namespace ABPGroup.Projects;

public class Prompt : Entity<long>
{
    public long ProjectId { get; set; }

    public Project Project { get; set; }

    public string Content { get; set; }

    public int Version { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
