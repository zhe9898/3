# Command Seam Hardening

## Goal
Harden the current module-owned command seam so the live codebase clearly reflects the post-migration architecture:
- `ModuleRunner<TState>.HandleCommand(...)` is the normal command-resolution path
- `PlayerCommandService` stays dispatch glue
- only modules with live module-owned command handlers advertise `AcceptedCommands`
- `GetMutableModuleState(...)` remains bootstrap / seeding / narrow test infrastructure rather than a normal gameplay-rule path

## Scope in / out
In scope:
- Audit which modules currently advertise accepted commands versus which modules actually own `HandleCommand(...)`.
- Remove stale `AcceptedCommands` advertising from non-migrated or partially migrated modules.
- Add tests that protect the current seam from drifting back toward Application-owned rules.
- Update docs where the live command-seam boundary needs to be stated more explicitly.

Out of scope:
- No new player-command feature design.
- No new module command formulas for Education / Trade / Population / SocialMemory / Conflict.
- No schema or save migration work.
- No UI or projection redesign.

## Touched code / docs
- `src/Zongzu.Application/*`
- `src/Zongzu.Modules.*` modules that currently over-advertise `AcceptedCommands`
- `tests/Zongzu.Architecture.Tests/*`
- `tests/Zongzu.Integration.Tests/*`
- `docs/MODULE_INTEGRATION_RULES.md`

## Current facts this pass must preserve
- Family / Office / Order / Warfare already resolve player commands through module-owned resolvers.
- `GameSimulation.IssueModuleCommand(...)` clones `KernelState`, delegates to the owning module, and applies command kernel state only on accepted results.
- `SimulationBootstrapper` still legitimately uses `GetMutableModuleState(...)` for bootstrap / seeding.

## Query / Command / DomainEvent notes
- No new cross-module query contracts.
- No new `DomainEvent` types.
- `AcceptedCommands` remains a contract surface and must describe live module-owned command support, not future intent.

## Determinism
No intentional change to simulation behavior. Tests should prove that accepted command handling mutates through the owning module seam and that rejected commands do not perturb replay-hash state through the command path.

## Save compatibility
None expected. This pass changes command-contract advertising and tests only.

## Milestones
- [x] Audit command-contract drift and list stale command advertisers.
- [x] Tighten module command-contract surfaces to match live ownership.
- [x] Add seam hardening tests (routing contract, replay-hash behavior, source-usage guardrails).
- [x] Update integration docs and verify build/test.

## Tests
- Architecture/source-scan test for `GetMutableModuleState(...)` usage staying in approved application files only.
- Integration/runtime test that accepted module command handling changes state and replay hash, while rejected command handling does not.
- Integration/contract test that current player-facing command names match the union of live command-owning module `AcceptedCommands`, and non-migrated modules do not advertise player commands.

## Risks
- Some old tests may still assert stale command names from pre-seam placeholders.
- Office may still advertise historical/future command names that no longer have a live player-facing path; those must be trimmed rather than preserved as fake contracts.

## Result
Completed on 2026-04-23.

Implemented:
- Removed stale `AcceptedCommands` advertising from `EducationAndExams`, `TradeAndIndustry`, `ConflictAndForce`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations`, which currently do not own live module command handling.
- Trimmed `OfficeAndCareer` command advertising to the live player-command names still resolved by the module.
- Added an architecture/source-guard test that keeps `GetMutableModuleState(...)` usage confined to `GameSimulation` plus `SimulationBootstrapper` bootstrap/seeding files.
- Added integration tests for accepted/rejected office command replay-hash behavior and for the live player-command contract matching the union of current command-owning modules.
- Updated `MODULE_INTEGRATION_RULES.md` so placeholder commands are no longer treated as part of the live contract surface.

Verification:
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~CommandSeamIntegrationTests"`
- `dotnet test tests/Zongzu.Modules.OfficeAndCareer.Tests/Zongzu.Modules.OfficeAndCareer.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test tests/Zongzu.Modules.ConflictAndForce.Tests/Zongzu.Modules.ConflictAndForce.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

Residual risk:
- This pass hardens the seam; it does not migrate new command depth into non-command-owner modules. If future work adds player-command formulas to those modules, they must first add module-owned `HandleCommand(...)` support and only then advertise `AcceptedCommands`.
