# PopulationAndHouseholds Subsistence Fragility Migration Extraction V1045-V1052

## Purpose

V1045-V1052 extracts the migration flag/risk contribution inside `ComputeSubsistenceFragilityPressure` into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous contribution: `IsMigrating` or `MigrationRisk >= 70` gives `1`, otherwise fallback gives `0`.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files:
  - `PopulationHouseholdMobilityRulesData.cs`
  - `PopulationAndHouseholdsModule.PressureProfiles.cs`
- Extracted parameters:
  - subsistence fragility migration risk threshold
  - subsistence fragility migration pressure score
  - subsistence fragility migration pressure fallback score
- Target schema/migration impact: none.

The owning module reads the extracted migration contribution through validated fallback getters when computing the private subsistence fragility pressure profile. This pass reads only the existing household `IsMigrating` flag and `MigrationRisk` value. Distress pressure, debt pressure, final fragility clamp, and interaction pressure remain unchanged in this pass.

## Out of scope

- No distress extraction.
- No debt extraction.
- No grain-buffer extraction.
- No price pressure extraction.
- No market dependency extraction.
- No labor-capacity pressure extraction.
- No dependent-count pressure extraction.
- No subsistence labor clamp extraction.
- No subsistence fragility clamp extraction.
- No subsistence interaction extraction.
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
- Application/UI/Unity do not calculate fragility migration pressure, household pressure, or household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Save/schema impact

Target schema/migration impact: none.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, fragility-migration state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted migration threshold/score/fallback preserve the old boolean expression. Validation is deterministic; malformed fragility-migration config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1045-V1052 subsistence fragility migration extraction ExecPlan.
2. Add default migration threshold/score/fallback validation and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputeSubsistenceFragilityPressure` migration boolean literal with an owner-consumed deterministic lookup.
4. Add focused tests proving explicit defaults preserve previous behavior, custom owner threshold is consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultFragilityMigrationRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomFragilityMigrationRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidFragilityMigrationRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_fragility_migration_extraction_v1045_v1052_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, fragility-migration state rollback, or production data rollback is required.

## Evidence log

- 2026-05-04 local build: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-04 focused owner tests: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultFragilityMigrationRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_CustomFragilityMigrationRulesDataIsOwnerConsumed|Name=GrainPriceSpike_InvalidFragilityMigrationRulesDataFallsBackToPreviousBaseline"` passed, 3/3.
- 2026-05-04 focused architecture guard: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_subsistence_fragility_migration_extraction_v1045_v1052_must_remain_owner_consumed_and_schema_neutral"` passed, 1/1.
- 2026-05-04 module regression: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed, 112/112.
- 2026-05-04 architecture regression: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed, 153/153.
- 2026-05-04 solution regression: `dotnet test Zongzu.sln --no-build` passed; replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-04 hygiene: `git diff --check` passed.
- 2026-05-04 encoding: touched-file replacement-character scan passed for 17 files.
