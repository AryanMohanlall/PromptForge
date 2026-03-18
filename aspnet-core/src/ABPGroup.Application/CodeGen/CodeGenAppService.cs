using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using Abp.Application.Services;
using Microsoft.Extensions.Configuration;

namespace ABPGroup.CodeGen
{
    public class CodeGenAppService : ApplicationService, ICodeGenAppService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private const string GroqEndpoint = "https://api.groq.com/openai/v1/chat/completions";
        private const string OutputBase    = "/app/GeneratedApps";
        private const int    MaxFixRetries = 3;

        public CodeGenAppService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration     = configuration;
        }

        public async Task<CodeGenResult> GenerateProjectAsync(CreateUpdateProjectDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var projectName = string.IsNullOrWhiteSpace(request.Name) ? "UnnamedProject" : request.Name;
            var projectDir  = Path.Combine(OutputBase, projectName);

            // ── 1. Generate initial files ──────────────────────────────────────
            var result = await CallGroqAsync(GenerateSystemMessage(request), GenerateUserMessage(request));

            if (result == null || result.Files == null || result.Files.Count == 0)
            {
                Logger.Warn("First AI attempt returned no files — retrying.");
                result = await CallGroqAsync(GenerateSystemMessage(request), GenerateUserMessage(request));
                if (result == null || result.Files == null || result.Files.Count == 0)
                    throw new Exception("AI returned no files after retry.");
            }

            WriteFiles(projectDir, result.Files);

            // ── 2. Validate: npm install + npm run build ───────────────────────
            for (var attempt = 1; attempt <= MaxFixRetries; attempt++)
            {
                var (success, output) = await RunNpmBuildAsync(projectDir);
                if (success)
                {
                    Logger.Info($"npm build passed on attempt {attempt}.");
                    break;
                }

                Logger.Warn($"npm build failed (attempt {attempt}/{MaxFixRetries}):\n{output}");

                if (attempt == MaxFixRetries)
                {
                    Logger.Error("Max fix retries reached. Returning last generated files.");
                    break;
                }

                // ── 3. Ask AI to fix the errors ────────────────────────────────
                var fixResult = await CallGroqAsync(
                    GenerateFixSystemMessage(),
                    GenerateFixUserMessage(result.Files, output));

                if (fixResult?.Files != null && fixResult.Files.Count > 0)
                {
                    // Overwrite only the files the AI returned
                    WriteFiles(projectDir, fixResult.Files);
                    foreach (var f in fixResult.Files)
                    {
                        var existing = result.Files.FirstOrDefault(x => x.Path == f.Path);
                        if (existing != null) existing.Content = f.Content;
                        else result.Files.Add(f);
                    }
                }
            }

            result.OutputPath          = projectDir;
            result.GeneratedProjectId  = request.Id;

            Logger.Info($"Generation complete. {result.Files.Count} files in {projectDir}");
            return result;
        }

        // ── File writing ────────────────────────────────────────────────────────
        private void WriteFiles(string projectDir, List<GeneratedFile> files)
        {
            foreach (var file in files)
            {
                var normalizedPath = file.Path
                    .Replace("/",  Path.DirectorySeparatorChar.ToString())
                    .Replace("\\", Path.DirectorySeparatorChar.ToString());
                var fullPath = Path.Combine(projectDir, normalizedPath);
                var dir = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(fullPath, file.Content, Encoding.UTF8);
                Logger.Debug($"Written: {fullPath}");
            }
        }

        // ── npm install + npm run build ─────────────────────────────────────────
        private async Task<(bool success, string output)> RunNpmBuildAsync(string projectDir)
        {
            if (!Directory.Exists(projectDir))
                return (false, $"Project directory does not exist: {projectDir}");

            var sb = new StringBuilder();

            // npm install
            var (installCode, installOut) = await RunProcessAsync("npm", "install --prefer-offline", projectDir, timeoutSeconds: 120);
            sb.AppendLine("=== npm install ===");
            sb.AppendLine(installOut);

            if (installCode != 0)
                return (false, sb.ToString());

            // npm run build
            var (buildCode, buildOut) = await RunProcessAsync("npm", "run build", projectDir, timeoutSeconds: 180);
            sb.AppendLine("=== npm run build ===");
            sb.AppendLine(buildOut);

            return (buildCode == 0, sb.ToString());
        }

        private static async Task<(int exitCode, string output)> RunProcessAsync(
            string command, string args, string workingDir, int timeoutSeconds = 120)
        {
            var psi = new ProcessStartInfo(command, args)
            {
                WorkingDirectory       = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };

            using var process = new Process { StartInfo = psi };
            var output = new StringBuilder();

            process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived  += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeout = TimeSpan.FromSeconds(timeoutSeconds);
            var finished = await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds));

            if (!finished)
            {
                process.Kill(entireProcessTree: true);
                return (-1, $"Process timed out after {timeoutSeconds}s.\n{output}");
            }

            return (process.ExitCode, output.ToString());
        }

        // ── Groq API call ───────────────────────────────────────────────────────
        private async Task<CodeGenResult> CallGroqAsync(string systemMessage, string userMessage)
        {
            var apiKey = _configuration["Groq:ApiKey"];
            var model  = _configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Groq:ApiKey is not configured.");

            var payload = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user",   content = userMessage   }
                },
                max_tokens  = 8192,
                temperature = 0.2
            };

            var json    = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.Timeout = TimeSpan.FromMinutes(3);

            var response       = await httpClient.PostAsync(GroqEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();

            Logger.Warn($"RAW GROQ [{(int)response.StatusCode}]: {responseString.Substring(0, Math.Min(300, responseString.Length))}");

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Groq API error {(int)response.StatusCode}: {responseString}");
                return null;
            }

            return ParseGroqResponse(responseString);
        }

        // ── Response parsing ────────────────────────────────────────────────────
        private CodeGenResult ParseGroqResponse(string responseString)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    Logger.Error("Groq content is empty.");
                    return null;
                }

                Logger.Info($"Groq content length: {text.Length} chars");
                return ParseDelimitedResponse(text);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to parse Groq response: {ex.Message}");
                return null;
            }
        }

        private CodeGenResult ParseDelimitedResponse(string text)
        {
            var result = new CodeGenResult
            {
                Files      = new List<GeneratedFile>(),
                ModuleList = new List<string>()
            };

            var archStart = text.IndexOf("===ARCHITECTURE===", StringComparison.Ordinal);
            var archEnd   = text.IndexOf("===END ARCHITECTURE===", StringComparison.Ordinal);
            if (archStart >= 0 && archEnd > archStart)
                result.ArchitectureSummary = text.Substring(archStart + 18, archEnd - archStart - 18).Trim();

            var modStart = text.IndexOf("===MODULES===", StringComparison.Ordinal);
            var modEnd   = text.IndexOf("===END MODULES===", StringComparison.Ordinal);
            if (modStart >= 0 && modEnd > modStart)
                result.ModuleList = text.Substring(modStart + 13, modEnd - modStart - 13)
                    .Trim().Split(',').Select(m => m.Trim()).Where(m => m.Length > 0).ToList();

            var searchFrom = 0;
            while (true)
            {
                var fileStart = text.IndexOf("===FILE===", searchFrom, StringComparison.Ordinal);
                if (fileStart < 0) break;

                var contentMarker = text.IndexOf("===CONTENT===", fileStart, StringComparison.Ordinal);
                if (contentMarker < 0) break;

                var fileEnd  = text.IndexOf("===END FILE===", contentMarker, StringComparison.Ordinal);
                var path     = text.Substring(fileStart + 10, contentMarker - fileStart - 10).Trim();
                var filecontent = fileEnd >= 0
                    ? text.Substring(contentMarker + 13, fileEnd - contentMarker - 13).Trim()
                    : text.Substring(contentMarker + 13).Trim();

                if (!string.IsNullOrWhiteSpace(path))
                    result.Files.Add(new GeneratedFile { Path = path, Content = filecontent });

                searchFrom = fileEnd >= 0 ? fileEnd + 14 : text.Length;
            }

            Logger.Info($"Parsed {result.Files.Count} files.");
            return result.Files.Count > 0 ? result : null;
        }

        // ── Prompt builders ─────────────────────────────────────────────────────
        private string GenerateSystemMessage(CreateUpdateProjectDto input)
        {
            var framework = FormatFramework(input.Framework);
            var language  = FormatLanguage(input.Language);
            var database  = FormatDatabase(input.DatabaseOption);
            var auth      = input.IncludeAuth ? "next-auth v5 with credentials + JWT" : "none";

            return $@"You are a senior full-stack developer. Generate a production-ready {framework} application.

Stack: {framework} | {language} | {database} | Auth: {auth}

Rules:
- Every file must be complete and runnable — no TODOs, no placeholders
- Use React 19 (useActionState not useFormState), next.config.ts (not .js)
- Turbopack is default — no webpack config
- The app MUST compile with zero errors when running: npm install && npm run build

Respond in EXACTLY this format. No JSON. No markdown. No extra text:

===ARCHITECTURE===
One or two sentences describing what was built.
===END ARCHITECTURE===

===MODULES===
comma,separated,module,names
===END MODULES===

===FILE===
package.json
===CONTENT===
{{full file content here}}
===END FILE===

===FILE===
tsconfig.json
===CONTENT===
{{full file content here}}
===END FILE===

Generate as many files as needed. Every file must have complete, working content.";
        }

        private string GenerateUserMessage(CreateUpdateProjectDto input)
        {
            var prompt = input.Prompt ?? string.Empty;
            var name   = string.IsNullOrWhiteSpace(input.Name) ? "Unnamed Project" : input.Name;
            return $"Build this app: {prompt}\nProject name: {name}";
        }

        private static string GenerateFixSystemMessage() =>
            @"You are a senior full-stack developer fixing build errors in a generated project.
