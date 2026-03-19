using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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

        private const string GroqEndpoint  = "https://api.groq.com/openai/v1/chat/completions";
        private const string DefaultOutput = "/app/GeneratedApps";
        private const int    MaxFixRetries = 3;

        private readonly string _outputBase;
        private readonly string _localCopyPath;
        private readonly bool   _skipBuild;

        public CodeGenAppService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration     = configuration;
            _outputBase        = _configuration["CodeGen:OutputPath"] ?? DefaultOutput;
            _localCopyPath     = _configuration["CodeGen:LocalCopyPath"];
            _skipBuild         = string.Equals(_configuration["CodeGen:SkipBuild"], "true", StringComparison.OrdinalIgnoreCase);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MAIN ENTRY POINT
        // ══════════════════════════════════════════════════════════════════════

        public async Task<CodeGenResult> GenerateProjectAsync(CreateUpdateProjectDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var projectName = SanitizeDirName(request.Name);
            var projectDir  = Path.Combine(_outputBase, projectName);

            // 1. Scaffold boilerplate (zero LLM tokens)
            var scaffold = ScaffoldBoilerplate(request);
            if (scaffold.Count > 0)
                WriteFiles(projectDir, scaffold);

            // 2. Generate app-specific code via LLM
            var result = await GenerateWithRetryAsync(request, scaffold);

            // 3. Merge scaffold + LLM files, write to disk
            result.Files = MergeFiles(scaffold, result.Files);
            WriteFiles(projectDir, result.Files);

            // 4. Validate build & auto-fix
            if (!_skipBuild)
                await BuildAndFixAsync(projectDir, result);

            // 5. Copy to local output path if configured
            if (!string.IsNullOrWhiteSpace(_localCopyPath))
            {
                var localDir = Path.Combine(_localCopyPath, projectName);
                CopyDirectory(projectDir, localDir);
                Logger.Info($"Copied project to local path: {localDir}");
            }

            result.OutputPath         = projectDir;
            result.GeneratedProjectId = request.Id;
            Logger.Info($"Generation complete. {result.Files.Count} files in {projectDir}");
            return result;
        }

        private async Task<CodeGenResult> GenerateWithRetryAsync(
            CreateUpdateProjectDto request, List<GeneratedFile> scaffold)
        {
            var scaffoldedPaths = scaffold.Select(f => f.Path).ToHashSet();
            var systemPrompt    = BuildSystemPrompt(request, scaffoldedPaths);
            var userPrompt      = BuildUserPrompt(request);

            var result = await CallGroqAsync(systemPrompt, userPrompt);
            if (result?.Files != null && result.Files.Count > 0)
                return result;

            Logger.Warn("First LLM call returned no files — retrying.");
            result = await CallGroqAsync(systemPrompt, userPrompt);
            if (result?.Files == null || result.Files.Count == 0)
                throw new InvalidOperationException("LLM returned no files after retry.");

            return result;
        }

        private static List<GeneratedFile> MergeFiles(List<GeneratedFile> baseFiles, List<GeneratedFile> overrides)
        {
            var merged = new List<GeneratedFile>(baseFiles);
            foreach (var f in overrides)
            {
                var idx = merged.FindIndex(x =>
                    string.Equals(x.Path, f.Path, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) merged[idx] = f;
                else merged.Add(f);
            }
            return merged;
        }

        private async Task BuildAndFixAsync(string projectDir, CodeGenResult result)
        {
            for (var attempt = 1; attempt <= MaxFixRetries; attempt++)
            {
                var (success, output) = await RunNpmBuildAsync(projectDir);
                if (success)
                {
                    Logger.Info($"Build passed on attempt {attempt}.");
                    return;
                }

                Logger.Warn($"Build failed (attempt {attempt}/{MaxFixRetries}).");
                if (attempt == MaxFixRetries)
                {
                    Logger.Error("Max fix retries reached. Returning last generated files.");
                    return;
                }

                var fixResult = await CallGroqAsync(
                    BuildFixSystemPrompt(),
                    BuildFixUserPrompt(result.Files, output));

                if (fixResult?.Files == null || fixResult.Files.Count == 0)
                    continue;

                WriteFiles(projectDir, fixResult.Files);
                result.Files = MergeFiles(result.Files, fixResult.Files);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BOILERPLATE SCAFFOLDING — saves ~3k tokens per generation
        // ══════════════════════════════════════════════════════════════════════

        private static List<GeneratedFile> ScaffoldBoilerplate(CreateUpdateProjectDto input)
        {
            if (input.Framework != Framework.NextJS)
                return new List<GeneratedFile>();

            var name     = input.Name ?? "my-app";
            var usePrisma = input.DatabaseOption != DatabaseOption.MongoCloud;
            var auth      = input.IncludeAuth;

            var files = new List<GeneratedFile>
            {
                MakePackageJson(name, auth, usePrisma, input.DatabaseOption == DatabaseOption.MongoCloud),
                MakeTsConfig(),
                MakeNextConfig(),
                MakePostCssConfig(),
                MakeRootLayout(name),
                MakeGlobalsCss(),
                MakeEnvExample(input.DatabaseOption),
            };

            if (usePrisma)
                files.Add(MakePrismaSchema(input.DatabaseOption, auth));

            if (auth)
                files.Add(MakeAuthConfig());

            return files;
        }

        private static GeneratedFile MakePackageJson(string name, bool auth, bool prisma, bool mongo)
        {
            var deps = new Dictionary<string, string>
            {
                ["next"]      = "^15.1.0",
                ["react"]     = "^19.0.0",
                ["react-dom"] = "^19.0.0"
            };
            if (prisma) deps["@prisma/client"] = "^6.0.0";
            if (mongo)  deps["mongoose"]       = "^8.0.0";
            if (auth)
            {
                deps["next-auth"] = "^5.0.0-beta.25";
                deps["bcryptjs"]  = "^2.4.3";
            }

            var pkg = new Dictionary<string, object>
            {
                ["name"]    = Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9-]", "-"),
                ["version"] = "0.1.0",
                ["private"] = true,
                ["scripts"] = new Dictionary<string, string>
                {
                    ["dev"]   = "next dev --turbopack",
                    ["build"] = "next build",
                    ["start"] = "next start",
                    ["lint"]  = "next lint"
                },
                ["dependencies"] = deps,
                ["devDependencies"] = new Dictionary<string, string>
                {
                    ["typescript"]          = "^5.7.0",
                    ["@types/node"]         = "^22.0.0",
                    ["@types/react"]        = "^19.0.0",
                    ["@types/react-dom"]    = "^19.0.0",
                    ["@tailwindcss/postcss"] = "^4.0.0",
                    ["tailwindcss"]         = "^4.0.0",
                    ["eslint"]              = "^9.0.0",
                    ["eslint-config-next"]  = "^15.0.0"
                }
            };

            return new GeneratedFile
            {
                Path    = "package.json",
                Content = JsonSerializer.Serialize(pkg, new JsonSerializerOptions { WriteIndented = true })
            };
        }

        private static GeneratedFile MakeTsConfig() => new GeneratedFile
        {
            Path = "tsconfig.json",
            Content = @"{
  ""compilerOptions"": {
    ""target"": ""ES2017"",
    ""lib"": [""dom"", ""dom.iterable"", ""esnext""],
    ""allowJs"": true,
    ""skipLibCheck"": true,
    ""strict"": true,
    ""noEmit"": true,
    ""esModuleInterop"": true,
    ""module"": ""esnext"",
    ""moduleResolution"": ""bundler"",
    ""resolveJsonModule"": true,
    ""isolatedModules"": true,
    ""jsx"": ""preserve"",
    ""incremental"": true,
    ""plugins"": [{ ""name"": ""next"" }],
    ""paths"": { ""@/*"": [""./src/*""] }
  },
  ""include"": [""next-env.d.ts"", ""**/*.ts"", ""**/*.tsx"", "".next/types/**/*.ts""],
  ""exclude"": [""node_modules""]
}"
        };

        private static GeneratedFile MakeNextConfig() => new GeneratedFile
        {
            Path    = "next.config.ts",
            Content = "import type { NextConfig } from 'next';\n\nconst nextConfig: NextConfig = {};\n\nexport default nextConfig;\n"
        };

        private static GeneratedFile MakePostCssConfig() => new GeneratedFile
        {
            Path    = "postcss.config.mjs",
            Content = "const config = {\n  plugins: {\n    '@tailwindcss/postcss': {},\n  },\n};\n\nexport default config;\n"
        };

        private static GeneratedFile MakeRootLayout(string appName) => new GeneratedFile
        {
            Path = "src/app/layout.tsx",
            Content = $@"import type {{ Metadata }} from 'next';
import './globals.css';

export const metadata: Metadata = {{
  title: '{appName.Replace("'", "\\'")}',
  description: 'Generated by PromptForge',
}};

export default function RootLayout({{ children }}: {{ children: React.ReactNode }}) {{
  return (
    <html lang=""en"">
      <body>{{children}}</body>
    </html>
  );
}}
"
        };

        private static GeneratedFile MakeGlobalsCss() => new GeneratedFile
        {
            Path    = "src/app/globals.css",
            Content = "@import 'tailwindcss';\n"
        };

        private static GeneratedFile MakeEnvExample(DatabaseOption db) => new GeneratedFile
        {
            Path = ".env.example",
            Content = db == DatabaseOption.MongoCloud
                ? "MONGODB_URI=mongodb+srv://user:pass@cluster.mongodb.net/mydb\nNEXTAUTH_SECRET=change-me\nNEXTAUTH_URL=http://localhost:3000\n"
                : "DATABASE_URL=postgresql://user:pass@localhost:5432/mydb\nNEXTAUTH_SECRET=change-me\nNEXTAUTH_URL=http://localhost:3000\n"
        };

        private static GeneratedFile MakePrismaSchema(DatabaseOption db, bool auth) => new GeneratedFile
        {
            Path = "prisma/schema.prisma",
            Content = $@"generator client {{
  provider = ""prisma-client-js""
}}

datasource db {{
  provider = ""postgresql""
  url      = env(""DATABASE_URL"")
}}{(auth ? @"

model User {
  id        String   @id @default(cuid())
  email     String   @unique
  password  String
  name      String?
  createdAt DateTime @default(now())
  updatedAt DateTime @updatedAt
}" : "")}
"
        };

        private static GeneratedFile MakeAuthConfig() => new GeneratedFile
        {
            Path = "src/lib/auth.ts",
            Content = @"import NextAuth from 'next-auth';
import Credentials from 'next-auth/providers/credentials';

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    Credentials({
      credentials: {
        email: { label: 'Email', type: 'email' },
        password: { label: 'Password', type: 'password' },
      },
      async authorize(credentials) {
        if (credentials?.email && credentials?.password) {
          return { id: '1', email: credentials.email as string, name: 'User' };
        }
        return null;
      },
    }),
  ],
  session: { strategy: 'jwt' },
  pages: { signIn: '/login' },
});
"
        };

        // ══════════════════════════════════════════════════════════════════════
        //  PROMPT BUILDERS — lean and targeted
        // ══════════════════════════════════════════════════════════════════════

        private string BuildSystemPrompt(CreateUpdateProjectDto input, HashSet<string> scaffolded)
        {
            var framework = FormatFramework(input.Framework);
            var db        = FormatDatabase(input.DatabaseOption);
            var auth      = input.IncludeAuth ? "next-auth v5 (pre-configured in src/lib/auth.ts)" : "none";

            var scaffoldNote = scaffolded.Count > 0
                ? $"\nThese files are ALREADY generated — do NOT output them:\n{string.Join(", ", scaffolded)}"
                : "";

            var stackRules = input.Framework switch
            {
                Framework.NextJS     => "- Next.js 15 App Router, React 19, Tailwind CSS v4\n- Imports use @/ alias (mapped to ./src/*)\n- 'use client' only on components with hooks/events/browser APIs",
                Framework.ReactVite  => "- React 19, Vite, Tailwind CSS v4, react-router-dom v7",
                Framework.Angular    => "- Angular 19 standalone components, signals",
                Framework.Vue        => "- Vue 3 Composition API, Vite, Tailwind CSS v4",
                _                    => "- Follow framework best practices"
            };

            return $@"You are a code generator. Output ONLY files. No markdown, no explanations, no commentary.
{scaffoldNote}

Stack: {framework} | TypeScript strict | {db} | Auth: {auth}
{stackRules}
- Every file COMPLETE — no TODOs, no placeholders, no ""..."", no truncation
- Must compile: npm install && npm run build

Format:
===ARCHITECTURE===
One sentence
===END ARCHITECTURE===
===MODULES===
comma,separated,names
===END MODULES===
===FILE===
path/to/file.tsx
===CONTENT===
<complete file content>
===END FILE===";
        }

        private static string BuildUserPrompt(CreateUpdateProjectDto input) =>
            $"Build: {input.Prompt ?? "a starter app"}\nProject: {input.Name ?? "my-app"}";

        private static string BuildFixSystemPrompt() =>
            "You fix build errors. Return ONLY changed files. Complete file contents, not diffs. No explanations.\n===FILE===\npath\n===CONTENT===\n<fixed file>\n===END FILE===";

        private static string BuildFixUserPrompt(List<GeneratedFile> files, string buildOutput)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BUILD ERRORS:");
            sb.AppendLine(buildOutput.Length > 4000 ? buildOutput.Substring(0, 4000) : buildOutput);

            // Identify files referenced in error output
            var errorFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in buildOutput.Split('\n'))
            {
                var match = Regex.Match(line, @"(?:\.\/)?([a-zA-Z0-9_\-\/\.]+\.(?:tsx?|jsx?|mjs|css))");
                if (match.Success) errorFiles.Add(match.Groups[1].Value);
            }

            sb.AppendLine("\nFILES:");
            foreach (var f in files)
            {
                sb.AppendLine($"\n--- {f.Path} ---");
                // Full content for error-referenced files; brief preview for others
                var isReferenced = errorFiles.Any(e =>
                    f.Path.EndsWith(e, StringComparison.OrdinalIgnoreCase) ||
                    f.Path.Contains(e, StringComparison.OrdinalIgnoreCase));

                if (isReferenced)
                    sb.AppendLine(f.Content);
                else if (f.Content.Length > 200)
                    sb.AppendLine(f.Content.Substring(0, 200)).AppendLine("...(truncated)");
                else
                    sb.AppendLine(f.Content);
            }
            return sb.ToString();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FILE I/O
        // ══════════════════════════════════════════════════════════════════════

        private void WriteFiles(string projectDir, List<GeneratedFile> files)
        {
            foreach (var file in files)
            {
                var normalized = file.Path
                    .Replace("/",  Path.DirectorySeparatorChar.ToString())
                    .Replace("\\", Path.DirectorySeparatorChar.ToString());
                var fullPath = Path.Combine(projectDir, normalized);
                var dir = Path.GetDirectoryName(fullPath);

                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(fullPath, file.Content, Encoding.UTF8);
            }
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDir, file);
                var destFile     = Path.Combine(destDir, relativePath);
                var destFileDir  = Path.GetDirectoryName(destFile);

                if (!string.IsNullOrEmpty(destFileDir) && !Directory.Exists(destFileDir))
                    Directory.CreateDirectory(destFileDir);

                File.Copy(file, destFile, overwrite: true);
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  NPM BUILD
        // ══════════════════════════════════════════════════════════════════════

        private async Task<(bool success, string output)> RunNpmBuildAsync(string projectDir)
        {
            if (!Directory.Exists(projectDir))
                return (false, $"Directory missing: {projectDir}");

            var sb = new StringBuilder();

            var (installCode, installOut) = await RunProcessAsync("npm", "install --prefer-offline", projectDir, 120);
            sb.AppendLine("=== npm install ===").AppendLine(installOut);
            if (installCode != 0) return (false, sb.ToString());

            var (buildCode, buildOut) = await RunProcessAsync("npm", "run build", projectDir, 180);
            sb.AppendLine("=== npm run build ===").AppendLine(buildOut);

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

            var finished = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));
            if (!finished)
            {
                process.Kill(entireProcessTree: true);
                return (-1, $"Timed out after {timeoutSeconds}s.\n{output}");
            }

            return (process.ExitCode, output.ToString());
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GROQ API — 32k tokens, low temperature
        // ══════════════════════════════════════════════════════════════════════

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
                max_tokens  = 32768,
                temperature = 0.1
            };

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            var response = await httpClient.PostAsync(GroqEndpoint,
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));
            var body = await response.Content.ReadAsStringAsync();

            Logger.Debug($"Groq [{(int)response.StatusCode}]: {body.Substring(0, Math.Min(300, body.Length))}");

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error($"Groq API error {(int)response.StatusCode}: {body}");
                return null;
            }

            return ParseGroqResponse(body);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  RESPONSE PARSING
        // ══════════════════════════════════════════════════════════════════════

        private CodeGenResult ParseGroqResponse(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                {
                    Logger.Error("Groq returned empty content.");
                    return null;
                }

                Logger.Info($"LLM response: {text.Length} chars");
                return ParseDelimitedResponse(text);
            }
            catch (Exception ex)
            {
                Logger.Error($"Parse error: {ex.Message}");
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

            var pos = 0;
            while (true)
            {
                var fileStart = text.IndexOf("===FILE===", pos, StringComparison.Ordinal);
                if (fileStart < 0) break;

                var contentMarker = text.IndexOf("===CONTENT===", fileStart, StringComparison.Ordinal);
                if (contentMarker < 0) break;

                var fileEnd = text.IndexOf("===END FILE===", contentMarker, StringComparison.Ordinal);
                var path    = text.Substring(fileStart + 10, contentMarker - fileStart - 10).Trim();
                var content = fileEnd >= 0
                    ? text.Substring(contentMarker + 13, fileEnd - contentMarker - 13).Trim()
                    : text.Substring(contentMarker + 13).Trim();

                if (!string.IsNullOrWhiteSpace(path))
                    result.Files.Add(new GeneratedFile { Path = path, Content = content });

                pos = fileEnd >= 0 ? fileEnd + 14 : text.Length;
            }

            Logger.Info($"Parsed {result.Files.Count} files.");
            return result.Files.Count > 0 ? result : null;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FORMATTERS
        // ══════════════════════════════════════════════════════════════════════

        private static string SanitizeDirName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "unnamed-project";

            // Replace spaces with hyphens, strip invalid path chars, collapse multiple hyphens
            var safe = Regex.Replace(name.Trim(), @"[^\w\-.]", "-");
            safe = Regex.Replace(safe, @"-{2,}", "-").Trim('-');

            // Cap length to avoid filesystem path limits
            if (safe.Length > 80)
                safe = safe.Substring(0, 80).TrimEnd('-');

            return string.IsNullOrWhiteSpace(safe) ? "unnamed-project" : safe.ToLowerInvariant();
        }

        private static string FormatFramework(Framework fw) => fw switch
        {
            Framework.ReactVite    => "React (Vite)",
            Framework.Angular      => "Angular",
            Framework.Vue          => "Vue",
            Framework.DotNetBlazor => ".NET Blazor",
            _                      => "Next.js 15 (App Router)"
        };

        private static string FormatDatabase(DatabaseOption opt) => opt switch
        {
            DatabaseOption.MongoCloud => "MongoDB via Mongoose",
            _                        => "PostgreSQL via Prisma"
        };
    }
}
