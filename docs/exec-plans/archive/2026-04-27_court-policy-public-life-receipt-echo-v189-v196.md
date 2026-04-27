# Court Policy Public-Life Receipt Echo V189-V196

## Intent

This ExecPlan covers Chain 8 court-policy public-life receipt echo parity v189-v196. It is a small first rule-density follow-up after v181-v188: public-life command/readback surfaces that already show old court-policy public-reading echo may also say that street/public-life reading does not turn a receipt into a new policy result.

This is still the rule-driven command / aftermath / social-memory / readback loop. It is not an event chain, not a full Court module, not a court engine, not a faction AI layer, not a full policy economy, and not an event pool. `DomainEvent` remains only one fact-propagation tool.

## Boundary

- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns policy windows, county execution, document/report aftermath, and implementation posture.
- `PublicLifeAndRumor` owns public interpretation, notice visibility, road-report texture, and street reading.
- `SocialMemoryAndRelations` owns later durable `office.policy_local_response...` residue.
- Application may assemble projected command readback from structured SocialMemory cause data and current PublicLife scalars.
- Unity shell copies projected `LeverageSummary` / `ReadbackSummary` only.

## Non-Goals

- No Court module.
- No complete court engine.
- No dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, docket ledger, receipt ledger, or public-life receipt echo ledger.
- No new public-life receipt echo ledger.
- No Application-side policy success/failure calculation.
- No UI/Unity court-policy calculation.
- No parsing of `DomainEvent.Summary`, memory summary prose, receipt prose, affordance prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller path.

## Schema / Migration Impact

Target impact: none.

No new persisted field, module schema version, root save version, migration, save manifest entry, projection cache, or ledger is planned. If implementation needs persisted state, stop and document the schema/migration impact before coding.

## Implementation Steps

1. Add a projection-only public-life receipt echo guard to the existing court-policy public-reading echo helper.
2. Keep `PostCountyNotice` and `DispatchRoadReport` reading the existing structured guidance path, so they inherit the guard without new command authority.
3. Prove integration behavior for matching/off-scope settlements and post-suggested-receipt readback.
4. Prove architecture boundaries: no prose parsing, no ledger, no schema, no manager/controller drift.
5. Prove Unity presentation copies projected public-life command fields only.
6. Update docs with schema-neutral and boundary evidence.

## Validation Plan

- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~OfficeCourtRegimePressureChainTests.Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue"`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests.Court_policy_public_life_receipt_echo_v189_v196_must_remain_projection_only_and_schema_neutral"`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests.Compose_CopiesCourtPolicyPublicLifeReceiptEchoAffordanceWithoutShellAuthority"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`
