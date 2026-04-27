## Goal
- extract notification and debug shell projection logic out of `FirstPassPresentationShell`
- keep the shell composer focused on surface assembly rather than lane-specific adapter work

## Scope in
- move notification-center mapping into a dedicated adapter
- move debug-panel mapping into a dedicated adapter
- preserve current shell wording and read-model boundaries
- add focused debug shell coverage

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no authoritative rule or projection behavior changes
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
1. extract notification-center projection into `NotificationShellAdapter`
2. extract debug-panel projection into `DebugShellAdapter`
3. keep `FirstPassPresentationShell` as a thin composer
4. add focused debug coverage and run presentation/full verification

## Tests to add/update
- add one focused `Presentation.Unity` test for debug adapter continuity
- keep existing notification shell tests green

## Rollback / fallback plan
- if extraction exposes a hidden composer dependency, restore the minimal helper and move it into the correct adapter in a follow-up slice

## Result
- extracted notification-center projection into `NotificationShellAdapter`
- extracted debug-panel projection into `DebugShellAdapter`
- reduced `FirstPassPresentationShell` to composer-level calls for notification and debug lanes, without changing contracts or authority behavior
- added focused debug shell coverage in `FirstPassPresentationShellTests`
- verification passed:
  - `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
  - `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
  - `dotnet build .\Zongzu.sln -c Debug -m:1`
  - `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
