using Abp.Dependency;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ABPGroup.Deployment.Vercel
{
    public class VercelDeploymentService : IVercelDeploymentService, ITransientDependency
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public VercelDeploymentService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<VercelDeploymentResult> TriggerDeploymentAsync(string repositoryFullName, string branch, string projectName)
        {
            if (string.IsNullOrWhiteSpace(repositoryFullName))
            {
                return new VercelDeploymentResult
                {
                    Triggered = false,
                    ErrorMessage = "Repository full name is required to trigger Vercel deployment."
                };
            }

            var token = _configuration["Vercel:Token"] ?? _configuration["Vercel__Token"];
            if (string.IsNullOrWhiteSpace(token))
            {
                return new VercelDeploymentResult
                {
                    Triggered = false,
                    ErrorMessage = "Vercel token is missing. Set Vercel__Token (maps to Vercel:Token)."
                };
            }

            var resolvedBranch = string.IsNullOrWhiteSpace(branch) ? "main" : branch.Trim();
            var resolvedProjectName = ResolveProjectName(projectName, repositoryFullName);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var requestBody = new
                {
                    name = resolvedProjectName,
                    gitSource = new
                    {
                        type = "github",
                        repo = repositoryFullName,
                        @ref = resolvedBranch
                    }
                };

                var response = await client.PostAsJsonAsync("https://api.vercel.com/v13/deployments", requestBody);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return new VercelDeploymentResult
                    {
                        Triggered = false,
                        ErrorMessage = string.Format("Vercel deployment request failed ({0}): {1}", (int)response.StatusCode, errorBody)
                    };
                }

                var payload = await response.Content.ReadFromJsonAsync<VercelCreateDeploymentResponse>();
                return new VercelDeploymentResult
                {
                    Triggered = !string.IsNullOrWhiteSpace(payload != null ? payload.Id : null),
                    DeploymentId = payload != null ? payload.Id : null,
                    Url = NormalizeVercelUrl(payload != null ? payload.Url : null),
                    InspectorUrl = NormalizeVercelUrl(payload != null ? payload.InspectorUrl : null),
                    State = payload != null ? payload.State : null,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                return new VercelDeploymentResult
                {
                    Triggered = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private static string ResolveProjectName(string projectName, string repositoryFullName)
        {
            if (!string.IsNullOrWhiteSpace(projectName))
            {
                return SanitizeProjectName(projectName);
            }

            var repoName = repositoryFullName;
            var slashIndex = repositoryFullName.IndexOf('/');
            if (slashIndex >= 0 && slashIndex < repositoryFullName.Length - 1)
            {
                repoName = repositoryFullName.Substring(slashIndex + 1);
            }

            return SanitizeProjectName(repoName);
        }

        private static string SanitizeProjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "promptforge-app";
            }

            var safe = value.Trim().ToLowerInvariant();
            safe = safe.Replace(" ", "-");

            var chars = safe.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                var isAlphaNum = (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9');
                if (!isAlphaNum && ch != '-')
                {
                    chars[i] = '-';
                }
            }

            safe = new string(chars).Trim('-');
            while (safe.Contains("--"))
            {
                safe = safe.Replace("--", "-");
            }

            if (safe.Length > 100)
            {
                safe = safe.Substring(0, 100).Trim('-');
            }

            return string.IsNullOrWhiteSpace(safe) ? "promptforge-app" : safe;
        }

        private static string NormalizeVercelUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            return string.Format("https://{0}", value.TrimStart('/'));
        }

        private class VercelCreateDeploymentResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("inspectorUrl")]
            public string InspectorUrl { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }
        }
    }
}