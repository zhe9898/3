# V1269-V1276 PopulationAndHouseholds Tax Season Registration Visibility Extraction

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the tax-season registration visibility pressure helper. The owner remains `PopulationAndHouseholds`, and the extracted rules are consumed only through in-memory `PopulationHouseholdMobilityRulesData`.

Runtime behavior change: default behavior unchanged. Target schema/migration impact: none.

## Extracted Hardcoded Rule

Previous hardcoded tax-season registration visibility livelihood exposure scores:
- `Tenant => 4`
- `Boatman => 3`
- `PettyTrader => 3`
- `Smallholder => 3`
- `Artisan => 2`
- `HiredLabor => 2`
- `SeasonalMigrant => 2`
- `Unknown => 2`
- `DomesticServant => 1`
- `YamenRunner => 1`
- `Vagrant => 1`
- fallback score `2`

Previous hardcoded tax-season registration visibility land visibility bands:
- `>= 80 => 4`
- `>= 40 => 3`
- `>= 15 => 2`
- `> 0 => 1`
- fallback score `0`

Previous hardcoded tax-season registration visibility clamp: `Math.Clamp(livelihoodExposure + landVisibility, 1, 7)`.

New owner-consumed defaults:
- `DefaultTaxSeasonRegistrationVisibilityLivelihoodExposureScoreWeights`
- `DefaultTaxSeasonRegistrationVisibilityLivelihoodExposureFallbackScore = 2`
- `DefaultTaxSeasonRegistrationVisibilityLandVisibilityScoreBands`
- `DefaultTaxSeasonRegistrationVisibilityLandVisibilityFallbackScore = 0`
- `DefaultTaxSeasonRegistrationVisibilityClampFloor = 1`
- `DefaultTaxSeasonRegistrationVisibilityClampCeiling = 7`

## Non-Goals

- No tax-season liquidity/labor/fragility/interaction/debt-delta formula extraction.
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
- Focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter TaxSeasonOpened` passed 12/12.
- Focused architecture tests: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Population_households_tax_season_registration_visibility_extraction_v1269_v1276"` passed 1/1.
- Focused project tests: PopulationAndHouseholds 205/205, Presentation.Unity 40/40, Integration 137/137 with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`, Architecture 181/181.
- Full tests: `dotnet test Zongzu.sln --no-build` passed.
- Diff check: `git diff --check` passed.
- Replacement-character scan: passed for 17 touched/untracked files.
