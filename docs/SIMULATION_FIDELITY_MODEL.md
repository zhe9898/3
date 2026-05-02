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

## V349-V356 Personnel Flow Gate Closeout

- V325-V348 is closed as a first personnel-flow gate/readback/display/containment layer, not a high-fidelity movement system.
- Fidelity remains bounded: current local household response can be read, Desk Sandbox can localize the readback, and distant settlements remain pooled summaries.
- Future movement, migration, office-service, assignment, or campaign-manpower fidelity must open a separate owner-lane plan with cardinality, deterministic cap/order, and schema impact before implementation.

## V357-V364 Personnel Flow Future Owner-Lane Preflight

- Future personnel-flow lanes cannot raise fidelity simply because a gate exists. They need owner-module rules, target scope, and cardinality limits first.
- `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` personnel-flow work must declare whether it affects named close-orbit actors, local pooled households, active-region pools, or distant summaries before code lands.
- Any lane that promotes more people into detail must declare deterministic selection, cap, cadence, schema impact, and projection readback separately.

## V365-V372 Personnel Flow Future Lane Surface

- The future-lane preflight is now visible as a projected Great Hall mobility readback, but it still does not raise fidelity or open a movement lane.
- The surface says future Family/Office/Warfare personnel-flow work must choose owner module, accepted command, target scope, hot path, cardinality, deterministic cap/order, schema impact, and validation before implementation.
- Desk Sandbox remains local: the new preflight is not spread onto every settlement node and does not turn distant summaries into named personnel detail.

## V373-V380 Personnel Flow Future Lane Closeout

- V357-V372 is closed only as preflight visibility: future-lane requirements are documented and visible in Great Hall, but no fidelity band, movement lane, or target cardinality changes.

## V381-V388 Commoner Social Position Preflight

- Commoner / class-position mobility is a future owner-lane problem over existing pressure carriers, not a new global fidelity mode.
- Existing person dossier labels can explain household, education, trade, office, family, and memory context, but they do not promote/demote people or resolve class movement.
- Future status drift must state whether it affects named close-orbit actors, local pooled households, active-region pools, or distant summaries before implementation.
- Any future lane needs owner module, pressure carrier, hot path, expected cardinality, deterministic order/cap, schema impact, no-touch boundary, and validation before changing fidelity or state.

## V389-V396 Commoner Social Position Readback

- `SocialPositionReadbackSummary` makes near/person-inspector status labels explain which owner snapshots made them visible.
- The field may thicken a close-orbit named dossier, but it does not raise a fidelity ring, open a movement lane, change target cardinality, or promote/demote people.
- Distant commoner society still remains summarized by pools and pressure carriers until a later owner lane chooses state, cadence, cap/order, and schema impact.
- Distant society remains summarized; active settlements still need structured local readiness before desk-local gate text appears.
- Future owner-lane depth must still declare precision band, deterministic selection, cap, cadence, schema impact, and projection readback separately.

## V397-V404 Social Position Owner-Lane Keys

- `SocialPositionSourceModuleKeys` exposes structured provenance for the close-orbit social-position readback without raising fidelity or scanning the world person-by-person.
- The key list may help the lineage/person inspector show which owner lanes made a named dossier legible, but it does not select new named actors, promote/demote people, or open a movement lane.
- Distant commoner society still remains summarized by pools and pressure carriers. Future owner-lane depth must still declare precision band, deterministic selection, cap, cadence, schema impact, and projection readback separately.

## V405-V412 Social Position Readback Closeout

- V381-V404 is closed as a first close-orbit readback layer only: nearby dossiers can explain social-position provenance, but fidelity rings, target cardinality, and movement lanes do not change.
- The closeout does not promote distant commoners into named simulation. Distant society still remains summarized by household pools, settlement pressure carriers, and module-owned drift.
- Future owner-lane depth must still declare precision band, deterministic selection, cap, cadence, schema impact, and projection readback before adding commoner status rules.

## V413-V420 Social Position Scale Budget

- `SocialPositionScaleBudgetReadbackSummary` makes the fidelity budget visible on the person dossier itself: close/local dossiers can carry structured social-position detail; regional or distant society remains summary-first.
- The field reads the current `FidelityRing`; it does not change rings, promote new named actors, or add a hidden selection pass.
- Future owner-lane depth that raises fidelity or changes target cardinality must still declare deterministic selection, cap, cadence, schema impact, and projection readback before implementation.

