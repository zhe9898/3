## Goal
- move the first documented `xun` behavior into real runtime ownership for `OrderAndBanditry`
- let disorder flare, intimidation heat, patrol weakness, and opportunistic gray-road pressure breathe inside the month while keeping readable order review month-bound

## Scope in
- add bounded `RunXun` behavior for `OrderAndBanditry`
- update disorder / route / coercion / black-route pressure over existing owned state
- refresh order-owned mirrors such as response activation, paper compliance, implementation drag, shielding, suppression relief, and retaliation risk inside `xun`
- keep month-end readable diffs, event emission, and explicit pressure wording as the primary visible order output
- add focused module coverage for xun behavior

## Scope out
- no schema bump
- no new order command verbs
- no xun diff or xun event output
- no office or conflict schema changes
- no UI or hall wording work

## Affected modules
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.OrderAndBanditry.Tests`
- this exec-plan note

## Save/schema impact
- no root schema bump
- no module schema bump
- xun behavior remains deterministic runtime evolution over existing order-owned state

## Determinism risk
- low to medium
- xun pulse should stay arithmetic and query-first
- no foreign writes
- month-end readable order review must remain the authoritative visible cadence

## Milestones
1. define bounded `OrderAndBanditry` xun pulse over opening flare, road opportunism, and late-month hardening
2. refresh order-owned mirrors from existing office / conflict / trade query seams during xun
3. add focused xun module tests
4. run build/test verification

## Tests to add/update
- `Zongzu.Modules.OrderAndBanditry.Tests`
- `dotnet build / test` verification against full solution

## Rollback / fallback plan
- if xun order drift makes month-end order review too jumpy, keep only mirror refresh in xun and move pressure deltas back to month
- if office/conflict refresh feels too coupled, keep xun order on settlement / household / social / trade inputs only and leave mirrors month-bound

## Open questions
- when to let `ConflictAndForce` and `OfficeAndCareer` gain matching first-pass `RunXun` behavior so order no longer reads only their month-carried state
- when to let `TradeAndIndustry` consume same-month order xun heat without changing module execution order or creating hidden write loops

## Result notes
- `OrderAndBanditry` now owns a bounded deterministic `RunXun` pulse over near-band disorder pressure
- `shangxun` now advances opening disorder flare and intimidation heat from settlement security, household distress, local fear/grudge, and current shielding posture
- `zhongxun` now advances road opportunism and gray-road pressure from active routes, shadow-ledger drift, implementation drag, and suppression-window weakness
- `xiaxun` now refreshes late-month hardening / cooling while also pulling current office and conflict mirrors back into order-owned query state
- xun order drift remains projection-silent in this slice: no xun diffs, no xun domain events, and no xun-readable pressure wording

## Verification
- `dotnet test .\tests\Zongzu.Modules.OrderAndBanditry.Tests\Zongzu.Modules.OrderAndBanditry.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
