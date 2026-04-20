## Goal
- surface `HallDocketStack.SecondaryItems` through the first-pass great-hall adapter as a read-only secondary-matters list
- keep the slice adapter-only so hall can show next matters without changing contracts, authority, or notification ordering

## Scope in
- add a thin great-hall secondary-docket viewmodel in `Zongzu.Presentation.Unity`
- map `HallDocket.SecondaryItems` into that adapter viewmodel
- keep summaries compact and derived from existing hall-docket text only
- add focused presentation tests
- update acceptance notes

## Scope out
- no changes to `Zongzu.Contracts`
- no authority changes
- no new sorting rules in application
- no notification-center behavior changes
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
1. add a thin great-hall secondary-docket adapter model
2. map hall-docket secondary items into great-hall summaries
3. keep hall lead / notification-center behavior intact
4. add focused shell coverage
5. run build/test verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if the secondary summaries feel too verbose, keep the list but trim them to `WhyNowSummary` only
- if some hall-docket paths prove too sparse, return an empty secondary list rather than inventing placeholder items

## Result notes
- great-hall now exposes a read-only secondary-docket list derived from `HallDocket.SecondaryItems`
- each secondary item stays thin: lane key, target label, headline, phase label, and a compact derived summary
- notification center remains notification-driven, and authority/application sorting stays unchanged

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
