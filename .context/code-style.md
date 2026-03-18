# Code style

## Backend — C# / .NET 9 / ABP Framework

### Naming conventions

| Concept              | Convention            | Example                            |
|----------------------|-----------------------|------------------------------------|
| Aggregate root       | PascalCase noun       | `AppRequest`, `BuildJob`           |
| Entity               | PascalCase noun       | `GeneratedArtifact`, `PromptSession`|
| Value object         | Descriptive noun      | `PromptText`, `LiveUrl`, `BuildLog`|
| Domain event         | PascalCase past tense | `AppRequestCreated`, `BuildJobFailed`|
| Repository port      | `I` + noun + `Repository` | `IAppRequestRepository`        |
| External service port| `I` + noun + `Port`   | `IAIGenerationPort`, `IGitHubPort` |
| Application service  | verb + noun + `Handler` | `SubmitAppRequestHandler`        |
| Command              | verb + noun + `Command` | `SubmitAppRequestCommand`        |
| Query                | `Get` + noun + `Query`  | `GetGenerationHistoryQuery`      |
| DTO (input)          | noun + `Input`          | `SubmitAppRequestInput`          |
| DTO (output)         | noun + `Dto`            | `AppRequestDto`                  |
| ACL translator       | noun + `ACL`            | `GitHubACL`, `AIProviderACL`     |
| ABP module class     | `PromptForge` + layer + `Module` | `PromptForgeDomainModule` |

### Patterns to always follow

- One handler class per use case — `SubmitAppRequestHandler` handles only
  `SubmitAppRequestCommand`
- Repository methods return domain types only — never DTOs, never anonymous types
- Value object constructors validate and throw `BusinessException` on violation —
  no public setters, no default constructors
- Aggregate methods are the only place invariants are enforced and state
  transitions happen — no invariant logic in handlers or services
- Every state-changing handler publishes at least one domain event after
  the transaction commits (Outbox pattern via ABP distributed event bus)
- Cross-aggregate references use typed ID value objects: `AppRequestId`,
  `BuildJobId` — never navigation properties across aggregate boundaries
- Use ABP's `ICurrentTenant` to read the current tenant — never read
  `TenantId` from the entity directly in application code

### Patterns to never use

- No static classes in Domain or Application layers
- No raw `string` for status, category, or type fields — always a value object
  or `enum`
- No direct `DbContext` injection outside the Infrastructure layer
- No business logic in controllers, job runners, or event consumers
- No EF Core navigation properties that cross aggregate boundaries
- No `public set` on aggregate or entity properties — use `private set`
  or `init` only
- No `[NotMapped]` domain objects — keep EF config in `IEntityTypeConfiguration`
  classes in Infrastructure, not in domain classes

### File structure per bounded context (backend)

```
PromptForge.Domain/
  GenerationContext/
    AppRequest.cs                  ← aggregate root
    PromptSession.cs               ← child entity / aggregate root
    GeneratedProject.cs
    Events/
      AppRequestCreated.cs
      GenerationCompleted.cs
    ValueObjects/
      PromptText.cs
      GenerationStatus.cs
      StackSelection.cs

PromptForge.Application/
  GenerationContext/
    Commands/
      SubmitAppRequestCommand.cs
      SubmitAppRequestHandler.cs
    Queries/
      GetGenerationHistoryQuery.cs
      GetGenerationHistoryHandler.cs
    Ports/
      IAIGenerationPort.cs
    Dtos/
      AppRequestDto.cs
      SubmitAppRequestInput.cs

PromptForge.EntityFrameworkCore/
  EntityFrameworkCore/
    Repositories/
      EfCoreAppRequestRepository.cs
    Configurations/
      AppRequestConfiguration.cs   ← IEntityTypeConfiguration<AppRequest>

PromptForge.Infrastructure/
  GenerationContext/
    Adapters/
      AIAdapter.cs                 ← implements IAIGenerationPort
    ACL/
      AIProviderACL.cs             ← maps provider types ↔ domain types
```

---

## Frontend — Next.js / TypeScript

### Naming conventions

| Concept           | Convention                     | Example                        |
|-------------------|--------------------------------|--------------------------------|
| Page component    | PascalCase in app dir          | `app/(tenant)/generate/page.tsx`|
| UI component      | PascalCase                     | `GeneratePromptForm.tsx`       |
| API hook          | `use` + noun                   | `useAppRequests`, `useBuildJob`|
| API client file   | noun + `.api.ts`               | `appRequest.api.ts`            |
| Type / interface  | PascalCase                     | `AppRequestDto`, `BuildJobStatus`|
| Context provider  | noun + `Provider`              | `TenantProvider`, `AuthProvider`|
| Utility           | camelCase                      | `formatLiveUrl`, `parseStatus` |

### Patterns to follow

- All API calls go through a typed hook in `hooks/` — no raw `fetch` in components
- Use the exact DTO type names from the backend — `AppRequestDto`, `BuildJobDto`
  etc. Mirror the ubiquitous language from `domain-model.md`
- Tenant-scoped pages live under `app/(tenant)/`
- Host admin pages live under `app/(host)/admin/`
- Never store OAuth tokens or secrets in `localStorage` — use HTTP-only cookies
  via the ABP auth flow

### Patterns to avoid

- No business logic in page components — move to hooks or utils
- No hardcoded tenant IDs or role strings — read from auth context
- No direct API URL strings in components — centralise in `*.api.ts` files

---

## General rules (all code)

- Use the exact terms from `domain-model.md` ubiquitous language table —
  in class names, variable names, API routes, UI copy, and comments
- Never use `ABPGroup` anywhere — namespace is always `PromptForge`
- Prefer explicit over implicit — no magic strings, no unnamed tuples
- Every public method on an aggregate or value object must have an XML doc
  comment explaining what invariant or behaviour it encapsulates
