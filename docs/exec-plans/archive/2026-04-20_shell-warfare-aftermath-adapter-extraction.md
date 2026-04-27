## Goal
- extract warfare aftermath and docket projection logic out of `WarfareCampaignShellAdapter`
- reduce the largest presentation hotspot without changing shell behavior

## Scope in
- move warfare aftermath signal composition into a dedicated adapter
- move great-hall, settlement, and campaign-board aftermath summary helpers into the new adapter
- reconnect warfare, great-hall, and desk callers through adapter calls only
- add one focused shell regression for aftermath-empty fallback continuity

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no authoritative warfare or aftermath rules changes
- no new shell fields or UI-object grammar

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only adapter extraction

## Milestones
1. extract aftermath/docket helpers into `WarfareAftermathShellAdapter`
2. reconnect warfare/great-hall/desk callers through the adapter
3. add one focused shell regression and run presentation/full verification

## Tests to add/update
- add one focused `Presentation.Unity` test for no-aftermath fallback continuity
- keep existing warfare summary and campaign-board tests green

## Rollback / fallback plan
- if extraction reveals a hidden warfare-board dependency, restore only the minimal helper and move it into the correct adapter in a follow-up slice

## Result
- extracted warfare aftermath and docket projection into `WarfareAftermathShellAdapter`
- rewired great-hall, desk settlement, and campaign-board aftermath summaries through the new adapter
- repaired `WarfareCampaignShellAdapter` after a failed bulk removal corrupted the file encoding/syntax, then reapplied the extraction as a clean minimal diff
- added a focused shell regression for no-aftermath fallback continuity and kept the existing warfare summary / campaign-board coverage green

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
