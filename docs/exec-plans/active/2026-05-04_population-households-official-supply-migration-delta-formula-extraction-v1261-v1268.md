# PopulationAndHouseholds Official Supply Migration Delta Formula Extraction V1261-V1268

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for the official-supply `MigrationDelta` formula inside `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-migration-delta state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for `OfficialSupplyBurdenProfile.MigrationDelta`:

- previous hardcoded official-supply migration delta distress delta divisor: `DistressDelta / 5`
- previous hardcoded official-supply migration delta debt delta divisor: `DebtDelta / 6`
- previous hardcoded official-supply migration delta fragility pressure threshold and boost: `FragilityPressure >= 5 ? 1 : 0`
- existing official-supply migration delta clamp stays owner-consumed and unchanged: `0..8`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyMigrationDeltaDistressDeltaDivisor = 5`
  - `DefaultOfficialSupplyMigrationDeltaDebtDeltaDivisor = 6`
  - `DefaultOfficialSupplyMigrationDeltaFragilityPressureThreshold = 5`
  - `DefaultOfficialSupplyMigrationDeltaFragilityBoostScore = 1`

PopulationAndHouseholds remains the sole owner and consumer. The extracted formula parameters are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when building the structured official-supply burden profile.

## Explicit Non-Goals

- No official-supply livelihood exposure retune.
- No official-supply resource buffer retune.
- No official-supply labor pressure retune.
- No official-supply liquidity pressure retune.
- No official-supply fragility pressure retune.
- No official-supply interaction pressure retune.
- No official-supply distress/debt/labor delta formula extraction.
- No official-supply event threshold retune.
- No tax-season formula extraction.
- No runtime rules-data loader.
- No rules-data loader.
- No rules-data file.
- No content/config namespace.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No runtime assemblies.
- No reflection-heavy rule loading.
- No household movement command.
- No direct route-history.
- No migration economy.
- No class/status engine.
- No persisted state.
- No schema bump.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, or pressure-profile ledger.
- No `PersonRegistry` expansion.
- No Application/UI/Unity authority.
- No prose parsing from `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.

## Determinism Risk

Runtime determinism risk is unchanged under default rules-data because the extracted divisors, fragility threshold/boost, and existing clamp preserve the old integer formula. Validation is deterministic; malformed formula config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1261-V1268 official-supply migration delta formula extraction ExecPlan.
2. Add default formula divisors, fragility threshold/boost, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace `OfficialSupplyBurdenProfile.MigrationDelta` hardcoded literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom formula rules-data is consumed, and malformed formula config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultMigrationDeltaFormulaRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomMigrationDeltaFormulaRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidMigrationDeltaFormulaRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_migration_delta_formula_extraction_v1261_v1268_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build -- NUnit.NumberOfTestWorkers=1`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build -- NUnit.NumberOfTestWorkers=1`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build -- NUnit.NumberOfTestWorkers=1`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-migration-delta state rollback, or production data rollback is required.

## Evidence log

- 2026-05-04: Added owner-consumed official-supply migration delta formula defaults, validation, and fallback getters in `PopulationHouseholdMobilityRulesData`; replaced the previous hardcoded `OfficialSupplyBurdenProfile.MigrationDelta` literals with deterministic owner-consumed profile fields. No persisted state, schema version, rules-data file, loader, route-history, movement command, PersonRegistry expansion, or Application/UI/Unity authority was added.
- 2026-05-04: Focused owner tests passed: `OfficialSupplyRequisition_DefaultMigrationDeltaFormulaRulesDataMatchesPreviousBaseline`, `OfficialSupplyRequisition_CustomMigrationDeltaFormulaRulesDataIsOwnerConsumed`, and `OfficialSupplyRequisition_InvalidMigrationDeltaFormulaRulesDataFallsBackToPreviousBaseline`.
- 2026-05-04: Focused architecture guard passed: `Population_households_official_supply_migration_delta_formula_extraction_v1261_v1268_must_remain_owner_consumed_and_schema_neutral`.
- 2026-05-04: Expanded validation passed: `Zongzu.Modules.PopulationAndHouseholds.Tests` 202/202, `Zongzu.Presentation.Unity.Tests` 40/40, `Zongzu.Integration.Tests` 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`, and `Zongzu.Architecture.Tests` 180/180.
- 2026-05-04: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-04: `dotnet test Zongzu.sln --no-build -- NUnit.NumberOfTestWorkers=1` passed: Kernel 2/2, Scheduler 4/4, Persistence 39/39, Integration 137/137, Architecture 180/180, PopulationAndHouseholds 202/202, Presentation.Unity 40/40, and all other module test projects passed.
