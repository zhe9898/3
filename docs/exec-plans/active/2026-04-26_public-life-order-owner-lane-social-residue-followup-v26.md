# Public-Life Order Closure v26: Owner-Lane Social Residue Follow-Up Guidance

## Goal
- Add projection/readback guidance that interprets visible v25 `社会余味读回` as a next-action hint: cool down, lightly continue in the owner lane, switch owner-lane tactic, or wait rather than press the home household again.
- Keep the player-facing loop clear: the home-household response can only缓住/暂压/吃紧/放置 its own side; owner-lane responses can repair/contain/escalate/ignore their lane; SocialMemory residue can show whether the after-account is渐平、留账、转硬、发酸; v26 only tells the player how to read that as follow-up guidance.
- This is not a new command system, not an event pool, not an “事件链”, not a hidden cooldown ledger, and not a thick household economy / yamen formula / runner faction / patrol AI pass. DomainEvent remains one factual propagation tool, not the design body.

## Scope In
- Application projection/read-model wording appended after matching `社会余味读回`, with tokens such as `余味冷却提示`, `余味续接提示`, and `余味换招提示`.
- Public-life, governance docket, and family-facing surfaces should copy the projected follow-up cue so the player can read:
  - `后账渐平` means stop pressing the household and watch the owner lane / SocialMemory cool.
  - `后账暂压留账` means light continuation belongs in the owner lane, not the ordinary household.
  - `后账转硬` or `后账放置发酸` means change tactic or wait for a better owner-lane entry, not force the home-household line.
- Tests for Order lane and Office/Family lane guidance, no summary/prose parsing, Unity shell copy-only behavior, and architecture guardrails.

## Scope Out
- No new player commands.
- No new cooldown ledger, owner-lane ledger, follow-up ledger, receipt-status ledger, outcome ledger, household target field, command queue, or repeated-response counter.
- No new SocialMemory rule formulas, durable memory namespace, or persisted projection cache.
- No command resolution in Application, UI, or Unity.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, or SocialMemory summary prose.
- No expansion of `PersonRegistry`.

## Affected Modules
- `Zongzu.Application`: projects owner-lane social-residue follow-up guidance from existing structured fields (`SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane command/outcome traces).
- `Zongzu.Presentation.Unity`: remains copy-only through existing view-model fields; tests assert the shell displays projected text without calculating follow-up validity.
- Docs and tests for boundary and acceptance alignment.

## Ownership Notes
- `OrderAndBanditry` still owns order repair, road watch, route pressure repair, and route/banditry owner-lane command outcomes.
- `OfficeAndCareer` still owns county-yamen催办, document landing, and clerk-drag handling.
- `FamilyCore` still owns clan elder explanation, household guarantee repair, and lineage face repair.
- `PopulationAndHouseholds` still owns home-household low-power responses and household labor/debt/distress/migration traces.
- `SocialMemoryAndRelations` still does not process commands. It reads structured owner-lane aftermath during monthly simulation and writes only its own memory/narrative/climate residue.

## Save / Schema Impact
- Target impact: none.
- v26 adds projection/readback guidance only. It must not add persisted fields, bump module schema versions, alter manifests, or add migrations.
- Existing SocialMemory records remain in schema `3`; existing owner-module response trace fields remain unchanged.
- If implementation discovers a genuine need for new persisted state, stop before coding it and document the owning module schema bump, migration, roundtrip tests, legacy migration tests, `DATA_SCHEMA.md`, and `SCHEMA_NAMESPACE_RULES.md` impact.

## Determinism Risk
- Low. Projection reads already-built deterministic snapshots after simulation state exists.
- No new randomness, scheduler cadence, event handling, save serialization shape, or cross-module authoritative mutation is introduced.

## Milestones
1. Read v25 code/docs and owner-lane/SocialMemory projection seams.
2. Add projection helper for `余味冷却提示` / `余味续接提示` / `余味换招提示` using structured SocialMemory residue only.
3. Thread projected guidance through public-life, governance docket, and family-facing surfaces without UI/Unity inference.
4. Add/extend integration, architecture, and Unity presentation tests.
5. Update docs with v26 no-schema/no-migration impact and the “home household is not a universal follow-up lane” boundary.
6. Run build, focused tests, `git diff --check`, full `dotnet test --no-build`, then commit and push.

## Tests To Add / Update
- v25 social-residue readback should now include v26 follow-up guidance after the later monthly SocialMemory pass.
- Cover at least two directions:
  - Order lane repaired / `后账渐平` -> `余味冷却提示`.
  - Office or Family contained/escalated/ignored -> `余味续接提示` or `余味换招提示`.
- Architecture guard: owner-lane follow-up projection must stay in Application read model, use structured cause keys/outcome codes only, and avoid summary/prose parsing.
- Unity presentation guard: shell copies projected follow-up text only.

## Rollback / Fallback Plan
- If the projection becomes too ambiguous without new state, keep v26 as docs-only and stop before adding a ledger.
- If tests show the guidance needs durable state, stop and open a schema/migration plan instead of smuggling state into Application or Unity.

## Open Questions
- None blocking. The intended v26 answer is projection-only guidance over existing v25 residue.

## Evidence Checklist
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture / Unity presentation tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] docs updated
- [x] schema/migration impact explicitly documented as no field/schema/envelope/migration impact
- [x] branch committed and pushed
