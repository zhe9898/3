# Skill External-Calibrated Current-Codebase Alignment

Date: 2026-04-25

## Goal

Align the local Zongzu skill pack and the repo skill rationalization matrix with the current codebase, while using external official sources only as calibration for architecture, Unity asset structure, testing, and accessibility.

## Current Code Facts

- Current branch: `codex/public-life-order-leverage-v3`.
- Current branch head after the concurrent v19 update: `1006dc5 Add home-household follow-up affordance readback`.
- Repo worktree still contains untracked generated Unity art under `unity/Zongzu.UnityShell/Assets/Art/Generated/`; this pass did not edit those assets.
- Unity project version: `6000.3.13f1`.
- .NET SDK baseline: `global.json` pins `10.0.202`.
- Stable public-life/order closure is committed through v19 on the current branch.

## External Calibration Sources

- Microsoft Learn .NET unit testing best practices: fast, isolated, repeatable, self-checking, behavior-readable tests.
- Unity Manual assembly definitions: Unity code assembly references may organize shell code, but not move simulation authority into Unity.
- Unity Manual asset metadata and Unity project organization guidance: `.meta` files, import settings, generated assets, manifests, and folder discipline matter for Unity assets.
- W3C WCAG 2.2: shell text, status, labels, focus/read order, and non-text alternatives should remain testable and technology-independent.
- Xbox Accessibility Guidelines 102 and 106: contrast, non-color redundancy, semantic labels, narration, focus/context/status announcements.

## Skill Updates

- `zongzu-architecture-boundaries`: current v19 architecture facts and external calibration boundaries.
- `zongzu-game-design`: v19 follow-up affordance framing as current projection-only playability.
- `zongzu-pressure-chain`: public-life/order v3-v19 current chain proof language.
- `zongzu-ui-shell`: projection-only follow-up/readback surfaces plus WCAG/XAG calibration.
- `zongzu-unity-shell`: Unity `6000.3.13f1`, `.meta`/generated asset discipline, and copy-only ViewModel guidance.
- `zongzu-content-authoring`: UTF-8 Chinese projection wording, generated-asset/content provenance, and no prose-as-authority rules.
- `zongzu-ancient-china`: local yamen/household/public-memory phrases as historical pressure carriers, not fixed triggers.
- `zongzu-save-and-schema`: current schema anchors and v19 no-schema-impact rule.
- `zongzu-simulation-validation`: v18/v19 validation lanes and when runtime tests are unnecessary for skill-only edits.

## Impact Notes

- Query / Command / DomainEvent impact: none.
- Runtime simulation impact: none.
- Read-model / projection impact: none from this alignment pass.
- Unity / asset impact: documentation and skill guidance only; no generated asset edits.
- Save/schema impact: no save/schema impact.
- Determinism impact: none.

## Validation Plan

- `git diff --check`
- Inspect changed skill files for current public-life/order v18/v19 anchors.
- No C# runtime tests are required for skill/doc-only alignment.

## Validation Result

- `git diff --check` passed for the tracked working tree on 2026-04-25.
- Local skill files were inspected with `Select-String` for v18/v19, current schema anchors, Unity `6000.3.13f1`, projection-only, WCAG/XAG, and generated-asset guidance.
- Runtime tests were not run because this pass changed local skill guidance plus documentation/orchestration only.

## Status

- [x] Confirm worktree state and current code facts.
- [x] Calibrate against external official sources.
- [x] Update repo matrix and local skills.
- [x] Run validation.
