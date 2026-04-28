# Personnel Flow Surface Echo V309-V316

Date: 2026-04-28

Baseline: `main` at `4eb8f01 Add personnel flow readiness readback (#22)`.

## Purpose

Close the first personnel-flow command readiness layer by giving the command surface a structured echo of the v301-v308 readbacks. This lets Great Hall / shell mobility readback say that personnel-flow pressure is visible as a bounded household-command readiness cue without turning it into a direct movement command.

This is still the same principle: near detail stays readable, distant people stay summarized, and the player sees bounded local leverage rather than a god-control personnel panel.

## Non-Goals

- No direct move, transfer, summon, assign, return, settle-person, or manpower routing command.
- No new `PersonnelFlow`, `Migration`, or `SocialMobility` module.
- No command, movement, personnel, assignment, focus, scheduler, or surface echo ledger.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application, UI, or Unity movement-success calculation.
- No parsing of `DomainEvent.Summary`, `ReadbackSummary`, receipt prose, notification text, mobility text, or public-life lines.
- No persisted state. Target schema/migration impact: none. If a persisted field becomes necessary, stop and write the schema/migration impact first.

## Owner Split

- `PopulationAndHouseholds` owns household livelihood, labor, distress, migration pressure, activity, and local household response resolution.
- `PersonRegistry` owns identity and `FidelityRing` only.
- `Application` may assemble a runtime `PlayerCommandSurfaceSnapshot.PersonnelFlowReadinessSummary` from structured command affordance/receipt fields.
- `Zongzu.Presentation.Unity` may append that projected summary to Great Hall mobility readback.
- Unity shell code remains copy/display only and must not infer hidden targets, rank people, or calculate movement outcomes.

## Implementation Plan

1. Add a runtime-only `PersonnelFlowReadinessSummary` field to `PlayerCommandSurfaceSnapshot`.
2. Build it only from non-empty `PlayerCommandAffordanceSnapshot.PersonnelFlowReadinessSummary` and `PlayerCommandReceiptSnapshot.PersonnelFlowReadinessSummary`.
3. Append the summary to Great Hall mobility readback through the presentation adapter, without parsing prose or calculating outcomes.
4. Add focused integration, architecture, and presentation tests.
5. Update docs and acceptance notes.

## Readback Tokens

- `人员流动命令预备汇总`
- `人员流动预备读回`
- `近处细`
- `远处汇总`
- `只汇总已投影`
- `不解析ReadbackSummary`
- `PopulationAndHouseholds拥有本户回应`
- `PersonRegistry只保身份/FidelityRing`
- `UI/Unity只复制投影字段`
- `不是直接调人、迁人、召人命令`

## Validation Plan

- Focused integration test for command-surface personnel-flow echo.
- Focused architecture test for v309-v316 boundary guard.
- Focused Unity presentation test for Great Hall copy/display behavior.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Completion Notes

- Implemented runtime-only `PlayerCommandSurfaceSnapshot.PersonnelFlowReadinessSummary`.
- The summary is assembled only from existing affordance/receipt `PersonnelFlowReadinessSummary` fields and explicitly guards against parsing `ReadbackSummary`, receipt prose, or event summaries.
- Great Hall mobility readback appends only the projected command-surface echo.
- Schema/migration impact: none. No persisted state, save namespace, module schema version, migration, projection cache, or ledger was added.
- Focused validation passed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter PartialWatchResidue_ProjectsHomeHouseholdLocalResponse_AndCommandMutatesOnlyPopulation`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Personnel_flow_surface_echo_v309_v316`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_CopiesMobilityAndFidelityReadbacksIntoGreatHallDeskAndLineage`