## V421-V428 Regional Scale Guard

- Regional dossiers are explicitly guarded as far-summary readback. A registry-only `FidelityRing.Regional` person reports `regional summary` and registry-only source.
- This does not lower or raise fidelity; it proves the existing scale budget keeps distant society summarized by pools and pressure carriers.
- Future work that pulls regional people into local detail still needs owner pressure, deterministic selection, cap/order, cadence, schema impact, and projection readback.

## V429-V436 Social Position Scale Closeout

- V381-V428 is closed as a first social-position visibility layer, not as a fidelity or class authority layer.
- The closeout covers readable near-person status context, structured provenance, scale-budget explanation, and regional summary guard.
- Future work that changes social status, pulls more regional people into detail, or writes durable residue still needs owner pressure, deterministic selection, cap/order, cadence, schema impact, projection readback, and validation.

## V437-V444 Commoner Status Owner-Lane Preflight

- Future commoner status depth should begin from `PopulationAndHouseholds` household pressure and pool ownership, not from `PersonRegistry` or UI selection.
- Any future status rule that raises precision or names more people must state target scope, deterministic cap/order, cadence, schema impact, projection readback, and no-touch boundary first.
- This preflight changes no fidelity rings and adds no new selector.

## V445-V452 Fidelity Scale Budget Preflight

- "Near detail, far summary" is the performance and authority rule for future social/commoner/personnel depth.
- Player-near and pressure-hit actors may become readable only through owner-laned, deterministic, capped projection paths. Distant society remains pool/pressure summary until a later owner lane explicitly promotes bounded detail.
- Future work must declare target scope, hot path, touched counts, deterministic cap/order, cadence, schema impact, projection fields, and validation before changing fidelity, target cardinality, or selection behavior.
- This preflight changes no fidelity rings, adds no selector, and adds no global per-person simulation.

## V453-V460 Household Mobility Dynamics Explanation

- Existing household pressure signals can now explain which dimensions shape a local household's mobility posture, but they do not raise fidelity or select new people.
- The explanation preserves the current scale rule: close/local household pressure may be readable in detail; distant society still remains pooled by settlement and pressure carriers.
- Future work that changes status, promotes more people into detail, stores route history, or writes durable residue still needs owner pressure, deterministic selection, cap/order, cadence, schema impact, projection readback, and validation.

## V461-V468 Household Mobility Dynamics Closeout

- V461-V468 closes v453-v460 as a first household mobility explanation layer only.
- The closeout preserves the scale rule: near/local household pressure may be readable through structured dimensions, while distant society remains pooled summary until an owner-lane plan promotes more detail.
- This closeout changes no fidelity rings, adds no selector, adds no route history, and adds no global per-person simulation.
- Future work that changes status, promotes more people into detail, stores route history, moves households, or writes durable residue still needs owner pressure, deterministic selection, cap/order, cadence, schema impact, projection readback, and validation.

## V469-V476 Household Mobility Owner-Lane Preflight

- V469-V476 records the gate for future household mobility rule depth; it does not change fidelity rings, add a selector, store route history, move households, or write durable residue.
- `PopulationAndHouseholds` is the default first owner lane because household livelihood, activity, distress, debt, labor, grain, land, migration pressure, and pool carriers already live there.
- The scale rule remains near detail, far summary. Player-near and pressure-hit households can later receive richer owner-laned rules; distant society remains pooled settlement pressure until explicitly promoted.
- Future work must declare target scope, hot path, touched counts, deterministic cap/order, cadence, schema impact, projection fields, validation, and no-touch boundary before changing runtime detail or target cardinality.

## V485-V492 Household Mobility Preflight Closeout

- V485-V492 closes v469-v476 as preflight governance only; it does not change fidelity rings, add a selector, store route history, move households, or write durable residue.
- The closeout preserves near detail, far summary: future household mobility rules may thicken player-near or pressure-hit households only after one owner lane declares target scope, touched counts, deterministic cap/order, cadence, schema impact, projection fields, validation, and no-touch boundary.

## V501-V508 Household Mobility First Runtime Rule And Rules-Data Readiness

