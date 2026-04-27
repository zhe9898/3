## Goal
- extend `OfficeAndCareer.Lite` from first appointment only into a bounded governance-v2 slice with promotion/demotion pressure, administrative tasks, and petition handling
- keep the slice inside office-owned state while preserving query-only downstream influence on order and local force modules

## Scope in
- `OfficeAndCareer` state/query/schema evolution
- `OfficeAndCareer` lite rules for service progression, administrative task assignment, petition outcomes, and bounded office loss/promotion
- governance-lite migration support for old `OfficeAndCareer` schema `1` saves
- module, integration, persistence, and migration coverage
- docs for schema, boundaries, integration, and acceptance

## Scope out
- no `WarfareCampaign` rules
- no black-route or black-market rules
- no final player-facing office UI
- no direct office writes into `OrderAndBanditry` or `ConflictAndForce`

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Modules.OfficeAndCareer`
- `Zongzu.Application`
- tests in module / integration / persistence layers
- schema / integration / acceptance docs

## Save/schema impact
- bump `OfficeAndCareer` module schema from `1` to `2`
- register a default `OfficeAndCareer` `1 -> 2` migration alongside existing default migrations
- keep root schema unchanged
- stable M2/M3 paths remain office-free by default

## Determinism risk
- medium
- office service progression now evolves month-to-month and can emit additional authority events
- mitigate with:
  - focused module tests for promotion/demotion and petition handling
  - integration determinism on the governance-lite path
  - roundtrip and migration tests for schema `1 -> 2`

## Milestones
1. define office-v2 state/query additions and migration seam
2. implement service progression, administrative task, and petition outcome logic
3. keep downstream effects query-only and bounded
4. extend tests for module behavior, governance-lite integration, and save migration
5. update docs and verify build/test

## Tests to add/update
- `OfficeAndCareerModuleTests`
- `M2LiteIntegrationTests`
- `SaveRoundtripTests`
- `SaveMigrationPipelineTests`
- docs / acceptance updates

## Rollback / fallback plan
- if petition/task logic creates too much churn, keep schema `2` but reduce it to descriptive fields without promotion/demotion side effects
- if migration proves noisy, keep the runtime behavior but gate governance-lite loading behind an explicit migration registration until the shape stabilizes

## Completion notes
- `OfficeAndCareer.Lite` now runs as governance-lite v2 with monthly service progression, administrative task assignment, petition handling, and bounded promotion/demotion pressure inside office-owned state.
- the office module schema is now `2`, and default governance-lite load registers an explicit `OfficeAndCareer` `1 -> 2` migration without changing the root schema.
- downstream order/force modules remain query-only consumers of office leverage/petition pressure; no direct foreign-state mutation was added.
- module coverage now includes initial appointment, promotion, office loss, governance-lite determinism, governance-lite roundtrip, and legacy office-save migration.

## Verification
- `& 'C:\Program Files\dotnet\dotnet.exe' build 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug --no-build`

## Save compatibility notes
- no root schema changed in this slice.
- `OfficeAndCareer` now uses module schema `2` on the governance-lite path.
- legacy governance-lite office saves at schema `1` migrate through the built-in `1 -> 2` module step during `LoadP1GovernanceLocalConflict`.
- stable M2 and stable M3 manifests remain office-free by default, so existing non-governance saves do not gain an `OfficeAndCareer` envelope implicitly.
