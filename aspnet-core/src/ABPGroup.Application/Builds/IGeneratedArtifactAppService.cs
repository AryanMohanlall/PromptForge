using Abp.Application.Services;
using ABPGroup.Builds.Dto;

namespace ABPGroup.Builds
{
    /// <summary>
    /// Read-only service for viewing generated artifacts.
    /// </summary>
    public interface IGeneratedArtifactAppService
        : IAsyncCrudAppService<GeneratedArtifactDto, long, PagedGeneratedArtifactResultRequestDto>
    {
    }
}
