# SIMULATION_FIDELITY_MODEL

This document defines how Zongzu keeps the whole society alive without simulating every person at the same fidelity.

It complements:
- `RULES_DRIVEN_LIVING_WORLD.md`
- `SIMULATION.md`
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `MAP_AND_SANDBOX_DIRECTION.md`

## Core declaration

Simulation precision is not chosen only by whether the player is a villager, county resident, or lineage head.

Zongzu should use this rule instead:

**focus ring decides precision; prosperity decides pool thickness.**

In other words:
- not every important place is high-fidelity all the time
- not every poor place is low-fidelity all the time
- the world stays alive everywhere
- only some parts of it become person-dense and fully agentized at a given moment

## Why this exists

This model is meant to preserve all of the following at once:
- living-world motion
- understandable cause and effect
- bounded performance cost
- room for downward mobility and social drift
- long-term save compatibility
- a shell that shows the player why something matters before exposing full detail

Without a fidelity model, the design tends to collapse into one of two bad shapes:
- full-map over-simulation that becomes expensive, noisy, and unreadable
- shallow backdrop simulation where upper layers stop feeling alive

## Anti-patterns

- tying simulation fidelity only to player identity
- simulating every person in the current settlement 1:1 forever
- turning upper layers into dead backdrop when the player starts low
- using prosperity as a direct synonym for precision
- forcing one global fidelity level across village, county, prefecture, and route scales
- refusing to promote or demote actors as they become more or less relevant

## Four fidelity drivers

Every person, household, node, or region should be evaluated through these four dimensions together:

1. distance from the player
2. whether current pressure actually hits that actor or layer
3. settlement scale or prosperity
4. whether the player must be able to read that layer clearly

Suggested priority order:
1. pressure hit
2. player distance
3. player readability and intervention potential
4. settlement scale or prosperity

This protects the design from a common mistake:
- a poor remote village may still need high fidelity because the player lives there
- a rich prefectural center may stay mostly summary-level because the player cannot yet reach or interpret it

## Three operational rings

The active simulation should normally be described through three operational rings.

### 1. Core ring

This is true 1:1 agent simulation.

Typical members:
- the player household
- close kin
- key affines
- feud partners
- key branch actors
- important dependents
- current hotspot actors
- nearby figures whose choices can immediately bend the month

Core-ring actors should support:
- individual relationships
- desire and fear
- obedience and independence
- household role
- mobility decisions
- trackable life events
- explainable action traces

This is where Zongzu should feel most like `living human lives`.

### 2. Local ring

This is mixed simulation:
- some named agents
- many summarized households and pools

Typical members:
- same-county important houses
- same-county institutions
- market and ferry nodes
- workshop clusters
- lineage-adjacent branches
- county-facing brokers and officials

Most ordinary people in this ring should not stay permanently 1:1.
Instead, they should mostly exist through structured abstractions such as:
- `household_band`
- `labor_pool`
- `marriage_pool`
- `rumor_pool`
- `migration_pool`
- `service_pool`

This ring exists to keep county society thick without exploding person count.

### 3. Regional ring

This is summary simulation with occasional person or node concretization.

Typical scope:
- prefectural layers
- route-level layers
- larger market corridors
- military or disorder pressure beyond one county
- wider office and education climate

This ring should usually run:
- grain and price heat
- route safety and blockage
- tax and extraction pressure
- official or institutional temperature
- disorder, bandit, and war pressure
- education and office opportunity climate
- implementation drag and policy unevenness

The regional ring should not generate and persist full populations by default.
It should become more concrete only when pressure, visibility, or intervention demands it.

## Upper rule and imperial pressure

For low-status or village-start play, upper layers should not disappear.

The throne, court, prefecture, route, and county apparatus may be less person-dense,
but they must remain alive as:
- policy pressure
- legal reach
- tax demand
- corvee and levy burden
- price movement
- road and water opportunity
- order and disorder temperature
- educational and office openings
- legitimacy and rumor climate

The important distinction is:
- upper layers do not always need dense people simulation
- upper layers must still exist as living pressure and opportunity fields

## Prosperity and settlement scale

Prosperity should mostly determine capacity, not fidelity.

It is best used to influence:
- population pool size
- market opportunity thickness
- marriage-pool depth
- education and office opportunity
- commercial and service demand
- inward and outward migration volume
- dynamic talent emergence probability

Prosperity should not directly force:
- permanent 1:1 simulation
- permanent high readability
- permanent narrative importance

## Promotion and demotion rules

Actors and households should be able to move between fidelity bands.

