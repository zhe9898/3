## Goal
- deepen runtime-only observability so heavy M3 runs expose clearer scale, load, and pressure signals
- harden save/load migration consistency checks and regression coverage across schema upgrades
- refine the read-only debug shell so hotspot, pressure, and migration data are easier for developers to scan

## Scope in
- richer runtime observability snapshots shared by diagnostics and presentation
- stronger migration report detail plus consistency-focused tests
- debug-shell view-model refinements for developer-facing diagnostics
- doc updates for acceptance, schema rules, and UI/presentation debug behavior

## Scope out
- no new authority modules
- no gameplay UI interaction changes for players
- no persisted diagnostics or persisted migration ledgers
- no post-MVP rule implementation for warfare, black routes, or diplomacy

## Affected modules
- `Zongzu.Application`
- `Zongzu.Contracts`
- `Zongzu.Persistence`
- `Zongzu.Presentation.Unity`
- integration, persistence, and presentation tests
- observability / schema / UI docs

## Save/schema impact
- no root schema bump planned
- no module schema bump planned
- migration detail remains runtime-only and excluded from saved authoritative state

## Determinism risk
- low to medium
- observability collectors must stay read-only
- migration consistency checks must not rewrite current-schema saves unexpectedly
- mitigate with:
  - migration roundtrip and replay regression tests
  - presentation boundary tests
  - full build/test verification

## Milestones
1. capture the slice in an ExecPlan
2. add richer shared runtime observability snapshots
3. harden migration consistency reporting and tests
4. refine debug-shell summaries for faster scanning
5. update docs and verify

## Tests to add/update
- `M2LiteIntegrationTests`
- `SaveMigrationPipelineTests`
- `FirstPassPresentationShellTests`
- acceptance / UI / schema docs

## Rollback / fallback plan
- if richer observability becomes too noisy, keep the new snapshots aggregated and hide only the verbose detail from the shell
- if migration consistency reporting becomes too invasive, keep the report but reduce it to summary labels plus invariant checks

## Completion notes
- runtime observability now tracks not only interaction pressure and hotspot settlements, but also scale summaries and top payload-footprint modules, and those snapshots are shared between diagnostics sweeps and the read-only debug shell.
- `SaveMigrationPipeline` now reports consistency metadata for enabled-module keys and module-envelope keys, while still keeping all migration detail runtime-only and outside the persisted save surface.
- the debug shell now exposes scan-friendly summaries for entity/institution scale, payload density, top payload modules, migration consistency, and hotspot/response state, without introducing any authority writes into presentation.
- migration regression coverage now includes explicit source-save immutability checks plus multi-step migration reporting, alongside the existing replay-equivalence coverage for legacy local-conflict stress saves.
- long stress diagnostics now surface peak scale metrics and payload footprints so heavier M3 scenarios are easier to profile before post-MVP systems are introduced.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`

## Save compatibility notes
- no root schema or module schema changed in this slice.
- scale summaries, payload footprints, migration consistency details, and debug-shell refinements all remain runtime-only overlays and do not enter saved authoritative state.
