# Chain 1 Household Tax-Burden Profile

## Goal

Thicken chain 1 by replacing the current flat tax-season debt bump with a small household-owned exposure profile.

Current thin chain:

`WorldSettlements.TaxSeasonOpened -> PopulationAndHouseholds.HouseholdDebtSpiked -> OfficeAndCareer.YamenOverloaded -> PublicLife heat`

This pass keeps the topology unchanged and only deepens how `PopulationAndHouseholds` converts `TaxSeasonOpened` into household debt pressure. It intentionally reuses the existing multi-dimensional household state instead of adding a second route system or a new application-layer rule table.

## Scope In / Out

In:
- keep `TaxSeasonOpened` as the upstream event
- filter tax pressure by event `EntityKey` when it contains a settlement id
- compute tax-season debt delta from existing household profile dimensions: livelihood exposure, land visibility, grain/cash buffer, labor/dependency load, debt/distress fragility, and interaction terms
- emit `HouseholdDebtSpiked` with structured cause/source/settlement/debt metadata
- add focused handler tests for differential burden and off-scope settlement protection
- update the thin-chain topology index and integration notes

Out:
- no new event name
- no schema version bump
- no full tax formula, precise household grade, named tax types, tenant rent cascade, or market cash squeeze
- no UI or projection changes

## Affected Modules

- `PopulationAndHouseholds`: owns household burden, distress, debt, livelihood, and household receipt events
- `WorldSettlements`: remains source of `TaxSeasonOpened`; no code changes planned
- `OfficeAndCareer` / `PublicLifeAndRumor`: unchanged downstream consumers

## Save/Schema Impact

None. The rule uses existing household fields:
- `Livelihood`
- `LandHolding`
- `GrainStore`
- `LaborCapacity`
- `DependentCount`
- `DebtPressure`
- `Distress`
- `MigrationRisk`
- `ToolCondition`
- `ShelterQuality`

## Determinism Risk

Low. The tax burden profile is deterministic and uses no new random calls.

## Milestones

1. [done] Add tests for tax-season handler profile and off-scope behavior.
2. [done] Replace flat `+15` tax debt bump with a multi-dimensional exposure calculation.
3. [done] Add structured metadata to `HouseholdDebtSpiked` emitted from tax-season handling.
4. [done] Update docs.
5. [done] Run full suite.

## Tests To Add/Update

- `TaxSeasonBurdenHandlerTests`:
  - pressed tenant / low-grain / low-labor households take more tax-season debt than buffered smallholders
  - settlement-scoped `TaxSeasonOpened` does not touch off-scope households
  - threshold-crossing `HouseholdDebtSpiked` carries cause/source/settlement/debt/profile metadata
  - symbolic `tax-season` still behaves as the current global thin signal until precise upstream settlement events land

## Rollback / Fallback Plan

Revert the handler and tests to the flat `+15` thin-chain rule. No schema migration is involved.

## Open Questions

- Later full chain needs household grade / zhuhu-kehu visibility rather than using livelihood and landholding as thin proxies.
- Later full chain should decide whether tax-season burden also becomes market cash squeeze in `TradeAndIndustry` and petition pressure in `OfficeAndCareer` via richer metadata.
