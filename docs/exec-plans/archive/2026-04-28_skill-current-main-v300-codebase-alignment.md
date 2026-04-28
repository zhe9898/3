# Skill Current Main V300 Codebase Alignment

Date: 2026-04-28

Baseline: `main` at `eeefddc Add personnel command preflight guard (#20)`.

## Purpose

Align the repo-tracked Zongzu skill pack and local Codex skill pack with the current codebase and docs after the v300 personnel-command preflight guard. This is a governance/skill alignment pass, not a runtime feature.

## Skill Sequence

1. `skill-creator` - keep the skills concise, scoped, and valid.
2. `zongzu-architecture-boundaries` - align module ownership, Query / Command / DomainEvent, scheduler, save/schema, and performance-boundary guidance.
3. `zongzu-game-design` - align living-world, bounded leverage, fidelity rings, monthly review, and no-player-as-god guidance.
4. `zongzu-pressure-chain` - align pressure-chain proof language through court-policy, social mobility, regime, and personnel preflight.
5. `zongzu-content-authoring` - align Chinese projection wording and keep copy downstream of authority.
6. `zongzu-save-and-schema` - confirm this pass has no persisted-state or migration impact.
7. `zongzu-simulation-validation` - choose skill/doc-only validation instead of runtime tests.
8. `zongzu-ui-shell` - keep shell surfaces projection-only.
9. `zongzu-unity-shell` - keep Unity as host/binding surface only.
10. `zongzu-ancient-china` - keep regime and mobility wording as historical pressure carriers, not fixed plot.
11. `microsoft-code-reference` - align .NET/Unity-facing implementation guidance with the current repo direction.

## Current Codebase Facts

- Current mainline includes v197-v204 Chain 8 court-policy closeout, v213-v292 social mobility / fidelity-ring / influence readback, v253-v268 Chain 9 regime-legitimacy readback and closeout, and v293-v300 personnel-command preflight.
- The older skill pack mostly stopped at v196. That was stale for current main.
- Active ExecPlans are empty; the v197-v300 plans are archived and represented in `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`.
- This pass changes skills and docs only. It does not change `src/`, `tests/`, `unity/`, or `content/`.

## External Calibration

- Microsoft Learn .NET diagnostics and `dotnet-counters`: CPU, GC, allocation rate, memory, exception-rate, and long-run health should be measured when runtime behavior changes.
- Microsoft Learn high-performance logging: source-generated logging or cached delegates are hot-path diagnostics tools, not player receipts or rule input.
- Unity Profiler / Unity UI optimization / Unity object pooling guidance: shell performance belongs in Unity presentation implementation and must not move authority into MonoBehaviours.

Reference URLs:

- https://learn.microsoft.com/dotnet/core/diagnostics/tools-overview
- https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters
- https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging
- https://docs.unity3d.com/Manual/profiler-cpu-introduction.html
- https://unity.com/en/how-to/unity-ui-optimization-tips
- https://learn.unity.com/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity

## Impact Table

| Surface | Impact |
| --- | --- |
| Query / Command / DomainEvent | No runtime contract change. Skills now tell future work to keep v300 personnel preflight separate from command implementation. |
| Owning modules | No module code touched. Skills now anchor Population/PersonRegistry mobility boundaries, Office/PublicLife regime readback, and no direct personnel command authority. |
| Projection / read model | No runtime projection change. Skills now describe v300 projection/readback surfaces accurately. |
| UI / Unity | No Unity asset or ViewModel code touched. Skills keep UI/Unity copy-only and projection-only. |
| Save/schema | No save/schema impact. No persisted state, schema version, migration, manifest, or serialized read-history shape changed. |
| Determinism | No runtime/determinism impact. No scheduler, event drain, replay hash, or command resolution changed. |
| Performance / algorithms | No runtime performance impact. Skill guidance now requires hot path, cardinality, deterministic ordering, cap/watermark/cadence, invalidation, and measurement before future optimization claims. |

## Files Planned

- `.github/skills/microsoft-code-reference/SKILL.md`
- `.github/skills/zongzu-architecture-boundaries/SKILL.md`
- `.github/skills/zongzu-game-design/SKILL.md`
- `.github/skills/zongzu-pressure-chain/SKILL.md`
- `.github/skills/zongzu-content-authoring/SKILL.md`
- `.github/skills/zongzu-save-and-schema/SKILL.md`
- `.github/skills/zongzu-simulation-validation/SKILL.md`
- `.github/skills/zongzu-ui-shell/SKILL.md`
- `.github/skills/zongzu-unity-shell/SKILL.md`
- `.github/skills/zongzu-ancient-china/SKILL.md`
- `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`
- local mirrors under `C:\Users\Xy172\.codex\skills\...`

## Validation Plan

- `git diff --check`
- quick skill validation for each touched repo skill using the bundled `skill-creator` validator
- scan touched repo files for newly introduced mojibake or replacement characters
- copy repo-tracked skill `SKILL.md` files into the local Codex skill folders and compare SHA-256
- `git status --short` to confirm no runtime source/test/Unity/content changes

Runtime `.NET` tests are intentionally not required because this is skill/doc governance only.

## Milestones

- [x] Identify current branch and latest mainline anchors.
- [x] Read matrix and stale skill anchors.
- [x] Update repo-tracked skills through v300.
- [x] Update matrix with v300 skill-pack alignment.
- [x] Mirror local Codex skills.
- [x] Run validation plan.

## Completion Notes

- `quick_validate.py` passed for all 10 touched repo skill folders with bundled Python and `PYTHONUTF8=1` so UTF-8 skill files are read correctly on Windows.
- Local mirrors under `C:\Users\Xy172\.codex\skills\...` were copied from `.github/skills\...` and SHA-256 compared successfully.
- `git diff --check` passed.
- Added-line scan found no mojibake or replacement characters.
- `git status --short` shows only `.github/skills`, `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`, and this ExecPlan changed; no `src/`, `tests/`, `unity/`, or `content/` files changed.
- No runtime tests were run because this pass is skill/doc governance only.
- Save/schema impact: no save/schema impact.
- Determinism impact: no runtime/determinism impact.
