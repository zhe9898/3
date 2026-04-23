# FULL_SYSTEM_SPEC

## Purpose

This document is the one-page whole-system synthesis for Zongzu.
It is meant to help Codex, future implementers, and design review stay aligned without having to rediscover the product from many scattered files each time.

It does not replace the more detailed specifications.
It compresses them into one connected pass.

Primary supporting documents remain:
- `PRODUCT_SCOPE.md`
- `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
- `RULES_DRIVEN_LIVING_WORLD.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `SIMULATION.md`
- `MODULE_CADENCE_MATRIX.md`
- `ENGINEERING_RULES.md`
- `STATIC_BACKEND_FIRST.md`
- `ARCHITECTURE.md`
- `VISUAL_FORM_AND_INTERACTION.md`
- `UI_AND_PRESENTATION.md`

## One-paragraph project statement

Zongzu is a Northern Song-inspired, multi-route, rules-driven simulation of a living Chinese ancient society.
The player is not a god and not always a secure elite manager.
The player enters the world through a household-side or lineage-side position inside a society that keeps moving on its own.
Households, lineages, commoners, markets, offices, local order, conflict, and wider imperial pressure all remain part of one linked field.
Long-run play may let that field bend history itself through rebellion, polity formation, succession struggle, usurpation, restoration, or dynasty repair, but only through earned rule chains rather than free timeline editing.
The backend is a modular monolith with one scheduler, one save root, one world-state container, and multiple authoritative modules with owned state and explicit contracts.
The shell must feel like a playable hall-and-desk game, not a dashboard, poster, or platform tool.

## Six top-level principles

### 1. The world self-runs; it is not event-pool driven

The world changes because state changes, pressure travels, and institutions or actors resolve that pressure.
Events, letters, rumors, memorials, and notices are readability layers built after authoritative change.

Correct order:

`state pressure -> resolution -> diff -> projection -> player review`

Not:

`random event draw -> world changes because the event says so`

### 2. The player intervenes in bounded ways; this is not god control

The player has leverage, not omnipotence.
Reach is strongest inside the household and lineage, weaker through marriage, obligation, prestige, office access, money, grain, and local intermediaries, and weakest at wider political scales.

Commands are intents that must resolve against:
- autonomy
- existing pressure
- institutions
- distance
- timing
- resources
- risk

### 3. The society is alive across multiple routes, not a fixed class tree

Zongzu is not a single elite-lineage game with decorative commoners.
It is a social field in which actors can drift, rise, fragment, endure, or fall.

Important route families include:
- household survival
- lineage management
- commerce and workshop life
- education and office access
- social governance
- disorder, shadow survival, and coercive routes
- later military and macro-governance pressure
- later rebellion / polity formation / dynasty-cycle pressure

Downward mobility is mandatory.
A world where everyone important stays above the floor is not alive.

History-change is also in scope.
The player may eventually alter regional or throne-facing history when household, lineage, office, force, logistics, legitimacy, public belief, and memory have created enough reach.
This is not god control; it is bounded leverage at larger scale.

### 4. The backend must stay modular and low-coupling

This is a modular monolith, not a featureless large blob.

Macro shape:
- one process
- one save root
- one scheduler
- one world-state container
- multiple authoritative modules

Rules:
- each module owns its own authoritative state
- other modules read through queries and react through domain events
- application services route intent but must not become a second rule engine
- shell and projection remain downstream of authority
- code structure must stay high-cohesion and low-coupling
- boundaries must stay explicit enough that ownership is visible in code review, not inferred from tribal memory

### 5. The system must remain pluggable and extensible

Zongzu must be able to grow by adding or deepening packs without breaking the spine.

This means:
- major capabilities should be attachable as feature packs or depth bands such as `absent / lite / full`
- a new pack should register modules, commands, queries, events, schema versions, migrations, tests, and projections without bypassing the scheduler or persistence rules
- deeper versions of a system should enrich the same ownership model rather than inventing a parallel hidden framework
- save roots must preserve which packs and depth bands are enabled
- route depth, warfare depth, office depth, order depth, and public-life depth should all be able to grow additively

