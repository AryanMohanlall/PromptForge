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
    /// Read-only service for viewing repository commit history.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_RepositoryCommits)]
    public class RepositoryCommitAppService
        : AsyncCrudAppService<RepositoryCommit, RepositoryCommitDto, long, PagedRepositoryCommitResultRequestDto>,
          IRepositoryCommitAppService
    {
        public RepositoryCommitAppService(IRepository<RepositoryCommit, long> repository) : base(repository)
        {
            // Read-only: disable create/update/delete
            CreatePermissionName = null;
            UpdatePermissionName = null;
            DeletePermissionName = null;
        }

        protected override IQueryable<RepositoryCommit> CreateFilteredQuery(PagedRepositoryCommitResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.Message.Contains(input.Keyword) || x.Sha.Contains(input.Keyword));
        }

        protected override IQueryable<RepositoryCommit> ApplySorting(IQueryable<RepositoryCommit> query, PagedRepositoryCommitResultRequestDto input)
        {
            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
}
