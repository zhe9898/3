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

## Current Repo Anchors

Treat live repo facts as stronger than older plans. As of the current Zongzu line:
- the Unity host project exists at `unity/Zongzu.UnityShell`, but authority still lives in the .NET modular monolith under `src/`
- `Zongzu.Kernel` and simulation modules must not reference Unity APIs; `Zongzu.Presentation.Unity` and ViewModels are projection/adaptation surfaces only
- `MonthlyScheduler` runs prepare, three xun pulses, month pass, bounded fresh-event drain, then projection; same-month chains must prove they pass through that drain
- `ModuleRunner<TState>` metadata (`AcceptedCommands`, `PublishedEvents`, `ConsumedEvents`, cadence, phase, schema version) is part of the contract surface and must be checked against real handlers/tests
- read-only helpers on snapshots are allowed when they normalize traversal of existing projection payloads, such as settlement/module notification matching; they must not become ranking, visibility, scheduler, command, or authority policy
- current court-policy branch is implemented through v196: v101-v108 audited the thin skeleton; v109-v116 adds first-layer court-policy process thickening; v117-v124 adds bounded Office-owned local response affordance; v125-v132 adds delayed SocialMemory court-policy residue; v133-v140 projects old policy-response residue into the next policy window; v141-v148 projects that residue as public-reading echo; v149-v196 stack public follow-up cue, docket guard, suggested-action guard, suggested-receipt guard, receipt-docket consistency guard, and public-life receipt echo guard on existing governance/public-life command surfaces
- `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` each keep their own command/state/residue authority while `PlayerCommandService` and `PlayerCommandCatalog` route and describe commands
- current save anchors include `TradeAndIndustry` schema `4`, `WorldSettlements` schema `8`, `OrderAndBanditry` schema `9`, `OfficeAndCareer` schema `7`, `FamilyCore` schema `8`, `PopulationAndHouseholds` schema `3`, and `SocialMemoryAndRelations` schema `3`
- player-command `LeverageSummary` / `CostSummary` / `ReadbackSummary`, `HouseholdSocialPressure`, `PresentationReadModelBundle.SocialMemories`, and governance/home-household receipt readback are projection/read-model facts only
- current v19-v196 projection/readback includes follow-up hints, owner-lane return guidance, Office/Family/Force/Warfare/Court readback fields, directive and aftermath docket readbacks, court-policy process/local-response/SocialMemory/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/public-life-receipt echo guard readbacks, and thin-chain closeout audit wording over existing structured snapshots
- `OwnerLaneReturnSurface` is a projection helper, not an owner-lane ledger, cooldown ledger, command resolver, or summary parser; code, docs, architecture tests, integration tests, and Unity/presentation tests are part of the current branch evidence
- event-contract health diagnostics classify emitted-but-unconsumed and declared-but-not-emitted event debt; they are test/diagnostic evidence only, not scheduler input, runtime authority, event-pool design, or a second rule layer
- v35-v196 use structured metadata/query snapshots and existing owner state or projection-only readbacks only: canal-window Trade/Order, household-family sponsor pressure, Family relief, Office policy implementation/readback, Office/Family lane closure, Force/Campaign owner-lane readback, Warfare directive/aftermath readback, Court-policy process/local-response/SocialMemory/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/public-life-receipt echo guard readback, and thin-chain closeout add no schema, migration, ledger, household target field, `PersonRegistry` expansion, or UI/Unity authority unless a future rule-density pass explicitly plans it
- external calibration sources such as Microsoft Learn .NET testing/performance/diagnostics/logging guidance, Unity Profiler/object-pooling/UI optimization guidance, Unity assembly/asset metadata docs, WCAG 2.2, and Xbox Accessibility Guidelines calibrate testability, hot-path discipline, Unity organization, and shell readability; they do not override Zongzu module ownership

## External Calibration Anchors

Use current first-party guidance as calibration, not as authority over the repo:
- Microsoft Learn .NET unit testing guidance supports fast, isolated, repeatable, self-checking tests; translate this into focused ownership/no-touch/scheduler tests rather than broad snapshots.
- Microsoft Learn diagnostics and `dotnet-counters` support first-level CPU, GC, allocation, memory, and exception-rate investigation when preview or long-run behavior changes.
- Microsoft Learn high-performance logging supports source-generated or delegate-cached logging for hot diagnostic paths; logs remain diagnostics and never become player receipts or rule input.
- Microsoft Learn collection guidance supports choosing data structures by access pattern, memory tradeoff, ordering needs, and concurrency boundary; it does not justify unordered authority traversal.
- Unity assembly definitions, `.meta` files, Profiler, GC, object pooling, and UI optimization guidance apply only to `unity/` and `Zongzu.Presentation.Unity`; they never move authority into MonoBehaviours.
- WCAG 2.2 and Xbox Accessibility Guidelines calibrate contrast, focus/read order, narration, and semantic labels for shell surfaces; they do not flatten the shell into a web dashboard.

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

## Fast Lane

