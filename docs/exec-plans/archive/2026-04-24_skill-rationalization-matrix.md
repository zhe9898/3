# 2026-04-24 Skill Rationalization Matrix

## Goal

Create a repo-local skill rationalization matrix that explains when to use each Zongzu Codex skill, how those skills map to the current codebase, and how external engineering/accessibility standards calibrate them without overriding Zongzu's product direction.

## In Scope

- Document all current Zongzu skills as a coordinated skill pack.
- Update repo navigation so the matrix is discoverable.
- Update `AGENTS.md` so the preferred skill list reflects the full Zongzu skill set, not only the original broad four.
- Record external calibration sources for Unity, .NET testing, and accessibility guidance.

## Out of Scope

- No runtime code changes.
- No save/schema changes.
- No scheduler, module authority, projection, or Unity asset changes.
- No new gameplay rule, feature pack, or content pack.

## Touched Modules / Docs

- Docs only:
  - `AGENTS.md`
  - `docs/README.md`
  - `docs/DOCUMENTATION_MAP.md`
  - `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`
  - `docs/exec-plans/active/2026-04-24_skill-rationalization-matrix.md`

No simulation module is touched.

## Query / Command / DomainEvent Impact

None. This pass changes skill orchestration documentation only.

## Determinism Impact

None. No authoritative code, scheduler cadence, RNG, tests, or persisted data are changed.

## Save / Schema Impact

None. No module state, root save envelope, migration, manifest, or read-model payload shape changes.

## Unity / Presentation Boundary Impact

Documentation-only clarification:
- Unity project root exists at `unity/Zongzu.UnityShell`.
- Unity shell remains a projection/adaptation host.
- `Zongzu.Kernel` and simulation modules must not reference Unity APIs.
- Unity Editor MCP workflow is not assumed available unless configured for a later task.

## Milestones

- [x] Identify all current Zongzu skills.
- [x] Browse/check external calibration sources for Unity, .NET testing, and accessibility.
- [x] Add the skill rationalization matrix.
- [x] Update repo navigation and `AGENTS.md`.
- [x] Run doc-only validation checks.

## Tests / Checks

- `git diff --check`
- `git status --short --untracked-files=all`
- `quick_validate.py` for all 9 Zongzu skill folders

Runtime test suite is not required because this pass is documentation-only.

## Rollback / Fallback Notes

Rollback is limited to reverting the four documentation files in this plan. Since there is no runtime, schema, save, scheduler, or Unity asset change, no migration or replay fallback is needed.
