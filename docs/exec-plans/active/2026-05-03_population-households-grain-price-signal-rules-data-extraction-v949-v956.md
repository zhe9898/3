# PopulationAndHouseholds Grain Price Signal Rules-Data Extraction V949-V956

## Purpose

V949-V956 extracts the grain-price shock signal fallback and clamp constants from `PopulationAndHouseholdsModule.PressureProfiles.cs` into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous grain shock signal behavior: current price fallback `130`, old price fallback `100`, supply fallback `50`, demand fallback `70`, current price clamp `50..200`, price delta clamp `0..150`, supply clamp `0..100`, and demand clamp `0..100`.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files:
  - `PopulationHouseholdMobilityRulesData.cs`
  - `PopulationAndHouseholdsModule.PressureProfiles.cs`
  - `PopulationAndHouseholdsModule.EventDispatch.cs`
- Extracted parameters:
  - grain price shock default current price
  - grain price shock default old price
  - grain price shock default supply
  - grain price shock default demand
  - grain price shock current price clamp floor/ceiling
  - grain price shock price delta clamp floor/ceiling
  - grain price shock supply clamp floor/ceiling
  - grain price shock demand clamp floor/ceiling
- Target schema/migration impact: none.

The owning module reads the extracted values through validated fallback getters when resolving `GrainPriceSpike` metadata into a private `GrainPriceShockSignal`.

## Out of scope

- No grain price pressure formula retune.
- No subsistence pressure profile retune.
- No tax-season formula extraction.
- No official-supply formula extraction.
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
- Application/UI/Unity do not calculate grain shock, household pressure, or household mobility outcomes.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Save/schema impact

Target schema/migration impact: none.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, grain-shock state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted values preserve the old signal fallback and clamp values. Validation is deterministic; malformed grain shock config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V949-V956 grain price signal rules-data extraction ExecPlan.
2. Add default constants, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace grain shock metadata fallback/clamp literals in `ResolveGrainPriceShockSignal` with owner-consumed rules-data getters.
4. Keep the grain-price event dispatch path private to `PopulationAndHouseholds` and instance-owned where it reads rules-data.
5. Add focused tests proving explicit defaults preserve previous missing-metadata fallback behavior and malformed grain shock config falls back deterministically.
6. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
7. Update required docs.
8. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=GrainPriceSpike_DefaultSignalRulesDataMatchesExplicitPreviousFallbackBaseline|Name=GrainPriceSpike_InvalidSignalRulesDataFallsBackToPreviousFallbackBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_grain_price_signal_rules_data_extraction_v949_v956_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, grain-shock state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: Added owner-consumed grain-price shock fallback/clamp defaults, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
- 2026-05-03: Replaced `ResolveGrainPriceShockSignal` metadata fallback/clamp literals with validated owner rules-data reads; only the grain-price event path became instance-owned to read rules-data.
- 2026-05-03: Added focused grain-price handler coverage for explicit previous fallback baseline equivalence and malformed grain shock config deterministic fallback.
- 2026-05-03: Added architecture guard `Population_households_grain_price_signal_rules_data_extraction_v949_v956_must_remain_owner_consumed_and_schema_neutral`; updated earlier file-split guards to recognize the owner-consumed getter path.
- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: Focused module tests passed: `GrainPriceSpike_DefaultSignalRulesDataMatchesExplicitPreviousFallbackBaseline` and `GrainPriceSpike_InvalidSignalRulesDataFallsBackToPreviousFallbackBaseline`.
- 2026-05-03: Focused architecture guards passed: V949-V956 grain price signal extraction and V893-V900 pressure profile file split.
- 2026-05-03: Full `Zongzu.Modules.PopulationAndHouseholds.Tests` passed, 83/83.
- 2026-05-03: Full `Zongzu.Architecture.Tests` passed, 141/141.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: Touched-file replacement-character and mojibake scan passed across 18 files.
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
