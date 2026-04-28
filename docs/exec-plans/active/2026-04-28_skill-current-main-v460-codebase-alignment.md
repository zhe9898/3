# Skill Current Main V460 Codebase Alignment

Date: 2026-04-28

Baseline: current working tree after v453-v460 household mobility dynamics explanation.

## Purpose

Align the repo-tracked Zongzu skill pack, the local Codex skill mirrors, and skill orchestration notes with the current codebase through v460. This is a governance/skill alignment pass, not a runtime feature.

The pass also expands skill guidance for the dimensions the repo now needs to keep explicit: performance budgets, scheduling cadence, algorithmic cardinality, module boundaries, topology growth, feature-pack/extensibility promises, Unity shell cost, content scale, historical fidelity, and gameplay visibility.

## Skill Sequence

1. `skill-creator` - keep skill edits concise, valid, and under progressive-disclosure control.
2. `zongzu-architecture-boundaries` - align module ownership, Query / Command / DomainEvent seams, scheduler, topology, extension, save/schema, and performance-boundary guidance.
3. `zongzu-game-design` - align world-first gameplay, bounded leverage, fidelity rings, commoner/social-position visibility, household mobility explanation, personnel-flow preflight, and no-player-as-god constraints.
4. `zongzu-pressure-chain` - keep v381-v460 readback/preflight/explanation work distinct from implemented propagation chains.
5. `zongzu-content-authoring` - keep Chinese/content/projection wording downstream of authority and bounded by provenance/cardinality.
6. `zongzu-save-and-schema` - confirm v381-v460 fields are runtime read models and no persisted state is changed.
7. `zongzu-simulation-validation` - define focused validation and record no runtime/performance validation for skill-only edits.
8. `zongzu-ui-shell` - keep Great Hall / Desk Sandbox / dossier surfaces projection-only and scale-aware.
9. `zongzu-unity-shell` - keep Unity ViewModels copy-only, profiler-driven, and free of simulation authority.
10. `zongzu-ancient-china` - keep commoner status, zhuhu/kehu, social position, household mobility dimensions, and fidelity scale as historically grounded pressure carriers, not a universal class engine.
11. `microsoft-code-reference` - align .NET/Unity-facing implementation guidance with current code facts and first-party performance/testing calibration.

## Current Codebase Facts

- Current mainline skills were aligned only through v380; the repo now carries v381-v460 documentation, architecture tests, integration tests, and presentation tests.
- v381-v388 documents commoner/social-position preflight: future owner-lane depth should start in `PopulationAndHouseholds` unless a plan proves otherwise.
- v389-v396 adds runtime `SocialPositionReadbackSummary` on person dossiers.
- v397-v404 adds runtime `SocialPositionSourceModuleKeys` over structured owner-module source keys.
- v405-v412 closes v381-v404 as projection/readback work only.
- v413-v420 adds runtime `SocialPositionScaleBudgetReadbackSummary`.
- v421-v428 adds regional/fidelity scale guard evidence.
- v429-v436 closes v381-v428 as scale-aware projection/readback only.
- v437-v444 documents commoner status owner-lane preflight.
- v445-v452 documents fidelity scale-budget preflight.
- v453-v460 adds runtime `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, and `HouseholdMobilityDynamicsSummary` over existing household pressure signals.
- `PersonRegistry` remains identity plus existing `FidelityRing`; it is not a social-class, status, mobility, or ledger owner.
- Current runtime readbacks are assembled from structured snapshots owned by `FamilyCore`, `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, and `SocialMemoryAndRelations`.
- This pass changes skills and docs only. It does not change `src/`, `tests/`, `unity/`, `content/`, schema versions, migrations, save manifests, scheduler behavior, or Unity assets.

## External Calibration

External sources calibrate implementation discipline only:

