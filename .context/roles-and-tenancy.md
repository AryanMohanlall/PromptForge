# Roles and tenancy

## Tenancy model

PromptForge uses ABP Framework's multi-tenancy module (open-source).

- One **tenant** = one customer organisation signing up to use PromptForge
- Each tenant gets a subdomain: `acme-team.promptforge.io`
- `TenantId` is stamped automatically by ABP on all tenant-scoped entities
- Data isolation is enforced by ABP's global EF Core query filter:
  `WHERE TenantId = @currentTenantId` — never write this filter manually
- Tenant = workspace. There is no sub-workspace concept (see ADR-002)

## Host vs Tenant

### Host (TenantId = null)
The PromptForge platform operator — the team building and running the platform.

- Owns: `Template`, `Edition`, global `DeploymentSettings`, job queue monitoring
- Role: `PlatformAdministrator`
- Signs in at: `admin.promptforge.io` (no tenant subdomain)
- Has visibility into all tenants' failed builds and queue usage

### Tenant (TenantId = <guid>)
A customer team using PromptForge to generate and deploy their applications.

- All their data carries `TenantId`: `AppRequest`, `PromptSession`,
  `GeneratedProject`, `Repository`, `BuildJob`, `Deployment`, `GitProfile`
- Roles: `ProductBuilder`, `Developer`

## Role definitions

| Spec role              | ABP side | Enum value             | Can do                                                    |
|------------------------|----------|------------------------|-----------------------------------------------------------|
| Product Builder/Founder| Tenant   | `ProductBuilder`       | Submit prompts, select templates, trigger builds, view URL|
| Developer              | Tenant   | `Developer`            | View history, compare versions, inspect commits, rerun    |
| Administrator          | Host     | `PlatformAdministrator`| Manage templates, deployment settings, queue, failed builds|

## Tenant-scoped entities (always carry TenantId)

```csharp
// Example — all tenant aggregates follow this pattern
public class AppRequest : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }  // set by ABP — never set manually
    // ...
}
```

Entities that implement `IMultiTenant`:
- `User` (ABP Identity — already multi-tenant)
- `GitProfile`
- `AppRequest`
- `PromptSession`
- `GeneratedProject`
- `Repository`
- `BuildJob`
- `Deployment`

## Host-only entities (no TenantId — never implement IMultiTenant)

- `Template`
- `Edition` (ABP built-in)
- `DeploymentSettings` (global config)

## GitProfile scoping

Each `GitProfile` carries a `TenantId`. A user who belongs to multiple tenants
(rare — see ADR-006) has a separate `GitProfile` per tenant. This mirrors how
Vercel handles team-level GitHub app installations vs personal ones.

## Cross-tenant users

ABP does not natively support one user record spanning multiple tenants.
If a user needs access to a second tenant, they are provisioned as a separate
`User` record in that tenant. Do not implement cross-tenant user membership
unless explicitly instructed.

## Database strategy (MVP)

Shared database — all tenants share one DB, isolated by `TenantId` column.
Per-tenant databases are deferred (see ADR-001). The `IConnectionStringResolver`
port is defined but uses a single connection string until ADR-001 is revisited.
