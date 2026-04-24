# Thin-Chain Topology Index

## Goal

Freeze the post-merge Renzong thin-chain topology in one index document and link it back into the roadmap, integration rules, and acceptance criteria.

This is a structure pass, not a rule-density pass.

## Scope In / Out

In:
- add a topology index for chains 1-9
- record locus, same-month behavior, watermarks, receipts, proof tests, and full-chain debt
- link the index from high-level docs

Out:
- no new simulation formulas
- no schema changes
- no new event names
- no test rewrites unless the docs reveal a concrete mismatch

## Affected Modules

Documentation-only pass. The referenced owners are:
- `WorldSettlements`
- `PopulationAndHouseholds`
- `TradeAndIndustry`
- `EducationAndExams`
- `FamilyCore`
- `OfficeAndCareer`
- `OrderAndBanditry`
- `PublicLifeAndRumor`

## Save/Schema Impact

None.

## Determinism Risk

None from this pass. The index documents existing determinism rules: bounded scheduler drain, fresh-event watermarks, scoped handlers, and module-owned watermarks.

## Milestones

1. Read project docs, `zongzu-game-design`, and `zongzu-ancient-china` references.
2. Add `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`.
3. Link the new index from docs that guide future chain work.
4. Verify docs and git diff.

## Tests To Add/Update

No code tests required for a documentation-only topology freeze.

Existing proof tests named in the index:
- `RenzongPressureChainTests`
- `ExamPrestigeChainTests`
- `ImperialAmnestyDisorderChainTests`
- `FrontierSupplyHouseholdChainTests`
- `DisasterDisorderPublicLifeChainTests`
- `OfficeCourtRegimePressureChainTests`

## Rollback / Fallback Plan

Revert the new index and link edits. No save or schema migration is involved.

## Open Questions

- Whether to later add a generated contract graph that validates `PublishedEvents` / `ConsumedEvents` against this index.
- Whether chain 1 should get an off-scope negative scheduler test before it is used as a template for thicker tax/corvee work.

