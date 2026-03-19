using Abp.UI;
using ABPGroup.Workspaces;
using ABPGroup.Workspaces.Dto;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace ABPGroup.Tests.Services
{
    public class WorkspaceAppServiceTests : ABPGroupTestBase
    {
        private readonly IWorkspaceAppService _workspaceAppService;

        public WorkspaceAppServiceTests()
        {
            _workspaceAppService = Resolve<IWorkspaceAppService>();
        }

        // ── CreateAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidInput_CreatesWorkspace()
        {
            var dto = new CreateWorkspaceDto { Name = "MyWorkspace", TenancyName = "MyWorkspace" };

            var result = await _workspaceAppService.CreateAsync(dto);

            result.ShouldNotBeNull();
            result.Id.ShouldBeGreaterThan(0);
            result.Name.ShouldBe("MyWorkspace");
        }

        [Fact]
        public async Task CreateAsync_NoTenancyName_DerivedFromName()
        {
            var dto = new CreateWorkspaceDto { Name = "Derived Name", TenancyName = null };

            var result = await _workspaceAppService.CreateAsync(dto);

            // Special chars stripped → "DerivedName"
            result.TenancyName.ShouldBe("DerivedName");
        }

        [Fact]
        public async Task CreateAsync_NameWithSpecialChars_StripsInvalidCharsFromTenancyName()
        {
            var dto = new CreateWorkspaceDto { Name = "Test & Co.", TenancyName = null };

            var result = await _workspaceAppService.CreateAsync(dto);

            // "Test & Co." → strips '&', ' ', '.' → "TestCo"
            result.TenancyName.ShouldBe("TestCo");
        }

        [Fact]
        public async Task CreateAsync_NameStartsWithDigit_PrependW()
        {
            // tenancy name "123abc" doesn't match TenancyNameRegex (must start letter/underscore)
            // so service prepends "w"
            var dto = new CreateWorkspaceDto { Name = "123abc", TenancyName = null };

            var result = await _workspaceAppService.CreateAsync(dto);

            result.TenancyName.ShouldStartWith("w");
        }

        [Fact]
        public async Task CreateAsync_AllSpecialCharsName_FallsBackToWorkspace()
        {
            var dto = new CreateWorkspaceDto { Name = "!@#$%", TenancyName = null };

            var result = await _workspaceAppService.CreateAsync(dto);

            result.TenancyName.ShouldBe("workspace");
        }

        // ── uniqueness ────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_DuplicateTenancyName_AppendsSuffix()
        {
            var first = await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "Duplicate", TenancyName = "Duplicate" });

            var second = await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "Another", TenancyName = "Duplicate" });

            first.TenancyName.ShouldBe("Duplicate");
            second.TenancyName.ShouldBe("Duplicate1");
        }

        [Fact]
        public async Task CreateAsync_ThreeSameName_AppendsSuffixesSequentially()
        {
            await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "Seq", TenancyName = "Seq" });
            await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "Seq", TenancyName = "Seq" });
            var third = await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "Seq", TenancyName = "Seq" });

            third.TenancyName.ShouldBe("Seq2");
        }

        // ── GetAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAsync_ValidId_ReturnsWorkspace()
        {
            var created = await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "GetTest", TenancyName = "GetTest" });

            var fetched = await _workspaceAppService.GetAsync(
                new Abp.Application.Services.Dto.EntityDto<int>(created.Id));

            fetched.Id.ShouldBe(created.Id);
            fetched.Name.ShouldBe("GetTest");
        }

        [Fact]
        public async Task GetAsync_ZeroIdWithTenantSession_UsesSessionTenant()
        {
            // AbpSession.TenantId is 1 (Default) from test base setup
            var result = await _workspaceAppService.GetAsync(
                new Abp.Application.Services.Dto.EntityDto<int>(0));

            result.Id.ShouldBe(AbpSession.TenantId!.Value);
        }

        [Fact]
        public async Task GetAsync_ZeroIdWithNoSession_ThrowsUserFriendlyException()
        {
            // Clear the tenant session so there is no ID to fall back to
            AbpSession.TenantId = null;

            await Should.ThrowAsync<UserFriendlyException>(
                () => _workspaceAppService.GetAsync(
                    new Abp.Application.Services.Dto.EntityDto<int>(0)));
        }

        // ── GetAllAsync (filtered) ────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_NoKeyword_ReturnsAllWorkspaces()
        {
            await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "WsAll1", TenancyName = "WsAll1" });
            await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "WsAll2", TenancyName = "WsAll2" });

            var result = await _workspaceAppService.GetAllAsync(
                new PagedWorkspaceResultRequestDto { MaxResultCount = 50, SkipCount = 0 });

            result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetAllAsync_WithKeyword_FiltersResults()
        {
            await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "UniqueKeywordWs", TenancyName = "UniqueKeywordWs" });
            await _workspaceAppService.CreateAsync(
                new CreateWorkspaceDto { Name = "OtherWorkspace", TenancyName = "OtherWorkspace" });

            var result = await _workspaceAppService.GetAllAsync(
                new PagedWorkspaceResultRequestDto
                {
                    Keyword = "UniqueKeywordWs",
                    MaxResultCount = 50,
                    SkipCount = 0
                });

            result.Items.ShouldAllBe(w => w.Name.Contains("UniqueKeywordWs"));
        }
    }
}
