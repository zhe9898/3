# Skill Current Main V380 Codebase Alignment

Date: 2026-04-28

Baseline: `main` at `ef13c94 Close personnel flow future lane surface`.

## Purpose

Align the repo-tracked Zongzu skill pack and local Codex skill pack with the current codebase after the personnel-flow future-lane closeout through v380. This is a governance/skill alignment pass, not a runtime feature.

## Skill Sequence

1. `skill-creator` - keep skill edits concise, scoped, and valid.
2. `zongzu-architecture-boundaries` - align module ownership, Query / Command / DomainEvent, scheduler, save/schema, and performance-boundary guidance.
3. `zongzu-game-design` - align living-world, bounded leverage, fidelity rings, monthly review, personnel-flow future-lane scope, and no-player-as-god guidance.
4. `zongzu-pressure-chain` - align pressure-chain proof language through court-policy, social mobility, regime, and personnel-flow readiness/gate/future-lane preflight.
5. `zongzu-content-authoring` - align Chinese projection wording and keep copy downstream of authority.
6. `zongzu-save-and-schema` - confirm this pass has no persisted-state or migration impact.
7. `zongzu-simulation-validation` - choose skill/doc-only validation instead of runtime tests.
8. `zongzu-ui-shell` - keep Great Hall and Desk Sandbox surfaces projection-only.
9. `zongzu-unity-shell` - keep Unity as host/binding surface only.
10. `zongzu-ancient-china` - keep personnel-flow lane wording as historical/social pressure carrier, not fixed plot or full population simulation.
11. `microsoft-code-reference` - align .NET/Unity-facing implementation guidance with the current repo direction.

## Current Codebase Facts

- Current mainline includes v301-v324 personnel-flow command-readiness/readback, v325-v356 personnel-flow owner-lane gate / desk echo / containment, and v357-v380 future Family/Office/Warfare personnel-flow lane preflight visibility and closeout.
- The repo skills were aligned through v300 but did not yet carry v301-v380 as current facts.
- Active ExecPlans were empty before this pass; the v301-v380 plans are archived and represented in docs.
- This pass changes skills and docs only. It does not change `src/`, `tests/`, `unity/`, or `content/`.

## External Calibration

- Microsoft Learn .NET diagnostics and `dotnet-counters`: CPU, GC, allocation rate, memory, exception-rate, and long-run health should be measured when runtime behavior changes.
- Microsoft Learn high-performance logging: generated/cached logging can reduce hot diagnostic allocations, but logs are not player receipts or authority input.
- Unity CPU Profiler / Unity UI optimization / Unity object pooling guidance: shell performance belongs in Unity presentation implementation and must not move authority into MonoBehaviours.

Reference URLs:

- https://learn.microsoft.com/dotnet/core/diagnostics/tools-overview
- https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters
- https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging
- https://docs.unity3d.com/Manual/profiler-cpu-introduction.html
- https://unity.com/how-to/unity-ui-optimization-tips
- https://learn.unity.com/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity

## Impact Table

| Surface | Impact |
| --- | --- |
| Query / Command / DomainEvent | No runtime contract change. Skills now tell future personnel-flow work to keep readback/preflight separate from command implementation. |
| Owning modules | No module code touched. Skills now anchor Population/PersonRegistry mobility boundaries and keep future Family/Office/Warfare personnel-flow lanes as planned owner lanes. |
| Projection / read model | No runtime projection change. Skills now describe v380 projection/readback surfaces accurately. |
| UI / Unity | No Unity asset or ViewModel code touched. Skills keep Great Hall / Desk Sandbox copy-only and projection-only. |
| Save/schema | No save/schema impact. No persisted state, schema version, migration, manifest, or serialized read-history shape changed. |
| Determinism | No runtime/determinism impact. No scheduler, event drain, replay hash, or command resolution changed. |
| Performance / algorithms | No runtime performance impact. Skill guidance now requires owner module, accepted command, target scope, hot path, expected cardinality, deterministic order/cap, cadence, schema impact, no-touch boundary, projection fields, and validation lane before future personnel-flow rule work. |

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
- [x] Update repo-tracked skills through v380.
- [x] Update matrix with v380 skill-pack alignment.
- [x] Mirror local Codex skills.
- [x] Run validation plan.

## Completion Notes

- `quick_validate.py` passed for all 10 touched repo skill folders.
- Local mirrors under `C:\Users\Xy172\.codex\skills\...` were copied from `.github/skills\...` and SHA-256 compared successfully.
- `git diff --check` passed.
- Added-line scan found no mojibake or replacement characters.
- ExecPlan trailing-whitespace check passed.
- `git status --short` shows only `.github/skills`, `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`, and this ExecPlan changed; no `src/`, `tests/`, `unity/`, or `content/` files changed by this pass.
- No runtime tests were run because this pass is skill/doc governance only.
- Save/schema impact: no save/schema impact.
- Determinism impact: no runtime/determinism impact.
