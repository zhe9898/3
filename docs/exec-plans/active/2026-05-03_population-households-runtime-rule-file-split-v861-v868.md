# PopulationAndHouseholds Runtime Rule File Split V861-V868

## Purpose

V861-V868 performs the first behavior-neutral split of the oversized `PopulationAndHouseholdsModule.cs` file after the household mobility runtime extraction closeout.

This is a behavior-neutral file split only. It moves the first household mobility runtime rule and its local helper methods into `PopulationAndHouseholdsModule.MobilityRuntime.cs` while preserving the same partial class, owner module, method names, deterministic ordering, rules-data consumption, and runtime behavior.

Runtime behavior change: none.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files: `PopulationAndHouseholdsModule.cs` and `PopulationAndHouseholdsModule.MobilityRuntime.cs`.
- Split unit: first monthly household mobility runtime rule plus its pool tie-break, household tie-break, candidate, score, and livelihood-score helper methods.
- Target schema/migration impact: none.

The existing `RunMonth` call remains in `PopulationAndHouseholdsModule.cs`; the moved implementation remains private owner code in the same module assembly.

## Out of scope

- No runtime rule formula change.
- No rules-data parameter change.
- No rule extraction change.
- No default-value change.
- No validator change.
- No fanout widening.
- No target ordering change.
- No scheduler cadence change.
- No second household mobility runtime rule.
- No rules-data loader.
- No rules-data file.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No reflection-heavy rule loading.
- No direct route-history.
- No household movement command.
- No migration economy.
- No class/status engine.
- No persisted state.
- No schema bump.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, tie-break ledger, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.
- No authored rules-data externalization in this split.

## Affected modules

- `src/Zongzu.Modules.PopulationAndHouseholds`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No Application, presentation, Unity, persistence, or `PersonRegistry` source should change.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, file-split state, ordering ledger, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because V861-V868 only moves private methods into a partial file. The first runtime rule still uses the same owner-consumed rules-data, deterministic active-pool ordering, candidate scoring, tie-break priorities, settlement and household caps, migration-risk delta, clamp, and edge-threshold event emission.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V861-V868 file-split ExecPlan.
2. Move the first household mobility runtime rule and local helpers into a new partial module file.
3. Update architecture tests that previously read only `PopulationAndHouseholdsModule.cs` so they inspect the module source aggregate after the split.
4. Add architecture guard proving behavior-neutral split, no schema drift, no second rule, no Application/UI/Unity authority, no `PersonRegistry` expansion, and no loader/plugin drift.
5. Update required docs.
6. Run focused architecture test, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_runtime_rule_file_split_v861_v868_must_preserve_owner_behavior_and_schema_neutrality"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, file-split state rollback, or production data rollback is required.

## Evidence log

- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_runtime_rule_file_split_v861_v868_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 1 passed, 0 failed.
- Focused regression guard passed after aggregate-source ordering adjustment:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_unmatched_livelihood_score_extraction_v837_v844_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift|Name=Population_households_runtime_rule_file_split_v861_v868_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 2 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 130 passed, 0 failed.
- Build passed:
  `dotnet build Zongzu.sln --no-restore`
  Result: 0 warnings, 0 errors.
- Whitespace check passed:
  `git diff --check`
- Encoding check passed:
  touched-file replacement-character scan
  Result: no replacement characters in touched files.
- Full no-build solution tests passed:
  `dotnet test Zongzu.sln --no-build`
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 79 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 130 passed.
- File split evidence: `PopulationAndHouseholdsModule.cs` shrank from roughly 94KB to roughly 82KB, and `PopulationAndHouseholdsModule.MobilityRuntime.cs` now owns roughly 12.8KB of first household mobility runtime rule implementation.
- Save/schema impact remained none: no persisted field, schema version, migration, save-manifest membership, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, file-split state, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, serialized projection cache, rules-data file, loader, or content/config namespace was added.
- Authority boundary remained unchanged: the moved methods remain private members of the same partial `PopulationAndHouseholdsModule`; Application/UI/Unity do not calculate household mobility outcomes; `PersonRegistry` is unchanged; V861-V868 adds no runtime plugin marketplace, arbitrary script rules, reflection-heavy rule loading, prose parsing, movement command, migration economy, class/status engine, second runtime rule, or authored rules-data externalization.
