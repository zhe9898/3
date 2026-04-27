## Goal
- move from M3 preflight scaffolding into an active local-conflict lite slice
- keep the existing M3 order-enabled bootstrap path valid
- add a conflict-enabled M3 bootstrap/load path where `ConflictAndForce.Lite` runs inside the authoritative monthly loop and affects downstream world pressure through clean query boundaries

## Scope in
- `ConflictAndForce.Lite` monthly authority rules
- new conflict-enabled M3 bootstrap and load entry
- query-only integration from `ConflictAndForce` into `OrderAndBanditry`
- narrative/projection support for conflict-lite events
- determinism, save roundtrip, and isolation tests for M2 / order-only M3 / conflict-enabled M3

## Scope out
- no tactical combat or battle-map gameplay
- no `WarfareCampaign`
- no player command implementation for force actions yet
- no Unity scene/prefab interaction work
- no post-MVP outlaw camps or full force logistics model

## Affected modules
- `Zongzu.Application`
- `Zongzu.Contracts`
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.ConflictAndForce`
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Persistence`
- integration and module test projects

## Save/schema impact
- keep root schema version unchanged
- keep `OrderAndBanditry` schema version unchanged
- activate `ConflictAndForce` schema version `1` in a new M3 conflict-enabled bootstrap path
- add default seeded `ConflictAndForce` state only when the conflict-enabled manifest turns the module on
- preserve clean loading for M2 and order-only M3 saves with conflict disabled

## Determinism risk
- medium
- `ConflictAndForce.Lite` adds new monthly simulation logic and event emission
- mitigation:
  - stable settlement/clan ordering
  - all randomness through kernel RNG only
  - deterministic replay coverage across the new bootstrap path

## Milestones
1. add ExecPlan and confirm the active bootstrap/load surface for M3
2. implement `ConflictAndForce.Lite` authority rules and conflict-enabled M3 bootstrap/load
3. wire `OrderAndBanditry` and `NarrativeProjection` to the new conflict outputs through read-only queries and events
4. add/update module, integration, and save tests
5. update docs and verify full build/test pass

## Tests to add/update
- `ConflictAndForceModuleTests`
- `OrderAndBanditryModuleTests`
- `M2LiteIntegrationTests`
- `SaveRoundtripTests`
- save compatibility or migration tests if the new M3 load path needs extra coverage

## Rollback / fallback plan
- if conflict-lite coupling proves too noisy, keep the new conflict-enabled bootstrap path but remove feedback into `OrderAndBanditry`
- if active conflict bootstrap destabilizes existing tests, preserve order-only M3 as the supported path and leave conflict behind a separate bootstrap entry

## Open questions
- how much of conflict-lite should feed back into trade directly versus only through order pressure
- whether the first-pass presentation read model needs explicit force snapshots in this slice or if narrative/debug exposure is sufficient

## Completion notes
- added `CreateM3LocalConflictModules`, `CreateM3LocalConflictBootstrap`, and `LoadM3LocalConflict` while preserving the existing order-only M3 bridge path
- activated `ConflictAndForce.Lite` inside the authoritative monthly loop with deterministic force-posture and local-conflict logic
- wired `OrderAndBanditry.Lite` to consume `ConflictAndForce` posture through queries only
- extended narrative projection, integration coverage, and save roundtrip coverage for the conflict-enabled M3 slice
- verified with:
  - `dotnet build E:\\zongzu_codex_spec_modular_rebuilt\\Zongzu.sln -c Debug`
  - `dotnet test E:\\zongzu_codex_spec_modular_rebuilt\\Zongzu.sln -c Debug --no-build`
