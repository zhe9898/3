# Project skill pack

## Goal
Create project-specific Codex skills that can support Zongzu's longer production arc beyond the current broad design, architecture, UI, and historical-grounding skills.

## In scope
- Add local skills under `C:/Users/Xy172/.codex/skills`.
- Cover simulation validation, pressure-chain work, save/schema compatibility, Unity shell implementation, and content authoring.
- Update `AGENTS.md` so future repo work knows these skills are preferred for matching tasks.

## Out of scope
- No simulation code changes.
- No save/schema changes.
- No Unity scene or asset changes.
- No content data changes.

## Touched modules
- Local Codex skills:
  - `zongzu-simulation-validation`
  - `zongzu-pressure-chain`
  - `zongzu-save-and-schema`
  - `zongzu-unity-shell`
  - `zongzu-content-authoring`
- Repo documentation:
  - `AGENTS.md`

## Schema/save impact
None. This pass creates guidance only.

## Determinism risk
None. No runtime code changes.

## Milestones
1. [x] Read `skill-creator` instructions and existing Zongzu skill style.
2. [x] Initialize five local skill folders.
3. [x] Replace template SKILL.md files with project-specific workflows.
4. [x] Generate `agents/openai.yaml` metadata for each skill.
5. [x] Validate each skill with `quick_validate.py`.
6. [x] Update `AGENTS.md` preferred-skill guidance.
7. [x] Check external Codex/Agent Skills guidance and tighten trigger boundaries so the skills stay focused instead of becoming mandatory ritual.

## Tests
- `quick_validate.py` passed for all five new skills.
- `git diff --check` should pass before finalizing.

Standards checked:
- OpenAI Codex Agent Skills docs: skills should be focused, use clear descriptions and boundaries, rely on progressive disclosure, and validate trigger behavior.
- Agent Skills open standard: `SKILL.md` carries required `name`/`description`, optional resources stay on demand, and full instructions should load only when relevant.

## Rollback path
Remove the five local skill folders and revert the `AGENTS.md` addition. Since no runtime files are touched, rollback has no save or determinism consequences.
