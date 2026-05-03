# PopulationAndHouseholds Warfare Aftermath File Split V925-V932

## Purpose

V925-V932 performs the ninth behavior-neutral split of the oversized `PopulationAndHouseholdsModule.cs` file.

This is a behavior-neutral file split only. It moves private warfare-campaign aftermath event handling into `PopulationAndHouseholdsModule.WarfareAftermath.cs` while preserving the same partial class, owner module, `HandleEvents` call order, campaign bundle handling, household ordering, aftermath deltas, emitted receipt text, metadata, and runtime behavior.

Runtime behavior change: none.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files: `PopulationAndHouseholdsModule.cs` and `PopulationAndHouseholdsModule.WarfareAftermath.cs`.
- Split unit: `ApplyWarfareCampaignAftermathEvents` plus campaign distress, debt, migration, and labor delta helpers.
- Target schema/migration impact: none.

The moved implementation remains private owner code in the same module assembly. `HandleEvents` still calls trade, world, family, office-supply, and warfare aftermath handling in the same order.

## Out of scope

- No warfare aftermath behavior change.
- No campaign delta formula change.
- No event behavior change.
- No emitted metadata change.
- No receipt/projection text rewrite.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, warfare-aftermath ledger, campaign-aftermath ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, warfare aftermath outcomes, campaign delta outcomes, or household pressure results.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, warfare-aftermath split state, warfare-aftermath ledger, campaign-aftermath ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because V925-V932 only moves private warfare aftermath handling into a partial file. `HandleEvents` still calls the same owner logic after the same dispatch calls, and household ordering remains `OrderBy(...Id.Value)`.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V925-V932 warfare-aftermath file-split ExecPlan.
2. Move private warfare aftermath handling into a new partial module file.
3. Keep aggregate module-source architecture helpers stable for split files.
4. Add architecture guard proving behavior-neutral split, no schema drift, no warfare aftermath authority drift, no second rule, no Application/UI/Unity authority, no `PersonRegistry` expansion, and no loader/plugin drift.
5. Update required docs.
6. Run focused architecture test, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_warfare_aftermath_file_split_v925_v932_must_preserve_owner_behavior_and_schema_neutrality"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, warfare-aftermath split state rollback, or production data rollback is required.

## Evidence log

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_warfare_aftermath_file_split_v925_v932_must_preserve_owner_behavior_and_schema_neutrality"`: passed, 1/1.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_migration_status_threshold_extraction_v813_v820_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift|Name=Population_households_runtime_rule_file_split_v861_v868_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_membership_focus_file_split_v869_v876_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_pool_rebuild_file_split_v877_v884_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_query_surface_file_split_v885_v892_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_pressure_profile_file_split_v893_v900_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_event_dispatch_file_split_v901_v908_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_livelihood_drift_file_split_v909_v916_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_monthly_pulse_file_split_v917_v924_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_warfare_aftermath_file_split_v925_v932_must_preserve_owner_behavior_and_schema_neutrality"`: passed, 10/10.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`: passed, 138/138. The first 240s attempt timed out without a failure result; the successful rerun used a longer timeout and completed in about five minutes.
- `dotnet build Zongzu.sln --no-restore`: passed, 0 warnings, 0 errors.
- `git diff --check`: passed.
- Touched-file replacement-character and known mojibake marker scan: passed.
- `dotnet test Zongzu.sln --no-build`: passed. Key touched lanes included `Zongzu.Modules.PopulationAndHouseholds.Tests` 79/79, `Zongzu.Integration.Tests` 137/137, and `Zongzu.Architecture.Tests` 138/138.
