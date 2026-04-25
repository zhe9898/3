# Public-Life Order Closure v19: Home-Household Follow-Up Affordance Readback

## Status

Implemented and validated on `codex/public-life-order-leverage-v3`.

## Framing

This pass keeps the public-life order closure in thin-chain mode and adds a projected follow-up affordance layer after the v12-v18 home-household local response lane.

The loop remains rule-driven:

1. Month N public-life order refusal / partial residue is owned by `OrderAndBanditry`.
2. Month N+1 projected read models expose bounded public-life / governance / family / home-household response affordances.
3. The selected command is resolved by its owning module.
4. `PopulationAndHouseholds` local responses may mutate only household labor, debt, distress, migration, and structured local response trace fields.
5. Receipts show short-term local consequence.
6. The next projected command window shows whether the household should repeat, switch, or cool down its own local response.
7. Month N+2 `SocialMemoryAndRelations` reads structured aftermath and adjusts durable memory / narrative / emotional climate residue.

This is not an event-chain, event-pool, or UI-authored outcome design. `DomainEvent` remains one fact propagation tool, not the design object.

## Why This Slice

v18 made command receipts say what the household locally eased, what got squeezed, and which外部后账 remained outside household authority. The next thin-chain gap is the following command window: a player should see whether repeating `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信` is light follow-up, risky overpressure, or better treated as a switch to another local move.

This improves playability without turning ordinary households into county-order repair owners or adding thick household economy / status rules.

## Scope

In scope:

- Add projection-only follow-up hints to home-household local response affordances.
- Distinguish:
  - `续接提示`
  - `换招提示`
  - `冷却提示`
  - `续接读回`
- Shape the hints from existing `HouseholdPressureSnapshot` fields plus structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`.
- Cover the repeated-command and switched-command paths for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.
- Prove Unity shell copies these projected fields only.
- Prove `SocialMemoryAndRelations` does not parse the new follow-up prose.
- Update docs and acceptance evidence with explicit no-schema-impact notes.

Out of scope:

- No new persisted state.
- No schema bump or migration.
- No new `HouseholdId` command target.
- No cooldown ledger, repeated-response counter, thick household economy, tax/rent formula, commoner pathway ladder, runner faction model, yamen office formula, or route AI.
- No change to `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, or `PersonRegistry` authority.
- No Application / UI / Unity command outcome computation.

## Boundary Rules

- `PopulationAndHouseholds` remains the owner of `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.
- The v19 follow-up helper may read projected household snapshot fields; it must not query modules, issue commands, parse summaries, or mutate state.
- Follow-up hints may advise repeat, switch, or cool down, but they are not a saved cooldown ledger and do not replace module-owned command resolution.
- `SocialMemoryAndRelations` still owns durable shame/fear/favor/grudge/obligation residue and must write only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- Unity may copy projected affordance / receipt fields only.

## Save And Schema Impact

No persisted shape changes are expected in v19.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root schema change.
- No migration.

Any future cooldown ledger, repeated-response counter, household target field, route-specific carryover field, or new SocialMemory field must bump the owning module schema and add migration / save roundtrip tests.

## Determinism Risk

Low. The intended implementation is projection-only and derives text from already deterministic snapshots and structured local-response codes. No scheduler cadence, random selection, persisted state, save manifest, or monthly authority order changes are planned.

## Milestones

1. Create this v19 ExecPlan.
2. Add a projection-only `HouseholdLocalResponseFollowUpHint` helper in the home-household local response read-model builder.
3. Add follow-up hints to availability / leverage / cost / readback fields for repeated and switched local response choices.
4. Add integration assertions proving the next projected affordance exposes follow-up and cooldown hints while command-time mutation remains confined to `PopulationAndHouseholds`.
5. Add architecture tests proving the projection helper exists and `SocialMemoryAndRelations` does not parse new follow-up prose.
6. Add Unity presentation tests proving shell copy-only display of projected follow-up affordance text.
7. Update schema / boundary / simulation / UI / acceptance docs with explicit no-schema-impact evidence.
8. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`
9. Commit and push the branch.

## Acceptance Notes

- v5/v12-v18 residue and local-response traces continue to drive readback through structured state, not summary parsing.
- Affordances must say whether the next local response is repeat, switch, or cooldown pressure.
- The hints must still say external after-accounts remain outside household authority when relevant.
- Same-month local response commands must not mutate `SocialMemoryAndRelations`.
- Month N+2 SocialMemory behavior remains owned by existing v13-v14 paths.
- Unity shell must display projected fields only.

## Rollback Path

Remove the projection follow-up helper, affordance text joins, v19 tests, and v19 doc notes. Since no schema changes are made, rollback does not require migration changes.

## Implementation Evidence

- `PresentationReadModelBuilder.PlayerCommands.HomeHouseholdLocalResponse.cs` now builds `HouseholdLocalResponseFollowUpHint` for local household response affordances.
- Affordances now expose `续接提示`, `换招提示`, `冷却提示`, and `续接读回` when a household has existing structured `LastLocalResponse*` aftermath.
- The helper derives text from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode` only.
- Integration tests prove repeated and switched local response affordances expose follow-up/cooldown hints while same-month command mutation remains confined to `PopulationAndHouseholds`.
- Architecture tests guard that the projection helper exists and that `SocialMemoryAndRelations` does not parse `续接提示`, `换招提示`, `冷却提示`, or `续接读回`.
- Unity presentation tests prove shell copy-only display of the projected follow-up affordance fields.
- Boundary, integration, relationship, schema, simulation, UI, acceptance, skill-matrix, and alignment docs now record v19 as no-schema-impact readback enrichment.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` passed: 9 tests.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests"` passed: 26 tests.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests"` passed: 27 tests.
- `git diff --check` passed with a line-ending normalization warning for `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`.
- `dotnet test Zongzu.sln --no-build` passed.

## Schema / Migration Evidence

No persisted state was added.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root save version, module namespace, migration, save manifest update, cooldown ledger, repeated-response counter, or legacy migration test was required for v19.
