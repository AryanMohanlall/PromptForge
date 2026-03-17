using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.MultiTenancy;
using ABPGroup.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABPGroup.Authorization.Accounts.Dto;

public class RegisterInput : IValidatableObject
{
    [Required]
    [StringLength(AbpUserBase.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxSurnameLength)]
    public string Surname { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxUserNameLength)]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(AbpUserBase.MaxEmailAddressLength)]
    public string EmailAddress { get; set; }

    [Required]
    [StringLength(AbpUserBase.MaxPlainPasswordLength)]
    [DisableAuditing]
    public string Password { get; set; }

    [DisableAuditing]
    public string CaptchaResponse { get; set; }

    public int? TenantId { get; set; }

    public bool CreateTenant { get; set; }

    [StringLength(AbpTenantBase.MaxTenancyNameLength)]
    [RegularExpression(AbpTenantBase.TenancyNameRegex)]
    public string TenantTenancyName { get; set; }

    [StringLength(AbpTenantBase.MaxNameLength)]
    public string TenantName { get; set; }

    [StringLength(AbpTenantBase.MaxConnectionStringLength)]
    public string TenantConnectionString { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!UserName.IsNullOrEmpty())
        {
            if (!UserName.Equals(EmailAddress) && ValidationHelper.IsEmail(UserName))
            {
                yield return new ValidationResult("Username cannot be an email address unless it's the same as your email address!");
            }
        }

        if (CreateTenant)
        {
            if (TenantTenancyName.IsNullOrWhiteSpace())
            {
                yield return new ValidationResult("TenantTenancyName is required when CreateTenant is true.");
            }

            if (TenantName.IsNullOrWhiteSpace())
            {
                yield return new ValidationResult("TenantName is required when CreateTenant is true.");
            }
        }
        // If neither CreateTenant nor TenantId is provided, a tenant will be auto-generated from the email address.
    }
}
