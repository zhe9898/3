# PopulationAndHouseholds Official Supply Signal Normalization Clamp Extraction V1181-V1188

## Purpose

This ExecPlan covers a behavior-equivalent hardcoded-rule extraction for official-supply signal normalization clamps in `PopulationAndHouseholds`.

Runtime behavior change: default behavior unchanged.

Target schema/migration impact: none. `PopulationAndHouseholds` remains schema `3`.

No persisted field, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, official-supply-signal-normalization state, pressure-profile ledger, ordering ledger, validation ledger, diagnostic state, performance cache, rules-data file, loader, content/config namespace, or serialized projection cache is added.

## Scope

This is a behavior-equivalent hardcoded-rule extraction for the `ResolveOfficialSupplySignal` final normalization clamps:

- previous hardcoded official-supply signal normalization clamps: `frontier=0..100`, `supply=0..30`, `quota=0..20`, `docket=0..20`, `clerk=0..15`, `authority=0..12`
- new owner-consumed rules-data defaults:
  - `DefaultOfficialSupplyFrontierPressureClampFloor = 0`
  - `DefaultOfficialSupplyFrontierPressureClampCeiling = 100`
  - `DefaultOfficialSupplyPressureClampFloor = 0`
  - `DefaultOfficialSupplyPressureClampCeiling = 30`
  - `DefaultOfficialSupplyQuotaPressureClampFloor = 0`
  - `DefaultOfficialSupplyQuotaPressureClampCeiling = 20`
  - `DefaultOfficialSupplyDocketPressureClampFloor = 0`
  - `DefaultOfficialSupplyDocketPressureClampCeiling = 20`
  - `DefaultOfficialSupplyClerkDistortionPressureClampFloor = 0`
  - `DefaultOfficialSupplyClerkDistortionPressureClampCeiling = 15`
  - `DefaultOfficialSupplyAuthorityBufferClampFloor = 0`
  - `DefaultOfficialSupplyAuthorityBufferClampCeiling = 12`

PopulationAndHouseholds remains the sole owner and consumer. The extracted normalization clamps are validated by `PopulationHouseholdMobilityRulesData` and read only by the owner module when normalizing structured official-supply signal values.

## Explicit Non-Goals

- No official-supply fallback value retune.
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

Runtime determinism risk is unchanged under default rules-data because the extracted official-supply signal normalization clamps preserve the old bounds. Validation is deterministic; malformed clamp config falls back through owner getters to the same defaults.

No unordered authority traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, caches, persisted split state, or plugin loading are introduced.

## Milestones

1. Add V1181-V1188 official-supply signal normalization clamp extraction ExecPlan.
2. Add default official-supply signal normalization clamp validation plus fallback getters to `PopulationHouseholdMobilityRulesData`.
3. Replace the `ResolveOfficialSupplySignal` normalization clamp literals with owner-consumed deterministic values.
4. Add focused tests proving explicit defaults preserve previous behavior, custom clamp floors/ceilings are consumed, and malformed config falls back deterministically.
5. Add architecture guard proving no schema drift, no authority drift, no loader/plugin drift, no prose parsing, no `PersonRegistry` expansion, and no Application/UI/Unity authority.
6. Update required docs.
7. Run focused module/architecture tests, full architecture test, build, diff check, encoding scan, and full no-build test.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build --filter "Name=OfficialSupplyRequisition_DefaultSignalNormalizationClampRulesDataMatchesPreviousBaseline|Name=OfficialSupplyRequisition_CustomSignalNormalizationClampRulesDataIsOwnerConsumed|Name=OfficialSupplyRequisition_InvalidSignalNormalizationClampRulesDataFallsBackToPreviousBaseline"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Population_households_official_supply_signal_normalization_clamp_extraction_v1181_v1188_must_remain_owner_consumed_and_schema_neutral"`
- `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build`
- `git diff --check`
- touched-file replacement-character scan
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the code/docs/tests commit. No save migration, content migration, rules-data file rollback, official-supply-signal-normalization state rollback, or production data rollback is required.

## Evidence log

- 2026-05-04: `dotnet build Zongzu.sln --no-restore` passed.
- 2026-05-04: focused owner tests passed: `OfficialSupplyRequisition_DefaultSignalNormalizationClampRulesDataMatchesPreviousBaseline`, `OfficialSupplyRequisition_CustomSignalNormalizationClampRulesDataIsOwnerConsumed`, and `OfficialSupplyRequisition_InvalidSignalNormalizationClampRulesDataFallsBackToPreviousBaseline`.
- 2026-05-04: focused architecture guard passed: `Population_households_official_supply_signal_normalization_clamp_extraction_v1181_v1188_must_remain_owner_consumed_and_schema_neutral`.
- 2026-05-04: `dotnet test tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj --no-build` passed, 172/172.
- 2026-05-04: `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build` passed, 40/40.
- 2026-05-04: ten-year integration health check passed, 6/6, replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`.
- 2026-05-04: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --logger "console;verbosity=minimal"` passed, 170/170.
- 2026-05-04: `git diff --check` passed.
- 2026-05-04: touched-file replacement-character scan passed.
- 2026-05-04: `dotnet test Zongzu.sln --no-build` passed.
