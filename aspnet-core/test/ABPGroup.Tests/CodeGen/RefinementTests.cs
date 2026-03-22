using System.Collections.Generic;
using System;
using System.Net.Http;
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
    public class RefinementTests
    {
        [Fact]
        public async Task RefineSession_ReturnsOnlyChangedFiles()
        {
            var sessionId = Guid.NewGuid();
            var refinementResponse = @"===SUMMARY===
Added a new status field to the Task entity and updated the form to include a dropdown.
===END SUMMARY===

===FILE===
src/app/tasks/page.tsx
===CONTENT===
'use client';

import { useState } from 'react';

interface Task {
  id: number;
  title: string;
  status: 'pending' | 'in-progress' | 'done';
}

export default function TasksPage() {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [input, setInput] = useState('');
  const [status, setStatus] = useState<'pending' | 'in-progress' | 'done'>('pending');

  const addTask = () => {
    if (!input.trim()) return;
    setTasks(prev => [...prev, { id: Date.now(), title: input, status }]);
    setInput('');
  };

  return (
    <main className=""p-8"">
      <h1 className=""text-2xl font-bold mb-4"">Tasks</h1>
      <div className=""flex gap-2 mb-4"">
        <input value={input} onChange={e => setInput(e.target.value)} className=""border p-2 rounded"" />
        <select value={status} onChange={e => setStatus(e.target.value as any)} className=""border p-2 rounded"">
          <option value=""pending"">Pending</option>
          <option value=""in-progress"">In Progress</option>
          <option value=""done"">Done</option>
        </select>
        <button onClick={addTask} className=""bg-blue-600 text-white px-4 py-2 rounded"">Add</button>
      </div>
      <ul>
        {tasks.map(t => (
          <li key={t.id} className=""py-1"">{t.title} - {t.status}</li>
        ))}
      </ul>
    </main>
  );
}
===END FILE===";

            var handler = new MockHttpMessageHandler(refinementResponse);
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
                Prompt = "Build a task app",
                NormalizedRequirement = "Build a task app",
                DetectedFeaturesJson = "[\"task management\"]",
                DetectedEntitiesJson = "[\"Task\"]",
                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                SpecJson = "{\"entities\":[{\"name\":\"Task\",\"fields\":[{\"name\":\"title\",\"type\":\"string\"}]}],\"pages\":[{\"route\":\"/tasks\"}],\"apiRoutes\":[],\"validations\":[]}",
                GeneratedFilesJson = "[{\"path\":\"src/app/tasks/page.tsx\",\"content\":\"'use client';\\n\\nimport { useState } from 'react';\\n\\ninterface Task {\\n  id: number;\\n  title: string;\\n  done: boolean;\\n}\\n\\nexport default function TasksPage() {\\n  const [tasks, setTasks] = useState<Task[]>([]);\\n  const [input, setInput] = useState('');\\n\\n  const addTask = () => {\\n    if (!input.trim()) return;\\n    setTasks(prev => [...prev, { id: Date.now(), title: input, done: false }]);\\n    setInput('');\\n  };\\n\\n  return (\\n    <main className=\\\"p-8\\\">\\n      <h1 className=\\\"text-2xl font-bold mb-4\\\">Tasks</h1>\\n      <div className=\\\"flex gap-2 mb-4\\\">\\n        <input value={input} onChange={e => setInput(e.target.value)} className=\\\"border p-2 rounded\\\" />\\n        <button onClick={addTask} className=\\\"bg-blue-600 text-white px-4 py-2 rounded\\\">Add</button>\\n      </div>\\n      <ul>\\n        {tasks.map(t => (\\n          <li key={t.id} className=\\\"py-1\\\">{t.title}</li>\\n        ))}\\n      </ul>\\n    </main>\\n  );\\n}\"}]",
                Status = (int)CodeGenStatus.ValidationPassed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
            sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

            var service = CreateService(factory, config, templateRepo, sessionRepo);

            var result = await service.RefineSession(new RefinementInputDto
            {
                SessionId = sessionId.ToString(),
                ChangeRequest = "Add a status field to tasks with options: pending, in-progress, done"
            });

            Assert.NotNull(result);
            Assert.NotEmpty(result.ChangedFiles);
            Assert.Single(result.ChangedFiles); // Only the changed file should be returned
            Assert.Equal("src/app/tasks/page.tsx", result.ChangedFiles[0].Path);
            Assert.Contains("status", result.ChangedFiles[0].Content);
            Assert.Contains("pending", result.ChangedFiles[0].Content);
            Assert.Contains("in-progress", result.ChangedFiles[0].Content);
            Assert.Contains("done", result.ChangedFiles[0].Content);
            Assert.Empty(result.DeletedFiles);
            Assert.NotNull(result.Summary);
        }

        [Fact]
        public async Task RefineSession_PreservesUnaffectedFiles()
        {
            var sessionId = Guid.NewGuid();
            var refinementResponse = @"===SUMMARY===
Added a footer component to the layout.
===END SUMMARY===

===FILE===
src/components/Footer.tsx
===CONTENT===
export default function Footer() {
  return (
    <footer className=""p-4 text-center"">
      <p>&copy; 2026 Task App</p>
    </footer>
  );
}
===END FILE===";

            var handler = new MockHttpMessageHandler(refinementResponse);
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
                new GeneratedFileDto { Path = "src/app/page.tsx", Content = "export default function Home() { return <div>Home</div>; }" },
                new GeneratedFileDto { Path = "src/app/tasks/page.tsx", Content = "export default function Tasks() { return <div>Tasks</div>; }" },
                new GeneratedFileDto { Path = "package.json", Content = "{\"name\":\"test\"}" }
            };

            var session = new CodeGenSession
            {
                Id = sessionId,
                Prompt = "Build a task app",
                NormalizedRequirement = "Build a task app",
                DetectedFeaturesJson = "[\"task management\"]",
                DetectedEntitiesJson = "[\"Task\"]",
                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                SpecJson = "{\"entities\":[{\"name\":\"Task\"}],\"pages\":[{\"route\":\"/\"},{\"route\":\"/tasks\"}],\"apiRoutes\":[],\"validations\":[]}",
                GeneratedFilesJson = System.Text.Json.JsonSerializer.Serialize(existingFiles),
                Status = (int)CodeGenStatus.ValidationPassed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
            sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

            var service = CreateService(factory, config, templateRepo, sessionRepo);

            var result = await service.RefineSession(new RefinementInputDto
            {
                SessionId = sessionId.ToString(),
                ChangeRequest = "Add a footer component"
            });

            Assert.NotNull(result);
            Assert.Single(result.ChangedFiles);
            Assert.Equal("src/components/Footer.tsx", result.ChangedFiles[0].Path);
            Assert.Empty(result.DeletedFiles);

            // Verify the session was updated with merged files
            await sessionRepo.Received(1).UpdateAsync(Arg.Any<CodeGenSession>());
        }

        [Fact]
        public async Task RefineSession_UpdatesSpecFileManifest()
        {
            var sessionId = Guid.NewGuid();
            var refinementResponse = @"===SUMMARY===
Added a new settings page.
===END SUMMARY===

===FILE===
src/app/settings/page.tsx
===CONTENT===
export default function SettingsPage() {
  return (
    <main className=""p-8"">
      <h1 className=""text-2xl font-bold"">Settings</h1>
    </main>
  );
}
===END FILE===";

            var handler = new MockHttpMessageHandler(refinementResponse);
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
                new GeneratedFileDto { Path = "src/app/page.tsx", Content = "export default function Home() { return <div>Home</div>; }" }
            };

            var session = new CodeGenSession
            {
                Id = sessionId,
                Prompt = "Build a task app",
                NormalizedRequirement = "Build a task app",
                DetectedFeaturesJson = "[\"task management\"]",
                DetectedEntitiesJson = "[\"Task\"]",
                ConfirmedStackJson = "{\"framework\":\"Next.js\",\"language\":\"TypeScript\"}",
                SpecJson = "{\"entities\":[{\"name\":\"Task\"}],\"pages\":[{\"route\":\"/\"}],\"apiRoutes\":[],\"validations\":[]}",
                GeneratedFilesJson = System.Text.Json.JsonSerializer.Serialize(existingFiles),
                Status = (int)CodeGenStatus.ValidationPassed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);
            sessionRepo.UpdateAsync(Arg.Any<CodeGenSession>())
                .Returns(callInfo => Task.FromResult(callInfo.Arg<CodeGenSession>()));

            var service = CreateService(factory, config, templateRepo, sessionRepo);

            var result = await service.RefineSession(new RefinementInputDto
            {
                SessionId = sessionId.ToString(),
                ChangeRequest = "Add a settings page"
            });

            Assert.NotNull(result);
            Assert.Single(result.ChangedFiles);
            Assert.Equal("src/app/settings/page.tsx", result.ChangedFiles[0].Path);

            // Verify the session was updated with the new file
            await sessionRepo.Received(1).UpdateAsync(Arg.Is<CodeGenSession>(s =>
                s.GenerationMode == "refinement" &&
                s.RefinementHistoryJson != null
            ));
        }

        [Fact]
        public async Task RefineSession_ThrowsWhenNotValidated()
        {
            var sessionId = Guid.NewGuid();

            var handler = new MockHttpMessageHandler("{}");
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
                Prompt = "Build a task app",
                Status = (int)CodeGenStatus.Generating, // Not yet validated
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            sessionRepo.FirstOrDefaultAsync(sessionId).Returns(session);

            var service = CreateService(factory, config, templateRepo, sessionRepo);

            await Assert.ThrowsAsync<Abp.UI.UserFriendlyException>(() =>
                service.RefineSession(new RefinementInputDto
                {
                    SessionId = sessionId.ToString(),
                    ChangeRequest = "Add a footer"
                }));
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