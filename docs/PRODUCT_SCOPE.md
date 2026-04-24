# PRODUCT_SCOPE

This document defines the project through **14 formal dimensions** plus **3 cross-dimensional hard principles**.

## Core thesis
The player is not a god and not a single adventurer.
The project is a **Northern Song-inspired, multi-route, rules-driven simulation of a living Chinese ancient society**.
The player enters that society through a **home-household seat**: the continuing play perspective of one household / branch / doorway of obligation.
That seat is not the same as one person and is not the whole clan.
The current household spokesperson may change, people may die or leave, and social identities may drift, but play remains anchored in what this household can see, remember, afford, request, endure, or risk.
In Chinese design shorthand: 玩家持续经营的是本户的家计、人口、名声、债务、亲缘与位置；人物是感受和执行这些压力的入口，而不是玩家本体。
The seat is not permanently elite: a house may stabilize, rise, fragment, sink into commoner survival, or fall into gray dependence under pressure.

For the higher-level rules-driven living-world thesis, player leverage doctrine, and multi-generation consequence framing, see `RULES_DRIVEN_LIVING_WORLD.md`.
For how named historical figures, reforms, wars, and great trends enter as pressure rather than rails, see `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`.
For modern game-engineering standards, see `MODERN_GAME_ENGINEERING_STANDARDS.md`.

## Living society directive
One-line version: Zongzu is a **Northern Song-inspired living society simulation with a Renzong-era opening**, not a career tree, not a default lineage-only game, not a god game, and not a fixed historical rail.

The world must breathe through social layers before the player acts:
- households
- lineages
- markets
- education and exams
- yamen / document contact
- local governance
- military and border pressure
- temples and public life
- gray disorder
- imperial and court pressure

These layers are the world itself, not player classes.
A person or household does not "choose a route"; it is pushed by livelihood, debt, illness, grain price, study cost, marriage and funeral burden, tax and labor duty, market opportunity, public order, and protection ties.

The player acts only through the influence circle currently available:
- lineage prestige
- favors and obligation
- office authority
- wealth and credit
- coercive capacity
- public face
- information reach

The player may touch part of the living society through those levers, but may not become the whole society.
Lineage is a strong influence node, not the default identity of the player and not the boundary of the game.
The player's available power may expand outward from the household into clan, locality, office, force, or court-facing pressure, but it must always pass through concrete relationship chains rather than becoming free god-view control.

Imperial and court pressure is a real layer, but it is distant at MVP scale.
For a Northern Song Renzong-era opening, it should appear as initial pressure and institution tone:
- reign legitimacy, court tone, edicts, amnesties, and ritual authority
- appointment and dismissal pressure flowing into officials and yamen work
- examination policy, quota pressure, and literati factional debate
- tax, labor-service, granary, and relief expectations reaching households through local agents
- military budget, Western Xia / frontier pressure, and campaign aftermath reaching local society

Do not turn this into direct emperor play at the start.
It should first arrive through `OfficeAndCareer`, `WarfareCampaign`, `WorldSettlements`, `PublicLifeAndRumor`, and notice projections.
Also do not import later Wang Anshi / Shenzong-era structures as if they already govern the Renzong opening; later reforms may become pressure, rumor, future pack depth, or counterfactual branches only when rules earn them.

Historical figures and processes are allowed to bend the world.
They should enter as great-trend pressure, named-person potential, institutional windows, policy packages, local implementation, backlash, and memory.
The player may be swept up by those trends and, when their influence circle is strong enough, may also help carry, localize, accelerate, delay, distort, or resist them.
That is not free timeline editing; it is bounded leverage applied to historical pressure.
Long-run play may allow the player to change history at larger scale, including regional rebellion, polity formation, succession struggle, usurpation, restoration, or dynasty repair, when later packs provide the necessary office, force, legitimacy, logistics, court, and memory systems.
The opening must not hand the player emperor control, but the full product must leave room for history to be altered through earned rule chains.

## Historical grounding baseline
Default player-facing grounding should read as `Northern Song-inspired`, not as a dynasty-agnostic mashup and not as a strict documentary reenactment.

**Default player stratum:** The MVP opening positions the player as a **middling landed lineage (zhuhu, roughly 三等户 to 二等户)** in a single county. This is high enough to matter in local society—tax obligations, corvée liability, marriage networks, academy access, yamen contact—but not so high that the player begins with direct office authority or imperial reach. A poorer or richer start may be supported later through scenario selection, but the default must be this middle stratum so that upward drift and downward pressure are both meaningfully felt.

**Default region:** A **north-China road-county** (旱路县) or **Jiangnan water-network county** (江南水网县). The current seed world uses Jiangnan (兰溪) for its richer route topology, but the social-pressure grammar should read correctly for either. Regional differentiation (north-China dry road vs. Jiangnan canal vs. southwest mountain vs. border garrison) is a post-MVP depth pack.

That means the current baseline should lean toward:
- county society shaped by literati ambition, yamen paperwork, examination candidacy queues, and informal patronage ties (not a formal recommendation system; the Northern Song civil exam had largely abolished the Tang-era public-recommendation 公荐 practice by the early dynasty, though social connections and local reputation still matter)
- north-China road-county logic plus connected market and canal corridors where needed, rather than a flat all-China terrain voice
- family, office, trade, route security, and campaign pressure as one linked field

This baseline is a gameplay frame, not a claim that every mechanic is an exact Northern Song reconstruction.
The baseline should support historical process without locking the game into a dead year-by-year script.
Major people and events may appear, but their gameplay form must remain pressure-chain first: upstream causes, actor carriers, institutional openings, local consequences, and downstream residue.

