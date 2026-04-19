## Goal
- expose desk-settlement hall-agenda counts and distinct lane keys from existing same-settlement hall-docket rows
- keep the slice adapter-only so node markers can read hall coverage without recomputing hall ordering

## Scope in
- add thin desk-settlement hall-agenda count/lane-key fields in `Zongzu.Presentation.Unity`
- derive those fields from the existing adapter-side hall-agenda item list only
- add focused presentation coverage
- update acceptance notes

## Scope out
- no changes to `Zongzu.Contracts`
- no authority changes
- no new application ordering logic
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
1. add thin hall-agenda count/lane-key adapter fields to desk settlement nodes
2. derive count/lane keys from existing same-settlement hall-agenda rows
3. keep hall/application ordering untouched
4. add focused shell coverage
5. run build/test verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if lane keys feel too specific for current shell work, keep count only and leave lane keys empty
- if future adapter paths need richer grouping, derive them later from the same row list rather than changing contracts now

## Result notes
- desk settlement nodes now expose read-only `HallAgendaCount` and distinct `HallAgendaLaneKeys`
- both stay adapter-side and are derived from the existing same-settlement hall-agenda rows
- hall/application ordering remains unchanged

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
