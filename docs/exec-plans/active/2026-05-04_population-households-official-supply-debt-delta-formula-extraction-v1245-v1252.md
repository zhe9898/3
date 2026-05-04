# PopulationAndHouseholds Official Supply Debt Delta Formula Extraction V1245-V1252

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for the official-supply `DebtDelta` formula inside `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-debt-delta state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for `OfficialSupplyBurdenProfile.DebtDelta`:

- previous hardcoded official-supply debt quota pressure divisor: `QuotaPressure / 4`
- previous hardcoded official-supply debt liquidity pressure weight: `+ LiquidityPressure`
- previous hardcoded official-supply debt fragility pressure divisor: `FragilityPressure / 2`
- previous hardcoded official-supply debt interaction floor and weight: `Math.Max(0, InteractionPressure)`
- previous hardcoded official-supply debt clerk distortion divisor: `ClerkDistortionPressure / 4`
- previous hardcoded official-supply debt resource buffer divisor: `ResourceBuffer / 2`
- existing official-supply debt delta clamp stays owner-consumed and unchanged: `0..18`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyDebtDeltaQuotaPressureDivisor = 4`
  - `DefaultOfficialSupplyDebtDeltaLiquidityPressureWeight = 1`
  - `DefaultOfficialSupplyDebtDeltaFragilityPressureDivisor = 2`
  - `DefaultOfficialSupplyDebtDeltaInteractionPressureFloor = 0`
  - `DefaultOfficialSupplyDebtDeltaInteractionPressureWeight = 1`
  - `DefaultOfficialSupplyDebtDeltaClerkDistortionPressureDivisor = 4`
  - `DefaultOfficialSupplyDebtDeltaResourceBufferDivisor = 2`

PopulationAndHouseholds remains the sole owner and consumer. The extracted formula parameters are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when building the structured official-supply burden profile.

## Explicit Non-Goals

- No official-supply livelihood exposure retune.
- No official-supply resource buffer retune.
- No official-supply labor pressure retune.
- No official-supply liquidity pressure retune.
- No official-supply fragility pressure retune.
- No official-supply interaction pressure retune.
- No official-supply distress/labor/migration delta formula extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted divisors, interaction floor, and component weights preserve the old integer formula and existing clamp. Validation is deterministic; malformed formula config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1245-V1252 official-supply debt delta formula extraction ExecPlan.
2. Add default formula divisors, interaction floor, component weights, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace `OfficialSupplyBurdenProfile.DebtDelta` hardcoded literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom formula rules-data is consumed, and malformed formula config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultDebtDeltaFormulaRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomDebtDeltaFormulaRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidDebtDeltaFormulaRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_debt_delta_formula_extraction_v1245_v1252_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-debt-delta state rollback, or production data rollback is required.

## Evidence log

- 2026-05-04: Added owner-consumed official-supply debt delta formula defaults, validation, and fallback getters in `PopulationHouseholdMobilityRulesData`; replaced the previous hardcoded `OfficialSupplyBurdenProfile.DebtDelta` literals with deterministic owner-consumed profile fields. No persisted state, schema version, rules-data file, loader, route-history, movement command, PersonRegistry expansion, or Application/UI/Unity authority was added.
- 2026-05-04: Focused owner tests passed: `OfficialSupplyRequisition_DefaultDebtDeltaFormulaRulesDataMatchesPreviousBaseline`, `OfficialSupplyRequisition_CustomDebtDeltaFormulaRulesDataIsOwnerConsumed`, and `OfficialSupplyRequisition_InvalidDebtDeltaFormulaRulesDataFallsBackToPreviousBaseline`.
- 2026-05-04: Focused architecture guard passed: `Population_households_official_supply_debt_delta_formula_extraction_v1245_v1252_must_remain_owner_consumed_and_schema_neutral`.
- 2026-05-04: Expanded validation passed: `Zongzu.Modules.PopulationAndHouseholds.Tests` 196/196, `Zongzu.Presentation.Unity.Tests` 40/40, `Zongzu.Integration.Tests` 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`, and `Zongzu.Architecture.Tests` 178/178.
- 2026-05-04: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-04: `git diff --check` passed.
- 2026-05-04: Touched-file replacement-character scan passed across 17 files.
- 2026-05-04: `dotnet test Zongzu.sln --no-build -- NUnit.NumberOfTestWorkers=1` passed: Kernel 2/2, Scheduler 4/4, Persistence 39/39, Integration 137/137, Architecture 178/178, PopulationAndHouseholds 196/196, Presentation.Unity 40/40, and all other module test projects passed.