The world:
- changes before the player acts
- produces pressure through people, households, institutions, markets, order, weather, banditry, office, and war
- is then projected back to the player through space, visitors, reports, ledgers, household and lineage surfaces, desk-sandbox nodes, and conflict aftermath

## Core loop
Authoritative loop:
1. open a new monthly shell
2. run the world's internal day-level authority steps, skipping or batching quiet days when safe
3. consolidate month-end module state and structured diffs
4. build projections and notifications
5. let the player review pressure and opportunity
6. let the player issue bounded commands
7. carry those choices into the next month

This is not a flat once-per-month jump.
The month is the player's main review shell; lived pressure below it may move by day before review closes.
`xun` / early-mid-late month language may remain as an almanac label, UI grouping, or projection summary, but it is not the preferred bottom-level authority grid.

Monthly readability rule:
- the world runs first: household livelihood, marriage, birth, death, market movement, study progress, debt, public order, and border pressure
- modules emit structured changes: a household debt spike, a student leaving study, a shop labor shortage, a lineage relief gap, an unsafe road
- presentation translates those changes into visible social chains, not random event cards
- the player intervenes only through available reach: lend grain, guarantee debt, fund study, mediate, write a petition, escort a route, recommend someone, seek office help, or deliberately endure
- the next month carries the cost: credit spent, public face changed, obligations created, office attention raised, lineage resentment deepened

Playable loop lock:
- the current docs and architecture can support a complete world, but implementation is not playable until a real intervention loop is closed
- a playable slice must show pressure in the home-household shell, expose concrete leverage, accept a bounded command, resolve through module-owned rules, emit a receipt or refusal, leave memory/residue, and make the next month read differently
- read models alone are not a gameplay loop; they become play only when the player can judge, commit, and later understand consequence

## The 14 dimensions

### 1. Core gameplay and scope control
The irreplaceable loop is:
- review
- interpret
- intervene selectively
- accept consequences
- continue the lineage

Anything that does not strengthen this loop is expendable.

### 2. Engine and tools
The engine exists to host:
- the spatial shell
- the inspectors
- the desk sandbox
- the data-driven UI
- the asset pipeline

The simulation remains engine-agnostic.

### 3. Programming architecture
The project uses a **modular monolith**:
- small kernel
- feature modules
- deterministic scheduler
- persistence layer
- spatial presentation shell

This is required for extensibility and anti-coupling.

### 4. Art and audio assets
Low-to-moderate cost art direction is mandatory.
The game should rely on:
- room-state changes
- object-anchored UI
- stylized portrait modules
- short vignettes
- ambient sound layers
- detailed sourcing, license, and staging rules live in `ART_AND_AUDIO_ASSET_SOURCING.md`

### 5. Version control
Mainline must remain recoverable.
No zip-based backup workflow is acceptable.

### 6. Technical debt management
Debt must be treated as planned work.
No long-lived hack may silently become architecture.

### 7. Project organization
Repository and content organization must reflect module boundaries.

### 8. Data-driven content and configuration
Balance, authored templates, and system weights should live in validated config rather than hardcoded constants.

### 9. Testability and determinism
The world must be reproducible.
The same seed and inputs must yield the same outputs for authoritative systems.

### 10. UI and information architecture
The UI is not a thin wrapper.
It is a core expression of the game fantasy:
- great hall / study
- ancestral hall / lineage scroll
- desk sandbox
- conflict vignette
- inspectors and ledgers

### 11. Performance and simulation scale control
Simulation scale is achieved by tiered fidelity and tiered cadence, not by simulating everything at full detail or at one flat speed.

### 12. Saves and compatibility
Long-run saves are valuable.
Schema versioning and migration discipline are mandatory from the start.

### 13. Debugging and observability
The simulation must be inspectable.
The game may not become a black box.

### 14. Project management, milestone locks, and cutting discipline
Every release line needs:
- explicit done criteria
- explicit non-goals
- explicit cut rules

## Social and pathway structure
These are not separate game modes.
They are interconnected social positions and pathways:
- **Household / lineage position**: the player's nearest decision surface, but not the whole subject of the game
- **Commoners / households**: the social base and labor layer
- **Exams**: institutional upward mobility
- **Trade**: wealth and network mobility
- **Office**: formal authority and political leverage
- **Outlaw / banditry**: disorder, failure, coercion, and gray power
- **Imperial / court pressure**: distant legitimacy, appointments, fiscal-military pressure, reform debate, and ritual-political tone
- **Rebellion / regime-scale transformation**: later pressure chain from protection failure, armed autonomy, rebel governance, succession fracture, or court capture into polity formation, usurpation, restoration, or dynasty repair

These positions must be able to transform into one another through world pressure and personal circumstance.
That includes downward mobility:
- a once-solid house can become an ordinary struggling household
- a branch can lose status, labor security, and marriage value
- a respectable line can slide into debt, dependence, migration, hired service, or gray survival
- a low or poor start may remain precarious, stabilize, or climb

See `RULES_DRIVEN_LIVING_WORLD.md` for the stronger multi-route doctrine: these are overlapping social positions in one dynamic field, not hard career branches.

## Three cross-dimensional hard principles
1. **Explainable causality**
   - major outcomes must have readable cause traces

2. **Narrative and simulation separation**
   - text is never the authoritative state driver

3. **MVP and later versions stay structurally aligned**
   - MVP is not disposable prototype code
   - later versions extend the same kernel, module system, commands, events, IDs, and save rules

## Product anti-patterns
Do not turn the project into:
- a raw spreadsheet game
- a pure text parser
- a card battler
- an open-world walking simulator
- a detached tactics/RTS game
- a giant event-pool narrative machine
