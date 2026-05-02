# Household Mobility Runtime Threshold Event No-Touch Proof V653-V660

## Goal

Add focused threshold event no-touch evidence for the first household mobility runtime rule after V645-V652 per-pool cap proof.

This pass is owner-test and docs evidence only. It proves the existing `MigrationStarted` structured event is emitted only for a selected household that crosses the existing threshold during the owner runtime pass. It does not change runtime behavior, add a new event type, change event routing, widen fanout, add a loader, or change save schema.

## Scope

In scope:
- Add an owner test proving a selected household crossing from 79 to 80 emits the existing `PopulationEventNames.MigrationStarted` event.
- Prove the event is emitted only for the selected crossing household and not for unselected/off-cap households.
- Prove the event carries existing structured metadata for cause, settlement id, and household id.
- Prove the existing `Household mobility pressure` diff remains scoped to the selected crossing household.
- Keep active-pool selection, per-pool household cap application, threshold crossing, event emission, and no-touch behavior inside `PopulationAndHouseholds`.
- Add architecture guard coverage that this pass remains threshold event no-touch proof, not new authority.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change.
- No fanout widening.
- No new event type.
- No event routing change.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, performance cache, event ledger, threshold-event ledger, routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, event threshold crossing, target selection, fanout eligibility, or no-touch status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this threshold event no-touch proof.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, event ledger, threshold-event ledger, routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused owner test records existing deterministic threshold-event behavior:
- the selected household is the only capped household touched by the runtime pass;
- the selected household crosses the existing migration-started threshold;
- the existing structured `MigrationStarted` event is emitted only for that household;
- unselected/off-cap households emit no threshold event and receive no household mobility pressure diff;
- no Application/UI/Unity code calculates threshold crossing, event selection, or no-touch status.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted event state, or plugin loading are introduced by V653-V660.

## Milestones

1. Add V653-V660 threshold event no-touch ExecPlan.
2. Add focused owner threshold crossing event no-touch test.
3. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no new event type or routing path;
   - no loader or file;
   - no runtime plugin marketplace;
   - no fanout widening;
   - no pool, score, candidate ordering, threshold, or cap semantics retune;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority.
4. Update required docs.
5. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleEmitsThresholdEventOnlyForSelectedCrossingHousehold"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_threshold_event_no_touch_v653_v660_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests commit. No save migration, content migration, rules-data rollback, event routing rollback, or production data rollback is required.

## Evidence log

Completed on 2026-05-02:

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleEmitsThresholdEventOnlyForSelectedCrossingHousehold"` passed: 1/1.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_threshold_event_no_touch_v653_v660_must_remain_test_evidence_only_without_runtime_or_schema_drift"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Touched-file replacement-character scan passed for 15 files.
- `dotnet test Zongzu.sln --no-build` passed: 561 tests.
- Ten-year health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

Schema/migration impact remains none. Production source behavior is unchanged; V653-V660 adds owner-test, architecture guard, docs, and ExecPlan evidence only.
