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
                role = role,
                email = input.EmailAddress,
                expires = DateTime.UtcNow.AddDays(7) // Token valid for 7 days
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
                    name = "PromptForge",
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
                <head>
                    <meta charset='UTF-8' />
                    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                </head>
                <body style='margin:0; padding:0; background-color:#050b13; font-family: Outfit, -apple-system, sans-serif;'>
                    <table width='100%' cellpadding='0' cellspacing='0' style='background-color:#050b13; padding: 40px 16px;'>
                        <tr>
                            <td align='center'>
                                <table width='600' cellpadding='0' cellspacing='0' style='max-width:600px; width:100%;'>

                                    <!-- Header -->
                                    <tr>
                                        <td align='center' style='
                                            background: #0f1a27;
                                            border: 1px solid rgba(40,225,140,0.25);
                                            border-bottom: none;
                                            border-radius: 10px 10px 0 0;
                                            padding: 32px 40px;
                                        '>
                                            <img src='http://promptforgesa.vercel.app/_next/image?url=%2Flogo.svg&w=96&q=75'
                                                alt='PromptForge'
                                                width='48'
                                                style='display:block; margin: 0 auto 16px auto;' />
                                            <span style='
                                                font-size: 22px;
                                                font-weight: 700;
                                                color: #22c55e;
                                                letter-spacing: 0.5px;
                                            '>PromptForge</span>
                                        </td>
                                    </tr>

                                    <!-- Divider -->
                                    <tr>
                                        <td style='
                                            background: #0f1a27;
                                            border-left: 1px solid rgba(40,225,140,0.25);
                                            border-right: 1px solid rgba(40,225,140,0.25);
                                            padding: 0 40px;
                                        '>
                                            <div style='height:1px; background: rgba(40,225,140,0.2);'></div>
                                        </td>
                                    </tr>

                                    <!-- Body -->
                                    <tr>
                                        <td style='
                                            background: #0f1a27;
                                            border-left: 1px solid rgba(40,225,140,0.25);
                                            border-right: 1px solid rgba(40,225,140,0.25);
                                            padding: 40px 40px 32px 40px;
                                        '>
                                            <h2 style='
                                                margin: 0 0 16px 0;
                                                font-size: 22px;
                                                font-weight: 700;
                                                color: #e8f3ea;
                                            '>You've been invited!</h2>

                                            <p style='
                                                margin: 0 0 12px 0;
                                                font-size: 15px;
                                                color: #a7cfc1;
                                                line-height: 1.7;
                                            '>
                                                You've been invited to join <span style='color:#22c55e; font-weight:600;'>PromptForge</span> as a
                                                <span style='
                                                    color: #facc15;
                                                    font-weight: 700;
                                                    background: rgba(250,204,21,0.08);
                                                    padding: 2px 8px;
                                                    border-radius: 6px;
                                                    border: 1px solid rgba(250,204,21,0.2);
                                                '>{role}</span>.
                                            </p>

                                            <p style='
                                                margin: 0 0 32px 0;
                                                font-size: 15px;
                                                color: #a7cfc1;
                                                line-height: 1.7;
                                            '>
                                                Click the button below to create your account and get started.
                                            </p>

                                            <!-- CTA Button -->
                                            <div style='text-align:center; margin-bottom: 32px;'>
                                                <a href='{signupUrl}'
                                                style='
                                                    display: inline-block;
                                                    background: #22c55e;
                                                    color: #050b13;
                                                    font-size: 15px;
                                                    font-weight: 700;
                                                    text-decoration: none;
                                                    padding: 14px 36px;
                                                    border-radius: 10px;
                                                    letter-spacing: 0.3px;
                                                '>
                                                    Create Your Account
                                                </a>
                                            </div>

                                            <!-- Divider -->
                                            <div style='height:1px; background: rgba(40,225,140,0.2); margin-bottom: 24px;'></div>

                                            <p style='
                                                margin: 0;
                                                font-size: 12px;
                                                color: #5f9b86;
                                                line-height: 1.6;
                                            '>
                                                If you didn't expect this invitation, you can safely ignore this email.
                                            </p>
                                        </td>
                                    </tr>

                                    <!-- Footer -->
                                    <tr>
                                        <td style='
                                            background: #15202f;
                                            border: 1px solid rgba(40,225,140,0.25);
                                            border-top: none;
                                            border-radius: 0 0 10px 10px;
                                            padding: 20px 40px;
                                            text-align: center;
                                        '>
                                            <p style='
                                                margin: 0;
                                                font-size: 12px;
                                                color: #5f9b86;
                                            '>
                                                &copy; {DateTime.UtcNow.Year} PromptForge. All rights reserved.
                                            </p>
                                        </td>
                                    </tr>

                                </table>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>",
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