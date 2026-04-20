# Agrarian Calendar, Waterworks, and Hazards

Use this reference when the task is about farming rhythms, planting and harvest windows, irrigation, wells, canals, embankments, drainage, drought, flood, or labor peaks tied to the agricultural year.

## Core Rule

Do not model land as a static income tile.

Agrarian life should pulse through:
- labor peaks
- water control
- repair duty
- weather anxiety
- harvest timing
- route condition

## What To Distinguish

- sowing, transplanting, tending, and harvest windows
- irrigated fields versus rain-fed fields
- routine maintenance versus emergency embankment repair
- local water shortage versus region-wide drought
- seasonal passability on roads, ferries, and canals
- agricultural slack time versus labor pinch

## Good Zongzu Abstractions

- `season_labor_pinch`
- `water_control_confidence`
- `embankment_strain`
- `harvest_window`
- `repair_levy`
- `weather_anxiety`
- `route_mud_drag`

## Good Public and Map Signals

- canals, ditches, sluices, wells, and embankments
- flood marks and breach alerts
- threshing yards, storage courts, and milling points
- dredging crews, repair notices, and labor summons
- dry fields, waterlogged fields, and delayed crossings

## Design Guidance

- Agrarian rhythm should shape when households can spare labor, when officials can demand service, and when markets tighten or loosen.
- Water control is political as well as practical; people remember who repaired, delayed, stole, or failed.
- Flood and drought should alter migration, theft, petitioning, marriage timing, and bandit vulnerability, not only crop numbers.
- Regional packs should change how waterworks feel: canals and dikes in Jiangnan, wells and dry-road strain in north-China counties, gorge and slope control in mountain zones.

## Suggested Module Mapping

- `WorldSettlements`
- `PopulationAndHouseholds`
- `TradeAndIndustry`
- `OrderAndBanditry`
- `NarrativeProjection`
