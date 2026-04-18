## Goal
- activate the first `WarfareCampaign.Lite` slice as a campaign-board extension of the existing world simulation
- keep warfare downstream of local force, settlement, and office conditions
- surface warfare as a read-only desk-sandbox campaign board, not a detached tactics game

## Scope in
- active `WarfareCampaign` module project and tests project
- campaign-level owned state, queries, and deterministic monthly update
- dedicated campaign-enabled bootstrap/load path
- read-only campaign board export in the presentation read model and shell
- narrative projection support for campaign notices
- persistence, determinism, boundary, and shell coverage
- docs and acceptance updates

## Scope out
- no unit micro
- no RTS battlefield map
- no player authority UI for direct battlefield control
- no `black-route` rules
- no `WarfareCampaign` schema migration beyond schema `1` activation

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Modules.WarfareCampaign`
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Presentation.Unity`
- `Zongzu.Persistence.Tests`
- `Zongzu.Integration.Tests`
- `Zongzu.Modules.WarfareCampaign.Tests`
- warfare / presentation / acceptance docs

## Save/schema impact
- no root schema bump planned
- add active `WarfareCampaign` module schema `1`
- add a new campaign-enabled manifest/load path only; existing M2/M3/governance-lite paths remain unchanged

## Determinism risk
- medium
- this slice adds a new authority module and read-only campaign board, so deterministic replay, save roundtrip, and boundary isolation must be re-verified

## Milestones
1. add the active `WarfareCampaign` project, state, and query implementation
2. wire a campaign-enabled bootstrap/load path without mutating existing stable paths
3. export campaign board data into the read model and read-only shell
4. teach narrative projection to explain campaign notices
5. update tests, docs, and verification

## Tests to add/update
- `WarfareCampaignModuleTests`
- `M2LiteIntegrationTests`
- `SaveRoundtripTests`
- `FirstPassPresentationShellTests`
- docs / acceptance updates

## Rollback / fallback plan
- if campaign board surfacing feels too heavy, keep the module active but collapse presentation to summary-only strings
- if narrative event wording proves noisy, retain the campaign state and queries while reducing event titles/tiers to background notices
- if campaign activation destabilizes the governance-lite path, keep the module and tests but disable the new bootstrap until the integration gap is fixed

## Verification target
- `build` on the full solution
- targeted warfare, persistence, integration, and presentation tests
- full solution test pass if targeted tests are green

## Save compatibility notes
- old bootstraps and load paths stay campaign-free
- `WarfareCampaign` activates only in the new campaign-enabled path
- no migration path is planned for preflight-only reserved seams because those seams never shipped as an active registered module

## Completion notes
- `WarfareCampaign.Lite` is now an active module with schema `1`, owning bounded campaign-board state plus mobilization-signal snapshots.
- a dedicated `CreateP3CampaignSandboxBootstrap` / `LoadP3CampaignSandbox` path enables warfare-lite without mutating stable M2, M3, or governance-lite manifests.
- read-only campaign boards now surface through the application read-model bundle and the first-pass shell; desk-sandbox nodes, the great hall, and the dedicated warfare surface remain presentation-only.
- `NarrativeProjection` now explains campaign mobilization, front-pressure, supply-strain, and aftermath events without introducing tactical micro or authority UI.

## Verification
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Modules.WarfareCampaign.Tests\Zongzu.Modules.WarfareCampaign.Tests.csproj' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' build 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug --no-build`
