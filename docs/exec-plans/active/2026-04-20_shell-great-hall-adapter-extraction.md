## Goal
- extract `GreatHall` surface mapping out of `FirstPassPresentationShell`
- leave the shell as a pure top-level composer across all major presentation surfaces

## Scope in
- move great-hall projection into a dedicated adapter
- keep existing lead-notice, governance, family, public-life, warfare, and hall-docket wiring unchanged in behavior
- reconnect `FirstPassPresentationShell` through adapter calls only
- add one focused great-hall shell regression

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no authoritative simulation changes
- no new shell fields or UI-object grammar

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only adapter extraction

## Milestones
1. extract great-hall projection into `GreatHallShellAdapter`
2. reconnect `FirstPassPresentationShell` through the adapter only
3. add a focused great-hall regression and run presentation/full verification

## Tests to add/update
- add one focused `Presentation.Unity` test for great-hall continuity
- keep existing lead-notice, governance, warfare, and family hall tests green

## Rollback / fallback plan
- if extraction reveals a hidden surface dependency, restore only the minimal helper and move it into the adapter in a follow-up slice

## Result
- added [GreatHallShellAdapter.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/GreatHallShellAdapter.cs) to own great-hall projection
- rewired [FirstPassPresentationShell.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/FirstPassPresentationShell.cs) so `Compose` now delegates great-hall assembly through the adapter
- added a focused great-hall regression in [FirstPassPresentationShellTests.cs](/E:/zongzu_codex_spec_modular_rebuilt/tests/Zongzu.Presentation.Unity.Tests/FirstPassPresentationShellTests.cs)
- verification passed with `Zongzu.Presentation.Unity` build clean and `Zongzu.Presentation.Unity.Tests` at `15/15`
