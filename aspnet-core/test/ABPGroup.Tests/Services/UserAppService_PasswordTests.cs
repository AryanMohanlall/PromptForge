using Abp.UI;
using ABPGroup.Users;
using ABPGroup.Users.Dto;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Services
{
    /// <summary>
    /// Extended tests for UserAppService focusing on the ChangePassword and ResetPassword flows.
    /// The test base seeds a default admin user with password "123qwe".
    /// </summary>
    public class UserAppService_PasswordTests : ABPGroupTestBase
    {
        private readonly IUserAppService _userAppService;

        public UserAppService_PasswordTests()
        {
            _userAppService = Resolve<IUserAppService>();
        }

        // ── ChangePassword ────────────────────────────────────────────────

        [Fact]
        public async Task ChangePassword_CorrectCurrentPassword_ReturnsTrue()
        {
            var result = await _userAppService.ChangePassword(new ChangePasswordDto
            {
                CurrentPassword = "123qwe",
                NewPassword = "New@Password1"
            });

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task ChangePassword_WrongCurrentPassword_ThrowsAbpException()
        {
            await Should.ThrowAsync<Exception>(
                () => _userAppService.ChangePassword(new ChangePasswordDto
                {
                    CurrentPassword = "wrong-password",
                    NewPassword = "New@Password1"
                }));
        }

        [Fact]
        public async Task ChangePassword_PasswordActuallyChanges()
        {
            const string newPassword = "New@Password2";

            await _userAppService.ChangePassword(new ChangePasswordDto
            {
                CurrentPassword = "123qwe",
                NewPassword = newPassword
            });

            // Verify the password hash was updated — try changing again with new password
            var result = await _userAppService.ChangePassword(new ChangePasswordDto
            {
                CurrentPassword = newPassword,
                NewPassword = "Another@Password3"
            });

            result.ShouldBeTrue();
        }

        // ── ResetPassword ─────────────────────────────────────────────────

        [Fact]
        public async Task ResetPassword_AdminResetsOwnPassword_ReturnsTrue()
        {
            var adminUser = await GetCurrentUserAsync();

            var result = await _userAppService.ResetPassword(new ResetPasswordDto
            {
                UserId = adminUser.Id,
                AdminPassword = "123qwe",
                NewPassword = "Reset@Password1"
            });

            result.ShouldBeTrue();
        }

        [Fact]
        public async Task ResetPassword_WrongAdminPassword_ThrowsUserFriendlyException()
        {
            var adminUser = await GetCurrentUserAsync();

            await Should.ThrowAsync<UserFriendlyException>(
                () => _userAppService.ResetPassword(new ResetPasswordDto
                {
                    UserId = adminUser.Id,
                    AdminPassword = "wrong-admin-pass",
                    NewPassword = "Reset@Password1"
                }));
        }

        [Fact]
        public async Task ResetPassword_NotLoggedIn_ThrowsException()
        {
            // When no user is logged in, the class-level [AbpAuthorize] interceptor
            // throws AbpAuthorizationException before the method body is reached.
            AbpSession.UserId = null;

            await Should.ThrowAsync<Exception>(
                () => _userAppService.ResetPassword(new ResetPasswordDto
                {
                    UserId = 1,
                    AdminPassword = "123qwe",
                    NewPassword = "New@Password1"
                }));
        }

        // ── GetUsers (basic filtering) ────────────────────────────────────

        [Fact]
        public async Task GetUsers_FilterByIsActive_ReturnsOnlyActiveUsers()
        {
            var result = await _userAppService.GetAllAsync(
                new PagedUserResultRequestDto
                {
                    IsActive = true,
                    MaxResultCount = 50,
                    SkipCount = 0,
                    Sorting = "UserName"
                });

            result.Items.ShouldAllBe(u => u.IsActive);
        }

        [Fact]
        public async Task GetUsers_FilterByKeyword_MatchesUsernameOrEmail()
        {
            await _userAppService.CreateAsync(new CreateUserDto
            {
                Name = "Search",
                Surname = "User",
                UserName = "search_keyword_user",
                EmailAddress = "searchkeyword@example.com",
                Password = "Test@1234A",
                IsActive = true
            });

            var result = await _userAppService.GetAllAsync(
                new PagedUserResultRequestDto
                {
                    Keyword = "search_keyword",
                    MaxResultCount = 50,
                    SkipCount = 0,
                    Sorting = "UserName"
                });

            result.TotalCount.ShouldBeGreaterThanOrEqualTo(1);
        }

        // ── Activate / DeActivate ─────────────────────────────────────────

        [Fact]
        public async Task DeActivate_ActiveUser_SetsIsActiveFalse()
        {
            var created = await _userAppService.CreateAsync(new CreateUserDto
            {
                Name = "De",
                Surname = "Activate",
                UserName = "deactivate_user",
                EmailAddress = "deactivate@example.com",
                Password = "Test@1234A",
                IsActive = true
            });

            await _userAppService.DeActivate(new Abp.Application.Services.Dto.EntityDto<long>(created.Id));

            await UsingDbContextAsync(async ctx =>
            {
                var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(ctx.Users, u => u.Id == created.Id);
                user!.IsActive.ShouldBeFalse();
            });
        }

        [Fact]
        public async Task Activate_InactiveUser_SetsIsActiveTrue()
        {
            var created = await _userAppService.CreateAsync(new CreateUserDto
            {
                Name = "Re",
                Surname = "Activate",
                UserName = "reactivate_user",
                EmailAddress = "reactivate@example.com",
                Password = "Test@1234A",
                IsActive = false
            });

            await _userAppService.Activate(new Abp.Application.Services.Dto.EntityDto<long>(created.Id));

            await UsingDbContextAsync(async ctx =>
            {
                var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(ctx.Users, u => u.Id == created.Id);
                user!.IsActive.ShouldBeTrue();
            });
        }

        // ── DeleteAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_ExistingUser_RemovesFromDatabase()
        {
            var created = await _userAppService.CreateAsync(new CreateUserDto
            {
                Name = "Delete",
                Surname = "Me",
                UserName = "delete_me_user",
                EmailAddress = "deleteme@example.com",
                Password = "Test@1234A",
                IsActive = true
            });

            await _userAppService.DeleteAsync(
                new Abp.Application.Services.Dto.EntityDto<long>(created.Id));

            await UsingDbContextAsync(async ctx =>
            {
                var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(ctx.Users, u => u.Id == created.Id && !u.IsDeleted);
                user.ShouldBeNull();
            });
        }
    }
}