Pluggability here does **not** mean arbitrary runtime plugin loading.
It means disciplined additive growth inside one modular monolith.

Good pluggability means:
- a pack can be absent without corrupting the base world
- a lite pack can coexist with deeper packs in other domains
- a full pack can replace lite behavior through owned state and contracts rather than through hacks
- the shell can degrade gracefully when a pack is absent

### 6. The bottom must stay rigorous, but the surface must still feel like a game

The backend needs:
- determinism
- schema versioning
- migration discipline
- observability
- bounded runtime growth
- bounded save and notification growth
- explainable diffs
- stable cadence contracts
- leakage discipline
- IO discipline
- dependency hygiene

The player-facing shell still needs to feel like:
- a great hall
- an ancestral hall
- a desk sandbox
- a notice tray
- a conflict vignette
- a later campaign-lite board

Never let rigor produce a platform-looking shell.
Never let flavor break authority.

## Performance and scale requirements

Performance is a product requirement, not a late optimization pass.

The system must remain playable and inspectable under long deterministic runs.

That means:
- `xun` fidelity may deepen local life, but must not explode whole-world authority cost
- notification growth must stay bounded during long headless runs
- save payload growth must stay bounded and diagnosable
- runtime-only diagnostics may expose scale summaries, top payload modules, pressure counts, and hotspot scores without entering save compatibility
- multi-year and multi-seed stress sweeps must remain part of the acceptance bar, not an optional afterthought

The intended posture is:
- deeper local simulation
- bounded global cost
- readable runtime hotspots
- reproducible long-run diagnostics

If a new rule makes the world more vivid but silently destroys long-run scale behavior, it is not yet acceptable.

## Code architecture and quality requirements

The codebase must remain:
- structurally clear
- high-cohesion and low-coupling
- explicit at boundaries
- safe against hidden leakage
- safe against accidental IO stalls
- hostile to weak glue code

That means:
- application orchestration stays thin and does not become a second domain brain
- authority modules keep ownership visible and local
- source files and types stay maintainable in size; when one file starts carrying too many responsibilities, it must be split along ownership, cadence, or workflow seams
- player-facing shell does not read raw authority internals
- debug-only traces do not leak into normal play surfaces
- no ad hoc blocking filesystem or network IO inside authority hot paths
- no stringly-typed payload stitching or brittle concatenated cross-layer protocols where typed contracts can exist
- no new deprecated or obsolete libraries in fresh feature work without an explicit migration note
- no vague catch-all managers, utility sinks, or hidden singleton state in authority layers
- giant source files are treated as a structural smell, not as harmless style debt

In practice, Codex should read that as:
- around `400` logical lines is the point where file responsibilities should be questioned
- around `600` logical lines in a non-generated source file is the point where splitting becomes the default expectation
- authority hot paths must not perform blocking filesystem or network IO
- application code must not directly mutate foreign module state
- player-facing projections must not leak secrets, local machine paths, stack traces, hidden future information, or debug-only traces

Good code here should feel:
- strict at the bottom
- explainable in the middle
- playable at the top

## Product fantasy

The fantasy is not "I click and the world obeys."
The fantasy is:
- the world moves before I do
- I can read what changed and why
- I can intervene only where I still have reach
- my choices echo later through marriage, debt, education, resentment, inheritance, office, and conflict
- one household or lineage position becomes a window into a living society
- people can fall out of sight without falling out of history

## Historical grounding

The game is historically grounded, not a documentary simulator.

The baseline is:
- Northern Song inspired
- premodern Chinese social mechanisms
- explicit anti-anachronism discipline
- region-sensitive ecology and culture
- household, lineage, commoner, office, market, and imperial-local pressure all visible

History should become:
- pressure
- institutions
- route logic
- map nodes
- notice tone
- social surfaces
- durable memory and delayed return paths

