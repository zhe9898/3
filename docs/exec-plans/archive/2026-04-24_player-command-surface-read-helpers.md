# Player Command Surface Read Helpers

## Goal
Remove repeated surface-key filtering from presentation projection contexts and shell adapters by teaching `PlayerCommandSurfaceSnapshot` how to expose read-only affordance/receipt enumerators by surface and optional settlement scope.

This is contracts/presentation cleanup only:
- no authority changes
- no command routing changes
- no save/schema changes
- no UI write paths

## Scope in / out
In scope:
- add read-only affordance/receipt enumerators on `PlayerCommandSurfaceSnapshot`
- rewire presentation projection contexts and public-life adapter to use them
- add focused tests and update integration guidance

Out of scope:
- no ordering-policy changes beyond preserving existing sort sites
- no application-side authority logic changes
- no new command metadata

## Touched code / docs
- `src/Zongzu.Contracts/ReadModels/PlayerCommandReadModels.cs`
- `src/Zongzu.Presentation.Unity/ProjectionContexts/*`
- `src/Zongzu.Presentation.Unity/Adapters/PublicLife/PublicLifeShellAdapter.cs`
- `tests/Zongzu.Integration.Tests/ProjectionSelectorAlignmentTests.cs`
- `docs/MODULE_INTEGRATION_RULES.md`

## Query / Command / DomainEvent notes
- no query changes
- no command changes
- no domain-event changes

## Determinism
No simulation change. Read-model traversal only.

## Save compatibility
None expected.

## Milestones
- [x] Add read-only surface traversal helpers to `PlayerCommandSurfaceSnapshot`.
- [x] Rewire presentation contexts/adapters to use the helpers while preserving local ordering.
- [x] Validate with focused and full tests, then record residual risk.

## Risks
- helpers must stay traversal-only and not absorb sorting or policy
- callers still own presentation ordering and enabled/disabled display choices

## Outcome
- added read-only `EnumerateAffordances(...)` / `EnumerateReceipts(...)` helpers to `PlayerCommandSurfaceSnapshot` with optional settlement scoping and affordance enabled-filtering
- rewired `FamilyProjectionContext`, `OfficeProjectionContext`, `WarfareProjectionContext`, and `PublicLifeShellAdapter` to use the shared snapshot traversal helpers instead of repeating `SurfaceKey` filters locally
- added an integration test that exercises the new snapshot traversal helpers against surface/scope filtering
- updated integration guidance so future shell traversal stays on shared snapshot helpers while keeping ordering decisions local

## Validation
- `git diff --check`
- `dotnet build-server shutdown`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~ProjectionSelectorAlignmentTests"`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~FirstPassPresentationShellTests"`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

## Residual Risk
- these helpers intentionally stop at traversal/scope filtering; if later callers start asking them to own ordering or visibility policy, that belongs one layer up in projection or shell code
- application-side builders still keep their own local affordance/receipt enumerators because they currently carry slightly different downstream semantics; if those paths converge later, we should unify carefully without pushing application heuristics into contracts by accident
