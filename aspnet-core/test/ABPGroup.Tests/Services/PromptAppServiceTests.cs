using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using ABPGroup.Prompts;
using ABPGroup.Prompts.Dto;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Services
{
    public class PromptAppServiceTests : ABPGroupTestBase
    {
        private readonly IPromptAppService _promptAppService;
        private readonly IProjectAppService _projectAppService;

        public PromptAppServiceTests()
        {
            _promptAppService = Resolve<IPromptAppService>();
            _projectAppService = Resolve<IProjectAppService>();
        }

        private async Task<long> CreateProjectAsync(string name = "Prompt Test Project")
        {
            var project = await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = name,
                Prompt = "Initial prompt"
            });
            return project.Id;
        }

        // ── CreateAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_SetsCreatedAtTimestamp()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);
            var projectId = await CreateProjectAsync("TimestampProject");

            var result = await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId,
                Content = "Test prompt content",
                Version = 99
            });

            result.CreatedAt.ShouldBeGreaterThan(before);
            result.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public async Task CreateAsync_PersistsToDatabase()
        {
            var projectId = await CreateProjectAsync("PersistProject");

            var created = await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId,
                Content = "Persist this prompt",
                Version = 2
            });

            var fetched = await _promptAppService.GetAsync(
                new Abp.Application.Services.Dto.EntityDto<long>(created.Id));

            fetched.Content.ShouldBe("Persist this prompt");
            fetched.Version.ShouldBe(2);
        }

        // ── GetAllAsync (filtering) ───────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_FilterByProjectId_ReturnsOnlyThatProject()
        {
            var projectId = await CreateProjectAsync("FilterProject");

            await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId,
                Content = "Belongs to filter project",
                Version = 10
            });

            var result = await _promptAppService.GetAllAsync(new PagedPromptResultRequestDto
            {
                ProjectId = projectId,
                MaxResultCount = 50,
                SkipCount = 0
            });

            result.Items.ShouldAllBe(p => p.ProjectId == projectId);
        }

        [Fact]
        public async Task GetAllAsync_FilterByKeyword_ReturnsMatchingContent()
        {
            var projectId = await CreateProjectAsync("KeywordFilterProject");

            await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId,
                Content = "UniqueSearchableKeyword12345",
                Version = 20
            });
            await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId,
                Content = "Other content here",
                Version = 21
            });

            var result = await _promptAppService.GetAllAsync(new PagedPromptResultRequestDto
            {
                Keyword = "UniqueSearchableKeyword12345",
                MaxResultCount = 50,
                SkipCount = 0
            });

            result.Items.ShouldAllBe(p => p.Content.Contains("UniqueSearchableKeyword12345"));
        }

        [Fact]
        public async Task GetAllAsync_NoFilter_ReturnsAllPrompts()
        {
            var projectId = await CreateProjectAsync("AllPromptsProject");

            await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId, Content = "Prompt A", Version = 30
            });
            await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId, Content = "Prompt B", Version = 31
            });

            var result = await _promptAppService.GetAllAsync(new PagedPromptResultRequestDto
            {
                MaxResultCount = 50, SkipCount = 0
            });

            result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ChangesContent()
        {
            var projectId = await CreateProjectAsync("UpdatePromptProject");
            var created = await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId,
                Content = "Original content",
                Version = 40
            });

            await _promptAppService.UpdateAsync(new CreateUpdatePromptDto
            {
                Id = created.Id,
                ProjectId = projectId,
                Content = "Updated content",
                Version = 40
            });

            var fetched = await _promptAppService.GetAsync(
                new Abp.Application.Services.Dto.EntityDto<long>(created.Id));

            fetched.Content.ShouldBe("Updated content");
        }

        // ── DeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_RemovesFromDatabase()
        {
            var projectId = await CreateProjectAsync("DeletePromptProject");
            var created = await _promptAppService.CreateAsync(new CreateUpdatePromptDto
            {
                ProjectId = projectId,
                Content = "Delete me",
                Version = 50
            });

            await _promptAppService.DeleteAsync(
                new Abp.Application.Services.Dto.EntityDto<long>(created.Id));

            await UsingDbContextAsync(async ctx =>
            {
                // Prompt does not implement ISoftDelete, so a physical delete removes the row
                var prompt = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .FirstOrDefaultAsync(ctx.Prompts, p => p.Id == created.Id);
                prompt.ShouldBeNull();
            });
        }
    }
}
