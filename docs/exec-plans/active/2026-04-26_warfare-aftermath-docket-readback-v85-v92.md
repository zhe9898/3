# Warfare Aftermath Docket Readback v85-v92

Date: 2026-04-26
Branch: `codex/warfare-aftermath-docket-readback-v85-v92`

## Intent

Continue the global skeleton after v77-v84 by making the existing WarfareCampaign aftermath docket legible on player-facing surfaces. The player should read that merits, blames, relief needs, and route repairs belong to the campaign aftermath docket, not to county-yamen paperwork, Order/public-life repair, or ordinary households.

This is projection/readback guidance over existing structured snapshots. It is not a new command system, not an event pool, not a tactical battle loop, not a thick military economy, not a cleanup ledger, and not a SocialMemory parser. `DomainEvent` remains one fact-propagation tool, not the design body.

## Scope

- Reuse existing `WarfareCampaign` aftermath state and query snapshots:
  - `AftermathDocketState`
  - `AftermathDocketSnapshot`
  - `Merits`
  - `Blames`
  - `ReliefNeeds`
  - `RouteRepairs`
  - `DocketSummary`
- Add runtime read-model access to campaign aftermath dockets through `PresentationReadModelBundle`.
- Add campaign aftermath docket readback wording to player-command, governance, desk, and Unity-facing readbacks:
  - `战后案卷读回`
  - `记功簿读回`
  - `劾责状读回`
  - `抚恤簿读回`
  - `清路札读回`
  - `WarfareCampaign拥有战后案卷`
  - `战后案卷不是县门/Order代算`
  - `不是普通家户补战后`
  - `军务案卷防回压`
- Use structured docket list presence and counts. Do not parse `DocketSummary`, `LastDirectiveTrace`, receipt prose, or `DomainEvent.Summary`.
- Unity shell displays projected aftermath docket fields only.

## Non-Goals

- No new persisted state.
- No schema or migration impact.
- No aftermath ledger, relief ledger, route-repair ledger, cooldown ledger, owner-lane ledger, campaign closure ledger, force closure ledger, household target field, or command queue.
- No new military command, tactical map, unit micro, battle simulator, force economy, elder AI, regime AI, or thick post-battle formula.
- No Application-owned command result calculation.
- No UI/Unity calculation of campaign aftermath outcomes.
- No UI/Unity writes to `SocialMemoryAndRelations`.
- No parsing of `DocketSummary`, receipt prose, `DomainEvent.Summary`, `LastDirectiveTrace`, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or SocialMemory summary prose.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

If a new persisted field appears necessary, stop before implementing it and document schema namespace, migration, save manifest, and acceptance-test impact first.

## Version Slices

- v85: ExecPlan and schema-neutral aftermath docket target.
- v86: Read-model bundle exposes existing `AftermathDocketSnapshot` values.
- v87: Application projection composes `战后案卷读回` from structured docket lists.
- v88: Governance/public-life surfaces include the campaign aftermath docket without treating it as Office or Order aftermath.
- v89: Unity shell uses projected aftermath docket fields only.
- v90: Integration and Unity tests prove docket readback and copy-only display.
- v91: Architecture tests guard no authority drift, no summary parsing, no forbidden managers, no PersonRegistry expansion, and no schema drift.
- v92: Docs, evidence, full validation, commit, and push.

## Ownership

- `WarfareCampaign` owns campaign aftermath dockets, including merits, blames, relief needs, route repairs, and docket summary.
- `ConflictAndForce` owns local force readiness, command capacity, response activation, force fatigue, escorts, and force pools.
- `OfficeAndCareer` owns official coordination and county paperwork; it does not own campaign aftermath dockets.
- `OrderAndBanditry` owns public-order aftermath, not military post-campaign accounting.
- `PopulationAndHouseholds` owns ordinary household local response only; it does not repair campaign aftermath.
- `SocialMemoryAndRelations` owns durable residue on later monthly progression from structured aftermath and cause keys only.
- `Application` routes and assembles projected read models; it may select from structured snapshots but does not compute campaign outcomes.
- `Unity` copies projected readback fields and renders existing docket snapshots; it does not derive merits, blame, relief, or route repair from notifications or prose.

## Schema / Migration Impact

Target impact: none.

This pass uses existing `WarfareCampaign` schema v4 aftermath docket fields and existing query DTOs. Adding `CampaignAftermathDockets` to `PresentationReadModelBundle` is runtime read-model exposure only. No root save version, module schema version, migration, manifest, serialized payload shape, persisted projection cache, or ledger should change.

## Evidence Targets

- `dotnet build Zongzu.sln --no-restore`
- Focused integration tests for Warfare aftermath docket readback and module isolation.
- Focused architecture guard for schema-neutral owner-lane discipline.
- Focused Unity presentation test for projected aftermath docket display.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Evidence Checklist

- [x] ExecPlan created.
- [x] Existing aftermath docket snapshots exposed in runtime read model.
- [x] Application aftermath docket readback implemented with existing fields.
- [x] Unity aftermath docket display changed to projected snapshot use.
- [x] Focused tests added/updated.
- [x] Required docs updated.
- [x] No schema/migration impact documented.
- [x] Build passed.
- [x] Focused tests passed.
- [x] `git diff --check` passed.
- [x] Full solution tests passed.
- [x] Commit and push completed.

## Evidence Results

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration tests passed: Warfare aftermath docket readback plus existing campaign command/bundle coverage.
- Focused architecture tests passed: aftermath docket readback, directive choice depth, and force/campaign owner-lane guards.
- Focused Unity presentation tests passed: regional warfare aftermath shell and projected lane closure fields.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
