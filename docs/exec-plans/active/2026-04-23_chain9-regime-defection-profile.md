# Chain 9 Regime Defection Profile

> Date: 2026-04-23
> Parent topology: `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`
> Scope: first thickening for `RegimeLegitimacyShifted -> OfficeDefected`

## Boundary

- This remains a real-scheduler thin slice, not the full regime-recognition, compliance, grain-route control, household obedience, public legitimacy, ritual claim, force backing, rebellion, or dynasty-cycle chain.
- `WorldSettlements.RegimeLegitimacyShifted` is a regime/global pressure receipt. It does not directly mutate every official, household, market, public-life node, or force.
- `OfficeAndCareer` owns official defection risk and emits `OfficeDefected` only after mutating the selected official's appointment state.
- Domain events remain downstream receipts and cross-module handoffs after rule resolution; they are not the gameplay driver.

## Implemented

- Added defection metadata keys for:
  - baseline instability pressure
  - mandate deficit
  - demotion pressure
  - clerk pressure
  - petition pressure
  - reputation strain
  - authority / reputation buffer
- `OfficeAndCareer` now computes `DefectionProfile` from existing official state:
  - `DemotionPressure`
  - `ClerkDependence`
  - `PetitionPressure`
  - `OfficeReputation`
  - `AuthorityTier`
  - court-side `MandateConfidence`
- The handler persists `OfficialDefectionRisk`, selects only the highest-risk appointed official above threshold, mutates that official, and emits `OfficeDefected` as the receipt.
- A low mandate signal alone does not defect a well-buffered official.

## Tests

- Focused handler tests assert structured defection metadata.
- Focused handler tests prove a buffered official remains below threshold under low mandate confidence.
- Real scheduler integration still proves regime pressure defects only one high-risk appointed official.

## Determinism And Save Notes

- No new module state or schema migration is required.
- The formula is deterministic and uses only current event metadata plus `OfficeAndCareer` state.
- No Application-layer rule was added.

## Deferred

- Regime recognition, grain-route control, household compliance, public legitimacy, ritual claim, force backing, rebellion-to-polity escalation, and dynasty-cycle consequences remain full-chain debt.
