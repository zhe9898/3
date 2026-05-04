# PopulationAndHouseholds Official Supply Livelihood Exposure Extraction V1189-V1196

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for official-supply livelihood exposure and land visibility bands in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-livelihood-exposure state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for `ComputeOfficialSupplyLivelihoodExposurePressure`:

- previous hardcoded official-supply livelihood exposure scores: `Boatman=5`, `HiredLabor=4`, `SeasonalMigrant=4`, `Smallholder=3`, `Tenant=3`, `Artisan=2`, `PettyTrader=2`, `YamenRunner=2`, `Unknown=2`, `DomesticServant=1`, `Vagrant=1`, unmatched `2`
- previous hardcoded official-supply land visibility bands: `land>=70 => 2`, `land>=35 => 1`, fallback `0`
- previous hardcoded official-supply livelihood exposure clamp: `1..7`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyLivelihoodExposureScoreWeights`
  - `DefaultOfficialSupplyLivelihoodExposureFallbackScore = 2`
  - `DefaultOfficialSupplyLandVisibilityScoreBands`
  - `DefaultOfficialSupplyLandVisibilityFallbackScore = 0`
  - `DefaultOfficialSupplyLivelihoodExposureClampFloor = 1`
  - `DefaultOfficialSupplyLivelihoodExposureClampCeiling = 7`

PopulationAndHouseholds remains the sole owner and consumer. The extracted livelihood exposure values are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when building the structured official-supply burden profile.

## Explicit Non-Goals

- No official-supply fallback value retune.
- No official-supply signal normalization clamp retune.
- No official-supply resource buffer extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted official-supply livelihood exposure scores, land visibility bands, fallback values, and clamp preserve the old bounds. Validation is deterministic; malformed livelihood exposure config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1189-V1196 official-supply livelihood exposure extraction ExecPlan.
2. Add default official-supply livelihood exposure score weights, land visibility bands, clamp validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeOfficialSupplyLivelihoodExposurePressure` hardcoded literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom livelihood exposure rules-data is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultLivelihoodExposureRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomLivelihoodExposureRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidLivelihoodExposureRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_livelihood_exposure_extraction_v1189_v1196_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-livelihood-exposure state rollback, or production data rollback is required.

## Evidence log

- 2026-05-04: `dotnet build Zongzu.sln --no-restore` passed.
- 2026-05-04: focused owner tests passed: `OfficialSupplyRequisition_DefaultLivelihoodExposureRulesDataMatchesPreviousBaseline`, `OfficialSupplyRequisition_CustomLivelihoodExposureRulesDataIsOwnerConsumed`, and `OfficialSupplyRequisition_InvalidLivelihoodExposureRulesDataFallsBackToPreviousBaseline`.
- 2026-05-04: focused architecture guard passed: `Population_households_official_supply_livelihood_exposure_extraction_v1189_v1196_must_remain_owner_consumed_and_schema_neutral`.
- 2026-05-04: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed, 175/175.
- 2026-05-04: `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed, 40/40.
- 2026-05-04: ten-year integration health check passed, 6/6, replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-04: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --logger "console;verbosity=minimal"` passed, 171/171.
- 2026-05-04: `git diff --check` passed.
- 2026-05-04: touched-file replacement-character scan passed.
- 2026-05-04: `dotnet test Zongzu.sln --no-build` passed.
