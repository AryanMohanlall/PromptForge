using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using ABPGroup.Authentication.External;
using ABPGroup.Authentication.External.GitHub;
using ABPGroup.Authentication.JwtBearer;
using ABPGroup.Authorization;
using ABPGroup.Authorization.Users;
using ABPGroup.Models.TokenAuth;
using ABPGroup.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
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
            IConfiguration appConfiguration)
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
                RoleNames = roleNames
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
                    user = await _gitHubUserService.GetOrCreateAsync(githubUser, githubAccessToken);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("GitHub user creation failed", ex);
                return Redirect(clientRoot + "/auth?error=user_creation_failed");
            }

            var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity;
            var accessToken = CreateAccessToken(CreateJwtClaims(identity));
            var expireInSeconds = (int)_configuration.Expiration.TotalSeconds;

            return Redirect($"{clientRoot}/auth/github/callback?token={Uri.EscapeDataString(accessToken)}&userId={user.Id}&expireInSeconds={expireInSeconds}");
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
