# Household Mobility Runtime Migration Status Threshold Extraction V813-V820

## Purpose

V813-V820 extracts the migration status threshold used by the first household mobility runtime rule from hardcoded C# into `PopulationHouseholdMobilityRulesData`.

This is a literal owner-consumed rules-data extraction only. The default status threshold remains `80`.

## Scope

- Owner: `PopulationAndHouseholds`.
- Cadence: unchanged monthly owner pass.
- Target scope: unchanged active migration pools and eligible local households selected by the existing deterministic cap/order.
- Runtime behavior under default rules-data: unchanged.
- No runtime behavior change under default rules-data.
- Schema/migration impact: none.

The first runtime rule passes the owner-consumed threshold into `ResolveMigrationStatus` after the bounded monthly risk nudge. Other existing module paths continue to use the default threshold and do not become configurable widening surfaces in this pass.

## Out of scope

- No migration status retune.
- No migration-started event threshold retune.
- No candidate ceiling retune.
- No trigger extraction.
- No score formula retune.
- No fanout widening.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, migration-status state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `src/Zongzu.Modules.PopulationAndHouseholds`
- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No Application, presentation, Unity, persistence, or `PersonRegistry` source should change.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted migration-status state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces the hardcoded `MigrationRisk >= 80` migration-status check with a validated integer threshold consumed by the first runtime rule and a default-backed helper for existing paths.

Malformed migration status threshold data falls back to the default threshold `80`. No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted migration-status state, or plugin loading are introduced by V813-V820.

## Milestones

1. Add V813-V820 migration status threshold extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with monthly runtime migration status threshold, default, validation, and fallback getter.
3. Replace the first runtime rule's post-nudge `ResolveMigrationStatus` call with the owner-consumed threshold.
4. Keep other existing module paths on the default threshold without adding new configurable widening surfaces.
5. Add focused owner tests for default status behavior and malformed threshold fallback.
6. Add architecture guard proving owner-consumed rules-data extraction only, default threshold `80`, no status retune, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
7. Update required docs.
8. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultMigrationStatusThresholdPreservesPreviousStatusBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationStatusThresholdFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_migration_status_threshold_extraction_v813_v820_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, migration-status state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultMigrationStatusThresholdPreservesPreviousStatusBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationStatusThresholdFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
  Result: 4 passed, 0 failed.
- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_migration_status_threshold_extraction_v813_v820_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
  Result: 1 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 124 passed, 0 failed.
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
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 71 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 124 passed.

No save/schema impact was introduced. `PopulationAndHouseholds` remains schema `3`; no persisted state, migration step, rules-data file, loader, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, migration-status state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, or migration-started selector state was added.
