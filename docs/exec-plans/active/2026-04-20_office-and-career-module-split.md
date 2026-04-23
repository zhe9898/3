## Goal
- split the oversized `OfficeAndCareerModule` into concern-based partial files without changing monthly simulation behavior
- keep office drift, appointment updates, administrative aftermath, and query projection code easier to scan

## Scope in
- move `OfficeAndCareerModule` into a dedicated folder with partial files
- split by concern: module entrypoints, xun drift, career updates, order/campaign aftermath, administrative resolution, and queries
- preserve existing module key, cadence, queries, helper behavior, and nested support records
- run module and full-solution verification

## Scope out
- no balance or formula changes
- no contract, schema, or save changes
- no query-surface redesign
- no descriptor wording edits

## Affected modules
- `Zongzu.Modules.OfficeAndCareer`

## Save/schema impact
- none

## Determinism risk
- none
- refactor only

## Milestones
1. move `OfficeAndCareerModule` into a folder and split it into partial files
2. preserve all run entrypoints, helper behavior, and query projections
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if module tests and full solution tests remain green

## Rollback / fallback plan
- if the split introduces regressions, collapse the type back into one file without changing method bodies
