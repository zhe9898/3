## Goal
- regroup the read-only debug shell into clearer developer-facing sections for scale, pressure, hotspots, migration, and warnings
- push post-MVP preflight seams forward for `WarfareCampaign` and black-route depth without activating rules or adding authority writes

## Scope in
- `FirstPassPresentationShell` view-model refactor only; no authority or save-surface changes
- post-MVP contract additions for `WarfareCampaign` and black-route query/migration seams
- acceptance, persistence, integration, and presentation test coverage for the grouped shell and preflight seam behavior
- doc updates for boundaries, schema rules, post-MVP scope, and UI/presentation diagnostics

## Scope out
- no active `WarfareCampaign` rules
- no black-route gameplay rules, ledgers, or live module activation
- no player-facing UI interaction changes beyond read-only developer-shell grouping
- no root schema bump or existing M2/M3 manifest expansion

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Persistence`
- `Zongzu.Presentation.Unity`
- integration, persistence, and presentation tests
- post-MVP / schema / boundary / UI docs

## Save/schema impact
- no root schema bump planned
- no active module schema bump planned
- reserve `WarfareCampaign` as an explicit module key with schema/migration notes only
- keep black-route depth inside `OrderAndBanditry` and `TradeAndIndustry` namespaces; do not create a standalone `BlackRoute` save namespace

## Determinism risk
- low
- debug-shell regrouping must stay purely read-only
- post-MVP seam work must not alter active bootstrap manifests or inject new runtime rules
- mitigate with:
  - presentation grouping tests
  - migration seam tests
  - integration assertions that active M2/M3 slices remain unchanged

## Milestones
1. capture the slice in an ExecPlan
2. regroup the read-only debug shell into `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings`
3. add post-MVP preflight contracts for `WarfareCampaign` and black-route seams
4. add persistence and integration coverage for preflight migration/key-set behavior
5. update docs and verify build/tests

## Tests to add/update
- `FirstPassPresentationShellTests`
- `M2LiteIntegrationTests`
- `SaveMigrationPipelineTests`
- acceptance / schema / module-boundary / UI docs

## Rollback / fallback plan
- if grouped shell view models become awkward, keep the grouped top-level sections but reduce nested detail rather than restoring the flat layout
- if post-MVP seams feel too speculative, keep only the reserved module key plus minimal query contracts and migration-ownership tests

## Completion notes
- `FirstPassPresentationShell` now regroups the read-only debug view model into `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings`, while still composing only from `PresentationReadModelBundle`.
- `WarfareCampaign` is now reserved in code as an explicit module key plus query contract, but remains absent from active M2/M3 bootstraps and save output.
- black-route preflight seams now exist as contracts only: pressure snapshots stay aligned with `OrderAndBanditry`, while gray-route / illicit ledger snapshots stay aligned with `TradeAndIndustry`.
- persistence coverage now proves the generic `SaveMigrationPipeline` can carry a reserved `WarfareCampaign` module envelope and that black-route preflight migrations stay inside `OrderAndBanditry` and `TradeAndIndustry` envelopes without creating a detached module key.
- docs now explicitly describe the grouped debug shell and the fact that post-MVP warfare / black-route work is still preflight-only.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug --no-build`

## Save compatibility notes
- no root schema changed in this slice.
- no active module schema changed in this slice.
- `WarfareCampaign` is only a reserved key/query seam for now, so it still does not appear in active save manifests or module envelopes.
- black-route preflight remains inside `OrderAndBanditry` and `TradeAndIndustry` namespaces; no standalone `BlackRoute` save namespace or active module key was introduced.
- the regrouped debug shell remains runtime-only presentation structure and does not alter persisted authority data.
