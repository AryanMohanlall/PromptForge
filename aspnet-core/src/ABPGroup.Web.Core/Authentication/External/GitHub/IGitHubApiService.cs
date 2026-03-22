using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABPGroup.Authentication.External.GitHub
{
    public interface IGitHubApiService
    {
        Task<string> ExchangeCodeForAccessTokenAsync(string code, string clientId, string clientSecret, string redirectUri);
        Task<GitHubUserInfo> GetUserInfoAsync(string accessToken);
        Task<List<object>> GetCommitsAsync(string userAccessToken, string owner, string repo, string branch = null, int perPage = 30);
        Task<List<object>> GetBranchesAsync(string userAccessToken, string owner, string repo);

        Task<List<object>> GetUserRepositoriesAsync(string userAccessToken);
    }
}
