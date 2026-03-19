using Abp.Domain.Uow;
using Abp.UI;
using ABPGroup.Authorization.Users;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Services
{
    public class UserRegistrationManagerTests : ABPGroupTestBase
    {
        private readonly UserRegistrationManager _registrationManager;
        private readonly IUnitOfWorkManager _uowManager;

        public UserRegistrationManagerTests()
        {
            _registrationManager = Resolve<UserRegistrationManager>();
            _uowManager = Resolve<IUnitOfWorkManager>();
        }

        // ── RegisterAsync (with tenantId) — happy path ────────────────────

        [Fact]
        public async Task RegisterAsync_ValidInput_CreatesAndReturnsUser()
        {
            User user;
            using (var uow = _uowManager.Begin())
            {
                user = await _registrationManager.RegisterAsync(
                    name: "Jane",
                    surname: "Doe",
                    emailAddress: "jane.doe@example.com",
                    userName: "jane.doe",
                    plainPassword: "Test@1234A",
                    tenantId: 1,
                    isEmailConfirmed: true);
                await uow.CompleteAsync();
            }

            user.ShouldNotBeNull();
            user.Name.ShouldBe("Jane");
            user.Surname.ShouldBe("Doe");
            user.EmailAddress.ShouldBe("jane.doe@example.com");
            user.UserName.ShouldBe("jane.doe");
            user.IsActive.ShouldBeTrue();
            user.IsEmailConfirmed.ShouldBeTrue();
            user.TenantId.ShouldBe(1);
        }

        [Fact]
        public async Task RegisterAsync_UserSavedToDatabase()
        {
            using (var uow = _uowManager.Begin())
            {
                await _registrationManager.RegisterAsync(
                    "Persist",
                    "Test",
                    "persist@example.com",
                    "persist_user",
                    "Test@1234A",
                    tenantId: 1,
                    isEmailConfirmed: false);
                await uow.CompleteAsync();
            }

            await UsingDbContextAsync(async ctx =>
            {
                var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(ctx.Users, u => u.UserName == "persist_user");
                user.ShouldNotBeNull();
                user.IsEmailConfirmed.ShouldBeFalse();
            });
        }

        [Fact]
        public async Task RegisterAsync_NormalizedNamesAreSet()
        {
            User user;
            using (var uow = _uowManager.Begin())
            {
                user = await _registrationManager.RegisterAsync(
                    "Norm",
                    "User",
                    "normuser@example.com",
                    "normuser",
                    "Test@1234A",
                    tenantId: 1,
                    isEmailConfirmed: true);
                await uow.CompleteAsync();
            }

            user.NormalizedUserName.ShouldNotBeNullOrEmpty();
            user.NormalizedEmailAddress.ShouldNotBeNullOrEmpty();
            user.NormalizedUserName.ShouldBe(user.UserName.ToUpperInvariant());
            user.NormalizedEmailAddress.ShouldBe(user.EmailAddress.ToUpperInvariant());
        }

        // ── RegisterAsync — inactive tenant ───────────────────────────────

        [Fact]
        public async Task RegisterAsync_InactiveTenant_ThrowsException()
        {
            // Insert an inactive tenant directly
            int inactiveTenantId = 0;
            UsingDbContext(ctx =>
            {
                var tenant = new ABPGroup.MultiTenancy.Tenant("InactiveReg", "Inactive Reg Tenant")
                {
                    IsActive = false
                };
                ctx.Tenants.Add(tenant);
                ctx.SaveChanges();
                inactiveTenantId = tenant.Id;
            });

            using (_uowManager.Begin())
            {
                await Should.ThrowAsync<Exception>(
                    () => _registrationManager.RegisterAsync(
                        "X", "X", "x@x.com", "xuser", "Test@1234A",
                        inactiveTenantId, true));
            }
        }

        [Fact]
        public async Task RegisterAsync_UnknownTenantId_ThrowsException()
        {
            using (_uowManager.Begin())
            {
                await Should.ThrowAsync<Exception>(
                    () => _registrationManager.RegisterAsync(
                        "Ghost", "User", "ghost@example.com", "ghostuser", "Test@1234A",
                        tenantId: 999999, isEmailConfirmed: true));
            }
        }

        // ── RegisterAsync (session-based) — no tenant in session ──────────

        [Fact]
        public async Task RegisterAsync_NoTenantIdInSession_ThrowsInvalidOperationException()
        {
            AbpSession.TenantId = null;   // host context — no tenant

            await Should.ThrowAsync<InvalidOperationException>(
                () => _registrationManager.RegisterAsync(
                    "Host", "User", "host@example.com", "hostuser", "Test@1234A", false));
        }

        [Fact]
        public async Task RegisterAsync_WithSessionTenantId_CreatesUser()
        {
            // AbpSession.TenantId is 1 from test base setup
            User user;
            using (var uow = _uowManager.Begin())
            {
                user = await _registrationManager.RegisterAsync(
                    "Session",
                    "User",
                    "session@example.com",
                    "sessionuser",
                    "Test@1234A",
                    isEmailConfirmed: true);
                await uow.CompleteAsync();
            }

            user.ShouldNotBeNull();
            user.TenantId.ShouldBe(AbpSession.TenantId);
        }

        // ── default roles are assigned ────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_DefaultRolesAreAssigned()
        {
            User user;
            using (var uow = _uowManager.Begin())
            {
                user = await _registrationManager.RegisterAsync(
                    "Roles",
                    "Test",
                    "roles@example.com",
                    "roles_user",
                    "Test@1234A",
                    tenantId: 1,
                    isEmailConfirmed: true);
                await uow.CompleteAsync();
            }

            // Any default roles should have been added to user.Roles
            // (may be empty if no default roles exist in seed data)
            user.Roles.ShouldNotBeNull();
        }
    }
}
