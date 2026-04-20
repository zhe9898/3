## Goal
- split the oversized `PublicLifeAndRumorModule` into concern-based partial files without changing public-surface simulation behavior
- keep settlement pulse refresh, cadence labeling, venue selection, and query projection code easier to scan

## Scope in
- move `PublicLifeAndRumorModule` into a dedicated folder with partial files
- split by concern: module entrypoints, settlement refresh, feature-source builders, event/cadence labeling, narrative summary builders, and queries
- preserve existing published events, labels, summary strings, and query surfaces
- run module and full-solution verification

## Scope out
- no content or formula changes
- no contract, schema, or save changes
- no projection wording edits

## Affected modules
- `Zongzu.Modules.PublicLifeAndRumor`

## Save/schema impact
- none

## Determinism risk
- none
- refactor only

## Milestones
1. move `PublicLifeAndRumorModule` into a folder and split it into partial files
2. preserve settlement pulse flow, helper behavior, and query projections
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if module tests and full solution tests remain green

## Rollback / fallback plan
- if the split introduces regressions, collapse the type back into one file without changing method bodies
