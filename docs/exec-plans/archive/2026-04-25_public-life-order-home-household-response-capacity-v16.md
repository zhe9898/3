# Public-Life Order Closure v16: Home-Household Response Capacity Affordance

## Status

Implemented - 2026-04-25

## Framing

This pass keeps the public-life order closure in thin-chain mode and adds a playable capacity/readiness layer to the existing home-household local response lane.

The loop remains rule-driven:

Month N refused / partial public-life order residue -> Month N+1 projected home-household response affordance -> projected household capacity line explains which local response is bearable, risky, or not currently fit -> `PopulationAndHouseholds` resolves any issued local command inside its own namespace -> later `SocialMemoryAndRelations` may read structured aftermath -> projected readback / Unity copy-only shell.

This is not an event-chain and not an event-pool design. `DomainEvent` remains a fact propagation tool when needed, not the design body. No rule may parse `DomainEvent.Summary`, receipt prose, `LastLocalResponseSummary`, SocialMemory summary prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.

## Goal

Make the v12-v15 home-household local response lane more playable before adding thick household rules.

The player should see, from projected read models, whether the household can actually bear a local response:

- `暂缉夜行` should read as useful when migration pressure is high, but risky when labor is already at the floor.
- `赔脚户误读` should read as a real choice only while the household can still carry the new debt; if debt is already over the line, the affordance should warn or disable.
- `遣少丁递信` should read as a labor-spending option; if the household has too few laborers or too many dependents, the affordance should show that sending a runner is not currently fit.

This should make choice timing matter without letting the household lane repair county order, county-yamen documents, elder shame, or durable social memory by itself.

## Scope

In scope:

- Add projection-only command-specific capacity/readiness hints to the three home-household local response affordances.
- Let `IsEnabled` reflect a bounded projected capacity line derived from existing `HouseholdPressureSnapshot` fields:
  - `DebtPressure`
  - `LaborCapacity`
  - `Distress`
  - `MigrationRisk`
  - `DependentCount`
  - `LaborerCount`
  - `Livelihood`
- Add command-time summary/readback wording when a forced local response is issued near or past that household capacity line.
- Keep command-time mutation limited to `PopulationAndHouseholds` household labor, debt, distress, migration, and existing local response trace fields.
- Add integration, architecture, presentation, acceptance, and documentation proof.

Out of scope:

- No new persisted state.
- No schema bump or migration.
- No new `HouseholdId` command target.
- No new household class ladder, rent/tax contract formula, yamen incentive model, runner faction, repeated-response ledger, or thick household economy.
- No change to `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, or `PersonRegistry` authority.
- No UI / Unity / Application computation of command outcome.

## Ownership

- `PopulationAndHouseholds` owns any command-time local household response consequence and may mutate only household labor, debt, distress, migration, and local response trace fields.
- `SocialMemoryAndRelations` remains the only owner of durable shame, fear, favor, grudge, obligation, memory, narrative, and climate state.
- `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` remain owners of county-order repair, yamen/document repair, and elder / household-guarantee response traces.
- Application may build projection-only affordance capacity hints from existing read-model fields, but it may not compute final command outcomes.
- Unity may copy projected affordance / receipt fields only.

## Save And Schema Impact

No persisted shape changes are expected in v16.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root schema change.
- No migration.

Any later household capacity ledger, household target field, commoner status ladder, or new SocialMemory field must bump the owning module schema and add migration tests.

## Determinism Risk

Low.

- Inputs are existing deterministic household state fields and existing SocialMemory query snapshots.
- No random draw is added.
- No scheduler step is added.
- Projection capacity hints are rebuilt from current read models.
- Command outcome remains deterministic for the same state, command, and enabled module set.

## Milestones

1. Create this v16 ExecPlan.
2. Add a small projection-only capacity helper for home-household local response affordances.
3. Apply command-specific capacity hints and enable/disable state to `暂缉夜行`, `赔脚户误读`, and `遣少丁递信`.
4. Add command-time summary tails for forced responses near debt or labor capacity breakpoints.
5. Add integration and architecture tests for projection capacity, command ownership, no summary parsing, and no foreign mutation.
6. Add Unity presentation tests proving copy-only display of capacity/readiness text.
7. Update schema / boundary / simulation / UI / acceptance docs with explicit no-schema-impact evidence.
8. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`

## Tests

- A debt-over-line household projects `赔脚户误读` as disabled or strongly unfit from read models.
- A labor-floor household projects `遣少丁递信` as disabled or strongly unfit from read models.
- A migration-high household still projects `暂缉夜行` as useful when avoidance is the only bearable local move.
- Issuing a forced local response mutates only `PopulationAndHouseholds` at command time.
- Same-command response does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Read models expose the response capacity line without computing final outcome in UI / Unity.
- Architecture tests guard summary parsing, UI/Application outcome computation, forbidden manager/god-controller names, and `PersonRegistry` expansion.

## Rollback Path

Remove the projection capacity helper and summary tails, remove v16 tests, and revert v16 doc notes. Since no schema changes are made, rollback does not require migration changes.

## Implementation Evidence

- `PopulationAndHouseholdsCommandResolver` now derives hard capacity breakpoints from existing household debt/labor/dependent/laborer state and appends `回应承受线` to forced command summaries when a local response is over the line.
- `PresentationReadModelBuilder.PlayerCommands.HomeHouseholdLocalResponse` now projects command-specific `HouseholdLocalResponseAffordanceCapacity` for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`, including availability, cost, and readback strings copied by shell adapters.
- `PublicLifeOrderRefusalResponseRuleDrivenTests.HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome` proves debt-over-line compensation is disabled, migration-high night travel remains usable, labor-floor road messaging is disabled in projection, and a forced road-message command mutates only `PopulationAndHouseholds`.
- Architecture and Unity presentation tests now guard the projection-only capacity helper, existing-field derivation, and copy-only shell display.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors during implementation.
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
- No root save version, module namespace, migration, or save roundtrip test was required for v16.
