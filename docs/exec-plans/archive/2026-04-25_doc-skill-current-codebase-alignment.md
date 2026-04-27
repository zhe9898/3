# Doc And Skill Current-Codebase Alignment

Date: 2026-04-25

## Goal

Bring repo documentation and local Zongzu skill guidance back in line with the currently checked-out codebase on `codex/public-life-order-leverage-v3`.

This pass is documentation / orchestration only. It does not change simulation rules, module state, command behavior, migrations, read-model shape, Unity assets, or tests.

## Current Code Facts

- Current branch head: `43976cb Implement public-life order refusal residue v5`.
- .NET SDK baseline: `global.json` pins `10.0.202`; all source and test projects target `net10.0`.
- Public-life order closure is now beyond v2:
  - v3 adds runtime-only command leverage / cost / readback projection fields.
  - v4 adds SocialMemory-owned durable public-order residue using existing SocialMemory schema `3`.
  - v5 adds `OrderAndBanditry` schema `8` structured accepted / partial / refused command trace fields and refusal carryover.
- `OrderAndBanditry`, `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` use module-owned `HandleCommand(...)` overrides for migrated command lanes.
- `Application` remains routing/projection assembly; Unity/ViewModels remain projection-copy surfaces.
- `PresentationReadModelBundle.SocialMemories`, player-command leverage/cost/readback fields, and governance receipt readback are read models, not authoritative state.

## Touched Files

- Repo docs:
  - `README.md`
  - `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`
  - `docs/DESIGN_CODE_ALIGNMENT_AUDIT.md`
  - this ExecPlan
- CI/release packaging:
  - `.github/workflows/release-artifacts.yml`
- Local skills:
  - `zongzu-architecture-boundaries`
  - `zongzu-game-design`
  - `zongzu-pressure-chain`
  - `zongzu-ui-shell`
  - `zongzu-unity-shell`
  - `zongzu-content-authoring`
  - `zongzu-ancient-china`
  - `zongzu-save-and-schema`
  - `zongzu-simulation-validation`

## Impact Notes

- Query / Command / DomainEvent impact: none.
- Read-model / projection impact: none.
- Unity / presentation boundary impact: documentation only.
- Save/schema impact: no save/schema impact.
- Determinism impact: none.
- Historical/content impact: wording only; Chinese text is preserved as UTF-8.

## Validation Plan

- `git diff --check`
- No runtime test is required for docs/skill text changes. The release workflow path change is mechanical from current `net10.0` project targets.

## Validation Result

- `git diff --check` passed on 2026-04-25. Git reported the existing README CRLF normalization warning; the actual diff is a one-line path correction, not whole-file line-ending churn.
- Runtime tests were not run because this pass changed docs, local skill guidance, and the release artifact path only; no C# command/query/event/runtime authority changed.

## Status

- [x] Confirm current branch, stash state, and code facts.
- [x] Identify stale `net8.0` release artifact references.
- [x] Update docs and local skills.
- [x] Run validation.
