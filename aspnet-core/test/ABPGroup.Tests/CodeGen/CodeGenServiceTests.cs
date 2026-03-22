using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ABPGroup.CodeGen;
using ABPGroup.CodeGen.Dto;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using ABPGroup.Templates;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace ABPGroup.Tests.CodeGen
{
    public class CodeGenServiceTests
    {
        [Fact]
        public async Task CreateSession_InsertsNewSession_InsteadOfUpdating()
        {
            var createSessionResponse = @"===PROJECT_NAME===
todo-app
===END PROJECT_NAME===
===NORMALIZED_REQUIREMENT===
Build a todo app with tasks and statuses.
===END NORMALIZED_REQUIREMENT===
===DETECTED_FEATURES===
task management, status tracking
===END DETECTED_FEATURES===
===DETECTED_ENTITIES===
task
===END DETECTED_ENTITIES===";

            var handler = new SequentialMockHttpMessageHandler(createSessionResponse);
            var factory = new MockHttpClientFactory(handler);
            var config = new ConfigurationBuilder().Build();
            var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();
            var templateRepo = Substitute.For<IRepository<Template, int>>();

            sessionRepo.InsertAsync(Arg.Any<CodeGenSession>())
                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

            var service = CreateService(factory, config, templateRepo, sessionRepo);

            var result = await service.CreateSession(new CreateSessionInput
            {
                Prompt = "Build a todo app"
            });

            Assert.False(string.IsNullOrWhiteSpace(result.Id));
            await sessionRepo.Received(1).InsertAsync(Arg.Any<CodeGenSession>());
            await sessionRepo.DidNotReceive().UpdateAsync(Arg.Any<CodeGenSession>());
        }

        [Fact]
        public async Task GenerateSpec_ParsesNestedApiRouteShapes()
        {
                var sessionId = System.Guid.NewGuid();
                var readmeResponse = @"===README===
        # Task App
        ===END README===

        ===SUMMARY===
        Task app summary
        ===END SUMMARY===";

                var specResponse = @"===SPEC_JSON===
        {
        ""entities"": [
        {
        ""name"": ""Task"",
        ""tableName"": ""tasks"",
        ""fields"": [
        {
            ""name"": ""title"",
            ""type"": ""string"",
            ""required"": true,
            ""description"": ""Task title""
        }
        ],
        ""relations"": []
        }
        ],
        ""pages"": [
        {
        ""route"": ""/"",
        ""name"": ""Home"",
        ""layout"": ""public"",
        ""components"": [],
        ""dataRequirements"": [],
        ""description"": ""Landing page""
        }
        ],
        ""apiRoutes"": [
        {
        ""method"": ""GET"",
        ""path"": ""/api/tasks"",
        ""handler"": ""tasks.getAll"",
        ""requestBody"": {
        ""filter"": {
            ""status"": ""string""
        }
        },
        ""responseShape"": {
        ""items"": {
            ""id"": ""string"",
            ""title"": ""string""
        },
        ""meta"": {
            ""count"": ""number""
        }
        },
        ""auth"": true,
        ""description"": ""List tasks""
        }
        ],
        ""validations"": [],
        ""fileManifest"": [""src/app/page.tsx""]
        }
        ===END SPEC_JSON===";

                var handler = new SequentialMockHttpMessageHandler(readmeResponse, specResponse);                        var factory = new MockHttpClientFactory(handler);
                        var config = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                {
                                        ["Groq:ApiKey"] = "test-key"
                                })
                                .Build();

                        var templateRepo = Substitute.For<IRepository<Template, int>>();
                        var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();

                        var session = new CodeGenSession
                        {
                                Id = sessionId,
                                Prompt = "Build a task app",
                                NormalizedRequirement = "Build a task app",
                                DetectedFeaturesJson = "[\"task management\"]",
                                DetectedEntitiesJson = "[\"Task\"]",
                                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                                Status = (int)CodeGenStatus.StackConfirmed,
                                CreatedAt = System.DateTime.UtcNow,
                                UpdatedAt = System.DateTime.UtcNow,
                        };

                        sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
                        sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));
                        sessionRepo.InsertAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

                        var service = CreateService(factory, config, templateRepo, sessionRepo);

                        var result = await service.GenerateSpec(sessionId.ToString());

                        Assert.NotNull(result.Spec);
                        Assert.Single(result.Spec.Entities);
                        Assert.Single(result.Spec.Pages);
                        Assert.Single(result.Spec.ApiRoutes);
                        Assert.NotNull(result.Spec.ApiRoutes[0].ResponseShape);
                        Assert.Single(result.Spec.FileManifest);
                        Assert.Equal("src/app/page.tsx", result.Spec.FileManifest[0].Path);
                }

                [Fact]
                public async Task GenerateSpec_FillsMissingPagesAndValidations()
                {
                        var sessionId = System.Guid.NewGuid();
                        var readmeResponse = @"===README===
# Todos App
===END README===

===SUMMARY===
Todos app summary
===END SUMMARY===";

                        var specResponse = @"===SPEC_JSON===
{
    ""entities"": [
        {
            ""name"": ""User"",
            ""tableName"": ""users"",
            ""fields"": [],
            ""relations"": []
        }
    ],
    ""pages"": [],
    ""apiRoutes"": [
        {
            ""method"": ""GET"",
            ""path"": ""/api/todos"",
            ""handler"": ""todos.getAll"",
            ""responseShape"": {
                ""items"": []
            },
            ""auth"": true,
            ""description"": ""List todos""
        }
    ],
    ""validations"": [],
    ""fileManifest"": [
        {
            ""path"": ""src/app/todos/page.tsx"",
            ""type"": ""generated"",
            ""description"": ""Todos page""
        }
    ]
}
===END SPEC_JSON===";

                        var handler = new SequentialMockHttpMessageHandler(readmeResponse, specResponse);
                        var factory = new MockHttpClientFactory(handler);
                        var config = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                {
                                        ["Groq:ApiKey"] = "test-key"
                                })
                                .Build();

                        var templateRepo = Substitute.For<IRepository<Template, int>>();
                        var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();

                        var session = new CodeGenSession
                        {
                                Id = sessionId,
                                Prompt = "Build a todos app",
                                NormalizedRequirement = "Build a todos app",
                                DetectedFeaturesJson = "[\"todo management\"]",
                                DetectedEntitiesJson = "[\"User\"]",
                                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                                Status = (int)CodeGenStatus.StackConfirmed,
                                CreatedAt = System.DateTime.UtcNow,
                                UpdatedAt = System.DateTime.UtcNow,
                        };

                        sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
                        sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));
                        sessionRepo.InsertAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

                        var service = CreateService(factory, config, templateRepo, sessionRepo);

                        var result = await service.GenerateSpec(sessionId.ToString());

                        Assert.NotNull(result.Spec);
                        Assert.NotEmpty(result.Spec.Pages);
                        Assert.NotEmpty(result.Spec.Validations);
                        Assert.Contains(result.Spec.Pages, p => p.Route == "/todos");
                        Assert.Contains(result.Spec.Validations, v => v.Category == "build-passes");
                }

                [Fact]
                public async Task GenerateReadme_GeneratesPlanFromApprovedReadme()
                {
                        var sessionId = System.Guid.NewGuid();
                        var readmeResponse = @"===README===
# Reviewed App

## Features
- Watchlist management
- Authenticated dashboard

===END README===

===SUMMARY===
A watchlist application with authenticated CRUD flows.
===END SUMMARY===";

                        var planResponse = @"===SPEC_JSON===
{
  ""architectureNotes"": ""Use the approved README as the source of truth."",
  ""entities"": [
    {
      ""name"": ""WatchlistItem"",
      ""tableName"": ""watchlist_items"",
      ""fields"": [
        {
          ""name"": ""symbol"",
          ""type"": ""string"",
          ""required"": true,
          ""description"": ""Ticker symbol""
        }
      ],
      ""relations"": []
    }
  ],
  ""pages"": [
    {
      ""route"": ""/watchlist"",
      ""name"": ""Watchlist"",
      ""layout"": ""authenticated"",
      ""components"": [""WatchlistTable""],
      ""dataRequirements"": [""watchlistItems.symbol""],
      ""description"": ""Main watchlist page""
    }
  ],
  ""apiRoutes"": [
    {
      ""method"": ""GET"",
      ""path"": ""/api/watchlist"",
      ""handler"": ""watchlist.getAll"",
      ""responseShape"": {
        ""items"": []
      },
      ""auth"": true,
      ""description"": ""Load watchlist items""
    }
  ],
  ""validations"": [],
  ""fileManifest"": [],
  ""dependencyPlan"": {
    ""dependencies"": [],
    ""devDependencies"": [],
    ""envVars"": {
      ""DATABASE_URL"": ""PostgreSQL connection string""
    }
  }
}
===END SPEC_JSON===";

                        var handler = new SequentialMockHttpMessageHandler(readmeResponse, planResponse);
                        var factory = new MockHttpClientFactory(handler);
                        var config = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                {
                                        ["Gemini:ApiKey"] = "test-key"
                                })
                                .Build();

                        var templateRepo = Substitute.For<IRepository<Template, int>>();
                        var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();

                        var session = new CodeGenSession
                        {
                                Id = sessionId,
                                ProjectName = "Watchlist App",
                                Prompt = "Build a watchlist app",
                                NormalizedRequirement = "Build a watchlist app",
                                DetectedFeaturesJson = "[\"watchlist management\"]",
                                DetectedEntitiesJson = "[\"WatchlistItem\"]",
                                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                                Status = (int)CodeGenStatus.StackConfirmed,
                                CreatedAt = System.DateTime.UtcNow,
                                UpdatedAt = System.DateTime.UtcNow,
                        };

                        sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
                        sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));
                        sessionRepo.InsertAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

                        var service = CreateService(factory, config, templateRepo, sessionRepo);

                        var result = await service.GenerateReadme(sessionId.ToString());

                        Assert.NotNull(result);
                        Assert.Contains("Reviewed App", result.ReadmeMarkdown);
                        Assert.NotNull(result.Plan);
                        Assert.Contains(result.Plan.Entities, entity => entity.Name == "WatchlistItem");
                        Assert.Contains(result.Plan.Pages, page => page.Route == "/watchlist");
                        Assert.Contains("reviewed app", session.SpecJson, System.StringComparison.OrdinalIgnoreCase);
                }

                [Fact]
                public async Task GenerateReadme_EnrichesSparsePlanFromDetectedEntities()
                {
                        var sessionId = System.Guid.NewGuid();
                        var readmeResponse = @"===README===
# Todo App

## Features
- Create tasks
- Mark tasks complete

## Pages and Navigation
- /

## API Endpoints Overview
- Client-only app, no backend API routes required.

===END README===

===SUMMARY===
A lightweight todo application that stores tasks in client state.
===END SUMMARY===";

                        var planResponse = @"===SPEC_JSON===
{
  ""architectureNotes"": ""Client-only todo app."",
  ""entities"": [],
  ""pages"": [
    {
      ""route"": ""/"",
      ""name"": ""Home"",
      ""layout"": ""public"",
      ""components"": [],
      ""dataRequirements"": [],
      ""description"": ""Landing page""
    }
  ],
  ""apiRoutes"": [],
  ""validations"": [],
  ""fileManifest"": [],
  ""dependencyPlan"": {
    ""dependencies"": [],
    ""devDependencies"": [],
    ""envVars"": {}
  }
}
===END SPEC_JSON===";

                        var handler = new SequentialMockHttpMessageHandler(readmeResponse, planResponse);
                        var factory = new MockHttpClientFactory(handler);
                        var config = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                {
                                        ["Gemini:ApiKey"] = "test-key"
                                })
                                .Build();

                        var templateRepo = Substitute.For<IRepository<Template, int>>();
                        var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();

                        var session = new CodeGenSession
                        {
                                Id = sessionId,
                                ProjectName = "Todo App",
                                Prompt = "Build a simple todo app without a database",
                                NormalizedRequirement = "Build a simple todo app without a database",
                                DetectedFeaturesJson = "[\"task management\",\"client-side state\"]",
                                DetectedEntitiesJson = "[\"Task\"]",
                                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                                Status = (int)CodeGenStatus.StackConfirmed,
                                CreatedAt = System.DateTime.UtcNow,
                                UpdatedAt = System.DateTime.UtcNow,
                        };

                        sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
                        sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));
                        sessionRepo.InsertAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

                        var service = CreateService(factory, config, templateRepo, sessionRepo);

                        var result = await service.GenerateReadme(sessionId.ToString());

                        Assert.NotNull(result.Plan);
                        Assert.Contains(result.Plan.Entities, entity => entity.Name == "Task");
                        Assert.Contains(result.Plan.Entities[0].Fields, field => field.Name == "title");
                        Assert.Contains(result.Plan.Pages, page => page.Route == "/");
                        Assert.Empty(result.Plan.ApiRoutes);
                }

                [Fact]
                public async Task GenerateReadme_AddsHomePageWhenPlanMissesRootRoute()
                {
                        var sessionId = System.Guid.NewGuid();
                        var readmeResponse = @"===README===
# Notes Board

## Features
- Create quick notes
- Pin important notes

## Pages and Navigation
- /notes

===END README===

===SUMMARY===
A lightweight notes board for client-side note taking.
===END SUMMARY===";

                        var planResponse = @"===SPEC_JSON===
{
  ""architectureNotes"": ""Client-only notes app."",
  ""entities"": [
    {
      ""name"": ""Note"",
      ""tableName"": ""note"",
      ""fields"": [
        { ""name"": ""title"", ""type"": ""string"", ""required"": true, ""unique"": false }
      ],
      ""relations"": []
    }
  ],
  ""pages"": [
    {
      ""route"": ""/notes"",
      ""name"": ""Notes"",
      ""layout"": ""authenticated"",
      ""components"": [],
      ""dataRequirements"": [],
      ""description"": ""Main notes workspace.""
    }
  ],
  ""apiRoutes"": [],
  ""validations"": [],
  ""fileManifest"": [],
  ""dependencyPlan"": {
    ""dependencies"": [],
    ""devDependencies"": [],
    ""envVars"": {}
  }
}
===END SPEC_JSON===";

                        var handler = new SequentialMockHttpMessageHandler(readmeResponse, planResponse);
                        var factory = new MockHttpClientFactory(handler);
                        var config = new ConfigurationBuilder()
                                .AddInMemoryCollection(new Dictionary<string, string>
                                {
                                        ["Gemini:ApiKey"] = "test-key"
                                })
                                .Build();

                        var templateRepo = Substitute.For<IRepository<Template, int>>();
                        var sessionRepo = Substitute.For<IRepository<CodeGenSession, Guid>>();

                        var session = new CodeGenSession
                        {
                                Id = sessionId,
                                ProjectName = "Notes Board",
                                Prompt = "Build a simple notes app without a database",
                                NormalizedRequirement = "Build a simple notes app without a database",
                                DetectedFeaturesJson = "[\"notes\",\"client-side state\"]",
                                DetectedEntitiesJson = "[\"Note\"]",
                                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                                Status = (int)CodeGenStatus.StackConfirmed,
                                CreatedAt = System.DateTime.UtcNow,
                                UpdatedAt = System.DateTime.UtcNow,
                        };

                        sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
                        sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));
                        sessionRepo.InsertAsync(Arg.Any<CodeGenSession>())
                                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

                        var service = CreateService(factory, config, templateRepo, sessionRepo);

                        var result = await service.GenerateReadme(sessionId.ToString());

                        Assert.NotNull(result.Plan);
                        Assert.Contains(result.Plan.Pages, page => page.Route == "/");
                        Assert.Contains(result.Plan.Pages, page => page.Route == "/notes");
                        Assert.Contains(result.Plan.Pages, page => page.Route == "/" && page.Name == "Home");
                }

                [Fact]
        public async Task GenerateProjectAsync_ReturnsResult_WithValidInput()
        {
            // Phase 1: Requirements analysis response
            var requirementsResponse = @"===FEATURES===
task-management, dashboard, todo-list
===END FEATURES===
===ARCHITECTURE===
A todo app with task management.
===END ARCHITECTURE===
===PAGES===
home, tasks, dashboard
===END PAGES===
===API_ENDPOINTS===
GET /api/tasks, POST /api/tasks, DELETE /api/tasks/:id
===END API_ENDPOINTS===
===DB_ENTITIES===
Task(id, title, done, createdAt)
===END DB_ENTITIES===";

            // Phase 4: Frontend response
            var frontendResponse = @"===ARCHITECTURE===
Frontend with pages and components.
===END ARCHITECTURE===
===MODULES===
home,tasks
===END MODULES===
===FILE===
src/app/page.tsx
===CONTENT===
import Link from 'next/link';

export default function Home() {
  return (
    <main className=""min-h-screen p-8"">
      <h1 className=""text-3xl font-bold"">Todo App</h1>
      <Link href=""/tasks"" className=""text-blue-600 underline"">Go to Tasks</Link>
    </main>
  );
}
===END FILE===
===FILE===
src/app/tasks/page.tsx
===CONTENT===
'use client';

import { useState } from 'react';

interface Task {
  id: number;
  title: string;
  done: boolean;
}

export default function TasksPage() {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [input, setInput] = useState('');

  const addTask = () => {
    if (!input.trim()) return;
    setTasks(prev => [...prev, { id: Date.now(), title: input, done: false }]);
    setInput('');
  };

  return (
    <main className=""p-8"">
      <h1 className=""text-2xl font-bold mb-4"">Tasks</h1>
      <div className=""flex gap-2 mb-4"">
        <input value={input} onChange={e => setInput(e.target.value)} className=""border p-2 rounded"" />
        <button onClick={addTask} className=""bg-blue-600 text-white px-4 py-2 rounded"">Add</button>
      </div>
      <ul>
        {tasks.map(t => (
          <li key={t.id} className=""py-1"">{t.title}</li>
        ))}
      </ul>
    </main>
  );
}
===END FILE===";

            // Phase 5: Backend response
            var backendResponse = @"===ARCHITECTURE===
API routes for task CRUD.
===END ARCHITECTURE===
===MODULES===
api
===END MODULES===
===FILE===
src/app/api/tasks/route.ts
===CONTENT===
import { NextResponse } from 'next/server';

const tasks: any[] = [];

export async function GET() {
  return NextResponse.json(tasks);
}

export async function POST(request: Request) {
  const body = await request.json();
  tasks.push({ id: Date.now(), ...body });
  return NextResponse.json(tasks);
}
===END FILE===";

            // Phase 6: Database response
            var dbResponse = @"===ARCHITECTURE===
Prisma database layer.
===END ARCHITECTURE===
===MODULES===
database
===END MODULES===
===FILE===
src/lib/db.ts
===CONTENT===
import { PrismaClient } from '@prisma/client';

const globalForPrisma = globalThis as unknown as { prisma: PrismaClient };
export const prisma = globalForPrisma.prisma || new PrismaClient();
if (process.env.NODE_ENV !== 'production') globalForPrisma.prisma = prisma;
===END FILE===";

            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "codegen-test-" + System.Guid.NewGuid().ToString("N")[..8]);
            var handler = new SequentialMockHttpMessageHandler(
                requirementsResponse, frontendResponse, backendResponse, dbResponse);
            var factory = new MockHttpClientFactory(handler);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Groq:ApiKey"]         = "test-key",
                    ["Groq:Model"]          = "llama-3.3-70b-versatile",
                    ["CodeGen:OutputPath"]  = tempDir,
                    ["CodeGen:SkipBuild"]   = "true"
                })
                .Build();

            var templateRepo = Substitute.For<IRepository<Template, int>>();
            templateRepo.GetAllListAsync().Returns(new List<Template>());
            var service = CreateService(factory, config, templateRepo);
            var request = new CreateUpdateProjectDto
            {
                Id = 1,
                WorkspaceId = 1,
                PromptId = 1,
                Name = "TestApp",
                Prompt = "A simple todo app",
                PromptVersion = 1,
                PromptSubmittedAt = System.DateTime.UtcNow,
                Framework = Framework.NextJS,
                Language = ProgrammingLanguage.TypeScript,
                DatabaseOption = DatabaseOption.RenderPostgres,
                IncludeAuth = true,
                Status = ProjectStatus.PromptSubmitted
            };

            // Act
            CodeGenResult result;
            try
            {
                result = await service.GenerateProjectAsync(request);
            }
            finally
            {
                if (System.IO.Directory.Exists(tempDir))
                    System.IO.Directory.Delete(tempDir, true);
            }

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Files);
            Assert.True(result.Files.Count > 0, "Should have generated files");
            Assert.False(string.IsNullOrEmpty(result.OutputPath));
            Assert.Equal("A todo app with task management.", result.ArchitectureSummary);

            // Verify scaffold files are present
            Assert.Contains(result.Files, f => f.Path == "package.json");
            Assert.Contains(result.Files, f => f.Path == "tsconfig.json");
            Assert.Contains(result.Files, f => f.Path == "next.config.ts");
            Assert.Contains(result.Files, f => f.Path == "src/app/layout.tsx");
            Assert.Contains(result.Files, f => f.Path == "src/lib/auth.ts");
            Assert.Contains(result.Files, f => f.Path == "prisma/schema.prisma");

            // Verify LLM-generated frontend files are present
            Assert.Contains(result.Files, f => f.Path == "src/app/page.tsx");
            Assert.Contains(result.Files, f => f.Path == "src/app/tasks/page.tsx");

            // Verify LLM-generated backend files are present
            Assert.Contains(result.Files, f => f.Path == "src/app/api/tasks/route.ts");

            // Verify LLM-generated database files are present
            Assert.Contains(result.Files, f => f.Path == "src/lib/db.ts");
        }

        [Fact]
        public async Task GenerateProjectAsync_NonNextJS_IncludesAllPhases()
        {
            var requirementsResponse = @"===FEATURES===
app
===END FEATURES===
===ARCHITECTURE===
A React Vite app.
===END ARCHITECTURE===
===PAGES===
home
===END PAGES===
===API_ENDPOINTS===
GET /api/data
===END API_ENDPOINTS===
===DB_ENTITIES===
Item(id, name)
===END DB_ENTITIES===";

            var frontendResponse = @"===ARCHITECTURE===
Frontend.
===END ARCHITECTURE===
===MODULES===
app
===END MODULES===
===FILE===
src/App.tsx
===CONTENT===
export default function App() { return <div>Hello</div>; }
===END FILE===";

            var backendResponse = @"===ARCHITECTURE===
Backend.
===END ARCHITECTURE===
===MODULES===
api
===END MODULES===
===FILE===
server/index.ts
===CONTENT===
console.log('server');
===END FILE===";

            var dbResponse = @"===ARCHITECTURE===
Database.
===END ARCHITECTURE===
===MODULES===
db
===END MODULES===
===FILE===
src/lib/db.ts
===CONTENT===
export const db = {};
===END FILE===";

            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "codegen-test-" + System.Guid.NewGuid().ToString("N")[..8]);
            var handler = new SequentialMockHttpMessageHandler(
                requirementsResponse, frontendResponse, backendResponse, dbResponse);
            var factory = new MockHttpClientFactory(handler);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Groq:ApiKey"]        = "test-key",
                    ["CodeGen:OutputPath"] = tempDir,
                    ["CodeGen:SkipBuild"]  = "true"
                })
                .Build();

            var templateRepo = Substitute.For<IRepository<Template, int>>();
            templateRepo.GetAllListAsync().Returns(new List<Template>());
            var service = CreateService(factory, config, templateRepo);
            var request = new CreateUpdateProjectDto
            {
                Id = 2,
                Name = "ViteApp",
                Prompt = "A simple app",
                Framework = Framework.ReactVite,
                Language = ProgrammingLanguage.TypeScript,
                DatabaseOption = DatabaseOption.RenderPostgres,
                IncludeAuth = false,
                Status = ProjectStatus.PromptSubmitted
            };

            CodeGenResult result;
            try
            {
                result = await service.GenerateProjectAsync(request);
            }
            finally
            {
                if (System.IO.Directory.Exists(tempDir))
                    System.IO.Directory.Delete(tempDir, true);
            }

            Assert.NotNull(result);
            Assert.True(result.Files.Count > 0, "Should have generated files");
            // Should include scaffold + frontend + backend + db files
            Assert.Contains(result.Files, f => f.Path == "src/App.tsx");
            Assert.Contains(result.Files, f => f.Path == "server/index.ts");
            Assert.Contains(result.Files, f => f.Path == "src/lib/db.ts");
        }

        private CodeGenAppService CreateService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IRepository<Template, int> templateRepository = null,
            IRepository<CodeGenSession, Guid> sessionRepository = null)
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

    public class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public MockHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name) => new HttpClient(_handler);
    }

    /// <summary>Returns different responses for each sequential HTTP call (phases 1, 4, 5, 6).</summary>
    public class SequentialMockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string[] _responses;
        private int _callIndex;

        public SequentialMockHttpMessageHandler(params string[] responses)
        {
            _responses = responses;
            _callIndex = 0;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var index = Interlocked.Increment(ref _callIndex) - 1;
            var content = index < _responses.Length
                ? _responses[index]
                : _responses[_responses.Length - 1]; // fallback to last response

            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new { text = content }
                            }
                        }
                    }
                }
            });

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }

    /// <summary>Legacy mock that returns the same response for every call.</summary>
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;

        public MockHttpMessageHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = System.Text.Json.JsonSerializer.Serialize(new
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

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
