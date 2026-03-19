using Abp.Application.Services;
using ABPGroup.Invitations.Dto;
using System.Threading.Tasks;

namespace ABPGroup.Invitations;

public interface IInvitationAppService : IApplicationService
{
    Task SendInvitation(SendInvitationInput input);
}