History should not become:
- inert lore
- generic East Asian flavor
- one giant dynastic costume

## Macro software architecture

The macro software architecture is:

**a modular monolith with a central scheduler**

Top-level layers:

### 1. Kernel
- IDs
- time values
- RNG contracts
- common result and error types
- base event and command abstractions

### 2. Contracts
- module registration interfaces
- command DTOs
- query contracts
- domain-event contracts
- projection contracts
- save and state contracts

### 3. Scheduler
- module order
- cadence inspection
- `xun / month / seasonal` timing
- deterministic event queue handling
- diff aggregation boundaries

### 4. Authoritative modules
- domain-owned state
- owned commands
- domain handlers
- domain projections
- domain tests

### 5. Application
- orchestration
- use cases
- player command routing
- save/load coordination
- debug entry points
- module-pack enablement

### 6. Persistence
- save root
- module state serialization
- schema version checks
- migration pipeline
- replay snapshots

### 7. Presentation
- Unity scenes
- object-anchored shell
- hall, desk, and notice surfaces
- inspectors
- non-authoritative animation and vignette behavior

## Core authority rule

A feature is only acceptable if it can be added through:
- owned state in one module
- public queries
- commands
- domain events
- tests
- schema/version entries

without arbitrary direct writes into other module internals.

## Backend implementation rule

Build the backend as:

**stable structure first, rule density later**

That means:
- stabilize module boundaries first
- stabilize state shape first
- stabilize cadence first
- stabilize command/query/event contracts first
- stabilize save schema first
- then deepen formulas

This does **not** mean building dead DTO piles.
It means the first milestone should already be:
- schedulable
- saveable
- queryable
- projectable
- capable of proving at least one live pressure chain

That stable structure should also remain pluggable:
- new packs must enter through module registration and feature manifests
- deeper rule bands must replace or enrich owned logic without creating cross-module hacks
- projections and shell surfaces must survive `absent / lite / full` feature depth without changing authority ownership

## Time model

The player-facing shell is monthly.
The living world inside the month may pulse in three inner bands:
- upper xun
- middle xun
- lower xun

Cadence is therefore:
- `xun` for close life and local pressure
- `month` for review, consolidation, projection, and most bounded intervention
- `seasonal` for slower structures such as harvest, exam heat, and broad policy or war climate

No subsystem may invent private hidden clocks outside the scheduler.

## Simulation shape

The society stays alive through ring-based fidelity:

### Core ring
- player household
- close kin
- enemies
- affines
- current hotspot actors

These can run as full 1:1 agents when needed.

### Local ring
- same-county important households
- institutions
- route-linked nodes
- named supporting actors where useful
- summary pools for the rest

### Wider ring
- regional and imperial pressure
- route climate
- tax heat
- market temperature
- conflict posture
- opportunity climate

These do not need full person-by-person simulation.
They must still stay alive as pressure, opportunity, and message.

## Fidelity rule

Do not decide simulation detail only by social identity.

Decide it through:
- distance from the player
- whether pressure currently hits
- whether the player can meaningfully see or intervene
- settlement size and prosperity

Prosperity mainly determines pool thickness:
- population pool
- labor pool
- marriage pool
- education opportunity
- market chance
- migration pressure
- talent emergence probability

It does **not** by itself determine full-agent precision.

## What "self-running world" means in practice

If the player does nothing, the world still produces plausible history:
- households stabilize, fragment, migrate, or collapse
- lineages rise, weaken, split, or lose prestige
- markets heat and cool
- offices open and close access
- marriages produce alliance and burden
- resentment cools or reactivates
- conflict and coercion emerge from pressure rather than event tables

The player acts late in the chain, after the world has already moved.

## Player scope

The player is a household-side or lineage-side decision-maker.
The player chooses an entry position, not a custom universe.

The player may choose:
- era pack
- region
- initial standing
- initial resources and reputation
- house-style bias
- scenario-specific starting stratum when enabled

