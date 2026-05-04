# PopulationAndHouseholds Official Supply Fragility Pressure Extraction V1221-V1228

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for official-supply fragility pressure bands in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-fragility-pressure state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for `ComputeOfficialSupplyFragilityPressure`:

- previous hardcoded official-supply fragility distress bands: `distress>=80 => 3`, `distress>=65 => 2`, `distress>=50 => 1`, fallback `0`
- previous hardcoded official-supply fragility debt bands: `debt>=80 => 3`, `debt>=65 => 2`, `debt>=50 => 1`, fallback `0`
- previous hardcoded official-supply fragility migration score: `IsMigrating || MigrationRisk>=70 => 1`, fallback `0`
- previous hardcoded official-supply fragility shelter drag: `0<shelter<35 => 1`, fallback `0`
- previous hardcoded official-supply fragility pressure clamp: `0..8`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyFragilityDistressPressureBands`
  - `DefaultOfficialSupplyFragilityDistressPressureFallbackScore = 0`
  - `DefaultOfficialSupplyFragilityDebtPressureBands`
  - `DefaultOfficialSupplyFragilityDebtPressureFallbackScore = 0`
  - `DefaultOfficialSupplyFragilityMigrationRiskThreshold = 70`
  - `DefaultOfficialSupplyFragilityMigrationPressureScore = 1`
  - `DefaultOfficialSupplyFragilityMigrationPressureFallbackScore = 0`
  - `DefaultOfficialSupplyFragilityShelterDragQualityThreshold = 35`
  - `DefaultOfficialSupplyFragilityShelterDragPressureScore = 1`
  - `DefaultOfficialSupplyFragilityShelterDragPressureFallbackScore = 0`
  - `DefaultOfficialSupplyFragilityPressureClampFloor = 0`
  - `DefaultOfficialSupplyFragilityPressureClampCeiling = 8`

PopulationAndHouseholds remains the sole owner and consumer. The extracted fragility pressure values are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when building the structured official-supply burden profile.

## Explicit Non-Goals

- No official-supply livelihood exposure retune.
- No official-supply resource buffer retune.
- No official-supply labor pressure retune.
- No official-supply liquidity pressure retune.
- No official-supply interaction extraction.
- No official-supply formula divisor extraction.
- No official-supply distress/debt/labor/migration delta retune.
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

Runtime determinism risk is unchanged under default rules-data because the extracted distress bands, debt bands, migration threshold, shelter threshold, fallback values, and clamp preserve the old bounds. Validation is deterministic; malformed fragility pressure config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1221-V1228 official-supply fragility pressure extraction ExecPlan.
2. Add default official-supply fragility distress bands, debt bands, migration threshold, shelter threshold, clamp validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeOfficialSupplyFragilityPressure` hardcoded literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom fragility pressure rules-data is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultFragilityPressureRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomFragilityPressureRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidFragilityPressureRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_fragility_pressure_extraction_v1221_v1228_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-fragility-pressure state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors after the final code/test/doc updates.
- Focused owner tests passed: `OfficialSupplyRequisition_DefaultFragilityPressureRulesDataMatchesPreviousBaseline`, `OfficialSupplyRequisition_CustomFragilityPressureRulesDataIsOwnerConsumed`, and `OfficialSupplyRequisition_InvalidFragilityPressureRulesDataFallsBackToPreviousBaseline`.
- Focused architecture guards passed for V1221-V1228 and the adjacent V1213-V1220 official-supply liquidity pressure extraction guard.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 187/187.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 40/40.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~TenYearSimulationHealthCheckTests"` passed 6/6 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 175/175.
- `git diff --check` passed.
- Touched-file replacement-character scan passed across 17 files.
- `dotnet test Zongzu.sln --no-build` passed.
