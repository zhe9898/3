## Goal
- split the oversized `ConflictAndForceModule` into concern-based partial files without changing force simulation behavior
- keep xun posture updates, campaign fallout, conflict activation, and query projection code easier to scan

## Scope in
- move `ConflictAndForceModule` into a dedicated folder with partial files
- split by concern: module entrypoints, xun posture helpers, campaign-fallout handling, force/conflict modeling, and queries
- preserve existing commands, events, query surfaces, nested support types, and response-state behavior
- run module and full-solution verification

## Scope out
- no balance or formula changes
- no command/event contract changes
- no schema or save changes
- no descriptor wording edits

## Affected modules
- `Zongzu.Modules.ConflictAndForce`

## Save/schema impact
- none

## Determinism risk
- none
- refactor only

## Milestones
1. move `ConflictAndForceModule` into a folder and split it into partial files
2. preserve all run entrypoints, helper behavior, and query projections
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if module tests and full solution tests remain green

## Rollback / fallback plan
- if the split introduces regressions, collapse the type back into one file without changing method bodies
