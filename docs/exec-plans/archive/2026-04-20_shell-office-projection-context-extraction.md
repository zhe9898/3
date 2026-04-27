## Goal
- extract office surface data precomputation into a dedicated presentation-local helper
- reduce `OfficeShellAdapter` setup noise without changing office wording, fallback behavior, or command ordering

## Scope in
- add a helper that precomputes office counts, ordered appointments/jurisdictions, filtered office commands/receipts, and settlement-name lookup
- reconnect `OfficeShellAdapter` through the helper
- keep office summaries, fallback strings, appointment rows, and jurisdiction rows behaviorally unchanged
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new office fields
- no wording rewrites
- no sorting changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add office projection context helper
2. reconnect `OfficeShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing office shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures office surface composition, move only the precomputed selections back into `OfficeShellAdapter` and keep office behavior unchanged

## Result
- added `OfficeProjectionContext` as a presentation-local helper that precomputes office counts, settlement-name lookup, ordered appointments/jurisdictions, and filtered office command/receipt lanes
- reconnected `OfficeShellAdapter` through the helper without changing fallback strings, appointment/jurisdiction wording, or office command ordering

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
