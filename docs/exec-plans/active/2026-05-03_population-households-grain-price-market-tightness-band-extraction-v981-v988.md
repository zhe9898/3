# PopulationAndHouseholds Grain Price Market Tightness Band Extraction V981-V988

## Purpose

V981-V988 extracts the grain-price `marketTightness` threshold/score bands from `ComputePricePressure` into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous market-tightness band behavior: `>= 60 => 4`, `>= 40 => 3`, `>= 20 => 2`, `>= 8 => 1`, fallback `0`.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files:
  - `PopulationHouseholdMobilityRulesData.cs`
  - `PopulationAndHouseholdsModule.PressureProfiles.cs`
- Extracted parameters:
  - grain price market tightness pressure threshold/score bands
  - grain price market tightness pressure fallback score
- Target schema/migration impact: none.

The owning module reads the extracted values through validated fallback getters when computing the private grain-price `PricePressure` component. Band order is deterministic and validated as descending threshold order.

## Out of scope

- No grain price pressure clamp extraction.
- No grain price level band extraction.
- No price jump band extraction.
- No grain price signal fallback/clamp extraction.
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
- Application/UI/Unity do not calculate market tightness bands, household pressure, or household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Save/schema impact

Target schema/migration impact: none.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, market-tightness state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted bands preserve the old descending switch branch order and fallback score. Validation is deterministic; malformed market tightness band config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V981-V988 market tightness band extraction ExecPlan.
2. Add default bands, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputePricePressure` market-tightness switch with an owner-consumed deterministic band lookup.
4. Add focused tests proving explicit defaults preserve previous market-tightness behavior and malformed band config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultMarketTightnessBandRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_InvalidMarketTightnessBandRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_grain_price_market_tightness_band_extraction_v981_v988_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, market-tightness state rollback, or production data rollback is required.

## Evidence log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused module tests passed: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultMarketTightnessBandRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_InvalidMarketTightnessBandRulesDataFallsBackToPreviousBaseline"` passed 2/2.
- Focused architecture guard passed: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_grain_price_market_tightness_band_extraction_v981_v988_must_remain_owner_consumed_and_schema_neutral|Name=Population_households_grain_price_jump_band_extraction_v973_v980_must_remain_owner_consumed_and_schema_neutral"` passed 2/2.
- Full `PopulationAndHouseholds` tests passed: 91/91.
- Full architecture tests passed: 145/145.
- `git diff --check` passed.
- Touched-file replacement-character scan passed.
- `dotnet test Zongzu.sln --no-build` passed, including 137/137 integration tests and 145/145 architecture tests.
- Ten-year diagnostic replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
