# Public-Life Order Closure v6: Refusal Residue Response & Repair Surface

Status: implemented / validated  
Date: 2026-04-25  
Baseline: after commit `43976cb`, with v5 refusal / partial residue complete.

## Purpose

Close the post-v5 account by making refused or partial public-life order residue respondable without turning the design into an event-centered or event-pool model. The loop is rule-driven:

Month N public-life order refusal / partial residue -> Month N+1 projected readback and bounded response affordance -> owning module resolves the response -> structured `Repaired` / `Contained` / `Escalated` / `Ignored` aftermath -> Month N+2 `SocialMemoryAndRelations` reads structured aftermath and adjusts durable shame / fear / favor / grudge / obligation residue -> projected readback and shell visibility.

`DomainEvent` may still carry facts, but it is not the design body and must not be parsed as a response source.

## Scope

- Add bounded response affordances for visible v5 `添雇巡丁` / `严缉路匪` refused or partial residue:
  - `补保巡丁`
  - `请族老解释`
  - `押文催县门`
  - `赔脚户误读`
  - `暂缓强压`
  - `改走递报`
- Keep all response affordances in projected read models. UI and Unity copy fields only.
- Keep command resolution in the owning module:
  - `OrderAndBanditry`: order repair, road watch, route-pressure repair.
  - `OfficeAndCareer`: county-yamen催办, 文移落地, 胥吏拖延.
  - `FamilyCore`: 族老公开解释 and home-household guarantee repair.
  - `SocialMemoryAndRelations`: no commands; later monthly read of structured aftermath only.
- Add structured response outcome codes:
  - `Repaired`
  - `Contained`
  - `Escalated`
  - `Ignored`

## Non-Goals

- No event-pool core loop.
- No parsing `DomainEvent.Summary`, receipt summary, or `LastInterventionSummary`.
- No Application / UI / Unity computation of response effectiveness.
- No new `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.
- No SocialMemory writes outside `SocialMemoryAndRelations`.

## State And Schema Impact

This task persists response traces so save/load between Month N+1 response command and Month N+2 SocialMemory adjustment remains deterministic.

Implemented owning-module schema bumps:

- `OrderAndBanditry` schema 8 -> 9:
  - settlement-level response trace fields:
    - `LastRefusalResponseCommandCode`
    - `LastRefusalResponseCommandLabel`
    - `LastRefusalResponseSummary`
    - `LastRefusalResponseOutcomeCode`
    - `LastRefusalResponseTraceCode`
    - `ResponseCarryoverMonths`
- `OfficeAndCareer` schema 6 -> 7:
  - career / jurisdiction response trace fields mirroring the above.
- `FamilyCore` schema 7 -> 8:
  - clan response trace fields mirroring the above.

`SocialMemoryAndRelations` schema remains unchanged. Durable response residue continues to live in existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`; no consumed-response marker was needed.

Updated docs:

- `docs/DATA_SCHEMA.md`
- `docs/SCHEMA_NAMESPACE_RULES.md`
- `docs/MODULE_BOUNDARIES.md`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/RELATIONSHIPS_AND_GRUDGES.md`
- `docs/SIMULATION.md`
- `docs/UI_AND_PRESENTATION.md`
- `docs/ACCEPTANCE_TESTS.md`

## Determinism Notes

- Response commands resolve from module-owned state plus read-only queries only.
- Response outcome codes are structured fields, not inferred from prose.
- Month N+2 SocialMemory adjustment is driven by deterministic ordering over settlements, jurisdictions, and clans.
- Same-month response commands must not mutate SocialMemory.
- Carryover is one-month proof surface for later module reads; modules after SocialMemory may clear it after the monthly pass. FamilyCore executes before SocialMemory, so duplicate SocialMemory writes must be guarded by existing durable memory cause checks rather than clearing before read.

## Milestones

1. [x] Add contracts: response command names and response outcome / trace codes.
2. [x] Add persisted response trace fields, queries, migrations, and schema docs.
3. [x] Add owning-module command resolution for the six response affordances.
4. [x] Add read-model affordance and readback projection across public-life, governance, and family surfaces.
5. [x] Add SocialMemory Month N+2 structured aftermath adjustment.
6. [x] Update Unity shell copy tests / adapters only where projected fields are newly surfaced.
7. [x] Add focused tests and architecture guards.
8. [x] Run final validation:
   - `dotnet build Zongzu.sln --no-restore`
   - focused tests
   - `dotnet test Zongzu.sln --no-build`

## Acceptance Evidence To Capture

- [x] v5 refusal / partial residue produces Month N+1 bounded response affordances from read models.
- [x] Response command mutates only its owning module at command time.
- [x] Same-month response leaves `SocialMemoryAndRelations` unchanged.
- [x] Month N+2 SocialMemory reads structured response aftermath and adjusts durable residue.
- [x] Paths covered include `Repaired`, `Contained`, and `Escalated`; `Ignored` is represented by structured owner fallback.
- [x] Public-life receipt, governance lane / docket, and family surface include response result readback.
- [x] Unity shell displays projected response readback only.
- [x] Save/load preserves new response trace fields.
- [x] Legacy migration tests cover old schema.
- [x] Architecture tests guard boundary drift, summary parsing, forbidden manager/god-controller names, and PersonRegistry expansion.

## Implementation Evidence

- Contracts: `PublicLifeOrderResponseOutcomeCodes`, `PublicLifeOrderResponseTraceCodes`, and response command names were added to shared contracts.
- Ownership: `OrderAndBanditry` resolves `补保巡丁`, `赔脚户误读`, and `暂缓强压`; `OfficeAndCareer` resolves `押文催县门` and `改走递报`; `FamilyCore` resolves `请族老解释`.
- Read models: response affordances and response readback now project through public-life command surfaces, governance lane/docket summaries, and family-facing receipts without UI-side authority.
- Social memory: Month N+2 response residue reads structured owner aftermath and writes only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- Save/migration: `FamilyCore` `7 -> 8`, `OfficeAndCareer` `6 -> 7`, and `OrderAndBanditry` `8 -> 9` migration steps initialize response trace fields; SocialMemory remains schema `3`.
- Shell: Unity tests cover copied projected response readback only, with no module query or shell-side outcome computation.

## Validation Evidence

- `git diff --check` passed.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused v6 integration tests passed: 3 tests in `PublicLifeOrderRefusalResponseRuleDrivenTests`.
- Focused persistence / architecture / Unity checks passed for response trace roundtrip, summary-parsing guard, and projected shell readback.
- `dotnet test Zongzu.sln --no-build` passed across the full solution.
