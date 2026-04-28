# Personnel Flow Owner-Lane Gate V325-V332

Date: 2026-04-28

Baseline: `main` at `db3f056 Close personnel flow readiness layer (#24)`.

## Purpose

Add a runtime owner-lane gate readback for the first personnel-flow command-readiness layer.

The current playable surface already shows that `PopulationAndHouseholds` local-response commands can influence this household's livelihood, labor, distress, and migration pressure. This pass makes the owner-lane boundary explicit at the command-surface / Great Hall mobility level:

- current readable lane: `PopulationAndHouseholds` home-household response;
- future lanes requiring new plans: `FamilyCore` kin mediation, `OfficeAndCareer` document/service pressure, `WarfareCampaign` campaign manpower posture;
- `PersonRegistry` remains identity and `FidelityRing` only.

## Non-Goals

- No direct move, transfer, summon, assign, return, settle-person, or manpower-routing command.
- No new `PersonnelFlow`, `Migration`, or `SocialMobility` module.
- No command, movement, personnel, assignment, focus, scheduler, owner-lane-gate, or surface-echo ledger.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application, UI, or Unity movement-success calculation.
- No parsing of `DomainEvent.Summary`, `ReadbackSummary`, receipt prose, notification text, mobility text, public-life lines, or docs prose.
- No persisted state. Target schema/migration impact: none. If a persisted field becomes necessary, stop and write the schema/migration impact first.

## Owner Split

- `PopulationAndHouseholds` owns current household livelihood, labor, distress, migration pressure, activity, and local household response resolution.
- `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` are named future owner lanes only; this pass does not route commands to them.
- `PersonRegistry` owns identity and `FidelityRing` only.
- `Application` may assemble `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary` from structured affordance/receipt fields.
- `Zongzu.Presentation.Unity` may display the projected summary in Great Hall mobility readback.

## Implementation Plan

1. Add runtime-only `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary`.
2. Build it only from structured command affordance/receipt metadata and existing `PersonnelFlowReadinessSummary` presence.
3. Append it to Great Hall mobility readback through the presentation adapter.
4. Add focused integration, architecture, and presentation tests.
5. Update docs and acceptance notes.

## Readback Tokens

- `人员流动归口门槛`
- `当前可读归口为PopulationAndHouseholds本户回应`
- `FamilyCore亲族调处`
- `OfficeAndCareer文书役使`
- `WarfareCampaign军务人力`
- `另开owner-lane计划`
- `Application只汇总结构化命令字段`
- `UI/Unity只显示投影字段`
- `PersonRegistry只保身份/FidelityRing`
- `不是直接调人、迁人、召人命令`

## Validation Plan

- Focused integration test for command-surface owner-lane gate.
- Focused architecture test for v325-v332 boundary guard.
- Focused Unity presentation test for Great Hall projected display.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Completion Notes

- Implemented runtime-only `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary`.
- The gate is assembled only from structured command affordance/receipt metadata and projected personnel-flow readiness field presence.
- Great Hall mobility readback displays the projected gate without parsing prose or calculating movement outcomes.
- Schema/migration impact: none. No persisted state, save namespace, module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, owner-lane-gate ledger, durable residue, or serialized module payload was added.
- Focused validation passed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter PartialWatchResidue_ProjectsHomeHouseholdLocalResponse_AndCommandMutatesOnlyPopulation`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Personnel_flow_owner_lane_gate_v325_v332`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_CopiesMobilityAndFidelityReadbacksIntoGreatHallDeskAndLineage`
