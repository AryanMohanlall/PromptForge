using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using ABPGroup.Authentication.External.GitHub;
using ABPGroup.Authentication.JwtBearer;
using ABPGroup.Authorization.Users;
using ABPGroup.Controllers;
using ABPGroup.Identity;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.GitHub
{
    /// <summary>
    /// Unit tests for TokenAuthController's GitHub OAuth flow.
    /// All external HTTP calls are replaced with Moq stubs.
    /// </summary>
    public class GitHubAuthControllerTests
    {
        // ── shared test doubles ────────────────────────────────────────────

        private readonly Mock<IGitHubApiService> _gitHubApi;
        private readonly Mock<IGitHubUserService> _gitHubUserService;
        private readonly Mock<IConfiguration> _config;
        private readonly Mock<IUserClaimsPrincipalFactory<User>> _claimsFactory;
        private readonly Mock<IRepository<User, long>> _userRepository;
        private readonly Mock<IUnitOfWorkManager> _uowManager;
        private readonly TokenAuthConfiguration _tokenConfig;

        // HttpContext pieces
        private readonly Mock<HttpContext> _httpContext;
        private readonly Mock<HttpRequest> _request;
        private readonly Mock<HttpResponse> _response;
        private readonly Mock<IRequestCookieCollection> _requestCookies;
        private readonly Mock<IResponseCookies> _responseCookies;

        private const string ClientRoot = "https://app.example.com";
        private const string GitHubClientId = "gh-client-id";
        private const string GitHubClientSecret = "gh-client-secret";
        private const string GitHubRedirectUri = "https://app.example.com/callback";

        public GitHubAuthControllerTests()
        {
            _gitHubApi = new Mock<IGitHubApiService>();
            _gitHubUserService = new Mock<IGitHubUserService>();
            _config = new Mock<IConfiguration>();
            _claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _userRepository = new Mock<IRepository<User, long>>();
            _uowManager = new Mock<IUnitOfWorkManager>();

            // JWT config — key must be >= 256 bits for HMAC-SHA256
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("unit-test-secret-key-must-be-32-chars!"));
            _tokenConfig = new TokenAuthConfiguration
            {
                SecurityKey = key,
                Issuer = "test-issuer",
                Audience = "test-audience",
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
                Expiration = TimeSpan.FromDays(1)
            };

            // IConfiguration key lookups
            _config.Setup(c => c["App:ClientRootAddress"]).Returns(ClientRoot);
            _config.Setup(c => c["GitHub:ClientId"]).Returns(GitHubClientId);
            _config.Setup(c => c["GitHub:ClientSecret"]).Returns(GitHubClientSecret);
            _config.Setup(c => c["GitHub:RedirectUri"]).Returns(GitHubRedirectUri);

            // HttpContext mocks
            _requestCookies = new Mock<IRequestCookieCollection>();
            _responseCookies = new Mock<IResponseCookies>();

            _request = new Mock<HttpRequest>();
            _request.Setup(r => r.Cookies).Returns(_requestCookies.Object);
            _request.Setup(r => r.IsHttps).Returns(false);

            _response = new Mock<HttpResponse>();
            _response.Setup(r => r.Cookies).Returns(_responseCookies.Object);

            _httpContext = new Mock<HttpContext>();
            _httpContext.Setup(c => c.Request).Returns(_request.Object);
            _httpContext.Setup(c => c.Response).Returns(_response.Object);
        }

        private TokenAuthController BuildController()
        {
            var controller = new TokenAuthController(
                logInManager: null,              // not used in GitHub paths
                tenantCache: Mock.Of<ITenantCache>(),
                abpLoginResultTypeHelper: null,  // not used in GitHub paths
                configuration: _tokenConfig,
                userRepository: _userRepository.Object,
                unitOfWorkManager: _uowManager.Object,
                userClaimsPrincipalFactory: _claimsFactory.Object,
                gitHubApiService: _gitHubApi.Object,
                gitHubUserService: _gitHubUserService.Object,
                appConfiguration: _config.Object
            );

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext.Object
            };

            // ABP injects Logger via property; provide NullLogger so Logger.Error doesn't throw
            controller.Logger = NullLogger.Instance;

            return controller;
        }

        // ── GitHubLogin ────────────────────────────────────────────────────

        [Fact]
        public void GitHubLogin_ReturnsRedirectResult()
        {
            var controller = BuildController();

            var result = controller.GitHubLogin();

            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public void GitHubLogin_RedirectUrl_PointsToGitHubAuthorizeEndpoint()
        {
            var controller = BuildController();

            var result = (RedirectResult)controller.GitHubLogin();

            Assert.StartsWith("https://github.com/login/oauth/authorize", result.Url);
        }

        [Fact]
        public void GitHubLogin_RedirectUrl_ContainsClientId()
        {
            var controller = BuildController();

            var result = (RedirectResult)controller.GitHubLogin();

            Assert.Contains("client_id=" + GitHubClientId, result.Url);
        }

        [Fact]
        public void GitHubLogin_RedirectUrl_ContainsEncodedRedirectUri()
        {
            var controller = BuildController();

            var result = (RedirectResult)controller.GitHubLogin();

            Assert.Contains("redirect_uri=", result.Url);
            // Encoded URI should be present somewhere in the URL
            Assert.Contains("app.example.com", result.Url);
        }

        [Fact]
        public void GitHubLogin_RedirectUrl_ContainsScope()
        {
            var controller = BuildController();

            var result = (RedirectResult)controller.GitHubLogin();

            Assert.Contains("scope=", result.Url);
        }

        [Fact]
        public void GitHubLogin_RedirectUrl_ContainsStateParam()
        {
            var controller = BuildController();

            var result = (RedirectResult)controller.GitHubLogin();

            Assert.Contains("state=", result.Url);
        }

        [Fact]
        public void GitHubLogin_SetsStateCoookie()
        {
            var controller = BuildController();

            controller.GitHubLogin();

            _responseCookies.Verify(
                c => c.Append(
                    "github_oauth_state",
                    It.IsNotNull<string>(),
                    It.IsAny<CookieOptions>()),
                Times.Once);
        }

        // ── GitHubCallback — state validation ──────────────────────────────

        [Fact]
        public async Task GitHubCallback_MissingStateCookie_RedirectsWithInvalidState()
        {
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns((string)null);
            var controller = BuildController();

            var result = (RedirectResult)await controller.GitHubCallback("code", "some-state");

            Assert.Equal(ClientRoot + "/auth?error=invalid_state", result.Url);
        }

        [Fact]
        public async Task GitHubCallback_StateMismatch_RedirectsWithInvalidState()
        {
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns("saved-state");
            var controller = BuildController();

            var result = (RedirectResult)await controller.GitHubCallback("code", "different-state");

            Assert.Equal(ClientRoot + "/auth?error=invalid_state", result.Url);
        }

        // ── GitHubCallback — token exchange failure ─────────────────────────

        [Fact]
        public async Task GitHubCallback_TokenExchangeFails_RedirectsWithTokenError()
        {
            const string state = "matching-state";
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns(state);
            _gitHubApi
                .Setup(s => s.ExchangeCodeForAccessTokenAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string)null);

            var controller = BuildController();

            var result = (RedirectResult)await controller.GitHubCallback("code", state);

            Assert.Equal(ClientRoot + "/auth?error=token_exchange_failed", result.Url);
        }

        [Fact]
        public async Task GitHubCallback_TokenExchangeFails_DoesNotCallGetUserInfo()
        {
            const string state = "state-abc";
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns(state);
            _gitHubApi
                .Setup(s => s.ExchangeCodeForAccessTokenAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string)null);

            var controller = BuildController();
            await controller.GitHubCallback("code", state);

            _gitHubApi.Verify(
                s => s.GetUserInfoAsync(It.IsAny<string>()),
                Times.Never);
        }

        // ── GitHubCallback — user info failure ─────────────────────────────

        [Fact]
        public async Task GitHubCallback_UserInfoFails_RedirectsWithUserInfoError()
        {
            const string state = "state-xyz";
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns(state);
            _gitHubApi
                .Setup(s => s.ExchangeCodeForAccessTokenAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("gho_token");
            _gitHubApi
                .Setup(s => s.GetUserInfoAsync("gho_token"))
                .ReturnsAsync((GitHubUserInfo)null);

            var controller = BuildController();

            var result = (RedirectResult)await controller.GitHubCallback("code", state);

            Assert.Equal(ClientRoot + "/auth?error=user_info_failed", result.Url);
        }

        // ── GitHubCallback — user creation failure ─────────────────────────

        [Fact]
        public async Task GitHubCallback_UserCreationThrows_RedirectsWithUserCreationFailedError()
        {
            const string state = "state-err";
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns(state);

            var githubUser = new GitHubUserInfo
            {
                Id = 1, Login = "dev", Name = "Dev User", Email = "dev@example.com"
            };

            _gitHubApi
                .Setup(s => s.ExchangeCodeForAccessTokenAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("gho_token");
            _gitHubApi
                .Setup(s => s.GetUserInfoAsync("gho_token"))
                .ReturnsAsync(githubUser);
            _gitHubUserService
                .Setup(s => s.GetOrCreateAsync(It.IsAny<GitHubUserInfo>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Failed to create user: PasswordTooShort"));

            var controller = BuildController();

            var result = (RedirectResult)await controller.GitHubCallback("code", state);

            Assert.Equal(ClientRoot + "/auth?error=user_creation_failed", result.Url);
        }

        // ── GitHubCallback — happy path ────────────────────────────────────

        [Fact]
        public async Task GitHubCallback_Success_RedirectsToFrontendCallback()
        {
            const string state = "state-ok";
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns(state);

            var githubUser = new GitHubUserInfo
            {
                Id = 99, Login = "happy-dev", Name = "Happy Dev", Email = "happy@example.com"
            };
            var appUser = new User { Id = 5, UserName = "happy-dev" };
            var principal = BuildClaimsPrincipal("5");

            _gitHubApi
                .Setup(s => s.ExchangeCodeForAccessTokenAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("gho_token");
            _gitHubApi
                .Setup(s => s.GetUserInfoAsync("gho_token"))
                .ReturnsAsync(githubUser);
            _gitHubUserService
                .Setup(s => s.GetOrCreateAsync(It.IsAny<GitHubUserInfo>(), It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _claimsFactory
                .Setup(f => f.CreateAsync(appUser))
                .ReturnsAsync(principal);

            var controller = BuildController();

            var result = (RedirectResult)await controller.GitHubCallback("code", state);

            Assert.Equal(ClientRoot + "/auth/github/callback", result.Url);
        }

        [Fact]
        public async Task GitHubCallback_Success_SetsGitHubAuthResultCookie()
        {
            const string state = "state-cookie";
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns(state);

            var appUser = new User { Id = 7, UserName = "cookie-dev" };
            var principal = BuildClaimsPrincipal("7");

            _gitHubApi
                .Setup(s => s.ExchangeCodeForAccessTokenAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("gho_token");
            _gitHubApi
                .Setup(s => s.GetUserInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(new GitHubUserInfo { Id = 10, Login = "cookie-dev" });
            _gitHubUserService
                .Setup(s => s.GetOrCreateAsync(It.IsAny<GitHubUserInfo>(), It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _claimsFactory
                .Setup(f => f.CreateAsync(appUser))
                .ReturnsAsync(principal);

            var controller = BuildController();
            await controller.GitHubCallback("code", state);

            _responseCookies.Verify(
                c => c.Append(
                    "github_auth_result",
                    It.IsNotNull<string>(),
                    It.IsAny<CookieOptions>()),
                Times.Once);
        }

        [Fact]
        public async Task GitHubCallback_Success_CallsGetOrCreateWithCorrectToken()
        {
            const string state = "state-check";
            const string accessToken = "gho_access_token_xyz";
            _requestCookies.Setup(c => c["github_oauth_state"]).Returns(state);

            var githubUser = new GitHubUserInfo
            {
                Id = 55, Login = "verify-dev", Email = "verify@example.com"
            };
            var appUser = new User { Id = 3, UserName = "verify-dev" };

            _gitHubApi
                .Setup(s => s.ExchangeCodeForAccessTokenAsync(
                    It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(accessToken);
            _gitHubApi
                .Setup(s => s.GetUserInfoAsync(accessToken))
                .ReturnsAsync(githubUser);
            _gitHubUserService
                .Setup(s => s.GetOrCreateAsync(It.IsAny<GitHubUserInfo>(), It.IsAny<string>()))
                .ReturnsAsync(appUser);
            _claimsFactory
                .Setup(f => f.CreateAsync(appUser))
                .ReturnsAsync(BuildClaimsPrincipal("3"));

            var controller = BuildController();
            await controller.GitHubCallback("code", state);

            _gitHubUserService.Verify(
                s => s.GetOrCreateAsync(githubUser, accessToken),
                Times.Once);
        }

        // ── GitHubUserService — new user creation ──────────────────────────

        [Fact]
        public async Task GitHubUserService_NewUser_CreatesUserAndReturnsIt()
        {
            const string githubToken = "gho_new_user";
            var githubUser = new GitHubUserInfo
            {
                Id = 100, Login = "brandnew", Name = "Brand New", Email = "brand@example.com"
            };

            var userRepo = new Mock<IRepository<User, long>>();
            var uowManager = BuildUowManager();
            var userCreator = new Mock<IUserCreationService>();

            // No existing user by GitHub ID
            userRepo
                .Setup(r => r.FirstOrDefaultAsync(It.Is<System.Linq.Expressions.Expression<System.Func<User, bool>>>(
                    e => ExpressionMatches(e, new User { GitHubUsername = "100" }))))
                .ReturnsAsync((User)null);

            // No existing user by email
            userRepo
                .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                .ReturnsAsync((User)null);

            userCreator
                .Setup(c => c.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var sut = new GitHubUserService(userRepo.Object, uowManager, userCreator.Object);

            var result = await sut.GetOrCreateAsync(githubUser, githubToken);

            Assert.NotNull(result);
            Assert.Equal("100", result.GitHubUsername);
            Assert.Equal(githubToken, result.GitHubAccessToken);
            userCreator.Verify(
                c => c.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task GitHubUserService_ExistingUserByEmail_UpdatesAndReturnsUser()
        {
            const string githubToken = "gho_existing";
            var githubUser = new GitHubUserInfo
            {
                Id = 200,
                Login = "returning-dev",
                Email = "existing@example.com",
                AvatarUrl = "https://avatars.example.com/u/200"
            };

            var existingUser = new User
            {
                Id = 42,
                UserName = "old-username",
                EmailAddress = "existing@example.com",
                NormalizedEmailAddress = "EXISTING@EXAMPLE.COM"
            };

            var userRepo = new Mock<IRepository<User, long>>();
            var uowManager = BuildUowManager();
            var userCreator = new Mock<IUserCreationService>();

            // No match by GitHub ID
            userRepo
                .SetupSequence(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                .ReturnsAsync((User)null)   // first call: by GitHubUsername
                .ReturnsAsync(existingUser); // second call: by email

            userRepo
                .Setup(r => r.UpdateAsync(existingUser))
                .ReturnsAsync(existingUser);

            var sut = new GitHubUserService(userRepo.Object, uowManager, userCreator.Object);

            var result = await sut.GetOrCreateAsync(githubUser, githubToken);

            Assert.Equal(42, result.Id);
            Assert.Equal("200", result.GitHubUsername);
            Assert.Equal(githubToken, result.GitHubAccessToken);
            userCreator.Verify(
                c => c.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task GitHubUserService_UserCreationFails_ThrowsException()
        {
            var githubUser = new GitHubUserInfo
            {
                Id = 300, Login = "fail-dev", Email = "fail@example.com"
            };

            var userRepo = new Mock<IRepository<User, long>>();
            var uowManager = BuildUowManager();
            var userCreator = new Mock<IUserCreationService>();

            userRepo
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                .ReturnsAsync((User)null);

            var failedResult = IdentityResult.Failed(
                new IdentityError { Code = "PasswordTooWeak", Description = "Password is too weak." });
            userCreator
                .Setup(c => c.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(failedResult);

            var sut = new GitHubUserService(userRepo.Object, uowManager, userCreator.Object);

            var ex = await Assert.ThrowsAsync<Exception>(
                () => sut.GetOrCreateAsync(githubUser, "gho_fail"));

            Assert.Contains("Failed to create user", ex.Message);
            Assert.Contains("Password is too weak", ex.Message);
        }

        [Fact]
        public async Task GitHubUserService_ExistingUserByGitHubId_UpdatesTokenAndReturnsUser()
        {
            const string newToken = "gho_refreshed";
            var githubUser = new GitHubUserInfo
            {
                Id = 777, Login = "existing-gh", Email = "gh@example.com"
            };

            var existingUser = new User
            {
                Id = 10,
                GitHubUsername = "777",
                GitHubAccessToken = "gho_old_token"
            };

            var userRepo = new Mock<IRepository<User, long>>();
            var uowManager = BuildUowManager();
            var userCreator = new Mock<IUserCreationService>();

            // First call: match by GitHub ID
            userRepo
                .Setup(r => r.FirstOrDefaultAsync(
                    It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>()))
                .ReturnsAsync(existingUser);

            userRepo
                .Setup(r => r.UpdateAsync(existingUser))
                .ReturnsAsync(existingUser);

            var sut = new GitHubUserService(userRepo.Object, uowManager, userCreator.Object);

            var result = await sut.GetOrCreateAsync(githubUser, newToken);

            Assert.Equal(newToken, result.GitHubAccessToken);
            userCreator.Verify(
                c => c.CreateAsync(It.IsAny<User>(), It.IsAny<string>()),
                Times.Never);
        }

        // ── helpers ────────────────────────────────────────────────────────

        private static ClaimsPrincipal BuildClaimsPrincipal(string nameId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nameId),
                new Claim(ClaimTypes.Name, "Test User")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private static IUnitOfWorkManager BuildUowManager()
        {
            var uowHandle = new Mock<IUnitOfWorkCompleteHandle>();
            uowHandle.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

            var activeUow = new Mock<IActiveUnitOfWork>();
            activeUow
                .Setup(u => u.DisableFilter(It.IsAny<string[]>()))
                .Returns(new Mock<IDisposable>().Object);

            var manager = new Mock<IUnitOfWorkManager>();
            manager.Setup(m => m.Begin()).Returns(uowHandle.Object);
            manager.Setup(m => m.Begin(It.IsAny<UnitOfWorkOptions>())).Returns(uowHandle.Object);
            manager.Setup(m => m.Current).Returns(activeUow.Object);

            return manager.Object;
        }

        /// <summary>
        /// Compiles and evaluates an expression against a sample entity — used only to
        /// provide a readable "dummy" predicate check in Setup calls where the exact lambda
        /// contents matter less than the call sequence.
        /// </summary>
        private static bool ExpressionMatches(
            System.Linq.Expressions.Expression<System.Func<User, bool>> expr, User sample)
        {
            return expr.Compile()(sample);
        }
    }
}
