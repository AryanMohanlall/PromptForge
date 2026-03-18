using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using ABPGroup.Authorization;
using ABPGroup.Authorization.Users;
using ABPGroup.Persons.Dto;
using System.Linq;

namespace ABPGroup.Persons
{
    [AbpAuthorize]
    public class PersonAppService
        : AsyncCrudAppService<User, PersonDto, long, PagedPersonResultRequestDto, CreateUpdatePersonDto, CreateUpdatePersonDto>,
          IPersonAppService
    {
        public PersonAppService(IRepository<User, long> repository) : base(repository)
        {
            GetPermissionName = null;
            GetAllPermissionName = null;
            CreatePermissionName = PermissionNames.Pages_Persons_Create;
            UpdatePermissionName = PermissionNames.Pages_Persons_Edit;
            DeletePermissionName = PermissionNames.Pages_Persons_Delete;
        }

        protected override IQueryable<User> CreateFilteredQuery(PagedPersonResultRequestDto input)
        {
            return Repository.GetAll()
                .Where(x => x.TenantId == AbpSession.TenantId)
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    x => x.UserName.Contains(input.Keyword) ||
                         x.DisplayName.Contains(input.Keyword) ||
                         x.GitHubUsername.Contains(input.Keyword));
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedPersonResultRequestDto input)
        {
            return query.OrderByDescending(x => x.CreationTime);
        }
    }
}
