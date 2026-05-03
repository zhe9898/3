# PopulationAndHouseholds Grain Price Pressure Clamp Extraction V957-V964

## Purpose

V957-V964 extracts the grain-price price-pressure clamp bounds from `ComputePricePressure` into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous price-pressure clamp behavior: `Math.Clamp(priceLevel + priceJump + marketTightness, 4, 14)`.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files:
  - `PopulationHouseholdMobilityRulesData.cs`
  - `PopulationAndHouseholdsModule.PressureProfiles.cs`
- Extracted parameters:
  - grain price pressure clamp floor
  - grain price pressure clamp ceiling
- Target schema/migration impact: none.

The owning module reads the extracted values through validated fallback getters when computing the private grain-price `PricePressure` component.

## Out of scope

- No grain price band threshold extraction.
- No grain price band score extraction.
- No price jump band extraction.
- No market tightness band extraction.
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
- Application/UI/Unity do not calculate grain price pressure, household pressure, or household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Save/schema impact

Target schema/migration impact: none.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, grain-price-pressure state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted values preserve the old price-pressure clamp bounds. Validation is deterministic; malformed grain price pressure clamp config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V957-V964 grain price pressure clamp extraction ExecPlan.
2. Add default constants, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ComputePricePressure` final clamp bounds with owner-consumed rules-data getters.
4. Add focused tests proving explicit defaults preserve previous price-pressure behavior and malformed clamp config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultPricePressureClampRulesDataMatchesPreviousBaseline|Name=GrainPriceSpike_InvalidPricePressureClampRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_grain_price_pressure_clamp_extraction_v957_v964_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, grain-price-pressure state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: Added owner-consumed grain-price price-pressure clamp defaults, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
- 2026-05-03: Replaced the `ComputePricePressure` final `4..14` clamp literal with validated owner rules-data reads; price level, price jump, and market tightness bands remain unchanged.
- 2026-05-03: Added focused grain-price handler coverage for explicit previous clamp baseline equivalence and malformed clamp config deterministic fallback.
- 2026-05-03: Added architecture guard `Population_households_grain_price_pressure_clamp_extraction_v957_v964_must_remain_owner_consumed_and_schema_neutral`; updated earlier file-split/extraction guards to recognize the owner-consumed getter path.
- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: Focused module tests passed: `GrainPriceSpike_DefaultPricePressureClampRulesDataMatchesPreviousBaseline` and `GrainPriceSpike_InvalidPricePressureClampRulesDataFallsBackToPreviousBaseline`.
- 2026-05-03: Focused architecture guards passed: V957-V964 grain price pressure clamp extraction, V949-V956 grain price signal extraction, and V893-V900 pressure profile file split.
- 2026-05-03: Full `Zongzu.Modules.PopulationAndHouseholds.Tests` passed, 85/85.
- 2026-05-03: Full `Zongzu.Architecture.Tests` passed, 142/142.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: Touched-file replacement-character and mojibake scan passed across 17 files.
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
