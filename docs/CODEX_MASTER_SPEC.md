# CODEX_MASTER_SPEC

## One-paragraph summary
Build a Windows single-player living-society simulation inspired by Northern Song China, where the world advances through day-level internal authority motion inside a monthly review shell before the player acts. Day steps are lived-time cadence, not routine player turns; `xun` may appear as almanac wording or projection grouping, not as the bottom authority grid. Normal player review and command happen monthly, with only red-band irreversible crises allowed to interrupt. The player acts from a continuing home-household seat using bounded leverage inside a spatialized shell: great hall or study, ancestral hall, desk sandbox, and conflict or campaign vignettes. The authoritative architecture is a modular monolith whose modules own their own state and communicate through Query, Command, and DomainEvent. Household life, lineage pressure, commoner survival, exams, trade, office, outlaw or banditry, conflict, later campaign warfare, and eventual imperial / dynasty-cycle pressure are interlinked social pathways rather than isolated game modes. The Renzong-era opening is a calibrated starting field, not a fixed historical rail; later outcomes may diverge through earned rule chains, institution constraints, people, locality, and player influence.

## Non-negotiable pillars
- world-first living simulation with day-level authority motion inside a monthly review shell, without turning days or xun labels into routine player turns
- module-owned state namespaces
- deterministic authority
- explanation traces
- high-cohesion, low-coupling code structure
- modern game engineering standards apply to code, module, system, Unity presentation, content, performance, and observability layers
- maintainable file and type size; split oversized source files by ownership or workflow seam
- explicit boundary ownership
- no blocking IO in authority hot paths
- no authority or debug leakage into player-facing shell
- no fixed-person RPG identity model and no clan-god control; the player continuity is the home-household seat
- no player-facing emperor button, edict editor, or unearned court-control surface; imperial pressure must arrive through mediated objects, institutions, and rule chains
- no new deprecated libraries or weak glue-code layers
- narrative downstream of simulation
- MVP as the substrate for later versions
- history can be changed by earned rule chains, not by free timeline editing
- historical opening conditions are calibration, not fixed rails
- spatialized shell
- conflict integrated with lineage, economy, office, and grudge systems
- backend structure and contracts stabilize before deep rule density

## Codex implementation shorthand
When writing code, assume these rules are hard:
- if a source file is drifting past roughly `400` logical lines, check whether responsibilities are already mixed
- if a non-generated source file passes roughly `600` logical lines, splitting is the default expectation
- do not mix domain rules, command routing, persistence, projection wording, and diagnostics in one file
- do not perform blocking file or network IO inside `day`, `month`, `seasonal`, event-handling, or diff-generation paths
- application code may route commands, but must not directly mutate foreign authoritative module state
- presentation code may format and arrange, but must not resolve outcomes
- saves and public read models must not leak secrets, local machine paths, stack traces, hidden future information, or debug-only traces
- do not introduce deprecated libraries, weak glue layers, or stringly-typed cross-layer protocols where explicit contracts can exist
- every non-trivial system should be describable as `inputs -> owned state -> cadence -> rules -> diffs/events -> projections -> player leverage -> tests`

## Core module map
- WorldSettlements
- FamilyCore
- PopulationAndHouseholds
- SocialMemoryAndRelations
- EducationAndExams
- TradeAndIndustry
- OfficeAndCareer
- OrderAndBanditry
- ConflictAndForce
- WarfareCampaign
- NarrativeProjection

## Product form
Not:
- raw spreadsheet
- raw text parser
- card battler
- tactical battle game

Instead:
- spatialized living-society simulation

## Scope ladder
For the phase-by-phase implementation index, see `GAME_DEVELOPMENT_ROADMAP.md`.

### MVP
- kernel + modular spine
- household/lineage/commoner/social-memory substrate
- lite exams and trade
- spatial shell
- optional local conflict lite

### Later
- office
- full order/banditry
- full force system
- campaign sandbox
- regional breadth
- imperial / dynasty-cycle pressure, including rebellion, polity formation, succession struggle, usurpation, restoration, or dynasty repair when later packs exist
- richer room states and analytics

## Architectural rule
A feature is only acceptable if it can be added as:
- owned state in one module
- public queries
- commands
- domain events
- tests
- schema/version entries
without requiring arbitrary writes into other module internals.

## Backend implementation rule
Backend work should stabilize module ownership, state shape, cadence, command/query/event contracts, and save schema before deepening formulas. Application services stay thin, modules own consequences, and shell wording stays downstream of authoritative state.
