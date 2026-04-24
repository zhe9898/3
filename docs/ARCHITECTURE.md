# ARCHITECTURE

## Architectural choice
Use a **modular monolith**.

That means:
- one process
- one save root
- one scheduler
- one world state container
- multiple feature modules with owned state and explicit contracts

This architecture is chosen because the project needs:
- strong extensibility
- clean anti-coupling rules
- deterministic simulation
- practical single-developer ergonomics

## Top-level layers

### 1. Kernel
Owns:
- IDs
- time values
- numeric conventions
- RNG contracts
- common result/error types
- base event and command abstractions
- **PersonRegistry**: identity-only anchor (PersonId, display name, birth date, gender, life stage, alive/dead, fidelity ring)

Kernel does **not** know what exams, trade, office, banditry, or war are.

PersonRegistry is **not** a mutable world-state base. It holds only stable identity facts that change rarely (life stage advances monthly, death is a one-time transition). All domain-specific person state (personality, abilities, health, social position, activity, kinship, memories) belongs to the respective domain modules. See `PERSON_OWNERSHIP_RULES.md`.

The design test: if PersonRegistry grows beyond identity anchors + life stage + fidelity ring, something is leaking into Kernel that should live in a module.

### 2. Contracts
Owns:
- module registration interfaces
- query contracts
- command DTOs
- domain event DTOs
- projection contracts
- save/module-state contracts

### 3. Scheduler
Owns:
- module registration order
- `day / month / seasonal` cadence inspection
- day-level authority steps with safe batching/skipping of quiet spans, plus month-end consolidation
- deterministic event queue handling
- diff aggregation boundaries

### 4. Modules
Each authoritative module owns:
- its own state namespace
- its own commands
- its own event handlers
- its own projections
- its own tests

### 5. Application
Owns:
- orchestration
- use cases
- player command routing
- save/load orchestration
- debug operations
- module pack enabling

### 6. Persistence
Owns:
- save root serialization
- module state serialization
- schema version checks
- migrations
- replay snapshot support

### 7. Presentation
Owns:
- Unity scenes
- object-anchored UI
- great hall / ancestral hall / desk sandbox shell
- inspectors
- non-authoritative animations and vignettes

## Authoritative module list
0. `PersonRegistry` (Kernel layer, identity-only; see `PERSON_OWNERSHIP_RULES.md`)
1. `WorldSettlements`
2. `FamilyCore`
3. `PopulationAndHouseholds`
4. `SocialMemoryAndRelations`
5. `EducationAndExams`
6. `TradeAndIndustry`
7. `OfficeAndCareer`
8. `OrderAndBanditry`
9. `ConflictAndForce`
10. `WarfareCampaign`
11. `PublicLifeAndRumor`
12. `NarrativeProjection` (projection-oriented, not authority source)

## Base simulation flow
1. scheduler opens the monthly review shell
2. scheduler runs due day-level authority steps for modules that declare `day`, batching or skipping quiet spans deterministically
3. month-end authority consolidation runs for modules that declare `month`
4. modules emit domain events
5. deterministic event handling pass runs before projection
6. structured diff aggregation remains month-facing
7. narrative projection builds the readable month-end shell
8. application exposes bounded commands to the player review surface
9. player commands are staged for the next month or resolved only in explicitly permitted same-month windows

## Key architecture rule
A module may respond to another module’s event by updating **its own** state only.
It may not directly rewrite foreign state.

## Why this matters
This makes expansions additive:
- office can be added without rewriting family internals
- banditry can be added without hiding pressure inside generic random events
- warfare can be added as a campaign layer without replacing conflict or family loops
