using ABPGroup.Authentication.External.GitHub;
using System.Threading.Tasks;

namespace ABPGroup.Authentication.External
{
    public class GitHubAuthProvider : ExternalAuthProviderApiBase
    {
        private readonly GitHubApiService _gitHubApiService;

        public GitHubAuthProvider(GitHubApiService gitHubApiService)
        {
            _gitHubApiService = gitHubApiService;
        }

        public override async Task<ExternalAuthUserInfo> GetUserInfo(string accessCode)
        {
            var userInfo = await _gitHubApiService.GetUserInfoAsync(accessCode);
            if (userInfo == null)
                return null;

            var nameParts = (userInfo.Name ?? userInfo.Login).Split(' ', 2);

            return new ExternalAuthUserInfo
            {
                Provider = "GitHub",
                ProviderKey = userInfo.Id.ToString(),
                Name = nameParts[0],
                Surname = nameParts.Length > 1 ? nameParts[1] : nameParts[0],
                EmailAddress = userInfo.Email ?? $"{userInfo.Login}@users.noreply.github.com"
            };
        }
    }
}
