# Architecture

## Stack

| Layer       | Technology                              |
|-------------|-----------------------------------------|
| Backend     | C# / .NET 9, ABP Framework (open-source)|
| Frontend    | Next.js (App Router), TypeScript        |
| Database    | SQL Server / PostgreSQL via EF Core     |
| Auth        | ABP Identity + GitHub OAuth             |
| Queue       | ABP Background Jobs (Hangfire adapter)  |
| Blob store  | Configurable via `IBlobStoragePort`     |
| API style   | REST (ABP dynamic API) + Swagger        |

---

## Clean Architecture layers

Dependencies point inward only:

```
[Presentation] → [Infrastructure] → [Application] → [Domain]
```

### Domain layer — `PromptForge.Domain`
- Zero dependencies on any framework or outer layer
- Contains: aggregates, entities, value objects, domain events,
  repository interfaces (ports), domain services, domain exceptions
- No EF Core, no ABP application services, no HTTP concerns

### Application layer — `PromptForge.Application`
- Depends on Domain only
- Contains: use-case command/query handlers, application services,
  port interfaces for all external systems, DTOs for input/output
- No business logic — no if-statements that enforce domain invariants
- All port interfaces live here: `IAIGenerationPort`, `IGitHubPort`,
  `IDeploymentPort`, `IJobQueuePort`, `IBlobStoragePort`

### Infrastructure layer — `PromptForge.Infrastructure` / `PromptForge.EntityFrameworkCore`
- Implements all ports defined in Application
- Contains: EF Core repositories, GitHub adapter, AI provider adapter,
  deployment adapter, queue adapter, blob storage adapter, ACL translators
- EF Core `DbContext` lives here and nowhere else

### Presentation layer — `PromptForge.Web.Host` / `PromptForge.Web.Core`
- Contains: REST controllers, background job runners, event consumers,
  Swagger configuration, startup/middleware
- No business logic, no domain invariants

---

## Project structure

```
aspnet-core/src/
  PromptForge.Domain/
    IdentityContext/
    GenerationContext/
      AppRequest.cs
      PromptSession.cs
      GeneratedProject.cs
      Events/
      ValueObjects/
    RepositoryContext/
    DeploymentContext/
    TemplateContext/
    Shared/                        ← SharedKernel value objects

  PromptForge.Application/
    IdentityContext/
    GenerationContext/
      Commands/
      Queries/
      Ports/                       ← IAIGenerationPort, etc.
    RepositoryContext/
    DeploymentContext/
    TemplateContext/

  PromptForge.EntityFrameworkCore/
    EntityFrameworkCore/
      Repositories/
      Configurations/              ← EF entity configs per context
      Seed/

  PromptForge.Infrastructure/
    GenerationContext/
      Adapters/                    ← AIAdapter, GitHubAdapter
      ACL/                         ← AIProviderACL, GitHubACL, DeploymentACL
    DeploymentContext/
      Adapters/

  PromptForge.Web.Core/
    Authentication/
    Controllers/

  PromptForge.Web.Host/
    Startup/
    Controllers/

frontend/src/
  app/
    (tenant)/                      ← tenant-scoped pages
      dashboard/
      generate/
      history/
      deployments/
    (host)/                        ← host admin pages
      admin/
  components/
  providers/
  hooks/                           ← API hooks per bounded context
  utils/
```

---

## ABP module setup

```
PromptForge.Domain           → depends on Volo.Abp.Ddd.Domain
PromptForge.Application      → depends on Volo.Abp.Ddd.Application
PromptForge.EntityFrameworkCore → depends on Volo.Abp.EntityFrameworkCore
PromptForge.Web.Host         → depends on Volo.Abp.AspNetCore.Mvc
```

Multi-tenancy module: `Volo.Abp.TenantManagement.Application` (open-source)
Background jobs:      `Volo.Abp.BackgroundJobs.HangFire`
Blob storing:         `Volo.Abp.BlobStoring` with file system provider (swap for
                       Azure Blob / S3 in production)

---

## Port interfaces (Application layer only)

```csharp
interface IAIGenerationPort {
    Task<StructuredRequirements> ParsePromptAsync(PromptText text);
    Task<GeneratedArtifacts> GenerateAsync(StructuredRequirements req,
                                            TemplateConfig config);
}

interface IGitHubPort {
    Task<RepoReference> CreateRepositoryAsync(CreateRepoParams p);
    Task<CommitReference> PushCommitAsync(PushCommitParams p);
    Task<BranchName> CreateBranchAsync(CreateBranchParams p);
}

interface IDeploymentPort {
    Task<DeployJobReference> TriggerDeploymentAsync(DeployParams p);
    Task<DeploymentStatus> GetStatusAsync(BuildJobId jobId);
    Task CancelAsync(BuildJobId jobId);
}

interface IJobQueuePort {
    Task EnqueueAsync(QueuedJob job);
    Task MarkCompleteAsync(JobId id);
    Task MarkFailedAsync(JobId id, string reason);
}

interface IBlobStoragePort {
    Task<StorageUri> UploadAsync(ArtifactStream stream);
    Task<ArtifactStream> DownloadAsync(StorageUri uri);
}
```

---

## Event publishing rule

Domain events are published **after** the transaction commits.
Use ABP's built-in distributed event bus with the Outbox pattern on every
event publish step. Never publish inside the transaction boundary.
