using Abp.Application.Services.Dto;
using ABPGroup.Projects;
using ABPGroup.Projects.Dto;
using ABPGroup.Prompts;
using ABPGroup.Prompts.Dto;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Prompts;

public class PromptAppService_Tests : ABPGroupTestBase
{
    private readonly IPromptAppService _promptAppService;
    private readonly IProjectAppService _projectAppService;

    public PromptAppService_Tests()
    {
        LoginAsHostAdmin();
        _promptAppService = Resolve<IPromptAppService>();
        _projectAppService = Resolve<IProjectAppService>();
    }

    [Fact]
    public async Task Prompt_Crud_Endpoints_Should_Work()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var project = await _projectAppService.CreateAsync(new CreateUpdateProjectDto
        {
            WorkspaceId = 1,
            Name = $"PromptProject-{suffix}",
            Prompt = "Initial prompt",
            PromptVersion = 1,
            PromptSubmittedAt = DateTime.UtcNow,
            Framework = Framework.NextJS,
            Language = ProgrammingLanguage.TypeScript,
            DatabaseOption = DatabaseOption.NeonPostgres,
            IncludeAuth = true,
            Status = ProjectStatus.PromptSubmitted
        });

        var createInput = new CreateUpdatePromptDto
        {
            ProjectId = project.Id,
            Content = "Refined prompt",
            Version = 2,
            SubmittedAt = DateTime.UtcNow
        };

        var created = await _promptAppService.CreateAsync(createInput);
        created.Id.ShouldBeGreaterThan(0);
        created.ProjectId.ShouldBe(project.Id);

        var fetched = await _promptAppService.GetAsync(new EntityDto<long>(created.Id));
        fetched.Content.ShouldBe(createInput.Content);

        var paged = await _promptAppService.GetAllAsync(new PagedPromptResultRequestDto
        {
            MaxResultCount = 20,
            SkipCount = 0,
            ProjectId = project.Id
        });
        paged.Items.ShouldContain(x => x.Id == created.Id);

        var updateInput = new CreateUpdatePromptDto
        {
            Id = created.Id,
            ProjectId = project.Id,
            Content = "Refined prompt updated",
            Version = 2,
            SubmittedAt = DateTime.UtcNow
        };

        var updated = await _promptAppService.UpdateAsync(updateInput);
        updated.Content.ShouldBe(updateInput.Content);

        await _promptAppService.DeleteAsync(new EntityDto<long>(created.Id));

        var deleted = await UsingDbContextAsync(async context =>
            await Task.FromResult(context.Prompts.FirstOrDefault(x => x.Id == created.Id)));
        deleted.ShouldBeNull();
    }
}