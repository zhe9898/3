---
name: zongzu-architecture-boundaries
description: Use when working on Zongzu's modular-monolith boundaries, code structure, command/query/event seams, scheduler flow, save/version contracts, deterministic simulation, componentization, startup/load budgets, performance hot paths, feature-pack or mod extensibility, or when a change risks becoming a giant world manager, second domain layer, UI-owned rule path, stringly event glue, reflection-heavy startup, unsafe runtime plugin loading, or cross-module state mutation.
---

# Zongzu Architecture Boundaries

## Overview

Use this skill to keep implementation aligned with the live Zongzu architecture:
- modular monolith, not one giant manager
- module-owned authoritative state
- cross-module coordination through Query / Command / DomainEvent
- deterministic cadence, bounded scheduler drain, and save-compatible evolution
- module-owned command resolution through the current `HandleCommand(...)` seam
- projections, notices, adapters, and Unity shell downstream of authority
- explicit pressure locus, propagation, and off-scope boundaries for cross-module chains
- startup, load, and extensibility treated as first-class architecture constraints

Use it when design becomes code, when code structure is getting muddy, or when a pressure chain crosses modules and needs contracts, tests, schema, and docs to stay lined up.

## Use This Skill When

- adding or reviewing a module, state field, query, command, domain event, scheduler step, or pressure chain
- deciding where code belongs across `Zongzu.Contracts`, `Zongzu.Kernel`, `Zongzu.Scheduler`, modules, application orchestration, projections, tests, or Unity-facing shell code
- splitting large files or moving formulas / lifecycle / queries / handlers without changing ownership
- checking whether a module is leaking state, mutating another module, or growing into a second world manager
- reviewing command resolution, especially whether rules live in module-owned handlers or drift upward into application orchestration
- reviewing broad-to-local pressure such as frontier, court, disaster, campaign, reform, route, or regime pressure that may fan out into settlements, offices, households, clans, or routes
- adding event metadata, event constants, `PublishedEvents`, `ConsumedEvents`, schema versions, migrations, or feature-manifest updates
- reviewing Unity componentization, adapters, read models, startup/load paths, pooling, Addressables, or presentation-only boundaries
- reviewing performance, diagnostics, replay, observability, save compatibility, content/rules-data validation, or extension/mod/plugin proposals

## Workflow

1. Read the boundary docs first.

   Start with:
   - `docs/ENGINEERING_RULES.md`
   - `docs/ARCHITECTURE.md`
   - `docs/MODULE_BOUNDARIES.md`
   - `docs/MODULE_INTEGRATION_RULES.md`
   - `docs/EXTENSIBILITY_MODEL.md`
   - `docs/TECH_STACK.md`
   - `docs/STATIC_BACKEND_FIRST.md`
   - `docs/SCHEMA_NAMESPACE_RULES.md`
   - `docs/DATA_SCHEMA.md`
   - `docs/SIMULATION.md`
   - `docs/ACCEPTANCE_TESTS.md`
   - `docs/MODERN_GAME_ENGINEERING_STANDARDS.md`

   For larger work also read:
   - `PLANS.md`
   - `docs/exec-plans/README.md`
   - the active exec plan for the touched chain or module

2. Check current implementation facts before calling something a pattern.

   Inspect the live code when the task touches implementation shape:
   - `ModuleRunner<TState>` and `IModuleRunner`
   - module-owned `HandleCommand(...)` overrides and helper/resolver files
   - `GameSimulation` orchestration boundaries
   - `MonthlyScheduler` ordering and bounded event drain
   - `SimulationBootstrapper` seeding and feature-pack module sets
   - relevant tests under `tests/`

   Treat current facts as authoritative over stale comments or old plans.

3. Classify the touched layer.

   Determine whether the change is mainly:
   - contract
   - kernel primitive
   - scheduler
   - module authority
   - projection / read model
   - application orchestration
   - Unity shell / adapter
   - save / migration
   - diagnostics / observability
   - content / rules-data validation
   - startup / load
   - extension / mod surface

4. Reduce the problem to a stable structure.

   A good result answers:
   - which module owns the state
   - which command expresses intent
   - which rule path resolves it
   - which event records a fact that already happened
   - whether the effect is same-month, bounded-drain, or next-month echo
   - what the pressure locus and off-scope boundary are
   - what test proves both the positive case and the no-touch case
   - what docs, schemas, manifests, or acceptance criteria must change
   - what load/performance/component cost this adds

## Short Prompt Expansion

Treat short architecture prompts as whole-boundary prompts unless the user explicitly asks for a narrow definition.

For prompts like `模块边界`, `架构`, `解耦`, `command seam`, `scheduler`, `event contract`, `save schema`, `startup`, `load`, `mod`, `plugin`, `componentization`, or `performance`, default to:
- identify the owning module and state
- identify the contract seam
- identify whether the current code already has a supported path
- identify where application code should stop
- identify pressure locus and propagation boundary when events cross modules
- identify determinism, save, startup, and test impact

## Output Rules

- Do not introduce a giant world manager.
- Do not add a second domain-rule layer in application orchestration.
- Do not use UI, adapters, or MonoBehaviours as authority owners.
- Do not mutate foreign module state directly.
- Do not parse `DomainEvent.Summary` as rule input.
- Do not let broad pressure become local consequence without an explicit allocation rule.
- Do not let a stable high band pretend to be a fresh escalation every month; use edge detection, watermark state, or a recurring-demand model.
- Do not treat `GetMutableModuleState(...)` as a normal command-resolution path. It is bootstrap / seeding / narrow test infrastructure, not the preferred home for gameplay rules.
- Do not add runtime plugin loading, reflection-heavy discovery, or ad hoc foreign save writes as the default extensibility model.
- Do not claim a chain is end-to-end complete unless the real scheduler path, event contracts, off-scope entities, and projection/receipt behavior are covered or explicitly deferred.
- Do allow small read-only projection conveniences such as snapshot accessors, lane/item lookup helpers, and one-pass indexes/caches when they only normalize access to already-built payloads and do not create a second composition or policy layer.

## Zongzu-Specific Guidance

- `Zongzu.Contracts` is the cross-module language, not a dumping ground for internals.
- `Zongzu.Kernel` owns deterministic primitives and identity anchors, not domain formulas.
- `PersonRegistry` is identity-only; if a field says what a person does, feels, owns, remembers, or can leverage, it belongs in a domain module or projection.
- `Zongzu.Scheduler` orders the world and drains events; it should not become the domain brain.
- Simulation modules own state and rules. They may query other modules, react to events, and emit facts, but they do not directly rewrite another module.
- The current command seam already exists through module-owned `HandleCommand(...)`; new command logic should prefer that path.
- Application code may route, seed, and assemble, but it should stay thin and avoid becoming a second rules engine.
- When application or presentation code starts repeating the same visible-selection logic, prefer a projection-local helper or read-only snapshot accessor over copying `LeadItem` / `SecondaryItems` or affordance-scan logic into every caller.
- `NarrativeProjection` and Unity shell code are projection surfaces only; they do not author consequences.
- Feature packs are additive and manifest-gated. Data/content mods and runtime code plugins are different promises with different trust boundaries.
- Strong Zongzu architecture is not just "many files." It is clear ownership, explicit seams, deterministic cadence, readable projections, and startup/load discipline that still preserves the world-first simulation.
