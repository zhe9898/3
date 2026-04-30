# PopulationAndHouseholds First Mobility Runtime Rule V533-V540

## Goal

Land the first household mobility runtime rule in the owner lane while keeping the behavior intentionally small, deterministic, and schema-neutral.

This is the first household mobility runtime rule, not a migration engine. It nudges existing household mobility pressure signals so future movement work has a safe engineering track.

## Scope

In scope:
- Owner: `PopulationAndHouseholds`.
- Cadence: monthly.
- Existing state is sufficient for this slice.
- deterministic target selection from existing `MigrationPools` and household livelihood/distress/debt/labor/grain/land/migration pressure fields.
- bounded fanout through owner-consumed rules data:
  - active pool outflow threshold default 60;
  - monthly settlement cap default 1;
  - monthly household cap default 2;
  - monthly migration-risk delta default 1.
- near detail far summary: only the highest-pressure active local pool can select capped household candidates; quiet households, lower-priority active pools, and distant summaries remain untouched.
- Output uses existing owner-owned structured state and events: `MigrationRisk`, `IsMigrating`, `MigrationPools`, and existing `MigrationStarted` if a household crosses the existing threshold.

Out of scope:
- No direct route-history.
- No household movement command.
- No relocation command.
- No migration economy.
- No class/status engine.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, or cooldown ledger.
- No `PersonRegistry` expansion.
- No Application/UI/Unity outcome calculation.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No runtime plugin marketplace, arbitrary script rules, runtime assemblies, or reflection-heavy rule loading.

## Touched Modules

- `src/Zongzu.Modules.PopulationAndHouseholds`
- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required v533-v540 doc update set.

## Schema / Save Impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism

The rule is deterministic:
- active pools are ordered by outflow pressure descending, then settlement id;
- household candidates are scored from existing numeric state, ordered by score descending, then household id;
- cap values are validated and fall back to defaults when malformed;
- no random choice, unordered traversal, IO, prose parsing, reflection, runtime assembly loading, or external data reads are introduced.

The rule is bounded fanout:
- one settlement pool by default;
- two household candidates by default;
- one migration-risk point per selected household by default.

## Milestones

1. Extend `PopulationHouseholdMobilityRulesData` with monthly runtime rule caps, threshold, and delta.
2. Add owner-only monthly rule in `PopulationAndHouseholds` after the existing monthly pool rebuild.
3. Add focused tests:
   - first runtime rule touches only eligible in-scope household/pool/settlement;
   - quiet/off-scope/distant summary no-touch;
   - deterministic ordering/cap;
   - same seed replay stable;
   - malformed config falls back deterministically.
4. Add architecture guard:
   - no schema drift;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift;
   - no runtime plugin marketplace;
   - no movement, route-history, migration economy, or class/status engine.
5. Update required docs and evidence.

## Validation Plan

- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Population_households_first_mobility_runtime_rule_v533_v540_must_stay_population_owned_bounded_schema_neutral_and_ui_free"`
- Focused owner tests:
  - `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleTouchesOnlyCappedEligibleHouseholdsInActivePool|RunMonth_FirstMobilityRuntimeRuleReplaySameSeedStable|PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCapFallsBackToDefault"`
- Adjacent household mobility owner tests:
  - `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_HighPressureHousehold_DefaultMobilityRulesDataPromotesTwoRegionalPeopleInPersonIdOrder|PopulationHouseholdMobilityRulesData_InvalidFocusedMemberPromotionCapFallsBackToDefault"`
- Build:
  - `dotnet build Zongzu.sln --no-restore`
- Diff hygiene:
  - `git diff --check`
- Encoding:
  - touched-file replacement-character scan
- Full no-build solution test:
  - `dotnet test Zongzu.sln --no-build`

## Evidence Log

Local validation:
- Passed: `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_FirstMobilityRuntimeRuleTouchesOnlyCappedEligibleHouseholdsInActivePool|RunMonth_FirstMobilityRuntimeRuleReplaySameSeedStable|PopulationHouseholdMobilityRulesData_InvalidMonthlyRuntimeCapFallsBackToDefault"`
- Passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Population_households_first_mobility_runtime_rule_v533_v540_must_stay_population_owned_bounded_schema_neutral_and_ui_free"`
- Passed: `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_HighPressureHousehold_DefaultMobilityRulesDataPromotesTwoRegionalPeopleInPersonIdOrder|PopulationHouseholdMobilityRulesData_InvalidFocusedMemberPromotionCapFallsBackToDefault"`
- Passed: adjacent v501-v532 household mobility architecture guards.
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build`.

PR validation:
- Pending: PR CI.

## Rollback Path

Remove the monthly runtime rule helper and its rules-data fields, restore `PopulationAndHouseholdsModule` to the v525-v532 focused-member cap extraction shape, remove the focused tests and docs evidence, and confirm `PopulationAndHouseholds` remains schema `3`.
