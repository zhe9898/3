## Goal
- expose same-settlement `HallDocket` items on desk settlement nodes as thin structured agenda rows
- keep the slice adapter-only so desk surfaces can read hall pressure without re-parsing summary text or changing shared contracts

## Scope in
- add a thin desk-settlement hall-agenda item viewmodel in `Zongzu.Presentation.Unity`
- map same-settlement `HallDocket` lead/secondary items into that adapter viewmodel
- keep the existing compact hall-agenda summary for fallback/compatibility
- add focused presentation tests
- update acceptance notes

## Scope out
- no changes to `Zongzu.Contracts`
- no authority changes
- no new application sorting logic
- no notification-center changes
- no shell object grammar fields in shared contracts

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- none

## Determinism risk
- none in authority
- adapter reads existing deterministic hall-docket projections only

## Milestones
1. add a thin settlement hall-agenda item adapter model
2. map same-settlement hall-docket items into desk node rows
3. keep the existing summary as a compact compatibility string
4. add focused shell coverage
5. run build/test verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if desk nodes feel too verbose, keep only the summary string and return an empty item list
- if some hall-docket paths are too sparse, keep rows thin and omit placeholder text

## Result notes
- desk settlement nodes now expose thin read-only hall-agenda rows derived from same-settlement hall-docket items
- rows stay adapter-side only: lane key, headline, phase label, and compact summary
- hall/application ordering remains unchanged

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
