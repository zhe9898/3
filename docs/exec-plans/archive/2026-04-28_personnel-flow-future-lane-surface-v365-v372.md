# Personnel Flow Future Lane Surface V365-V372

## Intent

V365-V372 turns the v357-v364 future owner-lane preflight into a player-facing readback on the existing player-command / Great Hall mobility surface.

The purpose is to let the player read that `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` personnel-flow work still needs a separate owner-lane plan before any command, rule, state, schema, or validation lane exists.

## Non-Goals

- No new command.
- No movement resolver.
- No migration economy.
- No office-service, kin-transfer, assignment, return, summon, dispatch-labor, or campaign-manpower rule.
- No new `PersonnelFlow`, `SocialMobility`, or `Migration` module.
- No command/movement/personnel/assignment/focus/scheduler/future-lane-surface ledger.
- No Application movement authority.
- No UI/Unity authority.
- No prose parsing of `DomainEvent.Summary`, command summaries, receipt prose, notification prose, mobility text, public-life lines, or docs text.

## Implementation Shape

- Add `PlayerCommandSurfaceSnapshot.PersonnelFlowFutureOwnerLanePreflightSummary` as runtime read-model text only.
- Build the field in Application from already projected affordance/receipt `PersonnelFlowReadinessSummary` presence and counts.
- Display the field in Great Hall mobility readback as a projected field copy.
- Keep Desk Sandbox local: this pass does not append the future-lane preflight to settlement nodes.

## Target Schema / Migration

Target schema/migration impact: none.

If implementation requires persisted state, a module schema bump, a save manifest change, a projection cache, or a migration step, stop and write the schema impact before editing code.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- Focused Unity presentation test for Great Hall copy and Desk non-echo.
- Focused architecture surface/preflight test.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Completion Notes

- Added `PlayerCommandSurfaceSnapshot.PersonnelFlowFutureOwnerLanePreflightSummary` as runtime read-model text only.
- Application assembles the preflight from structured `PersonnelFlowReadinessSummary` affordance/receipt presence and counts.
- Great Hall copies the projected field into mobility readback; Desk Sandbox does not echo it onto settlement nodes.
- Updated topology, audit, boundary, integration, data schema, schema namespace, simulation, fidelity, UI, acceptance, player scope, and skill-matrix evidence.
- Schema/migration impact: none.
- Validation completed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_CopiesMobilityAndFidelityReadbacksIntoGreatHallDeskAndLineage`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter Personnel_flow_future_owner_lane_surface_v365_v372`
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`
