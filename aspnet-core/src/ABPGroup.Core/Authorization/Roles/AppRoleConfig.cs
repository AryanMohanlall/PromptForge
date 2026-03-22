using Abp.MultiTenancy;
using Abp.Zero.Configuration;

namespace ABPGroup.Authorization.Roles;

public static class AppRoleConfig
{
    // ABPGroup.Core/Authorization/Roles/AppRoleConfig.cs
    public static void Configure(IRoleManagementConfig roleManagementConfig)
    {
        roleManagementConfig.StaticRoles.Add(new StaticRoleDefinition(
            StaticRoleNames.Tenants.Admin,
            MultiTenancySides.Tenant
           )
        );

        roleManagementConfig.StaticRoles.Add(new StaticRoleDefinition(
            StaticRoleNames.Tenants.ProductBuilder,
            MultiTenancySides.Tenant) // set true if new users should get this role automatically
        );

        roleManagementConfig.StaticRoles.Add(new StaticRoleDefinition(
            StaticRoleNames.Tenants.Developer,
            MultiTenancySides.Tenant)
        );
    }
}
