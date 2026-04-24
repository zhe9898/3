# Chain 8 Court Policy Window Profile

> Date: 2026-04-23
> Parent topology: `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`
> Scope: first thickening for `CourtAgendaPressureAccumulated -> PolicyWindowOpened`

## Boundary

- This remains a real-scheduler thin slice, not the full court agenda, faction, appointment slate, policy wording, dispatch, implementation, household, market, public-life, or memory chain.
- `WorldSettlements.CourtAgendaPressureAccumulated` is a court/global pressure receipt. It does not directly mutate local jurisdictions.
- `OfficeAndCareer` owns policy-window allocation. The handler chooses one eligible court-facing jurisdiction before emitting `PolicyWindowOpened`.
- Domain events remain downstream receipts and module handoffs after rule resolution; they are not a free-standing event pool.

## Implemented

- Added policy-window metadata keys for:
  - window pressure
  - mandate deficit
  - authority signal
  - jurisdiction leverage signal
  - petition signal
  - administrative drag
  - clerk drag
  - backlog drag
- `OfficeAndCareer` now computes `PolicyWindowProfile` from existing jurisdiction state:
  - `AuthorityTier`
  - `JurisdictionLeverage`
  - `PetitionPressure`
  - `AdministrativeTaskLoad`
  - `ClerkDependence`
  - `PetitionBacklog`
  - court-side `MandateConfidence`
- Selection orders eligible jurisdictions by computed window pressure before authority / leverage tie-breakers.
- High local drag can suppress the policy window even when the court/global mandate signal is low.
- `PolicyWindowOpened` still emits a concrete settlement `EntityKey`.

## Tests

- Focused handler tests assert structured profile metadata.
- Focused handler tests include a negative case where clerk, backlog, and task drag absorb the court signal below threshold.
- Real scheduler integration still proves a court/global event opens only one allocated jurisdiction.

## Determinism And Save Notes

- No new module state or schema migration is required.
- The formula is deterministic and uses only current event metadata plus `OfficeAndCareer` jurisdiction state.
- No Application-layer rule was added.

## Deferred

- Court agenda kind, faction sponsorship, appointment slate, policy wording, dispatch arrival, implementation drag, policy capture, and downstream household / market / public-life consequences remain full-chain debt.
