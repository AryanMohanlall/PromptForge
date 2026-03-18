using ABPGroup.Validation;
using Xunit;

namespace ABPGroup.Tests.Services
{
    public class ValidationHelperTests
    {
        // ── valid emails ──────────────────────────────────────────────────

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("user.name@example.com")]
        [InlineData("user+tag@example.co.uk")]
        [InlineData("user-name@sub.domain.org")]
        [InlineData("u@a.io")]
        [InlineData("firstname.lastname@company.com")]
        [InlineData("user123@numbers456.net")]
        public void IsEmail_ValidEmail_ReturnsTrue(string email)
        {
            Assert.True(ValidationHelper.IsEmail(email));
        }

        // ── invalid emails ────────────────────────────────────────────────

        [Theory]
        [InlineData("plaintext")]
        [InlineData("@nodomain.com")]
        [InlineData("noatsign.com")]
        [InlineData("user@")]
        [InlineData("user@domain")]
        [InlineData("user @domain.com")]     // space inside
        [InlineData("user@ domain.com")]     // space after @
        [InlineData("user@@domain.com")]     // double @
        [InlineData("user..name@domain.com")] // double dot in local
        public void IsEmail_InvalidEmail_ReturnsFalse(string email)
        {
            Assert.False(ValidationHelper.IsEmail(email));
        }

        // ── null / empty ──────────────────────────────────────────────────

        [Fact]
        public void IsEmail_NullInput_ReturnsFalse()
        {
            Assert.False(ValidationHelper.IsEmail(null));
        }

        [Fact]
        public void IsEmail_EmptyString_ReturnsFalse()
        {
            Assert.False(ValidationHelper.IsEmail(string.Empty));
        }

        [Fact]
        public void IsEmail_WhitespaceOnly_ReturnsFalse()
        {
            Assert.False(ValidationHelper.IsEmail("   "));
        }

        // ── regex constant ─────────────────────────────────────────────────

        [Fact]
        public void EmailRegex_IsNotNullOrEmpty()
        {
            Assert.False(string.IsNullOrWhiteSpace(ValidationHelper.EmailRegex));
        }
    }
}