### Promotion to higher fidelity

A summarized actor or household should be eligible for promotion when they:
- marry into the player's orbit
- enter a debt or credit relationship with the player's network
- become feud-linked
- become institutionally significant
- trigger a legal or order problem
- become the face of a hotspot node
- get lifted by office, schooling, or patronage
- get forced into the player's path by migration, disaster, disorder, or trade failure

### Demotion to lower fidelity

A once-dense actor may be demoted when:
- they leave the player's reachable horizon
- they no longer carry active pressure
- they are no longer visible or readable
- no current command or consequence chain depends on them

Demotion must not erase:
- identity anchors
- key relationships
- lasting memory hooks
- re-promotion eligibility

### Heavy demotion into remote summary pools

Some actors should not simply fade into generic summary once they fall out of the core ring.

This is especially true for:
- defeated officials
- distant or displaced kin
- affines and marriage-linked households
- trusted friends, patrons, clients, or retainers
- disgraced brokers
- exiled branch actors
- displaced commanders
- outlaw figures pushed out of the local horizon

For these cases, the better model is:

**core-ring actor -> heavy demotion -> remote or exile pool with a live social-memory stub**

This means the actor may disappear from dense local visualization while still remaining:
- historically real
- geographically anchored
- socially remembered
- eligible to re-emerge later

Typical remote anchors may include:
- a distant prefectural queue
- a frontier service pool
- a Lingnan or Hainan-type exile pool when the active period pack supports that level of political geography
- a long-route clerical or military hardship posting

Heavy demotion should preserve at least:
- identity
- former role or office tier
- disgrace / shame / fear / obligation residue
- faction or patronage residue
- social-memory hooks
- geographic anchor
- possible hardening, frontier, hardship, or bitterness tags
- re-promotion and re-entry conditions

The important idea is:
- the actor is no longer dense
- the actor is not deleted
- the society may still remember them
- they may still remember the society
- kinship, friendship, patronage, and feud ties may all remain live enough to pull them back later

### Re-emergence from summary pools

A heavily demoted actor should be eligible to re-emerge when:
- faction balance changes
- amnesty or rehabilitation appears
- office demand or personnel shortage rises
- frontier service produces merit
- a patron, clan, or player-aligned network pulls them back
- a feud, investigation, or old obligation makes them relevant again

When they re-emerge, they should not return as a blank reset.

They may return carrying:
- old shame
- old resentment
- hardened political caution
- frontier toughness
- procedural ruthlessness
- dependency on new patrons

This helps the world feel like a society that remembers rather than a cast list that despawns and respawns.

## Time cadence by ring

Fidelity is not only about data density.
It is also about time density.

Use this default cadence split:

### Core ring
- may run at `xun` cadence
- supports short-band life strain, rumor, illness, debt, labor, and household reaction

### Local ring
- usually runs a mixed model:
  - xun-level summary movement
  - monthly consolidation
- only some named actors inside it need xun-level individual updates

### Regional ring
- usually runs at monthly or seasonal cadence
- should rarely need dense xun person simulation
- should instead leak shorter-term pressure through:
  - prices
  - route friction
  - notices
  - levy pressure
  - office temperature
  - security shifts

This keeps the time model aligned with the focus-ring model:
- nearby life breathes more often
- distant power still moves
- but it usually moves as pressure, access, and climate rather than as dense person traffic

## Talent emergence

Talent should not feel like blind card-spawn.

Use a weighted emergence model:

`talent_emergence = prosperity + education_resources + route_access + family_or_institution_pull + current_era_demand + disruption_pressure`

Good talent families:
- locally cultivated people
- migrants who arrive from elsewhere
- institution-lifted people
- disrupted people forced outward by debt, war, famine, or collapse

This lets the world create:
- stable local competence
- imported capability
- crisis-forged figures
- unexpected but explainable upward or sideways mobility

## Start-position interpretation

### Village or commoner-adjacent start

Expected shape:
- very dense core ring around household, neighbors, debt, labor, and marriage
- county institutions visible through notices, market effects, petty agents, and rumors
- prefecture, route, and court mostly pressure-shaped rather than person-shaped

The player should feel:
- exposed to larger power
- but not socially central to it

### Household or lineage-head start

Expected shape:
- larger core ring
- thicker named local ring
- more concrete marriage, branch, estate, and office intermediaries
- stronger direct visibility into county nodes and notable families

The player should feel:
- able to bend more local outcomes
- but still unable to command the world directly

### County-town or office-adjacent start

