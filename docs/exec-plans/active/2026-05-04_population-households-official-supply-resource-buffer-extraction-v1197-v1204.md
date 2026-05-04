# PopulationAndHouseholds Official Supply Resource Buffer Extraction V1197-V1204

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for official-supply resource buffer bands in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-resource-buffer state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for `ComputeOfficialSupplyResourceBuffer`:

- previous hardcoded official-supply grain buffer bands: `grain>=85 => 5`, `grain>=65 => 4`, `grain>=45 => 2`, `grain>=25 => 1`, fallback `0`
- previous hardcoded official-supply tool buffer threshold: `tool>=70 => 1`, fallback `0`
- previous hardcoded official-supply shelter buffer threshold: `shelter>=60 => 1`, fallback `0`
- previous hardcoded official-supply resource buffer clamp: `0..7`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyResourceGrainBufferScoreBands`
  - `DefaultOfficialSupplyResourceGrainBufferFallbackScore = 0`
  - `DefaultOfficialSupplyResourceToolConditionThreshold = 70`
  - `DefaultOfficialSupplyResourceToolBufferScore = 1`
  - `DefaultOfficialSupplyResourceToolBufferFallbackScore = 0`
  - `DefaultOfficialSupplyResourceShelterQualityThreshold = 60`
  - `DefaultOfficialSupplyResourceShelterBufferScore = 1`
  - `DefaultOfficialSupplyResourceShelterBufferFallbackScore = 0`
  - `DefaultOfficialSupplyResourceBufferClampFloor = 0`
  - `DefaultOfficialSupplyResourceBufferClampCeiling = 7`

PopulationAndHouseholds remains the sole owner and consumer. The extracted resource buffer values are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when building the structured official-supply burden profile.

## Explicit Non-Goals

- No official-supply livelihood exposure retune.
- No official-supply labor extraction.
- No official-supply liquidity extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted official-supply grain/tool/shelter buffer bands, fallback values, and clamp preserve the old bounds. Validation is deterministic; malformed resource buffer config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1197-V1204 official-supply resource buffer extraction ExecPlan.
2. Add default official-supply resource buffer bands, threshold scores, clamp validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeOfficialSupplyResourceBuffer` hardcoded literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom resource buffer rules-data is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultResourceBufferRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomResourceBufferRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidResourceBufferRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_resource_buffer_extraction_v1197_v1204_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-resource-buffer state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused owner tests passed: `OfficialSupplyRequisition_DefaultResourceBufferRulesDataMatchesPreviousBaseline`, `OfficialSupplyRequisition_CustomResourceBufferRulesDataIsOwnerConsumed`, and `OfficialSupplyRequisition_InvalidResourceBufferRulesDataFallsBackToPreviousBaseline` (3/3).
- Focused architecture guard passed: `Population_households_official_supply_resource_buffer_extraction_v1197_v1204_must_remain_owner_consumed_and_schema_neutral` (1/1).
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed (178/178).
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed (40/40).
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~TenYearSimulationHealthCheckTests"` passed (6/6), replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed (172/172).
- `git diff --check` passed.
- Touched-file replacement-character scan passed for 16 files.
- `dotnet test Zongzu.sln --no-build` passed.
