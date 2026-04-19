## Goal
- align the authoritative runtime with the documented `month review shell + three xun pulses` cadence
- make module cadence declaration a first-class contract instead of an implied doc-only rule
- land the scheduler foundation without forcing every module to implement rich xun behavior in one pass

## Scope in
- add cadence-band contracts for authoritative modules
- extend `MonthlyScheduler` to run three deterministic `xun` pulses before month-end consolidation
- expose current cadence / xun position in `ModuleExecutionContext`
- make active module runners declare their cadence bands according to `MODULE_CADENCE_MATRIX.md`
- add scheduler coverage proving xun ordering and month-only isolation
- update architecture / acceptance docs so code and docs no longer disagree on runtime cadence

## Scope out
- no full xun-specific rule rewrite for every module in this pass
- no save-schema changes
- no UI or presentation authority changes
- no seasonal execution loop beyond declaration support

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Scheduler`
- all active authoritative module runners
- `tests/Zongzu.Scheduler.Tests`
- architecture / acceptance / implementation docs

## Save/schema impact
- no root schema bump
- no module schema bump
- cadence declarations remain runtime contracts and do not enter save payloads

## Determinism risk
- medium
- scheduler now runs more internal steps per month, so ordering must stay explicit and stable
- xun passes must rebuild queries deterministically from current owned state
- month-end event handling and projection ordering must remain unchanged relative to authority completion

## Milestones
1. add cadence-band and xun-position contracts in `Zongzu.Contracts`
2. extend `MonthlyScheduler` to run three xun pulses before the month-end authority pass
3. update module runners to declare cadence bands from the documented matrix
4. add scheduler tests for xun execution order and month-only isolation
5. update docs and verify build/test

## Tests to add/update
- `Zongzu.Scheduler.Tests`
- full solution build/test verification

## Rollback / fallback plan
- if xun execution destabilizes module behavior, keep cadence declarations plus context fields and gate scheduler xun execution behind a conservative no-op path
- if broad module declaration churn becomes noisy, keep the scheduler and contract changes first and narrow follow-up fixes to the modules that still default incorrectly

## Open questions
- when to start moving specific module logic from month-only into real xun-owned behavior
- whether seasonal cadence should later get its own scheduler lane or stay declared-only until a concrete seasonal slice lands

## Completion notes
- `Zongzu.Contracts` now exposes explicit cadence declarations through `SimulationCadenceBand`, `SimulationXun`, and `IModuleRunner.CadenceBands`, while `ModuleExecutionContext` now carries the active cadence/xun position.
- `MonthlyScheduler` now opens each month with three deterministic `xun` passes before month-end consolidation, then rebuilds queries for month-end authority, event handling, and projection in stable order.
- active authoritative modules now declare cadence bands directly in code so the scheduler, tests, and future slices can inspect them instead of relying on doc-only timing assumptions.
- the first scheduler coverage now proves both `xun` ordering and month-only isolation without changing save schema or module ownership.

## Verification
- `dotnet test .\\tests\\Zongzu.Scheduler.Tests\\Zongzu.Scheduler.Tests.csproj -c Debug`
- `dotnet build .\\Zongzu.sln -c Debug -m:1`
- `dotnet test .\\Zongzu.sln -c Debug -m:1 --no-build`
