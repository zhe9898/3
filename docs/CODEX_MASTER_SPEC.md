# CODEX_MASTER_SPEC

## One-paragraph summary
Build a Windows single-player lineage simulation game in which a living world advances month by month before the player acts. The player is a clan head operating through bounded leverage inside a spatialized shell: great hall/study, ancestral hall, desk sandbox, and conflict/campaign vignettes. The authoritative architecture is a modular monolith whose modules own their own state and communicate through Query, Command, and DomainEvent. Family is the primary player system; commoners are the social base; exams, trade, office, outlaw/banditry, conflict, and later campaign warfare are interlinked social pathways rather than isolated game modes.

## Non-negotiable pillars
- world-first monthly simulation
- module-owned state namespaces
- deterministic authority
- explanation traces
- narrative downstream of simulation
- MVP as the substrate for later versions
- spatialized shell
- conflict integrated with lineage/economy/grudge systems

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
- spatialized family simulation

## Scope ladder
### MVP
- kernel + modular spine
- family/commoner/social-memory substrate
- lite exams and trade
- spatial shell
- optional local conflict lite

### Later
- office
- full order/banditry
- full force system
- campaign sandbox
- regional breadth
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
