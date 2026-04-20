## Goal
- split the oversized `PresentationReadModelBuilder` into concern-based partial files without changing behavior
- keep application-layer responsibilities readable while preserving current namespaces and call surfaces

## Scope in
- move `PresentationReadModelBuilder` into a dedicated folder with partial files
- split the class by concern: core composition, hall docket, player commands, governance, and diagnostics
- keep `BuildForM2` and all existing helper behavior unchanged
- run application and full-solution verification

## Scope out
- no projection wording changes
- no contract changes
- no feature behavior changes
- no `SimulationBootstrapper` split in this pass

## Affected modules
- `Zongzu.Application`

## Save/schema impact
- none

## Determinism risk
- none
- presentation/application refactor only

## Milestones
1. create the builder folder and split `PresentationReadModelBuilder` into partial files
2. keep namespaces and helper access intact
3. run build/test verification and record results

## Tests to add/update
- no new focused tests required if existing `Presentation.Unity` and solution tests stay green

## Rollback / fallback plan
- if the split introduces compile or navigation regressions, collapse the partials back into one file without changing method bodies
