# Public-Life Order Closure v15: Common Household Response Texture

## Status

Implemented - 2026-04-25

## Framing

This pass keeps the public-life order closure in thin-chain mode with stronger playable texture. It does not start the thick household-class, tax, yamen-incentive, debt-contract, or runner-faction rule layer.

The loop remains rule-driven:

Month N refused / partial public-life order residue -> Month N+1 home-household low-power response affordance -> `PopulationAndHouseholds` resolves local household cost through existing household pressure -> later `SocialMemoryAndRelations` may read structured aftermath -> projected readback / Unity copy-only shell.

This is not an event-chain and not an event-pool design. `DomainEvent` remains a fact propagation tool when needed, not the design body. No rule may parse `DomainEvent.Summary`, receipt prose, `LastLocalResponseSummary`, SocialMemory summary prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.

## Goal

Make ordinary / common household pressure legible enough that the three v12 local responses no longer feel like flat buttons:

- debt-heavy households should make `凑钱赔脚户` read as useful for stopping mouth-talk but costly in new debt;
- labor-thin households should make `暂缩夜行` and `遣少丁递信` read as more fragile because they spend scarce runners / household labor;
- distress-heavy households should make hard follow-up feel socially brittle and more likely to eat shame, fatigue, or local cost;
- migration-prone households should make `暂缩夜行` visibly meaningful as a local avoidance move.

The result should improve play readability without giving the player county-order, yamen, family, or SocialMemory authority through the household lane.

## Scope

In scope:

- Add a bounded `PopulationAndHouseholds` command-time household texture profile derived from existing household fields:
  - `DebtPressure`
  - `LaborCapacity`
  - `Distress`
  - `MigrationRisk`
  - `DependentCount`
  - `LaborerCount`
  - `Livelihood`
- Let that profile adjust only local household response costs and local outcome thresholds for:
  - `RestrictNightTravel`
  - `PoolRunnerCompensation`
  - `SendHouseholdRoadMessage`
- Add projection-only hints on home-household local response affordances / receipts so the player can see why one household response is cheaper, riskier, or more appropriate.
- Add integration, architecture, presentation, acceptance, and documentation proof.

Out of scope:

- No new persisted state.
- No schema bump or migration.
- No new `HouseholdId` command target.
- No new household class ladder, tenant/rent contract model, runner faction, yamen-incentive model, or repeated-response ledger.
- No change to `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, or `PersonRegistry` authority.
- No UI / Unity / Application computation of command outcome.

## Ownership

- `PopulationAndHouseholds` owns command-time household texture resolution and may mutate only household labor, debt, distress, migration, and local response trace fields.
- `SocialMemoryAndRelations` remains the only owner of durable shame, fear, favor, grudge, obligation, memory, narrative, and climate state.
- `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` remain owners of county-order repair, yamen/document repair, and elder / household-guarantee response traces.
- Application and Unity only project and copy read-model fields.

## Save And Schema Impact

No persisted shape changes in v15.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root schema change.
- No migration.
- Existing v12 save/load and migration tests remain the persistence proof for local response trace fields.

Any later household profile ledger, household target field, commoner status ladder, or new SocialMemory field must bump the owning module schema and add migration tests.

## Determinism Risk

Low.

- Inputs are existing deterministic household state fields and existing SocialMemory query snapshots from v14.
- No random draw is added.
- No scheduler step is added.
- Command outcome remains deterministic for the same state, command, and enabled module set.

## Milestones

1. Create this v15 ExecPlan.
2. Add a small household response texture profile in `PopulationAndHouseholdsCommandResolver`.
3. Apply texture profile adjustments to the three existing home-household local response commands.
4. Add projection-only texture hints to home-household affordances / receipts and Unity copy-only test coverage.
5. Add integration and architecture tests for no-touch boundaries and no summary parsing.
6. Update schema / boundary / simulation / UI / acceptance docs with explicit no-schema-impact evidence.
7. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`

## Tests

- A debt-heavy household projects a bounded local response hint explaining that compensation can stop runner / porter misread but adds debt.
- A labor-thin household projects a bounded local response hint explaining that night restriction or sending a young runner spends scarce household labor.
- Issuing the local response mutates only `PopulationAndHouseholds` at command time.
- Same-month response does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- At least two texture paths are covered, including debt-heavy compensation and labor-thin runner/message or night-travel friction.
- Read models expose household texture hints without computing the final outcome in UI / Unity.
- Architecture tests guard summary parsing, UI/Application outcome computation, forbidden manager/god-controller names, and `PersonRegistry` expansion.

## Implementation Evidence

- `PopulationAndHouseholdsCommandResolver` now builds a bounded `HouseholdLocalResponseTextureProfile` from existing population-owned fields: `DebtPressure`, `LaborCapacity`, `Distress`, `MigrationRisk`, `DependentCount`, `LaborerCount`, and `Livelihood`.
- The profile adjusts only the three existing home-household local response commands: `RestrictNightTravel`, `PoolRunnerCompensation`, and `SendHouseholdRoadMessage`.
- Command-time writes remain limited to household labor, debt, distress, migration, and local response trace fields. The resolver does not write `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Application read models project `本户底色` hints from `HouseholdPressureSnapshot`; Unity presentation tests prove those fields are copied only.
- Schema impact remains explicit no-op: no new persisted state, no module schema bump, no root schema bump, no migration, and no command target shape change.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` passed: 8 tests.
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests"` passed: 26 tests.
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests"` passed: 27 tests.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.

## Rollback Path

Remove the household texture helper and its command formula adjustments, remove projection hints and v15 tests, then revert v15 doc notes. Since no schema changes are made, rollback does not require migration changes.
