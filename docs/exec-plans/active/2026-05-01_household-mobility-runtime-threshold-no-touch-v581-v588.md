# Household Mobility Runtime Threshold No-Touch Proof V581-V588

## Goal

Add focused threshold no-touch evidence for the first household mobility runtime rule after V573-V580 fallback matrix.

This pass is owner-test and docs evidence only. It does not change runtime behavior, add a loader, widen fanout, add a second household mobility runtime rule, or change save schema.

## Scope

In scope:
- Add an owner test proving `MonthlyRuntimeActivePoolOutflowThreshold = 100` blocks the first runtime rule when every migration pool remains below that threshold.
- Prove the threshold-blocked monthly result signature equals a `MonthlyRuntimeRiskDelta = 0` baseline for the same fixture.
- Prove no `Household mobility pressure` diff entry is emitted when the active-pool threshold blocks target selection.
- Add architecture guard coverage that this pass remains threshold no-touch proof, not new authority.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change.
- No fanout widening.
- No second household mobility runtime rule.
- No rules-data loader.
- No rules-data file.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No runtime assemblies.
- No reflection-heavy rule loading.
- No direct route-history.
- No household movement command.
- No relocation command.
- No migration economy.
- No class/status engine.
- No recovery/decay formula change.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, or performance cache.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, active-pool eligibility, threshold blocking, target ordering, or no-touch status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this threshold no-touch proof.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused owner test records the existing deterministic threshold behavior:
- pool outflow values in the fixture remain below the maximum valid threshold;
- the first runtime rule selects no active pools at that threshold;
- household/pool result signature equals the zero-risk-delta baseline;
- no household mobility pressure diff is emitted.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, or plugin loading are introduced by V581-V588.

## Milestones

1. Add V581-V588 threshold no-touch ExecPlan.
2. Add focused owner threshold no-touch test.
3. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no loader or file;
   - no runtime plugin marketplace;
   - no fanout widening;
   - no second runtime rule;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no persisted touch-count/diagnostic/performance state;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift.
4. Update required docs and acceptance evidence.
5. Run focused owner/architecture validation plus full build/test hygiene.

## Tests to add/update

- Add:
  - `RunMonth_FirstMobilityRuntimeRuleActivePoolThresholdNoTouchesHouseholdsOrPools`
- Focused owner test:
  - `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleActivePoolThresholdNoTouchesHouseholdsOrPools"`
- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_threshold_no_touch_v581_v588_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
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
- Passed: focused owner test:
  `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleActivePoolThresholdNoTouchesHouseholdsOrPools"`
- Passed: focused architecture test:
  `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_threshold_no_touch_v581_v588_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build` with 543 tests and ten-year health replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

PR validation:
- Pending: PR CI.

## Rollback / fallback plan

Remove the V581-V588 focused test, docs, and architecture guard. No production code or save/schema rollback is required.

## Open questions

- None for this pass. Any future threshold tuning remains a separate owner-consumed rules-data pass.
