# Public-Life Order Closure v28: Owner Follow-Up Receipt Closure

## Goal
- When an owner-lane command has already followed the after-account, project a receipt-side closure reading so the player sees whether the owner lane reads as closed, still holding, hardened, or left for a different owner-lane tactic.
- Keep the receipt language from bouncing responsibility back to the home household.

## Scope In
- Projection wording such as `后手收口读回`, `已收口：不回压本户`, `仍留账：轻续本 lane`, `转硬待换招`, and `未接待承口`.
- Append this to existing owner-lane response receipts using structured owner outcome codes and SocialMemory residue where already available.
- Tests that Order and Office/Family owner-lane receipts show closure language after the same v19-v26 scenario.

## Scope Out
- No new owner response command.
- No persisted closure status, owner-lane ledger, outcome ledger, receipt-status ledger, or follow-up ledger.
- No receipt prose parsing by SocialMemory or projection code.
- No new schema, migration, command target, or PersonRegistry expansion.

## Save / Schema Impact
- Target impact: none.
- v28 is receipt/readback projection over existing owner-module traces and existing SocialMemory residue only.

## Milestones
1. Add structured receipt closure helper in the existing owner-lane projection helper.
2. Thread it through public-life owner-lane response receipts.
3. Add focused integration and architecture coverage.
4. Update docs and acceptance criteria.

## Evidence Checklist
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] docs updated
- [x] schema/migration impact explicitly documented as none
