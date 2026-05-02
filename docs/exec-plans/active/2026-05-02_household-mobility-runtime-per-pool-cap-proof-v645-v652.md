# Household Mobility Runtime Per-Pool Cap No-Touch Proof V645-V652

## Goal

Add focused per-selected-pool household cap evidence for the first household mobility runtime rule after V637-V644 pool-priority proof.

This pass is owner-test and docs evidence only. It does not change runtime behavior, add a loader, widen fanout, add a second household mobility runtime rule, or change save schema.

## Scope

In scope:
- Add an owner test proving `MonthlyRuntimeHouseholdCap` applies inside each selected active pool, not as a global cross-pool cap.
- Prove settlement cap two and household cap one touch one deterministic candidate in each selected active pool.
- Prove lower-score households inside each selected pool remain no-touch under the per-pool cap.
- Keep active-pool selection, per-pool household cap application, and household scoring authority inside `PopulationAndHouseholds`.
- Add architecture guard coverage that this pass remains per-pool cap no-touch proof, not new authority.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change.
- No fanout widening.
- No second household mobility runtime rule.
- No pool ordering retune.
- No score formula retune.
- No candidate ordering retune.
- No threshold retune.
- No cap semantics retune.
- No global household cap.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, performance cache, per-pool cap ledger, global household cap ledger, or active-pool ledger.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, active-pool priority, per-pool cap application, global cap semantics, fanout eligibility, or no-touch status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this per-pool cap no-touch proof.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, per-pool cap ledger, global household cap ledger, active-pool ledger, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused owner test records existing deterministic per-pool cap behavior:
- both fixture pools are active under the default threshold;
- settlement cap two selects both pools;
- household cap one touches one deterministic candidate in each selected pool;
- lower-score households inside each selected pool remain no-touch;
- no Application/UI/Unity code calculates cap scope, cross-pool target order, or no-touch status.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted cap state, or plugin loading are introduced by V645-V652.

## Milestones

1. Add V645-V652 per-pool cap no-touch ExecPlan.
2. Add focused owner per-selected-pool household-cap no-touch test.
3. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no loader or file;
   - no runtime plugin marketplace;
   - no fanout widening;
   - no pool, score, candidate ordering, threshold, or cap semantics retune;
   - no global household cap;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority.
4. Update required docs.
5. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleHouseholdCapAppliesPerSelectedPool"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_per_pool_cap_no_touch_v645_v652_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests commit. No save migration, content migration, rules-data rollback, or production data rollback is required.

## Evidence log

Completed on 2026-05-02:

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleHouseholdCapAppliesPerSelectedPool"` passed: 1/1.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_per_pool_cap_no_touch_v645_v652_must_remain_test_evidence_only_without_runtime_or_schema_drift"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Touched-file replacement-character scan passed for 14 files.
- `dotnet test Zongzu.sln --no-build` passed: 559 tests.
- Ten-year health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

Schema/migration impact remains none. Production source behavior is unchanged; V645-V652 adds owner-test, architecture guard, docs, and ExecPlan evidence only.
