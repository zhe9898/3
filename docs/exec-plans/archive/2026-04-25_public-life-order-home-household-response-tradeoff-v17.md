# Public-Life Order Closure v17: Home-Household Response Tradeoff Forecast

## Status

Implemented - 2026-04-25

## Framing

This pass keeps the public-life order closure in thin-chain mode and adds a playable tradeoff forecast layer on top of the v16 home-household response capacity line.

The loop remains rule-driven:

Month N refused / partial public-life order residue -> Month N+1 projected home-household response affordance -> projected tradeoff forecast explains expected benefit, recoil tail, and external boundary -> `PopulationAndHouseholds` resolves any issued local command inside its own namespace -> later `SocialMemoryAndRelations` may read structured aftermath -> projected readback / Unity copy-only shell.

This is not an event-chain and not an event-pool design. `DomainEvent` remains a fact propagation tool when needed, not the design body. No rule may parse `DomainEvent.Summary`, receipt prose, `LastLocalResponseSummary`, SocialMemory summary prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.

## Goal

Make the three low-authority home-household responses more playable before thick household rules exist.

The player should see a bounded "取舍预判" from projected read models:

- `暂缩夜行` expects to reduce night-road / migration risk, but may squeeze household labor and leave external order / yamen / family aftermath unrepaired.
- `凑钱赔脚户` expects to calm runner misunderstanding and street talk, but may sit as new debt when debt pressure is already high.
- `遣少丁递信` expects to clarify road information and runner accounts, but spends scarce household labor and is not county-yamen delivery.

This gives the player a meaningful local choice without letting ordinary households become an all-purpose repair lever.

## Scope

In scope:

- Add projection-only command-specific tradeoff forecast text to the three home-household local response affordances.
- Add command-time summary tails that describe the same tradeoff when a command is issued.
- Reuse existing household read-model fields:
  - `DebtPressure`
  - `LaborCapacity`
  - `Distress`
  - `MigrationRisk`
  - `DependentCount`
  - `LaborerCount`
  - `Livelihood`
- Keep command-time mutation limited to `PopulationAndHouseholds` household labor, debt, distress, migration, and existing local response trace fields.
- Add integration, architecture, presentation, acceptance, and documentation proof.

Out of scope:

- No new persisted state.
- No schema bump or migration.
- No new `HouseholdId` command target.
- No thick household economy, rent/tax formula, repeated-response ledger, commoner pathway ladder, runner faction model, or yamen office formula.
- No change to `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, or `PersonRegistry` authority.
- No UI / Unity / Application computation of command outcomes.

## Ownership

- `PopulationAndHouseholds` owns command-time local household response consequence and may mutate only household labor, debt, distress, migration, and local response trace fields.
- `SocialMemoryAndRelations` remains the only owner of durable shame, fear, favor, grudge, obligation, memory, narrative, and climate state.
- `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` remain owners of county-order repair, yamen/document repair, and elder / household-guarantee response traces.
- Application may build projection-only tradeoff forecast strings from existing read-model fields, but it may not compute final command outcomes.
- Unity may copy projected affordance / receipt fields only.

## Save And Schema Impact

No persisted shape changes are expected in v17.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root schema change.
- No migration.

Any later household tradeoff ledger, repeated-response memory, household target field, or new SocialMemory field must bump the owning module schema and add migration tests.

## Determinism Risk

Low.

- Inputs are existing deterministic household state fields and existing SocialMemory query snapshots.
- No random draw is added.
- No scheduler step is added.
- Projection tradeoff forecasts are rebuilt from current read models.
- Command outcome remains deterministic for the same state, command, and enabled module set.

## Milestones

1. Create this v17 ExecPlan.
2. Add a projection-only tradeoff forecast helper for home-household local response affordances.
3. Apply command-specific benefit / recoil / external-boundary text to `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.
4. Add command-time tradeoff summary tails inside `PopulationAndHouseholds`.
5. Add integration and architecture tests for projection tradeoff, command ownership, no summary parsing, and no foreign mutation.
6. Add Unity presentation tests proving copy-only display of tradeoff forecast text.
7. Update schema / boundary / simulation / UI / acceptance docs with explicit no-schema-impact evidence.
8. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`

## Tests

- Month N+1 affordances expose `取舍预判`, `预期收益`, `反噬尾巴`, and `外部后账` from read models.
- The three choices display distinct tradeoffs:
  - night travel favors migration / night-road relief and warns about labor squeeze;
  - runner compensation favors calming runner misunderstanding and warns about new debt;
  - road message favors road information and warns about scarce labor.
- Issuing a local response mutates only `PopulationAndHouseholds` at command time.
- Same-command response does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Read models and Unity shell expose tradeoff forecast text without computing final outcome in UI / Unity.
- Architecture tests guard summary parsing, UI/Application outcome computation, forbidden manager/god-controller names, and `PersonRegistry` expansion.

## Rollback Path

Remove the projection tradeoff helper and summary tails, remove v17 tests, and revert v17 doc notes. Since no schema changes are made, rollback does not require migration changes.

## Implementation Evidence

- `PresentationReadModelBuilder.PlayerCommands.HomeHouseholdLocalResponse` now projects command-specific `HouseholdLocalResponseTradeoffForecast` for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`, including `取舍预判`, `预期收益`, `反噬尾巴`, `外部后账`, and `取舍读回` text copied through affordances and receipts.
- `PopulationAndHouseholdsCommandResolver` now appends command-time `BuildHouseholdTradeoffSummaryTail` text after existing household texture / capacity tails while still mutating only population-owned household labor, debt, distress, migration, and local response trace fields.
- `PublicLifeOrderRefusalResponseRuleDrivenTests.HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome` now proves the three local choices expose distinct tradeoffs, a forced road-message command keeps command-time mutation in `PopulationAndHouseholds`, and same-command SocialMemory / Order / Office / Family mutation does not occur.
- Architecture and Unity presentation tests now guard the projection-only tradeoff helper, command-time summary tail, summary-parsing boundary, and shell copy-only display of tradeoff fields.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` passed: 9 tests.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests"` passed: 26 tests.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests"` passed: 27 tests.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.

## Save / Migration Evidence

No persisted state was added.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root save version, module namespace, migration, or save roundtrip test was required for v17.
