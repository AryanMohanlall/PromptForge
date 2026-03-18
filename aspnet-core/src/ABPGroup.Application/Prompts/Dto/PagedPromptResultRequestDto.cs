using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Runtime.Validation;

namespace ABPGroup.Prompts.Dto;

public class PagedPromptResultRequestDto : PagedAndSortedResultRequestDto, IShouldNormalize
{
    public string Keyword { get; set; }

    public long? ProjectId { get; set; }

    public void Normalize()
    {
        if (Sorting.IsNullOrWhiteSpace())
        {
            Sorting = "CreatedAt DESC";
        }
    }
}
