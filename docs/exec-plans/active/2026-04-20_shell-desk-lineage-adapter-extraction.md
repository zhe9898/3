## Goal
- extract `Lineage` and `DeskSandbox` surface mapping out of `FirstPassPresentationShell`
- keep the shell composer focused on top-level surface assembly instead of node-by-node shaping

## Scope in
- move lineage tile projection into a dedicated adapter
- move desk sandbox settlement-node projection into a dedicated adapter
- let the desk adapter own public-life hydration by calling the existing public-life adapter
- reconnect `FirstPassPresentationShell` through adapter calls only
- add one focused shell regression for lineage continuity

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new shell fields or UI-object grammar
- no authoritative simulation changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only adapter extraction

## Milestones
1. extract lineage tile projection into `LineageShellAdapter`
2. extract desk settlement-node projection into `DeskSandboxShellAdapter`
3. reconnect `FirstPassPresentationShell` through adapter calls only
4. add focused shell coverage and run presentation/full verification

## Tests to add/update
- add one focused `Presentation.Unity` test for lineage adapter continuity
- keep existing desk/public-life/governance/hall-agenda tests green

## Rollback / fallback plan
- if extraction reveals a hidden surface dependency, restore only the minimal helper and move it into the correct adapter in a follow-up slice

## Result
- added [LineageShellAdapter.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/LineageShellAdapter.cs) to own lineage tile projection
- added [DeskSandboxShellAdapter.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/DeskSandboxShellAdapter.cs) to own settlement-node shaping and desk public-life hydration
- rewired [FirstPassPresentationShell.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/FirstPassPresentationShell.cs) so `Compose` now delegates lineage and desk assembly through adapters only
- added a lineage-focused regression in [FirstPassPresentationShellTests.cs](/E:/zongzu_codex_spec_modular_rebuilt/tests/Zongzu.Presentation.Unity.Tests/FirstPassPresentationShellTests.cs)
- verification passed with `Zongzu.Presentation.Unity` build clean and `Zongzu.Presentation.Unity.Tests` at `14/14`
