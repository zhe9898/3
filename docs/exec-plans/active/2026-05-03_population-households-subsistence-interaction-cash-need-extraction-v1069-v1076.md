# PopulationAndHouseholds Subsistence Interaction Cash-Need Extraction V1069-V1076

## Purpose

V1069-V1076 extracts the cash-need interaction boost used by `ComputeSubsistenceInteractionPressure` into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous boost: grain-shortage plus cash-need livelihood contributes `+2`.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files:
  - `PopulationHouseholdMobilityRulesData.cs`
  - `PopulationAndHouseholdsModule.PressureProfiles.cs`
- Extracted parameter:
  - subsistence interaction cash-need boost score
- Target schema/migration impact: none.

The owning module reads the extracted boost through validated fallback getters when computing the private subsistence interaction pressure profile. Grain-shortage window, debt threshold boost, resilience relief, and interaction clamp remain unchanged in this pass.

## Out of scope

- No grain-shortage window extraction.
- No debt interaction threshold extraction.
- No resilience relief extraction.
- No subsistence interaction clamp extraction.
- No tax-season pressure extraction.
- No official-supply pressure extraction.
- No migration state or migration command change.
- No migration target selection change.
- No household target selection change.
- No fanout widening.
- No second household mobility runtime rule.
- No direct route-history.
- No household movement command.
- No migration economy.
- No class/status engine.
- No persisted state.
- No schema bump.
- No rules-data loader.
- No rules-data file.
- No content/config namespace.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No reflection-heavy rule loading.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate interaction cash-need pressure, household pressure, or household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Save/schema impact

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, interaction-cash-need state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted cash-need boost preserves the old `+2` contribution. Validation is deterministic; malformed cash-need config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1069-V1076 subsistence interaction cash-need extraction ExecPlan.
2. Add default cash-need boost validation and fallback getter to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeSubsistenceInteractionPressure` cash-need boost literal with an owner-consumed deterministic lookup.
4. Add focused tests proving explicit defaults preserve previous behavior, custom owner boost is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultInteractionCashNeedRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomInteractionCashNeedRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidInteractionCashNeedRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_interaction_cash_need_extraction_v1069_v1076_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, interaction-cash-need state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: focused owner tests passed: `GrainPriceSpike_DefaultInteractionCashNeedRulesDataMatchesPreviousBaseline`, `GrainPriceSpike_CustomInteractionCashNeedRulesDataIsOwnerConsumed`, and `GrainPriceSpike_InvalidInteractionCashNeedRulesDataFallsBackToPreviousBaseline` (3/3).
- 2026-05-03: focused architecture guard passed: `Population_households_subsistence_interaction_cash_need_extraction_v1069_v1076_must_remain_owner_consumed_and_schema_neutral` (1/1).
- 2026-05-03: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed (121/121).
- 2026-05-03: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed (156/156).
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; ten-year replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: touched-file replacement-character scan passed for 17 touched files.
