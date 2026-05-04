# V1301-V1308 PopulationAndHouseholds Tax Season Interaction Pressure Extraction

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the tax-season interaction pressure helper. The owner remains `PopulationAndHouseholds`, and the extracted rules are consumed only through in-memory `PopulationHouseholdMobilityRulesData`.

Runtime behavior change: default behavior unchanged. Target schema/migration impact: none.

## Extracted Hardcoded Rule

Previous hardcoded tax-season interaction tenant pressure:
- livelihood `Tenant`
- distress `>= 65`
- grain store `> 0 and < 25`
- pressure score `+2`
- fallback score `0`

Previous hardcoded tax-season interaction land/labor pressure:
- land holding `>= 40`
- labor capacity `< 35`
- pressure score `+1`
- fallback score `0`

Previous hardcoded tax-season interaction cash-need pressure:
- cash-need livelihoods: `PettyTrader`, `Boatman`, `Artisan`, `SeasonalMigrant`, `HiredLabor`
- grain store `> 0 and < 30`
- debt pressure `>= 60`
- pressure score `+1`
- fallback score `0`

Previous hardcoded tax-season interaction resilience relief:
- grain store `>= 70`
- labor capacity `>= 70`
- debt pressure `< 55`
- distress `< 45`
- pressure score `-2`
- fallback score `0`

Previous hardcoded tax-season interaction clamp: `Math.Clamp(interaction, -2, 4)`.

New owner-consumed defaults:
- `DefaultTaxSeasonInteractionTenantLivelihood = LivelihoodType.Tenant`
- `DefaultTaxSeasonInteractionTenantDistressThreshold = 65`
- `DefaultTaxSeasonInteractionTenantGrainStoreFloorExclusive = 0`
- `DefaultTaxSeasonInteractionTenantGrainStoreCeilingExclusive = 25`
- `DefaultTaxSeasonInteractionTenantPressureScore = 2`
- `DefaultTaxSeasonInteractionLandHoldingThreshold = 40`
- `DefaultTaxSeasonInteractionLaborCapacityThreshold = 35`
- `DefaultTaxSeasonInteractionLandLaborPressureScore = 1`
- `DefaultTaxSeasonInteractionCashNeedLivelihoodPressureScoreWeights`
- `DefaultTaxSeasonInteractionCashNeedGrainStoreFloorExclusive = 0`
- `DefaultTaxSeasonInteractionCashNeedGrainStoreCeilingExclusive = 30`
- `DefaultTaxSeasonInteractionCashNeedDebtPressureThreshold = 60`
- `DefaultTaxSeasonInteractionResilienceReliefGrainStoreThreshold = 70`
- `DefaultTaxSeasonInteractionResilienceReliefLaborCapacityThreshold = 70`
- `DefaultTaxSeasonInteractionResilienceReliefDebtPressureThresholdExclusive = 55`
- `DefaultTaxSeasonInteractionResilienceReliefDistressThresholdExclusive = 45`
- `DefaultTaxSeasonInteractionResilienceReliefScore = -2`
- `DefaultTaxSeasonInteractionPressureClampFloor = -2`
- `DefaultTaxSeasonInteractionPressureClampCeiling = 4`

## Non-Goals

- No tax-season debt-delta formula extraction.
- No tax-season registration visibility, liquidity, labor, or fragility retune.
- No official-supply extraction.
- No household movement command.
- No migration economy.
- No class/status engine.
- No route-history.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, or cooldown ledger.
- No rules-data loader.
- No rules-data file.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No runtime assemblies.
- No reflection-heavy rule loading.
- No persisted state.
- No schema bump.
- No `PersonRegistry` expansion.
- No Application/UI/Unity authority.
- No prose parsing.

## Validation Plan

- focused architecture test proves owner-only extraction, removed C# literals, docs evidence, no schema drift, and no UI/Application/Unity authority.
- focused `TaxSeasonBurdenHandlerTests` prove default config produces previous behavior, custom rules data is owner-consumed, and malformed config falls back deterministically.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Evidence

- Build: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter TaxSeasonOpened` passed 24/24.
- Focused architecture guard: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Population_households_tax_season_interaction_pressure_extraction_v1301_v1308"` passed 1/1.
- PopulationAndHouseholds project: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 217/217.
- Presentation project: `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 40/40.
- Integration project: `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build` passed 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- Architecture project: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 185/185.
- Full solution: `dotnet test Zongzu.sln --no-build` passed.
- Diff hygiene: `git diff --check` passed.
- Replacement-character scan: touched-file scan passed clean across 17 files.
