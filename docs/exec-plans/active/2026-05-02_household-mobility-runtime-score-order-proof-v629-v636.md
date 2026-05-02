# Household Mobility Runtime Score-Ordering No-Touch Proof V629-V636

## Goal

Add focused candidate score-ordering no-touch evidence for the first household mobility runtime rule after V621-V628 active-pool tie-break proof.

This pass is owner-test and docs evidence only. It does not change runtime behavior, add a loader, widen fanout, add a second household mobility runtime rule, or change save schema.

## Scope

In scope:
- Add an owner test proving higher runtime score outranks lower household id when the household cap is one.
- Prove the lower household id remains no-touch when the cap is consumed by the higher-score candidate.
- Keep candidate score ordering authority inside `PopulationAndHouseholds`.
- Add architecture guard coverage that this pass remains score-ordering no-touch proof, not new authority.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change.
- No fanout widening.
- No second household mobility runtime rule.
- No score formula retune.
- No candidate ordering retune.
- No pool ordering retune.
- No threshold retune.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, performance cache, score-order ledger, candidate-rank ledger, or ordering ledger.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, candidate score ordering, fanout eligibility, or no-touch status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this score-ordering no-touch proof.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, score-order ledger, candidate-rank ledger, ordering ledger, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused owner test records existing deterministic score-ordering behavior:
- the fixture reaches the runtime candidate ordering step with household id 2 scoring above household id 1;
- household cap one selects household id 2 through the existing score ordering;
- household id 1 remains no-touch and receives no `Household mobility pressure` diff;
- no Application/UI/Unity code calculates candidate score or ordering.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted ordering state, or plugin loading are introduced by V629-V636.

## Milestones

1. Add V629-V636 score-ordering no-touch ExecPlan.
2. Add focused owner candidate score-ordering no-touch test.
3. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no loader or file;
   - no runtime plugin marketplace;
   - no fanout widening;
   - no score formula, candidate ordering, pool ordering, or threshold retune;
   - no second runtime rule;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no persisted score-order/candidate-rank/touch-count/diagnostic/performance state;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift.
4. Update required docs and acceptance evidence.
5. Run focused owner/architecture validation plus full build/test hygiene.

## Tests to add/update

- Add:
  - `RunMonth_FirstMobilityRuntimeRuleScoreOrderingTouchesHigherScoreBeforeLowerHouseholdId`
- Focused owner test:
  - `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleScoreOrderingTouchesHigherScoreBeforeLowerHouseholdId"`
- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_score_ordering_no_touch_v629_v636_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
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
  `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleScoreOrderingTouchesHigherScoreBeforeLowerHouseholdId"`
- Passed: focused architecture test:
  `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_score_ordering_no_touch_v629_v636_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build` with 555 tests and ten-year health replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

PR validation:
- Pending: PR CI.

## Rollback / fallback plan

Remove the V629-V636 focused test, docs, and architecture guard. No production code or save/schema rollback is required.

## Open questions

- None for this pass. Any future score formula extraction remains a separate owner-consumed rules-data pass.
