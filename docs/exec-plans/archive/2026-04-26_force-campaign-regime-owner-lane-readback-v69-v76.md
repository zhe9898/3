# Force / Campaign / Regime Owner-Lane Readback Closure v69-v76

Date: 2026-04-26
Branch: `codex/global-skeleton-force-campaign-v69-v76`

## Intent

This pass adds thin global skeleton readback for the military lane after the v53-v60 Family closure and v61-v68 Family relief choice work. It is projection/readback guidance only: the player-facing surfaces should say that military aftermath, force readiness, supply-line aftercare, and official coordination must be read in their owner lanes instead of being pushed back onto ordinary households, Office-only paperwork, or Order/public-life after账.

This is not a new command system, event pool, event-chain layer, campaign ledger, tactical warfare loop, regime AI, court AI, or thick military economy. The authoritative shape remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` remains a fact-propagation tool, not the design object.

## Scope

- Add projected readback fields for Force/Campaign closure guidance to player command affordances and receipts.
- Project the same guidance through governance / owner-lane docket surfaces when a settlement is also the anchor for campaign pressure.
- Copy projected fields into Unity ViewModels; Unity must not infer, compute, or write military closure results.
- Use existing structured snapshots only:
  - `CampaignMobilizationSignalSnapshot`
  - `CampaignFrontSnapshot`
  - `JurisdictionAuthoritySnapshot`
  - existing SocialMemory structured cause keys where relevant
- Keep command-time force/campaign effects inside `WarfareCampaign` / `ConflictAndForce` owners.
- Keep ordinary home-household response in `PopulationAndHouseholds`.
- Keep durable residue in `SocialMemoryAndRelations` on later monthly progression.

## Non-Goals

- No new persisted state.
- No schema or migration impact.
- No cooldown ledger, owner-lane ledger, family closure ledger, force closure ledger, campaign closure ledger, household target field, or campaign target household field.
- No tactical map, unit micro, battle simulator, full guarantee formula, clan elder AI, regime faction AI, or thick war economy.
- No parsing of receipt prose, `DomainEvent.Summary`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No Application-owned command result calculation.
- No UI/Unity writes to `SocialMemoryAndRelations`.

If implementation discovers that a new persisted field is required, stop before coding that part and document the schema, namespace, migration, and acceptance-test impact first.

## Version Slices

- v69: ExecPlan and boundary readback target; explicitly schema-neutral.
- v70: Player-command affordance projection for military lane entry, force readiness, campaign aftermath, and no-loop guard.
- v71: Player-command receipt projection for active campaign directives and aftermath closure.
- v72: Governance / owner-lane docket projection so campaign/regime back账 is not misread as Office or Order only.
- v73: Social-memory follow-up guidance remains structured-cause only and does not parse readback prose.
- v74: Unity shell copies projected military closure fields only.
- v75: Architecture and focused presentation/integration tests cover no authority drift, no summary parsing, no forbidden manager/god controller names, no PersonRegistry expansion, and no new schema.
- v76: Docs/audit/evidence update and full validation.

## Required Readback Tokens

- `军务承接入口`
- `Force承接读回`
- `战后后账读回`
- `军务后手收口读回`
- `军务余味续接读回`
- `军务闭环防回压`
- `不是普通家户硬扛`
- `不是把军务后账误读成县门/Order后账`

## Ownership

- `WarfareCampaign` owns campaign board, directives, fronts, supply-line state, and campaign aftermath.
- `ConflictAndForce` owns local force readiness, command capacity, response activation, force fatigue, and escort strain.
- `OfficeAndCareer` owns official coordination and regime/office wavering readback.
- `SocialMemoryAndRelations` owns durable shame/favor/fear/grudge/obligation residue in later monthly progression.
- `PopulationAndHouseholds` owns ordinary household local response only.
- `Application` routes/assembles read models and may select from structured snapshots; it does not calculate command outcomes.
- `Unity` copies projected fields and does not derive owner-lane closure.

## Evidence Targets

- `dotnet build Zongzu.sln --no-restore`
- Focused integration / architecture / Unity presentation tests for Force/Campaign readback.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Schema / Migration Impact

Target impact: none. This pass adds read-model/ViewModel projection fields only. No module state schema version, root save version, migration, save manifest, or serialized module payload changes should be required.

## Completion Evidence - 2026-04-26

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration tests passed:
  - `CampaignSandboxBootstrap_ActivatesWarfareCampaignAndSurfacesReadOnlyBoard`
  - `CampaignBundle_ExportsReadOnlyPlayerCommandAffordances`
  - `PlayerCommandService_RoutesOfficeAndWarfareIntents_AndUpdatesReadModels`
- Focused architecture guard passed: `Force_campaign_owner_lane_readback_must_stay_projection_only_and_schema_neutral`.
- Focused Unity presentation tests passed:
  - `Compose_CopiesProjectedWarfareLaneClosureFieldsOnly`
  - `Compose_ProjectsRegionalWarfareAndAftermathIntoHallDeskAndCampaignBoard`
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed, including the full integration suite.
- Schema/migration result remained none: no persisted state, module schema version, root save version, migration, save manifest, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, or projection cache was added.
