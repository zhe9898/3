## Result
- completed the `FamilyCoreModule` concern split with no schema or behavior changes
- verification passed with focused FamilyCore tests, serial solution build, and serial solution test run

## Goal
- split the oversized `FamilyCoreModule` into concern-based files without changing family simulation behavior
- keep xun/month scheduling, lifecycle resolution, formula helpers, queries, and internal support types easier to scan

## Scope in
- keep `FamilyCoreModule.cs` focused on metadata, `RunXun`, `RunMonth`, and `HandleEvents`
- move lifecycle helpers into `FamilyCoreModule.Lifecycle.cs`
- move formula and analysis helpers into `FamilyCoreModule.Formulas.cs`
- move query implementation into `FamilyCoreQueries.cs`
- move module-shared signal records into `FamilyCoreSignals.cs`
- move internal trade-shock event constants into `FamilyCoreEventTypes.cs`

## Scope out
- no formula, balance, event, command, or projection wording changes
- no schema version changes
- no save migration changes
- no new gameplay rules

## Affected modules
- `Zongzu.Modules.FamilyCore`

## Save/schema impact
- none
- all moved code keeps the existing `FamilyCore` state shape and module schema version

## Determinism risk
- low
- refactor only; deterministic ordering, event emission, and registry calls should remain byte-for-byte equivalent in behavior

## Milestones
1. split the current module file into partial/helper files by responsibility
2. preserve all run entrypoints, helper behavior, and query projections
3. run build/test verification and record results

## Tests to add/update
- no new tests required because the focused module tests and existing solution tests remain green

## Rollback / fallback plan
- if the split introduces compile or behavior regressions, collapse the moved helpers back into `FamilyCoreModule.cs` while preserving the original method bodies

## Open questions
- none
