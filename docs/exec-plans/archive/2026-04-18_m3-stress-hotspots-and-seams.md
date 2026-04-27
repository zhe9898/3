## Goal
- extend M3 runtime-only observability so large multi-settlement runs can identify the hottest local-conflict settlements without adding new write coupling
- add broader regression coverage for stress bootstrap boundaries and legacy `ConflictAndForce` save migration
- refine post-MVP warfare and black-market seam notes into a more operational checklist

## Scope in
- runtime-only hotspot summaries derived from `ConflictAndForce` and `OrderAndBanditry`
- stress bootstrap parity between order-only and local-conflict slices
- multi-settlement boundary comparisons
- stronger legacy local-conflict migration replay coverage
- doc updates for diagnostics and post-MVP seams

## Scope out
- no new authority modules
- no new player commands
- no direct implementation of warfare, black-market, or diplomacy systems
- no Unity interaction changes

## Affected modules
- `Zongzu.Application`
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.ConflictAndForce`
- integration and persistence tests
- diagnostics / post-MVP docs

## Save/schema impact
- no new persisted state planned
- keep hotspot reporting runtime-only
- migration coverage may expand but should not require another schema bump

## Determinism risk
- low to medium
- stress bootstrap parity changes can affect seeded replay expectations
- mitigate with:
  - order-only vs conflict-enabled comparison tests
  - migration replay regression
  - full build/test verification

## Milestones
1. capture hotspot + seam refinement scope
2. add order-only stress bootstrap parity and runtime-only hotspot summaries
3. add multi-settlement boundary and migration regression tests
4. update docs and verify

## Tests to add/update
- `M2LiteIntegrationTests`
- `SaveMigrationPipelineTests`
- acceptance and diagnostics docs

## Rollback / fallback plan
- if hotspot summaries add too much noise, keep only top-1 settlement per month plus run peak snapshots
- if stress bootstrap parity becomes too maintenance-heavy, preserve the bootstrap but limit assertions to deterministic counts and active/inactive cohorts

## Completion notes
- `SimulationBootstrapper` now exposes matching order-only and local-conflict stress bootstraps, so large multi-settlement runs preserve settlement parity while keeping `ConflictAndForce` activation surfaces opt-in for M3 paths only.
- `M2DiagnosticsHarness` now emits runtime-only named hotspot summaries derived from `OrderAndBanditry` plus `ConflictAndForce`, which lets long stress runs identify the hottest settlements without adding new persisted or cross-module write state.
- integration coverage now verifies that activated-response settlements receive order relief for explicit local-conflict reasons while calm settlements remain visible but do not receive suppression support leakage.
- migration regression coverage now proves that downgraded legacy M3 stress saves upgrade through the default loader and converge back to the same replay hash as current-schema saves after further simulation.
- post-MVP force/disorder planning docs now include more operational pre-implementation checklists, so warfare and black-route expansion can extend the current module seams instead of bypassing them.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`

## Save compatibility notes
- no new persisted diagnostics were added; hotspot reporting remains runtime-only and outside the save surface.
- current M3 local-conflict saves remain on `ConflictAndForce` schema `2`, and legacy stress saves continue to migrate through the default loader before replay proceeds.
