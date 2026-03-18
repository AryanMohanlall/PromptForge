using System.Threading.Tasks;

namespace ABPGroup.Authentication.External.GitHub
{
    public interface IGitHubApiService
    {
        Task<string> ExchangeCodeForAccessTokenAsync(string code, string clientId, string clientSecret, string redirectUri);
        Task<GitHubUserInfo> GetUserInfoAsync(string accessToken);
    }
}
