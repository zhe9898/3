# Skill External-Calibrated Current-Codebase Alignment

Date: 2026-04-25

## Goal

Align the local Zongzu skill pack, the repo `.github/skills` skill copies, and the repo skill rationalization matrix with the current codebase, while using external official sources only as calibration for architecture, Unity asset structure, testing, accessibility, performance, scheduling, and algorithm boundaries.

## Current Code Facts

- Current branch: `codex/public-life-order-leverage-v3`.
- Current branch head after the concurrent v19 matrix update: `b9d183c Update skill matrix for v19 public-life order`.
- Repo worktree still contains untracked generated Unity art under `unity/Zongzu.UnityShell/Assets/Art/Generated/`; this pass did not edit those assets.
- Unity project version: `6000.3.13f1`.
- .NET SDK baseline: `global.json` pins `10.0.202`.
- Stable public-life/order closure is committed through v19 on the current branch.
- Repo-tracked skill copies currently exist for `zongzu-architecture-boundaries`, `zongzu-game-design`, `zongzu-ancient-china`, and `zongzu-ui-shell` under `.github/skills/`; the remaining Zongzu skills are local Codex skills outside this repository.

## External Calibration Sources

- Microsoft Learn .NET unit testing best practices: fast, isolated, repeatable, self-checking, behavior-readable tests.
- Microsoft Learn C# performance guidance: measure first, optimize repeated hot paths, and treat allocation/copy reduction as evidence-driven work.
- Microsoft Learn `dotnet-counters`: first-level runtime diagnostics for CPU, GC, allocation rate, memory, and exceptions when preview/long-run behavior is the risk.
- Microsoft Learn high-performance logging: prefer source-generated logging or equivalent low-allocation patterns for diagnostics in hot paths.
- Unity Manual assembly definitions: Unity code assembly references may organize shell code, but not move simulation authority into Unity.
- Unity Manual asset metadata and Unity project organization guidance: `.meta` files, import settings, generated assets, manifests, and folder discipline matter for Unity assets.
- Unity Manual Profiler guidance: performance claims in Unity-facing implementation should be based on CPU/memory/render profiling where relevant.
- Unity Learn object pooling guidance: pool repeatedly created/destroyed shell GameObjects or reusable C# entities only after profiling shows churn.
- Unity UI optimization guidance: split dynamic/static canvases, avoid per-frame layout/raycast churn, and pool UI objects carefully for shell surfaces.
- W3C WCAG 2.2: shell text, status, labels, focus/read order, and non-text alternatives should remain testable and technology-independent.
- Xbox Accessibility Guidelines 102 and 106: contrast, non-color redundancy, semantic labels, narration, focus/context/status announcements.

## Skill Updates

- `zongzu-architecture-boundaries`: current v19 architecture facts, hot-path ownership, scheduler/event-drain cardinality, low-allocation diagnostics, and cache/invalidation boundaries.
- `zongzu-game-design`: v19 follow-up affordance framing plus fidelity-ring / scale-budget language for future living-world growth.
- `zongzu-pressure-chain`: public-life/order v3-v19 current chain proof language plus fanout, allocation, watermark, and bounded-complexity checks.
- `zongzu-ui-shell`: projection-only follow-up/readback surfaces plus WCAG/XAG and Unity UI performance calibration.
- `zongzu-unity-shell`: Unity `6000.3.13f1`, `.meta`/generated asset discipline, copy-only ViewModel guidance, profiler/object-pool/UI hot-path rules.
- `zongzu-content-authoring`: UTF-8 Chinese projection wording, generated-asset/content provenance, no prose-as-authority rules, and content cardinality limits.
- `zongzu-ancient-china`: local yamen/household/public-memory phrases as historical pressure carriers, not fixed triggers, plus scale-aware fidelity guidance.
- `zongzu-save-and-schema`: current schema anchors, v19 no-schema-impact rule, bounded migration/load-cost guidance, and no persisted projection-cache rule.
- `zongzu-simulation-validation`: v18/v19 validation lanes plus performance validation lanes using targeted tests, counters, replay/hash, and long-run diagnostics only when runtime behavior changes.

## Impact Notes

- Query / Command / DomainEvent impact: none.
- Runtime simulation impact: none.
- Read-model / projection impact: none from this alignment pass.
- Unity / asset impact: documentation and skill guidance only; no generated asset edits.
- Repo skill impact: `.github/skills` SKILL.md copies and skill-chip metadata now include current v19, fidelity/scale, performance, and shell-boundary guidance where those tracked skills exist.
- Save/schema impact: no save/schema impact.
- Determinism impact: none.
- Performance/runtime impact: no runtime/performance validation required; this pass changes guidance only.

## Validation Plan

- `git diff --check`
- Inspect changed skill files for current public-life/order v18/v19 anchors.
- Inspect repo `.github/skills` copies and local Zongzu skills for branch/current-code alignment.
- Inspect changed skill files and matrix for performance/scheduling/algorithm calibration terms.
- No C# runtime tests are required for skill/doc-only alignment.

## Validation Result

- `git diff --check` passed for the tracked working tree on 2026-04-25.
- Local skill files and repo `.github/skills` copies were inspected with `Select-String` for v18/v19, current schema anchors, Unity `6000.3.13f1`, projection-only, WCAG/XAG, generated-asset guidance, performance hot paths, `dotnet-counters`, profiler guidance, fanout/cardinality, and corrected UTF-8 Chinese readback phrases.
- Runtime tests were not run because this pass changed local skill guidance plus documentation/orchestration only; no runtime/performance validation required.

## Status

- [x] Confirm worktree state and current code facts.
- [x] Calibrate against external official sources.
- [x] Update repo matrix and local skills.
- [x] Run validation for the supplemental performance/scheduling pass.