Expected shape:
- denser local ring around yamen, schools, merchants, and public nodes
- more concrete office and procedure actors
- household and village life still visible, but more often via petitions, prices, disorder, and notice flow

The player should feel:
- closer to institutions
- but still downstream of wider historical pressure

## Data-shape guidance

Authoritative state should usually live at several granularities:
- person
- household
- lineage
- settlement node
- institutional node
- regional pressure band

Do not force everything into person tables.

Prefer:
- agents where intention matters
- households where survival strategy matters
- nodes where spatial opportunity matters
- pressure bands where scale matters more than named actors

Projection can then turn this into:
- hall prompts
- desk-sandbox overlays
- notice trays
- route or market warnings
- hotspot summaries

## Module landing

Primary module ownership should normally look like this:

- `PersonRegistry` (Kernel layer)
  identity-only person anchors; fidelity ring assignment; promotion/demotion execution (see `PERSON_OWNERSHIP_RULES.md`)

- `PopulationAndHouseholds`
  household bands, labor pools, migration pools, survival pressure, health resilience, person activity

- `FamilyCore`
  clan-scoped kin agents, branch promotion, inheritance and support relevance, personality traits (does not hold global kinship or universal person state)

- `SocialMemoryAndRelations`
  promotion anchors, feud persistence, obligation carryover, reactivation hooks, dormant stubs for demoted persons

- `WorldSettlements`
  node heat, prosperity, route adjacency, local visibility signals

- `TradeAndIndustry`
  market thickness, work demand, transport pull, commercial opportunity

- `EducationAndExams`
  schooling opportunity, literacy pull, office-route emergence

- `OfficeAndCareer`
  local-to-regional institutional access, procedural openings, jurisdiction temperature

- `OrderAndBanditry`
  disorder pressure, shielding, suppression reach, unsafe-zone spillover

- `ConflictAndForce` and `WarfareCampaign`
  regional coercive pressure, levy burden, campaign fallout, force promotion triggers

- `NarrativeProjection`
  readable summaries, actor surfacing, escalation framing

- `Presentation.Unity`
  map density, hotspot reveal, route overlays, notice hierarchy

## Design checks

Use this checklist when introducing a new simulation layer or route:
- does it stay alive when the player does nothing?
- can it be represented without turning every person into a full agent?
- can it become denser when pressure hits?
- can it become lighter again without losing memory?
- does upper pressure still reach low-status starts?
- does prosperity change opportunity more than raw precision?
- can the player tell why this became visible now?

## One-line summary

Zongzu should not ask whether a place is `village play` or `prefectural play` first.

It should ask:

**who is under pressure, who is near the player, who must be readable now, and how thick the surrounding social pool should feel.**

## V213-V244 First Implementation

- `PopulationAndHouseholds` now owns the first rule-density layer for social mobility: monthly household pressure can drift livelihood, synchronize membership activity, and rebuild labor, marriage, and migration pools from existing state.
- `PersonRegistry` remains identity-only. It may execute `ChangeFidelityRing` against the existing `FidelityRing` field when a hot household needs local readback, but it does not gain household, livelihood, relation, office, memory, or capability state.
- Player-facing readback follows the intended scale rule: nearby/high-pressure people can become named local detail, while regional people remain pooled through `SettlementMobilitySnapshot` rather than being simulated one by one.
- Save/schema impact remains none: this pass reuses `PopulationAndHouseholds` schema `3` and `PersonRegistry` schema `1`.

## V245-V252 Closeout

- The v213-v244 branch is closed as first-layer fidelity substrate only. It proves the model can show near detail and far summary without simulating every person as an always-on agent.
- The closeout does not implement full demotion, dormant stubs, durable SocialMemory movement residue, class-mobility formulas, complete migration economics, or world-scale per-person simulation.
- Future fidelity work must name the owner module, expected cardinality, deterministic cap/order, schema impact, and validation lane before adding caches, ledgers, or new persisted state.
- UI and Unity may display projected fidelity/mobility readbacks, but must not infer precision, promote/demote people, parse prose, or calculate movement outcomes.

## V269-V276 Scale Budget Guard

- The stable rule is now explicit: **near detail, pressure-selected local detail, active-region pools, distant pressure summary**.
- Player household / close orbit can be named and person-dense because the player reads and acts there.
- Player influence footprint and active pressure can selectively promote detail, but only through owner-module rules and deterministic caps.
- Active chain regions should stay readable through `SettlementMobilitySnapshot`, labor/marriage/migration pools, and owner snapshots rather than permanent full populations.
- Distant society remains alive through pressure summaries, opportunity climate, route heat, legitimacy climate, and pool thickness. It must not become an all-world per-person monthly tick.
- Future personnel-flow work must record hot path, expected cardinality, deterministic order/cap, schema impact, and validation lane before deepening fidelity.