- V501-V508 adds no fidelity-ring mutation, target selector, route-history state, movement rule, or durable residue. It records the scale shape that a later first runtime rule must follow.
- The future target scope is player-near households, pressure-hit local households, active-region pools, and distant summaries. Distant society must remain pooled unless a separate plan promotes bounded detail.
- Future monthly fanout must declare maximum households, pools, and settlements touched per pass, sort deterministically before cap, and fall back to summary pressure when over cap.
- The hardcoded extraction map identifies thresholds, weights, caps, recovery/decay rules, deterministic ordering, regional/era assumptions, and pool limits that should move into owner-consumed rules-data over time without becoming a runtime plugin surface.
- Distant society remains summarized by pools and settlement pressure until a later owner-laned rule explicitly promotes bounded detail.

## V509-V516 Household Mobility Rules-Data Contract And Validator Preflight

- V509-V516 adds no fidelity-ring mutation, target selector, route-history state, movement rule, rules-data loader, validator implementation, or durable residue.
- The future contract keeps near detail, far summary by requiring deterministic fanout caps and tie-break priorities before any owner rule can touch households, pools, or settlements.
- Rules-data may later tune threshold bands, pressure weights, regional/era modifiers, recovery/decay rates, fanout caps, and deterministic tie-break priorities, but only after validation and owner consumption inside `PopulationAndHouseholds`.
- Application, UI, and Unity may not read rules-data to promote distant summaries, infer hidden households, calculate movement, or raise fidelity.
- Because the repo has no reusable runtime rules-data/content/config pattern today, this pass is docs/tests-only contract preflight.

## V517-V524 Household Mobility Default Rules-Data Skeleton

- V517-V524 adds no fidelity-ring mutation, target selector, route-history state, movement rule, default rules-data file, loader, validator implementation, or durable residue.
- The future skeleton keeps scale explicit through ordered fanout caps and tie-break priorities, but it remains contract text only in this pass.
- `ruleSetId`, `schemaVersion`, `ownerModule`, `defaultFallbackPolicy`, `parameterGroups`, and `validationResult` are future data-only skeleton fields, not save state or UI authority.
- Application, UI, and Unity may not read a skeleton to promote distant summaries, infer hidden households, calculate movement, or raise fidelity.
- Because no reusable runtime rules-data/content/config pattern exists, this pass does not create `content/rules-data`.

## V525-V532 PopulationAndHouseholds First Hardcoded Rule Extraction

- V525-V532 extracts a near-detail fanout cap already used by `PopulationAndHouseholds`; it does not add a new selector, fidelity-ring sweep, route-history state, movement rule, loader, or durable residue.
- Default focused member promotion remains capped at two regional members per pressure-hit household, ordered by household id and person id.
- The far-summary rule is unchanged: quiet households, off-scope settlements, and distant pooled society do not become hidden detailed targets.
- Application, UI, Unity, prose, and `PersonRegistry` do not consume the extracted rule data to raise detail or calculate movement.

## V533-V540 PopulationAndHouseholds First Household Mobility Runtime Rule

- V533-V540 keeps the first runtime rule in the near-detail band: one active settlement pool and two pressure-hit households by default.
- The far-summary rule is unchanged: quiet households, lower-priority active pools, off-scope settlements, and distant pooled society are not expanded into hidden household movement targets.
- The rule writes only existing `PopulationAndHouseholds` fields and existing pool summaries. It adds no route-history state, movement ledger, cooldown ledger, selector watermark, target-cardinality state, class/status state, or `PersonRegistry` detail expansion.
- Replay fidelity remains deterministic because selection is ordered by pool pressure / settlement id and household score / household id, with no random choice or prose parsing.

## V541-V548 Household Mobility First Runtime Rule Closeout

- V541-V548 is a fidelity closeout, not a new fidelity promotion rule.
- It preserves the V533-V540 budget: one active pool and two pressure-hit households by default, with distant society staying summarized.
- No new detail ring, selector watermark, route-history state, cooldown ledger, target-cardinality state, `PersonRegistry` expansion, or UI-owned eligibility logic is introduced.
- Any later expansion from pressure nudge to movement, projection, or persisted route history must name owner, cadence, fanout, schema impact, and no-touch proof in a new plan.

## V549-V556 Household Mobility Runtime Rule Health Evidence

- V549-V556 is a fidelity health-evidence pass, not a detail-ring promotion, movement rule, or fanout expansion.
- It keeps the V533-V540 budget unchanged: one active pool and two pressure-hit households by default, with deterministic cap/order and distant society remaining summarized.
- The next gate before widening requires touched household/pool/settlement counts, no-touch proof, same-seed replay evidence, pressure-band interpretation, and hot-path/cardinality notes.
- No new selector watermark, route-history state, cooldown ledger, target-cardinality state, `PersonRegistry` expansion, long-run saturation tuning, performance optimization claim, or UI-owned eligibility logic is introduced.

