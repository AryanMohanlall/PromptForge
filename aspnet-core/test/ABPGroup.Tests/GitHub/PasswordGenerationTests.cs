using ABPGroup.Authorization.Users;
using System.Linq;
using Xunit;

namespace ABPGroup.Tests.GitHub
{
    public class PasswordGenerationTests
    {
        [Fact]
        public void CreateRandomPassword_HasMinimumLength()
        {
            var password = User.CreateRandomPassword();

            // ASP.NET Identity default RequiredLength is 6; we verify at least 8 for safety
            Assert.True(password.Length >= 8,
                $"Expected length >= 8 but was {password.Length}");
        }

        [Fact]
        public void CreateRandomPassword_ExactLengthIs16()
        {
            // Pattern: 6 uppercase hex + 8 hex chars + "!1" = 16
            var password = User.CreateRandomPassword();

            Assert.Equal(16, password.Length);
        }

        [Fact]
        public void CreateRandomPassword_HasNonAlphanumericCharacter()
        {
            var password = User.CreateRandomPassword();

            Assert.True(password.Any(c => !char.IsLetterOrDigit(c)),
                "Password must contain at least one non-alphanumeric character (!)");
        }

        [Fact]
        public void CreateRandomPassword_HasDigit()
        {
            var password = User.CreateRandomPassword();

            Assert.True(password.Any(char.IsDigit),
                "Password must contain at least one digit");
        }

        [Fact]
        public void CreateRandomPassword_EndsWithExclamationAndOne()
        {
            // The suffix "!1" guarantees both non-alphanumeric and digit requirements
            var password = User.CreateRandomPassword();

            Assert.EndsWith("!1", password);
        }

        [Fact]
        public void CreateRandomPassword_FirstSixCharsAreUppercase()
        {
            // First 6 chars are GUID hex (0-9, a-f) uppercased → (0-9, A-F)
            var password = User.CreateRandomPassword();
            var firstSix = password.Substring(0, 6);

            foreach (var c in firstSix)
            {
                Assert.True(char.IsUpper(c) || char.IsDigit(c),
                    $"Character '{c}' in first-6 should be uppercase letter or digit");
            }
        }

        [Fact]
        public void CreateRandomPassword_ProducesDifferentValuesEachCall()
        {
            var passwords = Enumerable.Range(0, 20)
                .Select(_ => User.CreateRandomPassword())
                .ToList();

            // All 20 should be unique (Guid-based, astronomically unlikely to collide)
            var distinctCount = passwords.Distinct().Count();
            Assert.Equal(20, distinctCount);
        }

        [Fact]
        public void CreateRandomPassword_MeetsIdentityRequirements()
        {
            // Run many times to reduce probability of edge-case GUID (all-digit hex)
            for (var i = 0; i < 100; i++)
            {
                var password = User.CreateRandomPassword();

                Assert.True(password.Length >= 6, "RequiredLength");
                Assert.True(password.Any(c => !char.IsLetterOrDigit(c)), "RequireNonAlphanumeric");
                Assert.True(password.Any(char.IsDigit), "RequireDigit");
            }
        }
    }
}
