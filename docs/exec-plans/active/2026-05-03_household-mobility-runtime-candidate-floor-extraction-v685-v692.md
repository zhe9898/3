# Household Mobility Runtime Candidate Floor Extraction V685-V692

## Goal

Extract the first runtime rule's candidate migration-risk floor from a naked C# literal into `PopulationHouseholdMobilityRulesData` as owner-consumed rules-data shape.

This is a small hardcoded extraction pass, not a migration engine. The default candidate floor remains 55, so default runtime behavior remains equivalent to the previous rule. The extracted value is consumed only by `PopulationAndHouseholds` when deciding whether a household is eligible for the existing monthly pressure nudge.

## Scope

In scope:
- Add `MonthlyRuntimeCandidateMigrationRiskFloor` to `PopulationHouseholdMobilityRulesData`.
- Validate the candidate floor deterministically and fall back to the default on malformed rules-data.
- Use the owner-consumed value only inside `PopulationAndHouseholds.ApplyMonthlyHouseholdMobilityRuntimeRule`.
- Add focused owner tests proving default no-touch behavior is preserved and malformed candidate-floor data falls back to default.
- Add architecture guard coverage proving this is owner-consumed rules-data extraction without schema or authority drift.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change under default rules-data.
- No fanout widening.
- No high-risk filter retune.
- No general migration-state threshold retune.
- No event threshold retune.
- No pool ordering retune.
- No score formula retune.
- No candidate ordering retune.
- No cap semantics retune.
- No global household cap.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, candidate-floor state, validation ledger, diagnostic state, performance cache, event ledger, event-routing ledger, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, candidate-floor eligibility, target selection, fanout eligibility, validation fallback, or no-touch status.
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

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, persisted candidate-floor state, diagnostic state, performance cache, validation ledger, event ledger, event-routing ledger, migration-started selector state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk remains bounded because the extraction replaces one literal with an owner-consumed validated integer and keeps the default at 55.

The focused owner tests prove:
- default rules-data and explicit default candidate floor produce the same monthly runtime signature;
- the below-floor fixture remains no-touch compared with a zero-risk-delta baseline;
- malformed candidate-floor data falls back to the default floor;
- malformed runtime rules-data still produces the default monthly run signature.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted candidate-floor state, or plugin loading are introduced by V685-V692.

## Milestones

1. Add V685-V692 candidate-floor extraction ExecPlan.
2. Extend `PopulationHouseholdMobilityRulesData` with the monthly runtime candidate migration-risk floor, default, validation, and fallback getter.
3. Replace the first runtime rule's candidate-floor literal with the owner-consumed getter.
4. Add focused owner tests for default equivalence and malformed floor fallback.
5. Add architecture guard proving:
   - owner-consumed rules-data extraction only;
   - default candidate floor remains 55;
   - no fanout widening;
   - no high-risk filter retune;
   - no general migration-state threshold retune;
   - no second runtime rule;
   - no loader or file;
   - no runtime plugin marketplace;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority.
6. Update required docs.
7. Run focused owner and architecture tests, build, diff, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskFloorPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskFloorFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_candidate_floor_extraction_v685_v692_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, event routing rollback, candidate-floor state rollback, or production data rollback is required.

## Evidence log

Completed on 2026-05-03:

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "Name=RunMonth_FirstMobilityRuntimeRuleDefaultCandidateMigrationRiskFloorPreservesPreviousNoTouchBehavior|Name=PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCandidateMigrationRiskFloorFallsBackToDefault|Name=PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|Name=RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"` passed: 4/4.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_runtime_candidate_floor_extraction_v685_v692_must_remain_owner_consumed_rules_data_without_schema_or_authority_drift"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Touched-file replacement-character scan passed for 17 files.
- `dotnet test Zongzu.sln --no-build` passed: 571 tests.
- Ten-year health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

Schema/migration impact remains none. V685-V692 extracts one low-risk candidate-floor literal into owner-consumed `PopulationHouseholdMobilityRulesData`; default candidate floor 55 preserves previous no-touch behavior under default rules-data and adds no loader, file, movement authority, schema state, or Application/UI/Unity authority.
