using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using ABPGroup.Projects.Dto;

namespace ABPGroup.CodeGen
{
    public interface ICodeGenAppService : IApplicationService
    {
        Task<CodeGenResult> GenerateProjectAsync(CreateUpdateProjectDto request);
    }

    public class CodeGenResult
    {
        public long GeneratedProjectId { get; set; }
        public string OutputPath { get; set; }
        public List<GeneratedFile> Files { get; set; }
        public string ArchitectureSummary { get; set; }
        public List<string> ModuleList { get; set; }
    }

    public class GeneratedFile
    {
        public string Path { get; set; }
        public string Content { get; set; }
    }
}
