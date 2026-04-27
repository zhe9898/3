## Goal
- extract family-council data preparation into a dedicated projection context
- keep `FamilyShellAdapter` focused on family surface mapping and existing family wording

## Scope in
- add a helper that precomputes clan narrative lookup, ordered family affordances/receipts, ordered clans, and family summary counts
- reconnect `FamilyShellAdapter` through the helper
- preserve great-hall family summary, family council summary, clan lifecycle wording, and command ordering
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new family fields
- no wording rewrites
- no sorting changes beyond moving existing sorting into the helper

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add family projection context
2. reconnect `FamilyShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing family shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures family council composition, move only the ordering/count selection back into `FamilyShellAdapter` and keep family wording unchanged

## Result
- added `FamilyProjectionContext` as a presentation-local helper that precomputes clan narrative lookup, ordered family affordances/receipts, ordered clans, and family council summary counts
- reconnected `FamilyShellAdapter` through the helper so the adapter stays focused on family summary and tile mapping
- preserved great-hall family summary, family council wording, lifecycle prompts, and family command ordering

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
