using Abp.Authorization;
using Abp.Domain.Repositories;
using ABPGroup.Authentication.External.GitHub;
using ABPGroup.Authorization.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ABPGroup.Controllers
{
    [ApiController]
    [AbpAuthorize]
    [Route("api/github-app")]
    public class GitHubAppController : ABPGroupControllerBase
    {
        public class CreateRepositoryInput
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsPrivate { get; set; } = true;
            public bool AutoInit { get; set; } = true;
            public string Owner { get; set; }
            public string InstallationId { get; set; }
        }

        private readonly IConfiguration _configuration;
        private readonly GitHubApiService _gitHubApiService;
        private readonly IRepository<User, long> _userRepository;

        public GitHubAppController(
            IConfiguration configuration,
            GitHubApiService gitHubApiService,
            IRepository<User, long> userRepository)
        {
            _configuration = configuration;
            _gitHubApiService = gitHubApiService;
            _userRepository = userRepository;
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var appId = _configuration["GitHubApp:AppId"];
            var privateKeyPem = _configuration["GitHubApp:PrivateKeyPem"];

            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKeyPem))
            {
                return BadRequest(new
                {
                    message = "GitHub App credentials are not fully configured.",
                    hasAppId = !string.IsNullOrWhiteSpace(appId),
                    hasPrivateKey = !string.IsNullOrWhiteSpace(privateKeyPem)
                });
            }

            try
            {
                var appInfo = await _gitHubApiService.GetGitHubAppInfoAsync(appId, privateKeyPem);
                return Ok(new
                {
                    connected = true,
                    appInfo.Id,
                    appInfo.Slug,
                    appInfo.Name
                });
            }
            catch (Exception ex)
            {
                Logger.Warn("GitHub App status check failed.", ex);
                return StatusCode(502, new
                {
                    connected = false,
                    message = "GitHub App status check failed.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("repositories")]
        public async Task<IActionResult> Repositories([FromQuery] string installationId = null)
        {
            var appId = _configuration["GitHubApp:AppId"];
            var privateKeyPem = _configuration["GitHubApp:PrivateKeyPem"];
            var resolvedInstallationId = installationId ?? _configuration["GitHubApp:InstallationId"];

            if (string.IsNullOrWhiteSpace(appId) ||
                string.IsNullOrWhiteSpace(privateKeyPem) ||
                string.IsNullOrWhiteSpace(resolvedInstallationId))
            {
                return BadRequest(new
                {
                    message = "GitHub App configuration is incomplete.",
                    hasAppId = !string.IsNullOrWhiteSpace(appId),
                    hasPrivateKey = !string.IsNullOrWhiteSpace(privateKeyPem),
                    hasInstallationId = !string.IsNullOrWhiteSpace(resolvedInstallationId)
                });
            }

            try
            {
                var installationToken = await _gitHubApiService.CreateInstallationTokenAsync(
                    appId,
                    resolvedInstallationId,
                    privateKeyPem);

                var repositories = await _gitHubApiService.GetInstallationRepositoriesAsync(installationToken);
                return Ok(new
                {
                    installationId = resolvedInstallationId,
                    count = repositories.Count,
                    repositories
                });
            }
            catch (Exception ex)
            {
                Logger.Warn("GitHub installation repositories request failed.", ex);
                return StatusCode(502, new
                {
                    message = "Failed to query installation repositories.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("repositories")]
        public async Task<IActionResult> CreateRepository([FromBody] CreateRepositoryInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Name))
            {
                return BadRequest(new { message = "Repository name is required." });
            }

            var appId = _configuration["GitHubApp:AppId"];
            var privateKeyPem = _configuration["GitHubApp:PrivateKeyPem"];
            var resolvedInstallationId = input.InstallationId ?? _configuration["GitHubApp:InstallationId"];
            var canUseAppFlow =
                !string.IsNullOrWhiteSpace(appId) &&
                !string.IsNullOrWhiteSpace(privateKeyPem) &&
                !string.IsNullOrWhiteSpace(resolvedInstallationId);

            try
            {
                var owner = input.Owner;
                string installationAccountType = null;
                string installationAccountLogin = null;

                if (canUseAppFlow && string.IsNullOrWhiteSpace(owner))
                {
                    var installation = await _gitHubApiService.GetInstallationInfoAsync(
                        appId,
                        resolvedInstallationId,
                        privateKeyPem);

                    installationAccountType = installation?.Account?.Type;
                    installationAccountLogin = installation?.Account?.Login;

                    owner = installationAccountType == "Organization"
                        ? installationAccountLogin
                        : null;
                }

                if (canUseAppFlow && !string.IsNullOrWhiteSpace(owner))
                {
                    var installationToken = await _gitHubApiService.CreateInstallationTokenAsync(
                        appId,
                        resolvedInstallationId,
                        privateKeyPem);

                    var appRepository = await _gitHubApiService.CreateRepositoryAsync(
                        installationToken,
                        input.Name,
                        input.IsPrivate,
                        input.Description,
                        input.AutoInit,
                        owner);

                    return Ok(new
                    {
                        created = true,
                        authMode = "github-app",
                        repository = appRepository
                    });
                }

                // Fallback for user-owned installations: use the authenticated user's GitHub OAuth token.
                var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(userGitHubToken))
                {
                    return BadRequest(new
                    {
                        message = "GitHub OAuth token is missing for the current user.",
                        details = "Sign in with GitHub first, then retry repository creation.",
                        installationType = installationAccountType,
                        installationAccount = installationAccountLogin
                    });
                }

                var oauthRepository = await _gitHubApiService.CreateRepositoryWithUserTokenAsync(
                    userGitHubToken,
                    input.Name,
                    input.IsPrivate,
                    input.Description,
                    input.AutoInit,
                    input.Owner);

                return Ok(new
                {
                    created = true,
                    authMode = "oauth-user",
                    repository = oauthRepository
                });
            }
            catch (Exception ex)
            {
                Logger.Warn("GitHub repository creation failed.", ex);
                return StatusCode(502, new
                {
                    message = "Failed to create repository via GitHub App.",
                    error = ex.Message
                });
            }
        }

        private async Task<string> GetCurrentUserGitHubAccessTokenAsync()
        {
            if (!AbpSession.UserId.HasValue)
            {
                return null;
            }

            var user = await _userRepository.FirstOrDefaultAsync(AbpSession.UserId.Value);
            return user?.GitHubAccessToken;
        }
    }
}
