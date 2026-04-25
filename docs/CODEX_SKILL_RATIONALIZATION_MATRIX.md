# CODEX_SKILL_RATIONALIZATION_MATRIX

## Purpose

This matrix keeps the Zongzu Codex skill pack aligned with the current repository, external engineering/accessibility norms, and the game's actual direction.

Use it when:
- a prompt names a Zongzu skill but the task spans adjacent systems;
- a task is short or compressed and needs whole-skill expansion;
- a code or design pass risks drifting into a giant world manager, UI rule layer, event-pool design, or detached tactics game;
- an external "best practice" sounds useful but must be translated into Zongzu's modular-monolith and spatialized-shell rules.

External standards calibrate the skill pack; they do not override product or architecture authority. Zongzu remains a deterministic .NET modular monolith with a Unity presentation shell, bounded player leverage, and a Northern Song-inspired living-world direction.

## Current Repo Anchors

- Authoritative simulation lives under `src/` as a .NET modular monolith.
- `Zongzu.Kernel` and simulation modules must not reference Unity APIs.
- Unity host root exists at `unity/Zongzu.UnityShell` with `Assets/`, `Packages/`, and `ProjectSettings/`.
- `src/Zongzu.Presentation.Unity` and `src/Zongzu.Presentation.Unity.ViewModels` are projection/adaptation layers.
- Cross-module cooperation uses Query / Command / DomainEvent.
- `MonthlyScheduler` performs deterministic prepare / xun / month / bounded fresh-event drain / projection ordering.
- Notifications are read-side projection artifacts, not authority drivers.
- Read-only helpers may normalize traversal of existing read models, but may not become ranking, visibility, scheduler, command, or authority policy.
- Public-life order closure is implemented through v23 on the current branch: v3-v5 establish leverage/cost/readback, durable SocialMemory residue, and structured Order refusal traces; v6-v9 add bounded response affordances, owner-owned response traces, SocialMemory soft/hard residue, actor countermoves, and repeat-friction proof; v10-v11 add runtime ordinary-household after-account readback/stakes; v12 adds `PopulationAndHouseholds` schema `3` home-household local response commands/traces; v13-v14 route that aftermath through existing SocialMemory memory and later household repeat-friction paths; v15-v18 add common-household texture, capacity affordance, tradeoff forecast, and short-term receipt readback; v19 adds projection-only repeat/switch/cooldown follow-up affordance hints; v20 adds projection-only `外部后账归位` owner-lane return guidance; v21 carries that guidance into Office/Governance and Family-facing surfaces; v22 adds projected `承接入口` wording that points back to existing owner-lane affordances; v23 adds projection-only `归口状态` readback from existing owner-module response traces, still without schema changes.
- Player-command resolution for migrated command lanes uses module-owned `HandleCommand(...)` overrides. `PlayerCommandService` and `PlayerCommandCatalog` route and describe commands; they do not own consequence formulas.
- `PresentationReadModelBundle.SocialMemories`, player-command `LeverageSummary` / `CostSummary` / `ReadbackSummary`, and governance recent-receipt readback are projection/read-model facts. Unity and shell adapters may copy them, not infer or repair them.

## External Calibration Sources

