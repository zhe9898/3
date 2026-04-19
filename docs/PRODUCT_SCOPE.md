# PRODUCT_SCOPE

This document defines the project through **14 formal dimensions** plus **3 cross-dimensional hard principles**.

## Core thesis
The player is not a god and not a single adventurer.
The player is the head of a clan operating within a living world.

For the higher-level rules-driven living-world thesis, player leverage doctrine, and multi-generation consequence framing, see `RULES_DRIVEN_LIVING_WORLD.md`.

## Historical grounding baseline
Default player-facing grounding should read as `Northern Song-inspired`, not as a dynasty-agnostic mashup and not as a strict documentary reenactment.

That means the current baseline should lean toward:
- county society shaped by literati ambition, yamen paperwork, recommendation networks, and waiting for openings
- north-China road-county logic plus connected market and canal corridors where needed, rather than a flat all-China terrain voice
- family, office, trade, route security, and campaign pressure as one linked field

This baseline is a gameplay frame, not a claim that every mechanic is an exact Northern Song reconstruction.

The world:
- changes before the player acts
- produces pressure through people, households, institutions, markets, order, weather, banditry, office, and war
- is then projected back to the player through space, visitors, reports, ledgers, lineage surfaces, desk-sandbox nodes, and conflict aftermath

## Core loop
Authoritative loop:
1. advance one month
2. world modules simulate
3. structured diffs are produced
4. projections and notifications are built
5. player reviews pressure and opportunity
6. player issues bounded commands
7. the next month begins

## The 14 dimensions

### 1. Core gameplay and scope control
The irreplaceable loop is:
- review
- interpret
- intervene selectively
- accept consequences
- continue the lineage

Anything that does not strengthen this loop is expendable.

### 2. Engine and tools
The engine exists to host:
- the spatial shell
- the inspectors
- the desk sandbox
- the data-driven UI
- the asset pipeline

The simulation remains engine-agnostic.

### 3. Programming architecture
The project uses a **modular monolith**:
- small kernel
- feature modules
- deterministic scheduler
- persistence layer
- spatial presentation shell

This is required for extensibility and anti-coupling.

### 4. Art and audio assets
Low-to-moderate cost art direction is mandatory.
The game should rely on:
- room-state changes
- object-anchored UI
- stylized portrait modules
- short vignettes
- ambient sound layers
- detailed sourcing, license, and staging rules live in `ART_AND_AUDIO_ASSET_SOURCING.md`

### 5. Version control
Mainline must remain recoverable.
No zip-based backup workflow is acceptable.

### 6. Technical debt management
Debt must be treated as planned work.
No long-lived hack may silently become architecture.

### 7. Project organization
Repository and content organization must reflect module boundaries.

### 8. Data-driven content and configuration
Balance, authored templates, and system weights should live in validated config rather than hardcoded constants.

### 9. Testability and determinism
The world must be reproducible.
The same seed and inputs must yield the same outputs for authoritative systems.

### 10. UI and information architecture
The UI is not a thin wrapper.
It is a core expression of the game fantasy:
- great hall / study
- ancestral hall / lineage scroll
- desk sandbox
- conflict vignette
- inspectors and ledgers

### 11. Performance and simulation scale control
Simulation scale is achieved by tiered fidelity, not by simulating everything at full detail.

### 12. Saves and compatibility
Long-run saves are valuable.
Schema versioning and migration discipline are mandatory from the start.

### 13. Debugging and observability
The simulation must be inspectable.
The game may not become a black box.

### 14. Project management, milestone locks, and cutting discipline
Every release line needs:
- explicit done criteria
- explicit non-goals
- explicit cut rules

## Social and pathway structure
These are not separate game modes.
They are interconnected social positions and pathways:
- **Clan / family**: the player’s primary system
- **Commoners / households**: the social base and labor layer
- **Exams**: institutional upward mobility
- **Trade**: wealth and network mobility
- **Office**: formal authority and political leverage
- **Outlaw / banditry**: disorder, failure, coercion, and gray power

These positions must be able to transform into one another through world pressure and personal circumstance.
See `RULES_DRIVEN_LIVING_WORLD.md` for the stronger multi-route doctrine: these are overlapping social positions in one dynamic field, not hard career branches.

## Three cross-dimensional hard principles
1. **Explainable causality**
   - major outcomes must have readable cause traces

2. **Narrative and simulation separation**
   - text is never the authoritative state driver

3. **MVP and later versions stay structurally aligned**
   - MVP is not disposable prototype code
   - later versions extend the same kernel, module system, commands, events, IDs, and save rules

## Product anti-patterns
Do not turn the project into:
- a raw spreadsheet game
- a pure text parser
- a card battler
- an open-world walking simulator
- a detached tactics/RTS game
- a giant event-pool narrative machine
