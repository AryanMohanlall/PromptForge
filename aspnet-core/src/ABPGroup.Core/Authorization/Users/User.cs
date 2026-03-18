using Abp.Authorization.Users;
using Abp.Extensions;
using ABPGroup.Persons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Authorization.Users;

public class User : AbpUser<User>
{
    [MaxLength(256)]
    public string DisplayName { get; set; }

    public PersonRole Role { get; set; } = PersonRole.Admin;

    [MaxLength(256)]
    public string GitHubUsername { get; set; }

    [MaxLength(1024)]
    public string GitHubAccessToken { get; set; }

    [MaxLength(1024)]
    public string AvatarUrl { get; set; }

    public const string DefaultPassword = "123qwe";

    public static string CreateRandomPassword()
    {
        var guid = Guid.NewGuid().ToString("N");
        return guid.Substring(0, 6).ToUpper() + guid.Substring(6, 8) + "!1";
    }

    public static User CreateTenantAdminUser(int tenantId, string emailAddress)
    {
        var user = new User
        {
            TenantId = tenantId,
            UserName = AdminUserName,
            Name = AdminUserName,
            Surname = AdminUserName,
            EmailAddress = emailAddress,
            Roles = new List<UserRole>()
        };

        user.SetNormalizedNames();

        return user;
    }
}
