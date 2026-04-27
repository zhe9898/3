# Skill Guidance Codebase Alignment

## Goal
Keep the Zongzu skill pack aligned with the repository's current product truth, module boundaries, and code facts so that future Codex work follows the live architecture instead of stale guidance.

This pass treats `.github/skills/` as the tracked source of truth, then syncs the currently installed local Codex copies under `C:\Users\Xy172\.codex\skills\`.

## Scope in / out
In scope:
- Audit the current repo-local Zongzu skills against AGENTS, current docs, and current code facts.
- Fix stale or misleading skill guidance.
- Add the missing repo-local `zongzu-architecture-boundaries` skill referenced by `AGENTS.md`.
- Keep tracked skills and installed local skills aligned enough for immediate use in this environment.

Out of scope:
- No authoritative simulation rule changes.
- No schema or save migration work.
- No module or scheduler behavior changes.
- No runtime plugin/mod architecture change beyond skill guidance wording.

## Affected modules / surfaces
- No authoritative simulation module is changed.
- Documentation / tooling surface touched:
  - `.github/skills/zongzu-game-design/`
  - `.github/skills/zongzu-ancient-china/`
  - `.github/skills/zongzu-ui-shell/`
  - `.github/skills/zongzu-architecture-boundaries/` (new)
  - installed mirrors under `C:\Users\Xy172\.codex\skills\`

## Current code facts this plan must respect
- Module-owned command handling seam exists through `ModuleRunner<TState>.HandleCommand(...)`.
- `GameSimulation` routes player commands into module-owned handlers.
- `GetMutableModuleState(...)` still exists, but current use is bootstrap / seeding / test-facing, not the preferred command-resolution path.
- Query / Command / DomainEvent remains the cross-module contract.
- Scheduler event propagation uses bounded deterministic drain rather than unbounded recursive dispatch.

## Save/schema impact
None. This task changes skill guidance only.

## Determinism risk
None directly. The purpose of the pass is to reduce future architecture drift and keep deterministic-authority guidance current.

## Milestones
- [x] Re-read AGENTS and the mandatory project docs in compressed form.
- [x] Re-audit repo-local skills against current docs and code facts.
- [x] Identify whether the previously observed Chinese corruption was file corruption or Windows display-layer mojibake.
- [x] Patch repo-local skills where stale or missing.
- [x] Sync the patched repo-local skills into the currently installed local skill copies.
- [x] Validate skill links / references and working-tree cleanliness.

## Tests / verification
- Check changed `SKILL.md` files for UTF-8 readability.
- Check relative Markdown links for the changed repo-local skills.
- Run `git diff --check`.

## Rollback / fallback plan
Revert the skill markdown changes if they introduce broken relative links, duplicate guidance, or contradict current repo facts.

## Outcome notes
- The earlier "乱码" suspicion was confirmed as a Windows display-path issue, not UTF-8 file corruption in the tracked skill files.
- Repo-local `zongzu-game-design` was already the healthier source of truth than the installed home copy, so this pass syncs downward from repo to home.
- The missing repo-local `zongzu-architecture-boundaries` skill is now added and aligned to the live command seam (`HandleCommand(...)`) instead of the old "command gap" story.

## Remaining risk
- The installed home skill mirror is now aligned for this machine, but other machines or future reinstalls still depend on the repo-tracked `.github/skills/` copy being kept current.
