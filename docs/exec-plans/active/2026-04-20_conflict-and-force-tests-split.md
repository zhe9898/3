## Goal
- split the oversized `ConflictAndForceModuleTests` class into concern-based partial files without changing test coverage or assertions
- keep monthly posture, xun cadence, governance support, campaign fatigue, and warfare-fallout scenarios easier to scan

## Scope in
- move `ConflictAndForceModuleTests` into a dedicated folder with partial test files
- split by concern: baseline posture tests, xun cadence tests, support/fatigue tests, and campaign fallout plus shared helpers
- preserve existing test method names, assertions, helper behavior, and stub query behavior
- run targeted and full-solution verification

## Scope out
- no test logic changes
- no assertion changes
- no production-code changes beyond keeping the solution compiling with the split test class

## Affected modules
- `Zongzu.Modules.ConflictAndForce.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- test-only refactor

## Milestones
1. move `ConflictAndForceModuleTests` into a folder and split it into partial test files
2. preserve all current test methods, helper behavior, and stubs
3. run targeted and full solution verification and record results

## Tests to add/update
- no new tests; existing NUnit coverage must remain unchanged

## Rollback / fallback plan
- if the split introduces discovery or compile regressions, collapse the partial test class back into one file without changing method bodies
