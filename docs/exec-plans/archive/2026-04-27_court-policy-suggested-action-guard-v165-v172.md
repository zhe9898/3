# Court Policy Suggested Action Guard v165-v172

## Goal

Continue Chain 8 after v157-v164 by letting the governance docket's suggested action prompt carry the same anti-misread guard when it is already selecting an affordance that displays the court-policy public follow-up cue.

The target player-facing read is:

> The docket may suggest an action, but that suggestion only carries already-projected public follow-up meaning. It does not create a new priority rule, a policy result, a cooldown ledger, Order debt, Office success/failure, or a home-household bill.

This remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` records are fact propagation tools, not the design body.

## Scope In

- Reuse existing `PlayerCommandAffordanceSnapshot` fields.
- Reuse existing `CourtPolicyNoLoopGuardSummary`.
- Reuse existing `SelectPrimaryGovernanceAffordance` ordering without changing priority.
- Add projection-only suggested-action guard wording:
  - `建议动作防误读`
  - `只承接已投影的政策公议后手`
  - `不是冷却账本`
  - `不是Order后账`
  - `不是Office成败`
  - `不从本户硬补`
- Surface the guard through existing `SuggestedCommandPrompt` and docket `GuidanceSummary` copy paths.

## Scope Out

- No Court module.
- No full court engine, faction AI, full policy economy, event pool, or scripted court process.
- No new suggested-action ranking rule, command availability rule, command queue, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, public-follow-up ledger, public-reading ledger, docket ledger, memory-pressure ledger, or policy closure ledger.
- No new persisted field, root schema version, module schema version, save manifest entry, or migration.
- No Application calculation of policy success.
- No UI/Unity derivation of court-policy outcomes.
- No parsing of `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or memory `Summary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller path.

## Affected Modules

- `WorldSettlements`: remains only the court agenda / mandate pressure source.
- `OfficeAndCareer`: owns policy window, county execution, document/report aftermath, and implementation posture.
- `PublicLifeAndRumor`: owns public interpretation, notice visibility, street reading, and public-life presentation texture.
- `SocialMemoryAndRelations`: owns durable old-policy-response residue from later monthly passes.
- `Application`: assembles suggested-action guard text from already-projected affordance and no-loop guard fields.
- `Zongzu.Presentation.Unity`: copies projected governance/docket text only.

## Save/Schema Impact

Target impact: none.

This pass must not add persisted fields, namespaces, schema versions, migrations, save manifest changes, or feature-pack membership. If a durable suggested-action ranking state, cooldown marker, repeat-pressure counter, or policy-follow-up ledger proves necessary, stop and replace this work with a schema/migration plan.

## Determinism Risk

Low. The guard is deterministic projection over already-built affordance and governance guard text. It adds no randomness, IO, scheduler phase, fanout, state mutation, or ordering change.

## Milestones

1. Create this ExecPlan and keep it as the evidence anchor for v165-v172.
2. Add projection-only `BuildGovernanceSuggestedActionGuard` without changing `SelectPrimaryGovernanceAffordance` priority.
3. Keep the guard on existing `SuggestedCommandPrompt` and docket `GuidanceSummary` fields so no DTO schema or persistence migration is required.
4. Add focused integration and architecture tests for projected source use, no prose parsing, no schema drift, no forbidden managers/ledgers, and no owner-lane drift.
5. Add Unity presentation proof that governance/docket text copies the guard only.
6. Update topology, boundary, schema, simulation, UI, acceptance, audit, and skill-rationalization docs.
7. Run focused validation, `dotnet build Zongzu.sln --no-restore`, `git diff --check`, and `dotnet test Zongzu.sln --no-build`.

## Tests To Add/Update

- Integration:
  - later `office.policy_local_response...` residue appears as `建议动作防误读` in governance suggested prompt and docket guidance.
  - off-scope settlement does not inherit the suggested-action guard.
- Architecture:
  - suggested-action guard reads existing affordance and no-loop guard text only.
  - no summary/prose parsing.
  - no new ranking rule, ledger, manager/god-controller, schema field, migration, or `PersonRegistry` expansion.
- Unity presentation:
  - shell copies governance/docket `GuidanceSummary` guard text via existing fields only.

## Rollback / Fallback Plan

If existing affordance and no-loop guard projection fields are insufficient to provide the suggested-action guard without parsing prose or persisting new state, stop before adding state and document the needed owner module, persisted shape, schema version, migration, and tests.

## Evidence Log

- 2026-04-27: Created ExecPlan after v157-v164; intended implementation remains schema-neutral and stacked on the court-policy follow-up docket guard branch.
- 2026-04-27: Added projection-only `BuildGovernanceSuggestedActionGuard` on existing suggested-command prompt / docket guidance paths. The guard reads structured court-policy public follow-up eligibility and already-selected affordance data; it does not change `SelectPrimaryGovernanceAffordance` priority.
- 2026-04-27: Updated topology, boundary, integration, schema, simulation, UI, acceptance, alignment-audit, and skill-rationalization docs. Schema/migration impact remains none.
- 2026-04-27: Validation passed: `dotnet build Zongzu.sln --no-restore`; focused integration `Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue`; focused architecture `Court_policy_suggested_action_guard_v165_v172_must_remain_projection_only_and_schema_neutral`; focused Unity presentation `Compose_ProjectsGovernanceMomentumIntoGreatHallAndDeskSandbox`; `git diff --check`; `dotnet test Zongzu.sln --no-build`.
