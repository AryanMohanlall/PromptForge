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
    /// Manages Git repositories linked to projects.
    /// </summary>
    [AbpAuthorize(PermissionNames.Pages_ProjectRepositories)]
    public class ProjectRepositoryAppService
        : AsyncCrudAppService<ProjectRepository, ProjectRepositoryDto, long, PagedProjectRepositoryResultRequestDto, CreateUpdateProjectRepositoryDto, CreateUpdateProjectRepositoryDto>,
          IProjectRepositoryAppService
    {
        public ProjectRepositoryAppService(IRepository<ProjectRepository, long> repository) : base(repository)
        {
            CreatePermissionName = PermissionNames.Pages_ProjectRepositories_Create;
            UpdatePermissionName = PermissionNames.Pages_ProjectRepositories_Edit;
            DeletePermissionName = PermissionNames.Pages_ProjectRepositories_Delete;
        }

        protected override IQueryable<ProjectRepository> CreateFilteredQuery(PagedProjectRepositoryResultRequestDto input)
        {
            return Repository.GetAll()
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.FullName.Contains(input.Keyword) || x.Name.Contains(input.Keyword));
        }

        protected override IQueryable<ProjectRepository> ApplySorting(IQueryable<ProjectRepository> query, PagedProjectRepositoryResultRequestDto input)
        {
            return query.OrderByDescending(x => x.CreatedAt);
        }
    }
}
