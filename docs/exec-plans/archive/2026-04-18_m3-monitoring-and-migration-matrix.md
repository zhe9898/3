## Goal
- deepen runtime-only monitoring so heavy M3 runs expose clearer pressure distribution and payload-growth signals
- strengthen the save-migration regression matrix around consistency, explicit failure, and longer replay stability
- refine the read-only debug shell so developers can scan pressure, payload, and migration status faster

## Scope in
- richer runtime observability snapshots for pressure distribution and payload summaries
- diagnostics harness extensions that surface those summaries in long stress runs
- migration-report refinements and broader consistency regression tests
- debug-shell summary labels for pressure, payload, and migration state
- doc updates for acceptance, schema, and presentation behavior

## Scope out
- no new authority modules
- no gameplay UI changes for players
- no persisted diagnostics or migration ledgers
- no post-MVP rules for warfare, diplomacy, or black routes

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Persistence`
- `Zongzu.Presentation.Unity`
- integration, persistence, and presentation tests
- observability / schema / acceptance / UI docs

## Save/schema impact
- no root schema bump planned
- no module schema bump planned
- new monitoring and migration summaries must remain runtime-only and outside save compatibility

## Determinism risk
- low to medium
- observability collectors must remain read-only
- migration-report changes must not alter prepared save data unexpectedly
- mitigate with:
  - longer replay regression after migration
  - consistency and immutability tests
  - presentation boundary tests
  - full build/test verification

## Milestones
1. capture the monitoring + migration slice in an ExecPlan
2. add higher-signal pressure and payload observability snapshots
3. widen migration regression coverage for consistency and failure cases
4. refine debug-shell summaries for faster scanning
5. update docs and verify

## Tests to add/update
- `M2LiteIntegrationTests`
- `SaveMigrationPipelineTests`
- `FirstPassPresentationShellTests`
- acceptance / UI / schema docs

## Rollback / fallback plan
- if the new summaries are too noisy, keep the raw snapshots but reduce shell exposure to headline labels only
- if the migration matrix gets too expensive, keep the longer replay case and the key-set consistency case, and drop only redundant permutations

## Completion notes
- runtime monitoring now includes settlement pressure distribution, richer interaction-pressure summaries, payload-summary headlines, and top payload modules, all shared between diagnostics sweeps and the read-only debug shell.
- long stress diagnostics now retain peak pressure-distribution and payload-summary snapshots, so heavy M3 runs surface not just hotspots but also broader load shape and payload concentration.
- save-migration reporting now tracks source/prepared root schemas, applied-step counts, consistency pass/fail state, and source/prepared key-set preservation without mutating the caller's source save root.
- migration regression coverage now includes combined root+module reporting, explicit key-set warning capture for bad migrations, source-save immutability checks, and longer replay equivalence for migrated local-conflict stress saves.
- the read-only debug shell now groups pressure, payload, and migration status into faster-scanning summary labels while still consuming only read-model data.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`

## Save compatibility notes
- no root schema or module schema changed in this slice.
- pressure distributions, payload summaries, migration consistency details, and debug-shell summary labels remain runtime-only overlays and do not enter saved authoritative state.
