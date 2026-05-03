# PopulationAndHouseholds Subsistence Grain Buffer Extraction V1021-V1028

## Purpose

V1021-V1028 extracts the subsistence grain-buffer threshold-to-score portion of `ComputeGrainBufferPressure` into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous grain-store scoring: `>=85` gives `-5`, `>=65` gives `-3`, `>=45` gives `-1`, `>=25` gives `2`, `>0` gives `5`, and fallback gives `6`.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files:
  - `PopulationHouseholdMobilityRulesData.cs`
  - `PopulationAndHouseholdsModule.PressureProfiles.cs`
- Extracted parameters:
  - subsistence grain buffer pressure bands
  - subsistence grain buffer pressure fallback score
- Target schema/migration impact: none.

The owning module reads the extracted grain-buffer values through validated fallback getters when computing the private subsistence pressure profile. Price pressure, market dependency, labor pressure, fragility, interaction, tax-season, and official-supply formulas remain unchanged in this pass.

## Out of scope

- No price pressure extraction.
- No market dependency extraction.
- No labor-capacity pressure extraction.
- No dependent-count pressure extraction.
- No subsistence labor clamp extraction.
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
- Application/UI/Unity do not calculate grain-buffer pressure, household pressure, or household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Save/schema impact

Target schema/migration impact: none.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, grain-buffer state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted grain-buffer bands preserve the old switch behavior and fallback score. Validation is deterministic; malformed grain-buffer config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1021-V1028 subsistence grain-buffer extraction ExecPlan.
2. Add default threshold score bands, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeGrainBufferPressure` switch with an owner-consumed deterministic lookup.
4. Add focused tests proving explicit defaults preserve previous behavior, custom owner bands are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultGrainBufferRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomGrainBufferRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidGrainBufferRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_grain_buffer_extraction_v1021_v1028_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, grain-buffer state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed: 0 warnings, 0 errors.
- Focused `Zongzu.Modules.PopulationAndHouseholds.Tests` grain-buffer extraction tests passed: 3/3.
- Focused architecture guard `Population_households_subsistence_grain_buffer_extraction_v1021_v1028_must_remain_owner_consumed_and_schema_neutral` passed: 1/1.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed: 103/103.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed: 150/150.
- `git diff --check` passed.
- Touched-file replacement-character scan passed for 17 files.
- `dotnet test Zongzu.sln --no-build` passed; integration replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
