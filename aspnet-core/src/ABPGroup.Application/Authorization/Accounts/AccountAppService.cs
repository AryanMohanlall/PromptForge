using Abp.Configuration;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.Zero.Configuration;
using ABPGroup.Authorization.Roles;
using ABPGroup.Authorization.Accounts.Dto;
using ABPGroup.Authorization.Users;
using ABPGroup.Editions;
using ABPGroup.MultiTenancy;
using Abp.UI;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace ABPGroup.Authorization.Accounts;

public class AccountAppService : ABPGroupAppServiceBase, IAccountAppService
{
    // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
    public const string PasswordRegex = "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

    private readonly UserRegistrationManager _userRegistrationManager;
    private readonly TenantManager _tenantManager;
    private readonly EditionManager _editionManager;
    private readonly RoleManager _roleManager;
    private readonly UserManager _userManager;
    private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;

    public AccountAppService(
        UserRegistrationManager userRegistrationManager,
        TenantManager tenantManager,
        EditionManager editionManager,
        RoleManager roleManager,
        UserManager userManager,
        IAbpZeroDbMigrator abpZeroDbMigrator)
    {
        _userRegistrationManager = userRegistrationManager;
        _tenantManager = tenantManager;
        _editionManager = editionManager;
        _roleManager = roleManager;
        _userManager = userManager;
        _abpZeroDbMigrator = abpZeroDbMigrator;
    }

    public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
    {
        var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
        if (tenant == null)
        {
            return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
        }

        if (!tenant.IsActive)
        {
            return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
        }

        return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id);
    }

    public async Task<RegisterOutput> Register(RegisterInput input)
    {
        int tenantId;
        if (input.CreateTenant)
        {
            tenantId = await CreateTenantForRegistrationAsync(input);
        }
        else
        {
            tenantId = input.TenantId!.Value;
        }

        var user = await _userRegistrationManager.RegisterAsync(
            input.Name,
            input.Surname,
            input.EmailAddress,
            input.UserName,
            input.Password,
            tenantId,
            true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
        );

        if (input.CreateTenant)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                CheckErrors(await _userManager.AddToRoleAsync(user, adminRole.Name));
                await CurrentUnitOfWork.SaveChangesAsync();
            }
        }

        var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

        return new RegisterOutput
        {
            CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
        };
    }

    private async Task<int> CreateTenantForRegistrationAsync(RegisterInput input)
    {
        var existingTenant = await _tenantManager.FindByTenancyNameAsync(input.TenantTenancyName);
        if (existingTenant != null)
        {
            throw new UserFriendlyException("TenantTenancyName is already in use.");
        }

        var tenantConnectionString = NormalizeAndValidateTenantConnectionString(input.TenantConnectionString);

        var tenant = new Tenant(input.TenantTenancyName, input.TenantName)
        {
            IsActive = true,
            ConnectionString = tenantConnectionString.IsNullOrEmpty()
                ? null
                : SimpleStringCipher.Instance.Encrypt(tenantConnectionString)
        };

        var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName);
        if (defaultEdition != null)
        {
            tenant.EditionId = defaultEdition.Id;
        }

        await _tenantManager.CreateAsync(tenant);
        await CurrentUnitOfWork.SaveChangesAsync();

        try
        {
            _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException("Tenant database migration failed. Check TenantConnectionString and database accessibility.", ex.Message);
        }

        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));
            await CurrentUnitOfWork.SaveChangesAsync();

            var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
            await _roleManager.GrantAllPermissionsAsync(adminRole);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        return tenant.Id;
    }

    private static string NormalizeAndValidateTenantConnectionString(string tenantConnectionString)
    {
        if (tenantConnectionString.IsNullOrWhiteSpace())
        {
            return null;
        }

        var normalized = tenantConnectionString.Trim();

        // Swagger sample payload uses "string" placeholder. Treat it as not provided.
        if (normalized.Equals("string", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            _ = new DbConnectionStringBuilder { ConnectionString = normalized };
        }
        catch (ArgumentException)
        {
            throw new UserFriendlyException("TenantConnectionString is invalid. Provide a valid PostgreSQL connection string or leave it empty.");
        }

        return normalized;
    }

}
