## Goal
- extract office-facing text rendering and pressure-summary helpers into a dedicated presentation-local helper
- keep `OfficeShellAdapter` focused on view-model mapping without changing office wording, fallback behavior, or command ordering

## Scope in
- add a helper that owns office title/task/category rendering plus pressure-summary formatting
- reconnect `OfficeShellAdapter` through the helper
- preserve existing office copy and office surface behavior
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
1. add office text helper
2. reconnect `OfficeShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing office shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures office composition, move only the local text glue back into `OfficeShellAdapter` and keep office behavior unchanged

## Result
- added `OfficeShellTextAdapter` as a presentation-local helper for office title/task/category rendering, office pressure summary text, and petition outcome formatting
- reconnected `OfficeShellAdapter` through the helper so the adapter stays focused on office surface view-model mapping
- preserved existing office fallback strings, appointment rows, jurisdiction rows, and command ordering

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
