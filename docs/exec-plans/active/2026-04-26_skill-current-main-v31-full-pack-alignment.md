# Skill Current Main v31 Full-Pack Alignment

## Purpose

Align the Zongzu skill pack to current `main` after public-life/order v30 and the v31 mainline merge/branch-cleanup pass. This pass makes the repo-tracked skill package match the full nine-skill Zongzu pack instead of leaving five skills only in the local Codex skill directory.

This is a skill/docs governance pass. It is not a runtime feature, command system, projection wording change, event-topology pass, schema migration, Unity asset pass, or performance optimization implementation.

## Current Codebase Facts

- `main` is aligned with `origin/main` at `ec4cc86` when this pass began.
- v30 completes the public-life/order owner-lane closure arc:
  - v20-v26: owner-lane return/surface/entry/status/outcome/social-residue/residue-follow-up guidance.
  - v27: `现有入口读法` affordance echo.
  - v28: `后手收口读回` receipt closure.
  - v29: `闭环防回压` no-loop guard.
  - v30: closure audit evidence.
- v31 is a merge and branch-cleanup evidence pass. It adds no runtime rule, command, projection, schema, migration, or event-topology behavior.
- `.github/skills` previously tracked only four Zongzu skill directories. The local Codex skill directory already had all nine.
- The repo should track all nine Zongzu skills so skill orchestration, future agents, and repository documentation stay portable.

## Skill Package Scope

| Skill | Repo Action |
| --- | --- |
| `zongzu-game-design` | Update current anchors from v24 to v30/v31 and keep public-life/order described as rule-driven command / aftermath / readback, not an event chain. |
| `zongzu-architecture-boundaries` | Update current anchors from v24 to v30/v31 and keep owner-lane/readback/no-loop guidance projection-only. |
| `zongzu-pressure-chain` | Add repo-tracked skill copy and align current anchors to v19-v30 with fanout/no-summary/no-loop proof language. |
| `zongzu-ui-shell` | Update current anchors from v24 to v30 and keep shell surfaces display-only over projected fields. |
| `zongzu-ancient-china` | Update projection-carrier examples through v30 while keeping history as pressure carrier, not fixed trigger. |
| `zongzu-content-authoring` | Add repo-tracked skill copy and align UTF-8 Chinese projection wording examples through v30. |
| `zongzu-unity-shell` | Add repo-tracked skill copy and keep Unity copy-only for v20-v30 ViewModel fields. |
| `zongzu-save-and-schema` | Add repo-tracked skill copy and record v19-v30 owner-lane/readback guidance as no-save/no-schema unless future persisted ledgers or fields appear. |
| `zongzu-simulation-validation` | Add repo-tracked skill copy and align validation guidance for v19-v30 projection-only, no schema bump, no summary parsing, and no Application/UI/Unity outcome calculation. |

## External Calibration Carried Forward

- Microsoft Learn .NET performance guidance calibrates measured hot-path work: name path, cardinality, allocation risk, baseline, and evidence before adding caches or low-level rewrites.
- Microsoft Learn `dotnet-counters` calibrates first-level CPU, GC, allocation, memory, and exception diagnostics for runtime changes.
- Microsoft Learn high-performance logging guidance calibrates low-allocation/source-generated diagnostics when logging touches authority hot paths.
- Unity Profiler, Unity UI optimization, object pooling, asmdef, and `.meta` guidance calibrate shell implementation only; they do not move authority into Unity.
- Accessibility guidance calibrates shell readability and semantic labels; it does not turn Zongzu into a generic dashboard.

## Impact

- Runtime code impact: none.
- Query / Command / DomainEvent impact: none.
- Projection/read-model code impact: none.
- Unity asset / scene impact: none.
- Save/schema impact: no save/schema impact.
- Determinism impact: no determinism impact.
- Performance impact: no runtime performance impact; this updates future performance-validation guidance only.

## Validation Plan

- `git diff --check`
- Verify `.github/skills` includes all nine Zongzu skills.
- Search repo and local skill files for stale v24-only anchors.
- Search updated skill/matrix files for known mojibake fragments around v20-v30 Chinese readback terms.
- No runtime tests are required because this is docs/skills-only.

## Fallback Notes

- If future work changes runtime behavior, create a separate ExecPlan with owning module, scheduler, test, determinism, and save/schema impact.
- If future skill work changes local `C:\Users\Xy172\.codex\skills`, mirror the portable repo-tracked copy under `.github/skills` when it should be shared with the codebase.
