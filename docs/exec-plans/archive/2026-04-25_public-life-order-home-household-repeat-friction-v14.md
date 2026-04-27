# Public-Life Order Closure v14: Home-Household Local Response Repeat Friction

## Status

Implemented - 2026-04-25

## Framing

This pass still stays in the "thin chain with structural bones" stage. It adds a small playable feedback loop after v13, not a thick household-status or yamen-incentive formula layer.

The loop is rule-driven:

Month N refused / partial public-life order residue -> Month N+1 home-household local response -> Month N+2 `SocialMemoryAndRelations` reads structured local response aftermath -> Month N+3 `PopulationAndHouseholds` reads structured SocialMemory residue when resolving another local household response -> only population-owned household state changes -> projected receipt / shell readback.

This is not an event-chain and not an event-pool design. `DomainEvent` remains a fact propagation tool when needed; it is not the design body. No rule may parse `DomainEvent.Summary`, receipt prose, `LastLocalResponseSummary`, or SocialMemory summary text.

## Goal

Make v13 home-household social-memory residue lightly matter when the player tries another low-power household response:

- a remembered `Relieved` response can make another local response slightly easier to carry;
- a remembered `Contained` response can leave obligation drag;
- a remembered `Strained` or `Ignored` response can make the next household response more likely to eat debt, labor, or shame cost.

The point is not to repair county order or county-yamen authority. It is to make the home-household seat feel playable: local choices leave traces, and those traces shape later local choices inside bounded, owner-owned rules.

## Scope

In scope:

- `PopulationAndHouseholds` command resolver receives optional `ISocialMemoryAndRelationsQueries`.
- `PopulationAndHouseholds` reads active `SocialMemoryEntrySnapshot` records with cause keys under `order.public_life.household_response.{HouseholdId}.`.
- The resolver uses only structured cause-key outcome markers and weights:
  - `Relieved`
  - `Contained`
  - `Strained`
  - `Ignored`
- Command-time mutation remains limited to household labor, debt, distress, migration risk, and `LastLocalResponse*` trace fields.
- Projection affordances may show the current home-household memory readback as a visible hint, but they do not calculate command outcomes.
- Unity shell tests prove projected fields are copied only.
- Architecture tests guard no summary parsing, no SocialMemory writes, no foreign module state mutation, no manager/god-controller drift, and no `PersonRegistry` expansion.

Out of scope:

- No new `HouseholdId` command target.
- No new persisted state.
- No schema bump or migration.
- No thick formulas for zhuhu/kehu status, runner factions, clerk incentives, yamen punishment, household wealth classes, or repeated-response ledgers.
- No SocialMemory command handling.
- No new SocialMemory fields or processed markers.
- No UI / Application / Unity outcome calculation.
- No repair to `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore` authority from a household local response.

## Affected Modules

- `Zongzu.Modules.PopulationAndHouseholds`: reads structured SocialMemory residue and resolves local household repeat friction inside the population command resolver.
- `Zongzu.Application`: projects read-only SocialMemory hints onto local response affordances / receipts.
- `Zongzu.Presentation.Unity`: presentation tests prove the shell copies projected text only.
- Docs and tests.

## Ownership

- `PopulationAndHouseholds` owns the second local response's immediate household mutation and trace.
- `SocialMemoryAndRelations` owns durable residue. It is read as query data only and is not mutated by the command resolver.
- `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` keep their own response authority traces and are not changed by v14.
- Application, read models, and Unity only project and copy fields.

## Save And Schema Impact

No persisted shape changes in v14.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- No root schema change.
- No migration.
- Existing v12 save/load tests remain the persistence proof for local response trace fields; v13 proves SocialMemory uses existing memory/narrative/climate state.

Any future repeated-response ledger, processed marker, household target id, or new SocialMemory field must bump the owning module schema and add migration tests.

## Determinism Risk

Low.

- Inputs are existing query snapshots and structured cause-key outcome markers.
- Memory iteration is ordered by id after query projection.
- No random draw is added.
- Command outcome remains deterministic for the same state, command, and enabled module set.

## Milestones

1. Create this v14 ExecPlan.
2. Pass optional SocialMemory queries into `PopulationAndHouseholdsCommandResolver`.
3. Add bounded home-household residue friction from structured cause keys and weights.
4. Extend local response affordance readback with projected SocialMemory hints.
5. Add integration, architecture, and presentation tests.
6. Update schema / boundary / simulation / UI / acceptance docs with explicit no-schema-impact evidence.
7. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`

## Tests

- Month N+2 v13 residue exists for at least one relieved path and one strained path.
- A later local response command reads that structured SocialMemory residue and mutates only `PopulationAndHouseholds`.
- Same-command handling does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- The relieved/favor path softens or supports a later local response.
- The strained/debt path adds visible local cost or makes a later local response more likely to remain strained.
- Read models expose the SocialMemory hint on the local response affordance / receipt.
- Unity shell displays projected local response readback only.
- Architecture tests guard summary parsing, UI/Application outcome computation, forbidden manager/god-controller names, `PersonRegistry` expansion, and Application/UI/Unity writes to SocialMemory.

## Implementation Evidence

- `PopulationAndHouseholds` now receives optional `ISocialMemoryAndRelationsQueries` in its command context and reads active `SocialMemoryEntrySnapshot` records for `order.public_life.household_response.{HouseholdId}.` cause keys.
- Repeat friction uses structured cause-key outcome markers only (`Relieved`, `Contained`, `Strained`, `Ignored`) plus memory weight. It does not parse `DomainEvent.Summary`, memory summary prose, receipt prose, or `LastLocalResponseSummary`.
- Command-time writes remain population-owned household state and population local-response trace fields only. The command resolver does not write `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Application read models project existing home-household SocialMemory hints onto bounded local-response affordances and receipts. Unity shell tests cover copy-only display of those projected fields.
- Schema impact remains explicit no-op: no new persisted state, no module schema bump, no root schema bump, no migration.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed.
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` passed: 7 tests.
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests"` passed.
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests"` passed: 27 tests.
- `git diff --check` passed before final staging.
- `dotnet test Zongzu.sln --no-build` passed.

## Rollback Path

Remove the optional SocialMemory query from the population command resolver, remove the residue-friction helper and projection hint, remove v14 tests, and revert doc notes. Since no schema changes are made, rollback does not require migration changes.

## Open Questions

- Thick-rule follow-up: should repeated same household responses later need an explicit processed marker to allow multiple memories for the same household/command/outcome across years?
- Thick-rule follow-up: should household status, debt contract type, runner ties, and yamen clerk incentives split the current light friction into stronger rule profiles?
