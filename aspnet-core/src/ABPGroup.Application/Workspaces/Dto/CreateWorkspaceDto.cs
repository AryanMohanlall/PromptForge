using Abp.AutoMapper;
using Abp.MultiTenancy;
using ABPGroup.MultiTenancy;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Workspaces.Dto;

[AutoMapTo(typeof(Tenant))]
public class CreateWorkspaceDto
{
    [StringLength(AbpTenantBase.MaxTenancyNameLength)]
    [RegularExpression(AbpTenantBase.TenancyNameRegex)]
    public string TenancyName { get; set; }

    [Required]
    [StringLength(AbpTenantBase.MaxNameLength)]
    public string Name { get; set; }

    public bool IsActive { get; set; } = true;
}
