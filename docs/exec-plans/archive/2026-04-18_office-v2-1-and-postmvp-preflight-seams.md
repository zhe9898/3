## Goal
- refine `OfficeAndCareer` into a lighter v2.1 read-model slice with explicit petition outcome categories, stable administrative-task tiers, and clearer promotion/demotion traces
- keep UI read-only and extend post-MVP preflight seams for warfare and black-route contracts without adding rules

## Scope in
- `OfficeAndCareer` query/read-model refinements only
- first-pass office shell/read-model wording updates
- `WarfareCampaign` and black-route contract/seam expansion
- migration / presentation / integration coverage
- docs and acceptance updates

## Scope out
- no new office-owned schema fields planned
- no new authority UI
- no `WarfareCampaign` rules
- no black-route rules or standalone module namespace

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Modules.OfficeAndCareer`
- `Zongzu.Presentation.Unity`
- `Zongzu.Application`
- integration / persistence / presentation / office tests
- UI / post-MVP / acceptance docs

## Save/schema impact
- no root schema bump planned
- no module schema bump planned
- office v2.1 semantics should be derived from existing office schema `2` state and queries only

## Determinism risk
- low
- changes are query/presentation and explanation oriented, but governance-lite replay and migration parity should still be re-verified

## Milestones
1. refine office read-only descriptors and presentation labels
2. surface clearer governance summaries in the shell without adding authority behavior
3. extend warfare and black-route preflight seam contracts and migration-ownership assertions
4. update tests, docs, and verification

## Tests to add/update
- `OfficeAndCareerModuleTests`
- `FirstPassPresentationShellTests`
- `M2LiteIntegrationTests`
- `SaveMigrationPipelineTests`
- docs / acceptance updates

## Rollback / fallback plan
- if the richer labels feel too noisy, keep the contract fields but collapse the shell back to summary-only governance strings
- if the post-MVP seam fields prove too speculative, keep the seam owner/upstream arrays and drop only the extra snapshot properties

## Completion notes
- `OfficeAndCareer` now derives clearer read-only descriptors without a schema bump: petition outcomes expose stable categories, administrative work exposes stable task tiers, and promotion/demotion status exposes explainable trajectory wording through query/read-model fields only.
- governance-lite migration hardening now reconstructs missing schema-`2` office service/task/petition details from persisted explanation text before falling back to conservative defaults, so legacy office saves keep deterministic replay parity.
- the first-pass shell now surfaces current appointment, current task, petition backlog/category, and promotion/demotion wording as read-only office views only; stable M2/M3 paths remain office-empty.
- post-MVP seams now reserve richer warfare and black-route preflight contracts only: warfare mobilization signals carry office/order precursors, while black-route pressure/ledger seams carry suppression-window and diversion/seizure summaries without activating rules or new module namespaces.

## Verification
- `& 'C:\Program Files\dotnet\dotnet.exe' build 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Modules.OfficeAndCareer.Tests\Zongzu.Modules.OfficeAndCareer.Tests.csproj' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj' -c Debug --no-build`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug --no-build -m:1`

## Save compatibility notes
- no root schema changed in this slice.
- no module schema changed in this slice; office v2.1 descriptors remain derived from existing office schema `2` state and read-only queries only.
- legacy `OfficeAndCareer` schema `1` saves still migrate through the existing built-in `1 -> 2` step, now with stronger descriptor reconstruction for replay parity.
- `WarfareCampaign` and black-route work remains preflight-only: no new active save namespace, active manifest key, or standalone black-route envelope was introduced.
