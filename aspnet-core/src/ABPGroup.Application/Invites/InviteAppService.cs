using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Authorization;
using ABPGroup.Authorization;
using ABPGroup.Invites.Dto;
using Microsoft.Extensions.Configuration;

namespace ABPGroup.Invites
{
    public class InviteAppService : ABPGroupAppServiceBase, IInviteAppService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public InviteAppService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task InviteUserAsync(InviteUserInput input)
        {
            var tenantId = AbpSession.TenantId ?? 0;
            var role = input.Role.ToString();

            // Create token payload
            var tokenPayload = new
            {
                tenantId = tenantId,
                role = role
            };

            // Base64 encode the token
            var json = JsonSerializer.Serialize(tokenPayload);
            var tokenBytes = Encoding.UTF8.GetBytes(json);
            var token = Convert.ToBase64String(tokenBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            // Build signup URL
            var clientRoot = _configuration["App:ClientRootAddress"]?.TrimEnd('/') ?? "http://localhost:3000";
            var signupUrl = $"{clientRoot}/register?token={token}";

            // Send email via Brevo
            await SendInviteEmailAsync(input.EmailAddress, signupUrl, role);
        }

        private async Task SendInviteEmailAsync(string emailAddress, string signupUrl, string role)
        {
            var apiKey = _configuration["Brevo:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Brevo API key is not configured.");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var emailPayload = new
            {
                sender = new
                {
                    name = "Lethabo Maepa",
                    email = "lethabomaepa11@gmail.com"
                },
                to = new[]
                {
                    new
                    {
                        email = emailAddress
                    }
                },
                subject = "You're invited to join PromptForge",
                htmlContent = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: white; margin: 0;'>PromptForge</h1>
                        </div>
                        <div style='padding: 30px; background: #f9f9f9;'>
                            <h2 style='color: #333;'>You've been invited!</h2>
                            <p style='color: #666; line-height: 1.6;'>
                                You've been invited to join PromptForge as a <strong>{role}</strong>.
                            </p>
                            <p style='color: #666; line-height: 1.6;'>
                                Click the button below to create your account and get started:
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{signupUrl}' 
                                   style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                                          color: white; 
                                          padding: 15px 30px; 
                                          text-decoration: none; 
                                          border-radius: 5px; 
                                          display: inline-block;
                                          font-weight: bold;'>
                                    Create Your Account
                                </a>
                            </div>
                            <p style='color: #999; font-size: 12px; margin-top: 30px;'>
                                If you didn't expect this invitation, you can safely ignore this email.
                            </p>
                        </div>
                    </body>
                    </html>"
            };

            var json = JsonSerializer.Serialize(emailPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to send invite email: {errorContent}");
            }
        }
    }
}