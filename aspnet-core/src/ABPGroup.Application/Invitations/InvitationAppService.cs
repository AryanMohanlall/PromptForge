using Abp.Authorization;
using Abp.Net.Mail;
using ABPGroup.Invitations.Dto;
using System;
using System.Text;
using System.Threading.Tasks;
using Abp.Runtime.Session;

namespace ABPGroup.Invitations;

[AbpAuthorize]
public class InvitationAppService : ABPGroupAppServiceBase, IInvitationAppService
{
    private readonly IEmailSender _emailSender;

    public InvitationAppService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendInvitation(SendInvitationInput input)
    {
        var tenantId = AbpSession.GetTenantId();

        // Create JSON: {"tenantId": 1, "role": 2}
        var json = $"{{\"tenantId\":{tenantId},\"role\":{(int)input.Role}}}";
        var base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        // In a real scenario, this would come from configuration
        var frontendUrl = "http://localhost:3000";
        var invitationLink = $"{frontendUrl}/register?token={base64Token}";

        var subject = "You have been invited to join PromptForge";
        var body = $@"
            <h1>Welcome to PromptForge!</h1>
            <p>You have been invited to join our platform.</p>
            <p>Please click the link below to register and join your team:</p>
            <a href='{invitationLink}'>{invitationLink}</a>
        ";

        await _emailSender.SendAsync(input.Email, subject, body, isBodyHtml: true);
    }
}
