using System.Threading.Tasks;
using Abp.Dependency;

namespace ABPGroup.CodeGen;

public interface ICodeGenAiService : ITransientDependency
{
    Task<string> CallAiAsync(string systemPrompt, string userPrompt);
}
