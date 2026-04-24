# Chain 5 Frontier Supply Household Profile

> Date: 2026-04-23
> Status: Implemented in branch `codex/thin-chain-topology-index`
> Scope: first rule thickening on top of the existing chain-5 thin slice

## Goal

Keep the chain-5 route settlement-scoped and scheduler-real, but replace the fixed household burden response with an explainable multi-dimensional profile:

```
WorldSettlements.FrontierStrainEscalated
  -> OfficeAndCareer.OfficialSupplyRequisition
  -> PopulationAndHouseholds.HouseholdBurdenIncreased
```

This remains a thin route through the living society. It is not the full frontier/war economy implementation.

## Boundary

- `WorldSettlements` owns frontier pressure and declaration watermarks.
- `OfficeAndCareer` owns the conversion from frontier strain into official supply execution pressure for the matching jurisdiction only.
- `PopulationAndHouseholds` owns household burden effects and receipt emission.
- No module mutates another module's authoritative state.
- No handler parses event `Summary`; rule input is event type, entity key, queries, and structured metadata.
- No UI or narrative projection drives state.

## Implemented Milestones

1. `FrontierStrainEscalated` continues to carry `EntityKey = settlementId`; it now also carries `SettlementId` metadata for downstream validation.
2. `OfficeAndCareer` consumes the frontier event only for matching jurisdictions and emits `OfficialSupplyRequisition` with a supply-execution profile:
   - frontier pressure and severity
   - supply pressure
   - quota pressure
   - docket pressure
   - clerk distortion pressure
   - authority buffer
   - downstream distress/debt/labor/migration hints
3. `OfficeAndCareer` applies only office-owned pressure locally:
   - administrative task load
   - petition backlog
   - leverage/authority drift
4. `PopulationAndHouseholds` consumes `OfficialSupplyRequisition` through a household-owned profile:
   - livelihood exposure
   - grain/tool/shelter resource buffer
   - labor and dependent pressure
   - debt/distress fragility
   - migration pressure
   - interaction pressure
5. `HouseholdBurdenIncreased` remains a threshold-crossing receipt event and carries structured cause/source/settlement/profile metadata.

## Tests

- `FrontierSupplyHandlerTests` verifies scoped office dispatch, structured metadata, and no fan-out to unrelated jurisdictions.
- `OfficialSupplyBurdenHandlerTests` verifies household profile computation, off-scope protection, receipt metadata, and invalid-key no-op behavior.
- `FrontierSupplyHouseholdChainTests` verifies the real scheduler drains the chain through bounded rounds.

## Save / Schema

- No new module save schema fields in this thickening pass.
- New metadata keys were added to `DomainEventMetadataKeys`; they are event payload contract surface, not save root state.
- Existing chain-5 save impact remains the earlier thin-slice `WorldSettlements` frontier-pressure and watermark fields.

## Determinism

- The profile formulas are deterministic and use only local state, existing queries, and event metadata.
- Event scope remains a typed settlement key.
- No random draw was introduced in the new profile logic.

## Still Not Done

- Multiple frontier sectors and sector-to-settlement allocation.
- Formal grain/cash/labor quota schedules.
- WarfareCampaign mobilization windows.
- ConflictAndForce readiness demand.
- TradeAndIndustry market diversion.
- PublicLife military-burden projection.
- SocialMemory long residue for forced supply, favoritism, and resentment.
