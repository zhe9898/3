# PopulationAndHouseholds Subsistence Market Dependency Extraction V989-V996

## Purpose

V989-V996 extracts the subsistence `ComputeMarketDependencyPressure` livelihood-to-score mapping into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous livelihood scoring: petty trader and boatman `4`; artisan, hired labor, and seasonal migrant `3`; domestic servant, yamen runner, vagrant, tenant, and unknown `2`; smallholder `1`; fallback `2`.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files:
  - `PopulationHouseholdMobilityRulesData.cs`
  - `PopulationAndHouseholdsModule.PressureProfiles.cs`
- Extracted parameters:
  - subsistence market dependency livelihood score weights
  - subsistence market dependency fallback score
- Target schema/migration impact: none.

The owning module reads the extracted values through validated fallback getters when computing the private subsistence pressure profile.

## Out of scope

- No grain-price band extraction.
- No grain-buffer band extraction.
- No subsistence labor extraction.
- No subsistence fragility extraction.
- No subsistence interaction extraction.
- No tax-season pressure extraction.
- No official-supply pressure extraction.
- No subsistence pressure profile retune.
- No event metadata shape change.
- No emitted receipt/projection text rewrite.
- No scheduler cadence change.
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
- Application/UI/Unity do not calculate market dependency, household pressure, or household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Save/schema impact

Target schema/migration impact: none.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, market-dependency state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted livelihood score weights preserve the old switch behavior and fallback score. Validation is deterministic; malformed market dependency config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V989-V996 subsistence market dependency extraction ExecPlan.
2. Add default livelihood score weights, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace `ComputeMarketDependencyPressure` switch with an owner-consumed deterministic lookup.
4. Add focused tests proving explicit defaults preserve previous behavior and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultMarketDependencyRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_InvalidMarketDependencyRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_market_dependency_extraction_v989_v996_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, market-dependency state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: focused PopulationAndHouseholds market-dependency extraction tests passed, 2/2.
- 2026-05-03: focused architecture guard `Population_households_subsistence_market_dependency_extraction_v989_v996_must_remain_owner_consumed_and_schema_neutral` passed, 1/1.
- 2026-05-03: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed, 93/93.
- 2026-05-03: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed, 146/146.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: touched-file replacement-character scan passed for 17 files.
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; integration health replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- Schema/migration impact: none; `PopulationAndHouseholds` remains schema `3`.
