# Social Mobility Fidelity Ring V213-V244

## Goal

Implement the first playable rule-density pass for the existing "near is detailed, far is summarized" design. This pass turns the current `FidelityRing`, household livelihood, membership activity, and labor/marriage/migration pool anchors into a small rule-driven loop that can be read by player-facing surfaces and diagnostics.

This is the v213-v244 continuity slice:

- v213-v220: social mobility first layer through household livelihood drift and settlement pool readback.
- v221-v228: person movement first layer through membership activity and migration-pool summaries.
- v229-v236: pressure-triggered focus readback so nearby/hot households can surface as finer-grained without making the whole world 1:1.
- v237-v244: long-run scale/health diagnostics for ring and pool budgets.

This plan deliberately folds the missing v205-v212 fidelity-ring scale-budget skeleton into the start of v213, because the code already has `FidelityRing` and population pools but not enough readback/diagnostic evidence.

## Scope In

- Reuse existing persisted state:
  - `PersonRegistry.PersonRecord.FidelityRing`
  - `PopulationAndHouseholds.Households`
  - `PopulationAndHouseholds.Memberships`
  - `PopulationAndHouseholds.LaborPools`
  - `PopulationAndHouseholds.MarriagePools`
  - `PopulationAndHouseholds.MigrationPools`
- Add deterministic Population-owned rules that keep household livelihood, membership activity, and pool summaries in sync.
- Add read-model and Unity-facing projection fields that say:
  - core/local/regional counts
  - labor / marriage / migration pool thickness
  - why movement is being read as local named detail or regional summary
  - why the player cannot command every person in the world
- Add tests for:
  - livelihood drift from household pressure
  - migration/member activity and pool summaries
  - focus-ring readback and diagnostics
  - Application/UI/Unity copy-only boundaries
  - no schema migration or PersonRegistry expansion

## Scope Out

- No complete society engine.
- No individual-by-individual world simulation.
- No new migration ledger, social-mobility ledger, focus ledger, owner-lane ledger, cooldown ledger, or scheduler ledger.
- No new `WorldManager`, `PersonManager`, `CharacterManager`, or god controller.
- No Application-side social mobility calculation.
- No UI/Unity rule calculation.
- No parsing of `DomainEvent.Summary`, receipt prose, readback prose, notice lines, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or other prose fields.
- No expansion of `PersonRegistry` beyond its existing identity/fidelity-ring authority.
- No SocialMemory durable residue in the same-month movement pass; durable memory remains a later owner-owned pass.

## Affected Modules

- `PopulationAndHouseholds`: owner of household livelihood, membership activity, and summary pools.
- `PersonRegistry`: identity and existing fidelity ring only; no new state fields. This pass may read rings through existing queries and project them.
- `SocialMemoryAndRelations`: no same-month writes; future durable residue only.
- `Application`: projection assembly and diagnostics only.
- `Zongzu.Contracts`: read-model/diagnostic contracts only, no new persisted namespace.
- `Zongzu.Presentation.Unity` / ViewModels: copy projected fields only.

## Save / Schema Impact

Target impact: none.

No new persisted field, no module schema version bump, no root save version bump, no migration, and no manifest change are planned. Existing `PopulationAndHouseholds` schema v3 already contains the pool and membership carriers this pass needs. If implementation requires a new persisted field, stop and document schema/migration impact before coding.

## Determinism Risk

Risk is moderate because household drift and pool rebuilding run on simulation cadence. Mitigation:

- stable ordering by `SettlementId`, `HouseholdId`, and `PersonId`
- no wall-clock, random projection, reflection discovery, or unordered dictionary output
- use existing scheduler phases and module-owned rules
- keep projections read-only and rebuildable from owner snapshots

## Milestones

1. Document plan and confirm no schema target.
2. Implement Population-owned first-layer livelihood/activity/pool synchronization.
3. Add read models for fidelity scale and mobility/pool readback.
4. Copy readbacks into Unity shell ViewModels/adapters.
5. Add focused unit, integration, architecture, presentation, and diagnostic tests.
6. Update docs and close plan with validation evidence.

## Tests To Add / Update

- Population module tests for deterministic livelihood drift, member activity updates, and pool summaries.
- Integration tests proving player-facing projections read social/person movement from structured snapshots.
- Architecture tests guarding no `PersonRegistry` expansion, no Application/UI/Unity rule authority, no prose parsing, no manager/god-controller names, and no schema bump.
- Unity presentation tests proving shell ViewModels copy the new projected fields only.
- Ten-year health diagnostics/update proving ring/pool scale readback exists and stays classified as diagnostics, not authority.

## Rollback / Fallback

If rule behavior destabilizes long-run tests, keep read-model/diagnostic surfaces but revert the monthly drift formulas to pool-summary-only synchronization. If schema pressure appears, stop and split a schema/migration PR before any persisted shape change.

## Implementation Evidence

- `PopulationAndHouseholds` owns the first-layer rules: monthly livelihood drift, membership activity synchronization, deterministic pool rebuilds, and bounded hot-household focus requests.
- `PersonRegistry` now exposes `ChangeFidelityRing`, but it mutates only the existing identity/fidelity-ring field and emits structured metadata. No domain state was added to `PersonRecord`.
- `Application` projects `FidelityScaleSnapshot`, `SettlementMobilitySnapshot`, person dossier movement/fidelity readbacks, and runtime observability scale counters from structured queries only.
- `Zongzu.Presentation.Unity`, shared ViewModels, and Unity mirror ViewModels copy the projected mobility/fidelity fields only.
- Save/schema result: none. `PopulationAndHouseholds` remains schema `3`, `PersonRegistry` remains schema `1`, root save version and manifests are unchanged, and no migration is required.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings / 0 errors.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed.
- `dotnet test tests\Zongzu.Modules.PersonRegistry.Tests\Zongzu.Modules.PersonRegistry.Tests.csproj --no-build` passed.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~SocialMobilityFidelityRingIntegrationTests"` passed.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed.
- `git diff --check` passed; it reported only the existing CRLF-to-LF working-copy warning for `docs/SIMULATION_FIDELITY_MODEL.md`.
- `dotnet test Zongzu.sln --no-build` passed: 0 failed, full solution green.

## Closure

Completed and ready to archive. Schema/migration impact remains explicitly none: no new persisted fields, schema versions, root save version, feature manifest, or migration tests were required.

## Open Questions

- Whether remote/exile dormant stubs should be introduced only after SocialMemory owns durable movement residue.
- Whether the long-run health report should graduate from text output to a dedicated diagnostics artifact.
