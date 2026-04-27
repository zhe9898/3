## Goal
- thread the new governance public-momentum projection into the first-pass shell adapter
- keep the slice adapter-only so hall / desk can read the trend without adding authority logic or shell-object fields to shared contracts

## Scope in
- update `FirstPassPresentationShell` to prefer governance-lane / governance-docket summaries when available
- carry governance public-momentum text into existing great-hall and desk-settlement governance summaries
- preserve current office-based fallback when governance projections are absent
- add focused presentation tests
- update acceptance notes

## Scope out
- no module state changes
- no schema bump
- no changes to application read-model contracts
- no new presentation viewmodel fields
- no hall object grammar fields

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- none
- adapter consumes existing read models only

## Determinism risk
- none in authority
- adapter joins existing deterministic projections only

## Milestones
1. identify first-pass hall / desk governance summary join points
2. prefer governance projection summaries when present
3. keep office fallback untouched for projection-absent paths
4. add focused shell tests
5. run build/test verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if shell wording feels too presentation-shaped, keep the adapter hooks but stop appending the momentum clause
- if governance projection coverage is too sparse for shell tests, keep desk/hall on the old office fallback until more application bundles carry the lane

## Result notes
- first-pass hall governance summary now prefers governance lane / docket momentum when present
- desk settlement governance summary now prefers governance lane momentum when present
- office-only fallback remains unchanged for stable M2/M3 paths and governance-projection-absent tests

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
