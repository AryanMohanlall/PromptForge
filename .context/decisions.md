# Architecture Decision Records (ADRs)

These decisions are settled. Do not re-open without explicitly flagging the
ADR number and proposing a superseding record.

---

## ADR-001 — Shared database for MVP

**Decision:** Use a single shared database with `TenantId` column isolation
enforced by ABP's global EF Core query filter.

**Reason:** Per-tenant databases require either the ABP PRO SaaS module or a
custom `ITenantStore` + `IConnectionStringResolver` implementation (~300 lines).
Neither is worth the overhead at MVP stage. The `IBlobStoragePort` interface
is already defined so artefact storage can be swapped independently.

**Migration path:** When needed, implement `IConnectionStringResolver` as a
port in the Application layer. Infrastructure provides the implementation.
No domain or application layer changes required.

**Status:** Accepted

---

## ADR-002 — No workspace concept

**Decision:** Tenant = workspace. There is no sub-workspace or sub-team model.

**Reason:** The initial team structure is one founder, one developer, and one
platform administrator. All three work on the same projects with no need for
sub-team isolation.

**Migration path:** `AppRequest`, `BuildJob`, and `Deployment` have a nullable
`WorkspaceId` value object field reserved but not used. When sub-workspace
isolation is needed, add a `Workspace` aggregate to `IdentityContext` and
backfill the ID. No aggregate boundary changes required.

**Status:** Accepted

---

## ADR-003 — Platform Administrator is Host-level, not a tenant role

**Decision:** The `Administrator` role from the spec maps to `PlatformAdministrator`
on the Host side (`TenantId = null`). It is not a role inside any tenant.

**Reason:** All four admin use cases in the spec point to platform-level concerns:
- Manage templates → Templates are Host-owned reference data
- Manage deployment settings → Global infra configuration
- Manage queue/usage → Platform-wide job queue
- Monitor failed builds → Cross-tenant visibility required

**If needed later:** A `TenantAdmin` role can be added inside the tenant to
manage team membership and org-level preferences. This is distinct from
`PlatformAdministrator` and must not reuse the same enum value.

**Status:** Accepted

---

## ADR-004 — GeneratedArtifact stores URI only, never raw bytes

**Decision:** The `GeneratedArtifact` entity carries only metadata and a
`StorageUri` value object. Raw file bytes live in blob storage accessed via
`IBlobStoragePort`.

**Reason:** Aggregate size must remain predictable. Storing raw bytes on the
aggregate would make it non-serialisable for event sourcing and would bloat
the database row. Blob storage is an Infrastructure concern.

**Status:** Accepted

---

## ADR-005 — BuildJob and Deployment are separate aggregates

**Decision:** `BuildJob` (compile/package) and `Deployment` (place artefact in
target environment) are separate aggregate roots with independent lifecycles.

**Reason:** A build can succeed while a deployment fails and needs independent
retry. Merging them into one aggregate would conflate two distinct state machines
and make partial failure handling impossible without violating invariant boundaries.

**Status:** Accepted

---

## ADR-006 — GitProfile is tenant-scoped

**Decision:** `GitProfile` implements `IMultiTenant` and carries a `TenantId`.
A user provisioned in two tenants has two separate `GitProfile` records.

**Reason:** A founder may use a personal GitHub account for one project and a
GitHub organisation account for another. Scoping to tenant allows different
OAuth tokens per workspace context.

**Status:** Accepted

---

## ADR-007 — Secrets are never plain strings in the domain

**Decision:** `OAuthToken`, API keys, and all credentials are typed as
`EncryptedSecret` (a value object wrapping an opaque string). No aggregate,
entity, or value object may expose a secret as `string`.

**Reason:** Prevents accidental logging, serialisation to JSON, or inclusion
in domain events. Infrastructure resolves the actual value at runtime via
`ISecretResolverPort`.

**Status:** Accepted

---

## ADR-008 — Project namespace is PromptForge, not ABPGroup

**Decision:** All C# namespaces, class prefixes, and project names use
`PromptForge`. The `ABPGroup` placeholder from the ABP template is fully removed.

**Status:** Accepted

---

## ADR-009 — Template is Host-owned, consumed via Published Language

**Decision:** `Template` aggregate root has no `TenantId`. Tenants read the
template catalogue via a Published Language interface (read-only DTOs).
Tenants cannot create, modify, or deprecate templates.

**Reason:** Templates are platform IP managed by the PlatformAdministrator.
Allowing tenants to mutate templates would break isolation guarantees for all
other tenants using the same template.

**Status:** Accepted
