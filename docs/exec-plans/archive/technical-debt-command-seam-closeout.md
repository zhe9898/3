# Technical Debt Command Seam Closeout

> Status: Complete
> Started: 2026-04-23
> Skills: `zongzu-game-design`, `zongzu-ancient-china`

## Goal

Pay down the current command-routing and rule-layer technical debt after the pressure-tempering kernel and family command resolver work.

The target is not a new gameplay system. The target is to keep the architecture honest: player commands should route through a stable module-owned seam, Application should not grow into a second rule layer, dead command code should disappear, and oversized command files should be split before they harden.

## Scope In

- Add a `ModuleRunner` command-handling seam for player commands.
- Route family commands through the new seam instead of Application calling the resolver directly.
- Move Office, Order/PublicLife, and Warfare command consequence formulas into owning modules.
- Delete the old application-layer warfare command service and order dead-code branch if no longer needed.
- Split the oversized `FamilyCoreCommandResolver` into smaller partial files.
- Remove stale application command-resolution helpers if no longer referenced.
- Update docs and tests for the new boundary.

## Scope Out

- No new player authority lanes.
- No event-pool command loop.
- No schema/save migration unless command state shape changes, which is not expected.
- No rewrite of monthly scheduler, pressure chains, or historical-process formulas.
- No UI authority changes.

## Affected Modules

- `Zongzu.Contracts`
  - module command scope / runner seam.
- `Zongzu.Application`
  - keeps `PlayerCommandService` as dispatch glue only.
  - no domain command formulas should remain here for migrated commands.
- `Zongzu.Modules.FamilyCore`
  - command resolver split; module handles family commands.
- `Zongzu.Modules.OfficeAndCareer`
  - office command resolver.
- `Zongzu.Modules.OrderAndBanditry`
  - public-life/order intervention resolver and administrative-reach profile.
- `Zongzu.Modules.WarfareCampaign`
  - campaign directive command resolver.
- Docs and tests.

## Schema / Save Impact

- No schema bump expected.
- Existing command receipt fields remain in their owning module state.
- Replay hash changes only when the same owned state changes as before; routing path changes should not alter command outcomes.

## Query / Command / DomainEvent

### Query

- Command handlers may read through `ModuleCommandHandlingScope.Context.Queries`.
- Cross-module reads remain query-only.

### Command

- `IModuleRunner.HandleCommand(...)` becomes the common seam.
- `PlayerCommandService` maps command names to module keys and delegates.

### DomainEvent

- No new command events in this closeout.
- Command receipts remain owned state / read-model output, not event-pool drivers.

## Determinism

- Command handlers remain deterministic from current save state + command request.
- No wall clock, IO, unordered iteration, or hidden random source.
- If a handler needs random later, it must use the provided deterministic context.

## Milestones

### Milestone 1 - Inventory And Plan

Status: Complete

- Identify current command debt.
- Add this ExecPlan.
- Confirm build is clean before edits.

### Milestone 2 - Command Seam

Status: Complete

- Add module command-handling scope and default runner implementation.
- Add a `GameSimulation` internal routing method that builds queries and refreshes replay hash only after accepted mutation.
- Route FamilyCore through the seam.

### Milestone 3 - Owning Module Command Resolvers

Status: Complete

- Move Office command logic into `OfficeAndCareer`.
- Move Order/PublicLife command logic and administrative reach evaluation into `OrderAndBanditry`.
- Move Warfare directive logic into `WarfareCampaign`.
- Keep `PlayerCommandService` as name-to-module dispatch and disabled-path reporting.

### Milestone 4 - File Hygiene

Status: Complete

- Split `FamilyCoreCommandResolver` into smaller partial files.
- Delete stale application command helpers and services.
- Remove dead `#if false` branch.

### Milestone 5 - Docs, Tests, Commit, Push

Status: Complete

- Update module integration, boundaries, player-scope, and acceptance docs.
- Run build and targeted tests.
- Run full tests if feasible.
- Commit, push, and update the current PR.

## Tests To Run

- `dotnet build Zongzu.sln -p:UseSharedCompilation=false`
- `dotnet test tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj -p:UseSharedCompilation=false`
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --filter "FullyQualifiedName~PlayerCommandService_" -p:UseSharedCompilation=false`
- `dotnet test tests\Zongzu.Modules.OrderAndBanditry.Tests\Zongzu.Modules.OrderAndBanditry.Tests.csproj -p:UseSharedCompilation=false`
- `dotnet test tests\Zongzu.Modules.OfficeAndCareer.Tests\Zongzu.Modules.OfficeAndCareer.Tests.csproj -p:UseSharedCompilation=false`
- `dotnet test tests\Zongzu.Modules.WarfareCampaign.Tests\Zongzu.Modules.WarfareCampaign.Tests.csproj -p:UseSharedCompilation=false`
- Full `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false` if targeted tests pass.

## Rollback / Fallback Plan

- If full Office/Order/Warfare migration becomes risky, keep the module command seam and FamilyCore path, then document remaining modules as residual debt.
- If a moved resolver changes behavior, use existing integration tests to compare receipts and restore the old formula in the owning module rather than returning it to Application.

## Open Questions

- None blocking. The project docs already state command resolution belongs in owning modules and Application must stay thin.

## Progress Log

- 2026-04-23: Started from a clean branch after `0a3b6e1`. Inventory found four concrete debts: missing `ModuleRunner` command seam, Office/Order/Warfare Application-owned command formulas, a 900+ line FamilyCore command resolver, and stale active ExecPlan/dead command code residue.
- 2026-04-23: Added `ModuleCommandHandlingScope` and `IModuleRunner.HandleCommand`, routed FamilyCore through the shared seam, and moved Office, Order/PublicLife, and Warfare command formulas into owning module resolvers.
- 2026-04-23: Split `FamilyCoreCommandResolver` into responsibility-focused partial files and deleted stale Application command helpers plus legacy Warfare-specific intent DTOs.
- 2026-04-23: Updated command-routing docs in module integration, module boundaries, player scope, and acceptance criteria.
- 2026-04-23: Archived the completed pressure-tempering kernel and family command autonomy ExecPlans out of `active/`.
- 2026-04-23: Hardened command determinism by giving command handlers a cloned `KernelState` and committing random/id counters only when the command is accepted.
- 2026-04-23: Validation passed: `dotnet build Zongzu.sln -p:UseSharedCompilation=false`; targeted FamilyCore, OfficeAndCareer, OrderAndBanditry, WarfareCampaign, and PlayerCommandService integration tests; full `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false`.

## Result

Complete. Player-command routing now has a shared module-owned seam. Family, Office, Order/PublicLife, and Warfare command formulas live in their owning modules, while `PlayerCommandService` remains dispatch / disabled-path glue. The oversized FamilyCore command resolver is split by responsibility, stale Application command helpers are removed, and command-boundary docs now describe the current architecture.

## Residual Risk / Follow-Up

- Some older historical ExecPlans in `docs/exec-plans/active/` still describe the first application-routed slices as they existed when written. They were left in place unless they were directly completed by this closeout, to avoid rewriting historical task records without a separate archive pass.
- The command seam is intentionally deterministic and module-owned, but future command depth still needs per-module tests proving disabled-module hiding, query-only cross-module reads, and no UI authority drift.
