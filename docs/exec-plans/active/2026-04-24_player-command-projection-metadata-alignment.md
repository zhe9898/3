# Player Command Projection Metadata Alignment

## Goal
Align `PresentationReadModelBuilder` player-command affordances and receipts with the shared `PlayerCommandCatalog` so application projections stop hand-copying module/surface/default-label metadata.

This pass should keep the current architecture sharp:
- module-owned `HandleCommand(...)` remains the only authority path
- `PlayerCommandCatalog` stays the shared route metadata source
- read-model builders may still own projection heuristics, targeting, summaries, and execution hints
- application code must not grow a second command metadata switch beside the catalog

## Scope in / out
In scope:
- add projection-local helpers that build affordance/receipt snapshots from `PlayerCommandCatalog`
- replace hand-copied `ModuleKey` / `SurfaceKey` / default label assembly in `PresentationReadModelBuilder`
- remove dead duplicate receipt-building code if it is no longer used
- add regression coverage that projection metadata aligns with the shared catalog
- update integration docs for this projection-side seam

Out of scope:
- no new player commands
- no command formula changes
- no affordance-availability rule changes
- no save/schema changes
- no UI/shell behavior changes

## Touched code / docs
- `src/Zongzu.Application/PresentationReadModelBuilder/*PlayerCommands*`
- `src/Zongzu.Application/PlayerCommandService/*`
- `tests/Zongzu.Integration.Tests/*`
- `docs/MODULE_INTEGRATION_RULES.md`

## Query / Command / DomainEvent notes
- no new query contracts
- no new command names
- no new domain events
- this is projection assembly cleanup only; command authority stays in module resolvers

## Determinism
No intended simulation behavior change. Projection snapshots remain read-only derivatives of authoritative state plus the shared catalog. No command execution or replay-hash behavior should change.

## Save compatibility
None expected. No authoritative state, serialized read models, or module schema versions change.

## Milestones
- [x] Audit the duplicated player-command projection metadata seams and confirm the helper shape.
- [x] Add projection-local catalog helpers and refactor affordance/receipt assembly to use them.
- [x] Remove dead duplicated receipt code and application label wrappers that become unnecessary.
- [x] Add regression tests for projection metadata alignment and run build/test verification.
- [x] Update docs and mark remaining projection-side risk.

## Tests
- integration test proving built player-command affordances use catalog module/surface/label metadata
- integration test proving built player-command receipts keep catalog module/surface alignment after accepted commands
- full solution build/test pass to confirm no projection regressions

## Risks
- some receipts intentionally use module-owned last-command labels rather than the catalog default label; the helper must preserve those overrides while still sourcing module/surface from the catalog
- old or unexpected command codes inside projection state would now fail faster if the builder assumes every live command is cataloged; use the current command contract as the source of truth and keep explicit fallbacks only where already needed

## Result
Completed on 2026-04-24.

Implemented:
- added projection-local snapshot helpers in `PresentationReadModelBuilder` so affordance and receipt assembly now sources module/surface/default label metadata from `PlayerCommandCatalog`
- refactored family, office, public-life, order, and warfare player-command projections to use those helpers instead of open-coded route metadata
- removed the dead `BuildPublicLifeReceiptsNormalized(...)` duplicate path
- removed now-unused label-forwarding helpers from `PlayerCommandService` and the now-unused `PlayerCommandCatalog.DetermineLabel(...)` shim
- updated `MODULE_INTEGRATION_RULES.md` so the projection-side contract explicitly prefers helper-based snapshot assembly over repeated inline route metadata
- extended integration coverage so real presentation bundles assert affordance metadata alignment with the catalog and accepted-command receipts assert route ownership alignment after command execution

Verification:
- `git diff --check`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~CommandSeamIntegrationTests"`
- `dotnet build src/Zongzu.Application/Zongzu.Application.csproj -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

Residual risk:
- receipt projections still allow module-owned label overrides for carried-forward command traces, which is intentional; future cleanup should keep those overrides explicit rather than silently forcing every historical receipt label back to the catalog default
- affordance availability, summary wording, and execution hints remain hand-authored projection heuristics in `PresentationReadModelBuilder`; this pass removes metadata duplication, not those projection heuristics
