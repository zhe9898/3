## Goal
- move the first visible lived-pressure behavior from doc-only cadence into real `xun` execution for `WorldSettlements` and `PublicLifeAndRumor`
- keep month-end readability intact so hall / desk still read as one review shell rather than three noisy sub-turns

## Scope in
- add bounded `RunXun` behavior for `WorldSettlements`
- add bounded `RunXun` behavior for `PublicLifeAndRumor`
- keep public-life readable diffs / events on the month pass only
- add targeted module coverage for xun refresh behavior
- update execution notes in the active cadence plan if needed

## Scope out
- no schema bump in this first slice
- no full xun rewrite across every module
- no UI changes
- no seasonal execution behavior yet

## Affected modules
- `Zongzu.Modules.WorldSettlements`
- `Zongzu.Modules.PublicLifeAndRumor`
- module tests for those modules
- active cadence exec-plan notes

## Save/schema impact
- no root schema bump
- no module schema bump
- xun behavior remains derived from existing owned state plus deterministic runtime context

## Determinism risk
- low to medium
- xun drift must remain deterministic and stable under repeated seeds
- month-end readable output must not multiply into three separate notice floods

## Milestones
1. add bounded xun drift to `WorldSettlements`
2. add xun refresh behavior to `PublicLifeAndRumor` while keeping readable notice output on the month pass
3. add/update focused module tests
4. run build/test verification

## Tests to add/update
- `Zongzu.Modules.WorldSettlements.Tests`
- `Zongzu.Modules.PublicLifeAndRumor.Tests`

## Rollback / fallback plan
- if month-end readability degrades, keep xun state refresh but drop xun diff output first
- if public-life xun refresh proves too noisy, keep xun recomputation internal and leave only world settlement drift active for this slice

## Open questions
- which next modules should move from declared xun cadence into true xun-owned behavior after this first public-surface slice
- whether later month-end projection should explicitly distinguish xun-carried drift from month-consolidated change

## Completion notes
- `WorldSettlements` now spends its first real `xun` ownership on bounded short-band security/prosperity drift. The drift is deterministic, produces one internal diff entry per changed settlement, and does not emit `SettlementPressureChanged` during xun pulses.
- `WorldSettlements.RunMonth` is intentionally quiet in this slice so the three inner pulses become the real owner of short-band settlement movement. Month-end consolidation can be added later without re-breaking the cadence boundary.
- `PublicLifeAndRumor` now recomputes settlement public pulse during `RunXun`, but xun passes stay projection-silent: no readable diffs and no public events are emitted during the inner pulses.
- `PublicLifeAndRumor.RunMonth` still owns the readable month-end surface. It reuses the same pulse builder, then emits the single diff / primary public-life event only on the month pass when thresholds warrant it.
- Focused tests now cover:
  - `WorldSettlements` xun drift bounds and quiet month pass
  - `PublicLifeAndRumor` xun refresh without readable output
  - `PublicLifeAndRumor` month-end event emission and seasonal cadence changes
  - `PublicLifeAndRumor` query projection cloning

## Verification
- `dotnet test .\tests\Zongzu.Modules.WorldSettlements.Tests\Zongzu.Modules.WorldSettlements.Tests.csproj -c Debug`
- `dotnet test .\tests\Zongzu.Modules.PublicLifeAndRumor.Tests\Zongzu.Modules.PublicLifeAndRumor.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
- Result: all passed. The solution build hit transient `MSB3026` retry warnings caused by a locked testhost copy in `Zongzu.Integration.Tests`, but the retry succeeded and no code changes were required.
