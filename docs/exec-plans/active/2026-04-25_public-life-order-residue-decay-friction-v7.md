# Public-Life Order Closure v7: Residue Decay, Repeat Friction & Actor Countermove

## Status

Complete - 2026-04-25

## Framing

This plan extends the v6 rule-driven command / residue / social-memory / response loop. It is not an event-pool or event-centered design, and it should not be described as an event-chain. `DomainEvent` remains a deterministic fact propagation tool, not the design center.

v6 made refusal and partial public-life order after账 respondable. v7 makes the structured response residue continue to live after the first readback:

Month N response aftermath -> Month N+2 SocialMemory-owned residue -> later monthly softening / hardening -> later owner-module command friction -> projected readback.

## Goal

Give `Repaired`, `Contained`, `Escalated`, and `Ignored` response aftermath a bounded afterlife without moving authority into Application, UI, Unity, or SocialMemory command handlers.

The main player-facing cases remain:

- `添雇巡丁` refusal / half-landing followed by `补保巡丁`, `赔脚户误读`, `请族老解释`, or `押文催县门`
- `严缉路匪` refusal / backlash followed by `暂缓强压`, `改走递报`, or `押文催县门`

## Scope

In scope:

- SocialMemory-owned monthly drift for existing public-life response memories.
- Repaired/contained residue can soften shame/fear/grudge and leave trust/obligation.
- Escalated/ignored residue can harden fear/shame/bitterness and make the next response harder.
- OrderAndBanditry, OfficeAndCareer, and FamilyCore may read structured SocialMemory snapshots while resolving later commands, then mutate only their own state.
- Public-life, governance, and family readback should surface whether the after账 is easing, still held, hardening, or left sour.

Out of scope:

- New manager/controller classes.
- New `PersonRegistry` fields.
- UI or Unity rule computation.
- Parsing `DomainEvent.Summary`, `LastInterventionSummary`, `LastRefusalResponseSummary`, or receipt prose.
- New persisted schema fields unless implementation proves they are unavoidable.

## Ownership

- `SocialMemoryAndRelations` owns durable residue drift and may write only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- `OrderAndBanditry` owns road watch / route pressure repair command results and order-side response trace.
- `OfficeAndCareer` owns county-yamen催办, 文移落地, and 胥吏拖延 command results and office-side response trace.
- `FamilyCore` owns 族老解释 / 本户担保 command results and family-side response trace.
- Application routes commands and builds read models only.
- Unity shell copies projected fields only.

## Save And Schema Impact

Expected impact: no new persisted state.

The v7 afterlife should reuse existing SocialMemory schema 3 fields:

- `MemoryRecordState.Weight`
- `MemoryRecordState.MonthlyDecay`
- `MemoryRecordState.LifecycleState`
- `MemoryRecordState.CauseKey`
- `ClanNarrativeState.GrudgePressure`
- `ClanNarrativeState.FearPressure`
- `ClanNarrativeState.ShamePressure`
- `ClanNarrativeState.FavorBalance`
- `ClanEmotionalClimateState`

If a new persisted field becomes necessary, this plan must be revised before code lands with:

- owning module schema bump
- migration
- save roundtrip and legacy migration tests
- updates to `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md`

## Milestones

1. Add SocialMemory response-residue drift.
   - Skip current-month response memories so Month N+2 recording remains distinct from later decay/hardening.
   - Repaired softens; contained carries obligation; escalated and ignored harden.

2. Add structured repeat-friction read helpers in owner modules.
   - Read `SocialMemoryEntrySnapshot.CauseKey`, `Type`, and `Weight`.
   - Never parse summaries.
   - Filter through local clan ownership for settlement-facing modules.

3. Apply bounded command friction.
   - Order repair receives support or drag from prior response residue.
   - Office yamen press sees repaired trust or hardened clerk drag.
   - Family elder explanation sees prior trust or public shame drag.

4. Extend projection readback.
   - Public-life and governance readbacks already include SocialMemory entries; update wording/tests so the drift state is visible.
   - Unity shell remains projection-only.

5. Add proof.
   - Focused tests for repaired softening and escalated/ignored hardening.
   - Command-time tests that later commands mutate only the owning module.
   - Architecture tests for summary parsing and boundary drift.
   - Full build and test run.

## Acceptance Proof Targets

- v6 response memory appears in Month N+2 and later changes by SocialMemory rules only.
- Repaired path shows weight/fear/shame/grudge softening and trust/favor readback.
- Escalated or ignored path shows hardening or clerk/social drag.
- Later Order / Office / Family commands read structured SocialMemory residue and mutate only their owning module at command time.
- Same-month commands still do not write SocialMemory.
- Read models expose the changed social residue.
- No schema bump unless a persisted field is added.
- `dotnet build Zongzu.sln --no-restore`
- focused tests
- `dotnet test Zongzu.sln --no-build`

## Implementation Evidence

- Added `SocialMemoryAndRelationsModule.PublicLifeOrderResponseDrift.cs` so response memories age after their creation month through SocialMemory-owned memory, narrative, and climate fields only.
- Added structured SocialMemory response-friction readers to `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` command resolvers. They read cause keys and weights, not summary prose, and they mutate only their owning module during command resolution.
- Added v7 integration coverage in `PublicLifeOrderResidueDecayFrictionRuleDrivenTests`: repaired residue later softens and helps repeat order repair; escalated residue later hardens and drags repeat yamen pressure.
- Added architecture guards for drift/reader summary parsing, plus existing forbidden-manager, PersonRegistry, and SocialMemory ownership guards.
- Updated schema, namespace, boundary, simulation, relationship, UI, acceptance, skill-matrix, and alignment docs. v7 adds no persisted fields and no migration.

## Validation Log

- `git diff --check`
- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests|FullyQualifiedName~PublicLifeOrderResidueDecayFrictionRuleDrivenTests|FullyQualifiedName~PublicLifeOrderSocialMemoryResidueRuleDrivenTests"`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Social_memory_public_life_order_response_drift_must_not_parse_response_or_event_summary|FullyQualifiedName~Public_life_response_friction_readers_must_not_parse_social_memory_summary|FullyQualifiedName~Source_must_not_define_forbidden_manager_or_god_controller_types|FullyQualifiedName~PersonRecord_must_remain_identity_only|FullyQualifiedName~Social_memory_residue_writes_must_stay_inside_social_memory_module"`
- `dotnet test tests/Zongzu.Persistence.Tests/Zongzu.Persistence.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrder|FullyQualifiedName~RefusalResponse|FullyQualifiedName~OrderAndBanditry_DefaultMigrationPipeline|FullyQualifiedName~OfficeAndCareer_DefaultMigrationPipeline|FullyQualifiedName~FamilyCore_DefaultMigrationPipeline"`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~Compose_ProjectsPublicLifeResponseReadbackWithoutShellAuthority|FullyQualifiedName~Compose_ProjectsSocialMemoryOrderReadbackWithoutShellAuthority|FullyQualifiedName~Compose_ProjectsPublicLifeAffordancesAndReceiptsIntoSettlementNodes"`
- `dotnet test Zongzu.sln --no-build`
