# PopulationAndHouseholds Monthly Pressure Rules-Data Extraction V941-V948

## Purpose

V941-V948 extracts the first remaining `RunMonth` baseline-pressure hardcoded thresholds from `PopulationAndHouseholdsModule.cs` into owner-consumed `PopulationHouseholdMobilityRulesData`.

This is a behavior-equivalent hardcoded-rule extraction. Default rules-data preserves the previous monthly prosperity/security/clan-relief/drift behavior while allowing the owning `PopulationAndHouseholds` module to validate and consume those parameters through deterministic fallback getters.

Runtime behavior change: default behavior unchanged.

## Scope

- Owner: `PopulationAndHouseholds`.
- Touched production files: `PopulationAndHouseholdsModule.cs` and `PopulationHouseholdMobilityRulesData.cs`.
- Extracted parameters:
  - monthly prosperity distress threshold
  - monthly prosperity relief threshold
  - monthly security distress threshold
  - monthly security relief threshold
  - monthly clan-support relief threshold
  - monthly random drift min inclusive
  - monthly random drift max exclusive
- Target schema/migration impact: none.

The owning module reads the extracted values once per monthly pass through validated fallback getters. Default values preserve the previous hardcoded C# behavior: prosperity `< 50`, prosperity `>= 60`, security `< 45`, security `>= 55`, clan support `>= 60`, and drift `NextInt(-1, 2)`.

## Out of scope

- No monthly pressure behavior change under default rules-data.
- No pressure formula retune.
- No livelihood baseline extraction.
- No debt/labor/migration delta extraction.
- No warfare aftermath formula extraction.
- No pressure-profile formula extraction.
- No health lifecycle formula extraction.
- No rules-data loader.
- No rules-data file.
- No content/config namespace.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No reflection-heavy rule loading.
- No scheduler cadence change.
- No second household mobility runtime rule.
- No direct route-history.
- No household movement command.
- No migration economy.
- No class/status engine.
- No persisted state.
- No schema bump.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, monthly-pressure ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or migration-started selector state.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate monthly pressure outcomes, household mobility outcomes, or household pressure results.
- No `DomainEvent.Summary` parsing.
- No parsing of projection prose, receipt text, public-life lines, or docs text.

## Affected modules

- `src/Zongzu.Modules.PopulationAndHouseholds`
- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No Application, presentation, Unity, persistence, or `PersonRegistry` source should change.

## Save/schema impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, content/config namespace, monthly-pressure state, monthly-pressure ledger, ordering ledger, validation ledger, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism risk

Runtime determinism risk is unchanged under default rules-data because the extracted values preserve the old branch thresholds and drift bounds. Validation is deterministic; malformed monthly pressure config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V941-V948 monthly pressure rules-data extraction ExecPlan.
2. Add default constants, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace `RunMonth` hardcoded monthly prosperity/security/clan-relief/drift values with owner-consumed rules-data getters.
4. Add focused tests proving explicit defaults preserve previous behavior and malformed monthly pressure config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation plan

- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=RunMonth_DefaultMonthlyPressureRulesDataMatchesExplicitPreviousBaseline|Name=RunMonth_InvalidMonthlyPressureRulesDataFallsBackToDefault"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Population_households_monthly_pressure_rules_data_extraction_v941_v948_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, monthly-pressure state rollback, or production data rollback is required.

## Evidence log

- 2026-05-03: Added owner-consumed monthly pressure defaults, validation, and fallback getters to `PopulationHouseholdMobilityRulesData`; replaced the matching `RunMonth` hardcoded prosperity/security/clan-support/drift literals with validated rules-data reads owned by `PopulationAndHouseholds`.
- 2026-05-03: Added focused module coverage for explicit default equivalence and malformed monthly pressure rules-data fallback.
- 2026-05-03: Added architecture guard `Population_households_monthly_pressure_rules_data_extraction_v941_v948_must_remain_owner_consumed_and_schema_neutral` covering owner-only consumption, no schema drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
- 2026-05-03: `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- 2026-05-03: Focused module tests passed: `RunMonth_DefaultMonthlyPressureRulesDataMatchesExplicitPreviousBaseline` and `RunMonth_InvalidMonthlyPressureRulesDataFallsBackToDefault`.
- 2026-05-03: Focused architecture guard passed.
- 2026-05-03: Full `Zongzu.Modules.PopulationAndHouseholds.Tests` passed, 81/81.
- 2026-05-03: Full `Zongzu.Architecture.Tests` passed, 140/140.
- 2026-05-03: `git diff --check` passed.
- 2026-05-03: Touched-file replacement-character and mojibake scan passed.
- 2026-05-03: `dotnet test Zongzu.sln --no-build` passed; replay hash remained `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-03: Post-evidence focused recheck passed: architecture guard 1/1, focused module tests 2/2, `git diff --check`, and touched-file replacement/mojibake scan across 17 files.