Use a fast boundary check for narrow edits: owner module, dependency direction, save/schema impact, deterministic ordering, UI/Application authority drift, and smallest test lane. Use a full architecture pass when a change crosses modules, changes scheduler cadence, adds state, changes persistence, alters command resolution, expands projection selection, or introduces caches/indexes/pooling/performance claims.

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
   - `PlayerCommandCatalog` / command resolver alignment when player commands are involved
   - `PresentationReadModelBuilder` and `Zongzu.Presentation.Unity` adapters when projection selection is involved
   - relevant tests under `tests/`

   For Renzong pressure-chain work, use `zongzu-pressure-chain` alongside this skill. For save/schema changes, use `zongzu-save-and-schema`; for deterministic proof or long-run health, use `zongzu-simulation-validation`; for Unity implementation details, use `zongzu-unity-shell`.

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
- identify the hot path, expected cardinality, allocation risk, cache invalidation point, and whether performance evidence is needed

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

## Performance / Algorithm / Scheduler Budget

Treat performance as an architecture boundary, not a late polish pass:
- name the hot path before optimizing: `day`, `month`, xun projection band, scheduler event drain, command handling, projection building, save migration/load, preview diagnostics, or Unity frame/update
- state expected cardinality in the change: settlements, clans, households, people, routes, events, notices, read-model rows, or Unity shell objects touched
- measure or cite current tests/diagnostics before introducing broad caches, pools, `Span`/`Memory`, ref rewrites, generated logging, or custom data structures
- prefer one-pass indexes and snapshot helpers over repeated global scans, but keep them read-only and rebuilt at deterministic projection/query boundaries
- scheduler drains need caps, watermarks, or explicit cadence; pressure chains need source -> locus -> fanout -> no-touch proof
- diagnostics and logging on hot authority paths should be module-attributed, low allocation, and guarded; source-generated logging is preferred when real logging is added
- performance caches must name owner, invalidation, determinism impact, and save/schema impact; no global mutable cache may become hidden authority
- Unity hot-path advice applies only to the shell: cache component references, profile before pooling, pool repeated notice/marker/UI objects, and avoid rule work in `Update`
- migration/load paths count as hot paths when old saves open; schema growth should consider payload size and deterministic default/backfill cost
- algorithmic changes must state complexity and cardinality before landing: settlements, clans, households, people, routes, events, notices, or Unity objects touched per scheduler pass, projection build, save load, or frame
- one-pass indexes over already-built snapshots are acceptable at query/projection boundaries; repeated LINQ/global scans in scheduler hot paths or Unity frame loops need evidence and a bounded replacement
- deterministic ordering is part of architecture. Any sort, tie-break, cap, queue, or fanout rule must be stable across runtimes and seeds.

## Zongzu-Specific Guidance

- `Zongzu.Contracts` is the cross-module language, not a dumping ground for internals.
- `Zongzu.Kernel` owns deterministic primitives and identity anchors, not domain formulas.
- `PersonRegistry` is identity-only; if a field says what a person does, feels, owns, remembers, or can leverage, it belongs in a domain module or projection.
- `Zongzu.Scheduler` orders the world and drains events; it should not become the domain brain.
- Simulation modules own state and rules. They may query other modules, react to events, and emit facts, but they do not directly rewrite another module.
- The current command seam already exists through module-owned `HandleCommand(...)`; new command logic should prefer that path.
- For public-life/order/court-policy work, preserve the current v196 split: each owning module resolves its own command/traces, SocialMemory owns durable social residue on its later cadence, PopulationAndHouseholds owns home-household local response state, Office/PublicLife own court-policy local execution/public reading, and Application/Unity copy projections only. v19-v196 follow-up, owner-lane return/status/outcome/residue/no-loop, Office/Family/Force/Warfare/Court readbacks, SocialMemory echo, memory-pressure readback, public-reading echo, public follow-up cue, docket guard, suggested-action guard, suggested-receipt guard, receipt-docket consistency guard, and public-life receipt echo guard fields do not become cooldown ledgers, owner-lane ledgers, follow-up/docket/suggested-action/receipt/receipt-docket/public-life-receipt ledgers, command resolvers, ranking rules, or rule layers.
- Application code may route, seed, and assemble, but it should stay thin and avoid becoming a second rules engine.
- When application or presentation code starts repeating the same visible-selection logic, prefer a projection-local helper or read-only snapshot accessor over copying `LeadItem` / `SecondaryItems` or affordance-scan logic into every caller.
- When application or presentation code repeats raw notification trace scans, prefer a narrow contracts/read-model helper such as settlement/module scope matching; keep caller-side ordering and UI choice outside the helper.
- `NarrativeProjection` and Unity shell code are projection surfaces only; they do not author consequences.
- Feature packs are additive and manifest-gated. Data/content mods and runtime code plugins are different promises with different trust boundaries.
- Strong Zongzu architecture is not just "many files." It is clear ownership, explicit seams, deterministic cadence, readable projections, and startup/load discipline that still preserves the world-first simulation.
