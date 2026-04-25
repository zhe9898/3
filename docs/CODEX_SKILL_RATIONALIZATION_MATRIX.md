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
- Public-life order closure is implemented through v12 on the current branch: v3 adds runtime-only leverage / cost / readback projections, v4 adds SocialMemory-owned durable public-order residue using existing SocialMemory schema `3`, v5 adds `OrderAndBanditry` schema `8` structured accepted / partial / refused command trace and refusal carryover, v6 adds bounded refusal-response affordances plus owner-owned response traces in `OrderAndBanditry` schema `9`, `OfficeAndCareer` schema `7`, and `FamilyCore` schema `8`, v7 adds SocialMemory-owned response-residue softening / hardening plus later owner-module repeat friction without a schema bump, v8 adds owner-module actor countermoves / passive back-pressure using existing SocialMemory snapshots and existing owner response trace fields without a schema bump, v9 hardens full soft/hard path proof plus minimum playable response affordance/readback acceptance without a schema bump, v10 adds ordinary-household public-life/order after-account readback as runtime `HouseholdSocialPressure` projection only, v11 enriches existing response affordances / receipts with projected ordinary-household stakes without adding persisted state, and v12 adds `PopulationAndHouseholds` schema `3` home-household local response commands/traces without adding `HouseholdId` command targeting or a new order/yamen/social-memory owner.
- Player-command resolution for migrated command lanes uses module-owned `HandleCommand(...)` overrides. `PlayerCommandService` and `PlayerCommandCatalog` route and describe commands; they do not own consequence formulas.
- `PresentationReadModelBundle.SocialMemories`, player-command `LeverageSummary` / `CostSummary` / `ReadbackSummary`, and governance recent-receipt readback are projection/read-model facts. Unity and shell adapters may copy them, not infer or repair them.

## External Calibration Sources

