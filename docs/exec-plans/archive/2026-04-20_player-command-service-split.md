## Goal
- split the oversized `PlayerCommandService` into concern-based partial files without changing command routing or outcomes
- keep family, office, public-life, and warfare command handling easier to scan while preserving the current surface

## Scope in
- move `PlayerCommandService` into a dedicated folder with partial files
- split by concern: top-level dispatch, family commands, office commands, public-life/order commands, and warfare bridge
- preserve existing namespaces, helpers, labels, and result payloads
- run application and full-solution verification

## Scope out
- no command wording changes
- no module behavior changes
- no contract or schema changes
- no command-surface redesign

## Affected modules
- `Zongzu.Application`

## Save/schema impact
- none

## Determinism risk
- none
- application-layer refactor only

## Milestones
1. move `PlayerCommandService` into a folder and split it into partial files
2. preserve all current command entrypoints and helper behavior
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if build and full solution tests remain green

## Rollback / fallback plan
- if the partial split introduces regressions, collapse the type back into one file without changing method bodies
