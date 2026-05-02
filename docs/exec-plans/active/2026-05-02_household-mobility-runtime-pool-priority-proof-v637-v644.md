# Household Mobility Runtime Pool-Priority No-Touch Proof V637-V644

## Goal

Add focused active-pool priority before cross-pool household score no-touch evidence for the first household mobility runtime rule after V629-V636 score-ordering proof.

This pass is owner-test and docs evidence only. It does not change runtime behavior, add a loader, widen fanout, add a second household mobility runtime rule, or change save schema.

## Scope

In scope:
- Add an owner test proving active-pool outflow ordering and settlement cap are applied before household score ordering across pools.
- Prove a higher-scoring household in an unselected lower-priority pool remains no-touch when settlement cap one is consumed by the higher-outflow pool.
- Keep active-pool priority and household scoring authority inside `PopulationAndHouseholds`.
- Add architecture guard coverage that this pass remains pool-priority no-touch proof, not new authority.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change.
- No fanout widening.
- No second household mobility runtime rule.
- No pool ordering retune.
- No score formula retune.
- No candidate ordering retune.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, performance cache, pool-priority ledger, cross-pool score ledger, or active-pool ledger.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, active-pool priority, cross-pool household score comparison, fanout eligibility, or no-touch status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this pool-priority no-touch proof.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, pool-priority ledger, cross-pool score ledger, active-pool ledger, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused owner test records existing deterministic pool-priority behavior:
- the fixture reaches active-pool ordering with settlement id 1 having higher outflow pressure than settlement id 2;
- household id 5 in settlement id 2 has a higher runtime score than every selected-pool household but remains outside the selected pool;
- settlement cap one selects settlement id 1 before any cross-pool household score can reorder targets;
- household id 5 and settlement id 2 remain no-touch and receive no `Household mobility pressure` diff;
- no Application/UI/Unity code calculates active-pool priority, cross-pool household scoring, or no-touch status.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted ordering state, or plugin loading are introduced by V637-V644.

## Milestones

1. Add V637-V644 pool-priority no-touch ExecPlan.
2. Add focused owner active-pool priority before cross-pool score no-touch test.
3. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no loader or file;
   - no runtime plugin marketplace;
   - no fanout widening;
   - no pool, score, candidate ordering, or threshold retune;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority.
4. Update required docs.
5. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRulePoolPriorityPrecedesCrossPoolHouseholdScore"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_pool_priority_no_touch_v637_v644_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests commit. No save migration, content migration, rules-data rollback, or production data rollback is required.

## Evidence log

Completed on 2026-05-02:

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRulePoolPriorityPrecedesCrossPoolHouseholdScore"` passed: 1/1.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_pool_priority_no_touch_v637_v644_must_remain_test_evidence_only_without_runtime_or_schema_drift"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Touched-file replacement-character scan passed for 14 files.
- `dotnet test Zongzu.sln --no-build` passed: 557 tests.
- Ten-year health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

Schema/migration impact remains none. Production source behavior is unchanged; V637-V644 adds owner-test, architecture guard, docs, and ExecPlan evidence only.
