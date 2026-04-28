# Skill Current Main V468 Codebase Alignment

Date: 2026-04-29

Baseline: current `main` after `499a98a Close household mobility explanation layer`.

## Purpose

Align the repo-tracked Zongzu skill pack and skill orchestration notes with the current codebase through v468. This is a governance/skill alignment pass, not a runtime feature.

V461-V468 closes the v453-v460 household mobility dynamics explanation branch as docs/tests governance only. The skill pack should now describe that boundary directly so future tasks do not mistake household-mobility readback for migration economy depth, route-history state, status drift, direct movement commands, durable residue, or `PersonRegistry` expansion.

## Skill Sequence

1. `skill-creator` - keep skill edits concise, valid, and under progressive-disclosure control.
2. `zongzu-architecture-boundaries` - align owner-lane, no-ledger, no-manager, and no-foreign-state boundaries through v468.
3. `zongzu-game-design` - keep household mobility explanation as scale-aware guidance, not player-as-god movement control.
4. `zongzu-pressure-chain` - keep v461-v468 as closeout governance, not a new runtime branch or event-chain.
5. `zongzu-save-and-schema` - record no save/schema impact and preserve future migration stop-points.
6. `zongzu-simulation-validation` - align validation guidance around the v461-v468 closeout guard.
7. `zongzu-ui-shell` / `zongzu-unity-shell` - keep household-mobility surfaces copy-only from projected fields.
8. `zongzu-content-authoring` / `zongzu-ancient-china` - keep wording historically plausible but downstream of authority.
9. `microsoft-code-reference` - align .NET/Unity-facing implementation guidance with current repo facts.

## Current Codebase Facts

