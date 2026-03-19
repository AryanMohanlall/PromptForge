using Abp.Authorization.Users;
using Abp.Domain.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using Abp.UI;
using ABPGroup.Authorization.Roles;
using ABPGroup.MultiTenancy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABPGroup.Authorization.Users;

public class UserRegistrationManager : DomainService
{
    public IAbpSession AbpSession { get; set; }

    private readonly TenantManager _tenantManager;
    private readonly UserManager _userManager;
    private readonly RoleManager _roleManager;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserRegistrationManager(
        TenantManager tenantManager,
        UserManager userManager,
        RoleManager roleManager,
        IPasswordHasher<User> passwordHasher)
    {
        _tenantManager = tenantManager;
        _userManager = userManager;
        _roleManager = roleManager;
        _passwordHasher = passwordHasher;

        AbpSession = NullAbpSession.Instance;
    }

    public async Task<User> RegisterAsync(string name, string surname, string emailAddress, string userName, string plainPassword, bool isEmailConfirmed)
    {
        CheckForTenant();

        return await RegisterAsync(name, surname, emailAddress, userName, plainPassword, AbpSession.TenantId!.Value, isEmailConfirmed);
    }

    public async Task<User> RegisterAsync(string name, string surname, string emailAddress, string userName, string plainPassword, int tenantId, bool isEmailConfirmed)
    {
        var tenant = await GetActiveTenantAsync(tenantId);

        using (CurrentUnitOfWork.SetTenantId(tenant.Id))
        {
            var user = new User
            {
                TenantId = tenant.Id,
                Name = name,
                Surname = surname,
                EmailAddress = emailAddress,
                IsActive = true,
                UserName = userName,
                IsEmailConfirmed = isEmailConfirmed,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            foreach (var defaultRole in await _roleManager.Roles.Where(r => r.IsDefault).ToListAsync())
            {
                user.Roles.Add(new UserRole(tenant.Id, user.Id, defaultRole.Id));
            }

            await _userManager.InitializeOptionsAsync(tenant.Id);

            var identityResult = await _userManager.CreateAsync(user, plainPassword);
            Logger.Info($"UserManager.CreateAsync returned: Succeeded={identityResult.Succeeded}, UserId={user.Id}, Username={user.UserName}, Email={user.EmailAddress}, Tenant={tenant.Id}");
            if (!identityResult.Succeeded)
            {
                var errorMessages = string.Join("; ", identityResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                Logger.Error($"UserManager.CreateAsync failed for user registration: {user.UserName}, {user.EmailAddress}, tenant: {tenant.Id}. Errors: {errorMessages}");
            }
            CheckErrors(identityResult);
            Logger.Info($"About to call SaveChangesAsync for user: {user.UserName}, UserId={user.Id}, Tenant={tenant.Id}");
            try
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                Logger.Info($"SaveChangesAsync succeeded for user: {user.UserName}, UserId={user.Id}, Tenant={tenant.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Logger.Error($"SaveChangesAsync failed for user registration: {user.UserName}, {user.EmailAddress}, tenant: {tenant.Id}", ex);
                throw new UserFriendlyException("User registration failed due to a database error.", ex.Message);
            }

            Logger.Info($"Returning user from RegisterAsync: UserId={user.Id}, Username={user.UserName}, Tenant={tenant.Id}");

            return user;
        }
    }

    private void CheckForTenant()
    {
        if (!AbpSession.TenantId.HasValue)
        {
            throw new InvalidOperationException("Can not register host users!");
        }
    }

    private async Task<Tenant> GetActiveTenantAsync()
    {
        if (!AbpSession.TenantId.HasValue)
        {
            return null;
        }

        return await GetActiveTenantAsync(AbpSession.TenantId.Value);
    }

    private async Task<Tenant> GetActiveTenantAsync(int tenantId)
    {
        var tenant = await _tenantManager.FindByIdAsync(tenantId);
        if (tenant == null)
        {
            throw new UserFriendlyException(L("UnknownTenantId{0}", tenantId));
        }

        if (!tenant.IsActive)
        {
            throw new UserFriendlyException(L("TenantIdIsNotActive{0}", tenantId));
        }

        return tenant;
    }

    protected virtual void CheckErrors(IdentityResult identityResult)
    {
        identityResult.CheckErrors(LocalizationManager);
    }
}