## V557-V564 Household Mobility Runtime Widening Gate

- V557-V564 is a fidelity widening gate, not a fidelity widening implementation.
- The near-detail budget stays unchanged: one active pool and two pressure-hit households by default, with distant society summarized.
- Future fanout expansion must name current/proposed touched households, pools, and settlements per month, then prove deterministic cap/order and no-touch behavior before implementation.
- Future recovery/decay changes must first classify pressure-band meaning; future performance work must first name the hot path and cardinality. This pass adds no selector watermark, route-history state, cooldown ledger, target-cardinality state, touch-count state, diagnostic state, performance cache, `PersonRegistry` expansion, long-run saturation tuning, or UI-owned eligibility logic.

## V565-V572 Household Mobility Runtime Touch-Count Proof

- V565-V572 is fidelity proof, not fidelity widening.
- The current near-detail budget is now covered by focused owner-test evidence: one selected active pool and two eligible households in the fixture receive the existing monthly pressure nudge.
- The proof uses a zero-risk-delta baseline to count touched households without adding persisted counters, diagnostic state, performance cache, projection fields, or new rule authority.
- The lower-priority selected-pool candidate, quiet household, lower-priority active pool, off-scope settlements, distant pooled society, `PersonRegistry`, Application, UI, and Unity remain no-touch. Future widening still needs a separate ExecPlan before changing fanout, formulas, schema, or presentation.

## V573-V580 Household Mobility Rules-Data Fallback Matrix

- V573-V580 is fallback proof, not fidelity widening.
- Malformed active-pool threshold, settlement cap, household cap, and risk delta values fall back to defaults, preserving the existing near-detail budget.
- The owner-result proof requires malformed runtime rules-data to produce the same monthly run signature as default rules-data.
- This adds no runtime loader, rules-data file, persisted counters, diagnostic state, performance cache, projection fields, plugin surface, or new rule authority.
## V581-V588 Household Mobility Runtime Threshold No-Touch Proof

V581-V588 keeps household mobility fidelity bounded by proving an active-pool threshold can leave the first runtime rule inert for below-threshold pools. This reinforces near detail / far summary discipline: only eligible active pools may be considered, while below-threshold pools and their households remain untouched.

No runtime behavior, schema, cache, diagnostic state, or performance claim is added.
## V589-V596 Household Mobility Runtime Zero-Cap No-Touch Proof

V589-V596 reinforces household mobility fidelity budget limits by proving zero fanout caps leave the first runtime rule inert. This keeps near detail bounded by explicit caps and prevents quiet/off-scope households from being promoted into rule work through accidental fanout.

No runtime behavior, schema, cache, diagnostic state, or performance claim is added.
## V597-V604 Household Mobility Runtime Zero-Risk-Delta No-Touch Proof

V597-V604 reinforces household mobility fidelity budget limits by proving zero risk delta leaves the first runtime rule inert. This keeps pressure mutation bounded by owner-consumed parameters and prevents accidental household touches when risk increment is disabled.

No runtime behavior, schema, cache, diagnostic state, or performance claim is added.
## V605-V612 Household Mobility Runtime Candidate Filter No-Touch Proof

V605-V612 reinforces household mobility fidelity budget limits by proving candidate filters keep already-migrating/high-risk and below-floor households out of the first runtime rule. This keeps near detail bounded by eligibility gates rather than expanding into universal household scanning.

No runtime behavior, schema, cache, diagnostic state, or performance claim is added.
## V613-V620 Household Mobility Runtime Tie-Break No-Touch Proof

V613-V620 reinforces household mobility fidelity budget limits by proving equal-score runtime candidates resolve through stable household-id ordering before the cap is applied. This keeps near-detail promotion bounded and deterministic while the tied higher household id remains summary/no-touch.

No runtime behavior, schema, cache, diagnostic state, ordering retune, score retune, or performance claim is added.
## V621-V628 Household Mobility Runtime Pool Tie-Break No-Touch Proof

V621-V628 reinforces household mobility fidelity budget limits by proving equal-outflow active pools resolve through stable settlement-id ordering before the cap is applied. This keeps near-detail pool promotion bounded and deterministic while the tied higher settlement id remains summary/no-touch.

No runtime behavior, schema, cache, diagnostic state, pool ordering retune, threshold retune, or performance claim is added.
## V629-V636 Household Mobility Runtime Score-Ordering No-Touch Proof

