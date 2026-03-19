using ABPGroup.Authorization.Users;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ABPGroup.Identity
{
    public interface IUserCreationService
    {
        Task<IdentityResult> CreateAsync(User user, string password);
    }
}
