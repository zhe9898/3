# PopulationAndHouseholds Official Supply Distress Delta Formula Extraction V1237-V1244

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for the official-supply `DistressDelta` formula inside `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-distress-delta state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for `OfficialSupplyBurdenProfile.DistressDelta`:

- previous hardcoded official-supply distress supply pressure divisor: `SupplyPressure / 4`
- previous hardcoded official-supply distress livelihood exposure weight: `+ LivelihoodExposurePressure`
- previous hardcoded official-supply distress labor pressure weight: `+ LaborPressure`
- previous hardcoded official-supply distress fragility pressure weight: `+ FragilityPressure`
- previous hardcoded official-supply distress clerk distortion divisor: `ClerkDistortionPressure / 3`
- previous hardcoded official-supply distress interaction pressure weight: `+ InteractionPressure`
- previous hardcoded official-supply distress resource buffer relief weight: `- ResourceBuffer`
- previous hardcoded official-supply distress authority buffer divisor: `AuthorityBuffer / 3`
- existing official-supply distress delta clamp stays owner-consumed and unchanged: `0..24`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyDistressDeltaSupplyPressureDivisor = 4`
  - `DefaultOfficialSupplyDistressDeltaLivelihoodExposureWeight = 1`
  - `DefaultOfficialSupplyDistressDeltaLaborPressureWeight = 1`
  - `DefaultOfficialSupplyDistressDeltaFragilityPressureWeight = 1`
  - `DefaultOfficialSupplyDistressDeltaClerkDistortionPressureDivisor = 3`
  - `DefaultOfficialSupplyDistressDeltaInteractionPressureWeight = 1`
  - `DefaultOfficialSupplyDistressDeltaResourceBufferWeight = 1`
  - `DefaultOfficialSupplyDistressDeltaAuthorityBufferDivisor = 3`

PopulationAndHouseholds remains the sole owner and consumer. The extracted formula parameters are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when building the structured official-supply burden profile.

## Explicit Non-Goals

- No official-supply livelihood exposure retune.
- No official-supply resource buffer retune.
- No official-supply labor pressure retune.
- No official-supply liquidity pressure retune.
- No official-supply fragility pressure retune.
- No official-supply interaction pressure retune.
- No official-supply debt/labor/migration delta formula extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted divisors and component weights preserve the old integer formula and existing clamp. Validation is deterministic; malformed formula config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1237-V1244 official-supply distress delta formula extraction ExecPlan.
2. Add default formula divisors and component weights, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace `OfficialSupplyBurdenProfile.DistressDelta` hardcoded literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom formula rules-data is consumed, and malformed formula config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultDistressDeltaFormulaRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomDistressDeltaFormulaRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidDistressDeltaFormulaRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_distress_delta_formula_extraction_v1237_v1244_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-distress-delta state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused owner tests passed 3/3: default distress delta formula rules-data matches previous baseline, custom formula rules-data is owner-consumed, and malformed formula rules-data falls back deterministically.
- Focused architecture guard passed 1/1: `Population_households_official_supply_distress_delta_formula_extraction_v1237_v1244_must_remain_owner_consumed_and_schema_neutral`.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 193/193.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 40/40.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build` passed 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 177/177.
- `git diff --check` passed.
- Touched-file replacement-character scan passed across 17 files.
- `dotnet test Zongzu.sln --no-build` passed, including PopulationAndHouseholds 193/193, Presentation.Unity 40/40, Integration 137/137, Architecture 177/177, and replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
