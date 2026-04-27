## Goal
- move the first documented `xun` behavior into real runtime ownership for `TradeAndIndustry`
- let petty trade, route friction, and gray-route drift breathe inside the month while keeping readable trade review month-bound

## Scope in
- add bounded `RunXun` behavior for `TradeAndIndustry`
- update market / route / black-route-ledger pressure inside xun
- keep clan cash / grain / debt settlement and readable trade outcome month-bound
- add focused module coverage for xun behavior

## Scope out
- no schema bump
- no new player command surfaces
- no month-end explanation rewrite
- no public-life wording pass
- no same-month trade interrupt window

## Affected modules
- `Zongzu.Modules.TradeAndIndustry`
- `Zongzu.Modules.TradeAndIndustry.Tests`
- this exec-plan note

## Save/schema impact
- no root schema bump
- no module schema bump
- xun behavior remains deterministic runtime evolution over existing owned state

## Determinism risk
- low to medium
- xun pulse should stay arithmetic and query-first
- no foreign writes
- month-end trade explanation must remain the primary readable business summary

## Milestones
1. define bounded `TradeAndIndustry` xun pulse over market / route / gray-route state
2. add focused xun module tests
3. run build/test verification

## Tests to add/update
- `Zongzu.Modules.TradeAndIndustry.Tests`
- `Zongzu.Integration.Tests`

## Rollback / fallback plan
- if xun trade drift makes month-end outcomes too jumpy, keep xun on market / route heat only and move gray-route ledger drift back to month
- if xun updates blur route readability, leave labels month-bound and only keep numeric pressure in xun

## Open questions
- when to let `PublicLifeAndRumor` read or stage xun trade heat without turning the shell into pulse spam
- when to let office/order same-month pressure meaningfully bend petty trade without making trade resolution too coupled

## Result notes
- `TradeAndIndustry` now owns a bounded deterministic `RunXun` pulse over market heat, gray-route ledger drift, and route constraint pressure while keeping clan cash / grain / debt settlement month-bound
- `shangxun` now nudges market price, demand, and local risk from prosperity, household distress, and black-route pressure
- `zhongxun` now nudges gray-route ledger state such as shadow price, diversion share, blocked shipments, and seizure risk from the same trade-owned ledger seam
- `xiaxun` now lets late-month route friction refresh active-route risk and constraint labels without creating xun diff/event spam
- the first pass exposed one bootstrap assumption bug: not every active route had a matching social narrative, so the xun route pass now falls back to a neutral clan narrative snapshot rather than throwing
- long-run local-conflict diagnostics were re-verified after the trade xun slice; growth save-payload ceiling now needs a small calibration to `46000`

## Verification
- `dotnet test .\tests\Zongzu.Modules.TradeAndIndustry.Tests\Zongzu.Modules.TradeAndIndustry.Tests.csproj -c Debug`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --filter "FullyQualifiedName~BootstrapWorld_IsDeterministicAcrossTwelveMonths|FullyQualifiedName~CampaignSandboxBootstrap_ActivatesWarfareCampaignAndSurfacesReadOnlyBoard"`
- `dotnet test .\tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj -c Debug --filter "FullyQualifiedName~SaveCodec_RoundtripPreservesM2LiteSimulationState|FullyQualifiedName~PrepareForLoad_SameVersionSave_PassesThroughCurrentSchemas"`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
