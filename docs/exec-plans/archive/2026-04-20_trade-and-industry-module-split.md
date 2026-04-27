## Goal
- split the oversized `TradeAndIndustryModule` into concern-based partial files without changing trade simulation behavior
- keep xun pulses, monthly clan trade updates, campaign spillover, black-route ledger logic, and query projections easier to scan

## Scope in
- move `TradeAndIndustryModule` into a dedicated folder with partial files
- split by concern: module entrypoints, xun pulse helpers, campaign-event spillover, order/ledger helpers, and queries
- preserve existing commands, events, query surfaces, nested support records, and black-route ledger behavior
- run module and full-solution verification

## Scope out
- no balance or formula changes
- no command/event contract changes
- no schema or save changes
- no descriptor wording edits

## Affected modules
- `Zongzu.Modules.TradeAndIndustry`

## Save/schema impact
- none

## Determinism risk
- none
- refactor only

## Milestones
1. move `TradeAndIndustryModule` into a folder and split it into partial files
2. preserve all run entrypoints, helper behavior, and query projections
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if module tests and full solution tests remain green

## Rollback / fallback plan
- if the split introduces regressions, collapse the type back into one file without changing method bodies
