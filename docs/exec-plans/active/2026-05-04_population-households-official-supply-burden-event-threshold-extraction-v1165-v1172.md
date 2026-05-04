# PopulationAndHouseholds Official Supply Burden Event Threshold Extraction V1165-V1172

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for the official-supply household burden event threshold in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-burden-event-threshold state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one `DispatchOfficeSupplyEvents` event threshold:

- previous hardcoded official-supply burden event distress threshold: `80`
- new owner-consumed rules-data default:
  - `DefaultOfficialSupplyBurdenEventDistressThreshold = 80`

PopulationAndHouseholds remains the sole owner and consumer. The extracted threshold is validated by `PopulationHouseholdMobilityRulesData` and read by the owner module before emitting `HouseholdBurdenIncreased`.

## Explicit Non-Goals

- No official-supply distress delta retune.
- No official-supply debt delta retune.
- No official-supply labor drop retune.
- No official-supply migration delta retune.
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

Runtime determinism risk is unchanged under default rules-data because the extracted event threshold preserves the old official-supply burden distress boundary of `80`. Validation is deterministic; malformed threshold config falls back through the owner getter to the same default.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1165-V1172 official-supply burden event threshold extraction ExecPlan.
2. Add default official-supply burden event threshold validation plus fallback getter to `PopulationHouseholdMobilityRulesData`.
3. Replace the `DispatchOfficeSupplyEvents` threshold literal with the owner-consumed deterministic threshold.
4. Add focused tests proving explicit defaults preserve previous behavior, custom threshold is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultBurdenEventDistressThresholdRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomBurdenEventDistressThresholdRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidBurdenEventDistressThresholdRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_burden_event_threshold_extraction_v1165_v1172_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-burden-event-threshold state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  - `OfficialSupplyRequisition_DefaultBurdenEventDistressThresholdRulesDataMatchesPreviousBaseline`
  - `OfficialSupplyRequisition_CustomBurdenEventDistressThresholdRulesDataIsOwnerConsumed`
  - `OfficialSupplyRequisition_InvalidBurdenEventDistressThresholdRulesDataFallsBackToPreviousBaseline`
- Focused architecture guard passed: `Population_households_official_supply_burden_event_threshold_extraction_v1165_v1172_must_remain_owner_consumed_and_schema_neutral`.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed: 166/166.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed: 168/168.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed: 40/40.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "Name=CampaignEnabledStressSandbox_TenYearHealthReport"` passed with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `dotnet test Zongzu.sln --no-build` passed, including architecture 168/168 and integration replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `git diff --check` passed.
- Touched-file replacement-character scan passed.
