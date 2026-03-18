using ABPGroup.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace ABPGroup.Tests.Services
{
    public class ExternalLoginInfoHelperTests
    {
        // ── GivenName + Surname claims ────────────────────────────────────

        [Fact]
        public void GetNameAndSurname_BothGivenNameAndSurname_ReturnsBoth()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, "Alice"),
                new Claim(ClaimTypes.Surname, "Smith")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal("Alice", name);
            Assert.Equal("Smith", surname);
        }

        [Fact]
        public void GetNameAndSurname_OnlyGivenName_NameSetSurnameNull()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, "Bob")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal("Bob", name);
            Assert.Null(surname);
        }

        [Fact]
        public void GetNameAndSurname_OnlySurname_SurnameSetNameNull()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Surname, "Jones")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Null(name);
            Assert.Equal("Jones", surname);
        }

        // ── Name claim fallback ───────────────────────────────────────────

        [Fact]
        public void GetNameAndSurname_NameClaimWithSpace_SplitsOnLastSpace()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "John Doe")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal("John", name);
            // surname includes the leading space per the LastIndexOf split
            Assert.Equal(" Doe", surname);
        }

        [Fact]
        public void GetNameAndSurname_NameClaimMultipleWords_SplitsOnLastSpace()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Mary Jane Watson")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal("Mary Jane", name);
            Assert.Equal(" Watson", surname);
        }

        [Fact]
        public void GetNameAndSurname_NameClaimSingleWord_UsesSameValueForBoth()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Madonna")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal("Madonna", name);
            Assert.Equal("Madonna", surname);
        }

        [Fact]
        public void GetNameAndSurname_NameClaimSpaceAtStart_TreatedAsSingleToken()
        {
            // lastSpaceIndex == 0 which is < 1, so treated as single-word
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, " Leading")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal(" Leading", name);
            Assert.Equal(" Leading", surname);
        }

        [Fact]
        public void GetNameAndSurname_NameClaimSpaceAtEnd_TreatedAsSingleToken()
        {
            // lastSpaceIndex == Length-1, so > (Length - 2), treated as single-word
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Trailing ")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal("Trailing ", name);
            Assert.Equal("Trailing ", surname);
        }

        // ── GivenName/Surname take priority over Name ─────────────────────

        [Fact]
        public void GetNameAndSurname_GivenNameAndNameClaim_GivenNameWins()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, "Explicit"),
                new Claim(ClaimTypes.Surname, "Surname"),
                new Claim(ClaimTypes.Name, "Should Be Ignored")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            Assert.Equal("Explicit", name);
            Assert.Equal("Surname", surname);
        }

        // ── empty claims list ─────────────────────────────────────────────

        [Fact]
        public void GetNameAndSurname_EmptyClaimsList_ReturnsNullNull()
        {
            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(new List<Claim>());

            Assert.Null(name);
            Assert.Null(surname);
        }

        [Fact]
        public void GetNameAndSurname_EmptyGivenNameValue_FallsBackToNameClaim()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.GivenName, ""),    // empty → treated as missing
                new Claim(ClaimTypes.Name, "Fallback Name")
            };

            var (name, surname) = ExternalLoginInfoHelper.GetNameAndSurnameFromClaims(claims);

            // GivenName is empty so it's skipped; Name fallback is used
            Assert.Equal("Fallback", name);
            Assert.Equal(" Name", surname);
        }
    }
}