You will be given the current files and the npm build error output.
Return ONLY the files that need to be fixed in the same delimiter format:

===FILE===
path/to/file
===CONTENT===
{complete fixed file content}
===END FILE===

Fix ALL errors. Return complete file contents, not diffs. No explanations.";

        private static string GenerateFixUserMessage(List<GeneratedFile> files, string buildOutput)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Build failed with these errors:");
            sb.AppendLine(buildOutput.Length > 3000 ? buildOutput.Substring(0, 3000) : buildOutput);
            sb.AppendLine("\nCurrent files:");
            foreach (var f in files)
            {
                sb.AppendLine($"\n--- {f.Path} ---");
                var snippet = f.Content.Length > 500 ? f.Content.Substring(0, 500) + "..." : f.Content;
                sb.AppendLine(snippet);
            }
            return sb.ToString();
        }

        private static string FormatFramework(Framework framework) => framework switch
        {
            Framework.ReactVite    => "React (Vite)",
            Framework.Angular      => "Angular",
            Framework.Vue          => "Vue",
            Framework.DotNetBlazor => ".NET Blazor",
            _                      => "Next.js 16.1 (App Router)"
        };

        private static string FormatLanguage(ProgrammingLanguage language) => language switch
        {
            ProgrammingLanguage.JavaScript => "JavaScript",
            ProgrammingLanguage.CSharp     => "C#",
            _                              => "TypeScript (strict)"
        };

        private static string FormatDatabase(DatabaseOption option) => option switch
        {
            DatabaseOption.MongoCloud => "MongoDB via Mongoose",
            _                         => "PostgreSQL via Prisma"
        };
    }
}
