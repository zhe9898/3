# Personnel Flow Desk Gate Echo V333-V340

Date: 2026-04-28

Baseline: `main` at `950451e Add personnel flow owner lane gate (#25)`.

## Purpose

Copy the v325-v332 personnel-flow owner-lane gate into the Desk Sandbox settlement mobility readback only when that settlement already has projected public-life personnel-flow readiness commands or receipts.

The desk should tell the player which owner lane can currently read personnel-flow pressure near the local settlement, but it must not infer hidden targets or spread a global gate across every node.

## Non-Goals

- No direct move, transfer, summon, assign, return, settle-person, or manpower-routing command.
- No new `PersonnelFlow`, `Migration`, or `SocialMobility` module.
- No command, movement, personnel, assignment, focus, scheduler, owner-lane-gate, desk-gate, or surface-echo ledger.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application, UI, or Unity movement-success calculation.
- No parsing of `DomainEvent.Summary`, `ReadbackSummary`, receipt prose, notification text, mobility text, public-life lines, or docs prose.
- No persisted state. Target schema/migration impact: none. If a persisted field becomes necessary, stop and write the schema/migration impact first.

## Owner Split

- `Application` already projects `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary`.
- `Zongzu.Presentation.Unity` may display that field on a settlement node only when structured public-life command affordances/receipts for that settlement carry `PersonnelFlowReadinessSummary`.
- `PopulationAndHouseholds` remains current household-response owner.
- `PersonRegistry` remains identity/FidelityRing only.
- `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` remain future owner-lane plans only.

## Implementation Plan

1. Append `PersonnelFlowOwnerLaneGateSummary` to `DeskSandbox` settlement mobility text only for settlements with local projected personnel-flow readiness commands/receipts.
2. Use `PlayerCommandSurfaceSnapshot.EnumerateAffordances` / `EnumerateReceipts`, not prose parsing.
3. Add focused presentation and architecture tests.
4. Update docs and acceptance notes.

## Readback Tokens

- `人员流动归口门槛`
- `当前可读归口为PopulationAndHouseholds本户回应`
- `desk settlement echo`
- `only when local projected readiness exists`
- `UI/Unity只显示投影字段`
- `不是直接调人、迁人、召人命令`

## Validation Plan

- Focused architecture test for v333-v340 boundary guard.
- Focused Unity presentation test for Desk Sandbox projected display.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Completion Notes

- Implemented Desk Sandbox settlement mobility echo for `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary`.
- The echo appears only when the settlement has structured public-life affordances or receipts with `PersonnelFlowReadinessSummary`.
- The adapter uses command-surface enumeration and does not parse prose, event summaries, receipts, or mobility text.
- Schema/migration impact: none. No persisted state, save namespace, module schema version, migration, projection cache, command ledger, movement ledger, personnel ledger, assignment ledger, focus ledger, scheduler ledger, desk-gate ledger, durable residue, or serialized module payload was added.
- Focused validation passed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Personnel_flow_desk_gate_echo_v333_v340`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_CopiesMobilityAndFidelityReadbacksIntoGreatHallDeskAndLineage`