The player may not directly rewrite:
- macro institutions
- climate law
- the existence of ambition in others
- the basic autonomy of adults
- the core logic of war outbreak or dynastic strain

## Command model

Player interaction should come through bounded verbs such as:
- order
- persuade
- reward
- punish
- subsidize
- cut support
- reassign property
- delay or accelerate marriage
- ask intermediaries to intervene
- borrow outside leverage
- absorb insult and keep score
- attempt mediation or private settlement

Commands should yield:
- receipts
- delays
- partial effects
- resistance
- side costs
- future consequences

They should not behave like guaranteed buttons.

## Core social layers

The living field includes:
- individuals
- households
- lineages
- settlements
- institutions
- region and world climate

Recurring structural pressures include:
- food and wage pressure
- debt and property strain
- marriage and fertility pressure
- illness, aging, and death
- education and office bottlenecks
- local order and coercion
- public reputation
- official and lineage entanglement
- imperial-local bargaining
- route and market constraint

## Conflict and force

Conflict must stay on the same structural spine as household, office, and trade pressure.

Do not collapse every conflict into one warfare layer.
Distinguish:
- ambient pressure
- local conflict vignette
- tactical-lite encounter when justified
- campaign-lite board when scale and consequence justify escalation

Private force, retainers, militia, yamen force, rebel bands, garrisons, and larger warfare all belong to one world, but not one presentation layer.

## Map and sandbox direction

The map system is sandboxed at multiple scales.

### Macro sandbox
- route
- prefectural and county relations
- water and land corridors
- imperial and regional pressure
- strategic bottlenecks

### County sandbox
- county seat
- market town
- ferry
- estate
- school
- temple
- yamen
- workshop
- storage
- pressure hotspots

Both are sandboxes.
The macro view is not a detached flat strategy map.
The county view is not a minimap.
They are different densities of the same desk-readable world.

## UI and shell rule

The shell must feel like a playable room-and-desk environment.

Priority surfaces:
- great hall
- ancestral hall
- desk sandbox
- notice tray
- conflict vignette
- later campaign-lite board

The shell must not feel like:
- a marketing poster
- a SaaS dashboard
- a document screenshot
- a spreadsheet with ornament

Use object anchors, trays, ledgers, markers, seals, notices, routes, and room logic.
Keep text downstream of structure.

## Observability and explanation

Important outcomes must be readable.
The player should be able to answer:
- what changed
- why it changed
- who carried the pressure
- what can still be done

Major changes should expose short cause chains, for example:

`study failure <- cash strain <- grain spike <- drought`

This is what turns hidden simulation into believable living-world play.

## Save, schema, and compatibility

The simulation must stay:
- deterministic
- versioned
- migratable
- replayable enough to debug

Requirements:
- root save version
- per-module schema version
- feature manifest persistence
- migration path for incompatible changes
- non-recycled durable IDs

Feature-manifest persistence is part of pluggability, not an optional convenience.
The save must know which packs and depth bands are active so that migration, shell degradation, and replay logic remain coherent.

## Anti-patterns

Do not build:
- event-pool authority
- god-control commands
- detached subsystem games
- UI-owned rules
- hidden clocks
- direct foreign-state writes
- application-layer domain creep
- authority state full of shell wording and labels
- tactical combat that replaces the living society

## What Codex should assume by default

When working on this project, assume:
- the world runs before the player
- the player acts through bounded leverage
- routes are open and pressure-driven, not fixed classes
- the backend is a modular monolith
- modules own their own state
- cadence is `xun / month / seasonal`
- structure and contracts should stabilize before deep formulas
- projection owns wording
- the shell must feel like a game, not a tool

## One-line summary

Zongzu is a rules-driven, historically grounded, living-society simulation built as a modular monolith with one scheduler, one save root, and multiple authoritative modules, where the world moves first, the player intervenes late and imperfectly, and the shell must make that rigor feel playable rather than technical.
