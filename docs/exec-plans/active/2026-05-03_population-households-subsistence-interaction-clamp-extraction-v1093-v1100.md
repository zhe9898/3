# PopulationAndHouseholds Subsistence Interaction Clamp Extraction V1093-V1100

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, interaction-clamp state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for one remaining `ComputeSubsistenceInteractionPressure` clamp:

- previous hardcoded interaction clamp: `Math.Clamp(interaction, -2, 4)`
- new owner-consumed rules-data defaults:
  - `DefaultSubsistenceInteractionPressureClampFloor = -2`
  - `DefaultSubsistenceInteractionPressureClampCeiling = 4`

PopulationAndHouseholds remains the sole owner and consumer. The extracted floor and ceiling are validated by `PopulationHouseholdMobilityRulesData` and read only inside the owner module's private subsistence pressure helper.

## Explicit Non-Goals

- No grain-shortage window extraction; already closed in V1061-V1068.
- No cash-need boost extraction; already closed in V1069-V1076.
- No debt threshold boost extraction; already closed in V1077-V1084.
- No resilience relief extraction; already closed in V1085-V1092.
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

Runtime determinism risk is unchanged under default rules-data because the extracted clamp preserves the old `-2..4` pressure band. Validation is deterministic; malformed clamp config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1093-V1100 subsistence interaction clamp extraction ExecPlan.
2. Add default clamp floor/ceiling validation plus fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeSubsistenceInteractionPressure` clamp literals with owner-consumed deterministic lookups.
4. Add focused tests proving explicit defaults preserve previous behavior, custom floor/ceiling are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultInteractionClampRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomInteractionClampFloorRulesDataIsOwnerConsumed|Name=GrainPriceSpike_CustomInteractionClampCeilingRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidInteractionClampRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_interaction_clamp_extraction_v1093_v1100_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, interaction-clamp state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: focused owner tests passed: `GrainPriceSpike_DefaultInteractionClampRulesDataMatchesPreviousBaseline`, `GrainPriceSpike_CustomInteractionClampFloorRulesDataIsOwnerConsumed`, `GrainPriceSpike_CustomInteractionClampCeilingRulesDataIsOwnerConsumed`, and `GrainPriceSpike_InvalidInteractionClampRulesDataFallsBackToPreviousBaseline` (4/4).
- 2026-05-03: focused architecture guard passed: `Population_households_subsistence_interaction_clamp_extraction_v1093_v1100_must_remain_owner_consumed_and_schema_neutral` (1/1).
- 2026-05-03: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed (133/133).
- 2026-05-03: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed (159/159).
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; ten-year replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: touched-file replacement-character scan passed for 17 touched files.
