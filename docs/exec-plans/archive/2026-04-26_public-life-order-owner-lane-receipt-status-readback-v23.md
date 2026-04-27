# Public-Life Order Closure v23: Owner-Lane Receipt Status Readback

## Goal

After v20-v22 have said that a home-household local response can only缓住 / 暂压 / 吃紧 / 放置 its own side, v23 adds one further projection/readback cue: when an existing owner lane has already received a structured response trace, the player-facing surface should say that the external after-account has been归口 to that owner lane.

The intended player read is:

- the home household has reached the limit of its low-power local response,
- v20-v22 still point the unresolved external after-account back to Order / Office / Family lanes,
- if an existing owner response such as 补保巡丁, 押文催县门, or 请族老解释 has already resolved, the surface can say `归口状态`,
- `已归口` is not `已修好`; the outcome and later residue still come from the owner lane and later SocialMemory readback.

This is not "被社会其他人接手". It is owner-lane归口 status inside the rule-driven command / aftermath / social-memory / readback loop. It is not an event chain, not an event pool, not a new command system, not a handoff ledger, not a thick household economy, not county-yamen formula work, not runner faction AI, and not patrol AI. `DomainEvent` remains only one fact-propagation tool.

## Scope In

- Add projected `归口状态` wording to owner-lane return surfaces when existing structured owner response traces are visible.
- Keep lane-specific text:
  - Order lane: `已归口到巡丁/路匪 lane`.
  - Office lane: `已归口到县门/文移 lane`.
  - Family lane: `已归口到族老/担保 lane`.
- Include `归口不等于修好` and `仍看 owner lane 下月读回` so the guidance cannot be mistaken for an automatic repair.
- Use existing structured read models only:
  - `HouseholdPressureSnapshot.LastLocalResponse*` to know there was a home-household local response.
  - `SettlementDisorderSnapshot.LastRefusalResponse*` for Order status.
  - `JurisdictionAuthoritySnapshot.LastRefusalResponse*` for Office status.
  - `ClanSnapshot.LastRefusalResponse*` for Family status.
- Ensure public-life, governance docket, office-facing, family-facing, and Unity-shell surfaces copy projected text only.

## Scope Out

- No new command family.
- No command queue or command recommendation state.
- No owner-lane ledger or receipt-status ledger.
- No new persisted state.
- No schema bump or migration.
- No cooldown ledger, household target field, or command-target expansion.
- No Application-owned command outcome formula.
- No UI/Unity owner-lane computation, module query, command execution, or SocialMemory write.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastRefusalResponseSummary`, or `LastLocalResponseSummary`.

## Affected Modules

- `Zongzu.Application`: projection/read-model guidance only.
- `Zongzu.Contracts`: no shape change expected.
- `Zongzu.Presentation.Unity`: no authority change; tests prove existing ViewModel fields copy the projected text.
- Tests:
  - integration coverage for Order and Office/Family owner-lane receipt-status readback,
  - architecture coverage for projection-only / no-summary-parsing / no-schema drift,
  - Unity presentation copy-only coverage.
- Docs:
  - skill matrix, alignment audit, module boundaries, integration rules, social memory docs, schema docs, simulation docs, UI docs, acceptance tests.

## Save/Schema Impact

No persisted state target.

Expected impact: none. v23 should only extend runtime projection wording over existing owner-module response trace fields. If implementation discovers a need for persisted归口 state, stop before code changes and document:

- owning module schema version bump,
- migration,
- save roundtrip and legacy migration tests,
- `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md` updates,
- why projection-only status readback was insufficient.

## Determinism Risk

Low. The projection is deterministic over existing household and owner-module read models. It does not alter scheduler flow, command resolution, module mutation, event emission, replay hash authority, or save payloads.

## Milestones

1. Confirm v22 owner-lane helper is the correct projection point.
2. Add lane-specific `归口状态` readback from existing structured owner response traces.
3. Update integration tests for Order plus Office/Family status surfaces after a home-household response.
4. Update architecture tests to keep the helper projection-only and no-schema/no-summary-parsing.
5. Update Unity presentation tests to prove copy-only display.
6. Update docs and record no schema/migration impact.
7. Run build, focused tests, `git diff --check`, and full tests.
8. Commit and push `codex/public-life-order-leverage-v3`.

## Tests To Add/Update

- `PublicLifeOrderRefusalResponseRuleDrivenTests`
  - after `SendHouseholdRoadMessage` local response and existing owner-lane responses, projected readback includes:
    - `归口状态`,
    - `已归口到巡丁/路匪 lane`,
    - `已归口到县门/文移 lane` or `已归口到族老/担保 lane`,
    - `归口不等于修好`,
    - `仍看 owner lane 下月读回`.
  - command-time local response still mutates only `PopulationAndHouseholds`.
  - same-month local response still does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`.
- Architecture tests
  - the owner-lane helper must not reference state mutation, schema/migration, command routing, summaries, or SocialMemory writers.
  - the helper must contain the projected `归口状态` tokens and structured owner snapshot types.
- Unity presentation tests
  - Office and Family shell surfaces copy `归口状态` guidance from existing DTO fields only.

## Rollback / Fallback Plan

If status wording risks implying automatic repair or a new owner-lane workflow, revert to v22 `承接入口` guidance and document v23 as blocked until the shell has a safer projection-only wording slot. Do not add state as a workaround.

## Open Questions

- Whether future shell surfaces should render `归口状态` as a small lane stamp, docket margin note, or plain readback text. Default for v23: plain projected text inside existing summary fields only.
- Whether `已归口` should be hidden when the owner lane outcome is `Ignored`. Default for v23: still show归口 status because the owner lane did receive the structured trace; the outcome text and `归口不等于修好` explain that it may remain unresolved.

## Implementation Evidence

- Extended `PresentationReadModelBuilder.PlayerCommands.OwnerLaneReturnSurface.cs` with lane-specific `归口状态` helpers.
- Order status reads only `SettlementDisorderSnapshot.LastRefusalResponseCommandCode`, label, and outcome code for `补保巡丁` / `赔脚户误读` / `暂缓强压`.
- Office status reads only `JurisdictionAuthoritySnapshot.LastRefusalResponseCommandCode`, label, and outcome code for `押文催县门` / `改走递报`.
- Family status reads only `ClanSnapshot.LastRefusalResponseCommandCode`, label, and outcome code for `请族老解释`.
- Public-life, Office/Governance, and Family-facing affordances now join v20-v22 owner-lane guidance with v23 status guidance when both a structured home-household local response and a structured owner response are present.
- Governance docket now receives existing jurisdiction snapshots so it can copy the same Office status guidance without reading receipts or parsing prose.
- Unity presentation tests continue to prove shell copy-only behavior through existing DTO fields.
- No command request shape, command queue, owner-lane ledger, receipt-status ledger, cooldown ledger, household target field, schema version, migration, `PersonRegistry` expansion, SocialMemory field, or Unity authority path was added.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed.
- Focused integration test passed: `HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome`.
- Focused architecture test passed: `Owner_lane_return_surface_readback_must_stay_projection_only`.
- Focused Unity presentation test passed: `Compose_ProjectsOwnerLaneReturnGuidanceInOfficeAndFamilySurfacesWithoutShellAuthority`.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
