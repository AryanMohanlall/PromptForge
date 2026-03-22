using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using ABPGroup.Authentication.External.GitHub;
using ABPGroup.VercelDeployment;
using ABPGroup.Deployments;
using ABPGroup.Git;
using ABPGroup.Projects;
using ABPGroup.Authorization.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ABPGroup.Controllers
{
    [ApiController]
    [AbpAuthorize]
    [Route("api/github-app")]
    public class GitHubAppController : ABPGroupControllerBase
    {
        private const long MaxCommitFileSizeBytes = 10L * 1024L * 1024L;

        private static readonly string[] IgnoredCommitDirectories =
        {
            ".git", "node_modules", ".next", "dist", "build",
            "bin", "obj", ".cache", ".turbo", "coverage", ".vercel", "TestResults"
        };

        private static readonly string[] IgnoredCommitFileNames =
        {
            ".DS_Store", "Thumbs.db"
        };

        public class CreateRepositoryInput
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsPrivate { get; set; } = true;
            public bool AutoInit { get; set; } = true;
            public string Owner { get; set; }
            public string InstallationId { get; set; }
            public long ProjectId { get; set; }
        }

        public class CommitGeneratedFilesInput
        {
            public long ProjectId { get; set; }
            public string RepositoryName { get; set; }
            public string RepositoryFullName { get; set; }
            public string Owner { get; set; }
            public string Branch { get; set; } = "main";
            public string CommitMessage { get; set; }
            public bool AutoDeploy { get; set; } = true;

            /// <summary>
            /// Numeric GitHub repository ID returned by the GitHub API when
            /// the repo was created (field "id"). Required for Vercel deployment.
            /// If not provided, the controller will fetch it automatically.
            /// </summary>
            public long RepoId { get; set; }
        }

        private readonly IConfiguration _configuration;
        private readonly GitHubApiService _gitHubApiService;
        private readonly IVercelDeploymentService _vercelDeploymentService;
        private readonly IVercelDeploymentPolicy _vercelDeploymentPolicy;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Project, long> _projectRepository;
        private readonly IRepository<ProjectRepository, long> _projectRepositoryRepository;
        private readonly IRepository<Deployment, long> _deploymentRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public GitHubAppController(
            IConfiguration configuration,
            GitHubApiService gitHubApiService,
            IVercelDeploymentService vercelDeploymentService,
            IVercelDeploymentPolicy vercelDeploymentPolicy,
            IRepository<User, long> userRepository,
            IRepository<Project, long> projectRepository,
            IRepository<ProjectRepository, long> projectRepositoryRepository,
            IRepository<Deployment, long> deploymentRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _configuration = configuration;
            _gitHubApiService = gitHubApiService;
            _vercelDeploymentService = vercelDeploymentService;
            _vercelDeploymentPolicy = vercelDeploymentPolicy;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _projectRepositoryRepository = projectRepositoryRepository;
            _deploymentRepository = deploymentRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        // ── Status ────────────────────────────────────────────────────────────

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
                return Ok(new { connected = true, appInfo.Id, appInfo.Slug, appInfo.Name });
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

        // ── Repositories ──────────────────────────────────────────────────────

        [HttpGet("repositories")]
        public async Task<IActionResult> GetRepositories([FromQuery] string installationId = null)
        {
            var appId = _configuration["GitHubApp:AppId"];
            var privateKeyPem = _configuration["GitHubApp:PrivateKeyPem"];
            var resolvedInstallationId = installationId ?? _configuration["GitHubApp:InstallationId"];
            var canUseAppFlow =
                !string.IsNullOrWhiteSpace(appId) &&
                !string.IsNullOrWhiteSpace(privateKeyPem) &&
                !string.IsNullOrWhiteSpace(resolvedInstallationId);

            // Fall back to OAuth user token if GitHub App is not configured
            if (!canUseAppFlow)
            {
                var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
                if (string.IsNullOrWhiteSpace(userGitHubToken))
                    return BadRequest(new { message = "GitHub OAuth token is missing. Sign in with GitHub first." });

                try
                {
                    var userRepos = await _gitHubApiService.GetUserRepositoriesAsync(userGitHubToken);
                    return Ok(new { count = userRepos.Count, repositories = userRepos });
                }
                catch (Exception ex)
                {
                    Logger.Warn("Failed to fetch user repositories.", ex);
                    return StatusCode(502, new { message = "Failed to fetch repositories.", error = ex.Message });
                }
            }

            try
            {
                var installationToken = await _gitHubApiService.CreateInstallationTokenAsync(
                    appId, resolvedInstallationId, privateKeyPem);
                var repositories = await _gitHubApiService.GetInstallationRepositoriesAsync(installationToken);
                return Ok(new { installationId = resolvedInstallationId, count = repositories.Count, repositories });
            }
            catch (Exception ex)
            {
                Logger.Warn("GitHub installation repositories request failed.", ex);
                return StatusCode(502, new { message = "Failed to query installation repositories.", error = ex.Message });
            }
        }

        [HttpPost("repositories")]
        public async Task<IActionResult> CreateRepository([FromBody] CreateRepositoryInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Name))
                return BadRequest(new { message = "Repository name is required." });

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
                        appId, resolvedInstallationId, privateKeyPem);

                    installationAccountType = installation?.Account?.Type;
                    installationAccountLogin = installation?.Account?.Login;
                    owner = installationAccountType == "Organization" ? installationAccountLogin : null;
                }

                if (canUseAppFlow && !string.IsNullOrWhiteSpace(owner))
                {
                    var installationToken = await _gitHubApiService.CreateInstallationTokenAsync(
                        appId, resolvedInstallationId, privateKeyPem);

                    var appRepository = await _gitHubApiService.CreateRepositoryAsync(
                        installationToken, input.Name, input.IsPrivate,
                        input.Description, input.AutoInit, owner);

                    await StoreRepositoryAsync(
                        input.ProjectId,
                        appRepository.FullName?.Split('/')[0] ?? owner,
                        appRepository.Name,
                        appRepository.FullName,
                        appRepository.HtmlUrl,
                        appRepository.Id.ToString(),
                        input.IsPrivate);

                    return Ok(new { created = true, authMode = "github-app", repository = appRepository });
                }

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
                    userGitHubToken, input.Name, input.IsPrivate,
                    input.Description, input.AutoInit, input.Owner);

                await StoreRepositoryAsync(
                    input.ProjectId,
                    oauthRepository.FullName?.Split('/')[0] ?? input.Owner,
                    oauthRepository.Name,
                    oauthRepository.FullName,
                    oauthRepository.HtmlUrl,
                    oauthRepository.Id.ToString(),
                    input.IsPrivate);

                return Ok(new { created = true, authMode = "oauth-user", repository = oauthRepository });
            }
            catch (Exception ex)
            {
                var duplicateRepo = ex.Message != null &&
                    ex.Message.IndexOf("name already exists on this account", StringComparison.OrdinalIgnoreCase) >= 0;

                if (duplicateRepo)
                {
                    try
                    {
                        var token = await GetCurrentUserGitHubAccessTokenAsync();
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            var owner = input.Owner;
                            if (string.IsNullOrWhiteSpace(owner))
                                owner = await _gitHubApiService.GetCurrentUserLoginAsync(token);

                            var existingRepository = await _gitHubApiService
                                .GetRepositoryWithUserTokenAsync(token, owner, input.Name);

                            await StoreRepositoryAsync(
                                input.ProjectId,
                                existingRepository.FullName?.Split('/')[0] ?? owner,
                                existingRepository.Name,
                                existingRepository.FullName,
                                existingRepository.HtmlUrl,
                                existingRepository.Id.ToString(),
                                existingRepository.Private);

                            return Ok(new
                            {
                                created = false,
                                reused = true,
                                authMode = "oauth-user",
                                repository = existingRepository,
                                message = "Repository already existed and was reused."
                            });
                        }
                    }
                    catch (Exception lookupEx)
                    {
                        Logger.Warn("Repository exists but lookup for reuse failed.", lookupEx);
                    }
                }

                Logger.Warn("GitHub repository creation failed.", ex);
                return StatusCode(502, new
                {
                    message = "Failed to create repository via GitHub App.",
                    error = ex.Message
                });
            }
        }

        // ── Commit & Deploy ───────────────────────────────────────────────────

        [HttpPost("commit-generated")]
        public async Task<IActionResult> CommitGenerated([FromBody] CommitGeneratedFilesInput input)
        {
            if (input == null || input.ProjectId <= 0)
                return BadRequest(new { message = "ProjectId is required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
            {
                return BadRequest(new
                {
                    message = "GitHub OAuth token is missing for the current user.",
                    details = "Sign in with GitHub first, then retry commit and push."
                });
            }

            var project = await _projectRepository.FirstOrDefaultAsync(input.ProjectId);
            if (project == null)
                return NotFound(new { message = "Project not found." });

            var owner = input.Owner;
            var repository = input.RepositoryName;

            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repository))
                ResolveOwnerAndRepository(input.RepositoryFullName, ref owner, ref repository);

            if (string.IsNullOrWhiteSpace(owner) && !string.IsNullOrWhiteSpace(repository))
                owner = await _gitHubApiService.GetCurrentUserLoginAsync(userGitHubToken);

            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repository))
            {
                return BadRequest(new
                {
                    message = "Repository owner and name are required.",
                    details = "Provide owner + repositoryName or repositoryFullName in the payload."
                });
            }

            var outputBase = _configuration["CodeGen:OutputPath"] ?? "/tmp/GeneratedApps";
            var projectDir = ResolveExistingProjectDirectory(outputBase, project.Name);
            if (string.IsNullOrWhiteSpace(projectDir))
            {
                return NotFound(new
                {
                    message = "Generated project files were not found on server.",
                    details = $"Checked under {outputBase} for project name '{project.Name}'."
                });
            }

            try
            {
                var commitFiles = BuildCommitFiles(projectDir);
                if (commitFiles.Count == 0)
                    return BadRequest(new { message = "No generated files were found to commit." });

                var result = await _gitHubApiService.CommitFilesToBranchAsync(
                    userGitHubToken,
                    owner,
                    repository,
                    string.IsNullOrWhiteSpace(input.Branch) ? "main" : input.Branch,
                    input.CommitMessage,
                    commitFiles);

                var repositoryFullName = string.Format("{0}/{1}", owner, repository);

                var deploymentDecision = _vercelDeploymentPolicy.Evaluate(
                    input.AutoDeploy,
                    project.Framework,
                    repositoryFullName);

                VercelDeploymentResult deploymentResult = null;

                if (deploymentDecision.ShouldDeploy)
                {
                    var repoId = input.RepoId;
                    if (repoId <= 0)
                    {
                        try
                        {
                            repoId = await _gitHubApiService.GetRepositoryIdAsync(
                                userGitHubToken, owner, repository);
                        }
                        catch (Exception repoLookupEx)
                        {
                            Logger.Warn($"Could not resolve GitHub repoId for {repositoryFullName}. " +
                                        $"Vercel deployment will be skipped. {repoLookupEx.Message}");

                            deploymentResult = new VercelDeploymentResult
                            {
                                Triggered = false,
                                ErrorMessage = $"Could not resolve GitHub repoId: {repoLookupEx.Message}"
                            };
                        }
                    }

                    if (repoId > 0 && deploymentResult == null)
                    {
                        deploymentResult = await _vercelDeploymentService.TriggerDeploymentAsync(
                            repositoryFullName,
                            repoId,
                            result.Branch,
                            project.Name,
                            result.CommitSha);

                        if (deploymentResult == null)
                        {
                            deploymentResult = new VercelDeploymentResult
                            {
                                Triggered = false,
                                ErrorMessage = "Vercel deployment service returned an empty result."
                            };
                        }

                        if (!deploymentResult.Triggered &&
                            !string.IsNullOrWhiteSpace(deploymentResult.ErrorMessage))
                        {
                            Logger.Warn(string.Format(
                                "Vercel deployment not triggered for project {0} ({1}). Reason: {2}",
                                project.Id,
                                repositoryFullName,
                                deploymentResult.ErrorMessage));
                        }

                        var projectRepo = await _projectRepositoryRepository.FirstOrDefaultAsync(
                            r => r.ExternalRepositoryId == repoId.ToString());

                        await StoreDeploymentAsync(project.Id, projectRepo?.Id ?? 0, deploymentResult);
                    }
                }

                return Ok(new
                {
                    committed = true,
                    owner,
                    repository,
                    branch = result.Branch,
                    commitSha = result.CommitSha,
                    committedFiles = commitFiles.Count,
                    deployment = new
                    {
                        attempted = deploymentDecision.ShouldDeploy,
                        triggered = deploymentResult != null && deploymentResult.Triggered,
                        skippedReason = deploymentDecision.ShouldDeploy ? null : deploymentDecision.Reason,
                        deploymentId = deploymentResult?.DeploymentId,
                        url = deploymentResult?.Url,
                        inspectorUrl = deploymentResult?.InspectorUrl,
                        state = deploymentResult?.State,
                        errorMessage = deploymentResult?.ErrorMessage
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Warn("GitHub commit and push failed.", ex);
                return StatusCode(502, new
                {
                    message = "Failed to commit and push generated files to GitHub.",
                    error = ex.Message
                });
            }
        }

        // ── Repository data endpoints ─────────────────────────────────────────

        [HttpGet("commits")]
        public async Task<IActionResult> GetCommits(
            [FromQuery] string owner,
            [FromQuery] string repo,
            [FromQuery] string branch = null,
            [FromQuery] int perPage = 30)
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var commits = await _gitHubApiService.GetCommitsAsync(
                    userGitHubToken, owner, repo, branch, perPage);
                return Ok(new { commits });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch commits.", ex);
                return StatusCode(502, new { message = "Failed to fetch commits.", error = ex.Message });
            }
        }

        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches(
            [FromQuery] string owner,
            [FromQuery] string repo)
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var branches = await _gitHubApiService.GetBranchesAsync(userGitHubToken, owner, repo);
                return Ok(new { branches });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch branches.", ex);
                return StatusCode(502, new { message = "Failed to fetch branches.", error = ex.Message });
            }
        }

        [HttpGet("pull-requests")]
        public async Task<IActionResult> GetPullRequests(
            [FromQuery] string owner,
            [FromQuery] string repo,
            [FromQuery] string state = "open")
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var prs = await _gitHubApiService.GetPullRequestsAsync(userGitHubToken, owner, repo, state);
                return Ok(new { pullRequests = prs });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch pull requests.", ex);
                return StatusCode(502, new { message = "Failed to fetch pull requests.", error = ex.Message });
            }
        }

        [HttpGet("issues")]
        public async Task<IActionResult> GetIssues(
            [FromQuery] string owner,
            [FromQuery] string repo,
            [FromQuery] string state = "open")
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var issues = await _gitHubApiService.GetIssuesAsync(userGitHubToken, owner, repo, state);
                return Ok(new { issues });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch issues.", ex);
                return StatusCode(502, new { message = "Failed to fetch issues.", error = ex.Message });
            }
        }

        [HttpGet("releases")]
        public async Task<IActionResult> GetReleases(
            [FromQuery] string owner,
            [FromQuery] string repo)
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var releases = await _gitHubApiService.GetReleasesAsync(userGitHubToken, owner, repo);
                return Ok(new { releases });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch releases.", ex);
                return StatusCode(502, new { message = "Failed to fetch releases.", error = ex.Message });
            }
        }

        [HttpGet("actions")]
        public async Task<IActionResult> GetWorkflowRuns(
            [FromQuery] string owner,
            [FromQuery] string repo,
            [FromQuery] int perPage = 10)
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var runs = await _gitHubApiService.GetWorkflowRunsAsync(userGitHubToken, owner, repo, perPage);
                return Ok(new { runs });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch workflow runs.", ex);
                return StatusCode(502, new { message = "Failed to fetch workflow runs.", error = ex.Message });
            }
        }

        [HttpGet("contents")]
        public async Task<IActionResult> GetContents(
            [FromQuery] string owner,
            [FromQuery] string repo,
            [FromQuery] string path = "")
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var contents = await _gitHubApiService.GetContentsAsync(userGitHubToken, owner, repo, path);
                return Ok(new { contents });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch contents.", ex);
                return StatusCode(502, new { message = "Failed to fetch contents.", error = ex.Message });
            }
        }

        [HttpGet("file")]
        public async Task<IActionResult> GetFileContent(
            [FromQuery] string owner,
            [FromQuery] string repo,
            [FromQuery] string path)
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo) || string.IsNullOrWhiteSpace(path))
                return BadRequest(new { message = "owner, repo and path are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var file = await _gitHubApiService.GetFileContentAsync(userGitHubToken, owner, repo, path);
                return Ok(new { file });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch file content.", ex);
                return StatusCode(502, new { message = "Failed to fetch file.", error = ex.Message });
            }
        }

        [HttpGet("stats/languages")]
        public async Task<IActionResult> GetLanguages(
            [FromQuery] string owner,
            [FromQuery] string repo)
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repo))
                return BadRequest(new { message = "owner and repo are required." });

            var userGitHubToken = await GetCurrentUserGitHubAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(userGitHubToken))
                return BadRequest(new { message = "GitHub OAuth token is missing." });

            try
            {
                var languages = await _gitHubApiService.GetLanguagesAsync(userGitHubToken, owner, repo);
                return Ok(new { languages });
            }
            catch (Exception ex)
            {
                Logger.Warn("Failed to fetch languages.", ex);
                return StatusCode(502, new { message = "Failed to fetch languages.", error = ex.Message });
            }
        }

        // ── Persistence helpers ───────────────────────────────────────────────

        private async Task StoreRepositoryAsync(
            long projectId,
            string owner,
            string name,
            string fullName,
            string htmlUrl,
            string externalRepositoryId,
            bool isPrivate)
        {
            try
            {
                await _projectRepositoryRepository.InsertAsync(new ProjectRepository
                {
                    ProjectId = projectId,
                    Provider = GitProvider.GitHub,
                    Owner = owner,
                    Name = name,
                    FullName = fullName,
                    DefaultBranch = "main",
                    Visibility = isPrivate ? RepositoryVisibility.Private : RepositoryVisibility.Public,
                    HtmlUrl = htmlUrl,
                    ExternalRepositoryId = externalRepositoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                await CurrentUnitOfWork.SaveChangesAsync();
                Logger.Info($"Stored ProjectRepository for {fullName}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to store ProjectRepository for {fullName}: {ex.Message}", ex);
            }
        }

        private async Task StoreDeploymentAsync(
            long projectId,
            long projectRepositoryId,
            VercelDeploymentResult deploymentResult)
        {
            try
            {
                await _deploymentRepository.InsertAsync(new Deployment
                {
                    ProjectId = projectId,
                    ProjectRepositoryId = projectRepositoryId,
                    Target = DeploymentTarget.Vercel,
                    EnvironmentName = "production",
                    Status = deploymentResult.Triggered
                        ? DeploymentStatus.InProgress
                        : DeploymentStatus.Failed,
                    Url = deploymentResult.Url,
                    ProviderDeploymentId = deploymentResult.DeploymentId,
                    ErrorMessage = deploymentResult.ErrorMessage,
                    TriggeredAt = DateTime.UtcNow
                });

                await CurrentUnitOfWork.SaveChangesAsync();
                Logger.Info($"Stored Deployment record for project {projectId}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to store Deployment for project {projectId}: {ex.Message}", ex);
            }
        }

        // ── Other helpers ─────────────────────────────────────────────────────

        private static void ResolveOwnerAndRepository(
            string repositoryFullName, ref string owner, ref string repository)
        {
            if (string.IsNullOrWhiteSpace(repositoryFullName)) return;

            var parts = repositoryFullName.Split(
                new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                owner = string.IsNullOrWhiteSpace(owner) ? parts[0] : owner;
                repository = string.IsNullOrWhiteSpace(repository) ? parts[1] : repository;
            }
        }

        private static List<GitHubApiService.GitHubCommitFile> BuildCommitFiles(string projectDir)
        {
            var files = new List<GitHubApiService.GitHubCommitFile>();
            var absoluteFiles = Directory.GetFiles(projectDir, "*", SearchOption.AllDirectories);

            foreach (var filePath in absoluteFiles)
            {
                if (!IsCommitEligibleFile(projectDir, filePath)) continue;

                var relativePath = filePath
                    .Substring(projectDir.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (string.IsNullOrWhiteSpace(relativePath)) continue;

                var normalizedPath = relativePath
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/');

                var bytes = System.IO.File.ReadAllBytes(filePath);
                files.Add(new GitHubApiService.GitHubCommitFile
                {
                    Path = normalizedPath,
                    ContentBase64 = Convert.ToBase64String(bytes)
                });
            }

            return files;
        }

        private static bool IsCommitEligibleFile(string projectDir, string filePath)
        {
            if (string.IsNullOrWhiteSpace(projectDir) ||
                string.IsNullOrWhiteSpace(filePath)) return false;

            var relativePath = filePath
                .Substring(projectDir.Length)
                .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (string.IsNullOrWhiteSpace(relativePath)) return false;

            var normalizedPath = relativePath
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');

            var pathSegments = normalizedPath.Split(
                new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < pathSegments.Length - 1; i++)
            {
                foreach (var ignored in IgnoredCommitDirectories)
                {
                    if (string.Equals(pathSegments[i], ignored, StringComparison.OrdinalIgnoreCase))
                        return false;
                }
            }

            var fileName = Path.GetFileName(filePath);
            foreach (var ignored in IgnoredCommitFileNames)
            {
                if (string.Equals(fileName, ignored, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists && fileInfo.Length > 0 && fileInfo.Length <= MaxCommitFileSizeBytes;
        }

        private static string ResolveExistingProjectDirectory(string outputBase, string projectName)
        {
            if (string.IsNullOrWhiteSpace(outputBase) || string.IsNullOrWhiteSpace(projectName))
                return null;

            var candidates = new List<string>
            {
                Path.Combine(outputBase, projectName.Trim()),
                Path.Combine(outputBase, SanitizeDirName(projectName))
            };

            foreach (var candidate in candidates)
            {
                if (Directory.Exists(candidate)) return candidate;
            }

            return null;
        }

        private static string SanitizeDirName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "unnamed-project";
            return Regex.Replace(name.Trim(), @"[^a-zA-Z0-9\-_]", "-").ToLowerInvariant();
        }

        private async Task<string> GetCurrentUserGitHubAccessTokenAsync()
        {
            if (!AbpSession.UserId.HasValue) return null;
            var user = await _userRepository.FirstOrDefaultAsync(AbpSession.UserId.Value);
            return user?.GitHubAccessToken;
        }
    }
}