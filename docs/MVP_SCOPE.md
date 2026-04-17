# MVP_SCOPE

This document defines the first shippable vertical slice.

## MVP question
The MVP exists to answer one question:

**Can a living local lineage world run month by month, with bounded player intervention, readable consequences, and a spatial shell that feels like being a clan head?**

If the answer is yes, the MVP is successful.

## MVP feature packs

### Mandatory kernel/infrastructure
- `Kernel`
- `Contracts`
- `Scheduler`
- `Application`
- `Persistence`
- `NarrativeProjection`
- `Presentation.Unity` shell

### Mandatory gameplay modules
- `WorldSettlements`
- `FamilyCore`
- `PopulationAndHouseholds`
- `SocialMemoryAndRelations`
- `EducationAndExams.Lite`
- `TradeAndIndustry.Lite`

### Conditional MVP-plus modules
Include only if they fit schedule without harming the above:
- `OrderAndBanditry.Lite`
- `ConflictAndForce.Lite`

### Explicitly not in MVP
- `OfficeAndCareer` full
- `WarfareCampaign`
- multi-region strategic play
- full outlaw faction play
- advanced institution politics
- free-roam exploration
- tactical battle maps

## MVP player-facing pillars
1. monthly review and command
2. family tree and branch management
3. marriage, birth, death, inheritance pressure
4. education/exam pressure in light form
5. local trade/economy in light form
6. memories, grudges, and visible consequences
7. spatial shell:
   - great hall / study
   - ancestral hall / lineage surface
   - desk sandbox
   - conflict vignette if conflict pack is enabled

## MVP scope by pathway

### Family / clan
Must be robust and complete enough to carry the game.

### Commoner / household base
Must exist as the labor, migration, pressure, and support layer.

### Exams
Must exist in light form:
- study state
- academy availability
- exam attempts
- pass/fail outcomes
- explanation traces

### Trade
Must exist in light form:
- estates and shops
- local routes
- cash/grain/debt pressure
- local profit/loss explanation

### Office
Only indirect effects through institutions and titles if needed.
No full office module yet.

### Outlaw / banditry
Only if the lite pack fits:
- local security pressure
- raid/disorder risk
- some actors falling into outlaw/bandit states
- no fully managed outlaw ecosystem

## MVP quality bar
The MVP must already provide:
- deterministic 20-year headless run
- playable 10-year interactive run
- valid save/load roundtrip
- no impossible kinship states
- readable cause traces for major outcomes
- no event-pool core loop
- no module boundary violations

## MVP cut rules
Cut any feature that does not directly strengthen:
- lineage pressure
- commoner/household pressure
- local institutions
- trade/exam upward mobility
- grudge persistence
- bounded command play
- readable explanation
