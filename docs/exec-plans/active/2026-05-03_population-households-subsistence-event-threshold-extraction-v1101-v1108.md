# PopulationAndHouseholds Subsistence Event Threshold Extraction V1101-V1108

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, subsistence-event-threshold state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one remaining `ApplyGrainPriceSubsistencePressure` event threshold:

- previous hardcoded event threshold: `oldDistress < 60 && household.Distress >= 60`
- new owner-consumed rules-data default:
  - `DefaultSubsistencePressureEventDistressThreshold = 60`

PopulationAndHouseholds remains the sole owner and consumer. The extracted threshold is validated by `PopulationHouseholdMobilityRulesData` and read only inside the owner module's grain-price subsistence event dispatch helper.

## Explicit Non-Goals

- No subsistence distress delta clamp extraction.
- No tax-season or official-supply formula extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted threshold preserves the old `60` distress crossing gate. Validation is deterministic; malformed threshold config falls back through owner getters to the same default.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1101-V1108 subsistence event threshold extraction ExecPlan.
2. Add default event threshold validation plus fallback getter to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ApplyGrainPriceSubsistencePressure` distress threshold literals with owner-consumed deterministic lookups.
4. Add focused tests proving explicit defaults preserve previous behavior, custom threshold is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultSubsistenceEventThresholdRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomSubsistenceEventThresholdRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidSubsistenceEventThresholdRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_event_threshold_extraction_v1101_v1108_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, subsistence-event-threshold state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors after a serial rerun.
- 2026-05-03: focused owner tests passed: `GrainPriceSpike_DefaultSubsistenceEventThresholdRulesDataMatchesPreviousBaseline`, `GrainPriceSpike_CustomSubsistenceEventThresholdRulesDataIsOwnerConsumed`, and `GrainPriceSpike_InvalidSubsistenceEventThresholdRulesDataFallsBackToPreviousBaseline` (3/3).
- 2026-05-03: focused architecture guard passed: `Population_households_subsistence_event_threshold_extraction_v1101_v1108_must_remain_owner_consumed_and_schema_neutral` (1/1).
- 2026-05-03: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed (136/136).
- 2026-05-03: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed (160/160).
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; ten-year replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: touched-file replacement-character scan passed for 17 touched files.
