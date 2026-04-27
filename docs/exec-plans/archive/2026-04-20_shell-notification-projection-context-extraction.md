## Goal
- extract notification item projection and lead-guidance lookup into a dedicated presentation-local helper
- reduce `NotificationShellAdapter` and composer duplication without changing notification wording or family-lifecycle guidance behavior

## Scope in
- add a helper that precomputes notification item view models and per-notification `what next` guidance
- reconnect `NotificationShellAdapter` through the helper
- feed the same helper into `GreatHallShellAdapter` for lead-notice guidance
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new notification fields
- no wording rewrites
- no notification ordering changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add notification projection context helper
2. reconnect `NotificationShellAdapter` and composer
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing lead-notice and notification-center coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures notification behavior, move only the `what next` projection back into `NotificationShellAdapter` and keep surface behavior unchanged

## Result
- added `NotificationProjectionContext` as a presentation-local helper that precomputes notification item view models and per-notification `what next` guidance
- reconnected `NotificationShellAdapter` through the helper and fed the same context into `GreatHallShellAdapter` for lead-notice guidance
- kept notification ordering, notification-center wording, and family-lifecycle guidance behavior unchanged

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
