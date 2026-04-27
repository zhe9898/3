# Skill Current Main v24 Performance Alignment

> Superseded note: current `main` has since advanced to the public-life/order v30 closure arc plus the v31 merge/cleanup evidence pass. Keep this file as v24 alignment evidence; use `2026-04-26_skill-current-main-v31-full-pack-alignment.md` for the current full skill-pack state.

## Purpose

Align the Zongzu skill pack and skill rationalization matrix to the current `main` branch after the public-life/order closure through v24. This is a skill/docs governance pass, not a runtime feature, not a new command system, and not a schema migration.

## Current Codebase Facts

- Branch baseline: `main` at `4d5c2e9` (`Merge public-life order leverage v3`), aligned with `origin/main` when this pass started.
- Runtime public-life/order closure is current through v24:
  - v19 adds projection-only repeat/switch/cooldown follow-up affordance hints.
  - v20 adds projection-only `外部后账归位` owner-lane return guidance.
  - v21 carries owner-lane return guidance into Office/Governance and Family-facing surfaces.
  - v22 adds projected `承接入口` handoff wording.
  - v23 adds projected `归口状态` readback.
  - v24 adds projected `归口后读法` outcome reading from existing owner-lane outcome codes.
- `OwnerLaneReturnSurface` remains projection/readback guidance over existing structured snapshots. It does not add command authority, parse summaries, or introduce a new ledger.
- `global.json` pins .NET SDK `10.0.202`.
- Unity host root exists at `unity/Zongzu.UnityShell`, with project version `6000.3.13f1` recorded in the Unity project.
- Repo-tracked skill copies exist for four skills under `.github/skills`: `zongzu-ancient-china`, `zongzu-architecture-boundaries`, `zongzu-game-design`, and `zongzu-ui-shell`.
- Local Codex skill copies exist for all nine Zongzu skills under `C:\Users\Xy172\.codex\skills`.

## External Calibration Sources

- Microsoft Learn: [Reduce memory allocations using C# features](https://learn.microsoft.com/dotnet/csharp/advanced-topics/performance/) calibrates "measure first, optimize hot paths" guidance.
- Microsoft Learn: [dotnet-counters](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters) calibrates first-level CPU, GC, allocation, exception, and memory diagnostics.
- Microsoft Learn: [High-performance logging in .NET](https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging) calibrates source-generated/low-allocation logging guidance.
- Unity Manual: [Profiler introduction](https://docs.unity3d.com/kr/6000.0/Manual/profiler-introduction.html) calibrates Unity shell profiling evidence.
- Unity: [UI optimization tips](https://unity.com/how-to/unity-ui-optimization-tips) calibrates Canvas split, GraphicRaycaster, layout, and UI pooling guidance.
- Unity Learn: [Use object pooling to boost performance of C# scripts in Unity](https://learn.unity.com/course/design-patterns-unity-6/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity) calibrates object pooling as a measured shell/runtime optimization, not default authority storage.

External sources calibrate implementation discipline only. They do not override Zongzu's modular-monolith authority, deterministic scheduler, or spatial shell product direction.

## Skill Alignment Scope

| Skill | Alignment Target |
| --- | --- |
| `zongzu-game-design` | Treat v20-v24 as a rule-driven command / aftermath / SocialMemory / home-household / owner-lane readback loop, not an event chain or event pool. Add fidelity, fanout, and performance-budget prompts for future scale. |
| `zongzu-architecture-boundaries` | Update current anchors from v19 to v24. Keep owner-lane return/readback projection-only. Strengthen scheduler, algorithm, hot-path, cache, projection-helper, and Unity-shell authority boundaries. |
| `zongzu-pressure-chain` | Add v20-v24 proof language: owner-lane guidance is projected readback over existing owner state; chain proof still requires source, locus, fanout, no-touch, and summary-parsing guards. |
| `zongzu-ui-shell` | Add v20-v24 public-life, governance, and family surface guidance: display `外部后账归位`, `承接入口`, `归口状态`, and `归口后读法` only from projected fields. |
| `zongzu-ancient-china` | Restore readable Chinese anchors for yamen, route/order, clan elder, household cost, SocialMemory, and owner-lane return semantics. History remains a pressure carrier, not a fixed trigger. |
| `zongzu-content-authoring` | Restore readable UTF-8 Chinese examples and mark v20-v24 wording as projection copy downstream of structured traces. |
| `zongzu-unity-shell` | Add v20-v24 ViewModel copy-only guidance; Unity must not calculate owner-lane return, status, or outcome reading. |
| `zongzu-simulation-validation` | Add validation guidance for v20-v24: projection-only, no schema bump, no summary parsing, no Application/UI/Unity outcome calculation, focused tests before broad solution tests. |
| `zongzu-save-and-schema` | Record v20-v24 as no-save/no-schema unless a future cooldown ledger, owner-lane ledger, target field, or persisted projection cache is introduced. |

## Impact

- Runtime code impact: none.
- Query / Command / DomainEvent impact: none.
- Read-model / projection impact: documentation only; no code changed.
- Unity / presentation boundary impact: skill guidance only; no Unity assets, scenes, or ViewModels changed.
- Save/schema impact: no save/schema impact.
- Determinism impact: no determinism impact.
- Performance impact: no runtime performance impact; this pass aligns future performance validation language.

## Validation Plan

- `git diff --check`
- Search updated files for current v24 anchors: `外部后账归位`, `承接入口`, `归口状态`, `归口后读法`, `v24`.
- Search updated skill/matrix files for known mojibake fragments around v20-v24 anchor text.
- No runtime test is required because this pass changes docs/skills only.

## Fallback Notes

- If runtime code or persisted state is found to require changes, stop and create a new ExecPlan with owning-module, schema, migration, and validation impact.
- If only repo-tracked skills are requested in a later pass, keep local Codex skills and `.github/skills` sync explicitly scoped so one does not silently drift from the other.
