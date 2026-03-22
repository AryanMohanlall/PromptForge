using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using ABPGroup.Authorization;
using ABPGroup.Git.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ABPGroup.Git
{
    /// <summary>
    /// Manages Git repositories linked to projects.
    /// </summary>
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

        public async Task<ProjectRepositoryDto> GetByExternalIdAsync(string externalId)
        {
            var repo = await Repository.FirstOrDefaultAsync(x => x.ExternalRepositoryId == externalId);
            return repo == null ? null : ObjectMapper.Map<ProjectRepositoryDto>(repo);
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

        public async Task<List<ProjectRepositoryDto>> GetByProjectId(long projectId)
        {
            var repositories = await Repository.GetAllListAsync(x => x.ProjectId == projectId);
            return ObjectMapper.Map<List<ProjectRepositoryDto>>(repositories);
        }
    }
}