V629-V636 reinforces household mobility fidelity budget limits by proving candidate score ordering selects the highest-scored pressure household before household-id tie-break applies. This keeps near-detail promotion bounded by existing pressure score while lower-score candidates remain no-touch under cap one.

No runtime behavior, schema, cache, diagnostic state, score formula retune, candidate ordering retune, or performance claim is added.

## V637-V644 Household Mobility Runtime Pool-Priority No-Touch Proof

V637-V644 reinforces household mobility fidelity budget limits by proving active-pool priority is applied before cross-pool household score comparison. This keeps near-detail promotion bounded by selected active pools while a higher-scoring household in a lower-priority pool remains summary/no-touch under settlement cap one.

No runtime behavior, schema, cache, diagnostic state, pool ordering retune, score formula retune, candidate ordering retune, threshold retune, or performance claim is added.

## V645-V652 Household Mobility Runtime Per-Pool Cap No-Touch Proof

V645-V652 reinforces household mobility fidelity budget limits by proving household cap application is per selected active pool, not a global cross-pool target selector. This keeps near-detail promotion bounded within each selected pool while lower-score households remain no-touch under per-pool cap one.

No runtime behavior, schema, cache, diagnostic state, cap semantics retune, global household cap, pool ordering retune, score formula retune, candidate ordering retune, threshold retune, or performance claim is added.

## V653-V660 Household Mobility Runtime Threshold Event No-Touch Proof

V653-V660 reinforces household mobility fidelity budget limits by proving threshold events are downstream of selected near-detail household touches only. A selected household that crosses the existing migration-started threshold may emit the existing structured event, while unselected/off-cap households remain summary/no-touch and emit no threshold event.

No runtime behavior, schema, cache, diagnostic state, new event type, event routing change, fanout widening, threshold retune, cap semantics retune, or performance claim is added.

## V661-V668 Household Mobility Runtime Event Metadata No-Prose Proof

V661-V668 reinforces household mobility fidelity budget limits by proving selected threshold-event readback can stay structured. Cause, settlement id, and household id come from metadata on the existing event, while prose stays a downstream explanation that does not promote distant summaries or unselected households into rule work.

No runtime behavior, schema, cache, diagnostic state, new event type, event routing change, prose parser, fanout widening, threshold retune, cap semantics retune, or performance claim is added.

## V669-V676 Household Mobility Runtime Event Metadata Replay Proof

V669-V676 reinforces household mobility fidelity budget limits by proving selected threshold-event metadata is replay-stable under the existing deterministic owner fixture. Replay comparison remains test evidence only and does not promote event signatures into saved state or presentation-side selectors.

No runtime behavior, schema, cache, diagnostic state, new event type, event routing change, replay ledger, fanout widening, threshold retune, cap semantics retune, or performance claim is added.

## V677-V684 Household Mobility Runtime Threshold Extraction

V677-V684 keeps household mobility fidelity bounded while extracting the first runtime rule's selected-household `MigrationStarted` event threshold into owner-consumed rules-data. Default threshold 80 preserves the prior near-detail behavior; malformed threshold values fall back to default instead of silently widening or suppressing runtime evidence.

No persisted schema, loader, rules-data file, cache, diagnostic state, fanout widening, candidate filter retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V685-V692 Household Mobility Runtime Candidate Floor Extraction

V685-V692 keeps household mobility fidelity bounded while extracting the first runtime rule's candidate migration-risk floor into owner-consumed rules-data. Default floor 55 preserves the prior near-detail eligibility behavior; malformed floor values fall back to default instead of silently widening runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, fanout widening, high-risk filter retune, general migration-state retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V693-V700 Household Mobility Runtime Score Weight Extraction

V693-V700 keeps household mobility fidelity bounded while extracting the first runtime rule's migration-risk score weight into owner-consumed rules-data. Default weight 4 preserves the prior candidate ordering behavior; malformed weight values fall back to default instead of silently widening or reordering runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, score formula retune, fanout widening, filter retune, threshold retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V701-V708 Household Mobility Runtime Labor Floor Extraction

V701-V708 keeps household mobility fidelity bounded while extracting the first runtime rule's labor-capacity pressure floor into owner-consumed rules-data. Default floor 60 preserves the prior candidate ordering behavior; malformed floor values fall back to default instead of silently widening or reordering runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, labor model retune, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V709-V716 Household Mobility Runtime Grain Floor Extraction

