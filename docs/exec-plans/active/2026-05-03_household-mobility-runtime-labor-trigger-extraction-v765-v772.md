# Household Mobility Runtime Labor Trigger Extraction V765-V772

## Goal

Extract the first household mobility runtime rule's labor-capacity trigger ceiling from a naked C# literal into `PopulationHouseholdMobilityRulesData` as owner-consumed rules-data shape.

This is a small hardcoded extraction pass, not a labor model retune and not a score-formula expansion. The default labor-capacity trigger ceiling remains 45, so default candidate eligibility and no-touch behavior remain equivalent to the previous rule.

## Scope

In scope:
- Add `MonthlyRuntimeLaborCapacityTriggerCeiling` to `PopulationHouseholdMobilityRulesData`.
- Validate the labor-capacity trigger ceiling deterministically and fall back to the default on malformed rules-data.
- Use the owner-consumed value only inside `PopulationAndHouseholds.ApplyMonthlyHouseholdMobilityRuntimeRule` and `IsMonthlyHouseholdMobilityRuntimeCandidate`.
- Add focused owner tests proving default labor-trigger no-touch behavior is preserved and malformed threshold data falls back to default.
- Add architecture guard coverage proving this is owner-consumed rules-data extraction without schema or authority drift.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change under default rules-data.
- No labor model retune.
- No debt trigger retune.
- No distress trigger retune.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, labor-trigger state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, labor eligibility, target selection, fanout eligibility, validation fallback, or no-touch status.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted labor-trigger state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces one labor-capacity trigger literal with an owner-consumed validated integer and keeps the default at 45. Malformed rules-data falls back to the previous default candidate trigger ceiling.

The focused owner tests prove:
- default rules-data and explicit default labor-capacity trigger ceiling produce the same monthly runtime signature;
- a household at the default labor trigger ceiling remains no-touch when no other trigger qualifies it;
- malformed labor-trigger data falls back to the default ceiling;
- malformed runtime rules-data still produces the default monthly run signature.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted labor-trigger state, or plugin loading are introduced by V765-V772.

## Milestones

1. Add V765-V772 labor-trigger extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with the monthly runtime labor-capacity trigger ceiling, default, validation, and fallback getter.
3. Replace the first runtime rule's labor-trigger literal with the owner-consumed getter.
4. Add focused owner tests for default no-touch equivalence and malformed threshold fallback.
5. Add architecture guard proving owner-consumed rules-data extraction only, default ceiling 45, no debt/distress/grain/land/livelihood trigger retune/extraction, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultLaborCapacityTriggerCeilingPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLaborCapacityTriggerCeilingFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_labor_trigger_extraction_v765_v772_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, labor-trigger state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultLaborCapacityTriggerCeilingPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLaborCapacityTriggerCeilingFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"` (4 tests).
- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_labor_trigger_extraction_v765_v772_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"` (1 test).
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore` (118 tests).
- Build passed:
  `dotnet build Zongzu.sln --no-restore`.
- Whitespace diff check passed:
  `git diff --check`.
- Replacement-character scan passed across 17 touched files.
- Full no-build suite passed:
  `dotnet test Zongzu.sln --no-build`.
