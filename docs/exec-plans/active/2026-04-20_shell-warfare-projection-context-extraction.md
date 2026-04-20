## Goal
- extract warfare surface data preparation into a dedicated projection context
- keep `WarfareCampaignShellAdapter` focused on warfare surface composition and mapping

## Scope in
- add a helper that precomputes ordered campaigns, ordered mobilization signals, ordered warfare affordances/receipts, settlement and trade-route lookups, active/peak counters, and lead-campaign selections
- reconnect `WarfareCampaignShellAdapter` through the helper
- preserve warfare summaries, campaign board ordering, mobilization ordering, and player-facing semantics
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new warfare fields
- no wording rewrites
- no changes to campaign sorting or aftermath behavior

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add warfare projection context
2. reconnect `WarfareCampaignShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing warfare shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures warfare surface composition, move only the local ordering/lookups back into `WarfareCampaignShellAdapter` and keep warfare behavior unchanged

## Result
- added `WarfareProjectionContext` as a presentation-local helper that precomputes ordered campaigns, ordered mobilization signals, ordered warfare affordances/receipts, settlement/trade-route lookups, active/peak counters, and lead-campaign selections
- reconnected `WarfareCampaignShellAdapter` through the helper for warfare surface composition and warfare summary preparation
- preserved warfare summaries, campaign board ordering, mobilization ordering, and player-facing semantics

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
