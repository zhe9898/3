# Skill Current Main v108 Codebase Alignment

## Purpose

Align the repo-tracked and local skill pack with current `main` after the thin-chain closeout audit through v108.

This pass is governance only. It updates skill instructions and the skill matrix so future Codex work treats v3-v100 as a completed thin skeleton, v101-v108 as closeout evidence, and full-chain rule density as future work. It is not a runtime feature, command system, event pool, projection feature, schema migration, Unity scene pass, or performance optimization.

## Triggered Skills

1. `zongzu-architecture-boundaries`
2. `zongzu-game-design`
3. `zongzu-pressure-chain`
4. `zongzu-ui-shell`
5. `zongzu-ancient-china`
6. `zongzu-content-authoring`
7. `zongzu-unity-shell`
8. `zongzu-simulation-validation`
9. `zongzu-save-and-schema`
10. repo-local `microsoft-code-reference`

## Current Codebase Facts

- `main` is at thin-chain closeout v101-v108.
- v61-v68 adds one narrow `FamilyCore` relief choice using existing schema `8` fields.
- v69-v76 adds Force/Campaign/Regime owner-lane readback through projected fields only.
- v77-v84 adds Warfare directive-choice readback over existing `WarfareCampaign` directive state.
- v85-v92 exposes Warfare aftermath docket readback over existing `WarfareCampaign` schema `4` fields.
- v93-v100 exposes court-policy process readback over existing `OfficeAndCareer` schema `7` and `PublicLifeAndRumor` schema `4` snapshots.
- v101-v108 closes the thin-chain skeleton as audit/evidence only and explicitly preserves full-chain rule-density debt.

## External Calibration

- [Microsoft Learn .NET diagnostic tools](https://learn.microsoft.com/dotnet/core/diagnostics/tools-overview) and [`dotnet-counters`](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters) calibrate first-level runtime evidence for CPU, GC, allocation, memory, and exceptions when runtime behavior changes.
- [Microsoft Learn high-performance logging](https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging) calibrates low-allocation diagnostics; logs remain diagnostics, not player receipts or rule input.
- [Unity Profiler](https://docs.unity3d.com/Manual/profiler-introduction.html), [Unity UI optimization](https://unity.com/how-to/unity-ui-optimization-tips), and [Unity object pooling](https://learn.unity.com/course/design-patterns-unity-6/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity) calibrate Unity shell performance only; they do not move authority into MonoBehaviours.

## Planned Changes

| Area | Change |
| --- | --- |
| Zongzu skills | Replace stale v52/v60 current anchors with v108 thin-chain closeout anchors. |
| `microsoft-code-reference` skill | Update stale module schema table and add current v108 chain/performance anchors. |
| Skill matrix | Update matrix rows from v52/v60 to v108 and add a 2026-04-27 alignment note. |
| Local Codex skills | Mirror repo-tracked skills into `C:\Users\Xy172\.codex\skills`. |

## Impact

| Contract | Impact |
| --- | --- |
| Query | No impact. |
| Command | No impact. |
| DomainEvent | No runtime impact. |
| Projection/read model | No runtime impact. |
| Unity/presentation | No runtime or asset impact. |
| Save/schema | No save/schema impact. No module state, schema version, migration, save manifest, serialized payload, or persisted projection cache changes. |
| Determinism | No determinism impact. Scheduler, event drain, replay, and module state are unchanged. |
| Performance | No runtime/performance validation required. The updated skills require future performance work to name baseline, hot path, cardinality, cap/watermark/cadence, deterministic ordering, and counter/profiler/test lane. |

## Validation Results

- `git diff --check` passed.
- Mojibake pattern scan over touched skill/doc files passed.
- All ten repo-tracked skills still have `SKILL.md`.
- Repo-tracked skills were mirrored into `C:\Users\Xy172\.codex\skills`; SHA-256 hashes match.
- Git status confirms no `src/`, `tests/`, `unity/`, or `content/` files changed.

## Fallback Notes

If a future alignment pass discovers real persisted state changes, stop and run `zongzu-save-and-schema` with schema/migration tests. If a future performance pass touches runtime code, stop treating it as governance-only and use `zongzu-simulation-validation` to choose focused tests, replay/hash checks, long-run diagnostics, or counters/profiling evidence.
