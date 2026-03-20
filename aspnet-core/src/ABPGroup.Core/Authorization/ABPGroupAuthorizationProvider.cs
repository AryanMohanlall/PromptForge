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

        var templatesPermission = context.CreatePermission(PermissionNames.Pages_Templates, L("Templates"));
        templatesPermission.CreateChildPermission(PermissionNames.Pages_Templates_Create, L("CreateTemplate"));
        templatesPermission.CreateChildPermission(PermissionNames.Pages_Templates_Edit, L("EditTemplate"));
        templatesPermission.CreateChildPermission(PermissionNames.Pages_Templates_Delete, L("DeleteTemplate"));

        var gitProfilesPermission = context.CreatePermission(PermissionNames.Pages_GitProfiles, L("GitProfiles"));
        gitProfilesPermission.CreateChildPermission(PermissionNames.Pages_GitProfiles_Create, L("CreateGitProfile"));
        gitProfilesPermission.CreateChildPermission(PermissionNames.Pages_GitProfiles_Edit, L("EditGitProfile"));
        gitProfilesPermission.CreateChildPermission(PermissionNames.Pages_GitProfiles_Delete, L("DeleteGitProfile"));

        var projectRepositoriesPermission = context.CreatePermission(PermissionNames.Pages_ProjectRepositories, L("ProjectRepositories"));
        projectRepositoriesPermission.CreateChildPermission(PermissionNames.Pages_ProjectRepositories_Create, L("CreateProjectRepository"));
        projectRepositoriesPermission.CreateChildPermission(PermissionNames.Pages_ProjectRepositories_Edit, L("EditProjectRepository"));
        projectRepositoriesPermission.CreateChildPermission(PermissionNames.Pages_ProjectRepositories_Delete, L("DeleteProjectRepository"));

        context.CreatePermission(PermissionNames.Pages_RepositoryCommits, L("RepositoryCommits"));

        context.CreatePermission(PermissionNames.Pages_BuildJobs, L("BuildJobs"));

        context.CreatePermission(PermissionNames.Pages_GeneratedArtifacts, L("GeneratedArtifacts"));

        var deploymentsPermission = context.CreatePermission(PermissionNames.Pages_Deployments, L("Deployments"));
        deploymentsPermission.CreateChildPermission(PermissionNames.Pages_Deployments_Create, L("CreateDeployment"));
        deploymentsPermission.CreateChildPermission(PermissionNames.Pages_Deployments_Edit, L("EditDeployment"));
        deploymentsPermission.CreateChildPermission(PermissionNames.Pages_Deployments_Delete, L("DeleteDeployment"));

        context.CreatePermission(PermissionNames.Pages_DeploymentLogs, L("DeploymentLogs"));
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, ABPGroupConsts.LocalizationSourceName);
    }
}
