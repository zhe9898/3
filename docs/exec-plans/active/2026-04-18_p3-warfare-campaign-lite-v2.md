## Goal
- deepen `WarfareCampaign.Lite` into a more legible campaign-board slice with explicit route/front/supply structure and clearer commander-fit summaries
- keep the slice campaign-level, deterministic, and query-only with respect to upstream force/office state

## Scope in
- `WarfareCampaign` board-depth refinement
- route / supply corridor / commander-fit descriptors
- read-only presentation updates for the campaign board
- legacy warfare-lite schema migration if a state bump is needed
- warfare persistence, integration, presentation, and migration tests
- docs and acceptance updates

## Scope out
- no unit micro
- no tactical battle resolution
- no direct player authority UI for war commands
- no black-route rules
- no detached battlefield map

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Modules.WarfareCampaign`
- `Zongzu.Application`
- `Zongzu.Presentation.Unity`
- `Zongzu.Persistence.Tests`
- `Zongzu.Integration.Tests`
- `Zongzu.Modules.WarfareCampaign.Tests`
- warfare / schema / presentation docs

## Save/schema impact
- likely `WarfareCampaign` schema bump if board-owned route/commander descriptors become persisted state
- old non-warfare paths stay unchanged
- campaign-enabled path must keep deterministic replay parity after migration

## Determinism risk
- medium
- this slice changes an active authority module and may add a module migration path

## Milestones
1. refine campaign-owned board structure and descriptors
2. wire any needed warfare schema migration into the campaign-enabled load path
3. surface richer read-only campaign board details in the shell
4. update tests, docs, and verification

## Tests to add/update
- `WarfareCampaignModuleTests`
- `M2LiteIntegrationTests`
- `SaveRoundtripTests`
- `SaveMigrationPipelineTests`
- `FirstPassPresentationShellTests`

## Rollback / fallback plan
- if persisted board-depth fields prove too migration-heavy, keep the richer labels derived at query/read-model level only
- if the new board structure feels noisy, keep commander/supply wording but collapse route detail back to one summary line
- if migration parity is shaky, hold the schema bump and ship only read-only descriptor refinements

## Completion notes
- `WarfareCampaign` now persists campaign-board labels, command-fit wording, commander summaries, and bounded route descriptors at schema `2`
- default campaign-enabled load now registers a built-in `WarfareCampaign` `1 -> 2` migration that backfills those descriptors deterministically
- read-only campaign shell now surfaces front labels, command-fit, commander summaries, and route flow summaries without introducing authority UI

## Verification
- `C:\Program Files\dotnet\dotnet.exe build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug --no-build`
