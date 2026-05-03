# Household Mobility Runtime Pressure Contribution Floor Extraction V845-V852

## Purpose

V845-V852 extracts the first household mobility runtime rule's non-negative pressure contribution floor from inline `Math.Max(0, ...)` behavior into `PopulationHouseholdMobilityRulesData`.

This is an owner-consumed hardcoded contribution-floor extraction only. The default pressure contribution floor remains `0`, so runtime behavior under default rules-data remains unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Cadence: unchanged monthly owner pass.
- Target scope: unchanged active migration pools and eligible local households under the existing deterministic cap/order.
- Runtime behavior under default rules-data: unchanged.
- Schema/migration impact: none.

The first runtime rule reads the validated pressure contribution floor before scoring candidates. Labor, grain, and land pressure contributions still use the existing pressure floors/divisors, but their non-negative score contribution floor is no longer an inline C# literal.

## Out of scope

- No pressure floor retune.
- No divisor retune.
- No score formula retune.
- No livelihood weight retune.
- No trigger-livelihood retune.
- No ordering retune.
- No fanout widening.
- No cap retune.
- No threshold retune.
- No risk-delta retune.
- No clamp retune.
- No migration status threshold retune.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, tie-break ledger, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, pressure-contribution ledger, or migration-started selector state.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted pressure-contribution state, ordering ledger, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction keeps the same default contribution floor `0` and preserves the existing deterministic candidate ordering and caps.

Malformed pressure contribution floor data falls back to the default value. No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted pressure-contribution state, or plugin loading are introduced by V845-V852.

## Milestones

1. Add V845-V852 pressure contribution floor extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with a monthly runtime pressure contribution floor default, validation, and fallback getter.
3. Replace the first runtime rule's inline `Math.Max(0, ...)` contribution floor with owner-consumed rules-data.
4. Keep labor/grain/land pressure floor and divisor behavior equivalent under default rules-data.
5. Add focused owner tests for default contribution-floor behavior and malformed fallback.
6. Add architecture guard proving owner-consumed rules-data extraction only, no formula retune, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
7. Update required docs.
8. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultPressureContributionFloorPreservesPreviousScoreContributions|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimePressureContributionFloorFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_pressure_contribution_floor_extraction_v845_v852_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, pressure-contribution state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultPressureContributionFloorPreservesPreviousScoreContributions|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimePressureContributionFloorFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
  Result: 4 passed, 0 failed.
- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_pressure_contribution_floor_extraction_v845_v852_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
  Result: 1 passed, 0 failed.
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
  Result: 128 passed, 0 failed.
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
  Result: all test projects passed, including `Zongzu.Modules.PopulationAndHouseholds.Tests` 79 passed, `Zongzu.Integration.Tests` 137 passed, and `Zongzu.Architecture.Tests` 128 passed.
- Save/schema impact remained none: no persisted field, schema version, migration, save-manifest membership, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-contribution ledger, ordering ledger, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, serialized projection cache, rules-data file, loader, or content/config namespace was added.
- Authority boundary remained unchanged: `PopulationAndHouseholds` consumes the validated pressure contribution floor; Application/UI/Unity do not calculate household mobility outcomes, `PersonRegistry` is unchanged, and V845-V852 adds no runtime plugin marketplace, arbitrary script rules, reflection-heavy rule loading, prose parsing, movement command, migration economy, or class/status engine.
