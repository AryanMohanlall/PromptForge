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

    public class GitHubApiService : ITransientDependency
    {
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
            {
                email = await FetchPrimaryEmailAsync(client);
            }

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
            {
                throw new Exception("GitHub App metadata response was empty.");
            }

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
            {
                throw new Exception("GitHub installation token response did not include a token.");
            }

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
            {
                throw new Exception("GitHub installation lookup returned empty payload.");
            }

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
            {
                throw new ArgumentException("Repository name is required.");
            }

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
            {
                throw new Exception("GitHub repository creation response was empty.");
            }

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
            {
                throw new ArgumentException("Repository name is required.");
            }

            if (string.IsNullOrWhiteSpace(userAccessToken))
            {
                throw new ArgumentException("User GitHub access token is required.");
            }

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
            {
                throw new Exception("GitHub repository creation response was empty.");
            }

            return repository;
        }

        private static string CreateGitHubAppJwt(string appId, string privateKeyPem)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentException("GitHub AppId is required.");
            }

            if (string.IsNullOrWhiteSpace(privateKeyPem))
            {
                throw new ArgumentException("GitHub App private key is required.");
            }

            var normalizedPem = privateKeyPem.Replace("\\n", "\n").Trim();
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(normalizedPem.ToCharArray());

                var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
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
        }

        private async Task<string> FetchPrimaryEmailAsync(HttpClient client)
        {
            var emails = await client.GetFromJsonAsync<GitHubEmailEntry[]>("https://api.github.com/user/emails");
            if (emails == null)
                return null;

            foreach (var entry in emails)
            {
                if (entry.Primary && entry.Verified)
                    return entry.Email;
            }

            return null;
        }

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
