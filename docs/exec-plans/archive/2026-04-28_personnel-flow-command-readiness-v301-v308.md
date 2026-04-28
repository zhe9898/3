# Personnel Flow Command Readiness V301-V308

Date: 2026-04-28

Baseline: `main` at `37d86b3 Align skill pack with v300 mainline (#21)`.

## Purpose

Add the first player-facing readiness readback for personnel-flow influence without adding a direct personnel command. This is a projection/read-model layer over existing `PopulationAndHouseholds` home-household local response commands.

This pass follows the v293-v300 preflight: personnel movement remains bounded leverage, not player-as-god control. The current playable command surfaces may explain that the player can influence migration risk, labor pressure, road-message clarity, and household willingness only through existing owner-lane commands such as `RestrictNightTravel`, `PoolRunnerCompensation`, and `SendHouseholdRoadMessage`.

## Non-Goals

- No direct move, transfer, summon, assign, return, or settle-person command.
- No new `PersonnelFlow`, `Migration`, or `SocialMobility` module.
- No command/movement/personnel/assignment/focus/scheduler ledger.
- No `PersonRegistry` domain expansion beyond existing identity and `FidelityRing`.
- No Application, UI, or Unity movement-success calculation.
- No prose parsing from `DomainEvent.Summary`, receipt text, readback text, `LastLocalResponseSummary`, or public-life lines.
- No new persisted state. Target schema/migration impact: none. If a persisted field becomes necessary, stop and write the schema/migration impact first.

## Owner Split

- `PopulationAndHouseholds` owns household livelihood, labor capacity, debt/distress, migration risk, activity, and local household response resolution.
- `PersonRegistry` owns identity and existing `FidelityRing` only.
- `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` remain future owner lanes for kin, office-service, and campaign-manpower requests.
- `Application` assembles runtime projections and does not choose people, rank movement targets, or resolve success.
- Unity copies projected command fields only.

## Implementation Plan

1. Add runtime-only personnel-flow readiness readback fields to player-command affordance and receipt read models.
2. Populate those fields only for existing `PopulationAndHouseholds` local response commands.
3. Copy the fields through the Unity command ViewModels/adapters.
4. Add focused integration assertions for projected affordances and receipts.
5. Add architecture guards for no direct personnel command drift, no schema drift, no prose parsing, and no `PersonRegistry` expansion.
6. Update docs and acceptance notes.

## Readback Tokens

- `人员流动预备读回`
- `近处细读`
- `远处汇总`
- `只影响本户生计/丁力/迁徙之念`
- `不是直接调人、迁人、召人命令`
- `PopulationAndHouseholds拥有本户回应`
- `PersonRegistry只保身份/FidelityRing`
- `UI/Unity只复制投影字段`

## Validation Plan

- Focused integration test for local response affordance / receipt readiness readback.
- Focused architecture test for v301-v308 boundary guard.
- Focused Unity presentation test for copy-only ViewModel propagation.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter PersonnelFlow`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter v301`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore --filter PersonnelFlow`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Completion Notes

- Implemented runtime-only `PersonnelFlowReadinessSummary` on player-command affordance and receipt read models, plus Unity ViewModel copy-through.
- Populated only existing `PopulationAndHouseholds` home-household local response commands; no direct move, transfer, summon, assign, return, or settle-person command was added.
- Schema/migration impact: none. No persisted state, save namespace, module schema version, migration, or ledger was added.
- Focused validation passed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter PartialWatchResidue_ProjectsHomeHouseholdLocalResponse_AndCommandMutatesOnlyPopulation`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Personnel_flow_command_readiness_v301_v308`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_ProjectsHomeHouseholdLocalResponsePublicLifeFieldsWithoutShellAuthority`
