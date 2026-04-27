# Court Policy Follow-Up Docket Guard v157-v164

## Goal

Continue Chain 8 after v149-v156 by letting governance / office / docket / desk surfaces read the public follow-up cue with an explicit anti-misread guard.

The target player-facing read is:

> The policy public follow-up cue may appear on the docket, but the docket should name what it is not: not a cooldown ledger, not Order debt, not Office success/failure, and not a home-household bill. It is an案牍提示 over old public reading, still split across Office/PublicLife/SocialMemory.

This remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` records are fact propagation tools, not the design body.

## Scope In

- Reuse existing `SocialMemoryKinds.OfficePolicyLocalResponseResidue` records.
- Reuse existing structured `OfficePolicyLocalResponseResidueCause.OutcomeCode` values.
- Reuse existing `SettlementPublicLifeSnapshot` public scalars.
- Add projection-only docket/no-loop guard wording:
  - `政策后手案牍防误读`
  - `公议后手只作案牍提示`
  - `不是冷却账本`
  - `不是Order后账`
  - `不是Office成败`
  - `不从本户硬补`
  - `仍等Office/PublicLife/SocialMemory分读`
- Surface the guard through existing `CourtPolicyNoLoopGuardSummary`, governance docket `GuidanceSummary`, and Unity copy paths.

## Scope Out

- No Court module.
- No full court engine, faction AI, full policy economy, event pool, or scripted court process.
- No dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, public-follow-up ledger, public-reading ledger, docket ledger, memory-pressure ledger, or policy closure ledger.
- No cooldown ledger: `不是冷却账本` is presentation guidance only.
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
- `Application`: routes/assembles/projects docket guard text from existing structured snapshots.
- `Zongzu.Presentation.Unity`: copies projected fields only.

## Save/Schema Impact

Target impact: none.

This pass must not add persisted fields, namespaces, schema versions, migrations, save manifest changes, or feature-pack membership. If a durable public follow-up docket state, cooldown marker, repeat-pressure counter, or policy-follow-up ledger proves necessary, stop and replace this work with a schema/migration plan.

## Determinism Risk

Low. The guard is deterministic projection over existing snapshots and existing SocialMemory residue cause data. It adds no randomness, IO, scheduler phase, fanout, or state mutation.

## Milestones

1. Create this ExecPlan and keep it as the evidence anchor for v157-v164.
2. Add projection-only `BuildCourtPolicyPublicFollowUpDocketGuard` on top of existing public follow-up cue inputs.
3. Keep the guard on existing governance / docket / Unity-copy fields so no DTO schema or persistence migration is required.
4. Add focused integration and architecture tests for structured source use, no prose parsing, no schema drift, no forbidden managers/ledgers, and no owner-lane drift.
5. Add Unity presentation proof that governance/docket fields copy the guard only.
6. Update topology, boundary, schema, simulation, UI, acceptance, audit, and skill-rationalization docs.
7. Run focused validation, `dotnet build Zongzu.sln --no-restore`, `git diff --check`, and `dotnet test Zongzu.sln --no-build`.

## Tests To Add/Update

- Integration:
  - later `office.policy_local_response...` residue appears as `政策后手案牍防误读` in governance no-loop / docket guidance.
  - off-scope settlement does not inherit the docket guard.
- Architecture:
  - projection reads structured outcome code and PublicLife scalars only.
  - no summary/prose parsing.
  - no new ledger, manager/god-controller, schema field, migration, or `PersonRegistry` expansion.
- Unity presentation:
  - shell copies governance/docket `CourtPolicyNoLoopGuardSummary` text via existing fields only.

## Rollback / Fallback Plan

If existing SocialMemory cause keys and public-life snapshots are insufficient to provide the docket guard without parsing prose or persisting new state, stop before adding state and document the needed owner module, persisted shape, schema version, migration, and tests.

## Evidence Log

- 2026-04-27: Created ExecPlan after v149-v156; intended implementation remains schema-neutral and stacked on the court-policy public follow-up cue branch.
- 2026-04-27: Implemented projection-only `BuildCourtPolicyPublicFollowUpDocketGuard` from existing `OfficePolicyLocalResponseResidueCause.OutcomeCode` and `SettlementPublicLifeSnapshot` scalars. The new guard emits `政策后手案牍防误读`, `公议后手只作案牍提示`, `不是Order后账`, `不是Office成败`, and `仍等Office/PublicLife/SocialMemory分读` through existing governance/docket no-loop fields only.
- 2026-04-27: Updated integration, architecture, Unity presentation, topology, boundary, integration, schema, simulation, UI, acceptance, audit, and skill-rationalization docs. Schema/migration impact remains none.
- 2026-04-27: Validation passed: `dotnet build Zongzu.sln --no-restore`; focused integration `Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue`; focused architecture `Court_policy_follow_up_docket_guard_v157_v164_must_remain_projection_only_and_schema_neutral`; focused Unity `Compose_CopiesOfficeYamenReadbackSpineWithoutShellAuthority`; `git diff --check`; `dotnet test Zongzu.sln --no-build`.
