# Hall Docket Lane Accessors

## Goal
Eliminate repeated `LeadItem` / `SecondaryItems` lane-selection logic around `HallDocketStackSnapshot` by adding read-only snapshot helpers for present-item enumeration and lane lookup.

This pass should stay strictly projection-side:
- no authority changes
- no module state changes
- no scheduler changes
- no save/schema changes
- no UI-owned rule logic

## Scope in / out
In scope:
- add read-only helper methods to `HallDocketStackSnapshot`
- rewire duplicated test or adapter callers that manually search lead/secondary items by lane
- keep helper semantics aligned with current hall-docket visibility rules (empty-headline items are absent)

Out of scope:
- no hall-docket ordering changes
- no new lane types
- no presentation copy changes
- no module or command behavior changes

## Touched code / docs
- `src/Zongzu.Contracts/ReadModels/HallReadModels.cs`
- `src/Zongzu.Presentation.Unity/ProjectionContexts/HallDocketProjectionContext.cs`
- `tests/Zongzu.Integration.Tests/*`
- `docs/exec-plans/active/*`

## Query / Command / DomainEvent notes
- no query changes
- no command changes
- no domain event changes

## Determinism
No simulation change. This is read-model convenience behavior only.

## Save compatibility
None expected.

## Milestones
- [x] Add read-only present-item / lane lookup helpers to `HallDocketStackSnapshot`.
- [x] Rewire duplicated test/presentation callers to the shared helper path.
- [x] Run focused and full validation, then record residual risk.

## Risks
- helper semantics must match current hall-docket visibility rules, especially the "empty headline means absent item" convention
- this should remain a thin DTO convenience seam, not become a second hall-docket composition layer

## Outcome
- added `HasLeadItem`, `EnumeratePresentItems()`, `HasLaneItem(...)`, and `TryGetLaneItem(...)` to `HallDocketStackSnapshot`
- rewired `HallDocketProjectionContext` to consume the shared present-item helper instead of re-implementing empty-headline filtering
- replaced duplicated family/warfare lane selection logic in integration tests with the shared snapshot helper
- added a guard test proving placeholder docket items are ignored during lane lookup

## Validation
- `git diff --check`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

## Residual Risk
- the helper intentionally follows current hall-docket visibility semantics by treating empty-headline items as absent; if hall-docket payload semantics change later, the helper and its guard test need to move with that contract rather than letting callers reintroduce ad hoc lane filtering
