using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using ABPGroup.Authentication.External;
using ABPGroup.Authentication.External.GitHub;
using ABPGroup.Authentication.JwtBearer;
using ABPGroup.Authorization;
using ABPGroup.Authorization.Roles;
using ABPGroup.Authorization.Users;
using ABPGroup.Editions;
using ABPGroup.Models.TokenAuth;
using ABPGroup.MultiTenancy;
using ABPGroup.Persons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ABPGroup.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : ABPGroupControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IRepository<User, long> _userRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IUserClaimsPrincipalFactory<User> _userClaimsPrincipalFactory;
        private readonly IGitHubApiService _gitHubApiService;
        private readonly IGitHubUserService _gitHubUserService;
        private readonly IConfiguration _appConfiguration;
        private readonly TenantManager _tenantManager;
        private readonly EditionManager _editionManager;
        private readonly RoleManager _roleManager;
        private readonly IAbpZeroDbMigrator _abpZeroDbMigrator;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IRepository<User, long> userRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
            IGitHubApiService gitHubApiService,
            IGitHubUserService gitHubUserService,
            IConfiguration appConfiguration,
            TenantManager tenantManager,
            EditionManager editionManager,
            RoleManager roleManager,
            IAbpZeroDbMigrator abpZeroDbMigrator)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _userRepository = userRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
            _gitHubApiService = gitHubApiService;
            _gitHubUserService = gitHubUserService;
            _appConfiguration = appConfiguration;
            _tenantManager = tenantManager;
            _editionManager = editionManager;
            _roleManager = roleManager;
            _abpZeroDbMigrator = abpZeroDbMigrator;
        }

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            string tenancyName;
            if (!string.IsNullOrWhiteSpace(model.TenancyName))
            {
                tenancyName = model.TenancyName.Trim();
            }
            else
            {
                tenancyName = GetTenancyNameOrNull()
                    ?? await ResolveTenanctNameFromUserAsync(model.UserNameOrEmailAddress);
            }

            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                tenancyName
            );

            var roleNames = loginResult.Identity.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Distinct()
                .ToArray();

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id,
                UserName = loginResult.User.UserName,
                Name = loginResult.User.Name,
                Surname = loginResult.User.Surname,
                EmailAddress = loginResult.User.EmailAddress,
                RoleNames = roleNames,
                Role = (int)loginResult.User.Role,
                RoleName = ((PersonRole)loginResult.User.Role).ToString()
            };
        }

        [HttpGet]
        public IActionResult GitHubLogin([FromQuery] long? linkUserId = null)
        {
            var state = Guid.NewGuid().ToString("N");

            Response.Cookies.Append("github_oauth_state", state, new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromMinutes(10)
            });

            if (linkUserId.HasValue)
            {
                Response.Cookies.Append("github_link_user_id", linkUserId.Value.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = HttpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    MaxAge = TimeSpan.FromMinutes(10)
                });
            }

            var clientId = GetGitHubOAuthConfig("ClientId");
            var redirectUri = Uri.EscapeDataString(GetGitHubOAuthConfig("RedirectUri"));
            var scope = Uri.EscapeDataString("user:email repo");

            return Redirect(
                "https://github.com/login/oauth/authorize" +
                "?client_id=" + clientId +
                "&redirect_uri=" + redirectUri +
                "&scope=" + scope +
                "&state=" + state);
        }

        [HttpGet]
        public async Task<IActionResult> GitHubCallback([FromQuery] string code, [FromQuery] string state)
        {
            var clientRoot = (_appConfiguration["App:ClientRootAddress"] ?? "").TrimEnd('/');

            var savedState = Request.Cookies["github_oauth_state"];
            if (string.IsNullOrEmpty(savedState) || savedState != state)
            {
                return Redirect(clientRoot + "/auth?error=invalid_state");
            }

            Response.Cookies.Delete("github_oauth_state");

            var linkedUserIdRaw = Request.Cookies["github_link_user_id"];
            Response.Cookies.Delete("github_link_user_id");

            var githubAccessToken = await _gitHubApiService.ExchangeCodeForAccessTokenAsync(
                code,
                GetGitHubOAuthConfig("ClientId"),
                GetGitHubOAuthConfig("ClientSecret"),
                GetGitHubOAuthConfig("RedirectUri"));

            if (string.IsNullOrEmpty(githubAccessToken))
            {
                return Redirect(clientRoot + "/auth?error=token_exchange_failed");
            }

            var githubUser = await _gitHubApiService.GetUserInfoAsync(githubAccessToken);
            if (githubUser == null)
            {
                return Redirect(clientRoot + "/auth?error=user_info_failed");
            }

            User user;
            try
            {
                if (!string.IsNullOrWhiteSpace(linkedUserIdRaw) && long.TryParse(linkedUserIdRaw, out var linkedUserId))
                {
                    user = await LinkGitHubToExistingUserAsync(linkedUserId, githubUser, githubAccessToken);
                }
                else
                {
                    // Returning user: reuse their tenant. First login: provision a new one.
                    var existingUser = await FindUserByGitHubIdAsync(githubUser.Id.ToString());
                    var tenantId = existingUser != null
                        ? existingUser.TenantId.Value
                        : await CreateTenantForGitHubUserAsync(githubUser);

                    using (_unitOfWorkManager.Current.SetTenantId(tenantId))
                    using (var uow = _unitOfWorkManager.Begin())
                    {
                        user = await _gitHubUserService.GetOrCreateAsync(githubUser, githubAccessToken, tenantId);
                        await uow.CompleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GitHub user creation failed", ex);
                return Redirect(clientRoot + "/auth?error=user_creation_failed");
            }

            // AFTER — scoped to the user's tenant so tenantId claim is included
            ClaimsIdentity identity;
            // Scope to the user's tenant so both UserId AND TenantId claims are stamped
            using (_unitOfWorkManager.Current.SetTenantId(user.TenantId))
            {
                var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
                identity = (ClaimsIdentity)principal.Identity;

                var roles = _roleManager.Roles
                    .Where(r => r.TenantId == user.TenantId)
                    .Select(r => r.Name)
                    .ToList();

                foreach (var role in roles)
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var accessToken = CreateAccessToken(CreateJwtClaims(identity));
            var expireInSeconds = (int)_configuration.Expiration.TotalSeconds;

            //return Redirect($"{clientRoot}/auth/github/callback?token={Uri.EscapeDataString(accessToken)}&userId={user.Id}&expireInSeconds={expireInSeconds}");
            return Redirect(
                $"{clientRoot}/auth/github/callback" +
                $"?token={Uri.EscapeDataString(accessToken)}" +
                $"&userId={user.Id}" +
                $"&expireInSeconds={expireInSeconds}" +
                $"&tenantId={user.TenantId}" +
                $"&userName={Uri.EscapeDataString(user.UserName ?? "")}" +
                $"&name={Uri.EscapeDataString(user.Name ?? "")}" +
                $"&surname={Uri.EscapeDataString(user.Surname ?? "")}" +
                $"&email={Uri.EscapeDataString(user.EmailAddress ?? "")}" +
                $"&avatarUrl={Uri.EscapeDataString(user.AvatarUrl ?? "")}" +
                $"&githubUsername={Uri.EscapeDataString(user.GitHubUsername ?? "")}" +
                $"&roleNames={Uri.EscapeDataString(string.Join(",", identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)))}" +
                $"&role={(int)user.Role}" +
                $"&roleName={Uri.EscapeDataString(((PersonRole)user.Role).ToString())}"
            );
        }


        private async Task<int> CreateTenantForGitHubUserAsync(GitHubUserInfo githubUser)
        {
            // Derive a safe tenancy name from the GitHub login, falling back to the
            // numeric GitHub user ID when the login produces an empty slug.
            var baseName = Regex.Replace(
                (githubUser.Login ?? githubUser.Id.ToString()).ToLowerInvariant(),
                @"[^a-z0-9\-]", "");

            if (baseName.IsNullOrWhiteSpace())
                baseName = $"gh{githubUser.Id}";

            // Ensure uniqueness.
            var tenancyName = baseName;
            var counter = 1;
            while (await _tenantManager.FindByTenancyNameAsync(tenancyName) != null)
            {
                tenancyName = $"{baseName}{counter++}";
            }

            var displayName = githubUser.Name.IsNullOrWhiteSpace()
                ? tenancyName
                : githubUser.Name;

            var tenant = new Tenant(tenancyName, displayName) { IsActive = true };

            var defaultEdition = await _editionManager.FindByNameAsync(EditionManager.DefaultEditionName);
            if (defaultEdition != null)
                tenant.EditionId = defaultEdition.Id;

            await _tenantManager.CreateAsync(tenant);
            await CurrentUnitOfWork.SaveChangesAsync();

            try
            {
                _abpZeroDbMigrator.CreateOrMigrateForTenant(tenant);
            }
            catch (Exception ex)
            {
                throw new Abp.UI.UserFriendlyException("Tenant database migration failed.", ex.Message);
            }

            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                CheckErrors(await _roleManager.CreateStaticRoles(tenant.Id));
                await CurrentUnitOfWork.SaveChangesAsync();

                var adminRole = _roleManager.Roles.Single(r => r.Name == StaticRoleNames.Tenants.Admin);
                await _roleManager.GrantAllPermissionsAsync(adminRole);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return tenant.Id;
        }

        private async Task<User> FindUserByGitHubIdAsync(string githubId)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                var user = await _userRepository.FirstOrDefaultAsync(
                    u => u.GitHubUsername == githubId && u.TenantId != null);

                uow.Complete();
                return user;
            }
        }

        private string GetGitHubOAuthConfig(string key)
        {
            var value = _appConfiguration[$"GitHubOAuth:{key}"];
            return string.IsNullOrEmpty(value) ? _appConfiguration[$"GitHub:{key}"] : value;
        }

        private async Task<User> LinkGitHubToExistingUserAsync(long userId, GitHubUserInfo githubUser, string githubAccessToken)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                var user = await _userRepository.FirstOrDefaultAsync(userId);
                if (user == null)
                {
                    throw new Exception($"User with id {userId} was not found for GitHub linking.");
                }

                user.GitHubUsername = githubUser.Id.ToString();
                user.GitHubAccessToken = githubAccessToken;
                if (!string.IsNullOrWhiteSpace(githubUser.AvatarUrl))
                {
                    user.AvatarUrl = githubUser.AvatarUrl;
                }

                await _userRepository.UpdateAsync(user);
                await uow.CompleteAsync();

                return user;
            }
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<string> ResolveTenanctNameFromUserAsync(string userNameOrEmailAddress)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                var normalized = userNameOrEmailAddress.ToUpperInvariant();
                var user = await _userRepository.FirstOrDefaultAsync(
                    u => u.TenantId != null &&
                         (u.NormalizedUserName == normalized || u.NormalizedEmailAddress == normalized)
                );

                string tenancyName = null;
                if (user?.TenantId != null)
                    tenancyName = _tenantCache.GetOrNull(user.TenantId.Value)?.TenancyName;

                uow.Complete();
                return tenancyName;
            }
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken);
        }
    }
}