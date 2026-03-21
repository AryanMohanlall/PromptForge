using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using ABPGroup.Authorization.Users;
using ABPGroup.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ABPGroup.Authentication.External.GitHub
{
    public class GitHubUserService : IGitHubUserService, ITransientDependency
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IUserCreationService _userCreationService;

        public GitHubUserService(
            IRepository<User, long> userRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IUserCreationService userCreationService)
        {
            _userRepository = userRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _userCreationService = userCreationService;
        }

        public async Task<User> GetOrCreateAsync(GitHubUserInfo githubUser, string githubAccessToken, int tenantId)
        {
            using (var uow = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
            {
                var githubIdString = githubUser.Id.ToString();
                var user = await _userRepository.FirstOrDefaultAsync(
                    u => u.GitHubUsername == githubIdString);

                if (user == null && !string.IsNullOrEmpty(githubUser.Email))
                {
                    var normalizedEmail = githubUser.Email.ToUpperInvariant();
                    user = await _userRepository.FirstOrDefaultAsync(
                        u => u.NormalizedEmailAddress == normalizedEmail);
                }

                if (user == null)
                {
                    var nameParts = (githubUser.Name ?? githubUser.Login).Split(new[] { ' ' }, 2);
                    var userName = await EnsureUniqueUserNameAsync(githubUser.Login);

                    user = new User
                    {
                        TenantId = tenantId,
                        UserName = userName,
                        Name = nameParts[0],
                        Surname = nameParts.Length > 1 ? nameParts[1] : nameParts[0],
                        EmailAddress = githubUser.Email ?? string.Format("{0}@users.noreply.github.com", githubUser.Login),
                        IsEmailConfirmed = !string.IsNullOrEmpty(githubUser.Email),
                        IsActive = true,
                        GitHubUsername = githubIdString,
                        GitHubAccessToken = githubAccessToken,
                        AvatarUrl = githubUser.AvatarUrl
                    };
                    user.SetNormalizedNames();

                    var result = await _userCreationService.CreateAsync(user, User.CreateRandomPassword());
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception(string.Format("Failed to create user: {0}", errors));
                    }
                }
                else
                {
                    user.GitHubUsername = githubIdString;
                    user.GitHubAccessToken = githubAccessToken;
                    if (!string.IsNullOrEmpty(githubUser.AvatarUrl))
                        user.AvatarUrl = githubUser.AvatarUrl;

                    await _userRepository.UpdateAsync(user);
                }

                await uow.CompleteAsync();
                return user;
            }
        }

        private async Task<string> EnsureUniqueUserNameAsync(string desiredUserName)
        {
            var candidate = desiredUserName;
            var suffix = 1;

            while (await _userRepository.FirstOrDefaultAsync(u => u.UserName == candidate) != null)
            {
                candidate = string.Format("{0}{1}", desiredUserName, suffix++);
            }

            return candidate;
        }
    }
}