V709-V716 keeps household mobility fidelity bounded while extracting the first runtime rule's grain-store pressure floor into owner-consumed rules-data. Default floor 25 preserves the prior candidate ordering behavior; malformed floor values fall back to default instead of silently widening or reordering runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, grain economy retune, grain pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V717-V724 Household Mobility Runtime Land Floor Extraction

V717-V724 keeps household mobility fidelity bounded while extracting the first runtime rule's land-holding pressure floor into owner-consumed rules-data. Default floor 20 preserves the prior candidate ordering behavior; malformed floor values fall back to default instead of silently widening or reordering runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, land economy retune, land pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V725-V732 Household Mobility Runtime Grain Divisor Extraction

V725-V732 keeps household mobility fidelity bounded while extracting the first runtime rule's grain-store pressure divisor into owner-consumed rules-data. Default divisor 2 preserves the prior candidate ordering behavior; malformed divisor values fall back to default instead of silently widening or reordering runtime work, and divisor 0 cannot reach the score path.

No persisted schema, loader, rules-data file, cache, diagnostic state, grain economy retune, grain floor retune, land pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V733-V740 Household Mobility Runtime Land Divisor Extraction

V733-V740 keeps household mobility fidelity bounded while extracting the first runtime rule's land-holding pressure divisor into owner-consumed rules-data. Default divisor 2 preserves the prior candidate ordering behavior; malformed divisor values fall back to default instead of silently widening or reordering runtime work, and divisor 0 cannot reach the score path.

No persisted schema, loader, rules-data file, cache, diagnostic state, land economy retune, land floor retune, grain pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V741-V748 Household Mobility Runtime Candidate Ceiling Extraction

V741-V748 keeps household mobility fidelity bounded while extracting the first runtime rule's high-risk candidate ceiling into owner-consumed rules-data. Default ceiling 80 preserves prior no-touch behavior for households already at the high-risk edge; malformed ceiling values fall back to default instead of silently widening or reordering runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, migration-started event threshold retune, candidate floor retune, trigger threshold extraction, score formula retune beyond literal extraction, fanout widening, filter expansion, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V749-V756 Household Mobility Runtime Distress Trigger Extraction

V749-V756 keeps household mobility fidelity bounded while extracting the first runtime rule's distress trigger threshold into owner-consumed rules-data. Default threshold 60 preserves prior no-touch behavior for households below the distress trigger when no other trigger qualifies them; malformed values fall back to default instead of silently widening runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, distress economy retune, debt/labor/grain/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, filter expansion, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V757-V764 Household Mobility Runtime Debt Trigger Extraction

V757-V764 keeps household mobility fidelity bounded while extracting the first runtime rule's debt-pressure trigger threshold into owner-consumed rules-data. Default threshold 60 preserves prior no-touch behavior for households below the debt trigger when no other trigger qualifies them; malformed values fall back to default instead of silently widening runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, debt economy retune, distress/labor/grain/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, filter expansion, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V765-V772 Household Mobility Runtime Labor Trigger Extraction

V765-V772 keeps household mobility fidelity bounded while extracting the first runtime rule's labor-capacity trigger ceiling into owner-consumed rules-data. Default ceiling 45 preserves prior no-touch behavior for households at the labor trigger boundary when no other trigger qualifies them; malformed values fall back to default instead of silently widening runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, labor model retune, debt/distress/grain/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, filter expansion, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V773-V780 Household Mobility Runtime Grain Trigger Extraction

V773-V780 keeps household mobility fidelity bounded while extracting the first runtime rule's grain-store trigger floor into owner-consumed rules-data. Default floor 25 preserves prior no-touch behavior for households at the grain trigger boundary when no other trigger qualifies them; malformed values fall back to default instead of silently widening runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, grain economy retune, labor/debt/distress/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, filter expansion, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.

## V781-V788 Household Mobility Runtime Land Trigger Extraction

V781-V788 keeps household mobility fidelity bounded while extracting the first runtime rule's land-holding trigger floor into owner-consumed rules-data. Default floor 15 preserves prior no-touch behavior for households at the land trigger boundary when no other trigger qualifies them; malformed values fall back to default instead of silently widening runtime work.

No persisted schema, loader, rules-data file, cache, diagnostic state, land economy retune, grain/labor/debt/distress/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, filter expansion, second runtime rule, movement authority, route-history model, migration economy, class/status engine, or performance claim is added.
