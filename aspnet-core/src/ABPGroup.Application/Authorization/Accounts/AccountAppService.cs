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
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccountAppService(
        UserRegistrationManager userRegistrationManager,
        TenantManager tenantManager,
        EditionManager editionManager,
        RoleManager roleManager,
        UserManager userManager,
        IAbpZeroDbMigrator abpZeroDbMigrator,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRegistrationManager = userRegistrationManager;
        _tenantManager = tenantManager;
        _editionManager = editionManager;
        _roleManager = roleManager;
        _userManager = userManager;
        _abpZeroDbMigrator = abpZeroDbMigrator;
        _httpContextAccessor = httpContextAccessor;
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
        bool createdNewTenant = false;
        int tenantId;
        if (input.CreateTenant)
        {
            tenantId = await CreateTenantForRegistrationAsync(input);
            createdNewTenant = true;
        }
        else if (input.TenantId.HasValue && input.TenantId.Value > 0)
        {
            tenantId = input.TenantId.Value;
        }
        else if (TryGetTenantIdFromHeader(out var headerTenantId))
        {
            tenantId = headerTenantId;
        }
        else
        {
            // Auto-generate a tenant from the user's email address
            tenantId = await CreateAutoTenantFromEmailAsync(input.EmailAddress);
            createdNewTenant = true;
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

        if (createdNewTenant)
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

    private bool TryGetTenantIdFromHeader(out int tenantId)
    {
        tenantId = 0;
        var headerValue = _httpContextAccessor.HttpContext?.Request.Headers["Abp.TenantId"].FirstOrDefault();
        return !string.IsNullOrWhiteSpace(headerValue) && int.TryParse(headerValue, out tenantId) && tenantId > 0;
    }

    private async Task<int> CreateAutoTenantFromEmailAsync(string email)
    {
        // Derive a tenancy name from the local part of the email (e.g. "john.doe@example.com" -> "johndoe")
        var localPart = email.Contains('@') ? email.Split('@')[0] : email;
        // Strip characters not allowed by TenancyNameRegex (only letters, digits and hyphens are allowed)
        var baseName = System.Text.RegularExpressions.Regex.Replace(localPart, @"[^a-zA-Z0-9\-]", "");
        if (string.IsNullOrEmpty(baseName))
        {
            baseName = "tenant";
        }

        // Ensure uniqueness by appending a numeric suffix when necessary
        var tenancyName = baseName;
        var counter = 1;
        while (await _tenantManager.FindByTenancyNameAsync(tenancyName) != null)
        {
            tenancyName = $"{baseName}{counter++}";
        }

        var displayName = tenancyName;

        var tenant = new Tenant(tenancyName, displayName)
        {
            IsActive = true
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
            throw new UserFriendlyException("Tenant database migration failed.", ex.Message);
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
