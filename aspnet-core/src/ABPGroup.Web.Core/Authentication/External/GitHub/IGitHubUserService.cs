using ABPGroup.Authorization.Users;
using System.Threading.Tasks;

namespace ABPGroup.Authentication.External.GitHub
{
    public interface IGitHubUserService
    {
        Task<User> GetOrCreateAsync(GitHubUserInfo userInfo, string accessToken, int tenantId);
    }
}
