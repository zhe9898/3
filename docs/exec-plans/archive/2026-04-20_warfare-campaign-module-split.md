## Goal
- split the oversized `WarfareCampaignModule` into concern-based partial files without changing campaign board behavior
- keep mobilization signaling, campaign board derivation, directive routing, and query projection code easier to scan

## Scope in
- move `WarfareCampaignModule` into a dedicated folder with partial files
- split by concern: module entrypoints, activation/metric calculations, directive/route builders, and queries
- preserve existing commands, events, query surfaces, and campaign descriptor behavior
- run module and full-solution verification

## Scope out
- no balance or formula changes
- no command/event contract changes
- no schema or save changes
- no descriptor wording edits

## Affected modules
- `Zongzu.Modules.WarfareCampaign`

## Save/schema impact
- none

## Determinism risk
- none
- refactor only

## Milestones
1. move `WarfareCampaignModule` into a folder and split it into partial files
2. preserve all run entrypoints, helper behavior, and query projections
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if module tests and full solution tests remain green

## Rollback / fallback plan
- if the split introduces regressions, collapse the type back into one file without changing method bodies
