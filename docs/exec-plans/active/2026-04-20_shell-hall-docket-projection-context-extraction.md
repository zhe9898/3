## Goal
- extract hall-docket selection and per-settlement projection context into a dedicated presentation-local helper
- reduce `HallDocketShellAdapter` filtering/setup noise without changing hall or desk agenda wording

## Scope in
- add a helper that precomputes the usable lead item, filtered secondary items, and per-settlement hall-docket item groups
- reconnect `HallDocketShellAdapter` through the helper
- keep great-hall lead, secondary dockets, desk hall-agenda summaries, and lead markers behaviorally unchanged
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new hall or desk fields
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
1. add hall-docket projection context helper
2. reconnect `HallDocketShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing hall/desk shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures hall or desk agenda composition, move only the lead/secondary/per-settlement selection back into `HallDocketShellAdapter` and keep summary behavior unchanged

## Result
- added `HallDocketProjectionContext` as a presentation-local helper that precomputes the usable hall-docket lead item, filtered secondary items, and per-settlement hall-agenda item groups
- reconnected `HallDocketShellAdapter` through the helper without changing great-hall lead behavior, secondary docket hydration, desk hall-agenda summaries, or desk lead-marker behavior

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
