# Household Mobility Runtime Touch-Count Proof V565-V572

## Goal

Convert the V557-V564 household mobility widening gate into focused owner-test evidence for the current first runtime rule's default touch budget.

This pass is focused test evidence and docs evidence only. It proves the existing V533-V540 monthly `PopulationAndHouseholds` rule still defaults to one active pool and two household candidates, with quiet/off-scope/lower-priority pool no-touch behavior. It does not change runtime behavior, widen fanout, add a second rule, or add persisted state.

## Scope

In scope:
- Add a focused `PopulationAndHouseholds` owner test proving:
  - current default fanout touches exactly two eligible households in one selected pool in the fixture;
  - touched households are selected by existing deterministic cap/order;
  - the lower-priority pressure-hit household in the selected pool stays untouched because of the household cap;
  - the quiet household stays untouched;
  - the lower-priority active pool stays untouched because of the settlement cap;
  - the proof uses existing state and diff output, not a new counter or diagnostic state.
- Add architecture guard coverage that this pass is focused test evidence, not runtime widening.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change.
- No fanout widening.
- No second household mobility runtime rule.
- No direct route-history.
- No household movement command.
- No relocation command.
- No migration economy.
- No class/status engine.
- No recovery/decay formula change.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, or performance cache.
- No persisted touch-count state.
- No diagnostic state.
- No performance cache.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, touched counts, target eligibility, health classification, or performance status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No runtime plugin marketplace, arbitrary script rules, runtime assemblies, or reflection-heavy rule loading.
- No long-run saturation tuning.
- No performance optimization claim.

## Touched Modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this touch-count proof pass.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused test records the existing deterministic proof path:
- active pools remain ordered by outflow pressure descending, then settlement id;
- household candidates remain scored from existing numeric state, ordered by score descending, then household id;
- default caps remain one settlement/pool and two households;
- the proof compares a default run with a zero-risk-delta baseline, so only the existing monthly runtime rule's touched household deltas are counted.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, or caches are introduced by V565-V572.

## Milestones

1. Add V565-V572 touch-count proof ExecPlan.
2. Add focused owner test evidence for the current default touch budget.
3. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no fanout widening;
   - no second runtime rule;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no persisted touch-count/diagnostic/performance state;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift;
   - no long-run saturation tuning;
   - no performance optimization claim.
4. Update required docs and acceptance evidence.
5. Run focused owner/architecture validation plus full build/test hygiene.

## Tests to add/update

- Add:
  - `RunMonth_FirstMobilityRuntimeRuleDefaultCapsTouchOnlyOnePoolAndTwoHouseholds`
- Focused owner tests:
  - `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleDefaultCapsTouchOnlyOnePoolAndTwoHouseholds|RunMonth_FirstMobilityRuntimeRuleTouchesOnlyCappedEligibleHouseholdsInActivePool|RunMonth_FirstMobilityRuntimeRuleReplaySameSeedStable"`
- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_touch_count_proof_v565_v572_must_stay_test_evidence_only_without_runtime_or_schema_drift"`
- Build:
  - `dotnet build Zongzu.sln --no-restore`
- Diff hygiene:
  - `git diff --check`
- Encoding:
  - touched-file replacement-character scan
- Full no-build solution test:
  - `dotnet test Zongzu.sln --no-build`

## Evidence Log

Local validation:
- Environment prep: ran `dotnet restore Zongzu.sln --configfile $env:APPDATA\NuGet\NuGet.Config` after local assets referenced a missing Visual Studio fallback package folder; the validation commands below still ran with `--no-restore` / `--no-build` as required.
- Passed: `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleDefaultCapsTouchOnlyOnePoolAndTwoHouseholds|RunMonth_FirstMobilityRuntimeRuleTouchesOnlyCappedEligibleHouseholdsInActivePool|RunMonth_FirstMobilityRuntimeRuleReplaySameSeedStable"`.
- Passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_touch_count_proof_v565_v572_must_stay_test_evidence_only_without_runtime_or_schema_drift"`.
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build`.
- Evidence note: the full no-build run produced ten-year diagnostic output with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`; V565-V572 records that as context only and makes no fanout widening, long-run saturation tuning, counter/cache addition, or performance optimization claim.

PR validation:
- Pending: PR CI.

## Rollback / fallback plan

Remove the V565-V572 focused test, docs, and architecture guard. No production code or save/schema rollback is required.

## Open questions

- None for this pass. Any future fanout widening or formula tuning still needs a separate ExecPlan with owner state, proposed touched counts, deterministic cap/order, no-touch proof, schema decision, and validation lane.
