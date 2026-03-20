using Abp.Application.Services;
using ABPGroup.Builds.Dto;

namespace ABPGroup.Builds
{
    /// <summary>
    /// Read-only service for viewing build job history.
    /// </summary>
    public interface IBuildJobAppService
        : IAsyncCrudAppService<BuildJobDto, long, PagedBuildJobResultRequestDto>
    {
    }
}
