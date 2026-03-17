using Abp.Dependency;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
