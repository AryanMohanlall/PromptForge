using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using ABPGroup.Authorization;
using ABPGroup.Git.Dto;
using System.Linq;

namespace ABPGroup.Git
{
    /// <summary>
    /// Manages Git provider connections for users.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_GitProfiles)]
    public class GitProfileAppService
        : AsyncCrudAppService<GitProfile, GitProfileDto, long, PagedGitProfileResultRequestDto, CreateUpdateGitProfileDto, CreateUpdateGitProfileDto>,
          IGitProfileAppService
    {
        public GitProfileAppService(IRepository<GitProfile, long> repository) : base(repository)
        {
            CreatePermissionName = PermissionNames.Pages_GitProfiles_Create;
            UpdatePermissionName = PermissionNames.Pages_GitProfiles_Edit;
            DeletePermissionName = PermissionNames.Pages_GitProfiles_Delete;
        }

        protected override IQueryable<GitProfile> CreateFilteredQuery(PagedGitProfileResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.Username.Contains(input.Keyword));
        }

        protected override IQueryable<GitProfile> ApplySorting(IQueryable<GitProfile> query, PagedGitProfileResultRequestDto input)
        {
            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
}
