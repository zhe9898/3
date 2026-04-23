# MAP_AND_SANDBOX_DIRECTION

This document fixes the direction of Zongzu's map stack so the project does not drift into a flat strategy map, a decorative poster, or a detached overworld minigame.

**Important:** Do not confuse "no detached map game" with "no big map." Large maps are valid and important when they project real module state, route pressure, public legitimacy, imperial reach, conflict heat, or historical momentum. The map stack is a sand-table pressure instrument at every scale, from county desk to realm overview.

For modern game-engineering standards (Unity, performance, observability), see `MODERN_GAME_ENGINEERING_STANDARDS.md`.
For the spatial skeleton backend spec, see `SPATIAL_SKELETON_SPEC.md`.

## Core thesis

The map system is **sandboxes at multiple scales**, not a normal game map plus one sandbox.

Zongzu should read as:
- a **realm / world sand table** (天下图) for imperial reach, frontier posture, dynasty-cycle strain, and historical-trend pressure
- a **macro / regional sand table** for route, prefecture, and regional pressure
- a **county desk sand table** for lived local pressure
- **conflict surfaces** for aftermath and escalation

Scale switching is part of play, not cinematic spectacle. Scale switches answer "how wide is this pressure." Overlay switches answer "which social chain is carrying it."

It should not read as:
- a modern province-and-highway atlas
- a free-roam world map
- a flat watercolor poster with icons
- a tactical battlefield first

## Scale stack

### 1. Realm / world sandbox (天下图)

Purpose:
- read dynasty-scale pressure: imperial legitimacy, frontier posture, historical trends, succession uncertainty
- understand how court pressure, fiscal demand, and military burden reach the player's region
- decide which region or frontier corridor deserves closer review

Primary scale:
- dynasty realm
- frontier belts (辽 / 西夏边界)
- major river systems and canal trunk lines
- circuit / route-level administrative bands

What belongs here:
- imperial edict reach and delay bands
- frontier garrison posture and campaign spillover
- dynasty-cycle legitimacy markers (mandate confidence, succession uncertainty)
- major grain-route corridors
- historical-trend pressure fronts (reform, rebellion, disaster waves)
- region-entry pins (路 / 道)

What does not belong here:
- county-level detail
- household or lineage genealogy
- street-level town dressing

**Spatial receipts for dynasty-cycle play:**
- `MandateConfidence` drop → grain-route markers show supply anxiety, office seals show fracture cracks
- `SuccessionUncertainty` → frontier garrison nodes show readiness pins, courier routes show message-delay bands
- `AmnestyWave` → prison/penal nodes change state, route checkpoints soften

### 2. Macro sandbox

Purpose:
- read the wider regional situation
- understand what pressure is moving toward the player's county slice
- decide which county or route corridor deserves closer review

Primary scale:
- `lu` / route-level
- prefecture-style seats
- county seats as entry pins rather than full local boards
- major rivers, canals, ferries, passes, and arterial roads

What belongs here:
- major waterways
- trunk roads
- prefecture and county bands
- grain and tax movement
- petition and dispatch routes
- military spillover
- flood, bandit, and market heat bands
- county-entry pins

What does not belong here:
- household-level clutter
- branch-level genealogy detail
- street-by-street town dressing
- tactics HUD

### 3. County desk sandbox

Purpose:
- show lived society
- reveal what households, lineages, offices, markets, ferries, schools, and routes are under pressure
- host the player's regular review and bounded intervention

Primary scale:
- one county or tightly bounded local slice
- county seat
- market towns
- ferry and gate nodes
- estate and village clusters

What belongs here:
- county yamen
- markets
- academies
- temples
- ferries
- estate clusters
- local conflict traces
- public-life signals
- route risk at local resolution

### 4. Conflict vignette and later campaign board

Purpose:
- visualize local damage, consequence, and escalation
- avoid turning every conflict into a board or tactics screen

Primary rule:
- local clashes stay local first
- campaign-lite only appears when pressure has clearly exceeded county-scale handling

## Historical hierarchy guidance

For the current Northern Song-inspired baseline:

- macro sandbox grammar should prefer `路 -> 州/府 -> 县`
- county sandbox grammar should prefer `县 -> 镇/市 -> 村/庄/渡口/路口`

Player-facing labels should remain era-safe and concise.
Do not force exact title fetishism when a neutral label is clearer.

## Visual direction

### The base is not pure ink painting

The macro sandbox should be a **realistic 2.5D sand table** first:
- carved or modeled relief
- wood-desk base
- low-rise terrain masses
- physically placed route bands and node markers

### Ink belongs to the information layer

Use ink for:
- route strokes
- boundary notation
- callout circles
- dispatch marks
- region labels
- directional annotations

Do not let ink wash become the whole world surface.

### Brocade belongs to the trim layer

Use brocade or silk only for:
- edge trim
- title plaques
- high-rank frame accents
- imperial or formal overlays

Do not use brocade as the entire map substrate.

### Desired feel

The macro board should feel like:
- a desk object
- a route-pressure instrument
- a readable war-and-governance overview

It should not feel like:
- wallpaper
- a loading-screen illustration
- a museum print pasted under icons

## Read order

The player should be able to read the macro sandbox in this order:

1. which region band is hottest
2. which route or waterway is transmitting pressure
3. which prefecture or county is implicated
4. what kind of pressure is moving there
5. where the player can drill down to the county desk sandbox

