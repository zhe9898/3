## Goal
- split the oversized `SimulationBootstrapper` into concern-based partial files without changing bootstrap or migration behavior
- keep module registration, seed setup, stress setup, load paths, and migration helpers easier to scan

## Scope in
- move `SimulationBootstrapper` into a dedicated folder with partial files
- split by concern: module sets, bootstrap/load entrypoints, seeding/stress helpers, and migrations
- preserve all public static entrypoints and existing namespaces
- run application and full-solution verification

## Scope out
- no bootstrap behavior changes
- no migration logic changes
- no feature-manifest or schema changes
- no test rewrites unless the build requires them

## Affected modules
- `Zongzu.Application`

## Save/schema impact
- none

## Determinism risk
- none
- refactor only

## Milestones
1. move `SimulationBootstrapper` into a folder and split it into partial files
2. preserve all current entrypoints and helper behavior
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if full build and solution tests remain green

## Rollback / fallback plan
- if the partial split introduces regressions, collapse the type back into one file without changing method bodies
