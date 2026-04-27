# Skill Current Branch v148 Codebase Alignment

## Purpose

Align the repo-tracked and local skill pack with the current branch `codex/court-policy-public-reading-echo-v141-v148`.

The previous skill alignment stopped at v108. Current branch history now includes v109-v148 court-policy rule-density/readback work, so skill instructions must reflect the current codebase direction before the next long task.

This is governance only. It updates skill instructions and the skill matrix. It is not a runtime feature, command system, event pool, projection feature, schema migration, Unity scene pass, or performance optimization.

## Current Branch Facts

- v109-v116 adds first-layer court-policy process thickening from structured Office/PublicLife snapshots.
- v117-v124 adds a bounded Office-owned local response affordance through existing command/readback paths.
- v125-v132 adds delayed `SocialMemoryAndRelations` court-policy local-response residue using existing memory records.
- v133-v140 projects old policy-response residue into the next policy window as `政策旧账回压读回`.
- v141-v148 projects old policy-response residue as public interpretation on public-life command readbacks, including `政策公议旧读回` and `公议旧账回声`.
- All v109-v148 work remains schema-neutral and relies on structured Office/PublicLife/SocialMemory snapshots rather than prose parsing.

## External Calibration

- [Microsoft Learn .NET diagnostic tools](https://learn.microsoft.com/dotnet/core/diagnostics/tools-overview) and [`dotnet-counters`](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters) calibrate first-level runtime evidence for CPU, GC, allocation, memory, and exceptions when runtime behavior changes.
- [Microsoft Learn high-performance logging](https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging) calibrates low-allocation diagnostics; logs remain diagnostics, not player receipts or rule input.
- [Unity Profiler](https://docs.unity3d.com/Manual/profiler-introduction.html), [Unity UI optimization](https://unity.com/how-to/unity-ui-optimization-tips), and [Unity object pooling](https://learn.unity.com/course/design-patterns-unity-6/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity) calibrate Unity shell performance only; they do not move authority into MonoBehaviours.

## Impact

| Contract | Impact |
| --- | --- |
| Query | No runtime impact. |
| Command | No runtime impact. |
| DomainEvent | No runtime impact. |
| Projection/read model | No runtime impact. |
| Unity/presentation | No runtime or asset impact. |
| Save/schema | No save/schema impact. No module state, schema version, migration, save manifest, serialized payload, or persisted projection cache changes. |
| Determinism | No determinism impact. Scheduler, event drain, replay, and module state are unchanged. |
| Performance | No runtime/performance validation required. Updated skills require future performance work to name baseline, hot path, cardinality, cap/watermark/cadence, deterministic ordering, and counter/profiler/test lane. |

## Validation Results

- `git diff --check` passed.
- Touched skill/doc files were scanned for replacement-character / UTF-8 mojibake marker patterns; none were found.
- `quick_validate.py` passed for all 10 repo skill folders with `PYTHONUTF8=1`.
- Repo-tracked skill files were mirrored to the local Codex skill directory; all 10 local `SKILL.md` hashes match the repo copies.
- `git status --short` confirms no `src/`, `tests/`, `unity/`, or `content/` files changed.
- No runtime/performance test was required because this pass changes skill governance and documentation only.
