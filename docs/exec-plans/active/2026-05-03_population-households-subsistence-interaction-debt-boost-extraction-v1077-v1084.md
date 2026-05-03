# PopulationAndHouseholds Subsistence Interaction Debt-Boost Extraction V1077-V1084

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, interaction-debt state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one remaining `ComputeSubsistenceInteractionPressure` debt interaction:

- previous hardcoded debt gate: `isGrainShortage && household.DebtPressure >= 60`
- previous hardcoded debt boost: `interaction += 1`
- new owner-consumed rules-data defaults:
  - `DefaultSubsistenceInteractionDebtPressureThreshold = 60`
  - `DefaultSubsistenceInteractionDebtPressureBoostScore = 1`

PopulationAndHouseholds remains the sole owner and consumer. The extracted threshold and boost are validated by `PopulationHouseholdMobilityRulesData` and read only inside the owner module's private subsistence pressure helper.

## Explicit Non-Goals

- No grain-shortage window extraction; already closed in V1061-V1068.
- No cash-need boost extraction; already closed in V1069-V1076.
- No resilience relief extraction.
- No subsistence interaction clamp extraction.
- No tax-season or official-supply formula extraction.
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

Runtime determinism risk is unchanged under default rules-data because the extracted debt threshold and boost preserve the old `>= 60` and `+1` contribution. Validation is deterministic; malformed debt interaction config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1077-V1084 subsistence interaction debt-boost extraction ExecPlan.
2. Add default debt threshold and boost validation plus fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeSubsistenceInteractionPressure` debt threshold and boost literals with owner-consumed deterministic lookups.
4. Add focused tests proving explicit defaults preserve previous behavior, custom threshold/boost are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultInteractionDebtPressureRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomInteractionDebtPressureThresholdRulesDataIsOwnerConsumed|Name=GrainPriceSpike_CustomInteractionDebtPressureBoostRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidInteractionDebtPressureRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_interaction_debt_boost_extraction_v1077_v1084_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, interaction-debt state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: focused owner tests passed: `GrainPriceSpike_DefaultInteractionDebtPressureRulesDataMatchesPreviousBaseline`, `GrainPriceSpike_CustomInteractionDebtPressureThresholdRulesDataIsOwnerConsumed`, `GrainPriceSpike_CustomInteractionDebtPressureBoostRulesDataIsOwnerConsumed`, and `GrainPriceSpike_InvalidInteractionDebtPressureRulesDataFallsBackToPreviousBaseline` (4/4).
- 2026-05-03: focused architecture guard passed: `Population_households_subsistence_interaction_debt_boost_extraction_v1077_v1084_must_remain_owner_consumed_and_schema_neutral` (1/1).
- 2026-05-03: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed (125/125).
- 2026-05-03: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed (157/157).
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; ten-year replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: touched-file replacement-character scan passed for 17 touched files.
