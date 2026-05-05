# V1325-V1332 PopulationAndHouseholds Subsistence Cash-Need Extraction

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the subsistence interaction cash-need livelihood predicate. The owner remains `PopulationAndHouseholds`, and the extracted livelihood list is consumed only through in-memory `PopulationHouseholdMobilityRulesData`.

Runtime behavior change: default behavior unchanged. Target schema/migration impact: none.

## Extracted Hardcoded Rule

Previous hardcoded subsistence interaction cash-need livelihood predicate:
- `PettyTrader`
- `Boatman`
- `Artisan`
- `SeasonalMigrant`
- `HiredLabor`

Previous subsistence interaction cash-need boost remains:
- cash-need boost score `2`

New owner-consumed default:
- `DefaultSubsistenceInteractionCashNeedLivelihoods`
- `GetSubsistenceInteractionCashNeedLivelihoodsOrDefault`
- `IsSubsistenceInteractionCashNeedLivelihoodOrDefault`

The previous shared `IsCashNeedLivelihood` helper is removed from `PopulationAndHouseholdsModule.PressureProfiles.cs`.

## Non-Goals

- No official-supply retune.
- No tax-season retune.
- No subsistence cash-need score retune.
- No subsistence interaction grain shortage, debt pressure, resilience, or clamp retune.
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

- focused architecture test proves owner-only extraction, the shared C# helper removal, docs evidence, no schema drift, and no UI/Application/Unity authority.
- focused `GrainPriceSubsistenceHandlerTests` prove default config produces previous behavior, custom livelihood list is owner-consumed, and malformed livelihood list falls back deterministically.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Evidence

- Build: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter GrainPriceSpike` passed 62/62.
- Focused architecture guard: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Population_households_subsistence_cash_need_extraction_v1325_v1332"` passed 1/1.
- PopulationAndHouseholds project: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 224/224.
- Presentation project: `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed 40/40.
- Integration project: `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build` passed 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- Architecture project: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 188/188.
- Full solution: `dotnet test Zongzu.sln --no-build` passed.
- Diff hygiene: `git diff --check` passed.
- Replacement-character scan: passed across 17 touched files.
