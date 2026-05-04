# PopulationAndHouseholds Official Supply Signal Fallback Clamp Extraction V1173-V1180

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for official-supply signal metadata fallback values and the derived supply-pressure fallback clamp in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-signal-fallback state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the `ResolveOfficialSupplySignal` fallback path:

- previous hardcoded official-supply signal fallback values: `frontier=60`, `quota=7`, `docket=1`, `clerk=0`, `authority=4`, `derived clamp=4..26`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyFallbackFrontierPressure = 60`
  - `DefaultOfficialSupplyFallbackQuotaPressure = 7`
  - `DefaultOfficialSupplyFallbackDocketPressure = 1`
  - `DefaultOfficialSupplyFallbackClerkDistortionPressure = 0`
  - `DefaultOfficialSupplyFallbackAuthorityBuffer = 4`
  - `DefaultOfficialSupplyFallbackDerivedPressureClampFloor = 4`
  - `DefaultOfficialSupplyFallbackDerivedPressureClampCeiling = 26`

PopulationAndHouseholds remains the sole owner and consumer. The extracted fallback values are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when structured `OfficeAndCareer` metadata is absent or partial.

## Explicit Non-Goals

- No official-supply normalization clamp extraction.
- No official-supply distress delta retune.
- No official-supply debt delta retune.
- No official-supply labor drop retune.
- No official-supply migration delta retune.
- No official-supply event threshold retune.
- No official-supply livelihood/resource/labor/liquidity/fragility/interaction formula extraction.
- No official-supply formula divisor extraction.
- No tax-season formula extraction.
- No runtime rules-data loader.
- No rules-data loader.
- No rules-data file.
- No content/config namespace.
- No runtime plugin marketplace.
- No arbitrary script rules.
- No runtime assemblies.
- No reflection-heavy rule loading.
- No household movement command.
- No direct route-history.
- No migration economy.
- No class/status engine.
- No persisted state.
- No schema bump.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, or pressure-profile ledger.
- No `PersonRegistry` expansion.
- No Application/UI/Unity authority.
- No prose parsing from `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.

## Determinism Risk

Runtime determinism risk is unchanged under default rules-data because the extracted official-supply signal fallbacks preserve the old values and derived fallback clamp. Validation is deterministic; malformed fallback config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1173-V1180 official-supply signal fallback clamp extraction ExecPlan.
2. Add default official-supply signal fallback validation plus fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ResolveOfficialSupplySignal` fallback literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom fallbacks are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultSignalFallbackRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomSignalFallbackRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidSignalFallbackRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_signal_fallback_clamp_extraction_v1173_v1180_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-signal-fallback state rollback, or production data rollback is required.

## Evidence log

- Focused owner tests passed:
  - `OfficialSupplyRequisition_DefaultSignalFallbackRulesDataMatchesPreviousBaseline`
  - `OfficialSupplyRequisition_CustomSignalFallbackRulesDataIsOwnerConsumed`
  - `OfficialSupplyRequisition_InvalidSignalFallbackRulesDataFallsBackToPreviousBaseline`
- Focused architecture guard passed: `Population_households_official_supply_signal_fallback_clamp_extraction_v1173_v1180_must_remain_owner_consumed_and_schema_neutral`.
- Existing pressure-profile file-split guard was updated and passed: `Population_households_pressure_profile_file_split_v893_v900_must_preserve_owner_behavior_and_schema_neutrality`.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed: 169/169.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build` passed: 169/169.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed: 40/40.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "Name=CampaignEnabledStressSandbox_TenYearHealthReport"` passed with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `dotnet test Zongzu.sln --no-build` passed, including architecture 169/169 and integration replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- `git diff --check` passed.
- Touched-file replacement-character scan passed.