## Recommended node grammar

Macro sandbox node families:
- route capital or regional anchor
- prefecture seat
- county seat entry pin
- river port
- canal junction
- ferry crossing
- mountain pass
- depot or granary
- military corridor marker

County desk sandbox node families:
- hall
- yamen
- market town
- academy
- temple
- village cluster
- estate cluster
- wharf
- bridge
- gate

## Mutable terrain and disaster state

The sand table is not static. When the world changes, the table should visibly change:

**Water and flood states:**
- `FloodRisk` ≥ 70 → farmland nodes show water tint; embankment nodes show breach marks
- `CanalWindow = Closed` → water routes turn white (icing in winter, silt in dry season)
- `EmbankmentStrain` ≥ 80 → embankment nodes show crack tokens; repair tokens appear if corvée is active

**Damage and destruction states:**
- `RouteConstraintEmerged` (bandit/raid) → route band shows scorch marks; nearby village nodes show damage debris
- Local conflict (scale 2) → affected node shows broken-building token; casualty tallies appear on conflict vignette
- Market disruption → market-town node dims; trade-route bands thin

**Growth and upgrade states:**
- Market town prosperity rising → node flag grows taller; market bands widen
- Road repaired → route band returns to normal color; repair token removed
- New settlement established → new node marker placed; connected by new route thread
- Settlement abandoned → node marker grayed and lowered; route thread frayed

**Node status changes:**
- Village → Abandoned hamlet (population collapse, disaster, or war damage)
- Market town → Walled town (prosperity + security investment)
- Ferry → Blocked crossing (flood, ice, or conflict)
- Granary → Depleted / Restocked (grain movement)

All terrain changes must come from module-owned state or read models. Do not fake permanent geographic change as flavor text if it affects routes, settlement reach, trade, tax, military movement, disease, or migration.

## Recommended overlay grammar

Macro overlays:
- grain flow
- petition delay
- market heat
- flood exposure
- bandit corridor risk
- military spillover
- prefecture pressure
- **reform pressure** (historical-process overlay)
- **legitimacy** (public-confidence overlay)

County overlays:
- lineage influence
- labor burden
- office pressure
- route safety
- public attention
- conflict heat
- subsistence strain
- **tax and paperwork** (administrative overlay)
- **public rumor** (information overlay)

Realm overlays:
- imperial reach
- frontier posture
- dynasty-cycle strain
- historical-trend front

**Overlay switching UI:** Overlays are selected through physical objects on the desk edge:
- A tray of **map-lens tokens** (one per overlay)
- The player places the desired lens token onto the sandbox surface
- Only one overlay active at a time per sandbox
- Overlay state is preserved when switching scales (cross-scale traceability)

## MVP implementation rule

The MVP must support **at least two scales**: macro sandbox (regional) and county desk sandbox (local). A bounded transition between them is acceptable for MVP.

Scale switching is a **core operational mechanic**, not optional polish. The player should be able to:
- see a pressure on the macro board
- touch the affected county entry pin
- arrive at the county desk sandbox with the same pressure chain still highlighted

**Cross-scale traceability rule:** When the player switches from macro to county (or back), the same `PressureKind` + `SettlementId` / `RouteId` chain must remain visually active. The pressure does not become a different unrelated screen.

For MVP:
- macro sandbox = one stable top-level board
- county entry pins = clear and touchable
- county drill-down = one bounded transition that preserves selection
- county desk sandbox = the main work surface

Post-MVP scales (realm / world sandbox, frontier board, campaign board) may be added additively without replacing the two-scale MVP foundation.

## Module mapping

- `WorldSettlements`: macro and county node topology
- `TradeAndIndustry`: route and market pressure
- `OfficeAndCareer`: petition and administrative delay overlays
- `OrderAndBanditry`: disorder and protection overlays
- `ConflictAndForce`: spillover and escalation markers
- `Presentation.Unity`: macro board, county board, transitions, labels

## Unity shell first-pass guidance

Inside the Unity shell:
- `MacroSandbox` should be a first-class surface
- it should own route bands, county entry pins, waterway emphasis, and regional pressure markers
- it should not yet own authoritative simulation logic
- it may read a dedicated read-model snapshot such as `macro-sandbox-snapshot.json`

## Taskful map surfaces

Every map scale must answer "why open this now?" A taskful map exposes at least one of:
- Current pressure objective (what the player must respond to)
- Visible route cost (travel time, grain expense, labor burden)
- Information reach (what the player can actually know)
- Travel/message delay (how stale is this report?)
- Risk band (where is danger concentrated?)
- Modifier/event (what seasonal or disaster condition is active?)
- Next drill-down locus (where to look closer?)

An overmap whose only job is choosing a decorative tile is not a Zongzu map.

## Fog and uncertainty

The player's view is bounded by influence footprint:
- **Fog of distance**: beyond information-network range, nodes show only rumor-grade summaries
- **Stale reports**: distant regions display last-known state with age tint
- **Partial overlays**: without clerk access, administrative data shows rough bands only
- **Rumor distortion**: public-life signals may contradict; player must judge reliability

A physical **"you are here / your reach ends here"** marker sits at the player's current locus. Commands resolve through influence footprint, route access, office access, public visibility, and message delay—not through omniscient map clicking.

## One-line rule

**The player does not open a map. The player studies the world as a sand table at different scales.**
