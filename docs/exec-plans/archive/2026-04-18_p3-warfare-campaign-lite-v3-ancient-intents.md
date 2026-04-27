## Goal
- add the first application-routed campaign-intent command seam for `WarfareCampaign.Lite`
- express that seam in Chinese-ancient desk-sandbox language rather than generic modern wargame phrasing
- keep all effects campaign-level, deterministic, and module-owned

## Scope in
- `WarfareCampaign` persisted intent fields and intent-driven board adjustments
- thin application command service for warfare intent routing
- Chinese-ancient directive, route, front, supply, morale, and command-fit wording on the warfare surface
- schema migration if intent-owned fields become persisted state
- tests and docs for command behavior, migration, determinism, and read-only presentation

## Scope out
- no tactical battle map
- no unit micro
- no detached strategy layer
- no black-route rules
- no authority UI buttons beyond read-only affordance / surfaced intent summaries

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Modules.WarfareCampaign`
- `Zongzu.Application`
- `Zongzu.Presentation.Unity`
- warfare integration / persistence / presentation tests
- warfare / schema / presentation docs

## Save/schema impact
- likely `WarfareCampaign` schema bump from `2` to `3` for persisted campaign intent fields
- default campaign-enabled load path must register `2 -> 3` migration
- `1 -> 2 -> 3` migration chain should remain deterministic for older warfare-lite saves

## Determinism risk
- medium
- campaign board outputs now depend on persisted intent state plus application-routed command staging

## Milestones
1. add warfare intent contracts and thin application command routing
2. persist campaign intent state and apply intent-driven adjustments inside `WarfareCampaign`
3. ancientize warfare-facing language on the campaign board and read-only shell
4. add migration, integration, persistence, and presentation coverage
5. update docs and verify build/tests

## Tests to add/update
- `WarfareCampaignModuleTests`
- `M2LiteIntegrationTests`
- `SaveRoundtripTests`
- `SaveMigrationPipelineTests`
- `FirstPassPresentationShellTests`

## Rollback / fallback plan
- if persisted intent state proves too migration-heavy, keep the thin application service but derive intent labels from current campaign posture only
- if Chinese-ancient naming becomes too broad for this slice, keep only directive labels localized while leaving lower-level labels unchanged
- if command staging threatens determinism, limit intent effects to objective/summary wording and hold numeric adjustments for a later slice

## Open questions
- should future command routing generalize into a broader application command bus, or remain pack-specific until more modules need it?
- should ancient-war wording later be extended into `NarrativeProjection`, or remain board-only for now?

## Completion note
- delivered `WarfareCampaign` schema `3` with persisted directive code/label/summary plus last directive trace
- added `WarfareCampaignCommandService` as a thin application-routed command seam for campaign intent staging
- converted warfare-lite board wording to Chinese-ancient desk-sandbox language while keeping authority campaign-level and read-only in presentation
- updated warfare module, integration, persistence, migration, and shell tests

## Verification
- `C:\Program Files\dotnet\dotnet.exe build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug --no-build`
