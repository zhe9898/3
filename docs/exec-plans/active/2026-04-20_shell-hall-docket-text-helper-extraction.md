## Goal
- extract hall-docket summary and agenda text glue into a dedicated presentation-local helper
- keep `HallDocketShellAdapter` focused on hall/desk agenda mapping rather than summary string assembly

## Scope in
- add a helper that owns hall lead guidance, great-hall secondary docket summary text, and desk hall-agenda summary/item summary text
- reconnect `HallDocketShellAdapter` through the helper
- preserve existing hall-docket ordering, agenda grouping, lane keys, and player-facing wording
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new hall-docket fields
- no wording rewrites
- no ordering changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add hall-docket text helper
2. reconnect `HallDocketShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing hall-docket shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures hall-docket composition, move only the local summary glue back into `HallDocketShellAdapter` and keep hall/desk behavior unchanged

## Result
- added `HallDocketTextAdapter` as a presentation-local helper for hall lead guidance, great-hall secondary docket summary text, and desk hall-agenda summary/item summary text
- reconnected `HallDocketShellAdapter` through the helper so the adapter stays focused on hall and desk agenda mapping
- preserved hall-docket ordering, agenda grouping, lane-key extraction, and existing player-facing wording

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
