# Public-Life Order Closure v29: Owner-Lane No-Loop / Stale-Guidance Guard

## Goal
- Prevent projected owner-lane guidance from reading like a loop that keeps pushing the player back to the home household or repeats stale follow-up text after the owner lane has already closed.
- Make the readback explicitly say when the owner-lane result is closed enough to stop repeating pressure.

## Scope In
- Projection wording such as `闭环防回压`, `后账已收束，不再回压本户`, `旧提示仅作读回`, and `不重复追本户`.
- Guard text derived from existing outcome codes only, especially repaired / cooled cases.
- Architecture tests that no loop guard is persisted and no summary/prose parsing is introduced.

## Scope Out
- No cooldown ledger, stale-guidance ledger, owner-lane ledger, receipt-status ledger, repeated-response counter, or hidden target.
- No UI decision policy or affordance availability changes.
- No scheduler step or SocialMemory command handling.

## Save / Schema Impact
- Target impact: none.
- v29 is projection guard wording only.

## Milestones
1. Add no-loop guard wording to repaired/cooling closure paths.
2. Ensure non-repaired paths still point to owner lanes rather than home household.
3. Add focused integration, architecture, and Unity copy-only assertions.
4. Update docs and acceptance criteria.

## Evidence Checklist
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture / Unity presentation tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] docs updated
- [x] schema/migration impact explicitly documented as none
