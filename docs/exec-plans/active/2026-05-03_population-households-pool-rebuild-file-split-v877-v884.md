# PopulationAndHouseholds Pool Rebuild File Split V877-V884

## Purpose

V877-V884 performs the third behavior-neutral split of the oversized `PopulationAndHouseholdsModule.cs` file.

This is a behavior-neutral file split only. It moves settlement summary rebuild, labor pool rebuild, marriage pool rebuild, migration pool rebuild, and their private helper methods into `PopulationAndHouseholdsModule.PoolRebuild.cs` while preserving the same partial class, owner module, method names, deterministic ordering, pool formulas, membership filtering, and runtime behavior.

Runtime behavior change: none.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files: `PopulationAndHouseholdsModule.cs` and `PopulationAndHouseholdsModule.PoolRebuild.cs`.
- Split unit: `RebuildSettlementSummaries`, `RebuildLaborPools`, `ComputeHouseholdLaborDemand`, `RebuildMarriagePools`, `IsMarriagePoolEligible`, and `RebuildMigrationPools`.
- Target schema/migration impact: none.

The existing call sites remain in `PopulationAndHouseholdsModule.cs`; the moved implementation remains private owner code in the same module assembly.

## Out of scope

- No settlement summary behavior change.
- No labor pool formula change.
- No marriage pool formula change.
- No migration pool formula change.
- No pool limit extraction.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, focus ledger, pool-rebuild ledger, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes or pool rebuild results.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, pool-rebuild split state, ordering ledger, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because V877-V884 only moves private methods into a partial file. Settlement, labor, marriage, and migration pool rebuilds still group by settlement id, order groups by settlement id, order household/member candidates by stable ids where the previous code did, and use the same formulas.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V877-V884 pool-rebuild file-split ExecPlan.
2. Move settlement summary and pool rebuild helpers into a new partial module file.
3. Keep aggregate module-source architecture helpers stable for split files.
4. Add architecture guard proving behavior-neutral split, no schema drift, no second rule, no Application/UI/Unity authority, no `PersonRegistry` expansion, and no loader/plugin drift.
5. Update required docs.
6. Run focused architecture test, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_pool_rebuild_file_split_v877_v884_must_preserve_owner_behavior_and_schema_neutrality"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, pool-rebuild split state rollback, or production data rollback is required.

## Evidence log

- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_pool_rebuild_file_split_v877_v884_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 1 passed, 0 failed.
- Focused split regression guards passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_unmatched_livelihood_score_extraction_v837_v844_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift|Name=Population_households_runtime_rule_file_split_v861_v868_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_membership_focus_file_split_v869_v876_must_preserve_owner_behavior_and_schema_neutrality|Name=Population_households_pool_rebuild_file_split_v877_v884_must_preserve_owner_behavior_and_schema_neutrality"`
  Result: 4 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 132 passed, 0 failed.
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
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 79 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 132 passed.
- File split evidence: `PopulationAndHouseholdsModule.cs` shrank from roughly 77KB after V869-V876 to roughly 69KB, and `PopulationAndHouseholdsModule.PoolRebuild.cs` now owns roughly 8.4KB of settlement summary plus labor/marriage/migration pool rebuild helper implementation.
- Save/schema impact remained none: no persisted field, schema version, migration, save-manifest membership, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pool-rebuild split state, pool ledger, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, serialized projection cache, rules-data file, loader, or content/config namespace was added.
- Authority boundary remained unchanged: the moved methods remain private members of the same partial `PopulationAndHouseholdsModule`; Application/UI/Unity do not calculate household mobility outcomes or pool rebuild results; `PersonRegistry` is unchanged and remains a query seam for marriage eligibility readback; V877-V884 adds no runtime plugin marketplace, arbitrary script rules, reflection-heavy rule loading, prose parsing, movement command, migration economy, class/status engine, second runtime rule, or authored rules-data externalization.
