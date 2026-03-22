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


        Task<List<object>> GetPullRequestsAsync(string userAccessToken, string owner, string repo, string state = "open");
        Task<List<object>> GetIssuesAsync(string userAccessToken, string owner, string repo, string state = "open");
        Task<List<object>> GetReleasesAsync(string userAccessToken, string owner, string repo);
        Task<List<object>> GetWorkflowRunsAsync(string userAccessToken, string owner, string repo, int perPage = 10);
        Task<List<object>> GetContentsAsync(string userAccessToken, string owner, string repo, string path = "");
        Task<object> GetFileContentAsync(string userAccessToken, string owner, string repo, string path);
        Task<Dictionary<string, long>> GetLanguagesAsync(string userAccessToken, string owner, string repo);
    }
}
