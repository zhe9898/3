# Household Mobility Runtime Grain Divisor Extraction V725-V732

## Goal

Extract the first household mobility runtime rule's grain-store pressure divisor from a naked C# literal into `PopulationHouseholdMobilityRulesData` as owner-consumed rules-data shape.

This is a small hardcoded extraction pass, not a grain economy retune and not a score-formula expansion. The default grain-store pressure divisor remains 2, so default candidate ordering remains equivalent to the previous rule. The extracted value is consumed only by `PopulationAndHouseholds` when scoring already-eligible households for the existing monthly pressure nudge.

## Scope

In scope:
- Add `MonthlyRuntimeGrainStorePressureDivisor` to `PopulationHouseholdMobilityRulesData`.
- Validate the grain-store pressure divisor deterministically, require it to be at least 1, and fall back to the default on malformed rules-data.
- Use the owner-consumed value only inside `PopulationAndHouseholds.ApplyMonthlyHouseholdMobilityRuntimeRule`.
- Add focused owner tests proving default score ordering is preserved and malformed grain-divisor data falls back to default.
- Add architecture guard coverage proving this is owner-consumed rules-data extraction without schema or authority drift.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change under default rules-data.
- No grain economy retune.
- No grain floor retune.
- No land pressure divisor extraction.
- No score formula retune beyond literal extraction.
- No migration-risk weight retune.
- No labor-floor retune.
- No land-floor retune.
- No livelihood weight extraction.
- No fanout widening.
- No candidate floor retune.
- No high-risk filter retune.
- No event threshold retune.
- No second household mobility runtime rule.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, grain-divisor state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, grain pressure, target selection, fanout eligibility, validation fallback, or no-touch status.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted grain-divisor state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces one divisor literal with an owner-consumed validated integer and keeps the default at 2. Validation rejects 0 so the score path cannot divide by zero through malformed rules-data.

The focused owner tests prove:
- default rules-data and explicit default grain-store pressure divisor produce the same monthly runtime signature;
- the existing score-ordering fixture still selects the same higher-score household under cap one;
- malformed grain-divisor data falls back to the default divisor;
- malformed runtime rules-data still produces the default monthly run signature.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted grain-divisor state, or plugin loading are introduced by V725-V732.

## Milestones

1. Add V725-V732 grain-divisor extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with the monthly runtime grain-store pressure divisor, default, validation, and fallback getter.
3. Replace the first runtime rule's grain-pressure divisor literal with the owner-consumed getter.
4. Add focused owner tests for default equivalence and malformed divisor fallback.
5. Add architecture guard proving owner-consumed rules-data extraction only, default divisor 2, no grain economy retune, no land divisor extraction, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultGrainStorePressureDivisorPreservesPreviousScoreOrdering|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStorePressureDivisorFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_grain_divisor_extraction_v725_v732_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, grain-divisor state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultGrainStorePressureDivisorPreservesPreviousScoreOrdering|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeGrainStorePressureDivisorFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- Focused V725 architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_grain_divisor_extraction_v725_v732_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- Full architecture tests passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- Build passed with 0 warnings and 0 errors:
  `dotnet build Zongzu.sln --no-restore`
- Whitespace diff check passed:
  `git diff --check`
- Touched-file replacement-character scan passed across 17 touched files.
- Full no-build test suite passed:
  `dotnet test Zongzu.sln --no-build`
