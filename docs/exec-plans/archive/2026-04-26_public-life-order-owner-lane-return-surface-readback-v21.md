# Public-Life Order Closure v21: Owner-Lane Return Surface Readback

## Goal

Carry the v20 external after-account owner-lane return guidance out of the home-household receipt/readback loop into the lane-facing read surfaces that naturally own the next action:

- `OrderAndBanditry`: road watch, road-bandit, route-pressure repair.
- `OfficeAndCareer`: county-yamen follow-up, document landing, clerk delay.
- `FamilyCore`: clan elder explanation, home-household guarantee, lineage face.
- `SocialMemoryAndRelations`: later-month durable shame/fear/favor/grudge/obligation residue only.

This is a projection/readback refinement. It is not a new command system, not an event pool, not a thick household economy, not county-yamen formula work, not runner faction AI, and not patrol AI. The design body remains the rule-driven command / aftermath / social-memory / readback loop; `DomainEvent` remains one fact-propagation tool rather than the design body.

## Scope In

- Add projected owner-lane return text to Office/Governance and Family-facing read surfaces when an existing home-household local response has already happened.
- Derive the guidance from existing structured projected fields, especially `HouseholdPressureSnapshot.LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, `SponsorClanId`, and settlement/clan ownership.
- Keep the public-life surface readable as: the home household has done what it can; the remaining after-account must return to its owner lane.
- Add focused tests proving:
  - v20 home-household response readback still carries owner-lane return guidance.
  - Office/Governance surfaces show county-yamen/document/clerk after-account return.
  - Family-facing surfaces show clan elder/guarantee after-account return.
  - Unity copies projected fields only.
  - No summary parsing or authority drift is introduced.

## Scope Out

- No new persisted state.
- No migration.
- No owner-lane ledger.
- No cooldown ledger.
- No household target field.
- No new command family.
- No Application-owned command outcome formulas.
- No UI/Unity calculation of owner lanes or command validity.
- No SocialMemory same-month writes and no SocialMemory parsing of projected prose.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`.

## Affected Modules

- `Zongzu.Application`: presentation read-model projection only.
- `Zongzu.Contracts`: no shape change expected.
- `Zongzu.Presentation.Unity`: no authority change; tests prove copy-only behavior through existing ViewModel fields.
- Tests:
  - integration tests for public-life owner-lane return surfaces,
  - architecture tests for summary parsing/schema drift,
  - Unity presentation tests for DTO copy-only display.
- Docs:
  - skill matrix, alignment audit, module boundaries, integration rules, social memory docs, schema docs, simulation docs, UI docs, acceptance tests.

## Save/Schema Impact

No persisted state target.

Expected impact: none. v21 should only add projection wording and tests around existing read models. If implementation discovers a need for persisted state, the task must stop before code changes and document:

- owning module schema version bump,
- migration,
- save roundtrip and legacy migration tests,
- `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md` updates,
- why projection-only state was insufficient.

## Determinism Risk

Low. The projection is deterministic over existing ordered read models. It must not change scheduler flow, module state mutation, command resolution, event emission, or save payloads.

## Milestones

1. Confirm v20 projection sources and current shell DTO copy paths.
2. Add projection-only owner-lane surface guidance for Office/Governance and Family surfaces.
3. Add focused integration tests for Office/Governance and Family after-account return readback after a v19/v20 local household response.
4. Add architecture tests guarding projection-only ownership, no summary parsing, no schema drift, no manager/god-controller drift, and no PersonRegistry expansion.
5. Add/extend Unity presentation tests proving shell surfaces only display projected fields.
6. Update docs and record no schema/migration impact.
7. Run build, focused tests, `git diff --check`, and full tests.
8. Commit and push `codex/public-life-order-leverage-v3`.

## Tests To Add/Update

- `PublicLifeOrderRefusalResponseRuleDrivenTests`
  - After `SendHouseholdRoadMessage` / v20 local response, next read-model bundle includes:
    - public-life owner-lane return guidance,
    - Office/Governance `该走县门/文移 lane` guidance,
    - Family `该走族老/担保 lane` guidance,
    - `本户不能代修`.
  - Command-time response still mutates only `PopulationAndHouseholds`.
  - Same-month response still does not mutate `SocialMemoryAndRelations`.
- Architecture tests
  - projection helper does not reference `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or SocialMemory mutators,
  - no new owner-lane/cooldown ledger or household target field,
  - no Application/UI/Unity authority drift,
  - no forbidden manager/god-controller names,
  - no PersonRegistry expansion,
  - no schema version changes without migration docs/tests.
- Unity presentation tests
  - Office and Family shell surfaces display projected owner-lane guidance from DTO fields only.

## Rollback / Fallback Plan

If any projection coupling becomes ambiguous, keep v20 as the stable baseline and only add documentation/tests that mark the next v21 slice as blocked by missing read-model fields. Do not add state as a workaround.

## Open Questions

- Whether Governance docket should echo the Office-lane guidance in `WhyNowSummary`, `GuidanceSummary`, or both. Default: `GuidanceSummary` for explicit next-lane instruction, and only add `WhyNowSummary` if needed for shell readability.
- Whether Family-facing readback is sufficient through Family command affordances or should also appear in hall docket family lane. Default: command affordance first, no new contract shape.

## Implementation Evidence

- Added `PresentationReadModelBuilder.PlayerCommands.OwnerLaneReturnSurface.cs` as a projection-only helper over existing `HouseholdPressureSnapshot` fields.
- Office/Governance surfaces now project `该走县门/文移 lane` guidance from structured local-response fields.
- Family-facing affordances now project `该走族老/担保 lane` guidance from structured local-response fields.
- Public-life Order affordances may echo `该走巡丁/路匪 lane` guidance when a local household response already exists.
- Unity shell remains copy-only through existing `CommandAffordanceViewModel` fields.
- Schema/migration impact: none. No persisted field, schema bump, migration, owner-lane ledger, cooldown ledger, household target field, command request shape change, `PersonRegistry` expansion, or SocialMemory field was added.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed.
- Focused architecture test passed: `Owner_lane_return_surface_readback_must_stay_projection_only`.
- Focused Unity presentation test passed: `Compose_ProjectsOwnerLaneReturnGuidanceInOfficeAndFamilySurfacesWithoutShellAuthority`.
- Focused integration test passed: `HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome`.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
