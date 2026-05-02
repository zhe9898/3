# Household Mobility Runtime Debt Trigger Extraction V757-V764

## Goal

Extract the first household mobility runtime rule's debt-pressure trigger threshold from a naked C# literal into `PopulationHouseholdMobilityRulesData` as owner-consumed rules-data shape.

This is a small hardcoded extraction pass, not a debt economy retune and not a score-formula expansion. The default debt-pressure trigger threshold remains 60, so default candidate eligibility and no-touch behavior remain equivalent to the previous rule.

## Scope

In scope:
- Add `MonthlyRuntimeDebtPressureTriggerThreshold` to `PopulationHouseholdMobilityRulesData`.
- Validate the debt-pressure trigger threshold deterministically and fall back to the default on malformed rules-data.
- Use the owner-consumed value only inside `PopulationAndHouseholds.ApplyMonthlyHouseholdMobilityRuntimeRule` and `IsMonthlyHouseholdMobilityRuntimeCandidate`.
- Add focused owner tests proving default debt-trigger no-touch behavior is preserved and malformed threshold data falls back to default.
- Add architecture guard coverage proving this is owner-consumed rules-data extraction without schema or authority drift.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change under default rules-data.
- No debt economy retune.
- No distress trigger retune.
- No labor trigger extraction.
- No grain trigger extraction.
- No land trigger extraction.
- No livelihood trigger extraction.
- No migration-started event threshold retune.
- No candidate floor retune.
- No candidate ceiling retune.
- No active-pool threshold retune.
- No migration-risk score weight retune.
- No labor-floor retune.
- No grain-floor retune.
- No land-floor retune.
- No grain pressure divisor extraction.
- No land pressure divisor extraction.
- No score formula retune beyond literal extraction.
- No fanout widening.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, debt-trigger state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, debt eligibility, target selection, fanout eligibility, validation fallback, or no-touch status.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted debt-trigger state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces one debt-pressure trigger literal with an owner-consumed validated integer and keeps the default at 60. Malformed rules-data falls back to the previous default candidate trigger threshold.

The focused owner tests prove:
- default rules-data and explicit default debt-pressure trigger threshold produce the same monthly runtime signature;
- a household below the default debt trigger remains no-touch when no other trigger qualifies it;
- malformed debt-trigger data falls back to the default threshold;
- malformed runtime rules-data still produces the default monthly run signature.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted debt-trigger state, or plugin loading are introduced by V757-V764.

## Milestones

1. Add V757-V764 debt-trigger extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with the monthly runtime debt-pressure trigger threshold, default, validation, and fallback getter.
3. Replace the first runtime rule's debt-trigger literal with the owner-consumed getter.
4. Add focused owner tests for default no-touch equivalence and malformed threshold fallback.
5. Add architecture guard proving owner-consumed rules-data extraction only, default threshold 60, no distress/labor/grain/land/livelihood trigger retune/extraction, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultDebtPressureTriggerThresholdPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDebtPressureTriggerThresholdFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_debt_trigger_extraction_v757_v764_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, debt-trigger state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultDebtPressureTriggerThresholdPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDebtPressureTriggerThresholdFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"` (4 tests).
- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_debt_trigger_extraction_v757_v764_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"` (1 test).
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore` (117 tests).
- Build passed:
  `dotnet build Zongzu.sln --no-restore`.
- Whitespace diff check passed:
  `git diff --check`.
- Replacement-character scan passed across 17 touched files.
- Full no-build suite passed:
  `dotnet test Zongzu.sln --no-build`.
