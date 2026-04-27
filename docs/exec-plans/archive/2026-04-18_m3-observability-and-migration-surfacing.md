## Goal
- strengthen runtime-only observability for heavier multi-settlement M3 runs so hotspot and pressure growth are easier to interpret
- surface save-migration activity into the read-only debug shell without expanding the save or authority surface
- broaden stress and migration regression coverage before any post-MVP warfare or black-route implementation starts

## Scope in
- shared runtime observability collectors for diagnostics harness and presentation debug snapshots
- runtime-only migration summaries shown through the presentation shell
- longer stress diagnostics and multi-step migration replay regression
- doc updates for observability, UI/presentation, acceptance, and post-MVP seams

## Scope out
- no new authority modules
- no new player commands or Unity interaction flows
- no persisted diagnostics or persisted migration ledger
- no warfare, diplomacy, or black-market rule implementation

## Affected modules
- `Zongzu.Application`
- `Zongzu.Persistence`
- `Zongzu.Contracts`
- `Zongzu.Presentation.Unity`
- integration, persistence, and presentation tests
- observability / UI / post-MVP docs

## Save/schema impact
- no root schema bump planned
- no module schema bump planned
- migration surfacing must remain runtime-only and absent from save payloads

## Determinism risk
- low to medium
- shared observability refactors must not influence simulation order or mutate module state
- migration summaries must be descriptive only and must not alter load outcomes
- mitigate with:
  - presentation boundary tests
  - longer stress diagnostics regression
  - migration replay equivalence tests
  - full build/test verification

## Milestones
1. capture observability and migration surfacing slice in an ExecPlan
2. extract shared runtime-only hotspot / pressure collection
3. surface hotspot and migration summaries into the read-only debug shell
4. add longer stress and multi-step migration regression coverage
5. update docs and verify

## Tests to add/update
- `M2LiteIntegrationTests`
- `SaveMigrationPipelineTests`
- `FirstPassPresentationShellTests`
- acceptance / UI / post-MVP docs

## Rollback / fallback plan
- if migration surfacing makes the shell too noisy, keep only aggregate labels plus warnings and omit per-step detail
- if longer stress tests become flaky or too slow, keep the same stress seed set but reduce month count while preserving hotspot and migration assertions

## Completion notes
- runtime-only hotspot and interaction-pressure collection now lives in shared application collectors, so the diagnostics harness and the first-pass presentation shell read the same stress signals without duplicating cross-module joins.
- `SaveMigrationPipeline` now emits a runtime migration report during load, and `GameSimulation` carries that report only as load-time metadata; it does not enter root or module save state.
- the read-only debug shell now surfaces current interaction pressure, named hotspot settlements, bootstrap vs save-load origin, and migration-step summaries without adding authority logic to presentation.
- stress diagnostics coverage now includes longer 360-month multi-settlement sweeps with hotspot-score and interaction-pressure ceilings, plus migrated local-conflict loads that surface migration/hotspot state through the debug shell.
- migration regression coverage now includes explicit multi-step module migration reporting and continued replay equivalence for legacy local-conflict stress saves.
- post-MVP docs now explicitly reserve hotspot and migration diagnostics as developer-facing runtime overlays during future black-route and warfare rollout.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`

## Save compatibility notes
- no persisted observability or migration-ledger state was added.
- load-migration summaries, hotspot summaries, and current interaction-pressure summaries remain runtime-only overlays outside the save compatibility surface.
