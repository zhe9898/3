## Goal
- extract family shell projection logic out of `FirstPassPresentationShell`
- keep family hall/council wording and command selection behavior unchanged while making the shell composer thinner

## Scope in
- move great-hall family summary logic into a dedicated adapter
- move family-council projection logic into a dedicated adapter
- move lifecycle prompt selection and family command priority helpers into that adapter
- keep current shell coverage green

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no authoritative family rules, commands, or projection behavior changes
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
1. extract family summary/council/lifecycle helpers into `FamilyShellAdapter`
2. reconnect `FirstPassPresentationShell` through adapter calls only
3. rely on existing family shell coverage and run presentation/full verification

## Tests to add/update
- no new behavior tests required
- keep existing family shell tests green

## Rollback / fallback plan
- if extraction reveals a hidden composer dependency, restore only the minimal helper and move it into the adapter in a follow-up slice

## Result
- extracted family hall/council projection logic into `FamilyShellAdapter`
- moved lifecycle prompt selection and family command priority helpers out of `FirstPassPresentationShell`
- reduced the shell composer to adapter calls for family lane output, without changing contracts or authority behavior
- existing family shell coverage remained green
- verification passed:
  - `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
  - `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
  - `dotnet build .\Zongzu.sln -c Debug -m:1`
  - `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
