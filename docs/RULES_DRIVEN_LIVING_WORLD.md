# RULES_DRIVEN_LIVING_WORLD

This document captures the project's high-level design thesis for a rules-driven living world.
It complements:
- `PRODUCT_SCOPE.md`
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `SIMULATION.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `SOCIAL_STRATA_AND_PATHWAYS.md`
- `RELATIONSHIPS_AND_GRUDGES.md`
- `LIVING_WORLD_DESIGN.md`
- `GAME_DEVELOPMENT_ROADMAP.md`

Use this file when evaluating whether a mechanic, UI surface, narrative wrapper, or roadmap proposal still feels like Zongzu.

## Core declaration

Zongzu's world is not a fixed profession tree, a static family chart, or an event table.
It is a society that keeps changing, splitting, recombining, and drifting under pressure.
The player may bias part of that motion, but may not freeze it.

## Rules-driven contract

A rule is not a story beat, popup, or hidden script.
A rule is allowed into the authoritative simulation only when it can answer:
- which module owns the state it changes
- which cadence runs it: xun, monthly, seasonal, annual, scenario setup, or command resolution
- which input state, query, command, or domain event it reads
- which deterministic calculation or table resolves it
- which owned state, domain event, structured diff, or projection receipt it emits
- which player command can influence it, if any
- which trace explains the result afterward

Short form:

`owned state + cadence + pressure + deterministic resolution -> owned state / event / diff -> projection -> bounded command -> next pressure`

If a mechanic cannot be placed on that chain, it is not yet a Zongzu rule.

## Rule stack

Most living-world mechanics should be decomposed into several small rule types instead of one large scripted event.

State rule:
- defines what exists and which module owns it

Pressure rule:
- converts circumstance into accumulating pressure, such as debt strain, heir insecurity, route exposure, faction heat, illness risk, or public shame

Resolution rule:
- resolves pressure at the declared cadence using deterministic inputs

Transfer rule:
- publishes cross-module consequences through query, command result, or domain event, never direct mutation

Visibility rule:
- turns structured diffs into notices, guidance, surfaces, and cause traces

Intervention rule:
- lets the player spend bounded leverage as an intent, then resolves that intent against autonomy, resources, institutions, risk, and world state

Memory rule:
- leaves residue: favor, shame, debt, fear, grudge, policy memory, faction label, or precedent

The stack may be lite in MVP, but it should still exist conceptually.
Do not compress everything into "event happened, stats changed, text appeared."

## Rule-driven is not formula-only

Rules may use formulas, tables, scenario data, authored descriptors, historical templates, or calibrated weights.
What matters is ownership and causality:
- data may configure a rule
- prose may explain a rule
- UI may expose a rule
- scenario setup may seed a rule
- only the owning module may apply the authoritative state change

This lets the game use historical material and authored flavor without becoming an event-pool narrative engine.

## Rule-driven is not history-locked

Rules-driven does not mean the historical path is fixed.
It means even large counterfactuals must be earned by state, pressure, leverage, and consequence.

The same spine that handles debt, marriage, death, office pressure, and local violence must eventually be able to scale into:
- protection failure
- armed autonomy
- rebel concentration
- provisional governance
- factional court struggle
- succession crisis
- usurpation, restoration, or regime repair
- dynastic consolidation or fatigue

The player may change history when the world-state path supports it.
The player may not edit history without passing through people, institutions, logistics, legitimacy, force, public belief, and memory.

## One-sentence pitch

The player is not the god of the world, but one household-side or lineage-side actor inside a living society.
Politics, economy, kinship, marriage, resentment, status, commoner survival, and local pressure keep moving on their own; the player can only shape a limited piece of that motion through leverage they can afford and relationships they can still reach.

## Core experience

The intended pleasure is not instant control.
The intended pleasure is:
- seeing the world move without waiting for the player
- understanding why a change happened
- using money, grain, prestige, obligation, office access, and clan authority to alter only part of the outcome
- accepting that many consequences land later rather than immediately
- recognizing, across years or generations, how one marriage, one favoritism, one compromise, or one revenge choice changed a house's later fate, whether that fate is endurance, rise, fragmentation, or collapse

## Four hard design principles

### 1. The world runs first; the player intervenes second

The game loop is:
- simulation first
- diff detection second
- projection and readable notice third
- player intervention last

Events are not the driver of state.
Events, letters, rumors, memorials, and reports are the readable layer built after state change.

This does not mean every internal cadence becomes a player turn.
The normal player rhythm is monthly review and bounded monthly command.
`xun` exists so illness, debt, route trouble, rumor, office delay, and disorder can accumulate with lived texture before consolidation.
Only red-band or irreversible pressure should interrupt the month, and even then the interrupt is a narrow response window rather than a replacement for the monthly shell.

### 2. The player's influence is always bounded

The player has strongest reach inside the household and lineage.
Influence becomes weaker through clan networks, affinal ties, local institutions, and wider politics.

There should be no one-click omnipotent success.

### 3. There are no fixed life tracks

People do not choose from rigid `civil / military / trade` routes.
They drift through opportunity structures shaped by:
- talent
- household investment
- local conditions
- institutional demand
- rivalry
- historical moment
- personal appetite and fear

### 3a. Downward mobility is mandatory

The world is not alive if everyone important stays above the social floor.

Houses, branches, and individuals must be able to:
- fall from local standing into ordinary commoner survival
- lose office access, educational momentum, or marriage value
- slide from respectability into debt dependence, migration, hired service, or gray survival
- remain poor for generations instead of automatically climbing

Likewise, a poor or ordinary start must be valid.
The player may begin from a precarious household position and try to stabilize, endure, or climb, rather than only inheriting a secure elite shell.

### 4. The world must self-run without the player

If player input is removed, the world should still produce plausible history across decades:
- families rise, decline, and drop in status
- lines continue or fail
- marriages create alliances and burdens
- resentments persist or cool
- local markets warm and collapse
- institutions open and close opportunity

## Player identity and boundaries

The player is a household-side or lineage-side decision maker, not a creator of the world's rules.
At start, the player chooses an entry position, not a custom universe.
That entry may be stronger or weaker, respectable or precarious, lineage-centered or closer to ordinary household survival.

The player may choose:
- era pack
- region
- initial family standing
- initial resources and reputation
- house-style bias
- scenario-specific starting stratum when the pack exposes it

The player may not directly rewrite:
- macro institutions
- climate law
- war outbreak rules
- the existence of other actors' ambition
- basic human autonomy

## The six-layer world

### 1. Individual

A person is a continuing agent, not a card.

Key concerns:
- age, sex, marriage and fertility status
- health, stress, mood, and lifespan risk
- study, competence, sociability, administrative ability
- desire for name, wealth, safety, heirs, power, or attachment
- obedience, independence, and risk tolerance
- relationships, place, role, and income source

### 2. Household

The household is the basic survival unit.

Key concerns:
- income, spending, and debt
- spousal tension
- children and dependency structure
- labor and support burden
- property, tenancy, and shop or land holdings
- internal authority distribution

### 3. Clan / lineage

This is a primary player surface, not a guarantee of permanent status or control.

Key concerns:
- genealogy
- lineage property
- ancestral prestige
- branch-mainline relations
- affinal network
- cohesion
- family memory and narrative
- grievance inheritance
- downward drift, fragmentation, and loss of standing

### 4. Settlement

Village, town, county, and prefectural environments are the stage of social life.

Key concerns:
- population
- grain price, wages, land value
- security
- industry mix
- school access
- office reach
- elite and strongman distribution
- route quality
- disaster exposure

### 5. Institution

Office, academy, guild, military unit, temple, escort bureau, and similar bodies must act on the world rather than sit as backdrop.

Key concerns:
- power
- recruitment demand
- screening bias
- resource absorption
- relation to local families and settlements

### 6. Region / world

The world layer shapes opportunity and constraint without becoming a grand-strategy map first.

Key concerns:
- harvest quality
- tax and policy shifts
- epidemic or war pressure
- trade heat
- exam and military opportunity climate

## Simulation shape

The living world should use tiered simulation and tiered cadence:
- core ring: the player's household, close kin, enemies, affines, and other high-importance actors
- local ring: same-county important households, institutions, and route-linked nodes
- wider ring: summary simulation for distant regions and macro pressure

The world should not breathe at one flat speed.
Use:
- `xun` pulses for nearby life, livelihood strain, rumor, delay, and local drift
- monthly review shells for consolidation, projection, and bounded player intervention
- seasonal or annual bands for slower structures such as harvest, exam heat, major war posture, and broader policy climate

The high-level order should remain:
1. open a monthly shell
2. let the world run multiple inner pulses
3. consolidate state and structured diffs at month-end
4. build readable projection
5. allow bounded player intervention
6. carry the result into the next month

The most important rule is still simple:
the player acts late, after the world has already moved.

The fidelity rule for deciding who becomes a full agent, who stays household- or node-level, how upper layers remain alive as pressure rather than dead backdrop, and how different rings use different time density is defined in `SIMULATION_FIDELITY_MODEL.md`.

## Events are a readability layer, not a driver

Correct logic:

`state change -> diff detection -> significance evaluation -> narrative packaging`

Not:

`draw event -> world changes because event says so`

Notifications should be stratified:
- must-handle
- worth-watching
- background rumor

Importance should be computed from:
- kinship or benefit distance
- affected population
- asset change ratio
- reversibility
- time sensitivity
- chain-reaction potential
- whether the player can meaningfully intervene

## MVP rule chain examples

These are concrete, playable examples that the MVP must support. They are not abstract shapes; they are acceptance criteria.

### MVP Example 1: Heir death → succession crisis → resolution
1. **Owning module:** `PopulationAndHouseholds` (mortality) + `FamilyCore` (succession)
2. **Cadence:** Monthly (death resolved at month-end; succession command in player phase)
3. **Pressure:** Adult death + no confirmed heir = succession insecurity (0–100)
4. **Deterministic resolution:** `FamilyCore` checks eligible heirs by branch rules (age, gender, proximity); if none, flag `UNSETTLED`
5. **Cross-module:** `SocialMemoryAndRelations` records branch tension; `NarrativeProjection` surfaces notice
6. **Player command:** `议定承祧` — spend prestige, select heir
7. **Autonomy check:** Selected heir may resist if too young or from rival branch
8. **Receipt:** `入谱定名` (confirmed) or `承祧未稳` (deferred)
9. **Memory:** Branch tension decays over months; if unresolved, `SocialMemoryAndRelations` may register `succession_grudge`
10. **Trace:** `death <- illness_progression <- age_vulnerability <- no_heir <- 承祧未定 <- 议定承祧 <- 入谱定名`

### MVP Example 2: Grain price spike → household debt → player intervention
1. **Owning module:** `WorldSettlements` (seasonal band) + `TradeAndIndustry` (price) + `PopulationAndHouseholds` (debt)
2. **Cadence:** Xun pulse (price movement) → Monthly (debt consolidation)
3. **Pressure:** `SeasonBand.HarvestWindowProgress` low + `LaborPinch` high → grain price band rises
4. **Deterministic resolution:** Price band feeds `TradeAndIndustry` route profitability; household grain consumption exceeds income
5. **Cross-module:** `PopulationAndHouseholds` accumulates debt strain; `FamilyCore` may receive support request
6. **Player command:** `GuaranteeDebt` or `InvestEstate` or `Endure`
7. **Autonomy check:** Debt guarantor (if external) may refuse if player credit low
8. **Receipt:** `保状留底` (guarantee accepted) or `债压加深` (endured)
9. **Memory:** Debt record persists; future `GuaranteeDebt` commands check against existing exposure
10. **Trace:** `drought <- harvest_failure <- grain_spike <- consumption_exceeds_income <- debt_pressure <- GuaranteeDebt <- 保状留底`

### MVP Example 3: Exam failure → social drift → household reabsorption
1. **Owning module:** `EducationAndExams` (exam resolution) + `PopulationAndHouseholds` (labor)
2. **Cadence:** Seasonal (exam season) → Monthly (result projection)
3. **Pressure:** Study investment + exam attempt + random competence factor
4. **Deterministic resolution:** Threshold check against exam difficulty, study quality, household support
5. **Cross-module:** `FamilyCore` receives result; aspirant mood updated; `PopulationAndHouseholds` adds labor if aspirant returns to household work
6. **Player command:** `FundStudy` (retry) or redirect to trade (implicit via `InvestEstate`) or `Endure`
7. **Autonomy check:** Aspirant may refuse further study if morale too low; may autonomously drift to trade or clerical work
8. **Receipt:** `科场未第` (failure) or `及第` (pass); labor change receipt
9. **Memory:** Exam result recorded in personal history; affects marriage value and branch prestige
10. **Trace:** `study_investment <- exam_attempt <- competence_roll <- failure <- 科场未第 <- FundStudy? <- 学业不继`

## Rule chain examples (general shapes)

Each example below is a shape, not a required exact formula.

Heir death:
- pressure starts as mortality / violence / illness in the owning cause module
- `FamilyCore` receives death as a fact, resolves succession insecurity, mourning burden, branch tension, and elder fragility
- `SocialMemoryAndRelations` records shame, favor, resentment, or succession grievance
- `NarrativeProjection` shows hall guidance: stable adult substitute, infant-only heir pressure, or no-heir crisis
- player can spend lineage authority, property, marriage leverage, adoption / succession commands when available, or mediation support

Commoner debt slide:
- `PopulationAndHouseholds` accumulates livelihood and debt strain
- `TradeAndIndustry` and `WorldSettlements` contribute grain price, wage, route, and market opportunity
- household may drift toward hired service, migration, study withdrawal, petition, patronage, or gray survival
- projection shows the social chain, not a route label
- player intervention depends on reachable leverage: grain, loan guarantee, work connection, lineage shelter, yamen document help, or doing nothing

Frontier pressure:
- seasonal / annual war posture raises fiscal, recruitment, transport, and public-rumor pressure
- `WarfareCampaign` owns campaign fatigue and military burden
- `WorldSettlements`, `TradeAndIndustry`, and `PopulationAndHouseholds` feel supply, price, labor, and route effects
- `PublicLifeAndRumor` projects frontier talk and legitimacy strain
- player can only respond through local support, escort, office leverage, grain, public stance, or later higher political reach

Historical reform:
- great-trend pressure accumulates before the famous policy year
- actor carriers gain reputation, enemies, writings, and institutional access
- a court or office window opens
- local modules receive policy pressure through tax, loan, school, militia, purchasing, or paperwork rules
- implementation may be delayed, captured, resisted, or remembered
- the player can carry, localize, resist, or distort it only through valid influence circles

## Player leverage instead of god-buttons

The player acts through four primary levers:
- money and grain
- prestige
- obligation and favor
- family authority

Typical actions include:
- order
- persuade
- reward
- punish
- subsidize
- cut support
- rebalance family property
- accelerate or delay marriage
- ask intermediaries to intervene
- borrow outside leverage
- absorb insult and keep score
- seek mediation or private settlement

Reach decreases outward:
- household: strongest but not absolute
- clan and affinal network: meaningful but unstable
- local society: indirect
- world politics: mostly adaptive

## Open pathways, not fixed routes

People should slide between positions rather than pick careers from a tree.

Five force families shape action:
- personal ability and lack
- household investment and constraint
- local demand and opportunity
- social and institutional access
- desire and risk appetite

This is how one literate child can move toward office, trade, military service, or domestic responsibility depending on pressure and circumstance.

## Multi-route society, not one family lane

Zongzu should not become "a family game plus decorations."
The living world should support several overlapping route families:
- lineage management **(MVP: full)**
- commoner household survival **(MVP: full)**
- commercial management **(MVP: lite)**
- official career and yamen service **(MVP: projection only)**
- social governance **(MVP: projection only)**
- bandit or shadow-power survival **(MVP: lite if M3 enabled)**
- later imperial or macro-governance pressure **(post-MVP)**

These are not rigid classes, fixed campaigns, or isolated game modes.
They are social positions and management fantasies that overlap in one society.

They should also be read as different forms of life rather than abstract career labels:
- lineage management is about preserving, dividing, and steering a powerful family
- commoner household survival is about keeping ordinary people fed, housed, mobile, and socially protected
- commercial management is about moving goods, credit, trust, labor, and transport through unstable local conditions
- official and yamen service is about entering institutions, waiting for openings, depending on patrons, and surviving procedural pressure
- social governance is about petitions, order, faction balance, public works, and local legitimacy
- bandit or shadow survival is about coercion, shelter, extraction, recruitment, and surviving outside regular protection
- imperial or macro-governance is about appointments, grain security, legitimacy, military burden, and dynastic fatigue

This means:
- a household may be commercial in one season and desperate in the next
- a lineage may operate estates, marriage strategy, office ambition, and private coercion at the same time
- a failed exam path may spill into trade, clerical service, migration, militia, or outlaw risk
- a commoner household may pressure elite systems through labor, tax, transport, petition, disorder, and marriage ties
- a local office actor may still remain trapped inside family, patronage, debt, and reputation obligations
- the same family may support several routes at once across siblings, branches, retainers, tenants, and affines
- not every route is upward mobility; some are fallback, drift, endurance, or damage control

The design goal is not "many classes."
The design goal is a dynamic social field in which actors slide between routes under pressure.

For the formal route-family matrix, module mapping emphasis, surfaces, and transition rules, see `MULTI_ROUTE_DESIGN_MATRIX.md`.

## Living human lives, not route tokens

Multi-route design matters because this project is about living people, not abstract role slots.

People should appear to live through:
- hunger and subsistence
- debt and rent pressure
- labor splitting inside a household
- schooling and abandonment of schooling
- marriage bargaining
- illness, aging, pregnancy, and death
- migration, transport work, service work, and workshop work
- patronage, shame, resentment, and dependency

The strongest version of Zongzu does not merely say that different routes exist.
It shows how different kinds of people keep trying to live:
- one child studies while another carries goods
- one branch seeks office while another keeps a shop alive
- one widow protects her household through kin pressure while another line collapses under debt
- one failed scholar turns clerk, trader, guard, or wanderer instead of vanishing from play

That is why route design must stay tied to household fragility, local opportunity, and changing social protection rather than becoming a menu of static life scripts.

## Marriage, upbringing, and inheritance are structural, not decorative

Marriage is one of the strongest cross-system interfaces in the game.
It should change:
- finance
- prestige
- kin network
- inheritance shape
- office opportunity
- grudge and reconciliation chains

Upbringing should work through household policy bias more than individual micromanagement:
- strict or lenient
- study, ritual, trade, or martial emphasis
- protect the main line or promote merit
- shield kin or enforce public judgment

Inheritance, succession, and branch split are not side dramas.
They are recurrent structural transformations.

## Wealth must stay embedded in local society

Industry is not an abstract money factory.
Fields, shops, workshops, and routes live inside:
- weather
- security
- labor supply
- local demand
- state pressure
- reputation
- competition

Wealth should feed back into:
- family attractiveness in marriage
- education investment
- envy and predation
- local voice
- external leverage

## Grudges and social memory are long-horizon engines

Without grudges, the world merely turns.
With grudges, the world keeps books on pain, shame, debt, fear, and obligation.

Each person and each lineage should effectively maintain:
- a public ledger
- a hidden ledger

Grievances should include at least:
- blood grievance
- property grievance
- reputation grievance
- authority grievance

Good revenge logic is not random.
It is:

`grievance + opportunity + capability + acceptable risk = possible action`

Resolution should include:
- revenge
- compensation
- apology
- formal judgment
- marriage-based settlement
- suppression without true reconciliation
- generational fading

False peace should exist.
So should old debts reactivating later.

## AI actors must feel self-directed

Adults are not puppets.

Agent decisions should be pulled by:
- survival pressure
- household duty
- personal desire
- current opportunity
- perceived risk
- relationship obligations
- available resources

The real gameplay of being family head is managing:
- who truly supports you
- who merely obeys for now
- who is silently keeping score
- who might defect when a better patron appears

## Causality and chronicle

The game must help the player understand:
- what changed
- why it changed
- who changed it
- what can still be done

Major outcomes should expose short cause chains.

Example form:

`study failed <- cash pressure <- grain spike <- drought`

This is what turns randomness into living-world believability.

## UI consequence

The UI should not feel like an event-popup game.
It should feel like a lineage observation and decision game.

Priority surfaces remain:
- family overview
- genealogy
- monthly change review
- letters and reports
- local rumor and notice
- industry ledgers
- affinity and enemy network
- yearbook or chronicle

Information should remain layered:
- household facts are clearest
- nearby families are clearer than distant ones
- remote truth often arrives through rumor, memorial, or letter
- secrets require time, inference, or investigation

## Integration rule

When using this design document:
- keep rule ownership in modules
- keep readable outcome text in projection
- keep shell work aligned to the spatialized hall / desk / lineage fantasy
- keep MVP and post-MVP on the same structural spine

If a proposal breaks those rules, it may be interesting, but it is no longer Zongzu.

## Rule acceptance checklist

Before adding a mechanic, scenario feature, historical process, or UI prompt, answer:
- What state change would happen if the player did nothing?
- Which module owns that change?
- Which cadence runs it?
- What pressure causes it?
- What downstream module can read it, and through which query / command / event seam?
- What diff proves that it happened?
- What projection explains it?
- What leverage can the player spend?
- What friction or autonomy can block the player's intent?
- What memory remains after the month closes?

If the answer begins with "show an event where..." rather than "resolve a rule that...", the design is still too soft.
