## Goal
- extract warfare campaign board regional/profile text helpers into a dedicated presentation-local helper
- keep `WarfareCampaignShellAdapter` focused on warfare surface composition instead of board/profile text assembly

## Scope in
- add a helper that owns campaign condition labels, campaign board surface/atmosphere/marker text, route-mix description, lead-route selection, and regional-profile selection
- reconnect `WarfareCampaignShellAdapter` through the helper
- preserve campaign board ordering, warfare summaries, regional labels, and player-facing semantics
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
1. add warfare campaign board text helper
2. reconnect `WarfareCampaignShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing warfare shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures warfare campaign composition, move only the regional/profile text glue back into `WarfareCampaignShellAdapter` and keep warfare behavior unchanged

## Result
- added `WarfareCampaignBoardTextAdapter` as a presentation-local helper for campaign condition labels, campaign board surface/atmosphere/marker text, route-mix description, lead-route selection, and regional-profile selection
- reconnected `WarfareCampaignShellAdapter` through the helper so the adapter stays focused on warfare surface composition
- preserved campaign board ordering, warfare summaries, regional labels, and existing player-facing semantics

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
