# Settlement-Scoped Projection Selectors

## Goal
Normalize settlement-scoped notification and lookup selection in `PresentationReadModelBuilder` so governance and warfare read models reuse the same projection-side selector seam instead of carrying copy-pasted settlement filtering and one-off index creation.

This pass stays strictly downstream of authority:
- no module rule changes
- no command-routing changes
- no save/schema changes
- no UI-owned rules

## Scope in / out
In scope:
- add a shared projection helper for "first item by settlement" indexing
- add a shared settlement-scoped notification selector helper
- rewire governance / warfare and nearby projection code to use those helpers
- add focused selector tests and update integration guidance

Out of scope:
- no simulation logic changes
- no command consequence changes
- no new projections or shell copy
- no notification-authoring changes

## Touched code / docs
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PlayerCommands.Selection.cs`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.Governance.cs`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.HallDocket.Warfare.cs`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.LivingSociety.cs`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PlayerCommands.cs`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PlayerCommands.Receipts.cs`
- `tests/Zongzu.Integration.Tests/ProjectionSelectorAlignmentTests.cs`
- `docs/MODULE_INTEGRATION_RULES.md`

## Query / Command / DomainEvent notes
- no query changes
- no command changes
- no domain-event changes

## Determinism
No simulation change. Read-model assembly only.

## Save compatibility
None expected.

## Milestones
- [x] Add shared projection helpers for settlement-scoped lookup/index selection.
- [x] Rewire governance / warfare / adjacent application builders to use the shared helpers.
- [x] Add focused selector tests, run validation, and record residual risk.

## Risks
- helper APIs must stay projection-local and read-only
- module filters and priority policies must remain caller-owned, not hidden inside a generic helper

## Outcome
- added a shared `IndexFirstBySettlement(...)` projection helper so application read-model code can normalize first-hit settlement lookups without scattering ad hoc `ToDictionary(...)` seams
- added a shared settlement-scoped notification selector helper and rewired governance / warfare notification selection to reuse it while keeping source-module filters and priority policies at the call sites
- rewired nearby player-command/public-life projection code to reuse the same settlement index helper instead of maintaining separate one-off settlement dictionaries
- added an integration test that exercises settlement/module/priority notification selection directly through the application assembly's internal projection seam

## Validation
- `git diff --check`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~ProjectionSelectorAlignmentTests|FullyQualifiedName~M2LiteIntegrationTests.LocalConflictBootstrap|FullyQualifiedName~M2LiteIntegrationTests.CampaignAndCommands|FullyQualifiedName~M2LiteIntegrationTests.PostMvpAndGovernance"`
- `dotnet build-server shutdown`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

## Residual Risk
- the new selector seam is intentionally narrow: it only normalizes settlement scoping plus stable ordering mechanics; if future code needs richer grouping than "same settlement and maybe same source module," that should remain caller-owned rather than turning this helper into a mini query DSL
- `InternalsVisibleTo("Zongzu.Integration.Tests")` was added so focused projection-selector tests can hit the application seam directly; if more assemblies start depending on these internals, we should pause and make sure we're not accidentally promoting projection helpers into shared product API
