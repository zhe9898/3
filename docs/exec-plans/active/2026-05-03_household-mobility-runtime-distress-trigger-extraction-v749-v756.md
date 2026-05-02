# Household Mobility Runtime Distress Trigger Extraction V749-V756

## Goal

Extract the first household mobility runtime rule's distress trigger threshold from a naked C# literal into `PopulationHouseholdMobilityRulesData` as owner-consumed rules-data shape.

This is a small hardcoded extraction pass, not a distress economy retune and not a score-formula expansion. The default distress trigger threshold remains 60, so default candidate eligibility and no-touch behavior remain equivalent to the previous rule.

## Scope

In scope:
- Add `MonthlyRuntimeDistressTriggerThreshold` to `PopulationHouseholdMobilityRulesData`.
- Validate the distress trigger threshold deterministically and fall back to the default on malformed rules-data.
- Use the owner-consumed value only inside `PopulationAndHouseholds.ApplyMonthlyHouseholdMobilityRuntimeRule` and `IsMonthlyHouseholdMobilityRuntimeCandidate`.
- Add focused owner tests proving default distress-trigger no-touch behavior is preserved and malformed threshold data falls back to default.
- Add architecture guard coverage proving this is owner-consumed rules-data extraction without schema or authority drift.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change under default rules-data.
- No distress economy retune.
- No debt trigger extraction.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, distress-trigger state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, distress eligibility, target selection, fanout eligibility, validation fallback, or no-touch status.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted distress-trigger state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces one distress-trigger literal with an owner-consumed validated integer and keeps the default at 60. Malformed rules-data falls back to the previous default candidate trigger threshold.

The focused owner tests prove:
- default rules-data and explicit default distress trigger threshold produce the same monthly runtime signature;
- a household below the default distress trigger remains no-touch when no other trigger qualifies it;
- malformed distress-trigger data falls back to the default threshold;
- malformed runtime rules-data still produces the default monthly run signature.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted distress-trigger state, or plugin loading are introduced by V749-V756.

## Milestones

1. Add V749-V756 distress-trigger extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with the monthly runtime distress trigger threshold, default, validation, and fallback getter.
3. Replace the first runtime rule's distress-trigger literal with the owner-consumed getter.
4. Add focused owner tests for default no-touch equivalence and malformed threshold fallback.
5. Add architecture guard proving owner-consumed rules-data extraction only, default threshold 60, no debt/labor/grain/land/livelihood trigger extraction, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultDistressTriggerThresholdPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDistressTriggerThresholdFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_distress_trigger_extraction_v749_v756_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, distress-trigger state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultDistressTriggerThresholdPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeDistressTriggerThresholdFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- Focused V749 architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_distress_trigger_extraction_v749_v756_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- Full architecture tests passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- Build passed with 0 warnings and 0 errors:
  `dotnet build Zongzu.sln --no-restore`
- Whitespace diff check passed:
  `git diff --check`
- Touched-file replacement-character scan passed across 17 touched files.
- Full no-build test suite passed:
  `dotnet test Zongzu.sln --no-build`
