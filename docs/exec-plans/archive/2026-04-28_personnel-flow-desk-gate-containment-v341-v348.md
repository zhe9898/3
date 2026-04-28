# Personnel Flow Desk Gate Containment V341-V348

## Intent

V341-V348 is a small containment pass over the v325-v340 personnel-flow owner-lane gate and Desk Sandbox echo.

The goal is to prove that a projected global owner-lane gate is not automatically local to every settlement. Desk Sandbox may echo the gate only for a settlement that already has structured public-life personnel-flow readiness affordances or receipts.

## Non-Goals

- Not a direct personnel command.
- Not a migration, assignment, transfer, return, summon, office-service, or manpower system.
- Not a new `PersonnelFlow`, `SocialMobility`, or `Migration` module.
- Not a movement/personnel/assignment/focus/scheduler/desk-gate ledger.
- Not Application movement resolution.
- Not UI/Unity authority.
- Not a prose parser over `DomainEvent.Summary`, command summaries, receipt prose, notification prose, mobility text, public-life lines, or documentation text.

## Ownership

- `PopulationAndHouseholds` continues to own current household response and migration-pressure facts.
- `PersonRegistry` continues to own identity and the existing `FidelityRing` only.
- Application may assemble projected command-surface fields.
- Desk Sandbox may copy already projected fields and use structured command-surface enumeration by settlement.
- Unity remains copy-only.
- `SocialMemoryAndRelations` writes no durable movement residue in this pass.

## Target Schema / Migration

Target schema/migration impact: none.

If implementation requires persisted state, a module schema bump, a save manifest change, a projection cache, or a migration step, stop and write the schema impact before editing code.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- Focused presentation test for no desk gate echo on a settlement without local readiness.
- Focused architecture test for v341-v348 containment docs, no prose parsing, and schema neutrality.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Completion Notes

- Added a negative presentation test proving Desk Sandbox does not echo `PersonnelFlowOwnerLaneGateSummary` onto a settlement without local public-life `PersonnelFlowReadinessSummary`.
- Added an architecture guard proving v341-v348 remains local projection only, schema-neutral, and parser-free.
- Updated topology, boundary, integration, schema, simulation, UI, acceptance, player-scope, and audit docs.
- Schema/migration impact: none.
- Validation completed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_DoesNotEchoPersonnelFlowGateToDeskSettlementWithoutLocalReadiness`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Personnel_flow_desk_gate_containment_v341_v348`
