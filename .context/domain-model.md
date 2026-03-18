# Domain model

## Bounded contexts

### IdentityContext
Manages user accounts, GitHub OAuth profiles, and authentication sessions.

Aggregate roots: `User`, `GitProfile`
Value objects:   `Email`, `OAuthToken` (opaque), `GitHubUsername`, `Role`
Events produced: `UserRegistered`, `GitProfileLinked`, `SessionRevoked`

### GenerationContext
Owns the full lifecycle of a prompt-to-artefacts generation job — from raw user
input through AI invocation to a validated, packaged file output.

Aggregate roots: `AppRequest`, `PromptSession`, `GeneratedProject`
Value objects:   `PromptText`, `AppCategory`, `StackSelection`,
                 `GenerationStatus`, `SemanticVersion`
Events produced: `AppRequestCreated`, `PromptSessionStarted`,
                 `GenerationCompleted`, `GenerationFailed`, `ArtifactPackageReady`

### RepositoryContext
Manages GitHub repository creation, branch strategy, and commit history for
generated projects.

Aggregate roots: `Repository`
Value objects:   `RepoVisibility`, `CommitMessage`, `BranchName`, `RepoReference`
Events produced: `RepositoryCreated`, `CodeCommitted`, `BranchCreated`
Consumes:        `ArtifactPackageReady` (from GenerationContext)

### DeploymentContext
Orchestrates the build-and-deploy pipeline, tracks job state, surfaces logs,
and exposes the live URL on success.

Aggregate roots: `BuildJob`, `Deployment`
Value objects:   `DeploymentTarget`, `DeploymentStatus`, `BuildLog`, `LiveUrl`
Events produced: `BuildJobQueued`, `BuildJobStarted`, `BuildJobSucceeded`,
                 `BuildJobFailed`, `DeploymentSucceeded`, `DeploymentFailed`,
                 `DeploymentCancelled`
Consumes:        `CodeCommitted` (from RepositoryContext)

### TemplateContext
Manages the template catalogue — create, version, deprecate — independently of
any AppRequest. Host-owned; tenants consume via Published Language.

Aggregate roots: `Template`
Value objects:   `TemplateConfig`, `SupportedStack`, `TemplateStatus`
Events produced: `TemplatePublished`, `TemplateDeprecated`
Consumes:        nothing (upstream reference data only)

### SharedKernel (read-only, no aggregate)
`UserId`, `TenantId`, `Timestamp`, `EncryptedSecret`, `StorageUri`,
`SemanticVersion`, `DomainEvent` (base type)

---

## Ubiquitous language — use these terms exactly, everywhere

| Term                | Meaning                                                         |
|---------------------|-----------------------------------------------------------------|
| AppRequest          | One user's request to build an app from a prompt               |
| PromptSession       | One generation attempt; immutable snapshot of prompt at submit  |
| GeneratedProject    | The output artefacts produced by a completed PromptSession      |
| BuildJob            | The act of compiling / packaging the generated code             |
| Deployment          | Placing a built artefact into a target environment              |
| LiveUrl             | The HTTPS URL returned on successful deployment                 |
| Template            | A scaffold definition owned by the Host (platform operator)     |
| GeneratedArtifact   | Metadata + StorageUri for one generated file (no raw bytes)     |
| PromptSnapshot      | Immutable copy of PromptText captured at PromptSession creation |
| ArtifactPackageReady| Domain event signalling all artefacts are validated and stored  |
| PlatformAdministrator| The Host-level operator role (not a tenant role)               |

---

## Key aggregate rules

### AppRequest state machine
```
DRAFT --(submit)--> PENDING
PENDING --(ai_start)--> IN_PROGRESS
IN_PROGRESS --(success)--> COMPLETED
IN_PROGRESS --(failure)--> FAILED
FAILED --(resubmit)--> PENDING
```
- `PromptText` is immutable once status leaves DRAFT
- `StackSelection` must match an ACTIVE `Template`

### BuildJob state machine
```
QUEUED --(start)--> RUNNING
RUNNING --(success)--> SUCCEEDED
RUNNING --(failure)--> FAILED
FAILED --(retry, count < max)--> QUEUED
FAILED --(retry, count >= max)--> PERMANENTLY_FAILED
```
- `RetryCount` may never exceed `MaxRetries`
- `BuildLog` entries are append-only — no mutation or deletion

### Deployment state machine
```
QUEUED --> RUNNING --> SUCCESS
                   --> FAILED
                   --> CANCELLED
```
- `LiveUrl` is set only when status = SUCCESS
- No reverting from SUCCESS or FAILED back to RUNNING

### Template state machine
```
DRAFT --> ACTIVE --> DEPRECATED
```
- A Template may never return to DRAFT from ACTIVE
- `SupportedStacks` must be non-empty for ACTIVE templates

---

## Cross-cutting rules

- Cross-aggregate references use typed ID value objects only — never object refs
- Domain event payloads contain only value objects and primitive types
- `GeneratedArtifact` stores only metadata and a `StorageUri` — never raw bytes
- `OAuthToken` and all secrets are `EncryptedSecret` type — never plain `string`
- `PromptSnapshot` inside `PromptSession` is immutable after creation
- Every state-changing operation raises at least one domain event
