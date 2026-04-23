# Chain 7/8/9 Review Hardening Addendum

> Parent plan: `2026-04-23_chain7-8-9-office-court-regime-thin-slice.md`
> Date: 2026-04-23
> Scope: review hardening for the office / court / regime thin slices

## Boundary Decisions

- Chain 7 remains a real-scheduler thin slice, not the full official-clerk-execution chain.
- Chain 8 remains a real-scheduler thin slice, not the full court-agenda / policy-dispatch chain.
- Chain 9 remains a real-scheduler thin slice, not the full regime-recognition / household-compliance chain.
- `DomainEvent` records remain downstream receipts and cross-module handoffs after module-owned rules resolve; they are not an event-pool gameplay layer.

## Implemented Corrections

- Chain 7 now persists `OfficeAndCareerState.ActiveClerkCaptureSettlementIds` as the owner-module edge watermark. A high clerk-capture condition emits `ClerkCaptureDeepened` once, does not repeat next month, clears when the condition drops, and remains settlement-scoped.
- Chain 8 now treats `CourtAgendaPressureAccumulated` as court/global input that must be allocated before local mutation. `OfficeAndCareer` opens exactly one `PolicyWindowOpened` for the selected court-facing jurisdiction when multiple jurisdictions exist.
- Chain 9 now treats `OfficeDefected` as a receipt after office-owned state mutation. `OfficialDefectionRisk` is persisted on each `OfficeCareerState`; only the highest-risk appointed official above threshold defects in the current thin slice.
- `WorldSettlements` default `MandateConfidence` is neutral (`70`) so an unseeded world does not emit court/regime crisis events or preempt unrelated pressure chains.
- `OfficeAndCareer` schema is now `6`; save migration `5 -> 6` backfills the new clerk-capture watermark list and official-defection risk fields without adding envelopes to disabled manifests.

## Verification

- Focused handler coverage: `Chain789OfficePressureHandlerTests`.
- Real scheduler coverage: `OfficeCourtRegimePressureChainTests`.
- Persistence coverage: governance migration and save roundtrip tests.

## Deferred Full-Chain Work

- Court-process owned state, policy dispatch, faction pressure, household compliance, market consequences, public legitimacy, force backing, memory residue, and future imperial / dynasty-cycle ownership remain deferred.
- The current slice proves module boundaries and scheduler propagation only; it should be deepened by owner modules instead of by adding Application-layer formulas.
