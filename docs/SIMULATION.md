# SIMULATION

This document defines the authoritative monthly simulation loop.

## Monthly tick phases

### Phase 0: prepare
- current date known
- staged player commands validated
- enabled feature manifest loaded
- projection caches invalidated as needed

### Phase 1: world baseline
`WorldSettlements`
- update environment/security/prosperity
- update routes and institution baseline availability

### Phase 2: population and household pressure
`PopulationAndHouseholds`
- livelihood pressure
- labor demand/supply
- migration pressure
- tenant and worker instability

### Phase 3: family structure
`FamilyCore`
- births, deaths, marriage status, inheritance, branch pressure
- family policy effects and support allocation outcomes

### Phase 4: social memory
`SocialMemoryAndRelations`
- memory promotion
- obligation, fear, shame, grudge drift
- clan narrative updates

### Phase 5: upward mobility and economy
Enabled modules run in deterministic order:
1. `EducationAndExams`
2. `TradeAndIndustry`
3. `OfficeAndCareer` if enabled
4. `OrderAndBanditry` if enabled
5. `ConflictAndForce` if enabled
6. `WarfareCampaign` if enabled

### Phase 6: domain event handling
- modules emit events
- deterministic event queue processed
- handlers update only owning module state

### Phase 7: diff generation
Structured diff records created for:
- people
- households
- clans
- settlements
- institutions
- conflicts/campaigns if enabled

### Phase 8: projection and narrative
`NarrativeProjection`
- urgent / consequential / background grouping
- letters, reports, rumors, council prompts
- explanation trails

### Phase 9: player review and command
The player acts through bounded commands:
- issue commands
- mark watch items
- adjust policies
- choose local interventions
- no direct rewrite of world state

### Phase 10: finalize month
- replay hash checkpoint
- autosave opportunity
- advance date

## Why this order exists
The world should not wait for the player to make it move.
The player reviews the movement after the world has already changed.

## Diff requirements
Every month should be able to answer:
- what changed?
- who changed?
- why did it change?
- what can the player still do?

## Scheduler extension rule
A new module must declare:
- which phase it participates in
- what events it emits
- what events it handles
- what projections it publishes

No module may insert ad hoc hidden execution outside the scheduler.
