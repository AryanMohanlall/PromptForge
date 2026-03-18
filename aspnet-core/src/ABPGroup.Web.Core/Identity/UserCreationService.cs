using Abp.Dependency;
using ABPGroup.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ABPGroup.Identity
{
    public class UserCreationService : IUserCreationService, ITransientDependency
    {
        private readonly UserManager _userManager;

        public UserCreationService(UserManager userManager)
        {
            _userManager = userManager;
        }

        public Task<IdentityResult> CreateAsync(User user, string password)
        {
            return _userManager.CreateAsync(user, password);
        }
    }
}
