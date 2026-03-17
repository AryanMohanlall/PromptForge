using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using ABPGroup.Projects;
using ABPGroup.Prompts.Dto;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace ABPGroup.Prompts;

public class PromptAppService : AsyncCrudAppService<Prompt, PromptDto, long, PagedPromptResultRequestDto, CreateUpdatePromptDto, CreateUpdatePromptDto>, IPromptAppService
{
    public PromptAppService(IRepository<Prompt, long> repository)
        : base(repository)
    {
        GetPermissionName = null;
        GetAllPermissionName = null;
        CreatePermissionName = null;
        UpdatePermissionName = null;
        DeletePermissionName = null;
    }

    protected override Prompt MapToEntity(CreateUpdatePromptDto createInput)
    {
        var entity = base.MapToEntity(createInput);
        entity.CreatedAt = DateTime.UtcNow;
        return entity;
    }

    protected override IQueryable<Prompt> CreateFilteredQuery(PagedPromptResultRequestDto input)
    {
        return Repository.GetAll()
            .WhereIf(input.ProjectId.HasValue, x => x.ProjectId == input.ProjectId.Value)
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Content.Contains(input.Keyword));
    }

    protected override IQueryable<Prompt> ApplySorting(IQueryable<Prompt> query, PagedPromptResultRequestDto input)
    {
        if (!input.Sorting.IsNullOrWhiteSpace())
        {
            return query.OrderBy(input.Sorting);
        }

        return query.OrderByDescending(x => x.CreatedAt);
    }
}