- Microsoft Learn unit-testing guidance supports fast, isolated, repeatable, self-checking tests.
- Microsoft Learn `dotnet-counters`, high-performance logging, and collection-choice guidance calibrate hot-path diagnosis, low-allocation diagnostics, and data-structure choice.
- Unity Profiler, managed-memory, assembly-definition, asset-metadata, and UI optimization guidance calibrate shell implementation without moving authority into Unity.
- WCAG 2.2 and Xbox Accessibility Guidelines calibrate contrast, narration, labels, and status readability for spatialized surfaces.

Reference URLs:

- https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices
- https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters
- https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging
- https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class
- https://docs.unity3d.com/Manual/profiler-cpu-introduction.html
- https://docs.unity3d.com/Manual/performance-managed-memory-introduction.html
- https://docs.unity.cn/2023.2/Documentation/Manual/assembly-definition-files.html
- https://docs.unity.cn/Manual/AssetMetadata.html
- https://unity.com/how-to/unity-ui-optimization-tips
- https://www.w3.org/TR/WCAG22/
- https://learn.microsoft.com/gaming/accessibility/xbox-accessibility-guidelines/102
- https://learn.microsoft.com/gaming/accessibility/xbox-accessibility-guidelines/106

## Impact Table

| Surface | Impact |
| --- | --- |
| Query / Command / DomainEvent | No runtime contract change. Skills now describe v381-v460 as readback/preflight/explanation and require future owner-lane work to name command, owner, fanout, metadata, and no-touch proof. |
| Owning modules | No module code touched. Skills now anchor commoner/social-position depth to owner modules, with `PopulationAndHouseholds` as the default future commoner/status owner lane unless a plan proves another owner. |
| Projection / read model | No runtime projection change. Skills now describe `SocialPositionReadbackSummary`, `SocialPositionSourceModuleKeys`, `SocialPositionScaleBudgetReadbackSummary`, `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, and `HouseholdMobilityDynamicsSummary` accurately as runtime read-model/ViewModel fields. |
| UI / Unity | No Unity asset or ViewModel code touched. Skills keep dossier/hall/desk surfaces copy-only and prohibit social-class controls, hidden ranking, or Unity-side rule resolution. |
| Save/schema | No save/schema impact. No persisted state, schema version, migration, manifest, or serialized authoritative cache changed. |
| Determinism | No runtime/determinism impact. No scheduler, event drain, replay hash, or command resolution changed. |
| Performance / algorithms | No runtime performance impact. Skill guidance now requires hot path, touched counts, complexity, ordering/cap, cache ownership/invalidation, cadence, save/schema status, and validation lane before future optimization or fidelity promotion. |
| Extensibility / topology | No runtime extension change. Skills now distinguish additive manifest-gated feature packs from unsafe runtime plugin marketplaces and require topology growth to declare node/edge owners and fanout. |

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

- `python C:\Users\Xy172\.codex\skills\.system\skill-creator\scripts\quick_validate.py <skill-folder>` for each touched repo skill
- compare repo-tracked `SKILL.md` files with local mirrors by SHA-256 after copying
- scan touched files for replacement characters
- `git diff --check`
- `git status --short` to confirm no runtime source/test/Unity/content changes

Runtime `.NET` tests are intentionally not required because this is skill/doc governance only.

## Milestones

- [x] Read mandatory repo docs and current v381-v460 code/doc anchors.
- [x] Check stale v380 skill anchors.
- [x] Update repo-tracked skills through v460.
- [x] Update skill rationalization matrix with v460 orchestration guidance.
- [x] Mirror local Codex skills.
- [x] Run validation plan.

## Completion Notes

- `quick_validate.py` passed for all 10 touched repo skill folders using bundled Python with UTF-8 mode.
- Local mirrors under `C:\Users\Xy172\.codex\skills\...` were copied from `.github/skills\...` and SHA-256 compared successfully.
- `git diff --check` passed.
- Touched-file scan found no replacement characters.
- `git status --short` shows only `.github/skills`, `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`, and this ExecPlan changed; no `src/`, `tests/`, `unity/`, or `content/` files changed by this pass.
- No runtime tests were run because this pass is skill/doc governance only.
- Save/schema impact: no save/schema impact.
- Determinism impact: no runtime/determinism impact.
