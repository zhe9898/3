# Household Mobility Runtime Pressure Score Weight Extraction V805-V812

## Purpose

V805-V812 extracts the first household mobility runtime rule's implicit distress/debt unit score weights from hardcoded C# addition into `PopulationHouseholdMobilityRulesData`.

This is a literal owner-consumed rules-data extraction only. The default weights remain:
- `Distress = 1`
- `DebtPressure = 1`

## Scope

- Owner: `PopulationAndHouseholds`.
- Cadence: unchanged monthly owner pass.
- Target scope: unchanged active migration pools and eligible local households selected by the existing deterministic cap/order.
- Runtime behavior under default rules-data: unchanged.
- No runtime behavior change under default rules-data.
- Schema/migration impact: none.

## Out of scope

- No pressure formula retune.
- No livelihood engine retune.
- No trigger extraction.
- No migration-risk score weight extraction.
- No livelihood score weight extraction.
- No pressure floor/divisor extraction.
- No score formula retune beyond literal extraction.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, pressure-score state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted pressure-score state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces implicit unit distress/debt score weights with two validated integers consumed only by `PopulationAndHouseholds`.

Malformed pressure score weights fall back to the default unit weights. No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted pressure-score state, or plugin loading are introduced by V805-V812.

## Milestones

1. Add V805-V812 pressure score-weight extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with monthly runtime distress/debt score weights, defaults, validation, and fallback getters.
3. Replace the first runtime rule's implicit `+ Distress` / `+ DebtPressure` score additions with owner-consumed weighted additions.
4. Add focused owner tests for default score ordering and malformed pressure-weight fallback.
5. Add architecture guard proving owner-consumed rules-data extraction only, default weights `Distress=1` and `DebtPressure=1`, no score retune, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultPressureScoreWeightsPreservePreviousScoreOrdering|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimePressureScoreWeightsFallBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_pressure_score_weight_extraction_v805_v812_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, pressure-score state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultPressureScoreWeightsPreservePreviousScoreOrdering|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimePressureScoreWeightsFallBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"` (4 tests).
- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_pressure_score_weight_extraction_v805_v812_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"` (1 test).
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore` (123 tests).
- Build passed:
  `dotnet build Zongzu.sln --no-restore`.
- Whitespace diff check passed:
  `git diff --check`.
- Replacement-character scan passed across 17 touched files.
- Full no-build suite passed:
  `dotnet test Zongzu.sln --no-build`.
