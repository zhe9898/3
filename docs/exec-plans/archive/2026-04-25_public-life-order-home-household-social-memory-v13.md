# Public-Life Order Closure v13: Home-Household Local Response Social-Memory Readback

## Status

Implemented - 2026-04-25

## Framing

This pass keeps the current work in the "thin chain with structural bones" stage. It does not add thick social formulas yet.

The loop is rule-driven:

Month N refused / partial public-life order residue -> Month N+1 projected ordinary-household pressure -> home-household local response resolved by `PopulationAndHouseholds` -> structured local response aftermath -> Month N+2 `SocialMemoryAndRelations` reads that structured aftermath -> durable shame / fear / favor / grudge / obligation residue appears in readback.

This is not an event-chain and not an event-pool design. `DomainEvent` remains a fact propagation tool when needed; it is not the design body, and `DomainEvent.Summary` is not rule input.

## Goal

Make v12 home-household local responses matter after the immediate household pressure mutation:

- `RestrictNightTravel` / 暂缩夜行 can become remembered relief or contained household obligation.
- `PoolRunnerCompensation` / 凑钱赔脚户 can become remembered obligation or shame/debt pressure.
- `SendHouseholdRoadMessage` / 遣少丁递信 can become remembered containment or strain.

The new readback should let the player see that a low-power household response did not repair county order or yamen/family authority, but it did change how the local clan and household are remembered.

## Scope

In scope:

- `SocialMemoryAndRelations` reads structured `PopulationAndHouseholds` query fields:
  - `LastLocalResponseCommandCode`
  - `LastLocalResponseOutcomeCode`
  - `LastLocalResponseTraceCode`
  - `LastLocalResponseCommandLabel`
- `SocialMemoryAndRelations` writes only existing SocialMemory-owned collections:
  - `Memories`
  - `ClanNarratives`
  - `ClanEmotionalClimates`
- Add additive `SocialMemoryKinds` constants for home-household local response residue.
- Add projected receipt readback for the social-memory result on home-household local response receipts.
- Add integration and architecture tests proving command-time isolation and next-month SocialMemory readback.
- Update schema / boundary / simulation / UI / acceptance docs with explicit no-schema-impact evidence.

Out of scope:

- No new command target field such as `HouseholdId`.
- No new persisted state.
- No schema bump or migration.
- No thick formulas for household class, local actor motives, yamen incentives, or long-run strategy curves.
- No repair to `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore` authority from a household local response.
- No SocialMemory command handling.
- No parsing `DomainEvent.Summary`, receipt summaries, `LastInterventionSummary`, `LastRefusalResponseSummary`, or `LastLocalResponseSummary`.
- No UI / Unity authority calculation.
- No new manager, god controller, or `PersonRegistry` expansion.

## Affected Modules

- `Zongzu.Modules.SocialMemoryAndRelations`: reads structured household local response aftermath and writes existing memory/narrative/climate state.
- `Zongzu.Contracts`: additive memory-kind constants only.
- `Zongzu.Application`: projected readback joins existing household response receipts with existing SocialMemory snapshots.
- `Zongzu.Presentation.Unity`: tests prove shell copies projected fields only.
- Docs and tests.

## Ownership

- `PopulationAndHouseholds` continues to own the local response command trace and immediate household labor / debt / distress / migration mutation.
- `SocialMemoryAndRelations` owns durable social residue after the next monthly pass.
- `OrderAndBanditry` continues to own refusal / partial public-life order authority trace.
- `OfficeAndCareer` and `FamilyCore` continue to own their own refusal-response authority traces.
- Application, read models, and Unity only project / copy fields.

## Save And Schema Impact

No persisted shape changes in v13.

- `PopulationAndHouseholds` remains schema `3`; v13 reads the v12 local-response fields already stored there.
- `SocialMemoryAndRelations` remains schema `3`; v13 writes existing `MemoryRecordState`, `ClanNarrativeState`, and `ClanEmotionalClimateState` records.
- No root schema change.
- No migration.
- Existing v12 save/load tests remain the persistence proof for the local response trace fields.

Because `PopulationAndHouseholds` runs before `SocialMemoryAndRelations` in the current monthly order and decrements `LocalResponseCarryoverMonths` first, v13 uses structured command / outcome / trace fields plus SocialMemory cause-key de-duplication rather than making carryover the only read gate.

## Determinism Risk

Low.

- Inputs are query snapshots and structured codes.
- Household iteration is ordered by settlement, clan, household id, command, outcome, and trace.
- Duplicate writes are guarded by deterministic cause keys.
- No random draw is added.
- Projection readback remains rebuilt from authoritative state.

## Milestones

1. Create this v13 ExecPlan.
2. Add additive SocialMemory kind constants.
3. Add a `SocialMemoryAndRelations` partial reader for home-household local response aftermath.
4. Invoke the reader in the monthly SocialMemory pass.
5. Extend home-household local response receipt readback with SocialMemory residue when present.
6. Add integration / architecture / presentation tests.
7. Update docs and acceptance evidence.
8. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`

## Tests

- v12 local response command mutates only `PopulationAndHouseholds` at command time.
- Same-month local response does not mutate `SocialMemoryAndRelations`.
- Month N+2 `SocialMemoryAndRelations` reads structured local response aftermath and writes durable residue.
- At least two paths are covered, including relieved and strained.
- SocialMemory writes only existing memory/narrative/climate state for this pass.
- Read models expose the adjusted social-memory readback on the local response receipt.
- Unity shell displays the projected readback only.
- Architecture tests guard summary parsing, foreign state mutation, manager/god-controller drift, and `PersonRegistry` expansion.

## Implementation Evidence

- Added `SocialMemoryAndRelationsModule.HomeHouseholdLocalResponseResidue.cs` as the SocialMemory-owned reader for structured `PopulationAndHouseholds` local-response aftermath.
- Added additive `SocialMemoryKinds` constants only; no persisted shape, module schema, root schema, or migration changed.
- `RunMonth` now invokes the home-household local response reader after current public-life order/response residue readers and before existing response-residue drift.
- Home-household local response receipts now join projected active SocialMemory snapshots by cause key and copy the resulting readback into `ReadbackSummary`.
- Architecture tests guard that the new reader does not parse `LastLocalResponseSummary`, receipt prose, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastRefusalResponseSummary`.

Validation:

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration tests passed: `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` -> 6 passed.
- Focused architecture tests passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests"` -> 24 passed before docs, 25 passed in full suite after adding the v13 guard.
- Focused Unity presentation tests passed: `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests"` -> 27 passed before final full suite; full suite later reports 30 presentation tests passed.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.

## Rollback Path

Remove the new SocialMemory partial reader, the receipt readback helper, additive memory-kind constants, v13 tests, and doc notes. Since no schema changes are made, rollback does not require migration changes.

## Open Questions

- Thick-rule follow-up: should repeated same household responses later create repeat memories with a response date or an explicit processed marker?
- Thick-rule follow-up: should household status, zhuhu/kehu pressure, runner ties, and yamen clerk incentives change outcome profiles?
