# Skill Current Main V476 Codebase Alignment

Date: 2026-04-29

Baseline: current `main` after `ace832c Preflight household mobility owner lane`.

## Purpose

Align the repo-tracked Zongzu skill pack and skill orchestration notes with the current codebase through v476. This is a governance/skill alignment pass, not a runtime feature.

V469-V476 records household mobility owner-lane preflight: future household mobility rule depth should start in `PopulationAndHouseholds` unless a later ExecPlan proves another owner lane, and must declare owner state, cadence, target scope, hot path, touched counts, deterministic cap/order, no-touch boundary, schema impact, projection fields, and validation before implementation.

## Skill Sequence

1. `skill-creator` - keep skill edits concise, valid, and under progressive-disclosure control.
2. `zongzu-architecture-boundaries` - align owner-lane, no-ledger, no-manager, and no-foreign-state boundaries through v476.
3. `zongzu-game-design` - keep household mobility preflight as scale-aware planning, not player-as-god movement control.
4. `zongzu-pressure-chain` - keep v469-v476 as preflight governance, not a new runtime branch or event-chain.
5. `zongzu-save-and-schema` - record no save/schema impact and preserve future migration stop-points.
6. `zongzu-simulation-validation` - align validation guidance around the v469-v476 preflight guard.
7. `zongzu-ui-shell` / `zongzu-unity-shell` - keep household-mobility surfaces copy-only from projected fields until real owner-lane rules exist.
8. `zongzu-content-authoring` / `zongzu-ancient-china` - keep wording historically plausible but downstream of authority.
9. `microsoft-code-reference` - align .NET/Unity-facing implementation guidance with current repo facts.

## Current Codebase Facts

- Current `main` is through v476.
- V453-V460 adds runtime `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, and `HouseholdMobilityDynamicsSummary` over existing `PopulationAndHouseholds` pressure signals.
- V461-V468 closes that branch as first-layer explanation only.
- V469-V476 adds docs/tests preflight for future household mobility rule depth. It adds no production behavior.
- `PopulationAndHouseholds` remains the owner for household livelihood/activity/pools and is the default first future owner lane for household mobility depth.
- `PersonRegistry` remains identity plus existing `FidelityRing`; it does not own social rank, migration, movement, route history, or household status drift.
- Application, UI, and Unity still route/assemble/copy projected fields only.
- This pass changes skills and docs only. It does not change `src/`, `tests/`, `unity/`, `content/`, schema versions, migrations, save manifests, scheduler behavior, or Unity assets.

## Scope

- Update `.github/skills` current anchors from v468 to v476 where they describe current mainline state.
- Add v469-v476 household mobility owner-lane preflight language to the relevant Zongzu skill guidance.
- Update `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md` with v476 skill-pack alignment evidence.
- Mirror the repo-tracked skills into the local Codex skill folders after validation.

## Non-Goals

- No runtime rule, command, scheduler, query, projection algorithm, or Unity adapter change.
- No migration economy, route-history model, movement command, commoner/status drift, durable residue, selector watermark, fidelity promotion, or distant per-person simulation.
- No schema bump, persisted state, migration, save manifest, feature-pack save membership, ledger, or projection cache.
- No Application/UI/Unity authority.
- No `PersonRegistry` expansion.
- No prose parsing.

## Schema / Migration

Target schema/migration impact: none.

If future skill-guided work requires persisted mobility history, route history, selector state, commoner status drift, durable residue, target-cardinality state, or projection caches, stop and write a separate owner-module schema/migration plan first.

## Determinism / Performance

- No runtime behavior, scheduler cadence, event flow, command route, save/load path, or projection algorithm changes.
- No performance claim is made by this pass.
- Skill guidance continues to require hot path, touched counts, deterministic cap/order, cadence, schema impact, projection fields, no-touch boundary, and validation before future runtime depth.

## Validation Plan

- `python C:\Users\Xy172\.codex\skills\.system\skill-creator\scripts\quick_validate.py <skill-folder>` for each touched repo skill.
- Scan touched files for replacement characters.
- `git diff --check`.
- `git status --short` to confirm no runtime source/test/Unity/content changes.

Runtime `.NET` tests are intentionally not required because this is skill/doc governance only.

## Milestones

- [x] Read v468 skill-alignment plan and v469-v476 preflight evidence.
- [x] Update repo-tracked skill anchors through v476.
- [x] Update skill rationalization matrix with v476 alignment evidence.
- [x] Run validation plan.
- [x] Mirror local Codex skills if validation passes.

## Completion Evidence

- Updated repo-tracked Zongzu skills and `microsoft-code-reference` from v468 current anchors to v476 current anchors.
- Added v469-v476 household mobility owner-lane preflight guidance that keeps future household movement, route-history, status, selector, durable-residue, and fidelity-promotion work behind owner/cadence/scope/fanout/schema/projection/validation gates.
- Mirrored all touched repo-tracked skills to `C:\Users\Xy172\.codex\skills\...` and verified SHA-256 equality.
- Schema / migration impact: none. No persisted state, schema version, migration, save manifest, module namespace, projection cache, selector state, route-history state, durable residue, or module payload changed.

Validation completed on 2026-04-29:

- `quick_validate.py` passed for all 10 touched repo skill folders using bundled Python with UTF-8 mode.
- `git diff --check` passed.
- Touched-file scan found no replacement characters.
- Local mirror SHA-256 comparison passed for all 10 touched skills.
- `git status --short` shows only `.github/skills`, `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`, and this ExecPlan changed; no `src/`, `tests/`, `unity/`, or `content/` files changed by this pass.

## Rollback

Revert this skill/docs alignment commit. No save migration, production data rollback, or runtime behavior rollback is required.
