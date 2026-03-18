using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace ABPGroup.Authorization;

public class ABPGroupAuthorizationProvider : AuthorizationProvider
{
    public override void SetPermissions(IPermissionDefinitionContext context)
    {
        context.CreatePermission(PermissionNames.Pages_Users, L("Users"));
        context.CreatePermission(PermissionNames.Pages_Users_Activation, L("UsersActivation"));
        context.CreatePermission(PermissionNames.Pages_Roles, L("Roles"));
        context.CreatePermission(PermissionNames.Pages_Tenants, L("Tenants"), multiTenancySides: MultiTenancySides.Host);

        var personsPermission = context.CreatePermission(PermissionNames.Pages_Persons, L("Persons"));
        personsPermission.CreateChildPermission(PermissionNames.Pages_Persons_Create, L("CreatePerson"));
        personsPermission.CreateChildPermission(PermissionNames.Pages_Persons_Edit, L("EditPerson"));
        personsPermission.CreateChildPermission(PermissionNames.Pages_Persons_Delete, L("DeletePerson"));

        var workspacesPermission = context.CreatePermission(PermissionNames.Pages_Workspaces, L("Workspaces"));
        workspacesPermission.CreateChildPermission(PermissionNames.Pages_Workspaces_Create, L("CreateWorkspace"));
        workspacesPermission.CreateChildPermission(PermissionNames.Pages_Workspaces_Edit, L("EditWorkspace"));
        workspacesPermission.CreateChildPermission(PermissionNames.Pages_Workspaces_Delete, L("DeleteWorkspace"));

        var projectsPermission = context.CreatePermission(PermissionNames.Pages_Projects, L("Projects"));
        projectsPermission.CreateChildPermission(PermissionNames.Pages_Projects_Create, L("CreateProject"));
        projectsPermission.CreateChildPermission(PermissionNames.Pages_Projects_Edit, L("EditProject"));
        projectsPermission.CreateChildPermission(PermissionNames.Pages_Projects_Delete, L("DeleteProject"));
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, ABPGroupConsts.LocalizationSourceName);
    }
}
