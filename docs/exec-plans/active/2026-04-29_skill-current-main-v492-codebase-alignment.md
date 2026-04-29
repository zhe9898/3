# Skill Current Main V492 Codebase Alignment

## Intent

Align the repo-tracked Zongzu skill pack and local Codex skill mirror with current main after v485-v492.

V485-V492 closes the household mobility owner-lane preflight as docs/tests governance only. Skills must now treat v469-v476 as a closed future-rule gate, not as runtime household movement, migration economy, route history, selector state, durable residue, status/class drift, fidelity promotion, or UI/Unity authority.

## Skills Used

1. `skill-creator` - validate existing skill folders after updates.
2. `zongzu-architecture-boundaries` - keep skill guidance aligned with module ownership and no-authority drift.
3. `zongzu-game-design` - keep closeout language as bounded future-rule guidance, not new gameplay authority.
4. `zongzu-save-and-schema` - record no persisted state, schema, migration, manifest, or projection-cache impact.
5. `zongzu-simulation-validation` - scope validation to skill/docs checks because runtime behavior does not change.

## Scope

- Update `.github/skills` Zongzu skills and the local Microsoft C# helper to reference current main through v492.
- Update `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md` with skill-pack alignment through v492.
- Mirror the repo-tracked skill bodies to `C:\Users\Xy172\.codex\skills`.
- Validate each skill folder with `quick_validate.py`.

## Non-Goals

- No production code change.
- No runtime simulation behavior change.
- No Unity, ViewModel, adapter, content, or asset change.
- No new skill folder, agents metadata, scripts, references, or assets.
- No schema bump, migration, save manifest change, persisted state, projection cache, ledger, selector, route-history state, movement command, or `PersonRegistry` expansion.

## Target Schema / Migration

Target schema/migration impact: none.

This pass edits skill guidance and docs only. It does not change save data, module schema versions, manifests, migrations, or runtime defaults.

## Validation Plan

- Run `quick_validate.py` for every repo-tracked `.github/skills/*` folder.
- Mirror changed skills to local Codex skill folders and verify SHA-256 equality.
- `git diff --check`
- scan touched files for replacement characters

Runtime .NET tests are intentionally unnecessary because no production code, contracts, schemas, projections, Unity adapters, or content assets change.

## Rollback

Revert the skill/docs commit and re-copy the previous skill bodies to the local mirror if needed. No save migration or production data rollback is required.

## Milestones

- [x] Update skill guidance.
- [x] Update skill matrix.
- [x] Mirror local skill folders.
- [x] Run skill validation and text checks.

## Completion Evidence

- Updated all repo-tracked `.github/skills` folders for current main through v492:
  - `microsoft-code-reference`
  - `zongzu-ancient-china`
  - `zongzu-architecture-boundaries`
  - `zongzu-content-authoring`
  - `zongzu-game-design`
  - `zongzu-pressure-chain`
  - `zongzu-save-and-schema`
  - `zongzu-simulation-validation`
  - `zongzu-ui-shell`
  - `zongzu-unity-shell`
- Updated `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md` with skill-pack alignment through v492.
- Mirrored the repo-tracked skill bodies to `C:\Users\Xy172\.codex\skills` and verified SHA-256 equality.
- Schema / migration impact: none. No persisted state, schema version, migration, save manifest, module namespace, projection cache, selector state, route-history state, durable residue, owner-lane state, or module payload changed.
- Runtime validation impact: none required. No production code, contracts, schemas, projections, Unity adapters, content assets, scheduler cadence, command route, or save/load behavior changed.

Validation completed on 2026-04-29:

- `quick_validate.py` passed for every repo-tracked `.github/skills/*` folder.
- Local skill mirror SHA-256 comparison passed.
- `git diff --check` passed.
- Touched-file replacement-character scan passed.
