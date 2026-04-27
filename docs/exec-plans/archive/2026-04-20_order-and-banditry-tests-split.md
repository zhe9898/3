## Goal
- split the oversized `OrderAndBanditryModuleTests` class into concern-based partial files without changing test coverage or assertions
- keep cadence, force-support, campaign spillover, and carryover scenarios easier to scan

## Scope in
- move `OrderAndBanditryModuleTests` into a dedicated folder with partial test files
- split by concern: baseline monthly projections, xun cadence, force/governance interaction, campaign spillover/carryover, and shared helpers
- preserve existing test method names, assertions, helper behavior, and stub query behavior
- run targeted and full-solution verification

## Scope out
- no test logic changes
- no assertion changes
- no production-code changes beyond keeping the solution compiling with the split test class

## Affected modules
- `Zongzu.Modules.OrderAndBanditry.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- test-only refactor

## Milestones
1. move `OrderAndBanditryModuleTests` into a folder and split it into partial test files
2. preserve all current test methods, helper behavior, and stubs
3. run targeted and full solution verification and record results

## Tests to add/update
- no new tests; existing NUnit coverage must remain unchanged

## Rollback / fallback plan
- if the split introduces discovery or compile regressions, collapse the partial test class back into one file without changing method bodies
