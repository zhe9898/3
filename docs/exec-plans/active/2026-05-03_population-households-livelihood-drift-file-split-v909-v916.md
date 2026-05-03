# PopulationAndHouseholds Livelihood Drift File Split V909-V916

## Purpose

V909-V916 performs the seventh behavior-neutral split of the oversized `PopulationAndHouseholdsModule.cs` file.

This is a behavior-neutral file split only. It moves private monthly livelihood drift helpers into `PopulationAndHouseholdsModule.LivelihoodDrift.cs` while preserving the same partial class, owner module, monthly call sites, livelihood drift thresholds, baseline distress mapping, drift reason text, emitted receipt text at call sites, deterministic household ordering, and runtime behavior.

Runtime behavior change: none.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files: `PopulationAndHouseholdsModule.cs` and `PopulationAndHouseholdsModule.LivelihoodDrift.cs`.
- Split unit: `LivelihoodDriftResult`, `TryApplyMonthlyLivelihoodDrift`, monthly livelihood resolution, livelihood drift reason rendering, collapse classification, livelihood diff rendering, and livelihood distress baseline mapping.
- Target schema/migration impact: none.

The moved implementation remains private owner code in the same module assembly. `RunMonth` still calls the same helper methods in the same order.

## Out of scope

- No monthly behavior change.
- No livelihood threshold change.
- No baseline distress mapping change.
- No drift reason or receipt/projection text rewrite.
- No event behavior change.
- No emitted metadata change.
- No pressure formula change.
- No metadata fallback change.
- No query behavior change.
- No snapshot field change.
- No ordering change.
- No rules-data parameter change.
- No rule extraction change.
- No default-value change.
- No validator change.
- No fanout widening.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, focus ledger, pool-rebuild ledger, query-surface ledger, pressure-profile ledger, event-dispatch ledger, livelihood-drift ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, livelihood drift outcomes, or household pressure results.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, livelihood-drift split state, livelihood-drift ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because V909-V916 only moves private monthly livelihood drift helpers into a partial file. `RunMonth` still executes the same ordered household pass, the same drift helper calls, and the same event emission call sites.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V909-V916 livelihood-drift file-split ExecPlan.
2. Move private livelihood drift helpers into a new partial module file.
3. Keep aggregate module-source architecture helpers stable for split files.
4. Add architecture guard proving behavior-neutral split, no schema drift, no livelihood authority drift, no second rule, no Application/UI/Unity authority, no `PersonRegistry` expansion, and no loader/plugin drift.
5. Update required docs.
6. Run focused architecture test, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_livelihood_drift_file_split_v909_v916_must_preserve_owner_behavior_and_schema_neutrality"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, livelihood-drift split state rollback, or production data rollback is required.

## Evidence log

- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_livelihood_drift_file_split_v909_v916_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 1 passed, 0 failed.
- Focused split regression guards passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_unmatched_livelihood_score_extraction_v837_v844_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift|Name=Population_households_runtime_rule_file_split_v861_v868_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_membership_focus_file_split_v869_v876_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_pool_rebuild_file_split_v877_v884_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_query_surface_file_split_v885_v892_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_pressure_profile_file_split_v893_v900_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_event_dispatch_file_split_v901_v908_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_livelihood_drift_file_split_v909_v916_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 8 passed, 0 failed.
- Migration-status guard replay passed after aggregate source ordering was updated for the new split:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_migration_status_threshold_extraction_v813_v820_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift|Name=Population_households_livelihood_drift_file_split_v909_v916_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 2 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 136 passed, 0 failed.
- Build passed:
  `dotnet build Zongzu.sln --no-restore`
  Result: 0 warnings, 0 errors.
- Whitespace check passed:
  `git diff --check`
- Encoding check passed:
  touched-file replacement-character and known mojibake marker scan
  Result: no replacement characters or known mojibake markers in touched files.
- Full no-build solution tests passed:
  `dotnet test Zongzu.sln --no-build`
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 79 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 136 passed.
- File split evidence: `PopulationAndHouseholdsModule.cs` shrank from roughly 27KB after V901-V908 to roughly 22KB, and `PopulationAndHouseholdsModule.LivelihoodDrift.cs` now owns roughly 5KB of private monthly livelihood drift helpers.
- Save/schema impact remained none: no persisted field, schema version, migration, save-manifest membership, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, livelihood-drift split state, livelihood-drift ledger, ordering ledger, validation ledger, diagnostic state, performance cache, serialized projection cache, rules-data file, loader, or content/config namespace was added.
- Authority boundary remained unchanged: the moved livelihood drift helpers remain private inside the same partial `PopulationAndHouseholdsModule`; `RunMonth` calls the same helpers in the same ordered household pass; Application/UI/Unity do not calculate household mobility outcomes, livelihood drift outcomes, or household pressure results; `PersonRegistry` is unchanged; V909-V916 adds no runtime plugin marketplace, arbitrary script rules, reflection-heavy rule loading, prose parsing, movement command, migration economy, class/status engine, second runtime rule, or authored rules-data externalization.
