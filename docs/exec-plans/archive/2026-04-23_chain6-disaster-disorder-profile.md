# Chain 6 Disaster Disorder Profile

> Date: 2026-04-23
> Status: Implemented in branch `codex/thin-chain-topology-index`
> Scope: first rule thickening on top of the existing chain-6 thin slice

## Goal

Keep chain 6 as the already-proven scheduler path:

```
WorldSettlements.DisasterDeclared
  -> OrderAndBanditry.DisorderSpike
  -> PublicLifeAndRumor street-talk heat
```

Replace the old fixed disaster delta with an order-owned, explainable disaster-disorder profile.

## Boundary

- `WorldSettlements` owns disaster declaration and flood watermark state.
- `OrderAndBanditry` owns local disorder mutation and threshold receipts.
- `PublicLifeAndRumor` owns downstream public heat projection.
- This pass does not add relief decisions, market panic, household migration, or memory residue.
- No handler parses event `Summary`; rules use event type, entity key, current module state, and metadata.

## Implemented Milestones

1. `OrderAndBanditry` still consumes only settlement-scoped `WorldSettlements.DisasterDeclared`.
2. Disaster pressure is computed from structured metadata:
   - severity
   - flood risk
   - embankment strain
3. Local order soil is computed from order-owned state:
   - disorder pressure
   - bandit threat
   - black-route pressure
   - coercion risk
4. Route rupture and administrative drag are included:
   - route pressure
   - retaliation risk
   - implementation drag
5. Suppression buffers can absorb moderate disasters:
   - suppression relief
   - route shielding
   - response activation
   - administrative suppression window
6. `DisorderSpike` now carries disaster-disorder profile metadata when the public threshold is crossed.

## Tests

- `DisasterDisorderHandlerTests` verifies:
  - severe flood still raises disorder enough for the existing thin slice
  - moderate flood can cross threshold with profile metadata
  - off-scope settlements remain untouched
  - summary text without metadata remains no-op
  - fragile local order amplifies a moderate disaster beyond the old flat delta
  - strong suppression buffers can absorb a moderate disaster
- `DisasterDisorderPublicLifeChainTests` verifies the real scheduler still drains into public-life heat and now carries profile metadata.

## Save / Schema

- No module save schema changes.
- New `DomainEventMetadataKeys` entries are event contract metadata only.

## Determinism

- The formula is deterministic and uses only event metadata plus the matched settlement's order state.
- The mutation remains settlement-scoped through `EntityKey`.
- No random draw was introduced.

## Still Not Done

- Drought, locust, epidemic, and simultaneous disaster loci.
- Relief decision and granary sufficiency.
- Household subsistence / migration / illness effects.
- Market panic and grain price shock.
- Route insecurity from refugees and grain scarcity.
- SocialMemory disaster residue.
- Public legitimacy shift from relief success or failure.
