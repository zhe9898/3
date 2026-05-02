# Household Mobility Runtime Candidate Ceiling Extraction V741-V748

## Goal

Extract the first household mobility runtime rule's candidate high-risk ceiling from a naked C# literal into `PopulationHouseholdMobilityRulesData` as owner-consumed rules-data shape.

This is a small hardcoded extraction pass, not a migration-started event threshold retune and not a candidate-floor retune. The default candidate migration-risk ceiling remains 80, so households at or above 80 remain no-touch candidates for the existing monthly pressure nudge under default rules-data.

## Scope

In scope:
- Add `MonthlyRuntimeCandidateMigrationRiskCeiling` to `PopulationHouseholdMobilityRulesData`.
- Validate the candidate migration-risk ceiling deterministically and fall back to the default on malformed rules-data.
- Use the owner-consumed value only inside `PopulationAndHouseholds.ApplyMonthlyHouseholdMobilityRuntimeRule` and `IsMonthlyHouseholdMobilityRuntimeCandidate`.
- Add focused owner tests proving default ceiling no-touch behavior is preserved and malformed ceiling data falls back to default.
- Add architecture guard coverage proving this is owner-consumed rules-data extraction without schema or authority drift.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change under default rules-data.
- No migration-started event threshold retune.
- No candidate floor retune.
- No active-pool threshold retune.
- No migration-risk score weight retune.
- No labor-floor retune.
- No grain-floor retune.
- No grain pressure divisor extraction.
- No land-floor retune.
- No land pressure divisor extraction.
- No trigger threshold extraction.
- No livelihood weight extraction.
- No score formula retune beyond literal extraction.
- No fanout widening.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, candidate-ceiling state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, high-risk eligibility, target selection, fanout eligibility, validation fallback, or no-touch status.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted candidate-ceiling state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces one candidate-ceiling literal with an owner-consumed validated integer and keeps the default at 80. Validation rejects malformed ceilings so invalid rules-data falls back to the previous default candidate window.

The focused owner tests prove:
- default rules-data and explicit default candidate migration-risk ceiling produce the same monthly runtime signature;
- a household already at the default ceiling remains no-touch under default rules-data;
- malformed candidate-ceiling data falls back to the default ceiling;
- malformed runtime rules-data still produces the default monthly run signature.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted candidate-ceiling state, or plugin loading are introduced by V741-V748.

## Milestones

1. Add V741-V748 candidate-ceiling extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with the monthly runtime candidate migration-risk ceiling, default, validation, and fallback getter.
3. Replace the first runtime rule's high-risk candidate exclusion literal with the owner-consumed getter.
4. Add focused owner tests for default no-touch equivalence and malformed ceiling fallback.
5. Add architecture guard proving owner-consumed rules-data extraction only, default ceiling 80, no migration-started threshold retune, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskCeilingPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskCeilingFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_candidate_ceiling_extraction_v741_v748_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, candidate-ceiling state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskCeilingPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskCeilingFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- Focused V741 architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_candidate_ceiling_extraction_v741_v748_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- Full architecture tests passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- Build passed with 0 warnings and 0 errors:
  `dotnet build Zongzu.sln --no-restore`
- Whitespace diff check passed:
  `git diff --check`
- Touched-file replacement-character scan passed across 17 touched files.
- Full no-build test suite passed:
  `dotnet test Zongzu.sln --no-build`
