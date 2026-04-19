# RULES_DRIVEN_LIVING_WORLD

This document captures the project's high-level design thesis for a rules-driven living world.
It complements:
- `PRODUCT_SCOPE.md`
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `SIMULATION.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `SOCIAL_STRATA_AND_PATHWAYS.md`
- `RELATIONSHIPS_AND_GRUDGES.md`

Use this file when evaluating whether a mechanic, UI surface, narrative wrapper, or roadmap proposal still feels like Zongzu.

## Core declaration

Zongzu's world is not a fixed profession tree, a static family chart, or an event table.
It is a society that keeps changing, splitting, recombining, and drifting under pressure.
The player may bias part of that motion, but may not freeze it.

## One-sentence pitch

The player is not the god of the world, but the head of one lineage inside it.
Politics, economy, kinship, marriage, resentment, status, and local pressure keep moving on their own; the player can only shape a limited piece of that motion through leverage they can afford and relationships they can still reach.

## Core experience

The intended pleasure is not instant control.
The intended pleasure is:
- seeing the world move without waiting for the player
- understanding why a change happened
- using money, grain, prestige, obligation, office access, and clan authority to alter only part of the outcome
- accepting that many consequences land later rather than immediately
- recognizing, across years or generations, how one marriage, one favoritism, one compromise, or one revenge choice changed the family's later fate

## Four hard design principles

### 1. The world runs first; the player intervenes second

The game loop is:
- simulation first
- diff detection second
- projection and readable notice third
- player intervention last

Events are not the driver of state.
Events, letters, rumors, memorials, and reports are the readable layer built after state change.

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

### 4. The world must self-run without the player

If player input is removed, the world should still produce plausible history across decades:
- families rise and decline
- lines continue or fail
- marriages create alliances and burdens
- resentments persist or cool
- local markets warm and collapse
- institutions open and close opportunity

## Player identity and boundaries

The player is a household or lineage head, not a creator of the world's rules.
At start, the player chooses an entry position, not a custom universe.

The player may choose:
- era pack
- region
- initial family standing
- initial resources and reputation
- house-style bias

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

This is the primary player surface.

Key concerns:
- genealogy
- lineage property
- ancestral prestige
- branch-mainline relations
- affinal network
- cohesion
- family memory and narrative
- grievance inheritance

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

The living world should use tiered simulation:
- core layer: the player's family, close kin, enemies, affines, and high-importance actors
- regional layer: same-county important households and institutions
- wider layer: summary simulation for distant regions and macro pressure

The monthly loop should keep this order:
1. environment and region pressure
2. production, prices, and routes
3. tax, security, and policy pressure
4. institution action
5. other families' local action
6. individual action
7. birth, illness, death, relationship, and asset settlement
8. diff detection
9. readable projection
10. player intervention
11. chronicle update

The most important rule is simple:
the player acts late in the month, after the world has already moved.

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
- lineage management
- commoner household survival
- commercial management
- official career and yamen service
- social governance
- bandit or shadow-power survival
- later imperial or macro-governance pressure

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
