# Court Policy Memory-Pressure Readback v133-v140

## Goal

Continue Chain 8 after v125-v132 by letting prior court-policy local-response residue become a first-layer readback input for the next visible policy window.

The target player-facing read is:

> This new policy window is not abstract and not cleanly fresh. The county gate and public reading remember earlier document/report handling, so the next readback shows old policy-response residue pressing on the current window. Results still belong to OfficeAndCareer, PublicLifeAndRumor, and SocialMemoryAndRelations; Application and Unity only project/copy.

This remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` records are fact propagation tools, not the design body.

## Scope In

- Reuse existing `SocialMemoryKinds.OfficePolicyLocalResponseResidue` records and existing `MemoryRecordState` fields.
- Reuse existing `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` structured scalar fields.
- Add projection-only readback that combines:
  - current Chain 8 policy process visibility;
  - existing SocialMemory court-policy local-response residue;
  - Office/PublicLife owner-lane wording.
- Surface the readback through existing governance / office / docket / Unity-copy fields.
- Add tests proving no prose parsing, no same-month durable write, no schema drift, and no UI/Unity authority.

Suggested readback tokens:

- `政策旧账回压读回`
- `旧文移余味`
- `下一次政策窗口读法`
- `公议旧读法续压`
- `仍由Office/PublicLife/SocialMemory分读`
- `不是本户硬扛朝廷旧账`

## Scope Out

- No Court module.
- No full court engine, faction AI, full policy economy, event pool, or scripted court process.
- No dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, response cooldown ledger, memory-pressure ledger, or policy closure ledger.
- No new persisted field, root schema version, module schema version, save manifest entry, or migration.
- No Application calculation of policy success.
- No UI/Unity derivation of court-policy outcomes.
- No parsing of `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or memory `Summary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller path.

## Affected Modules

- `OfficeAndCareer`: owns current policy window, county document/report command aftermath, and jurisdiction structured fields.
- `PublicLifeAndRumor`: owns public interpretation, notice visibility, and street reading.
- `SocialMemoryAndRelations`: owns durable residue already written in later monthly passes.
- `WorldSettlements`: remains the court agenda / mandate pressure source only.
- `Application`: projects structured memory-pressure readback only.
- `Zongzu.Presentation.Unity`: copies projected fields only.

## Save/Schema Impact

Target impact: none.

This pass must not add persisted fields, namespaces, schema versions, migrations, save manifest changes, or feature-pack membership. If a new persisted discriminator or repeat-pressure field proves necessary, stop and replace this work with a schema/migration plan.

## Determinism Risk

Low. The readback uses already-built deterministic read-model snapshots and active SocialMemory records. It adds no randomness, IO, scheduler phase, fanout, or state mutation.

## Milestones

1. Create this ExecPlan and keep it as the evidence anchor for v133-v140.
2. Add projection-only memory-pressure readback from structured SocialMemory cause/type/weight plus current Office/PublicLife snapshots.
3. Keep the readback on existing governance / office / docket fields so no DTO schema or persistence migration is required.
4. Add focused integration and architecture tests for structured source use, no prose parsing, no schema drift, no forbidden managers/ledgers, and no owner-lane drift.
5. Add Unity presentation test proof that the shell copies projected fields only.
6. Update topology, boundary, schema, simulation, UI, acceptance, audit, and skill-rationalization docs.
7. Run focused validation, `dotnet build Zongzu.sln --no-restore`, `git diff --check`, and `dotnet test Zongzu.sln --no-build`.

## Tests To Add/Update

- Integration:
  - old court-policy local response residue appears in a later next-window projection as `政策旧账回压读回`.
  - same-month command aftermath remains SocialMemory-neutral.
  - off-scope settlement does not inherit the old residue.
- Architecture:
  - projection reads structured SocialMemory cause/type/weight and Office/PublicLife snapshots only.
  - no summary/prose parsing.
  - no new ledger, manager/god-controller, schema field, migration, or `PersonRegistry` expansion.
- Unity presentation:
  - shell copies the projected memory-pressure readback via existing governance / office surfaces only.

## Rollback / Fallback Plan

If existing SocialMemory cause keys and jurisdiction/public-life snapshots are insufficient to identify the old policy-response residue without parsing prose, stop before adding state and document the needed owner module, persisted shape, schema version, migration, and tests.

## Evidence Log

- 2026-04-27: Created ExecPlan after v125-v132 push; current intended implementation remains schema-neutral and stacked on the court-policy local-response/social-memory branch.
- 2026-04-27: Implemented projection-only `政策旧账回压读回` in `PresentationReadModelBuilder.Governance` from structured SocialMemory cause/type/weight plus current `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` values.
- 2026-04-27: Reused existing `OfficeLaneResidueFollowUpSummary` governance/office/docket/Unity-copy fields; no DTO persistence field, schema version, migration, ledger, Court module, manager/god-controller path, or `PersonRegistry` expansion was added.
- 2026-04-27: Updated integration, architecture, Unity presentation, topology, boundary, integration, schema, simulation, UI, acceptance, audit, and skill-rationalization docs. Schema/migration impact remains none.
- 2026-04-27: Validation passed: `dotnet build Zongzu.sln --no-restore`; focused integration `Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue`; focused architecture `Court_policy_memory_pressure_readback_v133_v140_must_remain_projection_only_and_schema_neutral`; focused Unity `Compose_CopiesCourtPolicySocialMemoryEchoWithoutShellAuthority`; `git diff --check`; `dotnet test Zongzu.sln --no-build`.
