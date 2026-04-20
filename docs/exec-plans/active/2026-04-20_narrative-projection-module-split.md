## Goal
- split the oversized `NarrativeProjectionModule` into concern-based partial files without changing notification projection behavior
- keep event-to-notification assembly, context pull logic, title/surface derivation, and query projection code easier to scan

## Scope in
- move `NarrativeProjectionModule` into a dedicated folder with partial files
- split by concern: module entrypoints, trace/context building, surface/title routing, what-next builders, and queries
- preserve existing retention behavior, notification text generation, query surfaces, and snapshot cloning
- run module and full-solution verification

## Scope out
- no notification wording changes
- no contract or schema changes
- no save changes
- no projection feature redesign

## Affected modules
- `Zongzu.Modules.NarrativeProjection`

## Save/schema impact
- none

## Determinism risk
- none
- projection refactor only

## Milestones
1. move `NarrativeProjectionModule` into a folder and split it into partial files
2. preserve all notification assembly helpers and query projections
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if module tests and full solution tests remain green

## Rollback / fallback plan
- if the split introduces regressions, collapse the type back into one file without changing method bodies
