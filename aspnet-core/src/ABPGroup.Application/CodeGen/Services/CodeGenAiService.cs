using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Abp.Domain.Services;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABPGroup.CodeGen;

public class CodeGenAiService : DomainService, ICodeGenAiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IClaudeApiClient _claudeApiClient;

    public CodeGenAiService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IClaudeApiClient claudeApiClient)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _claudeApiClient = claudeApiClient;
    }

    public async Task<string> CallAiAsync(string systemPrompt, string userPrompt)
    {
        return await CallAiAsync(systemPrompt, userPrompt, null);
    }

    public async Task<string> CallAiAsync(string systemPrompt, string userPrompt, string responseMimeType = null)
    {
        var provider = _configuration["CodeGen:AiProvider"]?.ToLowerInvariant() ?? "gemini";
        
        if (provider == "claude")
        {
            return await _claudeApiClient.CallClaudeAsync(systemPrompt, userPrompt);
        }

        // Auto-detect JSON mode ONLY when the prompt explicitly asks for raw JSON output
        // (not when it merely mentions "json" in context like "SPEC_JSON" delimiters or "JSON file")
        // Delimited prompts (===SPEC_JSON===) need text/plain so the delimiters are preserved
        if (responseMimeType == null
            && !systemPrompt.Contains("===", StringComparison.Ordinal)
            && !userPrompt.Contains("===", StringComparison.Ordinal)
            && (systemPrompt.Contains("return a single valid JSON object", StringComparison.OrdinalIgnoreCase)
                || systemPrompt.Contains("return valid JSON", StringComparison.OrdinalIgnoreCase)))
        {
            responseMimeType = "application/json";
        }

        return await CallGeminiAsync(systemPrompt, userPrompt, responseMimeType);
    }

    private async Task<string> CallGeminiAsync(string systemPrompt, string userPrompt, string responseMimeType = null)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"] ?? "gemini-1.5-flash";
        var baseUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        int maxRetries = 3;
        int delaySeconds = 2;

        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(600);
                
                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = systemPrompt } }
                    },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = userPrompt } }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 65536,
                        response_mime_type = responseMimeType ?? "text/plain"
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var request = new HttpRequestMessage(HttpMethod.Post, baseUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                var response = await client.SendAsync(request);

                if (response.StatusCode == (System.Net.HttpStatusCode)429)
                {
                    if (i == maxRetries) throw new UserFriendlyException("AI service is currently overloaded. Please try again in a few minutes.");

                    Logger.Warn($"Gemini API Rate Limit (429). Retrying in {delaySeconds}s... (Attempt {i + 1}/{maxRetries})");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                    delaySeconds *= 2;
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                
                return doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? string.Empty;
            }
            catch (HttpRequestException ex) when (i < maxRetries)
            {
                Logger.Warn($"Gemini API Request failed: {ex.Message}. Retrying in {delaySeconds}s...");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                delaySeconds *= 2;
            }
        }

        throw new UserFriendlyException("Failed to communicate with the AI service after multiple attempts.");
    }
}
