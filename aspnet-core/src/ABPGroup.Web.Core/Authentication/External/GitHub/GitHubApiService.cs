using Abp.Dependency;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ABPGroup.Authentication.External.GitHub
{
    public class GitHubUserInfo
    {
        public long Id { get; set; }
        public string Login { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class GitHubApiService : IGitHubApiService, ITransientDependency
    {
        public class GitHubCommitFile
        {
            public string Path { get; set; }
            public string ContentBase64 { get; set; }
        }

        public class GitHubCommitPushResult
        {
            public string Branch { get; set; }
            public string CommitSha { get; set; }
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private const string GitHubApiVersion = "2022-11-28";

        public GitHubApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> ExchangeCodeForAccessTokenAsync(
            string code, string clientId, string clientSecret, string redirectUri)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code,
                ["redirect_uri"] = redirectUri
            });

            var response = await client.PostAsync("https://github.com/login/oauth/access_token", content);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>();
            return json?.AccessToken;
        }

        public async Task<GitHubUserInfo> GetUserInfoAsync(string accessToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            client.DefaultRequestHeaders.Add("User-Agent", "PromptForge");

            var user = await client.GetFromJsonAsync<GitHubApiUserResponse>("https://api.github.com/user");
            if (user == null)
                return null;

            var email = user.Email;
            if (string.IsNullOrWhiteSpace(email))
                email = await FetchPrimaryEmailAsync(client);

            return new GitHubUserInfo
            {
                Id = user.Id,
                Login = user.Login,
                Name = user.Name,
                Email = email,
                AvatarUrl = user.AvatarUrl
            };
        }

        public async Task<GitHubAppInfo> GetGitHubAppInfoAsync(string appId, string privateKeyPem)
        {
            var jwt = CreateGitHubAppJwt(appId, privateKeyPem);
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/app");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PromptForge", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", GitHubApiVersion);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub App metadata request failed: {(int)response.StatusCode} {errorBody}");
            }

            var appInfo = await response.Content.ReadFromJsonAsync<GitHubAppInfo>();
            if (appInfo == null)
                throw new Exception("GitHub App metadata response was empty.");

            return appInfo;
        }

        public async Task<string> CreateInstallationTokenAsync(string appId, string installationId, string privateKeyPem)
        {
            var jwt = CreateGitHubAppJwt(appId, privateKeyPem);
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://api.github.com/app/installations/{installationId}/access_tokens");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PromptForge", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", GitHubApiVersion);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub installation token request failed: {(int)response.StatusCode} {errorBody}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GitHubInstallationTokenResponse>();
            if (string.IsNullOrWhiteSpace(tokenResponse?.Token))
                throw new Exception("GitHub installation token response did not include a token.");

            return tokenResponse.Token;
        }

        public async Task<GitHubInstallationInfo> GetInstallationInfoAsync(string appId, string installationId, string privateKeyPem)
        {
            var jwt = CreateGitHubAppJwt(appId, privateKeyPem);
            var client = _httpClientFactory.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.github.com/app/installations/{installationId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PromptForge", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", GitHubApiVersion);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub installation lookup failed: {(int)response.StatusCode} {errorBody}");
            }

            var installation = await response.Content.ReadFromJsonAsync<GitHubInstallationInfo>();
            if (installation == null)
                throw new Exception("GitHub installation lookup returned empty payload.");

            return installation;
        }

        public async Task<List<GitHubRepositoryInfo>> GetInstallationRepositoriesAsync(string installationToken)
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/installation/repositories");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", installationToken);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PromptForge", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", GitHubApiVersion);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub installation repositories request failed: {(int)response.StatusCode} {errorBody}");
            }

            var repositoriesResponse = await response.Content.ReadFromJsonAsync<GitHubInstallationRepositoriesResponse>();
            return repositoriesResponse?.Repositories ?? new List<GitHubRepositoryInfo>();
        }

        public async Task<GitHubRepositoryInfo> CreateRepositoryAsync(
            string installationToken,
            string repositoryName,
            bool isPrivate,
            string description,
            bool autoInit,
            string owner)
        {
            if (string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("Repository name is required.");

            var client = _httpClientFactory.CreateClient();
            var route = string.IsNullOrWhiteSpace(owner)
                ? "https://api.github.com/user/repos"
                : $"https://api.github.com/orgs/{owner}/repos";

            var request = new HttpRequestMessage(HttpMethod.Post, route);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", installationToken);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PromptForge", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", GitHubApiVersion);
            request.Content = JsonContent.Create(new
            {
                name = repositoryName,
                description,
                @private = isPrivate,
                auto_init = autoInit
            });

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub repository creation failed: {(int)response.StatusCode} {errorBody}");
            }

            var repository = await response.Content.ReadFromJsonAsync<GitHubRepositoryInfo>();
            if (repository == null)
                throw new Exception("GitHub repository creation response was empty.");

            return repository;
        }

        public async Task<GitHubRepositoryInfo> CreateRepositoryWithUserTokenAsync(
            string userAccessToken,
            string repositoryName,
            bool isPrivate,
            string description,
            bool autoInit,
            string owner)
        {
            if (string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("Repository name is required.");
            if (string.IsNullOrWhiteSpace(userAccessToken))
                throw new ArgumentException("User GitHub access token is required.");

            var client = _httpClientFactory.CreateClient();
            var route = string.IsNullOrWhiteSpace(owner)
                ? "https://api.github.com/user/repos"
                : $"https://api.github.com/orgs/{owner}/repos";

            var request = new HttpRequestMessage(HttpMethod.Post, route);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userAccessToken);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PromptForge", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", GitHubApiVersion);
            request.Content = JsonContent.Create(new
            {
                name = repositoryName,
                description,
                @private = isPrivate,
                auto_init = autoInit
            });

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub repository creation with user token failed: {(int)response.StatusCode} {errorBody}");
            }

            var repository = await response.Content.ReadFromJsonAsync<GitHubRepositoryInfo>();
            if (repository == null)
                throw new Exception("GitHub repository creation response was empty.");

            return repository;
        }

        public async Task<string> GetCurrentUserLoginAsync(string userAccessToken)
        {
            if (string.IsNullOrWhiteSpace(userAccessToken))
                throw new ArgumentException("User GitHub access token is required.");

            var client = _httpClientFactory.CreateClient();
            var request = BuildGitHubRequest(HttpMethod.Get, "https://api.github.com/user", userAccessToken);
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub user lookup failed: {(int)response.StatusCode} {errorBody}");
            }

            var user = await response.Content.ReadFromJsonAsync<GitHubApiUserResponse>();
            if (string.IsNullOrWhiteSpace(user?.Login))
                throw new Exception("GitHub user lookup did not return login.");

            return user.Login;
        }

        public async Task<GitHubRepositoryInfo> GetRepositoryWithUserTokenAsync(
            string userAccessToken, string owner, string repositoryName)
        {
            if (string.IsNullOrWhiteSpace(userAccessToken))
                throw new ArgumentException("User GitHub access token is required.");
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("Repository owner and name are required.");

            var client = _httpClientFactory.CreateClient();
            var request = BuildGitHubRequest(
                HttpMethod.Get,
                $"https://api.github.com/repos/{owner}/{repositoryName}",
                userAccessToken);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub repository lookup failed: {(int)response.StatusCode} {errorBody}");
            }

            var repository = await response.Content.ReadFromJsonAsync<GitHubRepositoryInfo>();
            if (repository == null)
                throw new Exception("GitHub repository lookup response was empty.");

            return repository;
        }

        /// <summary>
        /// Returns the numeric GitHub repository ID required by the Vercel API (gitSource.repoId).
        /// Calls GET /repos/{owner}/{repo} and returns the "id" field.
        /// </summary>
        public async Task<long> GetRepositoryIdAsync(string userAccessToken, string owner, string repositoryName)
        {
            var repository = await GetRepositoryWithUserTokenAsync(userAccessToken, owner, repositoryName);

            if (repository.Id <= 0)
                throw new Exception($"GitHub repository lookup returned an invalid id for {owner}/{repositoryName}.");

            return repository.Id;
        }

        public async Task<GitHubCommitPushResult> CommitFilesToBranchAsync(
            string userAccessToken,
            string owner,
            string repositoryName,
            string branch,
            string commitMessage,
            List<GitHubCommitFile> files)
        {
            if (string.IsNullOrWhiteSpace(userAccessToken))
                throw new ArgumentException("User GitHub access token is required.");
            if (string.IsNullOrWhiteSpace(owner))
                throw new ArgumentException("Repository owner is required.");
            if (string.IsNullOrWhiteSpace(repositoryName))
                throw new ArgumentException("Repository name is required.");
            if (string.IsNullOrWhiteSpace(branch))
                branch = "main";
            if (files == null || files.Count == 0)
                throw new ArgumentException("At least one file is required for commit.");

            var client = _httpClientFactory.CreateClient();

            var resolvedBranch = await EnsureBranchExistsAsync(client, userAccessToken, owner, repositoryName, branch);
            var headRef = await GetRefAsync(client, userAccessToken, owner, repositoryName, resolvedBranch);
            var headCommitSha = headRef.Object.Sha;

            var commitResponse = await GetCommitAsync(client, userAccessToken, owner, repositoryName, headCommitSha);
            var baseTreeSha = commitResponse.Tree.Sha;

            var treeEntries = new List<object>();
            foreach (var file in files)
            {
                var blobSha = await CreateBlobAsync(client, userAccessToken, owner, repositoryName, file.ContentBase64);
                treeEntries.Add(new
                {
                    path = file.Path,
                    mode = "100644",
                    type = "blob",
                    sha = blobSha
                });
            }

            var newTreeSha = await CreateTreeAsync(client, userAccessToken, owner, repositoryName, baseTreeSha, treeEntries);
            var newCommitSha = await CreateCommitAsync(
                client, userAccessToken, owner, repositoryName,
                string.IsNullOrWhiteSpace(commitMessage) ? "chore: add generated project files" : commitMessage,
                newTreeSha, headCommitSha);

            await UpdateRefAsync(client, userAccessToken, owner, repositoryName, resolvedBranch, newCommitSha);

            return new GitHubCommitPushResult
            {
                Branch = resolvedBranch,
                CommitSha = newCommitSha
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task<string> EnsureBranchExistsAsync(
            HttpClient client, string token, string owner, string repo, string branch)
        {
            var escapedBranch = Uri.EscapeDataString(branch);
            var existingBranchResponse = await client.SendAsync(BuildGitHubRequest(
                HttpMethod.Get,
                $"https://api.github.com/repos/{owner}/{repo}/git/ref/heads/{escapedBranch}",
                token));

            if (existingBranchResponse.IsSuccessStatusCode)
                return branch;

            if (existingBranchResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var errorBody = await existingBranchResponse.Content.ReadAsStringAsync();
                throw new Exception($"GitHub branch lookup failed: {(int)existingBranchResponse.StatusCode} {errorBody}");
            }

            var repoResponse = await client.SendAsync(BuildGitHubRequest(
                HttpMethod.Get, $"https://api.github.com/repos/{owner}/{repo}", token));

            if (!repoResponse.IsSuccessStatusCode)
            {
                var repoError = await repoResponse.Content.ReadAsStringAsync();
                throw new Exception($"GitHub repository lookup failed: {(int)repoResponse.StatusCode} {repoError}");
            }

            var repoInfo = await repoResponse.Content.ReadFromJsonAsync<GitHubRepositoryDetails>();
            var defaultBranch = repoInfo?.DefaultBranch;
            if (string.IsNullOrWhiteSpace(defaultBranch))
                throw new Exception("GitHub repository did not return a default branch.");

            var defaultRef = await GetRefAsync(client, token, owner, repo, defaultBranch);
            var createRefRequest = BuildGitHubRequest(
                HttpMethod.Post,
                $"https://api.github.com/repos/{owner}/{repo}/git/refs",
                token);
            createRefRequest.Content = JsonContent.Create(new
            {
                @ref = $"refs/heads/{branch}",
                sha = defaultRef.Object.Sha
            });

            var createRefResponse = await client.SendAsync(createRefRequest);
            if (!createRefResponse.IsSuccessStatusCode)
            {
                var createRefError = await createRefResponse.Content.ReadAsStringAsync();
                throw new Exception($"GitHub branch creation failed: {(int)createRefResponse.StatusCode} {createRefError}");
            }

            return branch;
        }

        private async Task<GitHubRefResponse> GetRefAsync(
            HttpClient client, string token, string owner, string repo, string branch)
        {
            var escapedBranch = Uri.EscapeDataString(branch);
            var response = await client.SendAsync(BuildGitHubRequest(
                HttpMethod.Get,
                $"https://api.github.com/repos/{owner}/{repo}/git/ref/heads/{escapedBranch}",
                token));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub ref lookup failed: {(int)response.StatusCode} {errorBody}");
            }

            var gitRef = await response.Content.ReadFromJsonAsync<GitHubRefResponse>();
            if (gitRef?.Object?.Sha == null)
                throw new Exception("GitHub ref response did not include a commit sha.");

            return gitRef;
        }

        private async Task<GitHubCommitResponse> GetCommitAsync(
            HttpClient client, string token, string owner, string repo, string commitSha)
        {
            var response = await client.SendAsync(BuildGitHubRequest(
                HttpMethod.Get,
                $"https://api.github.com/repos/{owner}/{repo}/git/commits/{commitSha}",
                token));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub commit lookup failed: {(int)response.StatusCode} {errorBody}");
            }

            var commit = await response.Content.ReadFromJsonAsync<GitHubCommitResponse>();
            if (commit?.Tree?.Sha == null)
                throw new Exception("GitHub commit response did not include a tree sha.");

            return commit;
        }

        private async Task<string> CreateBlobAsync(
            HttpClient client, string token, string owner, string repo, string base64Content)
        {
            var request = BuildGitHubRequest(
                HttpMethod.Post,
                $"https://api.github.com/repos/{owner}/{repo}/git/blobs",
                token);
            request.Content = JsonContent.Create(new
            {
                content = base64Content,
                encoding = "base64"
            });

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub blob creation failed: {(int)response.StatusCode} {errorBody}");
            }

            var blob = await response.Content.ReadFromJsonAsync<GitHubObjectInfo>();
            if (blob == null || string.IsNullOrWhiteSpace(blob.Sha))
                throw new Exception("GitHub blob creation response did not include sha.");

            return blob.Sha;
        }

        private async Task<string> CreateTreeAsync(
            HttpClient client, string token, string owner, string repo,
            string baseTreeSha, List<object> treeEntries)
        {
            var request = BuildGitHubRequest(
                HttpMethod.Post,
                $"https://api.github.com/repos/{owner}/{repo}/git/trees",
                token);
            request.Content = JsonContent.Create(new
            {
                base_tree = baseTreeSha,
                tree = treeEntries
            });

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub tree creation failed: {(int)response.StatusCode} {errorBody}");
            }

            var tree = await response.Content.ReadFromJsonAsync<GitHubTreeResponse>();
            if (tree == null || string.IsNullOrWhiteSpace(tree.Sha))
                throw new Exception("GitHub tree creation response did not include sha.");

            return tree.Sha;
        }

        private async Task<string> CreateCommitAsync(
            HttpClient client, string token, string owner, string repo,
            string message, string treeSha, string parentSha)
        {
            var request = BuildGitHubRequest(
                HttpMethod.Post,
                $"https://api.github.com/repos/{owner}/{repo}/git/commits",
                token);
            request.Content = JsonContent.Create(new
            {
                message,
                tree = treeSha,
                parents = new[] { parentSha }
            });

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub commit creation failed: {(int)response.StatusCode} {errorBody}");
            }

            var commit = await response.Content.ReadFromJsonAsync<GitHubObjectInfo>();
            if (commit == null || string.IsNullOrWhiteSpace(commit.Sha))
                throw new Exception("GitHub commit response did not include sha.");

            return commit.Sha;
        }

        private async Task UpdateRefAsync(
            HttpClient client, string token, string owner, string repo,
            string branch, string commitSha)
        {
            var escapedBranch = Uri.EscapeDataString(branch);
            var request = BuildGitHubRequest(
                HttpMethod.Patch,
                $"https://api.github.com/repos/{owner}/{repo}/git/refs/heads/{escapedBranch}",
                token);
            request.Content = JsonContent.Create(new { sha = commitSha, force = false });

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"GitHub ref update failed: {(int)response.StatusCode} {errorBody}");
            }
        }

        private HttpRequestMessage BuildGitHubRequest(HttpMethod method, string url, string token)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("PromptForge", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-GitHub-Api-Version", GitHubApiVersion);
            return request;
        }

        private static string CreateGitHubAppJwt(string appId, string privateKeyPem)
        {
            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException("GitHub AppId is required.");
            if (string.IsNullOrWhiteSpace(privateKeyPem))
                throw new ArgumentException("GitHub App private key is required.");

            var normalizedPem = privateKeyPem.Replace("\\n", "\n").Trim();
            RSAParameters rsaParameters;
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(normalizedPem.ToCharArray());
                rsaParameters = rsa.ExportParameters(true);
            }

            var credentials = new SigningCredentials(
                new RsaSecurityKey(rsaParameters), SecurityAlgorithms.RsaSha256);
            var now = DateTimeOffset.UtcNow;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, now.AddMinutes(9).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Iss, appId)
            };

            var token = new JwtSecurityToken(claims: claims, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> FetchPrimaryEmailAsync(HttpClient client)
        {
            var emails = await client.GetFromJsonAsync<GitHubEmailEntry[]>("https://api.github.com/user/emails");
            if (emails == null) return null;

            foreach (var entry in emails)
            {
                if (entry.Primary && entry.Verified)
                    return entry.Email;
            }

            return null;
        }

        // ── Response models ───────────────────────────────────────────────────

        private class GitHubTokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }
        }

        public class GitHubAppInfo
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("slug")]
            public string Slug { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        private class GitHubInstallationTokenResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; }
        }

        public class GitHubInstallationInfo
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("account")]
            public GitHubInstallationAccount Account { get; set; }
        }

        public class GitHubInstallationAccount
        {
            [JsonPropertyName("login")]
            public string Login { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        private class GitHubInstallationRepositoriesResponse
        {
            [JsonPropertyName("repositories")]
            public List<GitHubRepositoryInfo> Repositories { get; set; }
        }

        public class GitHubRepositoryInfo
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("full_name")]
            public string FullName { get; set; }

            [JsonPropertyName("private")]
            public bool Private { get; set; }

            [JsonPropertyName("html_url")]
            public string HtmlUrl { get; set; }
        }

        private class GitHubRepositoryDetails
        {
            [JsonPropertyName("default_branch")]
            public string DefaultBranch { get; set; }
        }

        private class GitHubRefResponse
        {
            [JsonPropertyName("ref")]
            public string Ref { get; set; }

            [JsonPropertyName("object")]
            public GitHubObjectInfo Object { get; set; }
        }

        private class GitHubCommitResponse
        {
            [JsonPropertyName("sha")]
            public string Sha { get; set; }

            [JsonPropertyName("tree")]
            public GitHubObjectInfo Tree { get; set; }
        }

        private class GitHubTreeResponse
        {
            [JsonPropertyName("sha")]
            public string Sha { get; set; }
        }

        private class GitHubObjectInfo
        {
            [JsonPropertyName("sha")]
            public string Sha { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        private class GitHubApiUserResponse
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("login")]
            public string Login { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("avatar_url")]
            public string AvatarUrl { get; set; }
        }

        private class GitHubEmailEntry
        {
            [JsonPropertyName("email")]
            public string Email { get; set; }

            [JsonPropertyName("primary")]
            public bool Primary { get; set; }

            [JsonPropertyName("verified")]
            public bool Verified { get; set; }
        }
    }
}