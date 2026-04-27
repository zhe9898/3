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
- mutable terrain and settlement state when disasters, waterworks, warfare, migration, or trade shifts change how the place functions

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

## Mutable Node States

Nodes should be able to change visible state when the simulation justifies it:
- thriving
- strained
- damaged
- abandoned
- recovering
- fortified
- militarized
- upgraded
- downgraded
- newly founded

Examples:
- a market town grows after route safety and trade volume improve
- a village cluster thins after flood, debt flight, disease, or war requisition
- a ferry loses importance after silting or bridge construction
- a granary or garrison appears because state reach or military pressure changed
- a county seat shows crowding, refugee camps, burned quarters, repair scaffolds, or public notices after crisis

## Good Route Types

- main road
- local road
- mountain path
- river route
- canal route
- ferry link
- bridge crossing
- pass approach

## Mutable Route States

Routes should expose functional state, not just decorative lines:
- open
- delayed
- unsafe
- blocked
- seasonal
- washed out
- silted
- repaired
- guarded
- toll-heavy
- refugee-heavy
- military-controlled

## Why Water Matters

In Chinese ancient settings, waterways often carry:
- grain
- tax movement
- troop movement
- exam travel
- petitions and dispatches
- market integration

So do not build a road-only topology unless the region and period strongly justify it.

Water and terrain should also be allowed to change the map:
- floodwater can cover fields, damage hamlets, break bridges, or force ferry reroutes
- drought can dry wells, lower river reliability, expose famine risk, and shrink market movement
- silt can slow canals, shift river ports, or make grain movement unreliable
- embankment repair can restore route confidence and public legitimacy
- war or bandit pressure can burn markets, close passes, militarize roads, and create refugee nodes
- migration and trade growth can create new market nodes or change the rank of existing settlements

## Good Overlays

- tax reach
- grain movement
- exam access
- local order
- bandit risk
- flood exposure
- levy reach
- administrative delay
- drought and water stress
- route damage and repair state
- settlement growth or abandonment
- refugee movement
- war damage and fortification

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
