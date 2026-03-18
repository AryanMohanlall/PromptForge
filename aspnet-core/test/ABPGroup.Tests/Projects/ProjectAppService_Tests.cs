using Abp.Application.Services.Dto;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Projects;

public class ProjectAppService_Tests : ABPGroupTestBase
{
    private readonly IProjectAppService _projectAppService;

    public ProjectAppService_Tests()
    {
        LoginAsHostAdmin();
        _projectAppService = Resolve<IProjectAppService>();
    }

    [Fact]
    public async Task Project_Crud_Endpoints_Should_Work()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var createInput = new CreateUpdateProjectDto
        {
            WorkspaceId = 1,
            Name = $"Project-{suffix}",
            Prompt = "Build a todo app",
            PromptVersion = 1,
            PromptSubmittedAt = DateTime.UtcNow,
            Framework = Framework.NextJS,
            Language = ProgrammingLanguage.TypeScript,
            DatabaseOption = DatabaseOption.NeonPostgres,
            IncludeAuth = true,
            Status = ProjectStatus.Draft
        };

        var created = await _projectAppService.CreateAsync(createInput);
        created.Id.ShouldBeGreaterThan(0);
        created.Name.ShouldBe(createInput.Name);

        var fetched = await _projectAppService.GetAsync(new EntityDto<long>(created.Id));
        fetched.Id.ShouldBe(created.Id);

        var paged = await _projectAppService.GetAllAsync(new PagedProjectResultRequestDto
        {
            MaxResultCount = 20,
            SkipCount = 0,
            WorkspaceId = 1
        });
        paged.Items.ShouldContain(x => x.Id == created.Id);

        var updateInput = new CreateUpdateProjectDto
        {
            Id = created.Id,
            WorkspaceId = created.WorkspaceId,
            Name = $"Project-Updated-{suffix}",
            Prompt = "Build a kanban app",
            PromptVersion = 2,
            PromptSubmittedAt = DateTime.UtcNow,
            Framework = Framework.ReactVite,
            Language = ProgrammingLanguage.TypeScript,
            DatabaseOption = DatabaseOption.RenderPostgres,
            IncludeAuth = false,
            Status = ProjectStatus.CodeGenerationInProgress
        };

        var updated = await _projectAppService.UpdateAsync(updateInput);
        updated.Name.ShouldBe(updateInput.Name);
        updated.PromptVersion.ShouldBe(2);

        await _projectAppService.DeleteAsync(new EntityDto<long>(created.Id));

        var deleted = await UsingDbContextAsync(async context =>
            await Task.FromResult(context.Projects.FirstOrDefault(x => x.Id == created.Id)));
        deleted.ShouldBeNull();
    }
}