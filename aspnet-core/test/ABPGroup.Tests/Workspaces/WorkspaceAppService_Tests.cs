using Abp.Application.Services.Dto;
using ABPGroup.Workspaces;
using ABPGroup.Workspaces.Dto;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Workspaces;

public class WorkspaceAppService_Tests : ABPGroupTestBase
{
    private readonly IWorkspaceAppService _workspaceAppService;

    public WorkspaceAppService_Tests()
    {
        LoginAsHostAdmin();
        _workspaceAppService = Resolve<IWorkspaceAppService>();
    }

    [Fact]
    public async Task Workspace_Crud_Endpoints_Should_Work()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var createInput = new CreateWorkspaceDto
        {
            TenancyName = $"ws{suffix}",
            Name = $"Workspace-{suffix}",
            IsActive = true
        };

        var created = await _workspaceAppService.CreateAsync(createInput);
        created.Id.ShouldBeGreaterThan(0);
        created.Name.ShouldBe(createInput.Name);

        var fetched = await _workspaceAppService.GetAsync(new EntityDto<int>(created.Id));
        fetched.Id.ShouldBe(created.Id);

        var paged = await _workspaceAppService.GetAllAsync(new PagedWorkspaceResultRequestDto
        {
            MaxResultCount = 20,
            SkipCount = 0,
            Keyword = suffix
        });
        paged.Items.ShouldContain(x => x.Id == created.Id);

        var updateInput = new WorkspaceDto
        {
            Id = created.Id,
            Name = $"Workspace-Updated-{suffix}",
            TenancyName = created.TenancyName,
            IsActive = true,
            CreationTime = created.CreationTime
        };

        var updated = await _workspaceAppService.UpdateAsync(updateInput);
        updated.Name.ShouldBe(updateInput.Name);

        await _workspaceAppService.DeleteAsync(new EntityDto<int>(created.Id));

        var listAfterDelete = await _workspaceAppService.GetAllAsync(new PagedWorkspaceResultRequestDto
        {
            MaxResultCount = 20,
            SkipCount = 0,
            Keyword = suffix
        });
        listAfterDelete.Items.ShouldNotContain(x => x.Id == created.Id);

        var deleted = await UsingDbContextAsync(async context =>
            await Task.FromResult(context.Tenants.IgnoreQueryFilters().FirstOrDefault(x => x.Id == created.Id)));
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
    }
}