# Household Mobility Rules-Data Fallback Matrix V573-V580

## Goal

Add focused fallback evidence for the household mobility runtime rules-data shape after V565-V572 touch-count proof.

This pass is focused fallback evidence and docs evidence only. It does not change runtime behavior, add a loader, add a rules-data file, widen fanout, add a second household mobility runtime rule, or change save schema.

## Scope

In scope:
- Add focused `PopulationHouseholdMobilityRulesData` validation evidence proving malformed runtime parameters fall back deterministically to defaults:
  - `monthly_runtime_active_pool_outflow_threshold`;
  - `monthly_runtime_settlement_cap`;
  - `monthly_runtime_household_cap`;
  - `monthly_runtime_risk_delta`.
- Add an owner test proving malformed runtime rules-data produces the same `PopulationAndHouseholds` monthly result signature as default rules-data.
- Malformed runtime rules-data falls back deterministically to defaults.
- Add architecture guard coverage that this pass remains focused fallback evidence and not a runtime loader or plugin system.
- Update required docs with schema/migration impact and no-authority drift evidence.

Out of scope:
- No runtime behavior change.
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
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, persisted touch-count state, diagnostic state, or performance cache.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, touched counts, target eligibility, validation fallback, health classification, or performance status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No long-run saturation tuning.
- No performance optimization claim.

## Affected modules

- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this fallback matrix pass.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, persisted touch-count state, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged because production code is unchanged.

The focused owner test records the existing deterministic fallback behavior:
- malformed runtime rules-data validates with readable errors;
- all runtime fallback getters return the same default values;
- malformed runtime rules-data falls back deterministically to defaults;
- the resulting monthly run signature matches default rules-data exactly.

No random choice, unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, or plugin loading are introduced by V573-V580.

## Milestones

1. Add V573-V580 fallback matrix ExecPlan.
2. Add focused rules-data validation fallback test.
3. Add focused owner-result fallback equivalence test.
4. Add architecture guard proving this remains test/docs evidence only:
   - no runtime behavior change;
   - no loader or file;
   - no runtime plugin marketplace;
   - no fanout widening;
   - no second runtime rule;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no persisted touch-count/diagnostic/performance state;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift.
5. Update required docs and acceptance evidence.
6. Run focused owner/architecture validation plus full build/test hygiene.

## Tests to add/update

- Add:
  - `PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults`
  - `RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome`
- Focused owner tests:
  - `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_rules_data_fallback_matrix_v573_v580_must_remain_test_evidence_only_without_loader_or_schema_drift"`
- Build:
  - `dotnet build Zongzu.sln --no-restore`
- Diff hygiene:
  - `git diff --check`
- Encoding:
  - touched-file replacement-character scan
- Full no-build solution test:
  - `dotnet test Zongzu.sln --no-build`

## Evidence Log

Local validation:
- Passed: focused owner tests:
  `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "PopulationHouseholdMobilityRulesData_InvalidRuntimeParametersFallBackToDefaults|RunMonth_FirstMobilityRuntimeRuleMalformedRulesDataFallsBackToDefaultOutcome"`
- Passed: focused architecture test:
  `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_rules_data_fallback_matrix_v573_v580_must_remain_test_evidence_only_without_loader_or_schema_drift"`
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build` with 541 tests and ten-year health replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.

PR validation:
- Pending: PR CI.

## Rollback / fallback plan

Remove the V573-V580 focused tests, docs, and architecture guard. No production code or save/schema rollback is required.

## Open questions

- None for this pass. A real rules-data loader or default rules-data file still requires a separate ExecPlan and must remain owner-consumed only.
