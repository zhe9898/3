# Court Policy Public-Reading Echo v141-v148

## Goal

Continue Chain 8 after v133-v140 by letting the public-life command surface read the old court-policy local-response residue as public interpretation, not as a new policy result.

The target player-facing read is:

> The next notice or road-report choice can see that street talk still remembers the earlier county-document handling. PublicLifeAndRumor reads the public interpretation, OfficeAndCareer still owns county-gate execution, and SocialMemoryAndRelations still owns durable residue. The home household does not hard-carry the court's old account.

This remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` records are fact propagation tools, not the design body.

## Scope In

- Reuse existing `SocialMemoryKinds.OfficePolicyLocalResponseResidue` records and existing `MemoryRecordState` fields.
- Reuse existing `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` structured fields.
- Add projection-only public-reading echo text to existing governance and public-life command readbacks.
- Surface the echo through existing `CourtPolicyPublicReadbackSummary`, `LeverageSummary`, and `ReadbackSummary` paths.
- Prove Unity copies the projected public-life command fields only.

Suggested readback tokens:

- `政策公议旧读回`
- `公议旧账回声`
- `下一次榜示/递报旧读法`
- `PublicLife只读街面解释`
- `县门承接仍归OfficeAndCareer`
- `不是本户硬扛朝廷旧账`

## Scope Out

- No Court module.
- No full court engine, faction AI, full policy economy, event pool, or scripted court process.
- No dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, public-reading ledger, memory-pressure ledger, or policy closure ledger.
- No new persisted field, root schema version, module schema version, save manifest entry, or migration.
- No Application calculation of policy success.
- No UI/Unity derivation of court-policy outcomes.
- No parsing of `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or memory `Summary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller path.

## Affected Modules

- `WorldSettlements`: remains the court agenda / mandate pressure source only.
- `OfficeAndCareer`: owns policy window, county execution, county-document command aftermath, and implementation posture.
- `PublicLifeAndRumor`: owns public interpretation, notice visibility, street reading, and public-life presentation texture.
- `SocialMemoryAndRelations`: owns durable old-policy-response residue from later monthly passes.
- `Application`: routes/assembles/projects the public-reading echo only.
- `Zongzu.Presentation.Unity`: copies projected fields only.

## Save/Schema Impact

Target impact: none.

This pass must not add persisted fields, namespaces, schema versions, migrations, save manifest changes, or feature-pack membership. If a new public-reading state, residue discriminator, or repeat-pressure field proves necessary, stop and replace this work with a schema/migration plan.

## Determinism Risk

Low. The readback uses deterministic read-model snapshots and active SocialMemory records. It adds no randomness, IO, scheduler phase, fanout, or state mutation.

## Milestones

1. Create this ExecPlan and keep it as the evidence anchor for v141-v148.
2. Add projection-only public-reading echo from structured SocialMemory cause/type/weight plus current Office/PublicLife snapshots.
3. Keep the echo on existing governance / public-life command / Unity-copy fields so no DTO schema or persistence migration is required.
4. Add focused integration and architecture tests for structured source use, no prose parsing, no schema drift, no forbidden managers/ledgers, and no owner-lane drift.
5. Add Unity presentation proof that public-life command affordances copy projected echo fields only.
6. Update topology, boundary, schema, simulation, UI, acceptance, audit, and skill-rationalization docs.
7. Run focused validation, `dotnet build Zongzu.sln --no-restore`, `git diff --check`, and `dotnet test Zongzu.sln --no-build`.

## Tests To Add/Update

- Integration:
  - old court-policy local-response residue appears in later public-life notice/report command readbacks as `政策公议旧读回`.
  - same-month command aftermath remains SocialMemory-neutral.
  - off-scope settlement does not inherit the public-reading echo.
- Architecture:
  - projection reads structured SocialMemory cause/type/weight and Office/PublicLife snapshots only.
  - no summary/prose parsing.
  - no new ledger, manager/god-controller, schema field, migration, or `PersonRegistry` expansion.
- Unity presentation:
  - shell copies public-life command `LeverageSummary` / `ReadbackSummary` echo text via existing fields only.

## Rollback / Fallback Plan

If existing SocialMemory cause keys and jurisdiction/public-life snapshots are insufficient to identify the old policy-response public reading without parsing prose, stop before adding state and document the needed owner module, persisted shape, schema version, migration, and tests.

## Evidence Log

- 2026-04-27: Created ExecPlan after v133-v140 push; intended implementation remains schema-neutral and stacked on the court-policy memory-pressure readback branch.
- 2026-04-27: Implemented projection-only `政策公议旧读回` / `公议旧账回声` in `PresentationReadModelBuilder` from structured SocialMemory cause/type/weight plus current `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` values.
- 2026-04-27: Reused existing `CourtPolicyPublicReadbackSummary`, `PlayerCommandAffordanceSnapshot.LeverageSummary`, `PlayerCommandAffordanceSnapshot.ReadbackSummary`, and Unity command-copy paths; no DTO persistence field, schema version, migration, ledger, Court module, manager/god-controller path, or `PersonRegistry` expansion was added.
- 2026-04-27: Updated integration, architecture, Unity presentation, topology, boundary, integration, schema, simulation, UI, acceptance, audit, and skill-rationalization docs. Schema/migration impact remains none.
- 2026-04-27: Validation passed: `dotnet build Zongzu.sln --no-restore`; focused integration `Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue`; focused architecture `Court_policy_public_reading_echo_v141_v148_must_remain_projection_only_and_schema_neutral`; focused Unity `Compose_CopiesCourtPolicySocialMemoryEchoWithoutShellAuthority`; `git diff --check`; `dotnet test Zongzu.sln --no-build`.
