# V1309-V1316 PopulationAndHouseholds Tax Season Debt Delta Formula Extraction

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the tax-season debt-delta formula. The owner remains `PopulationAndHouseholds`, and the extracted formula parameters are consumed only through in-memory `PopulationHouseholdMobilityRulesData`.

Runtime behavior change: default behavior unchanged. Target schema/migration impact: none.

## Extracted Hardcoded Rule

Previous hardcoded tax-season debt-delta formula:
- base score `14`
- visibility pressure weight `1`
- liquidity pressure weight `1`
- labor pressure weight `1`
- fragility pressure weight `1`
- interaction pressure weight `1`
- existing clamp remains `TaxSeasonDebtDeltaClampFloor..TaxSeasonDebtDeltaClampCeiling`

Previous expression:

```csharp
Math.Clamp(14 + VisibilityPressure + LiquidityPressure + LaborPressure + FragilityPressure + InteractionPressure, ...)
```

New owner-consumed defaults:
- `DefaultTaxSeasonDebtDeltaBaseScore = 14`
- `DefaultTaxSeasonDebtDeltaVisibilityPressureWeight = 1`
- `DefaultTaxSeasonDebtDeltaLiquidityPressureWeight = 1`
- `DefaultTaxSeasonDebtDeltaLaborPressureWeight = 1`
- `DefaultTaxSeasonDebtDeltaFragilityPressureWeight = 1`
- `DefaultTaxSeasonDebtDeltaInteractionPressureWeight = 1`

## Non-Goals

- No tax-season clamp/event-threshold retune.
- No tax-season registration visibility, liquidity, labor, fragility, or interaction retune.
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
- Focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter TaxSeasonOpened` passed 27/27.
- Focused architecture guard: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Population_households_tax_season_debt_delta_formula_extraction_v1309_v1316"` passed 1/1.
- PopulationAndHouseholds project: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 220/220.
- Presentation project: `dotnet test tests\Zongzu.Presentation.Tests\Zongzu.Presentation.Tests.csproj --no-build` passed 40/40.
- Integration project: `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build` passed 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- Architecture project: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 186/186.
- Full solution: `dotnet test Zongzu.sln --no-build` passed.
- Diff hygiene: `git diff --check` passed.
- Replacement-character scan: passed across 17 touched files.
