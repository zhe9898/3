# Goal

Advance the repo in the user-requested order by polishing observability, adding an explicit save-migration seam, and creating a non-integrated M3 preflight scaffold for `OrderAndBanditry.Lite` and `ConflictAndForce.Lite`.

# Scope in / out

## In scope
- unify observability metric naming between diagnostics harness and debug shell
- add higher-signal runtime observability summaries
- add an explicit save migration pipeline with clear no-path failure behavior
- add root/module schema guardrails via the migration seam
- scaffold M3 preflight contracts and placeholder module projects for `OrderAndBanditry` and `ConflictAndForce`
- update tests and docs for observability, migration groundwork, and M3 preflight boundaries

## Out of scope
- real save migrations between incompatible schemas
- integrating M3 modules into bootstrap or the monthly loop
- implementing `OrderAndBanditry` or `ConflictAndForce` gameplay rules
- Unity scene work
- post-MVP warfare implementation

# Affected modules

- `Zongzu.Contracts`
- `Zongzu.Persistence`
- `Zongzu.Application`
- `Zongzu.Presentation.Unity`
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.ConflictAndForce`
- integration, persistence, presentation, and new M3 preflight tests
- docs for schema, integration, acceptance, and implementation planning

# Save/schema impact

- no root schema bump
- add a formal migration seam while keeping current behavior as no-op-or-explicit-failure
- reserve schema version `1` namespaces for `OrderAndBanditry` and `ConflictAndForce` preflight scaffolds without enabling them in current manifests
- keep diagnostics and observability summaries runtime-only and non-persisted

# Determinism risk

- observability summaries must be derived read models only
- migration seam must not mutate authoritative state during normal same-version loads
- M3 preflight scaffolds must stay disconnected from current M2 bootstrap paths

# Milestones

1. Add a combined observability metric shape and wire it through the diagnostics harness and debug shell.
2. Add a save migration pipeline with explicit root/module schema checks and no-path failure behavior.
3. Add persistence tests for root/module schema rejection and unchanged same-version behavior.
4. Scaffold `OrderAndBanditry` and `ConflictAndForce` contracts/projects/tests without integrating them into active simulation bootstraps.
5. Update docs to describe the polished observability model, migration groundwork, and M3 preflight status.

# Tests to add/update

- observability tests proving harness and debug shell expose aligned metric names and summaries
- persistence tests proving root-schema mismatch rejection through the migration seam
- persistence tests proving same-version loads remain clean through the migration seam
- M3 preflight tests proving placeholder modules expose stable default state/query seams and remain disabled by default

# Rollback / fallback plan

- if shared observability metric models become noisy, keep them minimal and reuse only the stable counts already collected
- if migration groundwork gets too broad, retain explicit failure paths and defer actual migration handlers
- if M3 preflight scaffolds feel too heavy, keep only contracts and placeholder module runners without integration

# Open questions

- whether future migrations should be registered in persistence or application composition roots
- whether M3 preflight modules should eventually emit placeholder derived projections before any authority logic lands

# Completion notes

- `ObservabilityMetricsSnapshot` now aligns diagnostics harness output with presentation debug snapshot output.
- save loading now passes through `SaveMigrationPipeline` with same-version pass-through and explicit no-path failure behavior.
- `OrderAndBanditry` and `ConflictAndForce` now have schema/version/query/module/test scaffolds only; they are not integrated into active M2 bootstraps.
- verification completed with `dotnet build Zongzu.sln -c Debug` and `dotnet test Zongzu.sln -c Debug --no-build`.
