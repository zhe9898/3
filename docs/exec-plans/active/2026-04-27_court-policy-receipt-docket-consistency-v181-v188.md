# Court Policy Receipt-Docket Consistency Guard V181-V188

## Intent

This ExecPlan continues Chain 8 court-policy first rule-density work. V181-V188 adds a receipt-to-docket consistency guard so the governance/docket readback and the suggested command receipt tell the same story: a later command receipt only recovers an already-projected court-policy public follow-up, while the docket must not read that receipt as a new policy result.

This remains a rule-driven command / aftermath / social-memory / readback loop. It is not an event-chain, not an event pool, and not a full court engine. `DomainEvent` remains only one structured fact propagation tool.

## Scope

- Add projection/readback text to the existing governance no-loop and docket guidance paths.
- Reuse existing `SocialMemoryEntrySnapshot.CauseKey`, `JurisdictionAuthoritySnapshot`, and `SettlementPublicLifeSnapshot` data.
- Keep `WorldSettlements`, `OfficeAndCareer`, `PublicLifeAndRumor`, and `SocialMemoryAndRelations` ownership unchanged.
- Prove the docket guard aligns with the suggested receipt guard without creating a second result channel.
- Prove Unity presentation copies the projected governance fields only.

## Non-Goals

- No Court module.
- No full court engine, faction AI, policy economy, or court-process simulation.
- No event pool.
- No new dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, docket ledger, receipt ledger, receipt-docket ledger, or docket-consistency ledger.
- No new receipt-docket ledger.
- No Application-side policy success calculation.
- No UI or Unity policy result calculation.
- No prose parsing from `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller path.

## Ownership

- `WorldSettlements` owns only court agenda / mandate pressure source facts.
- `OfficeAndCareer` owns policy windows, county document/report execution, local response aftermath, and implementation posture.
- `PublicLifeAndRumor` owns public interpretation, notice visibility, street reading, and report texture.
- `SocialMemoryAndRelations` owns durable residue only in later month advancement from structured aftermath.
- `Application` routes, assembles, and projects existing facts.
- Unity copies projected fields only.

## Save And Schema

Target impact: none.

No new persisted field, namespace, module schema version, root schema version, migration, save manifest entry, or persisted projection cache is introduced. If a future version needs durable receipt-docket state, cooldown, or court-policy follow-up state, work must stop first and document schema/migration impact.

## Implementation Notes

- Add `BuildCourtPolicyReceiptDocketConsistencyGuard(...)` in the existing governance presentation builder.
- The helper reads `SelectOfficePolicyResidue(...)` and `TryReadOfficePolicyLocalResponseResidueCause(...)` only through structured cause keys.
- The helper returns empty when no public-life snapshot, no court-policy process readback, or no structured policy-local-response residue exists.
- Readback text includes `回执案牍一致防误读`, `回执只回收已投影的政策公议后手`, `案牍不把回执读成新政策结果`, `不是Order后账`, and `仍等Office/PublicLife/SocialMemory分读`.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Court_policy_receipt_docket_consistency_v181_v188_must_remain_projection_only_and_schema_neutral`
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_ProjectsGovernanceMomentumIntoGreatHallAndDeskSandbox`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Evidence Log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings / 0 errors.
- Focused integration test `Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue` passed.
- Focused architecture test `Court_policy_receipt_docket_consistency_v181_v188_must_remain_projection_only_and_schema_neutral` passed.
- Focused Unity presentation test `Compose_ProjectsGovernanceMomentumIntoGreatHallAndDeskSandbox` passed.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
