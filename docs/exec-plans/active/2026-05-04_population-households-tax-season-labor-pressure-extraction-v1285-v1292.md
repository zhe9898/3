# V1285-V1292 PopulationAndHouseholds Tax Season Labor Pressure Extraction

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the tax-season labor pressure helper. The owner remains `PopulationAndHouseholds`, and the extracted rules are consumed only through in-memory `PopulationHouseholdMobilityRulesData`.

Runtime behavior change: default behavior unchanged. Target schema/migration impact: none.

## Extracted Hardcoded Rule

Previous hardcoded tax-season labor capacity bands:
- `>= 80 => -2`
- `>= 60 => -1`
- `>= 40 => 0`
- `>= 30 => 1`
- `>= 20 => 2`
- fallback score `3`

Previous hardcoded tax-season dependent count bands:
- `>= 5 => 2`
- `>= 3 => 1`
- fallback score `0`

Previous hardcoded tax-season dependent/labor ratio bonus:
- `DependentCount > 0 && LaborerCount > 0 && DependentCount > LaborerCount * 2 => +1`
- fallback score `0`

Previous hardcoded tax-season labor clamp: `Math.Clamp(laborPressure + dependencyPressure, -2, 5)`.

New owner-consumed defaults:
- `DefaultTaxSeasonLaborCapacityPressureBands`
- `DefaultTaxSeasonLaborCapacityPressureFallbackScore = 3`
- `DefaultTaxSeasonDependentCountPressureBands`
- `DefaultTaxSeasonDependentCountPressureFallbackScore = 0`
- `DefaultTaxSeasonDependentToLaborRatioMultiplier = 2`
- `DefaultTaxSeasonDependentToLaborRatioBonusScore = 1`
- `DefaultTaxSeasonDependentToLaborRatioFallbackScore = 0`
- `DefaultTaxSeasonLaborPressureClampFloor = -2`
- `DefaultTaxSeasonLaborPressureClampCeiling = 5`

## Non-Goals

- No tax-season fragility/interaction/debt-delta formula extraction.
- No tax-season registration visibility or liquidity retune.
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
- Focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter TaxSeasonOpened` passed 18/18.
- Focused architecture tests: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Population_households_tax_season_labor_pressure_extraction_v1285_v1292"` passed 1/1.
- Focused project tests: PopulationAndHouseholds 211/211, Presentation.Unity 40/40, Integration 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`, Architecture 183/183.
- Full tests: `dotnet test Zongzu.sln --no-build` passed.
- Diff check: `git diff --check` passed.
- Replacement-character scan: passed for 17 touched/untracked files.
