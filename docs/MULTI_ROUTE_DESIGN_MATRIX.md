# MULTI_ROUTE_DESIGN_MATRIX

This document formalizes Zongzu's multi-route society design.

Terminology note: "route" in this document means an architectural / design pathway for social pressure and module growth. It must not become a player-facing route system, job tree, or class picker. In code and presentation, prefer concrete projections such as household social pressure, social drift, influence footprint, module presence, or command affordance.

It exists to prevent the project from collapsing into:
- one family-management lane with decorative side systems
- a small set of rigid class trees
- route labels that do not change pressure, verbs, surfaces, or failure states

Read this together with:
- `RULES_DRIVEN_LIVING_WORLD.md`
- `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
- `SOCIAL_STRATA_AND_PATHWAYS.md`
- `PLAYER_SCOPE.md`
- `SIMULATION.md`
- `EXTENSIBILITY_MODEL.md`
- `ENGINEERING_RULES.md`
- `VERSION_ALIGNMENT.md`

## Core rule

These routes are not separate campaigns and not fixed jobs.
They are overlapping social positions inside one living world.

A person, household, branch, or lineage may:
- occupy more than one route at once
- drift from one route into another
- fall downward rather than rise upward
- use one route to support another

## Player-facing translation

Do not surface this as a "route system."
The player should read:
- which social chain is moving
- what pressure pushed it
- which influence circles can touch it
- what cost or backlash may carry into next month

The intended monthly chain is:
- world pressure moves first
- modules produce structured diffs and state changes
- projections show household, market, study, lineage, yamen, public-life, disorder, military, and imperial/court pressure as readable social chains
- player commands enter only through available influence circles
- delayed consequences return as credit loss, public-face change, obligations, official attention, lineage resentment, household drift, or disorder pressure

This keeps multi-route design as a living-world grammar rather than a class tree.
It also lets historical figures and great trends enter as pressure that people can carry, resist, or distort, not as a separate scripted route.

## Route matrix

## 1. Lineage management

Fantasy:
- preserve, grow, divide, and maneuver a lineage across marriage, inheritance, dependents, estates, and prestige

Typical actors:
- main-line head
- branch elder
- widow with household authority
- lineage steward
- dependent kin

Core verbs:
- allocate
- arrange
- adopt
- discipline
- sponsor
- reconcile or split

Main pressures:
- branch conflict
- inheritance ambiguity
- ritual burden
- lineage prestige
- dependent support load
- internal resentment

Main modules:
- `FamilyCore`
- `SocialMemoryAndRelations`
- `NarrativeProjection`

Primary surfaces:
- `great hall`
- `ancestral hall`
- family council or branch receipts

Typical transitions:
- lineage management -> official career through sponsorship
- lineage management -> commercial management through estate and shop control
- lineage management -> private force or feud pressure through retainers and branch conflict

## 2. Commoner household management

Fantasy:
- keep an ordinary household alive, solvent, mobile, and socially protected

Typical actors:
- tenant family
- smallholder
- laboring household
- widow-led household
- debt-pressed family

Core verbs:
- apportion labor
- borrow
- migrate
- petition
- trade small
- endure

Main pressures:
- subsistence risk
- rent and corvee
- debt
- illness and labor loss
- marriage burden
- lack of protection

Main modules:
- `PopulationAndHouseholds`
- `WorldSettlements`
- `SocialMemoryAndRelations`
- `NarrativeProjection`

Primary surfaces:
- `desk sandbox`
- household summaries
- letters, pleas, and background notices

Typical transitions:
- commoner household -> exam route through rare family investment
- commoner household -> commercial route through peddling, brokerage, workshop entry, or transport work
- commoner household -> bandit/shadow route through debt collapse, flight, or disorder

## 3. Commercial management

Fantasy:
- turn movement, credit, goods, brokerage, labor, and route risk into durable influence

Typical actors:
- shop household
- workshop master
- broker
- boatman or carrier family
- grain or cloth intermediary

Core verbs:
- buy
- sell
- broker
- ship
- stockpile
- hedge

Main pressures:
- route breakage
- price shock
- escort cost
- warehouse exposure
- labor shortage
- official squeeze

Main modules:
- `TradeAndIndustry`
- `WorldSettlements`
- `PopulationAndHouseholds`
- `NarrativeProjection`

Primary surfaces:
- ledgers
- route overlays
- market nodes
- workshop or wharf summaries

Typical transitions:
- commercial route -> lineage power through wealth and marriage leverage
- commercial route -> office route through patronage, recommendation, or document competence
- commercial route -> bandit pressure through route predation or smuggling dependency

## 4. Official career and yamen service

Fantasy:
- move from study, attachment, and recommendation into office while surviving backlog, patronage, and political exposure

Typical actors:
- exam aspirant
- academy-backed youth
- clerk
- runner-adjacent service actor
- formal office-holder

Core verbs:
- study
- clerk
- petition
- recommend
- adjudicate
- rise or cling on

Main pressures:
- access bottlenecks
- patron dependence
- docket pressure
- office risk
- family pull
- reputation exposure
- clerk dependence
- memorial attack risk
- court agenda pressure

Main modules:
- `EducationAndExams`
- `OfficeAndCareer`
- `SocialMemoryAndRelations`
- `NarrativeProjection`

Primary surfaces:
- `great hall`
- office-facing reports
- school or yamen nodes
- petition or appointment summaries

Typical transitions:
- official route -> social governance through local order and public works
- official route -> imperial route through rank and appointment reach
- official route -> household stress through distance, posting risk, faction loss, or dismissal
- official route -> court-facing pressure through memorials, appointment slates, faction backing, or censor attack

Official route depth should split four things that are often collapsed:
- **credential**: degree, recommendation, document literacy, or yamen attachment
- **post**: whether the actor actually holds an office or service position
- **reach**: whether clerks, runners, patrons, and local elites let the office act
- **exposure**: whether memorials, faction labels, family obligations, and evaluation pressure make action dangerous

This route should produce living officials, not disembodied offices.

## 5. Social governance

Fantasy:
- manage order, petitions, repair, public works, and legitimacy below the dynasty level

Typical actors:
- magistrate-facing household
- local notable
- mediator
- repair sponsor
- petition broker

Core verbs:
- arbitrate
- dispatch
- repair
- pacify
- sponsor
- balance factions

Main pressures:
- petition backlog
- route safety
- tax extraction
- public confidence
- elite/commoner tension
- disorder spillover

Main modules:
- `OfficeAndCareer`
- `OrderAndBanditry`
- `WorldSettlements`
- `NarrativeProjection`

Primary surfaces:
- `great hall`
- county or settlement reports
- notice trays
- route and repair signals

Typical transitions:
- social governance -> official route through office entrenchment
- social governance -> imperial route when local authority is scaled upward
- social governance -> bandit pressure when order breaks and coercion replaces legitimacy

## 6. Bandit or shadow-power management

Fantasy:
- survive or dominate through raiding, extraction, shelter, intimidation, and coalition growth outside regular protection

Typical actors:
- outlaw household
- shadow broker
- raiding group
- protection extractor
- proto-rebel leader

Core verbs:
- raid
- extract
- shelter
- recruit
- intimidate
- proclaim

Main pressures:
- suppression risk
- loyalty fracture
- grain shortage
- exposure on routes
- legitimacy gap
- revenge cycles

Main modules:
- `OrderAndBanditry`
- `ConflictAndForce`
- `SocialMemoryAndRelations`
- `NarrativeProjection`

Primary surfaces:
- `desk sandbox`
- hotspot summaries
- conflict vignettes
- rumor and fear overlays

Typical transitions:
- bandit route -> rebellion or polity formation
- bandit route -> lineage feud or local protection politics
- bandit route -> suppression, exile, or negotiated reintegration

## 7. Imperial or macro-governance pressure

Fantasy:
- hold a realm together through legitimacy, appointments, examination policy, extraction, grain security, military burden, ritual confidence, reform debate, and succession continuity

Typical actors:
- throne-centered authority
- court-aligned office holders
- provincial command actors
- dynasty-maintenance institutions
- remonstrance and policy officials
- degree-holders outside office

Core verbs:
- appoint
- remit
- mobilize
- legitimize
- integrate
- stabilize

Main pressures:
- dynastic fatigue
- court capture
- fiscal overreach
- regional fracture
- succession crisis
- military overstretch
- examination and appointment bottlenecks
- edict-to-county implementation drag
- reform faction dispute
- ritual legitimacy loss or repair

Main modules:
- `OfficeAndCareer`
- `WarfareCampaign`
- `PublicLifeAndRumor`
- `WorldSettlements`
- world-layer summaries
- `NarrativeProjection`

Primary surfaces:
- court or macro reports
- campaign-lite board
- high-order notices
- legitimacy or succession summaries
- exam / appointment pressure summaries
- county notice and yamen report overlays

Typical transitions:
- imperial pressure -> local governance burden
- imperial pressure -> office opportunity or collapse
- imperial pressure -> rebellion, regional militarization, or taxation shock
- imperial pressure -> household strain through tax, labor service, relief, or military supply
- imperial pressure -> public-life legitimacy wave through edicts, amnesties, ritual confidence, or reform rumors

Renzong-era opening rule:
- imperial authority is credible and morally legible, but it reaches the player through bureaucratic, ritual, military, fiscal, and public channels
- Qingli-style reform pressure may appear as court debate, examination/administrative pressure, and local implementation strain
- Wang Anshi / Shenzong-era New Policies and baojia-style household organization are later chronological material and must not be treated as default opening institutions

Sub-lanes inside imperial / macro pressure:
- **court process lane**: memorial queue, audience or council attention, censor pressure, appointment slate, policy window, court-time disruption
- **official alignment lane**: office-holders, clerks, patrons, local elites, and faction labels deciding whether to carry, delay, reinterpret, or sabotage court pressure
- **regime authority lane**: recognition, appointment reach, tax reach, grain-route reach, ritual claim, public belief, force backing, office defection
- **dynasty-cycle lane**: dynastic fatigue, succession clarity, restoration momentum, usurpation risk, repair after crisis
- **historical-carrier lane**: named figures, reform cohorts, conservative opposition, frontier commanders, and local implementers carrying great trends into ordinary society

These sub-lanes are not new player classes.
They are route lenses that decide which pressure chain is moving and which influence circles can touch it.

Renzong-era calibration:
- the opening should emphasize credible imperial legitimacy, scholar-official visibility, exam prestige, fiscal / military strain, and Qingli-era reform pressure as start conditions, not as a locked timeline
- it should avoid treating later Wang Anshi / Shenzong institutions as already active default structures
- it should let court debate and named officials matter as pressure carriers, while local households still experience the result through taxes, labor service, education cost, yamen documents, prices, relief, and rumor
- after the opening, rule resolution may bend, delay, intensify, localize, or derail historical tendencies; historical plausibility comes from pressure chains and institution constraints, not from forced event dates

## Route interaction rules

- no route should stay sealed from the others
- route identity is often temporary, partial, and conditional
- one household may split across several routes at once
- upward mobility is not the only motion; fallback, drift, compromise, and damage control are equally important
- different routes should change what the player reads first, what verbs matter, and what failure looks like

## Living-people check

If a route description can be mistaken for a class-select screen, it is too abstract.

Each route should also answer:
- how do people eat
- what do they fear losing
- who can shame or protect them
- what pushes them to move
- what everyday surface shows their pressure

## MVP emphasis

MVP should bias toward:
- `lineage management`
- `commoner household management`
- `commercial management`
- light `official career`

Conditional MVP-plus may include:
- light `bandit or shadow-power`
- light `social governance`

Full `imperial or macro-governance pressure` should mostly remain downstream and atmospheric until later packs deepen it.

## Engineering dimensions for multi-route design

Multi-route design is not complete if it only sounds good on paper.
For this repo, route design must also survive:
- additive expansion
- plugability across packs and route-depth layers
- save migration
- performance limits
- explainability requirements
- deterministic testing
- system-level coherence
- structural quality across modules
- coupling discipline at integration seams
- implementation and code quality
- data ownership discipline
- feature-pack degradation
- authoring and tuning throughput
- shell readability and cognitive load
- balance and stability across decades

## Extensibility

- route families are design lenses, not module keys by default
- do not hard-bake route families into one global class tree or one giant actor-state blob
- a route should usually emerge from owned module state plus projection logic rather than from a single monolithic `CurrentRoute` field
- add a new route by extending the correct owning modules, queries, events, projections, and feature-pack manifests
- create a new module only when the route introduces a genuinely separate state family, lifecycle, and pack boundary
- one actor or household must be able to express several route pressures at once without schema hacks

## Plugability and pack-local attach-detach

- route depth should be attachable in layers rather than welded permanently into the base game
- a route or route-deepening pack should be able to:
  - add projections
  - add commands
  - add notices
  - add tuning
  - add deeper causal resolution
  without requiring base systems to pretend the full version is always present
- plugability requires:
  - manifest-gated activation
  - stable public seams
  - fallback behaviors when the pack is absent
  - no hidden hard dependency from base code into optional route logic
- route additions should degrade to:
  - absent
  - lite
  - full
  in a predictable way
- if removing a pack leaves dead fields, broken projections, orphaned commands, or invalid saves, the route was not plugable enough
- route docs should state which parts are:
  - base-world compatible
  - optional enrichments
  - late-game or post-MVP deepenings

## Migration and save compatibility

- multi-route expansion must preserve old saves
- route labels shown in UI should be recomputable from authoritative state whenever possible
- avoid persisting brittle, single-value route tags when the same actor may occupy several positions at once
- if a new route deepening needs new authoritative state, bump the owning module schema version rather than inventing side blobs
- route-aware projections must degrade gracefully when an upstream pack is absent in older saves
- feature-manifest entries must remain the source of truth for whether route-related systems are active, lite, or absent
- current first living-society slice uses `HouseholdSocialPressureSnapshot` and `PlayerInfluenceFootprintSnapshot` as presentation read models only; they recompute from existing module queries, add no save fields, and must degrade to absent/watch-only when optional packs are not enabled

## Data ownership and schema shape

- every route-facing concept must answer which module owns the authoritative state
- route families should usually be projections over:
  - household condition
  - lineage structure
  - institution attachment
  - settlement pressure
  - conflict exposure
- do not duplicate the same social fact in three places just to make route summaries easy to render
- keep route projection queries explicit so the same actor can read as:
  - household-strained
  - exam-aspiring
  - commerce-dependent
  - feud-exposed
  at the same time without contradictory writes
- prefer stable owned fields such as debt, posting, branch status, tenancy, escort exposure, petition backlog, and route safety over vague umbrella flags
- schema additions should favor small owned facts that compose into route pressure instead of giant route-state enums

## System-level coherence

- multi-route design must still feel like one game, one world, and one simulation grammar
- route families should share:
  - the same monthly time model
  - the same pressure-first causality logic
  - the same ownership discipline
  - the same Query / Command / DomainEvent integration model
  - the same shell projection philosophy
- do not let each route invent its own mini-framework, hidden scheduler, private terminology, or bespoke exception ladder
- route deepening should enrich the common living-world framework rather than fork away from it
- system coherence review should ask:
  - does this route use the same nouns as the rest of the repo
  - does it resolve through the same monthly and diff-based structure
  - does it project into shell surfaces the same way other routes do
  - does it preserve one shared causality model instead of adding a disconnected minigame logic
- if two routes solve the same kind of pressure with different hidden rules, the design is no longer systemic enough
- if a route requires its own worldview, UI grammar, and simulation cadence, it probably belongs to a later pack or a narrower subsystem instead of the core social field

## Structural quality and modular shape

- route design must reinforce the repo's modular monolith, not blur it
- a good route addition should make owned state, public queries, commands, and emitted events more legible, not less
- structure quality should be reviewed for:
  - cohesive module ownership
  - small and readable type boundaries
  - clear projection layers
  - explicit command and event flow
  - lack of duplicate route logic scattered across unrelated files
- if one route requires edits in every module just to remain coherent, the design is too structurally expensive
- if route logic cannot be located by following module boundaries and public seams, the architecture is drifting toward hidden coupling
- route docs should prefer a few stable cross-module seams over ad hoc helper tunnels

## Coupling and integration hygiene

- route families are expected to connect many modules, but they should do so through disciplined seams only
- cross-module integration should remain limited to:
  - query
  - command
  - domain event
- do not let route implementation justify:
  - foreign state mutation
  - UI-side authority writes
  - narrative-side gameplay mutation
  - module A reaching into module B internals "because this route touches both"
- coupling review should ask:
  - is this dependency directional and explainable
  - could this be a projection instead of a backdoor write
  - is the event or query seam stable enough for save evolution
  - will this route make future packs harder to separate
- route depth should increase semantic linkage, not code entanglement
- when in doubt, prefer one more explicit domain event over one more hidden service dependency

## Feature-pack alignment and graceful degradation

- a route family may be:
  - absent
  - lite
  - full
  depending on pack activation and implementation maturity
- route design must state what still works when one adjacent pack is lite or missing
- lineage, household, and market pressure should remain coherent even if higher-order governance or conflict packs are still thin
- if a route depends on warfare, law, education, religion, or shadow-power systems, define:
  - minimum fallback behavior
  - upgraded behavior when the pack is active
  - what UI surface should stay hidden until the pack reaches usable depth
- do not let route docs promise a surface or command that cannot survive partial-pack installs
- version alignment and manifest gating must stay more authoritative than design prose

## Performance and simulation scale

- do not simulate every actor at full route detail every month
- core actors get full monthly route pressure resolution
- regional actors get summarized route pressure and only escalate when they become locally important
- distant actors should resolve through abstracted household, institution, market, and world summaries
- route transitions should be pressure-driven and diff-driven, not full graph rescoring of the whole world every tick
- projection should show a rich multi-route society without requiring every hidden background actor to run the same fidelity as the player's kin

## Implementation and code quality

- route implementation should keep authority code boring, explicit, and testable
- prefer cohesive files, strong value objects, and explicit side effects over convenience blobs and ambient state
- do not bury route rules in:
  - UI presenters
  - config loaders
  - loose utility classes
  - one-off patch scripts
- code quality review should ask:
  - where does the rule live
  - is its mutation path obvious
  - can a new engineer trace cause and effect without spelunking
  - are names aligned with design language and module ownership
  - is there primitive obsession where a richer type should exist
- avoid giant route-manager classes that centralize behavior which should stay owned by modules
- avoid hidden mutable singletons, temporal side channels, or convenience caches that can desync route projections from authoritative state
- route code should optimize for maintainability under years of content growth, not only for the first vertical slice

## Authoring and tuning throughput

- multi-route design must be tunable without constant code surgery
- route pressure should come from bounded coefficients, thresholds, weights, and selectors that can be adjusted per pack or region
- authoring should support:
  - default route pressure bands
  - region-specific biases
  - era-pack overrides
  - institution preference overrides
  - household or lineage trait modifiers
- route additions should not require handcrafted narrative content before they become legible in simulation
- content teams should be able to deepen a route by adding:
  - projections
  - labels
  - thresholds
  - manifests
  - copy templates
  without reopening every module seam
- if a route only works when a designer hand-authors dozens of bespoke exceptions, it is not structurally healthy enough yet

## Shell readability and cognitive load

- every route family needs a first-readable surface
- the player should be able to tell:
  - what social position is under pressure
  - what changed this month
  - whether intervention is even possible
  - what likely moves next
- route visibility must stay distributed across:
  - `great hall`
  - `desk sandbox`
  - notices
  - chronicle
  - conflict vignette
  - campaign-lite
  rather than collapsing into one giant summary board
- route richness should not turn the shell into a poster, dashboard wall, or prose dump
- if a new route cannot be explained in two or three shell surfaces with clear verbs and readable diffs, it is probably too abstract or too early

## Balance and long-horizon stability

- route design must hold up across decades, not just one satisfying month
- upward mobility, collapse, drift, entrenchment, and recovery should all remain possible
- no route should dominate purely because its verbs are easiest to optimize
- elite routes should not erase commoner pressure
- commoner routes should not become pure misery simulators with no leverage
- bandit or shadow routes should be dangerous and consequential without becoming a shortcut difficulty exploit
- official and imperial routes should amplify exposure and responsibility, not just prestige
- balance review should ask:
  - what makes this route attractive
  - what makes it costly
  - what causes exit or collapse
  - what keeps it connected to other routes instead of becoming an isolated minigame

## Determinism, replay, and causal audit

- route-heavy worlds create more cross-pressure and therefore more opportunities for hidden nondeterminism
- any route transition influenced by multiple modules should still replay the same way under the same seed and same month inputs
- the game should support causal audit for major route shifts:
  - before-state
  - pressure inputs
  - chosen actions or missed actions
  - resolved result
  - downstream route consequences
- chronicle and debug traces should be able to agree on why a branch became commerce-heavy, why a household slipped toward outlaw risk, or why an office line lost momentum
- if designers cannot replay and audit route drift, balancing the living world will become guesswork

## Explainability and observability

- major route shifts must expose why they happened
- when someone slides from one route family into another, the game should be able to answer:
  - what pressure pushed them
  - what support or protection failed
  - what opportunity opened
  - what becomes different next month
- notices, chronicle, and route-facing UI should report route movement as consequence, not as unexplained retagging
- debug and developer-facing surfaces should expose route pressure summaries, transition hotspots, and major cause traces

## Content production and maintenance cost

- route depth must scale with the team's actual throughput
- route families should become interesting first through simulation structure, not through massive bespoke event writing
- narrative, UI copy, and visual dressing should deepen already-legible routes instead of compensating for missing systemic shape
- every route should have a small-core version that is worth shipping before its deluxe version exists
- maintenance burden should be reviewed across:
  - system logic
  - copy and labels
  - save migration
  - test coverage
  - shell surfaces
  - pack compatibility
- if a route creates constant one-off exceptions in all of these layers, it may need to be narrowed, merged, or postponed

## Testability

- route movement must remain deterministic under replay
- any new authoritative route state requires save roundtrip coverage
- any new cross-module route interaction requires integration tests for Query / Command / DomainEvent seams
- route projections should have acceptance coverage proving that the same underlying world state yields stable route-facing summaries
- fallback, collapse, drift, and sideways movement deserve tests, not only upward-mobility cases

## Codex implementation checklist

When Codex adds or deepens a route, it should be able to answer all of these:

- what social fantasy does this route add
- what people or households actually occupy it
- what pressures feed it
- which module or modules own the authoritative state
- what can be derived in projection instead of persisted
- how it stays inside the shared monthly, pressure-first, living-world system instead of inventing a parallel game
- what structure this adds and whether it improves or weakens current module shape
- what cross-module seams it uses and whether they stay within Query / Command / DomainEvent discipline
- what new schema or migration cost exists, if any
- what simulation tier it belongs to
- what surfaces will show it first
- what failure state, fallback state, or sideways drift state it creates
- what pack or manifest state gates it
- whether the route can be attached or removed cleanly at absent / lite / full pack depth
- what authoring knobs tune it
- how players will read it without a wall of text
- what balance risk or exploit it introduces
- how it degrades when adjacent systems are lite
- what code-quality risk it introduces, such as god classes, duplicated rule logic, or hidden side effects
- what tests prove it does not break determinism, saves, or pack boundaries
