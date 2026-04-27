## Goal
- make the read-only warfare desk sandbox feel materially different as campaign conditions change
- derive those differences from existing campaign projections rather than adding new authority rules
- keep the shell aligned with a Chinese-ancient campaign board rather than a static generic panel

## Scope in
- `WarfareCampaign` read-model to shell composition only
- campaign board environment/atmosphere descriptors derived from front pressure, supply, morale, directive, and route mix
- presentation tests and integration assertions for varying board states
- presentation docs and acceptance notes

## Scope out
- no new `WarfareCampaign` authority logic
- no module schema bump
- no new save fields
- no unit micro, battlefield map, or authority UI

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Application`
- presentation / integration tests
- UI / acceptance docs

## Save/schema impact
- no root schema bump planned
- no module schema bump planned
- board-environment variation must be derived from existing read models only

## Determinism risk
- low
- changes stay in read-model composition and presentation wording, but campaign-enabled replay coverage should still be rechecked

## Milestones
1. add a bounded read-only campaign-board environment model
2. derive environment, atmosphere, and route posture labels from existing campaign snapshots
3. extend shell and integration tests to prove the board changes with campaign conditions
4. update docs and verify build/tests

## Tests to add/update
- `FirstPassPresentationShellTests`
- `M2LiteIntegrationTests`
- documentation / acceptance references

## Rollback / fallback plan
- if the richer descriptors feel noisy, keep only one environment headline plus one atmosphere summary
- if the variation model feels too speculative, limit it to deterministic route/front/supply descriptors and drop decorative fields

## Completion notes
- warfare shell now derives board-environment labels, board-surface labels, marker summaries, and atmosphere summaries from existing `WarfareCampaign` snapshots only
- great-hall warfare summary and settlement campaign summaries now reflect the varying board posture instead of a single static shell tone
- presentation and integration coverage now asserts that different campaign conditions produce different desk-sandbox board descriptors without adding authority writes or save fields

## Verification
- `C:\Program Files\dotnet\dotnet.exe build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug --no-build`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --no-build`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug --no-build`

## Save compatibility notes
- runtime board-environment descriptors remain read-only presentation data and do not enter save payloads
- campaign-enabled saves should continue to roundtrip unchanged because this slice adds no new module-owned state
