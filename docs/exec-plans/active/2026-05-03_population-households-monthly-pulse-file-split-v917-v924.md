# PopulationAndHouseholds Monthly Pulse File Split V917-V924

## Purpose

V917-V924 performs the eighth behavior-neutral split of the oversized `PopulationAndHouseholdsModule.cs` file.

This is a behavior-neutral file split only. It moves private xun/month pulse helpers into `PopulationAndHouseholdsModule.MonthlyPulse.cs` while preserving the same partial class, owner module, xun and monthly call sites, debt/labor/migration delta thresholds, clan support lookup, migration status threshold fallback, deterministic household ordering, and runtime behavior.

Runtime behavior change: none.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files: `PopulationAndHouseholdsModule.cs` and `PopulationAndHouseholdsModule.MonthlyPulse.cs`.
- Split unit: `ApplyXunPulseAdjustments`, `GetClanSupportReserve`, `ComputeDebtDelta`, `ComputeLaborDelta`, `ComputeMigrationDelta`, and `ResolveMigrationStatus` overloads.
- Target schema/migration impact: none.

The moved implementation remains private owner code in the same module assembly. `RunXun`, `RunMonth`, event handlers, and the mobility runtime rule still call the same helper methods.

## Out of scope

- No xun behavior change.
- No monthly behavior change.
- No debt/labor/migration delta change.
- No migration status threshold change.
- No clan support lookup change.
- No receipt/projection text rewrite.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, monthly-pulse ledger, migration-status ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, monthly pulse outcomes, migration status outcomes, or household pressure results.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, monthly-pulse split state, monthly-pulse ledger, migration-status ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because V917-V924 only moves private xun/month pulse helpers into a partial file. `RunXun`, `RunMonth`, event handlers, and the mobility runtime rule still execute the same helper calls over the same ordered household passes.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V917-V924 monthly-pulse file-split ExecPlan.
2. Move private monthly pulse helpers into a new partial module file.
3. Keep aggregate module-source architecture helpers stable for split files.
4. Add architecture guard proving behavior-neutral split, no schema drift, no monthly pulse authority drift, no second rule, no Application/UI/Unity authority, no `PersonRegistry` expansion, and no loader/plugin drift.
5. Update required docs.
6. Run focused architecture test, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_monthly_pulse_file_split_v917_v924_must_preserve_owner_behavior_and_schema_neutrality"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, monthly-pulse split state rollback, or production data rollback is required.

## Evidence log

- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_monthly_pulse_file_split_v917_v924_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 1 passed, 0 failed.
- Focused split regression guards passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_migration_status_threshold_extraction_v813_v820_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift|Name=Population_households_runtime_rule_file_split_v861_v868_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_membership_focus_file_split_v869_v876_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_pool_rebuild_file_split_v877_v884_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_query_surface_file_split_v885_v892_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_pressure_profile_file_split_v893_v900_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_event_dispatch_file_split_v901_v908_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_livelihood_drift_file_split_v909_v916_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_monthly_pulse_file_split_v917_v924_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 9 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 137 passed, 0 failed.
- Build passed after adding the required `Zongzu.Kernel` using for `ClanId`:
  `dotnet build Zongzu.sln --no-restore`
  Result: 0 warnings, 0 errors.
- Whitespace check passed:
  `git diff --check`
- Encoding check passed:
  touched-file replacement-character and known mojibake marker scan
  Result: no replacement characters or known mojibake markers in touched files.
- Full no-build solution tests passed:
  `dotnet test Zongzu.sln --no-build`
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 79 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 137 passed.
- File split evidence: `PopulationAndHouseholdsModule.cs` shrank from roughly 22KB after V909-V916 to roughly 19KB, and `PopulationAndHouseholdsModule.MonthlyPulse.cs` now owns roughly 3.6KB of private xun/month pulse helpers.
- Save/schema impact remained none: no persisted field, schema version, migration, save-manifest membership, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, monthly-pulse split state, monthly-pulse ledger, migration-status ledger, ordering ledger, validation ledger, diagnostic state, performance cache, serialized projection cache, rules-data file, loader, or content/config namespace was added.
- Authority boundary remained unchanged: the moved monthly pulse helpers remain private inside the same partial `PopulationAndHouseholdsModule`; `RunXun`, `RunMonth`, event handlers, and the mobility runtime rule call the same helpers; Application/UI/Unity do not calculate household mobility outcomes, monthly pulse outcomes, migration status outcomes, or household pressure results; `PersonRegistry` is unchanged; V917-V924 adds no runtime plugin marketplace, arbitrary script rules, reflection-heavy rule loading, prose parsing, movement command, migration economy, class/status engine, second runtime rule, or authored rules-data externalization.