## V277-V284 Influence Readback

- The first additional readback fields are runtime projections only: `FidelityScaleSnapshot.InfluenceFootprintReadbackSummary`, `SettlementMobilitySnapshot.ScaleBudgetReadbackSummary`, and `PersonDossierSnapshot.InfluenceFootprintReadbackSummary`.
- These fields explain the scale budget already chosen by owner modules. They do not choose promotion/demotion, resolve migration, write SocialMemory residue, or authorize a player command.
- Great hall, desk, lineage, and person inspector surfaces may display the fields so the player can read why a person is close detail or why a settlement remains pooled.

## V285-V292 Boundary Closeout

- The v213-v284 branch is closed as a first-layer mobility/personnel-flow substrate, not as a complete society engine.
- The stable performance rule remains: close orbit can be named, active pressure can select bounded local detail, active regions should use structured pools, and distant society remains summarized pressure.
- Future deeper mobility work must decide whether it is a `PopulationAndHouseholds` rule, `PersonRegistry` fidelity command, `SocialMemoryAndRelations` durable residue, or a new planned owner before adding state.
- Any future command, dormant-stub model, migration economy, durable residue, or cross-region personnel flow must declare hot path, expected cardinality, deterministic cap/order, schema impact, no-touch boundary, and validation lane.
- UI and Unity may show the v277-v284 projected readbacks, but must not parse them, infer hidden movement targets, or suggest the whole world is ticking person-by-person.

## V293-V300 Personnel Command Preflight

- Future personnel-flow commands must be scale-budgeted before they exist. They are bounded influence intents, not direct movement edits.
- A future command may deepen a close household, kin channel, office service channel, or campaign manpower channel only through its owning module and deterministic caps.
- `PersonRegistry.ChangeFidelityRing` stays a readability/fidelity seam. It must not become assignment, migration, household membership, office service, or campaign manpower state.
- Application, UI, and Unity must not rank personnel targets, compute movement success, or parse readback prose to decide who moves.
- Any future persisted command receipt, assignment state, durable residue, or migration cache must first open a schema/migration plan.

## V301-V308 Personnel Flow Command Readiness

- `PersonnelFlowReadinessSummary` is the first command-surface readback over the existing near-detail / far-summary substrate.
- It appears only on existing `PopulationAndHouseholds` household-response affordances and receipts, where the target is already a concrete home household.
- The readback may explain near detail, distant pool summary, and why the command only affects livelihood/labor/migration pressure. It does not move a person, rank a person, promote fidelity, or write durable movement residue.
- Save/schema impact remains none.

## V309-V316 Personnel Flow Surface Echo

- `PlayerCommandSurfaceSnapshot.PersonnelFlowReadinessSummary` echoes the existing command readiness layer at the command-surface level.
- Great Hall mobility readback may show `人员流动命令预备汇总`, but the echo is still derived from projected command readiness fields, not from per-person scanning or movement simulation.
- It preserves the same fidelity rule: player-near household leverage is readable, while distant people remain represented through pools and ring budgets.
- It adds no new person iteration budget, movement ledger, or persisted cache.

## V317-V324 Personnel Flow Readiness Closeout

- The v293-v316 branch is closed as a first personnel-flow command-readiness layer, not as a full movement simulation.
- Fidelity remains scale-budgeted: player-near household readiness can be read in command surfaces, while distant movement remains pooled and summarized.
- The closeout adds no person iteration budget, direct personnel command, movement ledger, persisted cache, or schema change.

## V325-V332 Personnel Flow Owner-Lane Gate

- `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary` names which owner lane currently makes personnel-flow pressure readable.
- The current readable lane is `PopulationAndHouseholds` home-household response; other possible lanes remain future plans, not hidden high-fidelity simulation.
- It preserves the same scale budget: no new per-person scan, no movement resolver, no direct assignment, and no persisted cache.

## V333-V340 Personnel Flow Desk Gate Echo

- Desk Sandbox may echo the owner-lane gate only when local projected readiness commands/receipts exist for that settlement.
- This keeps local detail pressure-selected rather than global: the desk does not smear a global personnel-flow label across every settlement node.
- It adds no per-person scan, movement resolver, direct assignment, hidden target inference, or persisted cache.
