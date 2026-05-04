# PopulationAndHouseholds Tax Season Debt Delta Clamp Extraction V1117-V1124

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, tax-debt-delta-clamp state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one `TaxSeasonBurdenProfile.DebtDelta` clamp:

- previous hardcoded tax debt delta clamp: `Math.Clamp(..., 8, 28)`
- new owner-consumed rules-data defaults:
  - `DefaultTaxSeasonDebtDeltaClampFloor = 8`
  - `DefaultTaxSeasonDebtDeltaClampCeiling = 28`

PopulationAndHouseholds remains the sole owner and consumer. The extracted clamp bounds are validated by `PopulationHouseholdMobilityRulesData` and passed into the private tax-season pressure profile produced by the owner module.

## Explicit Non-Goals

- No tax-season debt spike threshold extraction.
- No tax registration visibility formula extraction.
- No tax liquidity formula extraction.
- No tax labor formula extraction.
- No tax fragility formula extraction.
- No tax interaction formula extraction.
- No official-supply formula extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted clamp preserves the old `8..28` tax debt delta band. Validation is deterministic; malformed clamp config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1117-V1124 tax-season debt delta clamp extraction ExecPlan.
2. Add default tax debt delta clamp validation plus fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `TaxSeasonBurdenProfile.DebtDelta` clamp literals with owner-consumed deterministic bounds.
4. Add focused tests proving explicit defaults preserve previous behavior, custom floor/ceiling are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=TaxSeasonOpened_DefaultTaxDebtDeltaClampRulesDataMatchesPreviousBaseline|Name=TaxSeasonOpened_CustomTaxDebtDeltaClampFloorRulesDataIsOwnerConsumed|Name=TaxSeasonOpened_CustomTaxDebtDeltaClampCeilingRulesDataIsOwnerConsumed|Name=TaxSeasonOpened_InvalidTaxDebtDeltaClampRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_tax_season_debt_delta_clamp_extraction_v1117_v1124_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, tax-debt-delta-clamp state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused owner tests passed: `TaxSeasonOpened_DefaultTaxDebtDeltaClampRulesDataMatchesPreviousBaseline`, `TaxSeasonOpened_CustomTaxDebtDeltaClampFloorRulesDataIsOwnerConsumed`, `TaxSeasonOpened_CustomTaxDebtDeltaClampCeilingRulesDataIsOwnerConsumed`, and `TaxSeasonOpened_InvalidTaxDebtDeltaClampRulesDataFallsBackToPreviousBaseline` passed 4/4.
- Focused architecture guard passed: `Population_households_tax_season_debt_delta_clamp_extraction_v1117_v1124_must_remain_owner_consumed_and_schema_neutral` passed 1/1.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 144/144.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 162/162.
- `dotnet test Zongzu.sln --no-build` passed. Ten-year health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
