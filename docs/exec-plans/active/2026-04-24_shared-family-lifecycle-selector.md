# Shared Family Lifecycle Selector

## Goal
Remove the duplicated family-lifecycle affordance ordering brain that currently lives once in `Zongzu.Application` and once again in `Zongzu.Presentation.Unity`.

The shared logic is projection-only:
- no authoritative rule change
- no command-routing change
- no save/schema change
- no UI-owned write path

## Scope in / out
In scope:
- add a contracts-level read-only selector for family lifecycle affordance/receipt ordering
- rewire application and presentation callers to use the shared selector
- delete the presentation-local duplicate selector
- add or update focused tests and integration guidance

Out of scope:
- no command consequences
- no family-core formulas
- no shell copy rewrite
- no new projections

## Touched code / docs
- `src/Zongzu.Contracts/ReadModels/*`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PlayerCommands.Selection.cs`
- `src/Zongzu.Presentation.Unity/Adapters/Family/*`
- `tests/Zongzu.Integration.Tests/ProjectionSelectorAlignmentTests.cs`
- `tests/Zongzu.Presentation.Unity.Tests/*`
- `docs/MODULE_INTEGRATION_RULES.md`

## Query / Command / DomainEvent notes
- no query changes
- no command changes
- no domain-event changes

## Determinism
No simulation change. Projection/shell selection only.

## Save compatibility
None expected.

## Milestones
- [x] Add a shared contracts-level family lifecycle selector.
- [x] Rewire application + presentation to use the shared selector and delete the duplicate presentation implementation.
- [x] Validate with focused and full tests, then record residual risk.

## Risks
- the new helper must stay projection-only and not grow authority semantics
- contracts must expose only stable snapshot-side selection, not presentation wording or module mutation behavior

## Outcome
- added `FamilyLifecycleProjectionSelectors` under contracts read models so family lifecycle affordance/receipt ordering now has one shared projection-side home
- rewired `PresentationReadModelBuilder` to consume that shared selector for family hall / preview alignment
- rewired `FamilyShellAdapter` to consume the same shared selector and deleted the old presentation-local duplicate implementation
- added an integration test proving the shared selector matches the preview affordance selection that drives the existing family hall guidance
- updated module integration guidance so future shared snapshot-side ordering can be lifted into contracts instead of drifting across application and shell layers

## Validation
- `git diff --check`
- `dotnet build-server shutdown`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~ProjectionSelectorAlignmentTests|FullyQualifiedName~MvpFamilyLifecyclePreviewScenario"`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~FirstPassPresentationShellTests"`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

## Residual Risk
- this helper intentionally shares only lifecycle ordering over existing snapshots; if later work tries to push wording, command availability, or family consequence logic into it, we should stop and keep those in application or module-owned layers
- the helper currently keys one succession-gap branch off the existing family lifecycle trace phrase; if `FamilyCore` later exposes an explicit read-model field for that state, the selector should switch to the field and drop the trace-string check
