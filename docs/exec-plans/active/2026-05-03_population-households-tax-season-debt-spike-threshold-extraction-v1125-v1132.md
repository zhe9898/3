# PopulationAndHouseholds Tax Season Debt Spike Threshold Extraction V1125-V1132

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, tax-debt-spike-threshold state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one `ApplyTaxSeasonPressure` event crossing threshold:

- previous hardcoded tax debt spike threshold: `oldDebt < 70 && household.DebtPressure >= 70`
- new owner-consumed rules-data default:
  - `DefaultTaxSeasonDebtSpikeEventThreshold = 70`

PopulationAndHouseholds remains the sole owner and consumer. The extracted threshold is validated by `PopulationHouseholdMobilityRulesData` and used only by the owner module's private tax-season event emission gate.

## Explicit Non-Goals

- No tax-season debt delta clamp extraction; already closed in V1117-V1124.
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

Runtime determinism risk is unchanged under default rules-data because the extracted event gate preserves the old `70` debt crossing threshold. Validation is deterministic; malformed threshold config falls back through the owner getter to the same default.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1125-V1132 tax-season debt spike threshold extraction ExecPlan.
2. Add default debt spike threshold validation plus fallback getter to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ApplyTaxSeasonPressure` threshold literal with owner-consumed deterministic threshold.
4. Add focused tests proving explicit defaults preserve previous behavior, custom threshold is consumed without changing debt mutation, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=TaxSeasonOpened_DefaultDebtSpikeEventThresholdRulesDataMatchesPreviousBaseline|Name=TaxSeasonOpened_CustomDebtSpikeEventThresholdRulesDataIsOwnerConsumedWithoutChangingDebt|Name=TaxSeasonOpened_InvalidDebtSpikeEventThresholdRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_tax_season_debt_spike_threshold_extraction_v1125_v1132_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, tax-debt-spike-threshold state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused owner tests passed: `TaxSeasonOpened_DefaultDebtSpikeEventThresholdRulesDataMatchesPreviousBaseline`, `TaxSeasonOpened_CustomDebtSpikeEventThresholdRulesDataIsOwnerConsumedWithoutChangingDebt`, and `TaxSeasonOpened_InvalidDebtSpikeEventThresholdRulesDataFallsBackToPreviousBaseline` passed 3/3.
- Focused architecture guard passed: `Population_households_tax_season_debt_spike_threshold_extraction_v1125_v1132_must_remain_owner_consumed_and_schema_neutral` passed 1/1.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed 147/147.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed 163/163.
- `dotnet test Zongzu.sln --no-build` passed. Ten-year health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
