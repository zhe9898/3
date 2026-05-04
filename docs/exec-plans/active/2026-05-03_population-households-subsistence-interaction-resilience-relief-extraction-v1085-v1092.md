# PopulationAndHouseholds Subsistence Interaction Resilience Relief Extraction V1085-V1092

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, interaction-resilience state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one remaining `ComputeSubsistenceInteractionPressure` resilience relief:

- previous hardcoded resilience gate: `household.GrainStore >= 75 && household.LandHolding >= 35 && household.LaborCapacity >= 60`
- previous hardcoded resilience relief: `interaction -= 2`
- new owner-consumed rules-data defaults:
  - `DefaultSubsistenceInteractionResilienceReliefGrainStoreThreshold = 75`
  - `DefaultSubsistenceInteractionResilienceReliefLandHoldingThreshold = 35`
  - `DefaultSubsistenceInteractionResilienceReliefLaborCapacityThreshold = 60`
  - `DefaultSubsistenceInteractionResilienceReliefScore = 2`

PopulationAndHouseholds remains the sole owner and consumer. The extracted thresholds and relief score are validated by `PopulationHouseholdMobilityRulesData` and read only inside the owner module's private subsistence pressure helper.

## Explicit Non-Goals

- No grain-shortage window extraction; already closed in V1061-V1068.
- No cash-need boost extraction; already closed in V1069-V1076.
- No debt threshold boost extraction; already closed in V1077-V1084.
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

Runtime determinism risk is unchanged under default rules-data because the extracted resilience gate and relief preserve the old `75/35/60` thresholds and `-2` contribution. Validation is deterministic; malformed resilience relief config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1085-V1092 subsistence interaction resilience relief extraction ExecPlan.
2. Add default resilience thresholds and relief validation plus fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeSubsistenceInteractionPressure` resilience gate and relief literals with owner-consumed deterministic lookups.
4. Add focused tests proving explicit defaults preserve previous behavior, custom thresholds/relief are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultInteractionResilienceReliefRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomInteractionResilienceThresholdRulesDataIsOwnerConsumed|Name=GrainPriceSpike_CustomInteractionResilienceReliefScoreRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidInteractionResilienceReliefRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_interaction_resilience_relief_extraction_v1085_v1092_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, interaction-resilience state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: focused owner tests passed: `GrainPriceSpike_DefaultInteractionResilienceReliefRulesDataMatchesPreviousBaseline`, `GrainPriceSpike_CustomInteractionResilienceThresholdRulesDataIsOwnerConsumed`, `GrainPriceSpike_CustomInteractionResilienceReliefScoreRulesDataIsOwnerConsumed`, and `GrainPriceSpike_InvalidInteractionResilienceReliefRulesDataFallsBackToPreviousBaseline` (4/4).
- 2026-05-03: focused architecture guard passed: `Population_households_subsistence_interaction_resilience_relief_extraction_v1085_v1092_must_remain_owner_consumed_and_schema_neutral` (1/1).
- 2026-05-03: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed (129/129).
- 2026-05-03: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed (158/158).
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; ten-year replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: touched-file replacement-character scan passed for 17 touched files.
