## Goal
- extract repeated shell text-composition glue into one presentation-local helper
- reduce duplicate summary/guidance append logic without changing wording or lane behavior

## Scope in
- add a shared helper for combining non-empty shell text fragments and appending distinct suffix text
- reconnect hall-docket, governance, and notification adapters through the helper
- keep lane-local selection and wording ownership where they already live
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new shell fields
- no wording rewrites
- no lane-ordering changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add shared shell text helper
2. reconnect hall-docket, governance, and notification adapters
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if a helper obscures lane-local wording behavior, move only that lane back to local composition and keep the helper for the others

## Result
- added `ShellTextAdapter` as a presentation-local helper for combining non-empty shell text fragments and appending distinct suffix text
- rewired `HallDocketShellAdapter` to use the shared helper for lead guidance and agenda/secondary summary composition
- rewired `GovernanceShellAdapter` to use the shared helper for appending public-momentum summaries without changing lane selection
- rewired `NotificationShellAdapter` to use the shared helper for lifecycle prompt append logic without changing family-notification gating

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
