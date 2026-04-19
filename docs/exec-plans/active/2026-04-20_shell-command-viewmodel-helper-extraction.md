## Goal
- extract repeated command affordance and receipt view-model mapping into one presentation-local helper
- reduce adapter duplication without changing shell wording, ordering, or fields

## Scope in
- add a shared helper in `Zongzu.Presentation.Unity` for mapping command snapshots to shell view models
- reconnect family, office, public-life, and warfare adapters through the helper
- keep filtering, ordering, and lane-specific selection in each owning adapter
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new shell fields or wording changes
- no changes to authoritative command behavior

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add shared command view-model helper
2. reconnect family, office, public-life, and warfare adapters
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing shell tests remain green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if a shared helper collapses lane-specific behavior, move only the offending lane back to local mapping and keep the rest shared

## Result
- added `CommandShellAdapter` as a presentation-local helper for the common `PlayerCommand*Snapshot -> *ViewModel` mapping path
- rewired family, office, and public-life command projection to use the shared helper
- kept warfare receipts local because they still require `RenderCampaignSurfaceText`, while moving warfare affordance projection onto the shared helper
- preserved all lane-local filtering, ordering, and wording behavior inside the owning adapters

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
