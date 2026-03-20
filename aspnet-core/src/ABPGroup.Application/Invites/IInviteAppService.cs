using System.Threading.Tasks;
using Abp.Application.Services;
using ABPGroup.Invites.Dto;

namespace ABPGroup.Invites
{
    public interface IInviteAppService : IApplicationService
    {
        Task InviteUserAsync(InviteUserInput input);
    }
}