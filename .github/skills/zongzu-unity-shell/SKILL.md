---
name: zongzu-unity-shell
description: "Use when a task touches Zongzu's Unity host shell implementation, scenes, prefabs, MonoBehaviours, bindings, ViewModel adapters, presentation tests, Unity project layout, Addressables/assets, shell performance, screenshots, or play-mode/editor validation while keeping simulation authority outside Unity. Do not use for pure UI concept/design work with no Unity-facing implementation."
---

# Zongzu Unity Shell

## Overview

Use this skill when Zongzu work moves from shell design into Unity-facing implementation.

Unity is the host shell and interaction surface. It may display, bind, animate, and collect bounded intents; it must not own authoritative simulation rules.

For product-only surface design, use `zongzu-ui-shell` first. Use this skill when files, tests, or validation touch Unity-facing implementation.

## Current Repo Anchors

The Unity project root already exists at `unity/Zongzu.UnityShell` with `Assets/`, `Packages/`, and `ProjectSettings/`. The current project version file reports Unity `6000.3.13f1`.

That does not move authority into Unity:
- authoritative simulation, scheduler, commands, events, save/load, and read-model composition stay in `src/`
- `Zongzu.Kernel` and simulation modules must not reference Unity APIs
- `src/Zongzu.Presentation.Unity` adapts `PresentationReadModelBundle` / ViewModels and must not resolve domain consequences
- current player-command and governance/office ViewModels may carry projected leverage, cost, readback, SocialMemory residue, home-household local response, v19-v52 owner-lane / Office-lane readback fields, including `OfficeLaneEntryReadbackSummary`, `OfficeLaneReceiptClosureSummary`, `OfficeLaneResidueFollowUpSummary`, and `OfficeLaneNoLoopGuardSummary`; Unity shell code copies those fields only and must not query simulation modules directly or calculate owner-lane return guidance
- current v35-v52 canal-window, Family sponsor, and Office/yamen readback is still outside Unity authority: Unity may bind projected route/market/order/family/office fields, but must not calculate canal exposure, sponsor targeting, affected settlements, Office closure, module event consumers, or command availability from event metadata
- generated shell art currently appears under `unity/Zongzu.UnityShell/Assets/Art/Generated`; Unity asset work must preserve source/provenance manifests and matching `.meta` files
- Unity performance work should start with the Profiler when behavior changes, then apply shell-only fixes such as cached references, object pooling for repeated notice/marker/UI objects, stable canvas/layer structure, and bounded per-frame work
- Unity Editor MCP or live editor automation is not assumed available unless explicitly configured for the task

## External Calibration Anchors

Use Unity guidance as shell implementation discipline:
- Assembly definitions help control Unity-side dependencies and iteration cost; they must preserve the one-way dependency from projected data into presentation.
- Asset metadata guidance means `.meta` files, GUIDs, import settings, provenance manifests, and generated assets move together under version control.
- Profiler and Memory Profiler evidence should precede claims about frame, GC, rendering, or memory improvements when Unity behavior changes.
- Object pooling is for frequently created/destroyed shell rows, markers, and prefabs after churn is expected or measured; it must never pool authoritative simulation state or retained command outcomes.
- UI optimization guidance maps to stable canvas/layer structure, cached component references, bounded row counts, and no per-frame rule or module queries.

## Workflow

1. Confirm this is Unity-facing work.

   Use this skill when the task touches:
   - `unity/Zongzu.UnityShell`
   - `src/Zongzu.Presentation.Unity`
   - `src/Zongzu.Presentation.Unity.ViewModels`
   - `tests/Zongzu.Presentation.Unity.Tests`
   - Unity screenshots, scenes, prefabs, bindings, or editor/play-mode validation

2. Read shell and stack guidance.

   Start with:
   - `docs/VISUAL_FORM_AND_INTERACTION.md`
   - `docs/UI_AND_PRESENTATION.md`
   - `docs/TECH_STACK.md`
   - the active ExecPlan for the surface

   Add spatial skeleton, asset, writing, or modern engineering docs only when the task touches those concerns.

   Pair with `zongzu-ui-shell` for product/layout judgement.

3. Confirm implementation seams.

   Inspect:
   - `src/Zongzu.Presentation.Unity.ViewModels`
   - `src/Zongzu.Presentation.Unity`
   - `tests/Zongzu.Presentation.Unity.Tests`
   - `unity/Zongzu.UnityShell/Assets`
   - `unity/Zongzu.UnityShell/Packages`
   - `unity/Zongzu.UnityShell/ProjectSettings`

   Do not assume Unity Editor MCP or live editor automation exists unless the task explicitly configures it.

4. Classify the Unity work.

   Determine whether the change touches:
   - shared ViewModel DTOs
   - presentation adapters
   - Unity MonoBehaviours/bindings
   - prefab/scene layout
   - asset references, Addressables, or materials
   - input command collection
   - runtime/frame performance, object pooling, canvas rebuild cost, or generated asset footprint
   - screenshot/play-mode validation
   - tests only

5. Keep authority out of Unity.

   A good implementation:
   - reads ViewModels or presentation DTOs
   - sends bounded command intents through the existing command surface
   - never directly references simulation modules from Unity code
   - never recalculates domain rules in MonoBehaviours
   - caches component references and stable IDs instead of repeated scene/string lookups in hot paths
   - pools frequently created/destroyed shell objects only when profiling or expected churn justifies it
   - keeps any pooled object reset presentation-only; it must not retain command outcome, module state, or hidden authority facts
   - keeps labels, visual grouping, and object grammar traceable to read models
   - has tests for adapter/ViewModel behavior before relying on scene polish
   - moves repeated read-only matching into contracts/read-model helpers when appropriate, not into MonoBehaviours
   - shows public-life/order refusal, response, residue, follow-up, owner-lane return, `承接入口`, `归口状态`, `归口后读法`, or Office-lane closure readback only when it is already present in projected command receipts, governance lanes, household pressure, owner-lane surfaces, Office surfaces, or SocialMemory read models
   - shows canal-window route/market/order readback only when it is already present in projected read models or ViewModels, never by querying `WorldSettlements`, `TradeAndIndustry`, or `OrderAndBanditry` directly
   - follows Unity assembly and asset metadata guidance for shell organization without moving authority into Unity lifecycle methods

6. Validate the surface.

   Prefer:
   - adapter/ViewModel unit tests
   - `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore`
   - architecture tests for dependency direction
   - Unity Profiler / Memory Profiler / frame-debug style checks only when Unity implementation performance is in scope and tooling is available
   - scene/prefab/editor checks when Unity tooling is available
   - screenshots only after the surface can actually run

## Output Rules

- Do not put authoritative rules, scheduler logic, or module mutation in Unity code.
- Do not make a Unity-only fact that cannot be traced to read-model state.
- Do not bypass the command surface with direct state writes.
- Do not hide simulation cadence or command resolution in `Update`, `LateUpdate`, `FixedUpdate`, coroutines, animation events, or pooled object reset hooks.
- Do not pool authoritative simulation objects or persisted state. Pool shell rows, markers, prefabs, and view-only payloads.
- Do not turn the shell into generic dashboard panels.
- Prefer ViewModels and adapters over direct module access.
- Prefer stable object anchors: hall, desk, notice tray, route, seal, ledger, marker, visitor.
- Mention when Unity editor validation could not be run.
- Do not require scene or screenshot validation for a pure ViewModel or adapter unit-test change.
