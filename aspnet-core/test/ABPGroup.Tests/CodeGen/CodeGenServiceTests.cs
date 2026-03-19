using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ABPGroup.CodeGen;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ABPGroup.Tests.CodeGen
{
    public class CodeGenServiceTests
    {
        [Fact]
        public async Task GenerateProjectAsync_ReturnsResult_WithValidInput()
        {
            // Arrange — mock returns app-specific files (scaffold handles boilerplate)
            var mockResponse = @"===ARCHITECTURE===
A todo app with task management.
===END ARCHITECTURE===
===MODULES===
tasks,dashboard
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

            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "codegen-test-" + System.Guid.NewGuid().ToString("N")[..8]);
            var factory = new MockHttpClientFactory(new MockHttpMessageHandler(mockResponse));
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Groq:ApiKey"]         = "test-key",
                    ["Groq:Model"]          = "llama-3.3-70b-versatile",
                    ["CodeGen:OutputPath"]  = tempDir,
                    ["CodeGen:SkipBuild"]   = "true"
                })
                .Build();

            var service = new CodeGenAppService(factory, config);
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
            Assert.Contains("tasks", result.ModuleList);

            // Verify scaffold files are present
            Assert.Contains(result.Files, f => f.Path == "package.json");
            Assert.Contains(result.Files, f => f.Path == "tsconfig.json");
            Assert.Contains(result.Files, f => f.Path == "next.config.ts");
            Assert.Contains(result.Files, f => f.Path == "src/app/layout.tsx");
            Assert.Contains(result.Files, f => f.Path == "src/lib/auth.ts");
            Assert.Contains(result.Files, f => f.Path == "prisma/schema.prisma");

            // Verify LLM-generated files are present
            Assert.Contains(result.Files, f => f.Path == "src/app/page.tsx");
            Assert.Contains(result.Files, f => f.Path == "src/app/tasks/page.tsx");
        }

        [Fact]
        public async Task GenerateProjectAsync_NonNextJS_SkipsScaffold()
        {
            var mockResponse = @"===ARCHITECTURE===
A React Vite app.
===END ARCHITECTURE===
===MODULES===
app
===END MODULES===
===FILE===
package.json
===CONTENT===
{""name"":""test"",""scripts"":{""build"":""vite build""}}
===END FILE===
===FILE===
src/App.tsx
===CONTENT===
export default function App() { return <div>Hello</div>; }
===END FILE===";

            var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "codegen-test-" + System.Guid.NewGuid().ToString("N")[..8]);
            var factory = new MockHttpClientFactory(new MockHttpMessageHandler(mockResponse));
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Groq:ApiKey"]        = "test-key",
                    ["CodeGen:OutputPath"] = tempDir,
                    ["CodeGen:SkipBuild"]  = "true"
                })
                .Build();

            var service = new CodeGenAppService(factory, config);
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
            // No scaffold files for non-NextJS — all files come from LLM
            Assert.Equal(2, result.Files.Count);
            Assert.Contains(result.Files, f => f.Path == "package.json");
            Assert.Contains(result.Files, f => f.Path == "src/App.tsx");
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

    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;

        public MockHttpMessageHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { content = _content } } }
            });

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
