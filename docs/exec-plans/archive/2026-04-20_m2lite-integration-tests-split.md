## Goal
- split the oversized `M2LiteIntegrationTests` class into concern-based partial files without changing integration coverage or assertions
- keep bootstrap/determinism, local-conflict activation, campaign/player-command flows, and governance/local-conflict office-reach scenarios easier to scan

## Scope in
- move `M2LiteIntegrationTests` into a dedicated folder with partial test files
- split by concern: bootstrap and diagnostics, local-conflict/governance bootstraps, campaign and player-command flows, post-MVP seams and governance-local-conflict follow-through, and shared helpers
- preserve existing test method names, assertions, helper behavior, and save reconfiguration logic
- run targeted and full-solution verification

## Scope out
- no test logic changes
- no assertion changes
- no production-code changes beyond keeping the solution compiling with the split test class

## Affected modules
- `Zongzu.Integration.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- test-only refactor

## Milestones
1. move `M2LiteIntegrationTests` into a folder and split it into partial test files
2. preserve all current test methods, helper behavior, and save reconfiguration helpers
3. run targeted and full solution verification and record results

## Tests to add/update
- no new tests; existing NUnit integration coverage must remain unchanged

## Rollback / fallback plan
- if the split introduces discovery or compile regressions, collapse the partial test class back into one file without changing method bodies
