# Household Mobility Runtime Event Metadata No-Prose Proof V661-V668

## Goal

Add focused event metadata no-prose evidence for the first household mobility runtime rule after V653-V660 threshold-event proof.

This pass is owner-test and docs evidence only. It proves the existing `MigrationStarted` threshold event exposes machine-readable cause, settlement id, and household id through structured metadata without relying on `Summary` or other prose fields as rule/projection inputs. It does not change runtime behavior, add a new event type, change event routing, widen fanout, add a loader, or change save schema.

## Scope

In scope:
- Add an owner test proving the selected crossing household's `MigrationStarted` event carries structured cause, settlement id, and household id metadata.
- Prove the test can derive the authority tuple from `Metadata` and does not need `Summary` text.
- Prove `Summary` remains prose and does not carry the machine metadata keys or values required for authority.
- Keep active-pool selection, threshold crossing, event emission, structured metadata, and no-touch behavior inside `PopulationAndHouseholds`.
- Add architecture guard coverage that this pass remains event metadata no-prose proof, not new authority.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, performance cache, event ledger, event-metadata ledger, prose-parsing ledger, routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, event metadata meaning, event threshold crossing, target selection, fanout eligibility, or no-touch status.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this event metadata no-prose proof.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, event ledger, event-metadata ledger, prose-parsing ledger, routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused owner test records existing deterministic event metadata behavior:
- the selected household remains the only capped household that crosses the threshold in the fixture;
- the existing structured `MigrationStarted` event carries cause, settlement id, and household id through `Metadata`;
- `Summary` is not used to recover authority ids, cause, selection, cap behavior, or no-touch status;
- no Application/UI/Unity code calculates event metadata meaning, threshold crossing, event selection, or no-touch status.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted event state, or plugin loading are introduced by V661-V668.

## Milestones

1. Add V661-V668 event metadata no-prose ExecPlan.
2. Add focused owner structured-metadata/no-summary-parsing test.
3. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no new event type or routing path;
   - no `DomainEvent.Summary` parsing;
   - no projection prose, receipt text, public-life line, or docs text parser;
   - no loader or file;
   - no runtime plugin marketplace;
   - no fanout widening;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority.
4. Update required docs.
5. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleThresholdEventCarriesMetadataWithoutSummaryParsing"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_event_metadata_no_prose_v661_v668_must_remain_test_evidence_only_without_runtime_or_schema_drift"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests commit. No save migration, content migration, rules-data rollback, event routing rollback, or production data rollback is required.

## Evidence log

Completed on 2026-05-02:

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleThresholdEventCarriesMetadataWithoutSummaryParsing"` passed: 1/1.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_event_metadata_no_prose_v661_v668_must_remain_test_evidence_only_without_runtime_or_schema_drift"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Touched-file replacement-character scan passed for 15 files.
- `dotnet test Zongzu.sln --no-build` passed: 563 tests.
- Ten-year health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

Schema/migration impact remains none. Production source behavior is unchanged; V661-V668 adds owner-test, architecture guard, docs, and ExecPlan evidence only.
