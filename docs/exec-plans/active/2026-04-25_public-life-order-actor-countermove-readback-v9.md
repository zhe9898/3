# Public-Life Order Closure v9: Actor Countermove Readback Hardening & Full Path Proof

## Status

Completed - 2026-04-25

## Framing

This plan hardens the v8 actor-countermove slice inside the same rule-driven command / residue / social-memory / response loop. It is not an event-pool, not an event-centered design, and should not be described as an event-chain. `DomainEvent` remains a deterministic fact propagation tool after rules resolve.

v8 added bounded owner-module actor countermoves after response residue exists. v9 does not add a broader actor AI layer. It proves the full set of soft / hard paths and readback surfaces:

Month N response aftermath -> Month N+2 SocialMemory-owned response residue -> later owner-module actor countermove -> projected readback -> later SocialMemory residue when the owner trace is still visible.

Minimum playable-response proof is part of this hardening pass: when residue is visible, the player must see bounded response choices with projected availability, leverage, cost, execution, and readback text. A read model that only exposes hidden codes is not enough.

## Goal

Make the v8 after-account harder to regress by covering every owner module's actor-countermove lane:

- `OrderAndBanditry`: `巡丁自补保` and `脚户误读反噬`.
- `OfficeAndCareer`: `县门自补落地` and `胥吏续拖`.
- `FamilyCore`: `族老自解释` and `族老避羞`.

The player still sees bounded response affordances and readback. Actor movement stays small, deterministic, and module-owned.

## Scope

In scope:

- Add focused proof for Order hard path, Office soft path, Family soft path, and Family hard path.
- Prove current-month SocialMemory skip where module order requires it.
- Prove Family actor traces are owner-state facts first, then can be read by SocialMemory in the same scheduler pass because `FamilyCore` runs before `SocialMemoryAndRelations` and keeps response carryover visible.
- Prove the existing response affordance surface is minimally playable: at least three bounded choices expose projected availability, leverage, cost, execution, and next-readback text.
- Prove projected public-life / governance / family receipts carry actor-countermove labels and aftermath wording.
- Add Unity/presentation adapter proof that actor-countermove readback is copied from projected fields only.
- Update acceptance/docs to record v9 as proof/readback hardening with no schema impact.

Out of scope:

- New persisted fields.
- New actor scheduler, manager, controller, planner, or event pool.
- UI/Application/Unity computation of response effectiveness.
- Parsing `DomainEvent.Summary`, memory summaries, receipt summaries, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- PersonRegistry expansion.

## Ownership

- `OrderAndBanditry` owns route-watch / runner countermove traces and mutates only order-owned settlement disorder state.
- `OfficeAndCareer` owns yamen / clerk countermove traces and mutates only office-owned career/jurisdiction state.
- `FamilyCore` owns elder explanation / household guarantee countermove traces and mutates only family-owned clan state.
- `SocialMemoryAndRelations` owns durable residue only. It does not resolve actor countermoves.
- Application builds projected receipts. Unity shell copies projected fields only.

## Save And Schema Impact

Expected impact: no schema bump.

v9 is a proof/readback hardening pass. It reuses v6 response trace fields and v8 trace-code constants:

- `LastRefusalResponseCommandCode`
- `LastRefusalResponseCommandLabel`
- `LastRefusalResponseSummary`
- `LastRefusalResponseOutcomeCode`
- `LastRefusalResponseTraceCode`
- `ResponseCarryoverMonths`

`OrderAndBanditry` remains schema `9`, `OfficeAndCareer` remains schema `7`, `FamilyCore` remains schema `8`, and `SocialMemoryAndRelations` remains schema `3`. If implementation unexpectedly requires new persisted state, this plan must be revised before code lands with schema bump, migration, roundtrip and legacy migration tests, plus schema docs.

## Milestones

1. Add integration proof for `脚户误读反噬` from escalated residue.
2. Add integration proof for `县门自补落地` from repaired / contained residue.
3. Add integration proof for `族老自解释` and `族老避羞`.
4. Add presentation / Unity adapter proof for projected actor-countermove readback.
5. Add minimum playable response-loop proof over bounded projected affordances.
6. Update acceptance/docs.
7. Run:
   - `git diff --check`
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `dotnet test Zongzu.sln --no-build`

## Acceptance Proof Targets

- All three owner modules have soft and hard actor-countermove path proof across v8/v9 tests.
- Actor countermoves read structured SocialMemory snapshots and skip current-month memories.
- Order / Office / Family mutate only their own state at monthly actor-countermove time.
- Same-month response commands still do not mutate SocialMemory.
- Order and Office actor traces are read by SocialMemory on a later monthly pass because they run after SocialMemory.
- Family actor traces are owner-state facts first, then can be read by SocialMemory in the same scheduler pass through module order and response carryover; duplicate writes remain guarded by SocialMemory cause checks.
- Minimum playable response proof shows visible residue leading to at least three bounded response affordances with availability, leverage, cost, execution, and readback text.
- Public-life receipts, governance lane / docket, family-facing readback, and Unity shell surfaces expose actor-countermove aftermath from projections only.
- No schema bump or migration is introduced.

## Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration tests passed: `PublicLifeOrderActorCountermoveRuleDrivenTests`, `PublicLifeOrderRefusalResponseRuleDrivenTests`, and `PublicLifeOrderResidueDecayFrictionRuleDrivenTests`.
- Focused Unity/presentation test passed: `Compose_ProjectsActorCountermoveReadbackWithoutShellAuthority`.
- Focused architecture guard tests passed for actor-countermove structured reads, summary-parsing guards, forbidden manager/god-controller names, PersonRegistry scope, and SocialMemory write ownership.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
- Save/schema impact remains none: no new persisted fields, no schema version bump, and no migration.
