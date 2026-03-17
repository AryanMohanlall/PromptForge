using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.Projects;
using System;

namespace ABPGroup.Prompts.Dto;

[AutoMapFrom(typeof(Prompt))]
public class PromptDto : EntityDto<long>
{
    public long ProjectId { get; set; }

    public string Content { get; set; }

    public int Version { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
