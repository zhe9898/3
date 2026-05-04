# PopulationAndHouseholds Official Supply Debt Delta Clamp Extraction V1141-V1148

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for one official-supply burden clamp in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-debt-delta-clamp state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one `OfficialSupplyBurdenProfile.DebtDelta` clamp:

- previous hardcoded official-supply debt delta clamp: `Math.Clamp(..., 0, 18)`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyDebtDeltaClampFloor = 0`
  - `DefaultOfficialSupplyDebtDeltaClampCeiling = 18`

PopulationAndHouseholds remains the sole owner and consumer. The extracted clamp bounds are validated by `PopulationHouseholdMobilityRulesData` and passed into the private official-supply burden profile produced by the owner module.

## Explicit Non-Goals

- No official-supply distress delta retune.
- No official-supply labor drop clamp extraction.
- No official-supply migration delta clamp extraction.
- No official-supply event threshold extraction.
- No official-supply signal fallback/clamp extraction.
- No official-supply livelihood/resource/labor/liquidity/fragility/interaction formula extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted clamp preserves the old `0..18` official-supply debt delta band. Validation is deterministic; malformed clamp config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1141-V1148 official-supply debt delta clamp extraction ExecPlan.
2. Add default official-supply debt delta clamp validation plus fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `OfficialSupplyBurdenProfile.DebtDelta` clamp literals with owner-consumed deterministic bounds.
4. Add focused tests proving explicit defaults preserve previous behavior, custom floor/ceiling are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultDebtDeltaClampRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomDebtDeltaClampFloorRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_CustomDebtDeltaClampCeilingRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidDebtDeltaClampRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_debt_delta_clamp_extraction_v1141_v1148_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-debt-delta-clamp state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  - `OfficialSupplyRequisition_DefaultDebtDeltaClampRulesDataMatchesPreviousBaseline`
  - `OfficialSupplyRequisition_CustomDebtDeltaClampFloorRulesDataIsOwnerConsumed`
  - `OfficialSupplyRequisition_CustomDebtDeltaClampCeilingRulesDataIsOwnerConsumed`
  - `OfficialSupplyRequisition_InvalidDebtDeltaClampRulesDataFallsBackToPreviousBaseline`
- Focused architecture guard passed: `Population_households_official_supply_debt_delta_clamp_extraction_v1141_v1148_must_remain_owner_consumed_and_schema_neutral`.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed: 155/155.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed: 165/165.
- `dotnet test Zongzu.sln --no-build` passed, including integration replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `git diff --check` passed.
- Touched-file replacement-character scan passed.