| Source | What It Calibrates | Zongzu Translation |
| --- | --- | --- |
| [Unity Manual: Assembly definitions](https://docs.unity.cn/2023.2/Documentation/Manual/assembly-definition-files.html) | Assembly boundaries and iteration-time dependency control. | Unity shell code can use assembly boundaries for editor/runtime organization, but authority still stays in pure C# simulation modules. |
| [Unity Manual: Asset Metadata](https://docs.unity.cn/Manual/AssetMetadata.html) | `.meta` files, asset IDs, import settings, and version-control hazards. | Generated Unity art and shell assets must keep provenance, manifests, and matching `.meta` discipline; generated assets never become simulation authority. |
| [Unity: Organizing your project](https://unity.com/how-to/organizing-your-project) | Unity project folder consistency, `.meta` file discipline, naming conventions. | Keep `unity/Zongzu.UnityShell` organized for scenes/assets/prefabs while preserving repo/module ownership outside Unity. |
| [Unity: Project configuration and assets](https://unity.com/how-to/project-configuration-and-assets) | Asset and project performance configuration. | Apply to presentation assets and shell runtime only; do not move simulation hot paths into Unity lifecycle methods. |
| [Microsoft Learn: Unit testing best practices for .NET](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices) | Fast, isolated, repeatable, self-checking tests; clear test naming and minimal inputs. | Focused module/integration tests should prove ownership, event flow, no-touch boundaries, and deterministic behavior rather than giant brittle snapshots. |
| [Microsoft Learn: Reduce memory allocations using C# features](https://learn.microsoft.com/dotnet/csharp/advanced-topics/performance/) | Measure first, optimize hot paths, reduce allocations/copies only where repeated execution proves cost. | Scheduler, event drain, projection builders, migrations, and long-run diagnostics should name cardinality and allocation risk before adding broad caches or low-level rewrites. |
| [Microsoft Learn: dotnet-counters](https://learn.microsoft.com/dotnet/core/diagnostics/dotnet-counters) | First-level runtime performance monitoring for CPU, GC, allocation rate, exceptions, and memory. | Use counters for preview/long-run health investigations when runtime behavior changes; do not turn doc-only or skill-only edits into fake performance validation. |
| [Microsoft Learn: High-performance logging in .NET](https://learn.microsoft.com/dotnet/core/extensions/logging/high-performance-logging) | Source-generated logging avoids boxing, temporary allocations, and runtime template parsing. | Diagnostics in hot authority paths should be module-attributed and low-allocation; player-facing receipts remain projections, not logs. |
| [Unity Manual: Profiler introduction](https://docs.unity3d.com/kr/6000.0/Manual/profiler-introduction.html) | Unity profiling finds CPU, memory, rendering, audio, and frame spikes in the actual player/editor target. | Unity shell performance claims need profiling evidence when implementation changes; profiler data cannot justify moving authority into MonoBehaviours. |
| [Unity Learn: Object pooling in Unity 6](https://learn.unity.com/tutorial/use-object-pooling-to-boost-performance-of-c-scripts-in-unity) | Pool frequently created/destroyed GameObjects or reusable C# entities after profiling shows churn. | Pool notice rows, markers, and shell objects only in Unity/presentation; do not pool or cache mutable simulation state as authority. |
| [Unity: UI optimization tips](https://create.unity.com/Unity-UI-optimization-tips) | Canvas splitting, GraphicRaycaster limits, layout-group caution, UI object pooling, and efficient hiding. | Great hall, desk, notice tray, and public-life surfaces should use precomputed ViewModels and stable canvases; UI must not scan histories or recompute rules every frame. |
| [WCAG 2.2](https://www.w3.org/TR/WCAG22/) | Perceivable, operable, understandable, robust interface criteria and testable success criteria. | Shell surfaces should preserve contrast, focus/read order, non-color cues, labels, and understandable status changes even when rendered as in-world objects. |
| [Xbox Accessibility Guideline 102: Contrast](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102) | Game UI contrast for text, controls, symbols, HUD/map-like elements. | Notices, route markers, seals, conflict tokens, and sandbox nodes need contrast and non-color redundancy, not just atmospheric art direction. |
| [Xbox Accessibility Guideline 106: Screen narration](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/106) | Screen narration, labels, roles, values, context/focus/status announcements. | ViewModels/adapters should carry semantic labels and state summaries for object-anchored shell surfaces; Unity-only visuals must not be the only source of meaning. |

## Skill Matrix

| Skill | Primary Use | Repo Fact To Check | External Norm Fit | Must Not Do | Validation Surface |
| --- | --- | --- | --- | --- | --- |
| `zongzu-architecture-boundaries` | Module ownership, scheduler flow, command/query/event seams, save/version contracts, extension boundaries, hot-path ownership. | `ModuleRunner<TState>`, `MonthlyScheduler`, `GameSimulation`, module `PublishedEvents` / `ConsumedEvents`, boundary docs, projection contexts. | .NET modular testability, explicit dependency discipline, measured hot-path optimization, low-allocation diagnostics. | Add a giant world manager, Application second rule layer, runtime plugin marketplace, foreign state mutation, or global cache that changes determinism. | Architecture/integration tests, event-contract tests, `git diff --check`, focused compile/test lane, counter/profiler evidence when performance behavior changes. |
| `zongzu-game-design` | Living-world loop, bounded player leverage, monthly review, actor autonomy, MVP/post-MVP shape, fidelity/scale budget. | `PRODUCT_SCOPE.md`, `SIMULATION.md`, `SIMULATION_FIDELITY_MODEL.md`, `PLAYER_SCOPE.md`, implementation of command receipts and projections. | Game-system proposal discipline: owner, cadence, state, event, projection, player response, tests, cardinality/fanout budget. | Build an event-pool game, fixed route tree, player-as-god interface, detached tactics layer, or full-map high-fidelity simulation by default. | Acceptance tests, command-resolution tests, projection/receipt tests, long-run diagnostics when balance or scale changes. |
| `zongzu-pressure-chain` | Cross-module pressure propagation, especially Renzong thin/full chains and public-life/order/social-memory closure. | `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`, module event metadata, scheduler drain, off-scope tests, public-life order v3-v23 ExecPlans. | Traceable cause-effect design, minimal focused tests, explicit fanout/allocation proof. | Call a chain complete from docs alone, parse summaries as authority, fan out global pressure without locus/allocation, or describe refusal/response residue as an event-pool design. | Integration tests proving propagation, no-touch entities, metadata, projection receipts, SocialMemory-owned residue when in scope, and bounded chain complexity. |
| `zongzu-ancient-china` | Historical grounding, Northern Song/Renzong semantics, anti-anachronism, history-to-rule translation. | Historical claims in docs/content/code, module owner for each historical pressure carrier, fidelity ring for each historical scale. | Source calibration, confidence bands, scale-aware abstraction. | Turn history into fixed cutscenes, generic dynasty flavor, UI-owned historical authority, or over-modeled named-person density everywhere. | Source notes, content review, chain/module docs, tests if history changes rules. |
| `zongzu-content-authoring` | Chinese copy, descriptors, authored configs, projection wording, content packs, localization-facing text. | `content/`, docs, projection copy, ViewModel labels, `WRITING_AND_COPY_GUIDELINES.md`, generated asset manifests. | Content governance, localization hygiene, UTF-8 preservation, cardinality/provenance control. | Let prose become authority, replace structured rules with narrative triggers, corrupt Chinese text, or grow content into an unbounded event pool. | Encoding review, parser/schema tests when content is read by code, focused copy review for text-only changes, manifest/provenance review for generated assets. |
| `zongzu-ui-shell` | Spatialized shell design: great hall, ancestral hall, desk sandbox, notice tray, conflict/campaign surfaces. | `UI_AND_PRESENTATION.md`, `VISUAL_FORM_AND_INTERACTION.md`, read-model bundle shape, projection contexts such as desk/hall selectors. | WCAG/XAG contrast, focus/read order, non-color cues, semantic labels, Unity UI performance discipline. | Turn UI into dashboard/card wall/spreadsheet, resolve rules in UI, scan long histories per frame, or hide essential meaning in visuals only. | Presentation adapter tests, ViewModel review, screenshot/manual checks when UI is runnable, profiling only when implementation/performance changes. |
| `zongzu-unity-shell` | Unity host implementation, scenes, prefabs, MonoBehaviours, bindings, ViewModels, presentation tests. | `unity/Zongzu.UnityShell`, `src/Zongzu.Presentation.Unity`, `tests/Zongzu.Presentation.Unity.Tests`, Unity `6000.3.13f1`. | Unity project organization, `.meta` discipline, asmdef/dependency hygiene, Profiler/ObjectPool/UI Canvas performance practices. | Let MonoBehaviours own simulation rules, bypass command surface, invent Unity-only facts, hide simulation cadence in `Update`, or pool authoritative state. | `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore`; Unity profiler/editor/play-mode checks only when configured and relevant. |
| `zongzu-save-and-schema` | Persisted module state, root/module schema versions, migrations, manifests, feature-pack save membership. | `SCHEMA_NAMESPACE_RULES.md`, `DATA_SCHEMA.md`, module `ModuleSchemaVersion`, save/load tests; currently `OrderAndBanditry` schema `9`, `OfficeAndCareer` schema `7`, `FamilyCore` schema `8`, `PopulationAndHouseholds` schema `3`, and `SocialMemoryAndRelations` schema `3`. | Versioned data, migration discipline, bounded load/migration cost. | Bump schema casually, store UI-only facts as authority, persist denormalized projection caches without contract, mutate foreign state during migration, or treat runtime readback fields as persisted state. | Persistence round-trip, migration tests, manifest tests, determinism checks if backfill changes behavior, load-size/perf diagnostics when migration shape grows. |
| `zongzu-simulation-validation` | Determinism, replay, event flow, scheduler cadence, long-run pressure health, acceptance proof, performance diagnostics. | `MonthlyScheduler`, `ModuleRunner<TState>` metadata, ten-year diagnostics, integration tests, replay hash, counters when needed. | .NET test best practices: repeatable, isolated, self-checking evidence; `dotnet-counters` for first-level runtime metrics. | Treat high pressure as automatically wrong, tune blindly, ignore unconsumed/never-emitted events, or claim performance improvement without baseline. | Focused tests first, then `dotnet test Zongzu.sln --no-restore` when runtime behavior changes; optional counters/profiling for long-run or allocation-sensitive changes. |

## Default Sequencing

Use skills as one connected pass rather than isolated summaries:

| Task Shape | Recommended Skill Order |
| --- | --- |
| Broad codebase alignment | `zongzu-architecture-boundaries` -> `zongzu-game-design` -> `zongzu-pressure-chain` -> `zongzu-ui-shell` -> `zongzu-simulation-validation`; add `zongzu-ancient-china` for Renzong/historical semantics and `zongzu-save-and-schema` for persisted state. |
| Renzong / historical pressure | `zongzu-ancient-china` -> `zongzu-game-design` -> `zongzu-pressure-chain` -> `zongzu-architecture-boundaries` -> `zongzu-simulation-validation`. |
| Read-model / notification helper | `zongzu-architecture-boundaries` -> `zongzu-ui-shell`; add `zongzu-simulation-validation` only if behavior/tests need proof. |
| Unity presentation implementation | `zongzu-ui-shell` -> `zongzu-unity-shell` -> `zongzu-architecture-boundaries`; add accessibility calibration from WCAG/XAG. |
| Save or schema work | `zongzu-save-and-schema` -> `zongzu-architecture-boundaries` -> `zongzu-simulation-validation`. |
| Chinese copy or content | `zongzu-content-authoring` -> `zongzu-ancient-china` -> `zongzu-ui-shell` when player-facing surfaces are involved. |

## External Norm Translation Rules

- Use Unity assembly definitions and folder standards to keep shell implementation modular, not to make Unity the simulation host.
- Use accessibility standards to strengthen shell readability, narration, contrast, focus order, and semantic labels; do not flatten the shell into a generic web dashboard.
- Use .NET test best practices to keep tests readable and behavior-focused; do not replace cross-module evidence with snapshot mass.
- Use .NET performance guidance as a measured hot-path discipline: name the path, cardinality, allocation risk, and baseline before adding caches, pools, `Span`/`Memory`, or ref-style rewrites.
- Use `dotnet-counters` / runtime metrics for preview-run and long-run health investigations when CPU, GC, allocation rate, or exception rate is the actual risk.
- Use source-generated or otherwise low-allocation logging only for diagnostics; logs are not player receipts and never become rule input.
- Use Unity Profiler, object pooling, Canvas splitting, and UI hot-path advice inside the Unity shell only; simulation authority remains pure C# and projection-driven.
- Use source calibration for history, but translate it into pressure carriers, module state, events, projections, and bounded player leverage.
- Treat external references as evidence for "how to implement responsibly," not as permission to change Zongzu's product constraints.

## Performance / Scheduling / Algorithm Rules

- Every non-trivial system proposal should name its expected cardinality: settlements, clans, households, people, events, notices, routes, or UI objects touched per day/month/frame.
- Scheduler work should remain ordered, bounded, and deterministic. New drains, recovery passes, or recurring-demand models need a cap, watermark, or explicit cadence.
- Pressure chains must prove fanout: source -> locus -> affected owners -> no-touch boundary. "All settlements react" is a missing allocation rule unless deliberately specified and tested.
- Projection and shell code may build one-pass indexes over already-built read models. They may not re-scan long histories in every adapter, frame, or Unity binding.
- Performance caches are allowed only when their authority, invalidation point, determinism impact, and save/schema status are explicit.
- Full fidelity belongs near the player's focus ring or current pressure hit. Distant society should stay alive through summarized pressure until visibility or leverage justifies promotion.
- Migration/load paths are hot when old saves are opened; schema growth should consider payload size, deterministic defaults, and migration test cost.
- For doc-only, skill-only, or wording-only changes, record `no runtime/performance validation required` rather than running unrelated tests.

## Red Flags

- A skill answer stops after one narrow definition when the prompt implies a whole-system pass.
- A UI suggestion invents a player-facing fact not present in a projection or ViewModel.
- A pressure-chain suggestion lacks an owner module, settlement/locus, off-scope boundary, event metadata, and a no-touch test.
- A Unity suggestion depends on Editor MCP or scene automation that is not configured.
- A historical suggestion uses named people or reforms as direct gameplay triggers.
- A save/schema suggestion treats read-only helper methods as persisted state.
- A validation suggestion calls `dotnet test` enough proof when the real risk is event topology or long-run pressure saturation.
- A performance suggestion adds a cache, pool, or index without naming authority, invalidation, determinism, cardinality, and save/schema impact.
- A shell optimization moves rule resolution into Unity or scans projection history from `Update`.
- A historical or content expansion creates dense named actors everywhere instead of using focus rings and pressure-driven promotion.

## Current Impact

This document is governance/orchestration only. It has no runtime, save/schema, determinism, or Unity asset impact.
