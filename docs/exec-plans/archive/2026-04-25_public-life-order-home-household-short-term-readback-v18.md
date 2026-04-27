# Public-Life Order Closure v18: Home-Household Short-Term Consequence Readback

## Status

Implemented and validated on `codex/public-life-order-leverage-v3`.

## Framing

This pass keeps the public-life order closure in thin-chain mode and adds a short-term consequence readback layer after the v12-v17 home-household local response lane.

The loop remains rule-driven:

1. Month N public-life order refusal / partial residue is owned by `OrderAndBanditry`.
2. Month N+1 projected read models expose bounded public-life / governance / family / home-household response affordances.
3. The selected command is resolved by its owning module.
4. `PopulationAndHouseholds` local responses may mutate only household labor, debt, distress, migration, and structured local response trace fields.
5. Month N+1 receipts show the short-term local consequence from projected read-model fields.
6. Month N+2 `SocialMemoryAndRelations` reads structured aftermath and adjusts durable memory / narrative / emotional climate residue.

This is not an event-chain, event-pool, or UI-authored outcome design. `DomainEvent` remains one fact propagation tool, not the design object.

## Why This Slice

v17 made the local response lane more playable before command issue by showing `取舍预判`, `预期收益`, `反噬尾巴`, and `外部后账`. The next thin-chain gap is after the command resolves: the player can see `本户已缓` / `本户暂压` / `本户吃紧`, but the receipt should say more plainly what was locally eased, what got squeezed, and which county / order / family / SocialMemory after-account is still outside household authority.

This gives ordinary households a readable stake without turning them into county-order repair owners or adding thick household economy rules.

## Scope

In scope:

- Add projection-only short-term consequence readback text to home-household local response receipts.
- Distinguish:
  - `短期后果：缓住项`
  - `短期后果：挤压项`
  - `短期后果：仍欠外部后账`
- Shape the text from existing `HouseholdPressureSnapshot` fields plus structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`.
- Cover `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.
- Prove Unity shell copies these projected fields only.
- Prove `SocialMemoryAndRelations` does not parse the new readback prose.
- Update docs and acceptance evidence with explicit no-schema-impact notes.

Out of scope:

- No new persisted state.
- No schema bump or migration.
- No new `HouseholdId` command target.
- No thick household economy, tax/rent formula, repeated-response ledger, commoner pathway ladder, runner faction model, yamen office formula, or route AI.
- No change to `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, or `PersonRegistry` authority.
- No Application / UI / Unity command outcome computation.

## Boundary Rules

- `PopulationAndHouseholds` remains the owner of `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.
- `OrderAndBanditry` still owns refusal / partial public-life order authority and route-watch repair.
- `OfficeAndCareer` still owns county-yamen催办, 文移落地, and 胥吏拖延 handling.
- `FamilyCore` still owns族老解释 and本户担保 repair surfaces.
- `SocialMemoryAndRelations` still owns durable shame/fear/favor/grudge/obligation residue and must write only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- The v18 readback helper may read projected household snapshot fields; it must not query modules, issue commands, parse summaries, or mutate state.
- Unity may copy projected receipt fields only.

## Save And Schema Impact

No persisted shape changes are expected in v18.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root schema change.
- No migration.

Any future short-term consequence ledger, repeated-response counter, household target field, route-specific carryover field, or new SocialMemory field must bump the owning module schema and add migration / save roundtrip tests.

## Determinism Risk

Low. The intended implementation is projection-only and derives text from already deterministic snapshots and structured local-response codes. No scheduler cadence, random selection, persisted state, save manifest, or monthly authority order changes are planned.

## Milestones

1. Create this v18 ExecPlan.
2. Add a projection-only `HouseholdLocalResponseShortTermConsequenceReadback` helper in the home-household local response read-model builder.
3. Add receipt readback for `缓住项`, `挤压项`, and `仍欠外部后账`.
4. Add integration assertions proving response receipts expose the short-term readback and command-time mutation remains confined to `PopulationAndHouseholds`.
5. Add architecture tests proving the projection helper exists and `SocialMemoryAndRelations` does not parse new readback prose.
6. Add Unity presentation tests proving shell copy-only display of projected short-term consequence text.
7. Update schema / boundary / simulation / UI / acceptance docs with explicit no-schema-impact evidence.
8. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`
9. Commit and push the branch.

## Acceptance Notes

- v5/v12-v17 residue and local-response traces continue to drive readback through structured state, not summary parsing.
- Receipts must say which short-term local item was eased and which local cost was squeezed.
- Receipts must still say external after-accounts remain outside household authority.
- Same-month local response commands must not mutate `SocialMemoryAndRelations`.
- Month N+2 SocialMemory behavior remains owned by existing v13-v14 paths.
- Unity shell must display projected receipt fields only.

## Rollback Path

Remove the projection readback helper, receipt text joins, v18 tests, and v18 doc notes. Since no schema changes are made, rollback does not require migration changes.

## Implementation Evidence

- `PresentationReadModelBuilder.PlayerCommands.HomeHouseholdLocalResponse.cs` now builds `HouseholdLocalResponseShortTermConsequenceReadback` for local household receipts.
- Receipts now expose `短期后果：缓住项`, `短期后果：挤压项`, and `短期后果：仍欠外部后账` for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.
- The helper derives text from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode` only.
- Integration tests prove the road-message receipt exposes the short-term readback while same-month command mutation remains confined to `PopulationAndHouseholds`.
- Architecture tests guard that the projection helper exists and that `SocialMemoryAndRelations` does not parse `短期后果`, `缓住项`, `挤压项`, or `仍欠外部后账`.
- Unity presentation tests prove shell copy-only display of the projected short-term consequence receipt fields.
- Boundary, integration, relationship, schema, simulation, UI, acceptance, skill-matrix, and alignment docs now record v18 as no-schema-impact readback enrichment.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` passed: 9 tests.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests"` passed: 26 tests.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests"` passed: 27 tests.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.

## Schema / Migration Evidence

No persisted state was added.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root save version, module namespace, migration, save manifest update, or legacy migration test was required for v18.
