# Public-Life Order Closure v24: Owner-Lane Outcome Reading Guidance

## Goal

After v20-v23 have kept the home-household response lane bounded and returned external after-accounts to their owning lanes, v24 adds a small readback layer for how to read an existing owner-lane outcome after归口.

The intended player read is:

- the home household can only缓住 / 暂压 / 吃紧 / 放置 its own side;
- v20-v22 say unresolved external after-accounts should go back to Order / Office / Family lanes;
- v23 says the after-account has a structured owner-lane归口 status when an owner response trace already exists;
- v24 says what the existing owner-lane outcome means in plain readback wording:
  - `已修复：先停本户加压`,
  - `暂压留账：仍看本 lane 下月`,
  - `恶化转硬：别让本户代扛`,
  - `放置未接：仍回 owner lane`.

This is projection/readback guidance only. It is not a new command system, not an event pool, not an event chain, not a handoff system, not a receipt-status ledger, not thick household economy, not county-yamen formula work, not runner faction AI, and not patrol AI. `DomainEvent` remains only one fact-propagation tool inside the rule-driven command / aftermath / social-memory / readback loop.

## Scope In

- Add projected `归口后读法` wording to owner-lane return status guidance.
- Render only from existing structured owner-lane outcome codes:
  - `PublicLifeOrderResponseOutcomeCodes.Repaired`,
  - `Contained`,
  - `Escalated`,
  - `Ignored`.
- Keep the guidance attached to existing v23 status surfaces for Order, Office, and Family lanes.
- Keep the wording lane-neutral enough to avoid implying a new queue, but concrete enough for the player to understand whether the household should stop carrying the after-account.
- Ensure public-life, governance docket, family-facing, and Unity-shell surfaces copy projected fields only.

## Scope Out

- No new command family.
- No command queue, ranking, or recommendation state.
- No owner-lane ledger, receipt-status ledger, cooldown ledger, or household target field.
- No new persisted state.
- No schema bump or migration.
- No Application-owned command result formula.
- No UI/Unity owner-lane computation, module query, command execution, or SocialMemory write.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastRefusalResponseSummary`, or `LastLocalResponseSummary`.

## Affected Modules

- `Zongzu.Application`: projection/read-model wording only.
- `Zongzu.Contracts`: no shape change expected.
- `Zongzu.Presentation.Unity`: no authority change; existing ViewModel fields should carry the projected text.
- Tests:
  - integration coverage for Order plus Office/Family owner-lane outcome reading,
  - architecture coverage for projection-only / no-summary-parsing / no-schema drift,
  - Unity presentation copy-only coverage.
- Docs:
  - skill matrix, alignment audit, module boundaries, integration rules, social memory docs, schema docs, simulation docs, UI docs, acceptance tests.

## Save/Schema Impact

No persisted state target.

Expected impact: none. v24 should only map existing owner-module outcome codes to deterministic readback text in runtime projections. If implementation discovers a need for persisted归口结果读法 state, stop before code changes and document:

- owning module schema version bump,
- migration,
- save roundtrip and legacy migration tests,
- `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md` updates,
- why projection-only outcome reading was insufficient.

## Determinism Risk

Low. The projection is deterministic over existing owner-lane outcome codes and existing household local-response read models. It does not alter scheduler flow, command resolution, module mutation, event emission, replay hash authority, or save payloads.

## Milestones

1. Confirm v23 owner-lane status helper is the correct projection point.
2. Add `归口后读法` rendering from existing owner-lane outcome codes.
3. Update integration tests for at least two outcome meanings after a home-household local response and owner-lane responses.
4. Update architecture tests to keep the helper projection-only and no-schema/no-summary-parsing.
5. Update Unity presentation tests to prove copy-only display.
6. Update docs and record no schema/migration impact.
7. Run build, focused tests, `git diff --check`, and full tests.
8. Commit and push `codex/public-life-order-leverage-v3`.

## Tests To Add/Update

- `PublicLifeOrderRefusalResponseRuleDrivenTests`
  - after `SendHouseholdRoadMessage` local response and existing owner-lane responses, projected readback includes:
    - `归口后读法`,
    - `已修复`,
    - `先停本户加压`,
    - at least one non-repaired meaning such as `恶化转硬` or `暂压留账`.
  - command-time local response still mutates only `PopulationAndHouseholds`.
  - same-month local response still does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`.
- Architecture tests
  - the owner-lane helper must not reference state mutation, schema/migration, command routing, summaries, or SocialMemory writers.
  - the helper must contain `归口后读法` tokens and read only structured owner snapshot outcome codes.
- Unity presentation tests
  - Office and Family shell surfaces copy `归口后读法` guidance from existing DTO fields only.

## Rollback / Fallback Plan

If outcome wording risks implying automatic repair or a new owner-lane workflow, revert to v23 `归口状态` guidance and document v24 as blocked until the shell has a safer projection-only wording slot. Do not add state as a workaround.

## Open Questions

- Whether future shell surfaces should render outcome reading as a small lane stamp, docket margin note, or plain inline readback. Default for v24: plain projected text inside existing summary fields only.
- Whether `Ignored` should hide the v23 `已归口` wording. Default for v24: no; the owner lane may have received a structured trace and still left it放置, so the readback must say `放置未接：仍回 owner lane`.

## Implementation Evidence

- Extended `PresentationReadModelBuilder.PlayerCommands.OwnerLaneReturnSurface.cs` with `BuildOwnerLaneOutcomeReading`.
- The helper maps only existing `PublicLifeOrderResponseOutcomeCodes.Repaired`, `Contained`, `Escalated`, and `Ignored` values to projected `归口后读法` text.
- Order / Office / Family status guidance now appends outcome reading with `JoinOwnerLaneReturnSurfaceText` after v23 `归口状态`.
- No command resolver, command request shape, scheduler phase, module state class, schema version, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, household target field, `PersonRegistry`, SocialMemory writer, or Unity authority path was added.
- Integration coverage now proves the same scenario can read an Order repaired outcome as `已修复：先停本户加压` and an Office escalated outcome as `恶化转硬：别让本户代扛`.
- Architecture coverage now guards that the helper does not parse `DomainEvent.Summary`, `LastInterventionSummary`, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, or SocialMemory state, and that the SocialMemory local-response reader does not parse v24 projection tokens.
- Unity presentation coverage continues to prove shell surfaces copy projected DTO fields only.
- Docs updated: `CODEX_SKILL_RATIONALIZATION_MATRIX.md`, `DESIGN_CODE_ALIGNMENT_AUDIT.md`, `MODULE_BOUNDARIES.md`, `MODULE_INTEGRATION_RULES.md`, `RELATIONSHIPS_AND_GRUDGES.md`, `DATA_SCHEMA.md`, `SCHEMA_NAMESPACE_RULES.md`, `SIMULATION.md`, `UI_AND_PRESENTATION.md`, and `ACCEPTANCE_TESTS.md`.
- Schema/migration impact explicitly remains none.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed.
- Focused integration test passed: `HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome`.
- Focused architecture tests passed: `Owner_lane_return_surface_readback_must_stay_projection_only` and `Home_household_local_response_social_memory_reader_must_use_structured_population_aftermath_only`.
- Focused Unity presentation test passed: `Compose_ProjectsOwnerLaneReturnGuidanceInOfficeAndFamilySurfacesWithoutShellAuthority`.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
