using ABPGroup.Authorization.Accounts.Dto;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace ABPGroup.Tests.Services
{
    /// <summary>
    /// Tests for RegisterInput.Validate() — the custom IValidatableObject business rules.
    /// </summary>
    public class RegisterInputValidationTests
    {
        private static RegisterInput ValidBase() => new RegisterInput
        {
            Name = "Test",
            Surname = "User",
            UserName = "testuser",
            EmailAddress = "testuser@example.com",
            Password = "Test@1234"
        };

        private static List<ValidationResult> Validate(RegisterInput input)
        {
            var ctx = new ValidationContext(input);
            var results = new List<ValidationResult>();
            input.Validate(ctx);   // only collects custom results, not DataAnnotations
            // Use the IValidatableObject interface directly:
            foreach (var r in ((System.ComponentModel.DataAnnotations.IValidatableObject)input).Validate(ctx))
                results.Add(r);
            return results;
        }

        // ── username as email ─────────────────────────────────────────────

        [Fact]
        public void Validate_UserNameIsEmail_DifferentFromEmailAddress_ReturnsError()
        {
            var input = ValidBase();
            input.UserName = "other@example.com";  // email-format but != EmailAddress

            var errors = Validate(input);

            Assert.Contains(errors, e =>
                e.ErrorMessage.Contains("Username cannot be an email address"));
        }

        [Fact]
        public void Validate_UserNameIsEmail_SameAsEmailAddress_NoError()
        {
            var input = ValidBase();
            input.UserName = "testuser@example.com";   // same as EmailAddress
            input.EmailAddress = "testuser@example.com";

            var errors = Validate(input);

            Assert.DoesNotContain(errors, e =>
                e.ErrorMessage.Contains("Username cannot be an email address"));
        }

        [Fact]
        public void Validate_UserNameIsNotEmail_NoUsernameError()
        {
            var input = ValidBase();
            input.UserName = "just_a_username";

            var errors = Validate(input);

            Assert.DoesNotContain(errors, e =>
                e.ErrorMessage.Contains("Username cannot be an email address"));
        }

        [Fact]
        public void Validate_EmptyUserName_NoUsernameError()
        {
            var input = ValidBase();
            input.UserName = "";

            var errors = Validate(input);

            Assert.DoesNotContain(errors, e =>
                e.ErrorMessage.Contains("Username cannot be an email address"));
        }

        // ── CreateTenant validation ───────────────────────────────────────

        [Fact]
        public void Validate_CreateTenantTrue_MissingTenancyName_ReturnsError()
        {
            var input = ValidBase();
            input.CreateTenant = true;
            input.TenantTenancyName = null;
            input.TenantName = "My Tenant";

            var errors = Validate(input);

            Assert.Contains(errors, e =>
                e.ErrorMessage.Contains("TenantTenancyName is required"));
        }

        [Fact]
        public void Validate_CreateTenantTrue_MissingTenantName_ReturnsError()
        {
            var input = ValidBase();
            input.CreateTenant = true;
            input.TenantTenancyName = "MyTenant";
            input.TenantName = "  ";   // whitespace-only

            var errors = Validate(input);

            Assert.Contains(errors, e =>
                e.ErrorMessage.Contains("TenantName is required"));
        }

        [Fact]
        public void Validate_CreateTenantTrue_BothNamesProvided_NoTenantErrors()
        {
            var input = ValidBase();
            input.CreateTenant = true;
            input.TenantTenancyName = "MyTenant";
            input.TenantName = "My Tenant Display";

            var errors = Validate(input);

            Assert.DoesNotContain(errors, e =>
                e.ErrorMessage.Contains("TenantTenancyName is required") ||
                e.ErrorMessage.Contains("TenantName is required"));
        }

        [Fact]
        public void Validate_CreateTenantFalse_NamesOmitted_NoTenantErrors()
        {
            var input = ValidBase();
            input.CreateTenant = false;

            var errors = Validate(input);

            Assert.DoesNotContain(errors, e =>
                e.ErrorMessage.Contains("TenantTenancyName is required") ||
                e.ErrorMessage.Contains("TenantName is required"));
        }

        // ── combined rules ────────────────────────────────────────────────

        [Fact]
        public void Validate_CreateTenantTrue_AllProblems_ReturnsBothErrors()
        {
            var input = ValidBase();
            input.CreateTenant = true;
            input.TenantTenancyName = null;
            input.TenantName = null;

            var errors = Validate(input);

            Assert.Contains(errors, e => e.ErrorMessage.Contains("TenantTenancyName is required"));
            Assert.Contains(errors, e => e.ErrorMessage.Contains("TenantName is required"));
        }

        [Fact]
        public void Validate_HappyPath_NoErrors()
        {
            var input = ValidBase();  // no CreateTenant, non-email username

            var errors = Validate(input);

            Assert.Empty(errors);
        }
    }
}
