## Goal
- let desk settlement nodes see which same-settlement matters are already on the monthly hall docket
- keep the slice adapter-only so desk surfaces reflect hall prioritization without changing application ordering or notification behavior

## Scope in
- add a thin desk-settlement hall-agenda summary field in `Zongzu.Presentation.Unity`
- derive that summary from existing `HallDocket` lead/secondary items by matching settlement id only
- keep the output compact and read-only
- add focused presentation tests
- update acceptance notes

## Scope out
- no changes to `Zongzu.Contracts`
- no authority changes
- no new sorting logic in application
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
1. identify settlement-node hook for hall agenda projection
2. derive same-settlement hall agenda summary from hall-docket items
3. keep hall / notification / authority ordering untouched
4. add focused shell coverage
5. run build/test verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if the desk field feels too noisy, keep it empty unless two or more hall-docket items hit the same settlement
- if mixed-lane wording feels awkward, keep only headlines and drop phase labels from the desk summary

## Result notes
- desk settlement nodes now expose a read-only hall-agenda summary derived from same-settlement hall-docket items
- the summary may include both the lead docket item and secondary items when they point at the same settlement
- application ordering and notification behavior remain unchanged

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
