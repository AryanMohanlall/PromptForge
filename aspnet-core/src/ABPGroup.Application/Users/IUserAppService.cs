using Abp.Application.Services;
using Abp.Application.Services.Dto;
using ABPGroup.Roles.Dto;
using ABPGroup.Users.Dto;
using System.Threading.Tasks;

namespace ABPGroup.Users;

public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
{
    Task DeActivate(EntityDto<long> user);
    Task Activate(EntityDto<long> user);
    Task<ListResultDto<RoleDto>> GetRoles();
    Task ChangeLanguage(ChangeUserLanguageDto input);

    Task<bool> ChangePassword(ChangePasswordDto input);
    Task<bool> ResetPassword(ResetPasswordDto input);
}
