# Warfare Directive Choice Depth v77-v84

Date: 2026-04-26
Branch: `codex/warfare-directive-choice-v77-v84`

## Intent

Continue the global skeleton after v69-v76 by making the WarfareCampaign lane's own command choice readable. The player-facing surface should now say which military directive has been chosen, why it belongs to the campaign/mobilization owner lane, and why the result is not repaired by ordinary households, county paperwork, or Order/public-life back accounts.

This is a bounded owner-owned command / aftermath / social-memory / readback pass. It is not a new command system, not an event pool, not a tactical battle loop, not a thick military economy, not regime AI, and not a new ledger. `DomainEvent` remains one fact-propagation tool, not the design body.

## Scope

- Use existing WarfareCampaign command names:
  - `DraftCampaignPlan`
  - `CommitMobilization`
  - `ProtectSupplyLine`
  - `WithdrawToBarracks`
- Add readback wording for the directive choice itself:
  - `军令选择读回`
  - `案头筹议选择`
  - `点兵加压选择`
  - `粮道护持选择`
  - `归营止损选择`
  - `WarfareCampaign拥有军令`
  - `军务选择不是县门文移代打`
  - `不是普通家户硬扛`
- Keep command-time mutation inside `WarfareCampaign`, using existing persisted fields:
  - `ActiveDirectiveCode`
  - `ActiveDirectiveLabel`
  - `ActiveDirectiveSummary`
  - `LastDirectiveTrace`
- Add projected directive-choice readback to Warfare affordances and receipts through existing `ReadbackSummary`.
- Keep v69-v76 lane closure fields intact and compose them with directive-choice readback.
- Unity shell copies the projected command/receipt fields only.

## Non-Goals

- No new persisted state.
- No schema or migration impact.
- No cooldown ledger, owner-lane ledger, campaign closure ledger, force closure ledger, directive ledger, household target field, or command queue.
- No tactical map, unit micro, battle simulator, force economy, court/regime AI, or thick mobilization formula.
- No Application-owned command result calculation.
- No UI/Unity calculation of military directive outcomes.
- No UI/Unity writes to `SocialMemoryAndRelations`.
- No parsing of receipt prose, `DomainEvent.Summary`, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or SocialMemory summary prose.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

If a new persisted field appears necessary, stop before implementing it and document schema namespace, migration, save manifest, and acceptance-test impact first.

## Version Slices

- v77: ExecPlan and schema-neutral owner-lane target.
- v78: WarfareCampaign directive-result prose names the selected military choice without creating a new command system.
- v79: Application affordance projection composes directive-choice readback with the v69-v76 Warfare lane closure guidance.
- v80: Application receipt projection composes the same directive-choice readback with campaign closure fields.
- v81: Integration tests prove directive choice readback and command-time module isolation.
- v82: Unity presentation tests prove copy-only display of projected directive-choice readback.
- v83: Architecture tests guard no Application/UI/Unity authority drift, no summary parsing, no forbidden managers, no PersonRegistry expansion, and no schema drift.
- v84: Docs, evidence, full validation, commit, and push.

## Ownership

- `WarfareCampaign` owns campaign directives, front pressure, supply state, mobilization windows, active directive summaries, and directive traces.
- `ConflictAndForce` owns local force readiness, command capacity, response activation, force fatigue, escorts, and force pools.
- `OfficeAndCareer` owns official coordination and county paperwork; it does not choose or resolve military directives.
- `PopulationAndHouseholds` owns ordinary household local response only; it does not carry the military command result.
- `SocialMemoryAndRelations` owns durable residue on later monthly progression from structured aftermath.
- `Application` routes and assembles projected read models; it may select from structured snapshots but does not compute military command outcomes.
- `Unity` copies projected readback fields and does not derive owner-lane closure or directive success.

## Schema / Migration Impact

Target impact: none.

This pass uses existing `WarfareCampaign` state fields and existing command/read-model DTO fields. No root save version, module schema version, migration, manifest, serialized payload shape, persisted projection cache, or ledger should change.

## Evidence Targets

- `dotnet build Zongzu.sln --no-restore`
- Focused integration tests for Warfare directive-choice readback and module isolation.
- Focused architecture guard for schema-neutral owner-lane discipline.
- Focused Unity presentation test for copy-only projected readback.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Evidence Checklist

- [x] ExecPlan created.
- [x] WarfareCampaign directive-choice readback implemented with existing fields.
- [x] Application affordance/receipt projection updated.
- [x] Focused tests added/updated.
- [x] Required docs updated.
- [x] No schema/migration impact documented.
- [x] Build passed.
- [x] Focused tests passed.
- [x] `git diff --check` passed.
- [x] Full solution tests passed.
- [x] Commit and push completed.

## Completion Evidence - 2026-04-26

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration tests passed:
  - `CampaignSandboxCommandService_AppliesAncientDirectiveToCampaignBoard`
  - `CampaignBundle_ExportsReadOnlyPlayerCommandAffordances`
  - `PlayerCommandService_RoutesOfficeAndWarfareIntents_AndUpdatesReadModels`
- Focused architecture guards passed:
  - `Warfare_directive_choice_depth_must_stay_warfare_owned_and_schema_neutral`
  - `Force_campaign_owner_lane_readback_must_stay_projection_only_and_schema_neutral`
- Focused Unity presentation test passed:
  - `Compose_CopiesProjectedWarfareLaneClosureFieldsOnly`
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
- Schema/migration result remained none: no persisted state, module schema version, root save version, migration, save manifest, directive ledger, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, projection cache, manager/controller layer, or `PersonRegistry` expansion was added.
- Branch commit/push target: `codex/warfare-directive-choice-v77-v84`.
