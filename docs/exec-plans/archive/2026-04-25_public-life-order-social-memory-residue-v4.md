# Public-Life Order Closure v4: SocialMemory-Owned Residue & Refusal Readback

## Goal
Close the v3 projection-only public-life/order leverage loop with one minimal durable rule-driven residue slice:

`public-life/order command -> OrderAndBanditry-owned resolution -> query-visible order aftermath -> SocialMemoryAndRelations-owned residue -> next-month readback -> shell visibility -> acceptance proof`

Primary verbs:
- `FundLocalWatch` / 添雇巡丁
- `SuppressBanditry` / 严缉路匪

The residue belongs to `SocialMemoryAndRelations`, not `OrderAndBanditry`, `Application`, Unity, or narrative projection.

## Scope
In scope:
- SocialMemory reads OrderAndBanditry's structured settlement aftermath query during next-month monthly cadence.
- SocialMemory selects a deterministic local owner clan for the settlement residue and writes existing social-memory structures: `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- Read models expose the new memory entries so public-life receipts/governance docket/Unity shell readback can show durable residue instead of v3-only implied text.
- Tests prove ownership, persistence, readback, shell projection, and boundary guardrails.

Out of scope:
- No new player command.
- No event-driven command redesign.
- No parsing `DomainEvent.Summary`.
- No direct Application/UI/Unity writes to social memory.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller layer.
- No `PersonRegistry` expansion.

## Chosen Cut
Use the user-allowed "query-visible aftermath" seam rather than introducing an event-driven command path. `GameSimulation.IssueModuleCommand` currently constructs a `DomainEventBuffer` for command handling but does not drain or persist command-time events. Changing that plumbing would be wider than v4 needs and would blur this pass's rule-driven shape.

Instead:
1. `OrderAndBanditry` command handling continues to mutate only order-owned settlement state and its existing `LastIntervention*` / `InterventionCarryoverMonths` aftermath fields.
2. During the next monthly cadence, `SocialMemoryAndRelations` reads `IOrderAndBanditryQueries.GetSettlementDisorder()`.
3. If a supported public-life order command has `InterventionCarryoverMonths > 0`, SocialMemory records owner-owned residue against the strongest local clan by deterministic order: prestige, support reserve, clan name, id.
4. Order later consumes/clears carryover in its own monthly pass, so SocialMemory sees the aftermath once.

## Module Impact
- `Zongzu.Contracts`
  - Add public social-memory kind constants for public-order residue.
  - Add read-model bundle exposure for social memory entries.
- `Zongzu.Modules.SocialMemoryAndRelations`
  - Consume order aftermath by query in `RunMonth`.
  - Write only SocialMemory-owned memory/narrative/climate state.
- `Zongzu.Application`
  - Populate `PresentationReadModelBundle.SocialMemories`.
  - Join projected social-memory residue into public-life order readback.
- `Zongzu.Presentation.Unity`
  - No authority change. Existing command receipt/affordance adapters display readback fields.
- Docs/tests
  - Update schema/boundary/integration/simulation/UI/acceptance notes.

## Save And Schema Notes
No new persisted field is planned. The durable residue uses existing SocialMemory schema v3 fields:
- `SocialMemoryAndRelationsState.Memories`
- `ClanNarratives`
- `ClanEmotionalClimates`

Because no state shape changes, no schema version bump or migration step is required. Tests must still prove save/load preserves the new residue entries and existing migration paths remain compatible.

## Determinism Notes
- Owner clan selection is deterministic.
- Residue intensity is a pure function of query-visible order fields.
- No random draw and no narrative text parsing.
- No command-time event path is required for the residue rule.
- Scheduler order is intentional: SocialMemory execution order 400 reads Order execution order 700 carryover before Order clears it in the same monthly pass.

## Acceptance Targets
- Order command still mutates only order-owned state at command time.
- SocialMemory consumes query-visible aftermath and writes only SocialMemory state.
- Month N command produces Month N+1 durable residue visible in `ISocialMemoryAndRelationsQueries`.
- Public-life receipt/governance readback includes social-memory residue.
- Unity shell command receipt displays projected residue/readback only.
- Save/load preserves residue entries.
- Architecture tests protect UI/Application authority boundaries and forbidden manager/god-controller drift.

## Evidence Log
- Implemented query-visible order aftermath fields on `SettlementDisorderSnapshot` so SocialMemory can read structured carryover pressure without parsing receipt text.
- Terminology correction: v4 is documented as a rule-driven residue/readback loop over query-visible aftermath, not an event-chain design.
- Implemented `SocialMemoryAndRelations` public-life order residue handling for `EscortRoadReport`, `FundLocalWatch`, `SuppressBanditry`, `NegotiateWithOutlaws`, and `TolerateDisorder`. The priority v4 cut is covered by `FundLocalWatch` / `添雇巡丁`, with `SuppressBanditry` / `严缉路匪` using the same owner-owned profile seam.
- Exposed `PresentationReadModelBundle.SocialMemories` and joined public-order memories into public-life command receipts plus governance readback as `社会记忆读回`.
- Added tests for SocialMemory owner writes, Month N to Month N+1 integration/readback, save/load preservation, architecture boundary protection, and Unity shell projection-only display.
- Validation passed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Modules.SocialMemoryAndRelations.Tests\Zongzu.Modules.SocialMemoryAndRelations.Tests.csproj --no-build --filter "RunMonth_PublicLifeOrderAftermath_WritesOwnerOwnedResidueFromOrderQuery"`
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "MonthNOrderCommand_ProducesMonthNPlusOneSocialMemoryResidueAndReadback"`
  - `dotnet test tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj --no-build --filter "SaveCodec_RoundtripPreservesPublicLifeOrderSocialMemoryResidue"`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Social_memory_residue_writes_must_stay_inside_social_memory_module"`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "Compose_ProjectsSocialMemoryOrderReadbackWithoutShellAuthority"`
  - `dotnet test Zongzu.sln --no-build`
