# Chain 2 Household Grain-Subsistence Profile

## Goal

Thicken chain 2 by replacing the current flat grain-price distress bump with a household-owned subsistence pressure profile.

Current thin chain:

`WorldSettlements.SeasonPhaseAdvanced(Harvest) -> TradeAndIndustry.GrainPriceSpike -> PopulationAndHouseholds.HouseholdSubsistencePressureChanged`

This pass keeps the topology unchanged. `TradeAndIndustry` records the grain-market fact, while `PopulationAndHouseholds` decides which households feel it most severely from existing household dimensions.

## Scope In / Out

In:
- keep `GrainPriceSpike` as the upstream event
- add structured grain-market metadata to `TradeAndIndustry.GrainPriceSpike`
- compute household distress delta from price pressure, grain-store buffer, livelihood market dependency, labor/dependency load, existing debt/distress fragility, and interaction terms
- emit `HouseholdSubsistencePressureChanged` with structured cause/source/settlement/distress/profile metadata
- rebuild population settlement summaries after grain-price distress changes
- add focused handler tests for differential household impact and off-scope settlement protection
- update chain topology docs and acceptance notes

Out:
- no new event name
- no schema version bump
- no full yield-ratio formula, granary security, route risk, disaster relief, migration/death branch, or famine memory residue
- no UI or projection changes

## Affected Modules

- `TradeAndIndustry`: owns grain market price/supply/demand facts and emits `GrainPriceSpike`
- `PopulationAndHouseholds`: owns household subsistence distress and receipt events
- `WorldSettlements`: remains source of harvest phase; no code changes planned

## Save/Schema Impact

None. The rule uses existing household fields and event metadata only.

## Determinism Risk

Low. The profile is deterministic, orders households by id, and uses no new random calls.

## Milestones

1. [done] Add structured grain-market metadata to `GrainPriceSpike`.
2. [done] Replace flat `+12` distress bump with a household subsistence profile.
3. [done] Add structured metadata to `HouseholdSubsistencePressureChanged`.
4. [done] Add focused handler tests for differential impact and off-scope protection.
5. [done] Update docs.
6. [done] Run integration and full test suites.

## Tests To Add/Update

- `GrainPriceSubsistenceHandlerTests`:
  - low-grain, market-dependent households take more distress than buffered smallholders
  - settlement-scoped `GrainPriceSpike` does not touch off-scope households
  - threshold-crossing `HouseholdSubsistencePressureChanged` carries cause/source/settlement/distress/profile metadata

## Rollback / Fallback Plan

Revert the handler and tests to the flat `+12` thin-chain rule. No schema migration is involved.

## Open Questions

- Later full chain needs `HarvestPressureChanged/yieldRatio`, granary security, route risk, and disaster inputs before the market price formula is historically thicker.
- Later full chain should decide when subsistence pressure becomes migration, illness, theft, petition, relief demand, public rumor, and social-memory residue.
