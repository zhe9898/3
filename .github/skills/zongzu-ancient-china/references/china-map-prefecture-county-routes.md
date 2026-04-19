# China Map, Prefecture, County, and Routes

Use this reference when the task is about Chinese ancient map structure, prefecture and county style hierarchy, roads, waterways, ferries, canals, passes, node labels, or topological map design.

## Core Rule

Do not treat the map as a modern nation-state road map.

For game design, build the map out of:
- administrative hierarchy
- settlement reach
- route topology
- crossings and chokepoints
- water transport logic

## Hierarchy Guidance

When the exact era is not fixed, prefer era-safe wording such as:
- prefecture-style seat
- county seat
- market town
- estate cluster
- village cluster

If the task is tied to a specific dynasty, then use the historically correct title set for that period and say the assumption.

## Good Node Types

- prefecture seats
- county seats
- market towns
- walled towns
- estate zones
- village clusters
- ferries
- bridges
- fords
- river ports
- canal junctions
- mountain passes
- granaries
- academies
- yamen nodes
- garrisons where needed

## Good Route Types

- main road
- local road
- mountain path
- river route
- canal route
- ferry link
- bridge crossing
- pass approach

## Why Water Matters

In Chinese ancient settings, waterways often carry:
- grain
- tax movement
- troop movement
- exam travel
- petitions and dispatches
- market integration

So do not build a road-only topology unless the region and period strongly justify it.

## Good Overlays

- tax reach
- grain movement
- exam access
- local order
- bandit risk
- flood exposure
- levy reach
- administrative delay

## Label Guidance

- keep player-facing labels concise and heavy
- distinguish seat, town, crossing, port, pass, and granary clearly
- avoid modern province and highway language
- if the exact Chinese title is uncertain, use a neutral English label plus a note in docs rather than bluffing

## Suggested Module Mapping

- `WorldSettlements`
- `PopulationAndHouseholds`
- `TradeAndIndustry`
- `ConflictAndForce`
- `Presentation.Unity`
