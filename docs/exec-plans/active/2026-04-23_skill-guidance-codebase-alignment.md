# Skill Guidance Codebase Alignment

## Goal
Align the repository-scoped skills in `.github/skills/` with the current Zongzu codebase, docs, and externally verified skill-authoring conventions.

## Scope in / out
In scope:
- Update skill trigger descriptions and workflows when they are stale against current code and docs.
- Keep skills concise and progressive-disclosure friendly.
- Preserve existing skill locations under `.github/skills/`; do not create duplicate `.agents/skills` copies in this pass.
- Fix obvious mojibake in skill-facing examples where it blocks real triggering.

Out of scope:
- No simulation code changes.
- No schema or save migrations.
- No new feature pack promises.
- No new external source dataset import.

## Affected modules
- No authoritative simulation module is changed.
- Documentation/tooling surface touched: `.github/skills/*`.

## Save/schema impact
None. This task changes agent guidance only.

## Determinism risk
None directly. Updated skills should make future work more likely to respect deterministic scheduler, event, and module-boundary rules.

## Milestones
- Confirm official skill-format guidance and the repository's existing `.github/skills` convention.
- Read the mandatory product, architecture, simulation, presentation, and acceptance docs in compressed form.
- Compare skills against current code facts such as .NET target, module set, schema versions, scheduler drain, presentation projects, and Unity shell presence.
- Patch only stale or underspecified skill guidance.
- Validate frontmatter, relative references, and working tree impact.

## Tests to add/update
- Run skill validation against changed `SKILL.md` files when available.
- Check relative Markdown links from each skill root.
- Run `git diff --check`.

## Rollback / fallback plan
Revert the skill markdown changes if validation fails or if the edits create duplicate, noisy, or over-triggering guidance.

## Open questions
- Codex official docs prefer repo skills in `.agents/skills`; GitHub Copilot documents `.github/skills`. This repo already uses `.github/skills`, so this pass keeps that location unless a later migration is explicitly requested.
