# Jiangnan Water-Network County

Use this reference when the task needs a distinctly Chinese regional feel grounded in river links, canals, ferries, embankments, market towns, and dock-centered local life.

## Core Rule

Do not flatten the map into a dry-land road grid.

In a water-network county, power and survival move through:
- canals and river links
- ferries and crossings
- dock markets and warehouse nodes
- embankments and flood control
- boat labor and transport brokerage
- town strings tied by water traffic

## Good Zongzu Abstractions

- `water_route_reach`
- `ferry_choke_pressure`
- `dock_trade_density`
- `embankment_stress`
- `boat_labor_dependence`
- `flood_anxiety`
- `canal_notice_flow`
- `bridge_market_visibility`

## Design Guidance

- Water routes should change travel time, information speed, grain movement, and escape paths.
- A ferry, lock, or crossing can matter as much as a road gate.
- Markets, temples, inns, and yamen-facing space near the water should feel crowded with commerce, rumor, and surveillance.
- Flood and embankment risk should shape legitimacy and household anxiety, not only harvest output.

## Public-Life Surfaces

- ferry queues and quarrels
- bridge markets
- quay gossip and porter traffic
- temple fairs near canal or river edges
- grain-barge visibility
- embankment panic during bad weather

## Use Cases

- desk-sandbox node planning
- county-town and market topology
- transport and message-delay tuning
- river-bandit, escort, and levy movement pressure
- public-life surfaces around docks, bridges, ferries, and market quays

## Suggested Module Mapping

- `WorldSettlements`
- `TradeAndIndustry`
- `OrderAndBanditry`
- `NarrativeProjection`
- `Presentation.Unity`
