# MODERN_GAME_ENGINEERING_STANDARDS

This document bridges Zongzu's project-specific rules with modern game-engineering practice.

It is the common checkpoint for:
- code standards
- module standards
- system standards
- Unity presentation standards
- content and rules-data standards
- performance, diagnostics, and delivery standards

It complements rather than replaces `ENGINEERING_RULES.md`, `ARCHITECTURE.md`, `MODULE_BOUNDARIES.md`, `MODULE_INTEGRATION_RULES.md`, and `STATIC_BACKEND_FIRST.md`.

## Existing Coverage Audit

Current docs are already broad. The project already has:
- product and scope standards: `PRODUCT_SCOPE.md`, `MVP_SCOPE.md`, `POST_MVP_SCOPE.md`, `VERSION_ALIGNMENT.md`
- whole-system and roadmap standards: `FULL_SYSTEM_SPEC.md`, `GAME_DEVELOPMENT_ROADMAP.md`, `IMPLEMENTATION_PHASES.md`, `CODEX_MASTER_SPEC.md`
- engineering standards: `TECH_STACK.md`, `ENGINEERING_RULES.md`, `STATIC_BACKEND_FIRST.md`
- architecture standards: `ARCHITECTURE.md`, `MODULE_BOUNDARIES.md`, `MODULE_INTEGRATION_RULES.md`, `EXTENSIBILITY_MODEL.md`
- schema standards: `SCHEMA_NAMESPACE_RULES.md`, `DATA_SCHEMA.md`
- cadence and simulation standards: `SIMULATION.md`, `SIMULATION_FIDELITY_MODEL.md`, `MODULE_CADENCE_MATRIX.md`
- player and society standards: `PLAYER_SCOPE.md`, `SOCIAL_STRATA_AND_PATHWAYS.md`, `MULTI_ROUTE_DESIGN_MATRIX.md`, `INFLUENCE_POWER_AND_FACTIONS.md`
- history and living-world standards: `RULES_DRIVEN_LIVING_WORLD.md`, `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`, `LIVING_WORLD_DESIGN.md`
- presentation standards: `VISUAL_FORM_AND_INTERACTION.md`, `UI_AND_PRESENTATION.md`, `MAP_AND_SANDBOX_DIRECTION.md`, `SPATIAL_SKELETON_SPEC.md`, `WRITING_AND_COPY_GUIDELINES.md`, `ART_AND_AUDIO_ASSET_SOURCING.md`
- verification standards: `ACCEPTANCE_TESTS.md`, `DESIGN_CODE_ALIGNMENT_AUDIT.md`

Coverage is strong for architecture, simulation truth, module boundaries, historical process, player scope, and UI shell direction.

The main gaps before this document were:
- no single modern game-engineering standard index
- no explicit bridge from Microsoft C# / .NET analyzer practice to Zongzu's C# code policy
- no explicit bridge from Unity project-organization and performance guidance to Zongzu's presentation layer
- no standard system proposal block that every new feature can reuse
- no progressive analyzer-enforcement policy

## Standard Layers

Zongzu uses five standard layers.

### 1. Code Standards

Code standards own:
- C# style
- naming
- file shape
- type cohesion
- nullable safety
- analyzer and build gates
- testability

Current enforcement:
- `.editorconfig` owns shared editor formatting and C# style preferences
- `Directory.Build.props` enables nullable reference types
- `Directory.Build.props` disables implicit usings
- `Directory.Build.props` enables .NET analyzers and build-time code-style analysis
- warnings-as-errors are enabled for kernel, contracts, scheduler, persistence, and simulation modules

Rules:
- keep formatting, naming, and namespace style automated
- use file-scoped namespaces for new C# files
- use explicit access modifiers
- prefer `ArgumentNullException.ThrowIfNull` at boundaries
- prefer typed IDs, enums, records, and value objects over raw strings
- keep side effects visible
- keep parsing, rule resolution, projection formatting, persistence, and diagnostics separated
- do not introduce hidden clocks, hidden IO, hidden global caches, or nondeterministic collection order in authority code

Zongzu addition:
- code style is secondary to authority safety
- a tidy implementation that violates determinism, ownership, schema versioning, or projection boundaries is still wrong

### 2. Module Standards

Module standards own:
- state namespace
- public query surface
- command surface
- emitted events
- cadence
- schema version
- tests

Every module-level change must answer:
- what state does this module own
- what state it explicitly does not own
- which queries expose safe read-only information
- which commands enter as intents
- which events leave as structured facts
- which cadence runs it: `xun`, `month`, `seasonal`, or combined
- what schema version and migration effect changed

Rules:
- no module may mutate another module's authoritative state
- no historical-process, court, warfare, map, or content pack may bypass ownership
- feature packs are additive, not backdoors
- application may route commands but must not become a second domain-rule layer

### 3. System Standards

Every gameplay system must be describable as:

```text
inputs -> owned state -> cadence -> rules -> diffs/events -> projections -> player leverage -> tests
```

Minimum system proposal block:

```text
Owner:
Cadence:
Inputs:
Owned state:
Commands:
Queries:
Events:
Diffs:
Projection:
Player leverage:
Tests:
Save impact:
Determinism risk:
```

Rules:
- the owner module is named
- input sources are named
- deterministic cadence is named
- random source, if any, uses project RNG
- state transition stores facts or pressures, not final prose
- output includes structured diff, event, query, or projection
- player action is an intent, not a guaranteed outcome
- major outcomes expose top causes
- tests cover invariants and at least one live pressure chain

