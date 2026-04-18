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
4. `ConflictAndForce` if enabled
5. `OrderAndBanditry` if enabled
6. `WarfareCampaign` if enabled

Current M3 local-conflict note:
- `ConflictAndForce.Lite` may refresh force posture before `OrderAndBanditry.Lite` reads same-month response support
- only activated local-conflict response state may feed same-month order relief; calm or standing-but-untriggered posture stays visible but does not leak relief
- `ConflictAndForce.Lite` still reads only published query state and does not mutate `OrderAndBanditry` directly
- `OfficeAndCareer.Lite`, when enabled through the governance-lite path, now runs ahead of conflict/order so jurisdiction leverage can be read as bounded same-month administrative support without direct writes
- `ConflictAndForce.Lite` may now also carry campaign-fatigue and escort-strain fallout forward across months; those penalties recover during its own monthly pass and never require foreign-state writes

### Phase 6: domain event handling
- modules emit events
- deterministic event queue snapshot processed before projection
- handlers update only owning module state
- current active handler seam runs after authority modules finish their monthly pass and before `NarrativeProjection` builds notices
- current handler seam is single-sweep per month: follow-on events may reach projection, but they do not trigger another handler cascade in the same month
- `ConflictAndForce` now uses that seam to turn warfare aftermath into owned fatigue / readiness fallout after `WarfareCampaign` runs, so local-force wear can be visible immediately without moving authority rules into UI
- the same seam now also lets warfare aftermath land in civilian livelihood (`PopulationAndHouseholds`), settlement prosperity/security (`WorldSettlements`), and clan standing (`FamilyCore`) through owned-state updates only

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
- projection now sees both source campaign events and any handler-emitted follow-on events from downstream modules in the same month

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
