# Household Mobility Runtime Livelihood Score Weight Extraction V797-V804

## Purpose

V797-V804 extracts the first household mobility runtime rule's livelihood score weights from a hardcoded C# switch into `PopulationHouseholdMobilityRulesData`.

This is a literal owner-consumed rules-data extraction only. The default weights remain:
- `SeasonalMigrant = 18`
- `HiredLabor = 10`
- `Tenant = 6`
- unmatched livelihoods score `0`

## Scope

- Owner: `PopulationAndHouseholds`.
- Cadence: unchanged monthly owner pass.
- Target scope: unchanged active migration pools and eligible local households selected by the existing deterministic cap/order.
- Runtime behavior under default rules-data: unchanged.
- No runtime behavior change under default rules-data.
- Schema/migration impact: none.

## Out of scope

- No livelihood engine retune.
- No livelihood trigger extraction.
- No migration-risk score weight extraction.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, livelihood-score state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted livelihood-score state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces the hardcoded livelihood score switch with a small validated ordered list consumed only by `PopulationAndHouseholds`.

Malformed score weight data falls back to the default score-weight list. Missing livelihood entries score `0`, preserving the previous fallback behavior.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted livelihood-score state, or plugin loading are introduced by V797-V804.

## Milestones

1. Add V797-V804 livelihood score-weight extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with the monthly runtime livelihood score-weight list, default, validation, and fallback getter.
3. Replace the first runtime rule's livelihood score switch with the owner-consumed score-weight lookup.
4. Add focused owner tests for default score ordering and malformed score-weight fallback.
5. Add architecture guard proving owner-consumed rules-data extraction only, default weights `[SeasonalMigrant=18, HiredLabor=10, Tenant=6]`, no score retune, no schema drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultLivelihoodScoreWeightsPreservePreviousScoreOrdering|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLivelihoodScoreWeightsFallBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_livelihood_score_weight_extraction_v797_v804_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, livelihood-score state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultLivelihoodScoreWeightsPreservePreviousScoreOrdering|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeLivelihoodScoreWeightsFallBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"` (4 tests).
- Focused architecture guard passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_livelihood_score_weight_extraction_v797_v804_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"` (1 test).
- Full architecture suite passed:
  `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore` (122 tests).
- Build passed:
  `dotnet build Zongzu.sln --no-restore`.
- Whitespace diff check passed:
  `git diff --check`.
- Replacement-character scan passed across 16 touched files.
- Full no-build suite passed:
  `dotnet test Zongzu.sln --no-build`.
