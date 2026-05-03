# Household Mobility Runtime Migration Risk Clamp Extraction V821-V828

## Purpose

V821-V828 extracts the first household mobility runtime rule's post-nudge migration-risk clamp bounds from hardcoded C# into `PopulationHouseholdMobilityRulesData`.

This is an owner-consumed rules-data extraction only. The default migration-risk clamp remains `0..100`.

## Scope

- Owner: `PopulationAndHouseholds`.
- Cadence: unchanged monthly owner pass.
- Target scope: unchanged selected active migration pools and eligible local households under the existing deterministic cap/order.
- Runtime behavior under default rules-data: unchanged.
- Schema/migration impact: none.

The first runtime rule reads validated `MonthlyRuntimeMigrationRiskClampFloor` and `MonthlyRuntimeMigrationRiskClampCeiling` immediately after applying the bounded monthly risk delta. Other module paths continue to use their existing default range behavior and do not become configurable widening surfaces in this pass.

## Out of scope

- No migration-risk retune.
- No risk-delta retune.
- No candidate floor or ceiling retune.
- No migration status threshold retune.
- No migration-started event threshold retune.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, risk-clamp state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted risk-clamp state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces the first runtime rule's hardcoded `Math.Clamp(..., 0, 100)` migration-risk bounds with validated integer bounds consumed by the owner.

Malformed migration-risk clamp data falls back to the default `0..100` range. No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted risk-clamp state, or plugin loading are introduced by V821-V828.

## Milestones

1. Add V821-V828 migration-risk clamp extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with monthly runtime migration-risk clamp floor/ceiling defaults, validation, and fallback getters.
3. Replace the first runtime rule's post-nudge hardcoded clamp bounds with owner-consumed clamp bounds.
4. Keep other existing module paths on their current default range behavior without adding new configurable widening surfaces.
5. Add focused owner tests for default clamp behavior and malformed clamp fallback.
6. Add architecture guard proving owner-consumed rules-data extraction only, default clamp `0..100`, no risk retune, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
7. Update required docs.
8. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultMigrationRiskClampPreservesPreviousClampBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationRiskClampFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_migration_risk_clamp_extraction_v821_v828_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, risk-clamp state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultMigrationRiskClampPreservesPreviousClampBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeMigrationRiskClampFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
  Result: 4 passed, 0 failed.
- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_migration_risk_clamp_extraction_v821_v828_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
  Result: 1 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 125 passed, 0 failed.
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
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 73 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 125 passed.

No save/schema impact was introduced. `PopulationAndHouseholds` remains schema `3`; no persisted state, migration step, rules-data file, loader, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, risk-clamp state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, or migration-started selector state was added.