| Source | What It Calibrates | Zongzu Translation |
| --- | --- | --- |
| [Unity Manual: Assembly definitions](https://docs.unity.cn/2023.2/Documentation/Manual/assembly-definition-files.html) | Assembly boundaries and iteration-time dependency control. | Unity shell code can use assembly boundaries for editor/runtime organization, but authority still stays in pure C# simulation modules. |
| [Unity: Organizing your project](https://unity.com/how-to/organizing-your-project) | Unity project folder consistency, `.meta` file discipline, naming conventions. | Keep `unity/Zongzu.UnityShell` organized for scenes/assets/prefabs while preserving repo/module ownership outside Unity. |
| [Unity: Project configuration and assets](https://unity.com/how-to/project-configuration-and-assets) | Asset and project performance configuration. | Apply to presentation assets and shell runtime only; do not move simulation hot paths into Unity lifecycle methods. |
| [Microsoft Learn: Unit testing best practices for .NET](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices) | Fast, isolated, repeatable, self-checking tests; clear test naming and minimal inputs. | Focused module/integration tests should prove ownership, event flow, no-touch boundaries, and deterministic behavior rather than giant brittle snapshots. |
| [WCAG 2.2](https://www.w3.org/TR/WCAG22/) | Perceivable, operable, understandable, robust interface criteria and testable success criteria. | Shell surfaces should preserve contrast, focus/read order, non-color cues, labels, and understandable status changes even when rendered as in-world objects. |
| [Xbox Accessibility Guideline 102: Contrast](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/102) | Game UI contrast for text, controls, symbols, HUD/map-like elements. | Notices, route markers, seals, conflict tokens, and sandbox nodes need contrast and non-color redundancy, not just atmospheric art direction. |
| [Xbox Accessibility Guideline 106: Screen narration](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/106) | Screen narration, labels, roles, values, context/focus/status announcements. | ViewModels/adapters should carry semantic labels and state summaries for object-anchored shell surfaces; Unity-only visuals must not be the only source of meaning. |

## Skill Matrix

| Skill | Primary Use | Repo Fact To Check | External Norm Fit | Must Not Do | Validation Surface |
| --- | --- | --- | --- | --- | --- |
| `zongzu-architecture-boundaries` | Module ownership, scheduler flow, command/query/event seams, save/version contracts, extension boundaries. | `ModuleRunner<TState>`, `MonthlyScheduler`, `GameSimulation`, module `PublishedEvents` / `ConsumedEvents`, boundary docs. | .NET modular testability and explicit dependency discipline. | Add a giant world manager, Application second rule layer, runtime plugin marketplace, or foreign state mutation. | Architecture/integration tests, event-contract tests, `git diff --check`, focused compile/test lane. |
| `zongzu-game-design` | Living-world loop, bounded player leverage, monthly review, actor autonomy, MVP/post-MVP shape. | `PRODUCT_SCOPE.md`, `SIMULATION.md`, `PLAYER_SCOPE.md`, implementation of command receipts and projections. | Game-system proposal discipline: owner, cadence, state, event, projection, player response, tests. | Build an event-pool game, fixed route tree, player-as-god interface, or detached tactics layer. | Acceptance tests, command-resolution tests, projection/receipt tests, long-run diagnostics when balance changes. |
| `zongzu-pressure-chain` | Cross-module pressure propagation, especially Renzong thin/full chains and public-life/order/social-memory closure. | `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`, module event metadata, scheduler drain, off-scope tests, public-life order v3/v4/v5/v6/v7/v8/v9/v10/v11/v12 ExecPlans. | Traceable cause-effect design and minimal focused tests. | Call a chain complete from docs alone, parse summaries as authority, fan out global pressure without locus/allocation, or describe refusal/response residue as an event-pool design. | Integration tests proving propagation, no-touch entities, metadata, projection receipts, and SocialMemory-owned residue when in scope. |
| `zongzu-ancient-china` | Historical grounding, Northern Song/Renzong semantics, anti-anachronism, history-to-rule translation. | Historical claims in docs/content/code, module owner for each historical pressure carrier. | Source calibration and confidence bands. | Turn history into fixed cutscenes, generic dynasty flavor, or UI-owned historical authority. | Source notes, content review, chain/module docs, tests if history changes rules. |
| `zongzu-content-authoring` | Chinese copy, descriptors, authored configs, projection wording, content packs, localization-facing text. | `content/`, docs, projection copy, ViewModel labels, `WRITING_AND_COPY_GUIDELINES.md`. | Content governance, localization hygiene, UTF-8 preservation. | Let prose become authority, replace structured rules with narrative triggers, corrupt Chinese text. | Encoding review, parser/schema tests when content is read by code, focused copy review for text-only changes. |
| `zongzu-ui-shell` | Spatialized shell design: great hall, ancestral hall, desk sandbox, notice tray, conflict/campaign surfaces. | `UI_AND_PRESENTATION.md`, `VISUAL_FORM_AND_INTERACTION.md`, read-model bundle shape. | WCAG/XAG contrast, focus/read order, non-color cues, semantic labels. | Turn UI into dashboard/card wall/spreadsheet, resolve rules in UI, hide essential meaning in visuals only. | Presentation adapter tests, ViewModel review, screenshot/manual checks when UI is runnable. |
| `zongzu-unity-shell` | Unity host implementation, scenes, prefabs, MonoBehaviours, bindings, ViewModels, presentation tests. | `unity/Zongzu.UnityShell`, `src/Zongzu.Presentation.Unity`, `tests/Zongzu.Presentation.Unity.Tests`. | Unity project organization, `.meta` discipline, asmdef/dependency hygiene, asset performance practices. | Let MonoBehaviours own simulation rules, bypass command surface, invent Unity-only facts. | `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore`; Unity editor/play-mode checks only when configured. |
| `zongzu-save-and-schema` | Persisted module state, root/module schema versions, migrations, manifests, feature-pack save membership. | `SCHEMA_NAMESPACE_RULES.md`, `DATA_SCHEMA.md`, module `ModuleSchemaVersion`, save/load tests; currently `OrderAndBanditry` schema `9`, `OfficeAndCareer` schema `7`, `FamilyCore` schema `8`, `PopulationAndHouseholds` schema `3`, and `SocialMemoryAndRelations` schema `3`. | Versioned data and migration discipline. | Bump schema casually, store UI-only facts as authority, mutate foreign state during migration, or treat runtime readback fields as persisted state. | Persistence round-trip, migration tests, manifest tests, determinism checks if backfill changes behavior. |
| `zongzu-simulation-validation` | Determinism, replay, event flow, scheduler cadence, long-run pressure health, acceptance proof. | `MonthlyScheduler`, `ModuleRunner<TState>` metadata, ten-year diagnostics, integration tests. | .NET test best practices: repeatable, isolated, self-checking evidence. | Treat high pressure as automatically wrong, tune blindly, ignore unconsumed/never-emitted events. | Focused tests first, then `dotnet test Zongzu.sln --no-restore` when runtime behavior changes. |

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
- Use source calibration for history, but translate it into pressure carriers, module state, events, projections, and bounded player leverage.
- Treat external references as evidence for "how to implement responsibly," not as permission to change Zongzu's product constraints.

## Red Flags

- A skill answer stops after one narrow definition when the prompt implies a whole-system pass.
- A UI suggestion invents a player-facing fact not present in a projection or ViewModel.
- A pressure-chain suggestion lacks an owner module, settlement/locus, off-scope boundary, event metadata, and a no-touch test.
- A Unity suggestion depends on Editor MCP or scene automation that is not configured.
- A historical suggestion uses named people or reforms as direct gameplay triggers.
- A save/schema suggestion treats read-only helper methods as persisted state.
- A validation suggestion calls `dotnet test` enough proof when the real risk is event topology or long-run pressure saturation.

## Current Impact

This document is governance/orchestration only. It has no runtime, save/schema, determinism, or Unity asset impact.
