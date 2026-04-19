## Goal
- extract office surface and public-life desk hydration logic out of `FirstPassPresentationShell`
- keep the shell composer focused on surface assembly instead of lane-local mapping and wording helpers

## Scope in
- move great-hall and desk public-life summary/hydration logic into a dedicated adapter
- move office surface mapping and office wording helpers into a dedicated adapter
- reconnect desk/governance fallbacks through adapter calls only
- add one focused office shell regression test

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no authoritative office/public-life rules or projection behavior changes
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
1. extract public-life summary/hydration into `PublicLifeShellAdapter`
2. extract office surface and fallback wording into `OfficeShellAdapter`
3. reconnect `FirstPassPresentationShell` through adapter calls only
4. add office-focused shell coverage and run presentation/full verification

## Tests to add/update
- add one focused `Presentation.Unity` test for office surface continuity
- keep existing public-life and governance shell tests green

## Rollback / fallback plan
- if extraction reveals a hidden composer dependency, restore only the minimal helper and move it into the correct adapter in a follow-up slice

## Result
- added [PublicLifeShellAdapter.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/PublicLifeShellAdapter.cs) to own great-hall public-life wording and desk public-life hydration
- added [OfficeShellAdapter.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/OfficeShellAdapter.cs) to own office surface projection and governance fallback wording
- rewired [FirstPassPresentationShell.cs](/E:/zongzu_codex_spec_modular_rebuilt/src/Zongzu.Presentation.Unity/FirstPassPresentationShell.cs) so these lanes now flow through adapters only
- added an office-focused regression in [FirstPassPresentationShellTests.cs](/E:/zongzu_codex_spec_modular_rebuilt/tests/Zongzu.Presentation.Unity.Tests/FirstPassPresentationShellTests.cs)
- verification passed with `Zongzu.Presentation.Unity` build clean and `Zongzu.Presentation.Unity.Tests` at `13/13`
