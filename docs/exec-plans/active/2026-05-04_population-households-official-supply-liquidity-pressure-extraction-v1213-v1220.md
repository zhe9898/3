# PopulationAndHouseholds Official Supply Liquidity Pressure Extraction V1213-V1220

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for official-supply liquidity pressure bands in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-liquidity-pressure state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for `ComputeOfficialSupplyLiquidityPressure`:

- previous hardcoded official-supply liquidity grain strain bands: `grain>=80 => -2`, `grain>=55 => -1`, `grain>=25 => 1`, `grain>0 => 3`, fallback `2`
- previous hardcoded official-supply cash-need score: cash-need livelihood `2`, fallback `0`; the shared cash-need livelihood predicate is not retuned in this pass
- previous hardcoded official-supply tool drag threshold: `tool>0 && tool<35 => 1`, fallback `0`
- previous hardcoded official-supply debt drag bands: `debt>=65 => 2`, `debt>=50 => 1`, fallback `0`
- previous hardcoded official-supply liquidity pressure clamp: `-2..7`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyLiquidityGrainStrainPressureBands`
  - `DefaultOfficialSupplyLiquidityGrainStrainPressureFallbackScore = 2`
  - `DefaultOfficialSupplyLiquidityCashNeedPressureScore = 2`
  - `DefaultOfficialSupplyLiquidityCashNeedPressureFallbackScore = 0`
  - `DefaultOfficialSupplyLiquidityToolDragConditionThreshold = 35`
  - `DefaultOfficialSupplyLiquidityToolDragPressureScore = 1`
  - `DefaultOfficialSupplyLiquidityToolDragPressureFallbackScore = 0`
  - `DefaultOfficialSupplyLiquidityDebtDragPressureBands`
  - `DefaultOfficialSupplyLiquidityDebtDragPressureFallbackScore = 0`
  - `DefaultOfficialSupplyLiquidityPressureClampFloor = -2`
  - `DefaultOfficialSupplyLiquidityPressureClampCeiling = 7`

PopulationAndHouseholds remains the sole owner and consumer. The extracted liquidity pressure values are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when building the structured official-supply burden profile.

## Explicit Non-Goals

- No official-supply livelihood exposure retune.
- No official-supply resource buffer retune.
- No official-supply labor pressure retune.
- No shared cash-need livelihood predicate extraction.
- No official-supply fragility extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted grain strain bands, cash-need score, tool drag threshold, debt drag bands, fallback values, and clamp preserve the old bounds. Validation is deterministic; malformed liquidity pressure config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1213-V1220 official-supply liquidity pressure extraction ExecPlan.
2. Add default official-supply liquidity grain strain bands, cash-need score, tool drag threshold, debt drag bands, clamp validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeOfficialSupplyLiquidityPressure` hardcoded literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom liquidity pressure rules-data is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultLiquidityPressureRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomLiquidityPressureRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidLiquidityPressureRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_liquidity_pressure_extraction_v1213_v1220_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-liquidity-pressure state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused owner tests passed: `OfficialSupplyRequisition_DefaultLiquidityPressureRulesDataMatchesPreviousBaseline`, `OfficialSupplyRequisition_CustomLiquidityPressureRulesDataIsOwnerConsumed`, and `OfficialSupplyRequisition_InvalidLiquidityPressureRulesDataFallsBackToPreviousBaseline`.
- Focused architecture guards passed for V1213-V1220 and the adjacent V1205-V1212 official-supply labor pressure extraction guard.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 184/184.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 40/40.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~TenYearSimulationHealthCheckTests"` passed 6/6 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 174/174.
- `git diff --check` passed.
- Touched-file replacement-character scan passed across 17 files.
- `dotnet test Zongzu.sln --no-build` passed.
