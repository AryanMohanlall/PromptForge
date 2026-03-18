using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Projects;
using System;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Prompts.Dto;

[AutoMapTo(typeof(Prompt))]
public class CreateUpdatePromptDto : EntityDto<long>
{
    [Required]
    public long ProjectId { get; set; }

    [Required]
    public string Content { get; set; }

    public int Version { get; set; } = 1;

    public DateTime? SubmittedAt { get; set; }
}
