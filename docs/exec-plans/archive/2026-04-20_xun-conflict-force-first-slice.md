## Goal
- move the first documented `xun` behavior into real runtime ownership for `ConflictAndForce`
- let guard posture, escort motion, readiness, command grip, and active-response temperature breathe inside the month while keeping readable conflict settlement month-bound

## Scope in
- add bounded `RunXun` behavior for `ConflictAndForce`
- update escort posture, readiness, command capacity, and active-response mirrors over existing owned state
- let same-month `OrderAndBanditry` read refreshed force posture through the existing query seam
- keep readable diffs, event emission, commander harm, and larger local-conflict settlement month-bound
- add focused module coverage for xun behavior

## Scope out
- no schema bump
- no new conflict commands
- no xun diff or xun event output
- no new fatigue or non-campaign escort schema
- no UI or hall wording work

## Affected modules
- `Zongzu.Modules.ConflictAndForce`
- `Zongzu.Modules.ConflictAndForce.Tests`
- this exec-plan note

## Save/schema impact
- no root schema bump
- no module schema bump
- xun behavior remains deterministic runtime evolution over existing force-owned state

## Determinism risk
- low to medium
- xun pulse should stay arithmetic and query-first
- no foreign writes
- month-end local-conflict settlement must remain the authoritative visible cadence

## Milestones
1. define bounded `ConflictAndForce` xun pulse over escort posture, readiness, command grip, and response activation
2. keep larger guard / militia recompute and readable conflict resolution month-bound
3. add focused xun module tests
4. run build/test verification

## Tests to add/update
- `Zongzu.Modules.ConflictAndForce.Tests`
- `dotnet build / test` verification against full solution

## Rollback / fallback plan
- if xun force posture makes month-end conflict too jumpy, keep only response/trace refresh in xun and move readiness / escort deltas back to month
- if same-month order interaction feels too strong, keep xun force local to query visibility and narrow the response thresholds

## Open questions
- when to give `OfficeAndCareer` a matching first-pass `RunXun` so administrative support stops being purely month-carried
- whether later xun conflict slices need a dedicated non-campaign escort fatigue field, or whether month-bound route relief is enough

## Result notes
- `ConflictAndForce` now owns a bounded deterministic `RunXun` pulse over short-band escort posture, readiness, command grip, and active-response state
- `shangxun` now nudges escort posture and readiness from route pressure, support reserve, distress, and administrative support
- `zhongxun` now nudges command grip and hotspot pressure so active response can tip on inside the month without creating xun-readable spam
- `xiaxun` now lets calm surfaces cool escort posture while pressured surfaces hold the watch line, still without month-end conflict resolution
- xun active-conflict thresholds are now staged: `shangxun` only carries already-burning situations forward, while `zhongxun/xiaxun` can seat new force response under clearly higher disorder pressure
- xun force drift remains projection-silent in this slice: no xun diffs, no xun domain events, and no xun conflict-resolution outcomes

## Verification
- `dotnet test .\tests\Zongzu.Modules.ConflictAndForce.Tests\Zongzu.Modules.ConflictAndForce.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
