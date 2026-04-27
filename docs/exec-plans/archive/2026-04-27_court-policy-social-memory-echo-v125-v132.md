# Court Policy Social-Memory Echo v125-v132

## Goal

Add the delayed social-memory echo after v117-v124 court-policy local response affordances.

The target player-facing read is:

> The player may have lightly continued a county document/report path, but any durable residue appears only in a later SocialMemory month pass, from structured Office aftermath. It should read as court-policy local response residue, not as Order after-accounting and not as home-household debt.

This remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` remains a fact propagation tool, not the design body.

## Scope In

- Reuse existing `OfficeAndCareer` response fields:
  `LastRefusalResponseCommandCode`, `LastRefusalResponseOutcomeCode`, `LastRefusalResponseTraceCode`, and `ResponseCarryoverMonths`.
- Let `SocialMemoryAndRelations` write a delayed durable memory for court-policy local response only from structured `JurisdictionAuthoritySnapshot` values.
- Keep same-month behavior unchanged: the command writes Office structured aftermath, but SocialMemory durable residue appears only in a later monthly pass.
- Keep Application readback projection-only by selecting structured SocialMemory cause keys, memory type, and weight.
- Keep Unity copy-only via existing projected governance/readback fields.

## Scope Out

- No Court module.
- No full court engine, faction AI, full policy economy, event pool, or scripted court process.
- No dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, response cooldown ledger, social-memory ledger, or policy closure ledger.
- No social-memory ledger.
- No Application calculation of policy success.
- No UI/Unity derivation of court-policy outcomes.
- No parsing of `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller path.

## Affected Modules

- `SocialMemoryAndRelations`: owns the later-month durable residue using existing memory records and existing schema `3`.
- `OfficeAndCareer`: remains the owner of county document/report command aftermath and response carryover fields.
- `PublicLifeAndRumor`: remains owner of public interpretation and street reading; no new state in this pass.
- `WorldSettlements`: remains court agenda / mandate pressure source only.
- `Application`: projects SocialMemory residue readback from structured memory cause/type/weight only.
- `Zongzu.Presentation.Unity`: copies projected governance/readback fields only.

## Save/Schema Impact

Target impact: none.

This pass may add a `SocialMemoryKinds` constant and write existing `MemoryRecordState` records with existing fields. It must not add fields, namespaces, root/module schema versions, migrations, save manifest changes, or feature-pack membership.

If implementation proves a new persisted field is required to distinguish court-policy local response from order response reliably, stop and document the schema/migration impact before adding it.

## Determinism Risk

Low. The path reads deterministic Office snapshots during the existing SocialMemory monthly cadence. It adds no randomness, IO, scheduler phase, broad fanout, or UI authority.

## Milestones

1. Inspect existing Office policy residue and public-life response residue paths.
2. Add a SocialMemory court-policy local response residue writer using structured Office snapshot fields only.
3. Prevent the same structured Office court-policy response from being mislabeled as `order.public_life.response`.
4. Extend Application Office-lane residue readback to include the new structured cause prefix.
5. Add focused SocialMemory, integration, architecture, and Unity/presentation coverage where needed.
6. Update topology, boundary, schema, simulation, UI, acceptance, audit, and skill-rationalization docs.
7. Run focused validation, `dotnet build Zongzu.sln --no-restore`, `git diff --check`, and `dotnet test Zongzu.sln --no-build`.

## Tests To Add/Update

- SocialMemory:
  - next-month Office court-policy local response writes durable residue from structured `JurisdictionAuthoritySnapshot` fields only.
  - it does not parse `LastRefusalResponseSummary`, `LastAdministrativeTrace`, `LastPetitionOutcome`, public-life prose, receipt prose, or `DomainEvent.Summary`.
  - off-scope settlements/clans remain untouched.
  - when the response is classed as court-policy local response, it is not also recorded as `order.public_life.response`.
- Integration:
  - v117 affordance command remains same-month SocialMemory-neutral.
  - next month writes `office.policy_local_response...` residue and governance readback names it as SocialMemory residue, not Order/home-household debt.
- Architecture:
  - no Application/UI/Unity authority drift.
  - no forbidden manager/controller, no new schema, no new ledger.
- Unity presentation:
  - existing governance/readback fields are copied only; no Unity calculation.

## Rollback / Fallback Plan

If structured Office fields are insufficient to distinguish court-policy local response from order response, stop before adding state and convert this plan into a schema-aware follow-up that explicitly names the owning module, field, migration, and tests.

## Implementation Evidence

- Added `SocialMemoryKinds.OfficePolicyLocalResponseResidue` as a code constant only.
- Added `SocialMemoryAndRelationsModule.CourtPolicyLocalResponseResidue.cs` to write delayed `office.policy_local_response...` residue from structured `JurisdictionAuthoritySnapshot` response fields, bounded to county-yamen landing / report-reroute trace codes so existing actor-countermove escalation paths stay in their original owner lanes.
- Updated public-life order response residue handling so structured Office court-policy local response is not also mislabeled as `order.public_life.response...`.
- Updated governance readback to project `政策回应余味续接读回` from structured SocialMemory cause/type/weight only.
- Added focused SocialMemory, integration, architecture, and Unity presentation tests for same-month neutrality, later-month durable residue, no prose parsing, no Order/home-household mislabel, no schema drift, and copy-only presentation.
- Updated topology, module boundary, integration, schema, simulation, UI/presentation, acceptance, audit, and skill-rationalization docs.

## Validation Log

- `dotnet build Zongzu.sln --no-restore` — passed, 0 warnings/errors.
- `dotnet test tests/Zongzu.Modules.SocialMemoryAndRelations.Tests/Zongzu.Modules.SocialMemoryAndRelations.Tests.csproj --no-build --filter RunMonth_CourtPolicyLocalResponseResidue_ReadsStructuredOfficeResponseWithoutOrderMislabel` — passed.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue` — passed.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter "EscalatedResidue_LaterOfficeActorsContinueClerkDelayWithoutPlayerCommand|EscalatedResidue_LaterOrderActorsHardenRunnerMisreadWithoutPlayerCommand|EscalatedResidue_LaterFamilyEldersAvoidGuaranteeAndSocialMemoryReadsSamePass|HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome|EscalatedResponseResidue_HardensLater_AndOfficeRepeatPressCarriesClerkDrag|Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue"` — passed.
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-build --filter Court_policy_social_memory_echo_v125_v132_must_remain_structured_and_schema_neutral` — passed.
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_CopiesCourtPolicySocialMemoryEchoWithoutShellAuthority` — passed.
- `git diff --check` — passed.
- `dotnet test Zongzu.sln --no-build` — passed.

Schema/migration result: no persisted fields, no root/module schema bump, no migration, and no save manifest change. If a future court-policy echo needs a persisted discriminator beyond existing `MemoryRecordState.Kind` and `CauseKey`, that future work must stop for a schema/migration plan first.
