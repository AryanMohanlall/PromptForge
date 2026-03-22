using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ABPGroup.CodeGen;
using ABPGroup.CodeGen.Dto;
using ABPGroup.Templates;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace ABPGroup.Tests.CodeGen
{
    public class RepairTests
    {
        [Fact]
        public async Task Repair_UsesDiffTargetsForMissingNextShellFile()
        {
            var sessionId = Guid.NewGuid();
            var repairResponse = @"===SUMMARY===
Created the missing home page and styled it in place.
===END SUMMARY===

===FILE===
src/app/page.tsx
===CONTENT===
export default function HomePage() {
  return (
    <main className=""min-h-screen bg-slate-950 p-12 text-white"">
      <section className=""mx-auto max-w-3xl rounded-3xl border border-white/10 bg-white/5 p-10"">
        <h1 className=""text-4xl font-semibold"">Todo App</h1>
        <p className=""mt-4 text-lg text-slate-200"">Track work without a database.</p>
      </section>
    </main>
  );
}
===END FILE===";

            var handler = new PromptCapturingHttpMessageHandler(repairResponse);
            var factory = new MockHttpClientFactory(handler);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Gemini:ApiKey"] = "test-key"
                })
                .Build();

            var templateRepo = Substitute.For<IRepository<Template, int>>();
            var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();

            var existingFiles = new List<GeneratedFileDto>
            {
                new() { Path = "package.json", Content = "{\"name\":\"todo-app\"}" },
                new() { Path = "src/app/layout.tsx", Content = "export default function RootLayout({ children }: { children: React.ReactNode }) { return <html><body>{children}</body></html>; }" }
            };

            var session = new CodeGenSession
            {
                Id = sessionId,
                Prompt = "Build a todo app",
                NormalizedRequirement = "Build a todo app",
                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                SpecJson = "{\"pages\":[{\"route\":\"/\",\"name\":\"Home\",\"layout\":\"public\",\"components\":[],\"dataRequirements\":[],\"description\":\"Landing page\"}],\"validations\":[]}",
                GeneratedFilesJson = JsonSerializer.Serialize(existingFiles),
                Status = (int)CodeGenStatus.ValidationFailed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
            sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

            var service = CreateService(factory, config, templateRepo, sessionRepo);

            var result = await service.Repair(new TriggerRepairInput
            {
                SessionId = sessionId.ToString(),
                Failures = new List<ValidationResultDto>
                {
                    new() { Id = "shell-next-home-page", Status = "failed", Message = "Next.js shell file missing: src/app/page.tsx." },
                    new() { Id = "shell-styled-home-route", Status = "failed", Message = "No styled landing/home route found in src/app/page.tsx." }
                }
            });

            Assert.Contains("EXPECTED AFFECTED FILES", handler.LastUserPrompt);
            Assert.Contains("src/app/page.tsx", handler.LastUserPrompt);
            Assert.Equal("repair", result.GenerationMode);
            Assert.Equal((int)CodeGenStatus.ValidationPassed, result.Status);
            Assert.Contains(result.GeneratedFiles, file => file.Path == "src/app/page.tsx");
            Assert.Contains(result.ValidationResults, validation =>
                validation.Id == "shell-next-home-page" && validation.Status == "passed");
            Assert.Contains(result.ValidationResults, validation =>
                validation.Id == "shell-styled-home-route" && validation.Status == "passed");
        }

        [Fact]
        public async Task Repair_UsesSpecValidationTargetWhenFailureMessageHasNoPath()
        {
            var sessionId = Guid.NewGuid();
            var repairResponse = @"===SUMMARY===
Added the missing environment example file.
===END SUMMARY===

===FILE===
.env.example
===CONTENT===
API_URL=http://localhost:3000
===END FILE===";

            var handler = new PromptCapturingHttpMessageHandler(repairResponse);
            var factory = new MockHttpClientFactory(handler);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Gemini:ApiKey"] = "test-key"
                })
                .Build();

            var templateRepo = Substitute.For<IRepository<Template, int>>();
            var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();

            var existingFiles = new List<GeneratedFileDto>
            {
                new() { Path = "package.json", Content = "{\"name\":\"todo-app\"}" },
                new() { Path = "src/app/layout.tsx", Content = "export default function RootLayout({ children }: { children: React.ReactNode }) { return <html><body>{children}</body></html>; }" },
                new() { Path = "src/app/page.tsx", Content = "export default function HomePage() { return <main className=\"p-8\">Home</main>; }" }
            };

            var spec = new AppSpecDto
            {
                Pages = new List<PageSpecDto>
                {
                    new()
                    {
                        Route = "/",
                        Name = "Home",
                        Layout = "public",
                        Components = new List<string>(),
                        DataRequirements = new List<string>(),
                        Description = "Landing page"
                    }
                },
                Validations = new List<ValidationRuleDto>
                {
                    new()
                    {
                        Id = "env-vars-defined",
                        Category = "file-exists",
                        Description = "Environment example should exist.",
                        Target = ".env.example",
                        Assertion = ".env.example exists with required keys.",
                        Automatable = true
                    }
                }
            };

            var session = new CodeGenSession
            {
                Id = sessionId,
                Prompt = "Build a todo app",
                NormalizedRequirement = "Build a todo app",
                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                SpecJson = JsonSerializer.Serialize(spec),
                GeneratedFilesJson = JsonSerializer.Serialize(existingFiles),
                Status = (int)CodeGenStatus.ValidationFailed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
            sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

            var service = CreateService(factory, config, templateRepo, sessionRepo);

            var result = await service.Repair(new TriggerRepairInput
            {
                SessionId = sessionId.ToString(),
                Failures = new List<ValidationResultDto>
                {
                    new() { Id = "env-vars-defined", Status = "failed", Message = "Required environment configuration is missing." }
                }
            });

            Assert.Contains("EXPECTED AFFECTED FILES", handler.LastUserPrompt);
            Assert.Contains(".env.example", handler.LastUserPrompt);
            Assert.Equal((int)CodeGenStatus.ValidationPassed, result.Status);
            Assert.Contains(result.GeneratedFiles, file => file.Path == ".env.example");
            Assert.Contains(result.ValidationResults, validation =>
                validation.Id == "env-vars-defined" && validation.Status == "passed");
        }

        private sealed class PromptCapturingHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _content;

            public PromptCapturingHttpMessageHandler(string content)
            {
                _content = content;
            }

            public string LastUserPrompt { get; private set; } = string.Empty;

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var requestJson = await request.Content.ReadAsStringAsync();
                using var requestDoc = JsonDocument.Parse(requestJson);
                LastUserPrompt = requestDoc.RootElement
                    .GetProperty("contents")[0]
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? string.Empty;

                var body = JsonSerializer.Serialize(new
                {
                    candidates = new[]
                    {
                        new
                        {
                            content = new
                            {
                                parts = new[]
                                {
                                    new { text = _content }
                                }
                            }
                        }
                    }
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
            }
        }

        private CodeGenAppService CreateService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IRepository<Template, int> templateRepository,
            IRepository<CodeGenSession, Guid> sessionRepository)
        {
            var claudeClient = Substitute.For<IClaudeApiClient>();
            var aiService = new CodeGenAiService(httpClientFactory, configuration, claudeClient);
            var uowManager = Substitute.For<Abp.Domain.Uow.IUnitOfWorkManager>();
            var sessionManager = new CodeGenSessionManager(sessionRepository, aiService, uowManager);
            var scaffolder = new CodeGenScaffolder();
            var validator = new CodeGenValidator();
            var planner = new CodeGenPlanner(aiService, sessionManager);
            var engine = new CodeGenEngine(aiService, planner, scaffolder, configuration);
            var refiner = new CodeGenRefiner(aiService, sessionManager, validator);
            var scopeFactory = Substitute.For<IServiceScopeFactory>();

            return new CodeGenAppService(
                sessionManager,
                engine,
                planner,
                refiner,
                aiService,
                templateRepository,
                scopeFactory);
        }
    }
}
