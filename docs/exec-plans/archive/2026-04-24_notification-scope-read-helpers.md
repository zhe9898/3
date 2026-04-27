# Notification Scope Read Helpers

## Goal
Normalize settlement/module matching for narrative notifications so application selectors and presentation adapters stop repeating raw trace scans.

This pass is strictly read-side:
- no narrative-authority changes
- no scheduler or module changes
- no save/schema changes
- no UI-owned rule path

## Scope in / out
In scope:
- add narrow read-only helper methods on notification snapshots for settlement/module matching
- rewire application settlement notification selection to use them
- rewire warfare aftermath notification filtering to use them
- add focused test coverage and update integration guidance

Out of scope:
- no notification authoring changes
- no new projection bundle fields
- no generalized notification query DSL

## Touched code / docs
- `src/Zongzu.Contracts/Queries/NarrativeProjectionQueries.cs`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PlayerCommands.Selection.cs`
- `src/Zongzu.Presentation.Unity/Adapters/Warfare/WarfareAftermathShellAdapter.cs`
- `tests/Zongzu.Integration.Tests/ProjectionSelectorAlignmentTests.cs`
- `docs/MODULE_INTEGRATION_RULES.md`

## Query / Command / DomainEvent notes
- no query contract change beyond read-only helper methods on existing snapshots
- no command changes
- no domain-event changes

## Determinism
No simulation change. Read-side matching only.

## Save compatibility
None expected.

## Milestones
- [ ] Add narrow notification scope helpers for settlement and module matching.
- [ ] Rewire application/presentation callers to use the helpers.
- [ ] Validate with focused and full tests, then record residual risk.

## Risks
- helpers must stay narrow and descriptive, not grow into a generic query mini-language
- caller-side ordering and interpretation still belong outside the snapshot helper
