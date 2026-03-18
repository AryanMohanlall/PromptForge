using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ABPGroup.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [Route("api/github/webhook")]
    public class GitHubWebhookController : ABPGroupControllerBase
    {
        private readonly IConfiguration _configuration;

        public GitHubWebhookController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var webhookSecret = _configuration["GitHubApp:WebhookSecret"];
            if (string.IsNullOrWhiteSpace(webhookSecret))
            {
                return StatusCode(500, new { message = "GitHub webhook secret is not configured." });
            }

            var signatureHeader = Request.Headers["X-Hub-Signature-256"].ToString();
            if (string.IsNullOrWhiteSpace(signatureHeader))
            {
                return Unauthorized(new { message = "Missing X-Hub-Signature-256 header." });
            }

            string payload;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, false, 1024, true))
            {
                payload = await reader.ReadToEndAsync();
            }

            if (!IsValidSignature(signatureHeader, payload, webhookSecret))
            {
                return Unauthorized(new { message = "Invalid webhook signature." });
            }

            var eventType = Request.Headers["X-GitHub-Event"].ToString();
            var deliveryId = Request.Headers["X-GitHub-Delivery"].ToString();
            Logger.Info($"GitHub webhook received. Event={eventType}, Delivery={deliveryId}");

            // GitHub expects any 2xx response for successful delivery.
            return Ok(new { received = true, eventType, deliveryId });
        }

        private static bool IsValidSignature(string signatureHeader, string payload, string secret)
        {
            if (!signatureHeader.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var providedSignature = signatureHeader.Substring("sha256=".Length);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);
            byte[] secretBytes = Encoding.UTF8.GetBytes(secret);

            string expectedSignature;
            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hash = hmac.ComputeHash(payloadBytes);
                expectedSignature = Convert.ToHexString(hash).ToLowerInvariant();
            }

            var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);
            var providedBytes = Encoding.UTF8.GetBytes(providedSignature.ToLowerInvariant());
            return expectedBytes.Length == providedBytes.Length
                && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
        }
    }
}
