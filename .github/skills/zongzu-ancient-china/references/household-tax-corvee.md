# Household Registration, Tax, and Corvee

Use this reference when the task is about household registration, landholding, tax, labor service, transport burden, grain movement, extraction pressure, or state visibility.

## What To Distinguish

- registered visibility versus actual population on the ground
- productive capacity versus taxable capacity
- land control versus formal title
- tax quota versus actual collection
- labor obligation versus cash or grain substitution
- transport responsibility versus local production

## Good Zongzu Abstractions

- `registered_households`
- `taxable_pressure`
- `corvee_load`
- `estate_yield`
- `transport_drag`
- `evasion_rate`
- `granary_security`
- `route_exposure`

These values work best when they shape the map and desk board through movement cost, extraction spikes, shortage risk, and local resentment.

## Map and Board Signals

- estates, fields, granaries, ferries, roads, river crossings, and market nodes
- tax or labor overlays on specific routes and settlements
- seasonal pressure windows for transport and collection
- chokepoints where grain movement or labor drafts create conflict

## Design Guidance

- Do not collapse land, tax, and labor into one generic economy stat.
- Household registration should matter because it changes what the state can see, claim, and punish.
- Corvee should feel like a drag on time, labor, morale, and route security, not just a flat money loss.
- Grain transport and route exposure are often more gameable than quoting precise historical tax formulas.

## Suggested Module Mapping

- `PopulationAndHouseholds`
- `TradeAndIndustry`
- `WorldSettlements`
- `OrderAndBanditry`
- `ConflictAndForce`
