# V1277-V1284 PopulationAndHouseholds Tax Season Liquidity Pressure Extraction

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the tax-season liquidity pressure helper. The owner remains `PopulationAndHouseholds`, and the extracted rules are consumed only through in-memory `PopulationHouseholdMobilityRulesData`.

Runtime behavior change: default behavior unchanged. Target schema/migration impact: none.

## Extracted Hardcoded Rule

Previous hardcoded tax-season liquidity grain pressure bands:
- `>= 80 => -3`
- `>= 60 => -2`
- `>= 40 => -1`
- `>= 20 => 1`
- `> 0 => 3`
- fallback score `0`

Previous hardcoded tax-season liquidity cash-need scores:
- `PettyTrader => 2`
- `Boatman => 2`
- `Artisan => 2`
- `SeasonalMigrant => 2`
- `HiredLabor => 1`
- `Vagrant => 1`
- `Tenant => 1`
- fallback score `0`

Previous hardcoded tax-season liquidity tool drag: `ToolCondition is > 0 and < 35 ? 1 : 0`.

Previous hardcoded tax-season liquidity clamp: `Math.Clamp(grainPressure + cashNeed + toolDrag, -3, 5)`.

New owner-consumed defaults:
- `DefaultTaxSeasonLiquidityGrainPressureBands`
- `DefaultTaxSeasonLiquidityGrainPressureFallbackScore = 0`
- `DefaultTaxSeasonLiquidityCashNeedScoreWeights`
- `DefaultTaxSeasonLiquidityCashNeedFallbackScore = 0`
- `DefaultTaxSeasonLiquidityToolDragConditionThreshold = 35`
- `DefaultTaxSeasonLiquidityToolDragScore = 1`
- `DefaultTaxSeasonLiquidityToolDragFallbackScore = 0`
- `DefaultTaxSeasonLiquidityPressureClampFloor = -3`
- `DefaultTaxSeasonLiquidityPressureClampCeiling = 5`

## Non-Goals

- No tax-season labor/fragility/interaction/debt-delta formula extraction.
- No tax-season registration visibility retune.
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
- Focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter TaxSeasonOpened` passed 15/15.
- Focused architecture tests: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Population_households_tax_season_liquidity_pressure_extraction_v1277_v1284"` passed 1/1.
- Focused project tests: PopulationAndHouseholds 208/208, Presentation.Unity 40/40, Integration 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`, Architecture 182/182.
- Full tests: `dotnet test Zongzu.sln --no-build` passed.
- Diff check: `git diff --check` passed.
- Replacement-character scan: passed for 17 touched/untracked files.