### 4. Unity Presentation Standards

Unity presentation owns:
- scenes
- prefabs
- MonoBehaviours
- ScriptableObjects for presentation data or static settings
- assets
- animations
- shell object binding
- visual state

Unity presentation may:
- read application read models and projections
- animate state already produced by authority
- cache component references
- use prefabs and nested prefabs to reduce scene churn
- split scenes additively when the Unity project becomes collaborative or too large
- use ScriptableObjects for shared display settings, art/audio references, or non-authoritative presentation configuration

Unity presentation may not:
- resolve authoritative game rules
- mutate module state directly
- invent hidden player knowledge
- become the only place where a gameplay concept exists
- hide simulation cadence inside `Update`
- use Unity random or wall-clock time for authority

Rules:
- document folder and naming conventions
- avoid spaces in Unity asset and folder names
- keep internal assets separate from third-party assets
- keep sandbox/prototype scenes separate from production scenes
- cache references instead of repeatedly searching hot paths
- avoid noisy release logging and stack traces unless intentionally configured
- use object pooling for frequently created/destroyed objects

Zongzu addition:
- Unity is the shell, not the simulation
- hall, desk, map, notices, vignettes, and boards consume read models
- the shell can be rich, but authority remains pure C#

### 5. Content And Rules-Data Standards

Content and rules-data standards own:
- authored JSON
- descriptors
- labels
- historical calibration notes
- narrative text
- rule parameters
- generated content

Rules-data may:
- tune thresholds
- provide labels
- describe historical or regional variants
- seed scenarios
- feed projection language

Rules-data may not:
- become an event-pool authority layer
- override module ownership
- contain hidden future information in player-facing outputs
- embed local machine paths, secrets, or debug stack traces

Zongzu addition:
- history is pressure, not rails
- local society, court rhythm, reform pressure, war, disaster, and dynasty-cycle movement must become rule chains before they become text

## File Size And Splitting Standard

Ordinary target:
- source files should usually stay under about `400` logical lines

Default split threshold:
- non-generated source files past about `600` logical lines should be split unless there is a documented reason

Split by:
- domain rule resolution
- command routing
- persistence or migration
- projection wording
- diagnostics
- fixture setup

Generated, table-like, or intentionally consolidated files may be larger, but the reason should be obvious.

## Simulation Quality Standard

A system is not live just because data exists.

It must show at least one pressure chain:

```text
world state -> pressure change -> authority outcome -> structured diff/event -> projection -> bounded player response
```

Examples:
- illness -> care burden -> household strain -> notice -> relief/support command
- route damage -> market price shift -> household debt -> desk-map marker -> repair/escort/fund response
- death -> mourning/inheritance pressure -> ancestral-hall guidance -> bounded family command
- frontier news -> public rumor/court pressure -> legitimacy drift -> hall docket -> office/force response

## Debug And Observability Standard

Debug outputs should be:
- deterministic
- seed-aware
- module-attributed
- projection-safe
- separable from player-facing text

Required for deep systems:
- invariant tests
- deterministic replay tests
- save roundtrip tests
- at least one integration test across module boundaries
- readable trace for major outcomes

Never leak into normal player-facing surfaces:
- stack traces
- local absolute paths
- hidden future information
- raw internal exception dumps
- debug-only module internals

## Performance Standard

Authority hot paths include:
- `xun`
- `month`
- `seasonal`
- event handling
- diff generation
- save migration when invoked

Authority hot paths must avoid:
- blocking network IO
- blocking filesystem IO
- wall-clock dependency
- arbitrary sleeps or polling
- repeated expensive reflection
- nondeterministic iteration order

Unity presentation hot paths should avoid:
- repeated scene searches in `Update`
- frequent `Instantiate` / `Destroy` churn for common objects
- high-volume logging in `Update`, `LateUpdate`, or `FixedUpdate`
- string parameter lookups where cached IDs are available
- long frame spikes from doing all work in one frame when time slicing is possible

## Analyzer Enforcement Policy

Current policy:
- .NET analyzers are enabled
- build-time code-style analysis is enabled
- core authority libraries treat warnings as errors
- `.editorconfig` sets selected code-style rules to warning
- broad .NET code-quality categories are not yet globally promoted to warning/error because the current codebase still contains intentional test naming, calibration constants, and culture/performance warnings that need a planned cleanup batch

Progression rule:
- do not add a broad analyzer category as `error` until current violations are measured
- promote rules in small batches
- every promotion must either pass cleanly or include targeted fixes/suppressions

## External Calibration

This repo uses external standards as calibration, not as orders to rewrite the architecture.

Useful reference families:
- Microsoft C# conventions and .NET analyzers for code style and enforcement
- Unity project organization guidance for assets, prefabs, scenes, naming, and collaboration
- Unity code architecture guidance for hot-path behavior, caching, pooling, logging, and ScriptableObject usage
- Unity modular architecture guidance for ScriptableObject data and event-channel ideas, with the caveat that Zongzu authority remains pure C#

## Acceptance Gate

A non-trivial feature is ready only when:
- code compiles
- tests pass
- ownership is documented
- schema impact is documented
- deterministic behavior remains valid
- projections remain downstream
- Unity/presentation does not own authority
- player leverage stays bounded
- the change follows this document or explicitly documents why it departs

## One-Line Rule

Modern game engineering practice is welcome in Zongzu only when it strengthens deterministic modules, clear contracts, testable systems, and a spatialized shell without moving authority out of the pure C# simulation.