- Current `main` is through v468.
- V453-V460 adds runtime `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, and `HouseholdMobilityDynamicsSummary` over existing `PopulationAndHouseholds` pressure signals.
- V461-V468 closes that branch as first-layer explanation only. It adds docs/tests governance and an architecture guard, not production behavior.
- `PopulationAndHouseholds` remains the owner for household livelihood/activity/pools and household mobility dynamics.
- `PersonRegistry` remains identity plus existing `FidelityRing`; it does not own social rank, migration, movement, route history, or household status drift.
- Application, UI, and Unity still route/assemble/copy projected fields only.
- This pass changes skills and docs only. It does not change `src/`, `tests/`, `unity/`, `content/`, schema versions, migrations, save manifests, scheduler behavior, or Unity assets.

## External Calibration

External sources calibrate implementation discipline only; current repo facts remain authoritative for what exists today.

- Microsoft Learn unit-testing guidance supports fast, isolated, repeatable, self-checking tests and small focused test acts before broad proof.
- Microsoft Learn `dotnet-counters`, high-performance logging, and collection-choice guidance supports first-level performance investigation, low-allocation diagnostics, and data-structure selection by access pattern.
- Unity Manual CPU Profiler, managed-memory, and assembly-definition guidance supports profiler-first shell optimization, low `GC.Alloc` pressure, bounded managed allocations, and explicit Unity assembly boundaries.
- Unity UI optimization guidance supports bounded canvas/UI rebuild work and copy-only shell binding.
- WCAG 2.2 and Xbox Accessibility Guidelines calibrate contrast, non-color cues, status readability, and screen narration for spatialized game surfaces.

Applied skill dimensions:

- Performance / diagnostics: require hot path, touched counts, allocation/GC risk, counter/profiler lane, and collection choice before optimization claims.
- Scheduler / topology: require source pressure, owner state, edge owner, sink owner, deterministic cap/order, drain behavior, repeated-pressure model, and no-touch proof.
- Algorithm / scale: preserve "near detail, far summary"; require active locus, sampled exemplars, aggregate summaries, and player-visible readback before widening detail.
- Boundary / pluggability: keep feature packs manifest-gated and save-explicit; reject runtime plugin marketplaces, UI-loaded rules, and reflection-heavy authority.
- Gameplay: pair simulation math with player levers, uncertainty, tradeoffs, counterpressure, and aftermath readability.
- UI / Unity: keep shell copy-only, bounded by role/locus, profiler-first, accessible, and free of per-frame simulation queries.
- Content / historical calibration: keep stable IDs, tags, bands, provenance, regional variation, uncertainty notes, and projection wording downstream of authority.

Reference URLs:

- https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices
- https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters
- https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging
- https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class
- https://docs.unity3d.com/Manual/profiler-cpu-introduction.html
- https://docs.unity3d.com/Manual/performance-managed-memory-introduction.html
- https://docs.unity3d.com/Manual/assembly-definition-files.html
- https://unity.com/how-to/unity-ui-optimization-tips
- https://www.w3.org/TR/WCAG22/
- https://learn.microsoft.com/gaming/accessibility/xbox-accessibility-guidelines/102
- https://learn.microsoft.com/gaming/accessibility/xbox-accessibility-guidelines/106

## Scope

- Update `.github/skills` current anchors from v460 to v468 where they describe current mainline state.
- Add v461-v468 closeout language to the relevant Zongzu skill guidance.
- Update `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md` with the v468 skill-pack alignment evidence.
- Optionally mirror the repo-tracked skills into the local Codex skill folders after validation.

## Non-Goals

- No runtime rule, command, scheduler, query, projection algorithm, or Unity adapter change.
- No migration economy, route-history model, movement command, commoner/status drift, durable residue, selector watermark, fidelity promotion, or distant per-person simulation.
- No schema bump, persisted state, migration, save manifest, feature-pack save membership, ledger, or projection cache.
- No Application/UI/Unity authority.
- No `PersonRegistry` expansion.
- No prose parsing.

## Schema / Migration

Target schema/migration impact: none.

If future skill-guided work requires persisted mobility history, route history, selector state, commoner status drift, durable residue, target-cardinality state, or projection caches, stop and write a separate owner-module schema/migration plan first.

## Determinism / Performance

- No runtime behavior, scheduler cadence, event flow, command route, save/load path, or projection algorithm changes.
- No performance claim is made by this pass.
- Skill guidance continues to require hot path, touched counts, deterministic cap/order, cadence, schema impact, projection fields, and validation before future runtime depth.

## Validation Plan

- `python C:\Users\Xy172\.codex\skills\.system\skill-creator\scripts\quick_validate.py <skill-folder>` for each touched repo skill.
- Scan touched files for replacement characters.
- `git diff --check`.
- `git status --short` to confirm no runtime source/test/Unity/content changes.

Runtime `.NET` tests are intentionally not required because this is skill/doc governance only.

## Milestones

- [x] Read v460 skill-alignment plan and v461-v468 closeout evidence.
- [x] Update repo-tracked skill anchors through v468.
- [x] Update skill rationalization matrix with v468 alignment evidence.
- [x] Run validation plan.
- [x] Mirror local Codex skills if validation passes.

## Completion Evidence

- Updated repo-tracked Zongzu skills and `microsoft-code-reference` from v460 current anchors to v468 current anchors.
- Added v461-v468 closeout guidance that keeps household mobility explanation bounded to docs/tests governance, existing `PopulationAndHouseholds` signals, and projected/read-model copy-only surfaces.
- Added external calibration notes and concrete skill dimensions for performance, scheduling, algorithms, boundaries, topology expansion, feature-pack pluggability, gameplay, UI/Unity, content scale, and historical calibration using official Microsoft Learn, Unity, W3C, and Xbox Accessibility sources only as engineering discipline references.
- Mirrored all touched repo-tracked skills to `C:\Users\Xy172\.codex\skills\...` and verified SHA-256 equality.
- Schema / migration impact: none. No persisted state, schema version, migration, save manifest, module namespace, projection cache, selector state, route-history state, durable residue, or module payload changed.

Validation completed on 2026-04-29:

- `quick_validate.py` passed for all 10 touched repo skill folders using bundled Python with UTF-8 mode.
- Local mirror SHA-256 comparison passed for all 10 touched skills.
- `git diff --check` passed.
- Touched-file scan found no replacement characters.
- `git status --short` shows only `.github/skills`, `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`, and this ExecPlan changed; no `src/`, `tests/`, `unity/`, or `content/` files changed by this pass.

## Rollback

Revert this skill/docs alignment commit. No save migration, production data rollback, or runtime behavior rollback is required.
