Result: completed on 2026-04-18. Added `ConflictAndForce`-owned campaign fatigue and escort-strain fallout, deterministic `2 -> 3` migration, campaign-enabled integration coverage, and read-model-safe boundary documentation without introducing tactical micro or foreign-state writes.

## Goal
Implement the next `WarfareCampaign.Lite` aftermath slice so campaign fallout feeds back into `ConflictAndForce` as persistent local fatigue and readiness fallout, while keeping all state ownership inside `ConflictAndForce` and preserving the campaign-level desk-sandbox model.

## Scope in / out
### In
- Let `ConflictAndForce` consume settlement-targeted warfare aftermath events.
- Add owned persistent fallout fields for local force fatigue / escort strain / fallout trace.
- Apply those fields during monthly force refresh so campaign wear carries into later months.
- Apply a bounded immediate same-month fallout adjustment during the handler pass for campaign-enabled paths.
- Add/update tests for boundaries, migration, determinism, and campaign-enabled integration.
- Update docs for module boundaries, schema versions, simulation flow, and acceptance.

### Out
- No tactical battle map, unit micro, or detached wargame loop.
- No direct mutation of `OrderAndBanditry`, `TradeAndIndustry`, `OfficeAndCareer`, or `WarfareCampaign` from `ConflictAndForce`.
- No new player-facing warfare authority UI.
- No regional terrain authority module.

## Affected modules
- `Zongzu.Modules.ConflictAndForce`
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Persistence.Tests`
- `Zongzu.Integration.Tests`
- docs: `CONFLICT_AND_FORCE.md`, `MODULE_BOUNDARIES.md`, `MODULE_INTEGRATION_RULES.md`, `SCHEMA_NAMESPACE_RULES.md`, `DATA_SCHEMA.md`, `SIMULATION.md`, `ACCEPTANCE_TESTS.md`

## Save/schema impact
- Expected `ConflictAndForce` module schema bump from `2` to `3`.
- New fallout fields remain owned by `ConflictAndForce` only.
- Add a default `2 -> 3` migration that backfills zero fatigue / zero escort strain / empty fallout traces.
- No root schema bump.

## Determinism risk
- Medium.
- Risks:
  - same-month handler fallout drifting if bundles are iterated unstably
  - persistent fatigue over-correcting and destabilizing replay
  - migration defaulting producing different post-load force posture
- Controls:
  - bundle warfare events by settlement in stable order
  - keep fallout calculations pure and bounded
  - re-run `ConflictAndForceResponseStateCalculator.Refresh(...)` after migrations and handler updates
  - verify replay parity between migrated and current campaign-enabled saves

## Milestones
1. Add owned fallout fields, query exposure, schema bump, and migration seam.
2. Apply monthly fallout recovery + penalties inside `ConflictAndForce.RunMonth`.
3. Add warfare aftermath handling inside `ConflictAndForce.HandleEvents`.
4. Extend tests/docs and run focused build/test verification.

## Tests to add/update
- `ConflictAndForceModuleTests` for owned-state fallout handling and monthly recovery.
- `M2LiteIntegrationTests` for campaign-enabled fallout reaching local force posture.
- `SaveMigrationPipelineTests` for `ConflictAndForce` `2 -> 3` migration and replay parity.
- `SaveRoundtripTests` for updated `ConflictAndForce` schema version in local-conflict/campaign paths.

## Rollback / fallback plan
- If persistent fallout proves too replay-sensitive, keep the handler trace but reduce fallout to bounded same-month readiness drag only.
- If the schema bump becomes too invasive, keep the new fields internal to current state shape and default them through migration without adding new downstream contracts.
