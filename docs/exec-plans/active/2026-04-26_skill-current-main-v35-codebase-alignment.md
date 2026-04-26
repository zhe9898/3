# Skill Current Main v35 Codebase Alignment

## Purpose

Align the repo-tracked and local Zongzu skill pack with the latest `main` codebase after:
- v32 backend event-contract health diagnostics
- v33 no-unclassified event-contract gate
- v34 owner/evidence diagnostic backlinks
- v35 `WorldSettlements.CanalWindowChanged` thin owner-lane handoff into `TradeAndIndustry` and `OrderAndBanditry`

This is a skill/governance/documentation alignment pass only. It is not a runtime feature, projection feature, command system, event pool, UI implementation, schema migration, or Unity asset pass.

## Skill Order

1. `zongzu-architecture-boundaries`
2. `zongzu-game-design`
3. `zongzu-pressure-chain`
4. `zongzu-ui-shell`
5. `zongzu-ancient-china`
6. `zongzu-content-authoring`
7. `zongzu-unity-shell`
8. `zongzu-simulation-validation`
9. `zongzu-save-and-schema`

## Current Code Facts

- `WorldSettlements.CanalWindowChanged` now carries structured `canalWindowBefore` / `canalWindowAfter` metadata.
- `TradeAndIndustry` consumes canal-window events through `IWorldSettlementsQueries` and adjusts existing market, route, and black-route ledger state.
- `OrderAndBanditry` consumes canal-window events through `IWorldSettlementsQueries` and adjusts existing settlement route/order pressure state.
- v35 tests prove structured metadata beats summary/entity prose, exposed water-route/canal loci change, and off-scope settlements/routes remain untouched.
- v32-v34 event-contract health work is diagnostic/test evidence. It classifies event debt, gates `Unclassified` debt, and prints `owner=<module>` / `evidence=<doc-or-test>` readback; it is not runtime authority.

## External Calibration

- [Microsoft Learn .NET performance guidance](https://learn.microsoft.com/dotnet/csharp/advanced-topics/performance/): measure hot paths, allocation risk, and workload before optimizing.
- [Microsoft Learn `dotnet-counters`](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters): first-level CPU, GC, allocation, memory, and exception investigation for runtime behavior changes.
- [Microsoft Learn high-performance logging](https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging): source-generated logging is useful for low-allocation diagnostics, but logs are not player receipts or rule input.
- [Unity Profiler](https://docs.unity3d.com/Manual/profiler-introduction.html), [Unity object pooling](https://learn.unity.com/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity), [Unity UI optimization](https://create.unity.com/Unity-UI-optimization-tips), assembly definition, and asset metadata guidance calibrate the Unity shell only; they do not move authority into MonoBehaviours.

## Changes Planned

| Area | Change |
| --- | --- |
| `.github/skills/zongzu-architecture-boundaries` | Add v32-v35 current anchors, event-contract diagnostic boundary, canal-window owner-lane handoff constraints. |
| `.github/skills/zongzu-game-design` | Distinguish diagnostic evidence from playable loops; frame v35 as a thin handoff, not a thick new system. |
| `.github/skills/zongzu-pressure-chain` | Add v35 as live thin-chain example and prohibit treating v32-v34 diagnostics as pressure chains. |
| `.github/skills/zongzu-simulation-validation` | Add no-unclassified gate, owner/evidence backlinks, and canal-window consumer proof as validation surfaces. |
| `.github/skills/zongzu-save-and-schema` | Record v35 no-save/no-schema impact and current Trade/World/Order schema anchors. |
| `.github/skills/zongzu-ui-shell` | Confirm shell may show projected route/market/order readback but must not compute canal exposure. |
| `.github/skills/zongzu-unity-shell` | Confirm Unity may bind projected readback but must not query modules or inspect event metadata. |
| `.github/skills/zongzu-content-authoring` | Add canal/route/order wording as downstream projection/diagnostic wording. |
| `.github/skills/zongzu-ancient-china` | Add water-network route/order historical carrier framing without universalizing the formula. |
| `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md` | Update matrix rows and performance/scheduling rules for v32-v35. |
| local `C:\Users\Xy172\.codex\skills\zongzu-*` | Mirror the repo-tracked skill files after review. |

## Impact

| Contract | Impact |
| --- | --- |
| Query | No impact. |
| Command | No impact. |
| DomainEvent | No runtime impact; skill text now describes v32-v35 facts. |
| Projection/read model | No runtime impact. |
| Unity/presentation | No runtime or asset impact. |
| Save/schema | No save/schema impact. No module state, schema version, migration, manifest, serialized read history, or persisted metadata changes. |
| Determinism | No determinism impact. Runtime scheduler, event drain, module state, and replay behavior are unchanged. |
| Performance | No runtime/performance validation required for this skill-only pass. The skills now require measured hot-path evidence for future performance work. |

## Validation Results

- `git diff --check` passed for tracked changes.
- New ExecPlan trailing-whitespace check passed.
- All nine repo-tracked Zongzu skills still have `SKILL.md`.
- Repo-tracked skills were mirrored into `C:\Users\Xy172\.codex\skills\zongzu-*`; SHA-256 hashes match.
- Mojibake pattern scan over touched skill/doc files passed.
- Git status confirms no `src/`, `tests/`, `unity/`, or `content/` changes.

## Fallback Notes

If a future skill-alignment pass discovers persisted state changes, stop and use `zongzu-save-and-schema` for schema/version/migration/test planning. If a future performance pass touches scheduler/event drain/projection builders/Unity frame paths, require a named workload, baseline, counter/profiler lane, deterministic replay scope, and owner-module boundary proof before claiming improvement.
