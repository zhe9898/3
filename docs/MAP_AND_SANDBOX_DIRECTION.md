# MAP_AND_SANDBOX_DIRECTION

This document fixes the direction of Zongzu's map stack so the project does not drift into a flat strategy map, a decorative poster, or a detached overworld minigame.

## Core thesis

The map system is **sandboxes at multiple scales**, not a normal game map plus one sandbox.

Zongzu should read as:
- a macro sand table for route, prefecture, and regional pressure
- a county sand table for lived local pressure
- conflict surfaces for aftermath and escalation

It should not read as:
- a modern province-and-highway atlas
- a free-roam world map
- a flat watercolor poster with icons
- a tactical battlefield first

## Scale stack

### 1. Macro sandbox

Purpose:
- read the wider situation
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

### 2. County desk sandbox

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

### 3. Conflict vignette and later campaign board

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

## Recommended overlay grammar

Macro overlays:
- grain flow
- petition delay
- market heat
- flood exposure
- bandit corridor risk
- military spillover
- prefecture pressure

County overlays:
- lineage influence
- labor burden
- office pressure
- route safety
- public attention
- conflict heat
- subsistence strain

## MVP implementation rule

Do not spend MVP effort on expensive scale-switch spectacle.

Instead:
- make the macro sandbox one stable top-level board
- make county entry pins clear
- let county drill-down happen through one bounded transition
- keep the county desk sandbox as the main work surface

The project should become playable before it becomes cinematically seamless.

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

## One-line rule

**The player does not open a map. The player studies the world as a sand table at different scales.**
