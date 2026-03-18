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
            // Arrange
            var factory = new MockHttpClientFactory(new MockHttpMessageHandler());
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Groq:ApiKey"] = "test-key",
                    ["Groq:Model"]  = "llama-3.3-70b-versatile"
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
            var result = await service.GenerateProjectAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Files);
            Assert.True(result.Files.Count > 0);
            Assert.False(string.IsNullOrEmpty(result.OutputPath));
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
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var content = "===ARCHITECTURE===\nA test app.\n===END ARCHITECTURE===\n===MODULES===\ntest\n===END MODULES===\n===FILE===\nREADME.md\n===CONTENT===\n# Test App\n===END FILE===";
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { content } } }
            });
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
