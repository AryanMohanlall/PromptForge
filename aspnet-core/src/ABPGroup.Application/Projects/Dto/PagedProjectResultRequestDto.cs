using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Runtime.Validation;

namespace ABPGroup.Projects.Dto;

public class PagedProjectResultRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
{
    public string Keyword { get; set; }

    public int? WorkspaceId { get; set; }

    public void Normalize()
    {
        if (Sorting.IsNullOrWhiteSpace())
        {
            Sorting = "UpdatedAt DESC";
        }
    }
}
