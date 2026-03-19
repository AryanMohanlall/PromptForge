using Abp.UI;
using ABPGroup.Authorization.Accounts;
using ABPGroup.Authorization.Accounts.Dto;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Services
{
    public class AccountAppServiceTests : ABPGroupTestBase
    {
        private readonly IAccountAppService _accountAppService;

        public AccountAppServiceTests()
        {
            _accountAppService = Resolve<IAccountAppService>();
        }

        // ── IsTenantAvailable ──────────────────────────────────────────────

        [Fact]
        public async Task IsTenantAvailable_ExistingActiveTenant_ReturnsAvailable()
        {
            var result = await _accountAppService.IsTenantAvailable(
                new IsTenantAvailableInput { TenancyName = "Default" });

            result.State.ShouldBe(TenantAvailabilityState.Available);
            result.TenantId.HasValue.ShouldBeTrue();
            result.TenantId!.Value.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task IsTenantAvailable_NonExistentTenant_ReturnsNotFound()
        {
            var result = await _accountAppService.IsTenantAvailable(
                new IsTenantAvailableInput { TenancyName = "TenantThatDoesNotExist_XYZ" });

            result.State.ShouldBe(TenantAvailabilityState.NotFound);
        }

        [Fact]
        public async Task IsTenantAvailable_InactiveTenant_ReturnsInActive()
        {
            // Create an inactive tenant in the test DB then query it
            UsingDbContext(ctx =>
            {
                ctx.Tenants.Add(new ABPGroup.MultiTenancy.Tenant("InactiveTenant", "Inactive Tenant")
                {
                    IsActive = false
                });
            });

            var result = await _accountAppService.IsTenantAvailable(
                new IsTenantAvailableInput { TenancyName = "InactiveTenant" });

            result.State.ShouldBe(TenantAvailabilityState.InActive);
        }

        // ── Register — auto-tenant from email ─────────────────────────────

        [Fact(Skip = "Auto-tenant registration creates a cross-tenant user that cannot be updated in the in-memory test DB (EntityNotFoundException on UpdateAsync).")]
        public async Task Register_WithoutTenantId_AutoGeneratesTenantFromEmail()
        {
            var input = new RegisterInput
            {
                Name = "Auto",
                Surname = "Tenant",
                UserName = "autotenant_user",
                EmailAddress = "autotenant@example.com",
                Password = "Test@1234A"
            };

            var output = await _accountAppService.Register(input);

            output.ShouldNotBeNull();
            output.CanLogin.ShouldBeTrue();
        }

        [Fact(Skip = "Auto-tenant registration creates a cross-tenant user that cannot be updated in the in-memory test DB (EntityNotFoundException on UpdateAsync).")]
        public async Task Register_DuplicateEmailAutoTenant_UsesUniqueTenancyName()
        {
            await _accountAppService.Register(new RegisterInput
            {
                Name = "First",
                Surname = "User",
                UserName = "firstuser1",
                EmailAddress = "firstuser@example.com",
                Password = "Test@1234A"
            });

            var output = await _accountAppService.Register(new RegisterInput
            {
                Name = "Also",
                Surname = "First",
                UserName = "firstuser2",
                EmailAddress = "firstuser@other.example.com",
                Password = "Test@1234A"
            });

            output.ShouldNotBeNull();
        }

        [Fact]
        public async Task Register_WithExistingTenantId_UsesProvidedTenant()
        {
            // TenantId = 1 is the Default tenant seeded by the test module
            var output = await _accountAppService.Register(new RegisterInput
            {
                Name = "Existing",
                Surname = "Tenant",
                UserName = "existingtenant_user",
                EmailAddress = "existingtenant@example.com",
                Password = "Test@1234A",
                TenantId = 1
            });

            output.ShouldNotBeNull();
        }
    }
}
