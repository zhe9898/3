# V1293-V1300 PopulationAndHouseholds Tax Season Fragility Pressure Extraction

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the tax-season fragility pressure helper. The owner remains `PopulationAndHouseholds`, and the extracted rules are consumed only through in-memory `PopulationHouseholdMobilityRulesData`.

Runtime behavior change: default behavior unchanged. Target schema/migration impact: none.

## Extracted Hardcoded Rule

Previous hardcoded tax-season fragility distress bands:
- `>= 80 => 3`
- `>= 65 => 2`
- `>= 50 => 1`
- fallback score `0`

Previous hardcoded tax-season fragility debt bands:
- `>= 80 => 3`
- `>= 65 => 2`
- `>= 55 => 1`
- fallback score `0`

Previous hardcoded tax-season fragility shelter drag: `ShelterQuality is > 0 and < 35 ? 1 : 0`.

Previous hardcoded tax-season fragility migration drag: `IsMigrating || MigrationRisk >= 70 ? 1 : 0`.

Previous hardcoded tax-season fragility clamp: `Math.Clamp(distressPressure + debtPressure + shelterDrag + migrationDrag, 0, 7)`.

New owner-consumed defaults:
- `DefaultTaxSeasonFragilityDistressPressureBands`
- `DefaultTaxSeasonFragilityDistressPressureFallbackScore = 0`
- `DefaultTaxSeasonFragilityDebtPressureBands`
- `DefaultTaxSeasonFragilityDebtPressureFallbackScore = 0`
- `DefaultTaxSeasonFragilityShelterDragQualityThreshold = 35`
- `DefaultTaxSeasonFragilityShelterDragPressureScore = 1`
- `DefaultTaxSeasonFragilityShelterDragPressureFallbackScore = 0`
- `DefaultTaxSeasonFragilityMigrationRiskThreshold = 70`
- `DefaultTaxSeasonFragilityMigrationPressureScore = 1`
- `DefaultTaxSeasonFragilityMigrationPressureFallbackScore = 0`
- `DefaultTaxSeasonFragilityPressureClampFloor = 0`
- `DefaultTaxSeasonFragilityPressureClampCeiling = 7`

## Non-Goals

- No tax-season interaction/debt-delta formula extraction.
- No tax-season registration visibility, liquidity, or labor retune.
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

- Focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter TaxSeasonOpened` passed 21/21.
- Focused architecture guard: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Population_households_tax_season_fragility_pressure_extraction_v1293_v1300"` passed 1/1.
- PopulationAndHouseholds project: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 214/214.
- Presentation project: `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 40/40.
- Integration project: `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build` passed 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- Architecture project: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 184/184.
- Build: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Full solution: `dotnet test Zongzu.sln --no-build` passed.
- Diff hygiene: `git diff --check` passed.
- Replacement-character scan: touched-file scan passed clean across 17 files.
