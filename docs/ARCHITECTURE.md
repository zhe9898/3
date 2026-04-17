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

Kernel does **not** know what exams, trade, office, banditry, or war are.

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
- monthly phase execution
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
11. `NarrativeProjection` (projection-oriented, not authority source)

## Base simulation flow
1. scheduler begins month
2. world/settlement pressure updates
3. population/household pressure updates
4. family structure and household status updates
5. education, trade, office, order, conflict, war modules run if enabled
6. modules emit domain events
7. deterministic event handling pass
8. structured diff aggregation
9. narrative projection build
10. application exposes commands to player
11. player commands are staged for next month or resolved in permitted same-month windows

## Key architecture rule
A module may respond to another module’s event by updating **its own** state only.
It may not directly rewrite foreign state.

## Why this matters
This makes expansions additive:
- office can be added without rewriting family internals
- banditry can be added without hiding pressure inside generic random events
- warfare can be added as a campaign layer without replacing conflict or family loops
