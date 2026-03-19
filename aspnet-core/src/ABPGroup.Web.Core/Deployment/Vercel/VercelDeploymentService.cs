using Abp.Dependency;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // repoId is the numeric GitHub repository ID returned by the GitHub API
        // on repo create/get — e.g. { "id": 123456789, "full_name": "owner/repo" }
        public async Task<VercelDeploymentResult> TriggerDeploymentAsync(
            string repositoryFullName,
            long repoId,
            string branch,
            string projectName,
            string commitSha)
        {
            if (string.IsNullOrWhiteSpace(repositoryFullName))
            {
                return new VercelDeploymentResult
                {
                    Triggered = false,
                    ErrorMessage = "Repository full name is required to trigger Vercel deployment."
                };
            }

            if (repoId <= 0)
            {
                return new VercelDeploymentResult
                {
                    Triggered = false,
                    ErrorMessage = "A valid numeric GitHub repoId is required to trigger Vercel deployment."
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
            var deploymentEndpoint = BuildDeploymentEndpoint();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var query = new Dictionary<string, string>();

                var skipAutoDetectionConfirmation = _configuration["Vercel:SkipAutoDetectionConfirmation"];
                if (!string.IsNullOrWhiteSpace(skipAutoDetectionConfirmation))
                    query["skipAutoDetectionConfirmation"] = skipAutoDetectionConfirmation;

                var teamId = _configuration["Vercel:TeamId"];
                if (!string.IsNullOrWhiteSpace(teamId))
                    query["teamId"] = teamId;

                var slug = _configuration["Vercel:Slug"];
                if (!string.IsNullOrWhiteSpace(slug))
                    query["slug"] = slug;

                var projectId = _configuration["Vercel:ProjectId"];
                var projectNameOrId = !string.IsNullOrWhiteSpace(projectId)
                    ? projectId
                    : (_configuration["Vercel:ProjectName"] ?? resolvedProjectName);

                var requestBody = new
                {
                    name = resolvedProjectName,
                    project = string.IsNullOrWhiteSpace(projectNameOrId) ? null : projectNameOrId,
                    target = "production",
                    // gitMetadata is informational only — does not affect routing
                    gitMetadata = new
                    {
                        remoteUrl = string.Format("https://github.com/{0}", repositoryFullName),
                        commitRef = resolvedBranch,
                        commitSha = string.IsNullOrWhiteSpace(commitSha) ? null : commitSha,
                        dirty = false,
                        ci = false
                    },
                    // gitSource MUST use numeric repoId — not repoUrl, not repo name
                    gitSource = new
                    {
                        type = "github",
                        repoId = repoId.ToString(),   // ← the fix
                        @ref = resolvedBranch,
                        sha = string.IsNullOrWhiteSpace(commitSha) ? null : commitSha
                    }
                };

                var requestUri = BuildRequestUri(deploymentEndpoint, query);
                var response = await client.PostAsJsonAsync(requestUri, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var parsedError = TryExtractVercelError(errorBody);

                    var errorParts = new List<string>
                    {
                        string.Format("Vercel deployment request failed ({0}).", (int)response.StatusCode)
                    };

                    if (!string.IsNullOrWhiteSpace(parsedError))
                        errorParts.Add(parsedError);

                    if (!string.IsNullOrWhiteSpace(errorBody))
                        errorParts.Add(errorBody);

                    return new VercelDeploymentResult
                    {
                        Triggered = false,
                        ErrorMessage = string.Join(" ", errorParts.Where(p => !string.IsNullOrWhiteSpace(p)))
                    };
                }

                var payload = await response.Content.ReadFromJsonAsync<VercelCreateDeploymentResponse>();
                return new VercelDeploymentResult
                {
                    Triggered = !string.IsNullOrWhiteSpace(payload?.Id),
                    DeploymentId = payload?.Id,
                    Url = NormalizeVercelUrl(payload?.Url),
                    InspectorUrl = NormalizeVercelUrl(payload?.InspectorUrl),
                    State = payload?.State,
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

        private string BuildDeploymentEndpoint()
        {
            var endpoint = _configuration["Vercel:DeploymentsEndpoint"];
            return string.IsNullOrWhiteSpace(endpoint)
                ? "https://api.vercel.com/v13/deployments?skipAutoDetectionConfirmation=1&"
                : endpoint;
        }

        private static string BuildRequestUri(string endpoint, IDictionary<string, string> query)
        {
            if (query == null || query.Count == 0)
                return endpoint;

            var separator = endpoint.Contains("?") ? "&" : "?";
            var queryString = string.Join("&", query
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value != null)
                .Select(kv => string.Format("{0}={1}",
                    Uri.EscapeDataString(kv.Key),
                    Uri.EscapeDataString(kv.Value))));

            return string.IsNullOrWhiteSpace(queryString)
                ? endpoint
                : string.Format("{0}{1}{2}", endpoint, separator, queryString);
        }

        private static string ResolveProjectName(string projectName, string repositoryFullName)
        {
            if (!string.IsNullOrWhiteSpace(projectName))
                return SanitizeProjectName(projectName);

            var repoName = repositoryFullName;
            var slashIndex = repositoryFullName.IndexOf('/');
            if (slashIndex >= 0 && slashIndex < repositoryFullName.Length - 1)
                repoName = repositoryFullName.Substring(slashIndex + 1);

            return SanitizeProjectName(repoName);
        }

        private static string SanitizeProjectName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "promptforge-app";

            var safe = value.Trim().ToLowerInvariant().Replace(" ", "-");
            var chars = safe.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var ch = chars[i];
                var isAlphaNum = (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9');
                if (!isAlphaNum && ch != '-')
                    chars[i] = '-';
            }

            safe = new string(chars).Trim('-');
            while (safe.Contains("--"))
                safe = safe.Replace("--", "-");

            if (safe.Length > 100)
                safe = safe.Substring(0, 100).Trim('-');

            return string.IsNullOrWhiteSpace(safe) ? "promptforge-app" : safe;
        }

        private static string NormalizeVercelUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? value
                : string.Format("https://{0}", value.TrimStart('/'));
        }

        private static string TryExtractVercelError(string rawError)
        {
            if (string.IsNullOrWhiteSpace(rawError))
                return null;

            try
            {
                var payload = System.Text.Json.JsonSerializer.Deserialize<VercelErrorResponse>(rawError);
                if (payload?.Error == null) return null;

                var message = payload.Error.Message;
                if (string.IsNullOrWhiteSpace(message)) return null;

                var code = payload.Error.Code;
                return string.IsNullOrWhiteSpace(code)
                    ? message
                    : string.Format("{0}: {1}", code, message);
            }
            catch { return null; }
        }

        // ── Private response models ───────────────────────────────────────────

        private class VercelCreateDeploymentResponse
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("url")]
            public string Url { get; set; }

            [JsonPropertyName("inspectorUrl")]
            public string InspectorUrl { get; set; }

            [JsonPropertyName("readyState")]
            public string State { get; set; }
        }

        private class VercelErrorResponse
        {
            [JsonPropertyName("error")]
            public VercelErrorDetail Error { get; set; }
        }

        private class VercelErrorDetail
        {
            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
        }
    }
}