# Family Command Autonomy Seam

> Status: Complete
> Started: 2026-04-23
> Skills: `zongzu-game-design`, `zongzu-ancient-china`

## Goal

Close the first part of the current command-routing gap by moving family command consequence logic out of `PlayerCommandService` and into `FamilyCore`, while letting the newly implemented `SocialMemoryAndRelations` pressure-tempering kernel shape command friction.

This is not a new player authority lane. Commands remain bounded intents. `FamilyCore` owns family state, and `SocialMemoryAndRelations` is read only through queries for clan emotional climate and person tempering. The player still cannot directly edit emotions, force adult compliance, or bypass clan/institution pressure.

## Scope In

- Add a `FamilyCore`-owned command resolver for current family commands:
  - `ArrangeMarriage`
  - `DesignateHeirPolicy`
  - `SupportNewbornCare`
  - `SetMourningOrder`
  - `SupportSeniorBranch`
  - `OrderFormalApology`
  - `PermitBranchSeparation`
  - `SuspendClanRelief`
  - `InviteClanEldersMediation`
  - `InviteClanEldersPubliclyBroker`
- Keep `PlayerCommandService` as a thin router for this slice.
- Read optional `IPersonRegistryQueries` for alive / age facts.
- Read optional `ISocialMemoryAndRelationsQueries` for:
  - clan climate pressure,
  - adult restraint / hardening / bitterness / volatility among clan-linked adults.
- Make social residue influence command outcomes as deterministic modifiers, not random moods.
- Add module-level tests proving emotional pressure can bend command resolution.
- Update docs to mark the family command seam as partially closed.

## Scope Out

- No new global command manager.
- No new schema in this slice.
- No DomainEvent-driven command loop.
- No direct writes from `FamilyCore` into `SocialMemoryAndRelations`.
- No Office / Order / Warfare command migration in this slice.
- No UI authority changes.

## Affected Modules

- `Zongzu.Modules.FamilyCore`
  - owns family command resolution formulas and state mutation.
- `Zongzu.Application`
  - remains the router and refreshes replay hash after accepted family commands.
- `Zongzu.Contracts`
  - no new contracts expected; uses existing player command DTOs and query interfaces.
- Tests
  - add focused `FamilyCore` command resolver tests,
  - keep existing integration `PlayerCommandService` tests green.
- Docs
  - `MODULE_INTEGRATION_RULES.md`
  - `PLAYER_SCOPE.md`
  - `RELATIONSHIPS_AND_GRUDGES.md`
  - `ACCEPTANCE_TESTS.md`

## Schema / Save Impact

- No schema bump.
- Existing `FamilyCore` receipt fields remain the authoritative command receipt storage for this slice.
- Existing `SocialMemoryAndRelations` schema `3` is queried but not mutated.

## Query / Command / DomainEvent

### Queries

- `FamilyCoreCommandResolver` may receive:
  - `IPersonRegistryQueries?`
  - `ISocialMemoryAndRelationsQueries?`

Both are read-only.

### Commands

- Family commands remain the same contract names already declared by `FamilyCore.AcceptedCommands`.
- The application service routes the existing `PlayerCommandRequest` into the owning module resolver.

### DomainEvents

- No new domain events in this slice.
- Command receipts remain stored as `FamilyCore` last-command fields and surfaced through read models.
- Later work may add formal command receipt DomainEvents, but this slice does not add an event-pool loop.

## Determinism

- No random input, wall clock, IO, or unordered iteration.
- Clan selection ordered by prestige then typed id.
- Adult tempering aggregation ordered by person id and clamped to integer bands.
- Missing SocialMemory query means neutral modifiers.

## Historical / Design Grounding

Northern Song-style household / lineage commands should feel like social negotiation, not a button press:

- strong grief, shame, anger, or volatility makes commands land with backlash;
- trust, obligation, restraint, mediation, and prestige help commands land cleanly;
- hardening can make harsh commands easier but reconciliation harder;
- adult tempering matters because adults may delay, resist, reinterpret, or exploit clan decisions.

## Milestones

### Milestone 1 - Plan and Code Boundary

Status: Complete

- Add this ExecPlan.
- Inspect current family command routing and tests.
- Define a small module-owned resolver seam.

### Milestone 2 - FamilyCore-Owned Resolver

Status: Complete

- Add resolver/context/modifier types in `Zongzu.Modules.FamilyCore`.
- Move current family command formulas into module ownership.
- Keep `PlayerCommandService` as thin routing glue.

### Milestone 3 - SocialMemory Autonomy Modifiers

Status: Complete

- Aggregate clan climate and adult person tempering.
- Apply deterministic modifiers to reconciliation, separation, relief sanction, mourning, heir, care, and marriage profiles.
- Keep missing SocialMemory neutral.

### Milestone 4 - Tests

Status: Complete

- Add focused FamilyCore tests for:
  - neutral SocialMemory preserves baseline command behavior,
  - high volatility / bitterness adds backlash or weakens mediation,
  - trust / restraint improves mediation or apology,
  - missing SocialMemory query is neutral.
- Run existing integration command tests.

### Milestone 5 - Docs, Validation, Commit, Push, PR

Status: Complete

- Update integration / player-scope / social-memory docs.
- Run build and relevant tests.
- Commit, push, and update PR.

## Open Questions

- None blocking. The repo already documents the command-routing gap and says domain consequence logic should move into owning modules.

## Progress Log

- 2026-04-23: Started after completing the pressure-tempering kernel. Skills and project docs point to FamilyCore command handling as the next clean seam because it connects adult autonomy, clan memory, and bounded player leverage without adding a global manager.
- 2026-04-23: Added `FamilyCoreCommandResolver` inside `Zongzu.Modules.FamilyCore`; `PlayerCommandService` now routes family commands through the owning module and only refreshes replay hash after accepted mutation.
- 2026-04-23: Removed the old family conflict/lifecycle formula files from `Zongzu.Application`, leaving family command consequence formulas module-owned.
- 2026-04-23: Added SocialMemory read-only modifiers for clan climate and adult pressure-tempering residues. Strong bitterness/volatility weakens mediation; trust/restraint/obligation strengthen apology or mediation; missing SocialMemory remains neutral.
- 2026-04-23: Added focused FamilyCore command resolver tests and verified `dotnet test tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj -p:UseSharedCompilation=false`.
- 2026-04-23: Updated module-boundary, integration, player-scope, relationship, and acceptance docs to mark the family command seam as module-owned while Office / Order / Warfare command slices remain temporary application-routed seams.
- 2026-04-23: Validation passed: `dotnet build Zongzu.sln -p:UseSharedCompilation=false`; `dotnet test tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj --no-build -p:UseSharedCompilation=false`; `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PlayerCommandService_" -p:UseSharedCompilation=false`; `dotnet test tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj --no-build -p:UseSharedCompilation=false`; and full `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false`.

## Result

Complete. Current family commands now resolve through `FamilyCoreCommandResolver` inside `Zongzu.Modules.FamilyCore`. `PlayerCommandService` remains thin routing glue for this slice and no longer owns family conflict / lifecycle consequence formulas. Social-memory pressure tempering is read through queries as deterministic command friction, with missing query access staying neutral.

## Residual Risk / Follow-Up

- Follow-up update: the `technical-debt-command-seam-closeout` plan resolved the general `ModuleRunner<TState>` command seam and moved Office, Order, and Warfare command slices into owning module resolvers.
- The social-memory modifier weights are first-pass deterministic bands. They are intentionally conservative, but long-run balance should watch whether volatility/bitterness makes reconciliation too hard in late-pressure saves.
