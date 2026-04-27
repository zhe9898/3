# Court Policy Suggested Receipt Guard V173-V180

## Intent

This ExecPlan continues Chain 8 court-policy first rule-density work. V173-V180 adds a suggested receipt guard: when a later month already exposes structured court-policy public follow-up residue, the command receipt may remind the player that the receipt is only recovering an already-projected public-policy follow-up, not creating a fresh court result.

This is part of the rule-driven command / aftermath / social-memory / readback loop. It is not an event-chain, not an event pool, and not a full court engine. `DomainEvent` remains only one structured fact propagation tool.

## Scope

- Add projection/readback text to existing `PlayerCommandReceiptSnapshot.ReadbackSummary` paths.
- Reuse existing `SocialMemoryEntrySnapshot.CauseKey`, `JurisdictionAuthoritySnapshot`, and `SettlementPublicLifeSnapshot` data.
- Keep `WorldSettlements`, `OfficeAndCareer`, `PublicLifeAndRumor`, and `SocialMemoryAndRelations` ownership unchanged.
- Prove same-month SocialMemory neutrality for the follow-up command receipt.
- Prove Unity presentation copies the projected receipt field only.

## Non-Goals

- No Court module.
- No full court engine, faction AI, policy economy, or court-process simulation.
- No event pool.
- No new dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, cooldown ledger, docket ledger, suggested-action ledger, or suggested receipt ledger.
- No new suggested receipt ledger.
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

No new persisted field, namespace, module schema version, root schema version, migration, save manifest entry, or persisted projection cache is introduced. If a future version needs durable receipt-state, cooldown, or court-policy follow-up state, work must stop first and document schema/migration impact.

## Implementation Notes

- Add `BuildCourtPolicySuggestedReceiptGuard(...)` in the existing presentation receipt builder.
- The helper reads `SelectOfficePolicyResidue(...)` and `TryReadOfficePolicyLocalResponseResidueCause(...)` only through structured cause keys.
- The helper must return empty when no public-life snapshot, no court-policy process readback, or no structured policy-local-response residue exists.
- Receipt text should include `建议回执防误读`, `只回收已投影的政策公议后手`, `回执不是新政策结果`, `不是Order后账`, and `仍等Office/PublicLife/SocialMemory分读`.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Court_policy_suggested_receipt_guard_v173_v180_must_remain_projection_only_and_schema_neutral`
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter Compose_ProjectsPublicLifeAffordancesAndReceiptsIntoSettlementNodes`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Evidence Log

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings / 0 errors.
- Focused integration test `Chain8_CourtPolicyLocalResponseAffordanceResolvesThroughOfficeLaneWithoutOrderResidue` passed.
- Focused architecture test `Court_policy_suggested_receipt_guard_v173_v180_must_remain_projection_only_and_schema_neutral` passed.
- Focused Unity presentation test `Compose_ProjectsPublicLifeAffordancesAndReceiptsIntoSettlementNodes` passed.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
