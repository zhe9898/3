## Goal
- make the warfare desk sandbox vary not just by campaign pressure but also by local regional character
- derive that local character from existing settlement and route projections only
- keep all regional flavor read-only so no new geography authority layer is implied

## Scope in
- `Zongzu.Presentation.Unity` warfare board read-model composition
- regional board descriptors derived from settlement security/prosperity and route names/roles
- presentation and integration tests for region-driven board variation
- UI / acceptance docs for the new read-only descriptors

## Scope out
- no new region/climate/terrain authority module
- no schema bump
- no save payload changes
- no new warfare rules or route simulation

## Affected modules
- `Zongzu.Presentation.Unity`
- presentation / integration tests
- UI / acceptance docs

## Save/schema impact
- no root schema bump planned
- no module schema bump planned
- regional board descriptors stay derived from existing settlement / route read models only

## Determinism risk
- low
- changes remain inside presentation composition, but campaign-enabled shell tests should still re-run

## Milestones
1. add bounded regional-profile descriptors to campaign board view models
2. derive those descriptors from settlement/world and route naming signals already in the bundle
3. extend tests to prove different local environments yield different board presentation
4. update docs and verify build/tests

## Tests to add/update
- `FirstPassPresentationShellTests`
- `M2LiteIntegrationTests`

## Rollback / fallback plan
- if region inference feels too noisy, keep only a single `RegionalProfileLabel`
- if route-name heuristics feel too brittle, fall back to settlement prosperity/security driven variation only

## Completion notes
- warfare shell now derives regional-profile and regional-backdrop descriptors from existing settlement projections plus local route naming signals only
- campaign board summaries now distinguish water-linked counties, hill-route passes, market centers, insecure border counties, and plain inland counties without adding any new geography authority rules
- presentation and integration coverage now asserts that different local-route environments produce different campaign-board descriptors while authority, save shape, and determinism remain unchanged

## Verification
- `C:\Program Files\dotnet\dotnet.exe build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug --no-build`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --no-build`
- `C:\Program Files\dotnet\dotnet.exe test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug --no-build`

## Save compatibility notes
- new regional descriptors remain read-only presentation data only
- campaign-enabled saves should roundtrip unchanged because no new module-owned state is added
