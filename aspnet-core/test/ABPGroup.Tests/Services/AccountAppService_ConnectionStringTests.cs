using Abp.UI;
using ABPGroup.Authorization.Accounts;
using System.Reflection;
using Xunit;

namespace ABPGroup.Tests.Services
{
    /// <summary>
    /// Tests for the private static AccountAppService.NormalizeAndValidateTenantConnectionString().
    /// Accessed via reflection since it contains standalone business logic that is worth
    /// verifying independently of the full registration flow.
    /// </summary>
    public class AccountAppService_ConnectionStringTests
    {
        private static readonly MethodInfo _method =
            typeof(AccountAppService).GetMethod(
                "NormalizeAndValidateTenantConnectionString",
                BindingFlags.NonPublic | BindingFlags.Static)!;

        private static string Invoke(string input)
        {
            try
            {
                return (string)_method.Invoke(null, new object[] { input });
            }
            catch (TargetInvocationException tie)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo
                    .Capture(tie.InnerException!).Throw();
                throw; // unreachable
            }
        }

        // ── null / empty → null ───────────────────────────────────────────

        [Fact]
        public void NormalizeAndValidate_NullInput_ReturnsNull()
        {
            Assert.Null(Invoke(null));
        }

        [Fact]
        public void NormalizeAndValidate_EmptyString_ReturnsNull()
        {
            Assert.Null(Invoke(string.Empty));
        }

        [Fact]
        public void NormalizeAndValidate_WhitespaceOnly_ReturnsNull()
        {
            Assert.Null(Invoke("   "));
        }

        // ── Swagger placeholder "string" ──────────────────────────────────

        [Fact]
        public void NormalizeAndValidate_LiteralString_ReturnsNull()
        {
            Assert.Null(Invoke("string"));
        }

        [Fact]
        public void NormalizeAndValidate_LiteralString_CaseInsensitive_ReturnsNull()
        {
            Assert.Null(Invoke("STRING"));
        }

        [Fact]
        public void NormalizeAndValidate_LiteralString_MixedCase_ReturnsNull()
        {
            Assert.Null(Invoke("String"));
        }

        // ── valid connection string ───────────────────────────────────────

        [Fact]
        public void NormalizeAndValidate_ValidConnectionString_ReturnsNormalized()
        {
            var cs = "Host=localhost;Database=mydb;Username=admin;Password=secret";

            var result = Invoke(cs);

            Assert.Equal(cs, result);
        }

        [Fact]
        public void NormalizeAndValidate_ValidConnectionStringWithLeadingSpaces_ReturnsTrimmed()
        {
            var cs = "   Host=localhost;Database=mydb   ";

            var result = Invoke(cs);

            Assert.Equal(cs.Trim(), result);
        }

        // ── invalid connection string ─────────────────────────────────────

        [Fact]
        public void NormalizeAndValidate_InvalidConnectionString_ThrowsUserFriendlyException()
        {
            // A string that is not empty/whitespace/"string" but cannot be parsed
            // as a DbConnectionStringBuilder connection string — e.g. an unbalanced quote.
            var ex = Assert.Throws<UserFriendlyException>(() => Invoke("key='unclosed"));

            Assert.Contains("invalid", ex.Message.ToLower());
        }

        // ── password regex constant is well-formed ────────────────────────

        [Fact]
        public void PasswordRegex_IsNotNullOrEmpty()
        {
            Assert.False(string.IsNullOrWhiteSpace(AccountAppService.PasswordRegex));
        }

        [Theory]
        [InlineData("Test1234")]            // missing special char — but pattern is permissive
        [InlineData("Abcdefg1")]
        public void PasswordRegex_AcceptsValidPasswords(string password)
        {
            var match = System.Text.RegularExpressions.Regex.IsMatch(password, AccountAppService.PasswordRegex);
            Assert.True(match);
        }

        [Theory]
        [InlineData("short")]              // < 8 chars
        [InlineData("alllowercase1")]      // no uppercase
        [InlineData("ALLUPPERCASE1")]      // no lowercase
        [InlineData("NoDigitsHere!!")]     // no digit
        public void PasswordRegex_RejectsInvalidPasswords(string password)
        {
            var match = System.Text.RegularExpressions.Regex.IsMatch(password, AccountAppService.PasswordRegex);
            Assert.False(match);
        }
    }
}
