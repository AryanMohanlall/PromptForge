using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using ABPGroup.MultiTenancy;
using System;

namespace ABPGroup.Workspaces.Dto;

[AutoMapFrom(typeof(Tenant))]
public class WorkspaceDto : EntityDto<int>
{
    public string TenancyName { get; set; }

    public string Name { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }
}
