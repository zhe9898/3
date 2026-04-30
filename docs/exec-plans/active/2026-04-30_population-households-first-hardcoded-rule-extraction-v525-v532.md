# PopulationAndHouseholds First Hardcoded Rule Extraction V525-V532

## Goal

Extract one low-risk hardcoded household-mobility-adjacent rule from `PopulationAndHouseholds` into owner-consumed rules-data shape without changing runtime behavior.

This pass is the first implementation step after the v501-v524 readiness / contract / skeleton preflight. It is still not a household migration system.

## Scope

In scope:
- Extracted rule: focused member promotion fanout cap.
- Owner: `PopulationAndHouseholds`.
- Default behavior: default remains 2 promoted regional members per pressure-hit household.
- Deterministic order: deterministic household-id then person-id order is preserved before the cap is applied.
- Validation: owner data validates the cap and an invalid cap falls back to default deterministically.
- Architecture proof: Application/UI/Unity do not consume rules data or calculate household mobility outcomes.

Out of scope:
- No household migration engine.
- No relocation command.
- No route-history model.
- No migration economy engine.
- No class/status engine.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, or cooldown ledger.
- No `PersonRegistry` expansion.
- No loader.
- No `content/rules-data`.
- No Application/UI/Unity rules-data consumption.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.

## Touched Modules

- `src/Zongzu.Modules.PopulationAndHouseholds`
- `tests/Zongzu.Modules.PopulationAndHouseholds.Tests`
- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required v525-v532 doc update set.

## Schema / Save Impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, manifest membership change, rules-data file, config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism

The previous behavior used:
- grouping by household;
- household-id ordering;
- person-id ordering within each household;
- a cap of 2.

The extracted cap keeps those same orderings and keeps `DefaultFocusedMemberPromotionCap` at `2`. The extraction does not introduce random choice, unordered traversal, IO, reflection, runtime assembly loading, or external data reads.

Malformed owner rules data does not throw inside the monthly pass. The validator reports the error and `GetFocusedMemberPromotionCapOrDefault()` falls back to the same default cap.

## Milestones

1. Add `PopulationHouseholdMobilityRulesData` with default focused-member promotion cap and deterministic validation/fallback.
2. Replace the naked `.Take(2)` in `PromoteHotHouseholdMembers` with the owner-consumed cap.
3. Add focused module tests:
   - default config produces previous behavior;
   - malformed config falls back deterministically.
4. Add architecture guard:
   - owner-only consumption;
   - no Application/UI/Unity authority drift;
   - no schema/migration drift;
   - no `PersonRegistry` expansion;
   - no runtime plugin marketplace / loader / `content/rules-data`.
5. Update required docs and evidence.

## Validation Plan

- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Population_households_first_hardcoded_rule_extraction_v525_v532_must_stay_owner_consumed_schema_neutral_and_ui_free"`
- Focused owner tests:
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
- Passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Population_households_first_hardcoded_rule_extraction_v525_v532_must_stay_owner_consumed_schema_neutral_and_ui_free"`
- Passed: `dotnet test tests/Zongzu.Modules.PopulationAndHouseholds.Tests/Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-restore --filter "RunMonth_HighPressureHousehold_DefaultMobilityRulesDataPromotesTwoRegionalPeopleInPersonIdOrder|PopulationHouseholdMobilityRulesData_InvalidFocusedMemberPromotionCapFallsBackToDefault"`
- Passed: adjacent v501-v524 household mobility architecture guards.
- Passed: `dotnet build Zongzu.sln --no-restore`
- Passed: `git diff --check`
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build`

PR validation:
- Pending: PR CI.

## Rollback Path

Remove `PopulationHouseholdMobilityRulesData`, restore the focused member promotion cap to the previous literal `2`, remove the focused tests and docs evidence, and confirm `PopulationAndHouseholds` remains schema `3`.
