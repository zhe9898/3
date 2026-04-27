# Court Policy Public Follow-Up Cue v149-v156

## Goal

Continue Chain 8 after v141-v148 by letting the public-life command/readback surface show a first public follow-up cue for old court-policy local-response residue.

The target player-facing read is:

> The street remembers the earlier county-document handling. A later notice or road-report choice can say whether the public reading should cool, lightly continue, change method, or wait for a county/dispatch opening. This is projection guidance over old residue, not a cooldown ledger, not a new policy result, and not the home household paying the court's old account.

This remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` records are fact propagation tools, not the design body.

## Scope In

- Reuse existing `SocialMemoryKinds.OfficePolicyLocalResponseResidue` records.
- Reuse existing structured `OfficePolicyLocalResponseResidueCause.OutcomeCode` values.
- Reuse existing `SettlementPublicLifeSnapshot` public scalars.
- Add projection-only cue wording:
  - `政策公议后手提示`
  - `公议冷却提示`
  - `公议轻续提示`
  - `公议换招提示`
  - `下一步仍看榜示/递报承口`
  - `不是冷却账本`
  - `不从本户硬补`
- Surface the cue through existing governance/public-life command readback fields and Unity copy paths.

## Scope Out

- No Court module.
- No full court engine, faction AI, full policy economy, event pool, or scripted court process.
- No dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, public-follow-up ledger, public-reading ledger, memory-pressure ledger, or policy closure ledger.
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
- `Application`: routes/assembles/projects public follow-up cue text from existing structured snapshots.
- `Zongzu.Presentation.Unity`: copies projected fields only.

## Save/Schema Impact

Target impact: none.

This pass must not add persisted fields, namespaces, schema versions, migrations, save manifest changes, or feature-pack membership. If a durable public follow-up state, cooldown marker, repeat-pressure counter, or policy-follow-up ledger proves necessary, stop and replace this work with a schema/migration plan.

## Determinism Risk

Low. The cue is deterministic projection over existing snapshots and existing SocialMemory residue cause data. It adds no randomness, IO, scheduler phase, fanout, or state mutation.

## Milestones

1. Create this ExecPlan and keep it as the evidence anchor for v149-v156.
2. Add projection-only `BuildCourtPolicyPublicFollowUpCue` on top of existing public-reading echo.
3. Keep the cue on existing governance / public-life command / Unity-copy fields so no DTO schema or persistence migration is required.
4. Add focused integration and architecture tests for structured source use, no prose parsing, no schema drift, no forbidden managers/ledgers, and no owner-lane drift.
5. Add Unity presentation proof that public-life command affordances copy projected cue fields only.
6. Update topology, boundary, schema, simulation, UI, acceptance, audit, and skill-rationalization docs.
7. Run focused validation, `dotnet build Zongzu.sln --no-restore`, `git diff --check`, and `dotnet test Zongzu.sln --no-build`.

## Tests To Add/Update

- Integration:
  - later `office.policy_local_response...` residue appears as `政策公议后手提示`.
  - `Contained` outcome projects `公议轻续提示`.
  - off-scope settlement does not inherit the cue.
- Architecture:
  - projection reads structured outcome code and PublicLife scalars only.
  - no summary/prose parsing.
  - no new ledger, manager/god-controller, schema field, migration, or `PersonRegistry` expansion.
- Unity presentation:
  - shell copies public-life command `LeverageSummary` / `ReadbackSummary` cue text via existing fields only.

## Rollback / Fallback Plan

If existing SocialMemory cause keys and public-life snapshots are insufficient to provide the follow-up cue without parsing prose or persisting new state, stop before adding state and document the needed owner module, persisted shape, schema version, migration, and tests.

## Evidence Log

- 2026-04-27: Created ExecPlan after v141-v148; intended implementation remains schema-neutral and stacked on the court-policy public-reading echo branch.
- 2026-04-27: Implemented projection-only `BuildCourtPolicyPublicFollowUpCue` from existing `OfficePolicyLocalResponseResidueCause.OutcomeCode` and `SettlementPublicLifeSnapshot` scalars. The new readback emits `政策公议后手提示`, `公议轻续提示`, `下一步仍看榜示/递报承口`, `不是冷却账本`, and `不从本户硬补` through existing governance/public-life command fields only.
- 2026-04-27: Updated integration, architecture, Unity presentation, topology, boundary, integration, schema, simulation, UI, acceptance, audit, and skill-rationalization docs. Schema/migration impact remains none.
- 2026-04-27: Validation passed: `dotnet build Zongzu.sln --no-restore`; focused integration `Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue`; focused architecture `Court_policy_public_follow_up_cue_v149_v156_must_remain_projection_only_and_schema_neutral`; focused Unity `Compose_CopiesCourtPolicySocialMemoryEchoWithoutShellAuthority`; `git diff --check`; `dotnet test Zongzu.sln --no-build`.
