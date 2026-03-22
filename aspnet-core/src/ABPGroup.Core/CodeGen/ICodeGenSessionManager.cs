using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Abp.Dependency;
using ABPGroup.CodeGen.Dto;

namespace ABPGroup.CodeGen;

public interface ICodeGenSessionManager : ITransientDependency
{
    Task<CodeGenSession> GetSessionAsync(string sessionId);
    Task<CodeGenSessionDto> GetSessionDtoAsync(string sessionId);
    Task SaveSessionAsync(CodeGenSession session, bool isNew = false);
    Task<CodeGenSessionDto> CreateSessionAsync(CreateSessionInput input, long? userId = null);
    CodeGenSessionDto MapToDto(CodeGenSession session);
    System.Linq.IQueryable<CodeGenSession> GetSessionQuery();
}
