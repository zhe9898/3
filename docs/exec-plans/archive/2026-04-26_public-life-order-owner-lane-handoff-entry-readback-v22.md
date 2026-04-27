# Public-Life Order Closure v22: Owner-Lane Handoff Entry Readback

## Goal

Carry the v20-v21 owner-lane return guidance one small step further by naming the existing owner-lane command entry points that the player should look at after a home-household local response has reached its limit.

The intended player read is:

- the home household has already缓住 / 暂压 / 吃紧 / 放置 its own side,
- the unresolved external after-account is still returned to Order / Office / Family owner lanes,
- the next inspection should use existing lane affordances such as 添雇巡丁, 押文催县门, or 请族老解释,
- no new command, queue, command router, ledger, or persisted target is introduced.

This remains projection/readback guidance inside the rule-driven command / aftermath / social-memory / readback loop. It is not an event chain, not an event pool, not a new command system, not a thick household economy, not county-yamen formula work, not runner faction AI, and not patrol AI. `DomainEvent` remains only one fact-propagation tool.

## Scope In

- Add projected `承接入口` wording to the owner-lane return guidance generated from existing `HouseholdPressureSnapshot` structure.
- Keep the wording lane-specific:
  - Order lane: existing road-watch / banditry / route-pressure commands.
  - Office lane: existing county-yamen / document / clerk-delay commands.
  - Family lane: existing clan-elder / guarantee-face commands.
- Ensure Office/Governance, Family, and public-life command surfaces copy this guidance through existing summary fields.
- Add focused tests proving the guidance appears after a v19-v21 home-household response and remains projection-only.

## Scope Out

- No new command family.
- No command queue or command recommendation state.
- No new persisted state.
- No schema bump or migration.
- No owner-lane ledger, cooldown ledger, household target field, or command-target expansion.
- No Application-owned command outcome formula.
- No UI/Unity owner-lane computation, module query, or SocialMemory write.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastRefusalResponseSummary`, or `LastLocalResponseSummary`.

## Affected Modules

- `Zongzu.Application`: presentation read-model guidance only.
- `Zongzu.Contracts`: no shape change expected.
- `Zongzu.Presentation.Unity`: no authority change; tests prove existing ViewModel fields copy the projected text.
- Tests:
  - integration coverage for Order, Office/Governance, and Family handoff-entry wording,
  - architecture coverage for projection-only / no-summary-parsing / no-schema drift,
  - Unity presentation copy-only coverage.
- Docs:
  - skill matrix, alignment audit, module boundaries, integration rules, social memory docs, schema docs, simulation docs, UI docs, acceptance tests.

## Save/Schema Impact

No persisted state target.

Expected impact: none. v22 should only extend projection wording over existing read-model fields. If implementation discovers a need for persisted handoff state, stop before code changes and document:

- owning module schema version bump,
- migration,
- save roundtrip and legacy migration tests,
- `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md` updates,
- why projection-only guidance was insufficient.

## Determinism Risk

Low. The projection is deterministic over existing household read models and does not alter scheduler flow, command resolution, module mutation, event emission, replay hash authority, or save payloads.

## Milestones

1. Confirm v21 owner-lane guidance helper is the correct projection point.
2. Add lane-specific `承接入口` readback text from existing structured household response fields.
3. Update integration tests for Order, Office/Governance, and Family surfaces.
4. Update architecture tests to keep the helper projection-only and no-schema/no-summary-parsing.
5. Update Unity presentation tests to prove copy-only display.
6. Update docs and record no schema/migration impact.
7. Run build, focused tests, `git diff --check`, and full tests.
8. Commit and push `codex/public-life-order-leverage-v3`.

## Tests To Add/Update

- `PublicLifeOrderRefusalResponseRuleDrivenTests`
  - after `SendHouseholdRoadMessage` local response, projected readback includes:
    - `承接入口`,
    - Order existing command entry wording for road-watch / route repair,
    - Office existing command entry wording for yamen / document follow-up,
    - Family existing command entry wording for clan elder / guarantee explanation.
  - command-time response still mutates only `PopulationAndHouseholds`.
  - same-month response still does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`.
- Architecture tests
  - the owner-lane helper must not reference state mutation, schema/migration, command routing, summaries, or SocialMemory writers.
  - the helper must contain the projected `承接入口` token and lane-specific command-entry labels.
- Unity presentation tests
  - Office and Family shell surfaces copy `承接入口` guidance from existing DTO fields only.

## Rollback / Fallback Plan

If command-entry wording risks implying automatic execution or a new recommendation system, revert to v21 owner-lane return guidance and document v22 as blocked until the shell has an explicit projection-only hint field. Do not add state as a workaround.

## Open Questions

- Whether future shell surfaces should render command-entry labels as links, badges, or plain readback text. Default for v22: plain projected text inside existing summary fields only.
- Whether Order-lane entry text should name all existing public-life order commands or only the likely first two. Default for v22: name a compact set that covers watch, banditry, route, and response-afteraccount repair without ranking them as authority.

## Implementation Evidence

- Extended `PresentationReadModelBuilder.PlayerCommands.OwnerLaneReturnSurface.cs` so the three v20-v21 owner-lane guidance strings also append projected `承接入口` text.
- Order guidance now points back to existing Order affordance labels such as `添雇巡丁`, `严缉路匪`, `催护一路`, `补保巡丁`, and `赔脚户误读`.
- Office guidance now points back to existing Office/PublicLife affordance labels such as `押文催县门`, `改走递报`, `批覆词状`, and `发签催办`.
- Family guidance now points back to existing Family/PublicLife affordance labels such as `请族老解释`, `请族老出面`, and `请族老调停`.
- No command request shape, command queue, owner-lane ledger, cooldown ledger, household target field, schema version, migration, `PersonRegistry` expansion, SocialMemory field, or Unity authority path was added.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed.
- Focused architecture test passed: `Owner_lane_return_surface_readback_must_stay_projection_only`.
- Focused Unity presentation test passed: `Compose_ProjectsOwnerLaneReturnGuidanceInOfficeAndFamilySurfacesWithoutShellAuthority`.
- Focused integration test passed: `HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome`.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
