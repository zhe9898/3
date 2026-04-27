# Doc/code alignment audit

## Goal
Bring the top-level repo guidance and architecture docs back in line with the current codebase after the recent module, command-surface, thin-chain, and Unity shell progress.

## In scope
- Update repo guidance where it still describes an older SDK or missing Unity project root.
- Update `TECH_STACK.md` so project layout and dependency rules match the current `src/`, `tests/`, `tools/`, and `unity/` structure.
- Add a current snapshot to `DESIGN_CODE_ALIGNMENT_AUDIT.md` so old resolved findings are not misread as current failures.
- Remove the obsolete historical audit body from the live alignment page; old baselines remain available through git history.
- Add a small event-observability rule for thin pressure-chain completion.

## Out of scope
- No simulation behavior changes.
- No schema, save, or module-version changes.
- No new pressure-chain formulas.
- No Unity scene or asset edits.

## Touched modules
- Documentation only.
- Conceptual surfaces touched: architecture boundaries, game-design progression, UI/presentation shell status.

## Schema/save impact
None. This pass does not change state, manifests, migrations, command payloads, events, or save envelopes.

## Determinism risk
None. No runtime code changes.

## Milestones
1. [x] Confirm current project/module/test topology from the repository.
2. [x] Patch stale repo guidance and tech-stack dependency rules.
3. [x] Patch the design/code audit with a current snapshot and remaining debt list.
4. [x] Add event-observability classification guidance for pressure-chain work.
5. [x] Run repository validation.

## Tests
- `git diff --check`
- `dotnet test Zongzu.sln --no-restore`

Validation result:
- `git diff --check` passed on 2026-04-24.
- `dotnet test Zongzu.sln --no-restore` passed on 2026-04-24.

Follow-up edit:
- The historical audit body was removed from `DESIGN_CODE_ALIGNMENT_AUDIT.md` after review because it described resolved gaps and made the current alignment state harder to read.

## Rollback path
Revert the documentation edits in this plan. Since no runtime files are touched, rollback has no save or determinism consequences.
