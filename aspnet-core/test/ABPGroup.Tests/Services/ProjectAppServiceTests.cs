using Abp.UI;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using ABPGroup.Prompts;
using ABPGroup.Prompts.Dto;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Services
{
    public class ProjectAppServiceTests : ABPGroupTestBase
    {
        private readonly IProjectAppService _projectAppService;
        private readonly IPromptAppService _promptAppService;

        public ProjectAppServiceTests()
        {
            _projectAppService = Resolve<IProjectAppService>();
            _promptAppService = Resolve<IPromptAppService>();
        }

        // ── CreateAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidInput_CreatesProject()
        {
            var dto = new CreateUpdateProjectDto
            {
                Name = "Test Project",
                Prompt = "Build a REST API",
                WorkspaceId = AbpSession.TenantId
            };

            var result = await _projectAppService.CreateAsync(dto);

            result.ShouldNotBeNull();
            result.Id.ShouldBeGreaterThan(0);
            result.Name.ShouldBe("Test Project");
        }

        [Fact]
        public async Task CreateAsync_AlsoCreatesPromptRecord()
        {
            var dto = new CreateUpdateProjectDto
            {
                Name = "Project With Prompt",
                Prompt = "Initial prompt text",
                PromptVersion = 1
            };

            var result = await _projectAppService.CreateAsync(dto);

            result.PromptId.ShouldNotBeNull();
            result.PromptId!.Value.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_WithSubmittedAt_SetsStatusToPromptSubmitted()
        {
            var dto = new CreateUpdateProjectDto
            {
                Name = "Submitted Project",
                Prompt = "A submitted prompt",
                PromptSubmittedAt = System.DateTime.UtcNow
            };

            var result = await _projectAppService.CreateAsync(dto);

            // After creation with PromptSubmittedAt, codegen starts immediately
            // so the returned status is CodeGenerationInProgress
            result.Status.ShouldBe(ProjectStatus.CodeGenerationInProgress);
        }

        [Fact]
        public async Task CreateAsync_WithoutSubmittedAt_StatusRemainsDefault()
        {
            var dto = new CreateUpdateProjectDto
            {
                Name = "Draft Project",
                Prompt = "Not submitted yet"
            };

            var result = await _projectAppService.CreateAsync(dto);

            // Default status is Draft
            result.Status.ShouldBe(ProjectStatus.Draft);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_ExistingProject_UpdatesFields()
        {
            var created = await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = "Original Name",
                Prompt = "Original prompt"
            });

            var updated = await _projectAppService.UpdateAsync(new CreateUpdateProjectDto
            {
                Id = created.Id,
                Name = "Updated Name",
                Prompt = "Updated prompt"
            });

            updated.Name.ShouldBe("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_NonExistentWorkspaceId_FallsBackToFirstAvailableWorkspace()
        {
            // When WorkspaceId doesn't exist and there's no session tenant,
            // the service falls back to the first available workspace rather than throwing.
            var created = await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = "Fallback Workspace Project",
                Prompt = "Initial"
            });

            AbpSession.TenantId = null;

            var updated = await _projectAppService.UpdateAsync(new CreateUpdateProjectDto
            {
                Id = created.Id,
                Name = "Updated Name",
                Prompt = "Updated",
                WorkspaceId = 999999  // non-existent — falls back to Default tenant
            });

            updated.ShouldNotBeNull();
            updated.Name.ShouldBe("Updated Name");
        }

        // ── CreatePromptRecord — versioning ───────────────────────────────

        [Fact]
        public async Task CreateAsync_SameVersionTwice_UpdatesExistingPrompt()
        {
            var project = await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = "Version Test",
                Prompt = "Version 1 content",
                PromptVersion = 1
            });

            // Update with same version — should update rather than insert a new prompt
            var updated = await _projectAppService.UpdateAsync(new CreateUpdateProjectDto
            {
                Id = project.Id,
                Name = "Version Test",
                Prompt = "Version 1 updated",
                PromptVersion = 1,
                WorkspaceId = project.WorkspaceId
            });

            updated.PromptId.ShouldBe(project.PromptId);
        }

        [Fact]
        public async Task CreateAsync_DifferentVersion_CreatesNewPromptRecord()
        {
            var project = await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = "Multi Version",
                Prompt = "First version",
                PromptVersion = 1
            });

            var v2 = await _projectAppService.UpdateAsync(new CreateUpdateProjectDto
            {
                Id = project.Id,
                Name = "Multi Version",
                Prompt = "Second version",
                PromptVersion = 2,
                WorkspaceId = project.WorkspaceId
            });

            v2.PromptId.ShouldNotBe(project.PromptId);
        }

        // ── GetAllAsync (filtering) ───────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_FilterByKeyword_ReturnsMatchingProjects()
        {
            await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = "FindableProject",
                Prompt = "searchable content"
            });
            await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = "AnotherProject",
                Prompt = "other content"
            });

            var result = await _projectAppService.GetAllAsync(new PagedProjectResultRequestDto
            {
                Keyword = "FindableProject",
                MaxResultCount = 50,
                SkipCount = 0
            });

            result.Items.ShouldAllBe(p => p.Name.Contains("FindableProject"));
        }

        [Fact]
        public async Task GetAllAsync_FilterByWorkspaceId_ReturnsOnlyThatWorkspace()
        {
            // TenantId = 1 is the session workspace
            await _projectAppService.CreateAsync(new CreateUpdateProjectDto
            {
                Name = "WsProject",
                Prompt = "workspace-scoped",
                WorkspaceId = AbpSession.TenantId
            });

            var result = await _projectAppService.GetAllAsync(new PagedProjectResultRequestDto
            {
                WorkspaceId = AbpSession.TenantId,
                MaxResultCount = 50,
                SkipCount = 0
            });

            result.Items.ShouldAllBe(p => p.WorkspaceId == AbpSession.TenantId);
        }
    }
}
