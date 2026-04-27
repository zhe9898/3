# Court Policy Local Response Affordance v117-v124

## Goal

Add the next bounded layer after Chain 8 v109-v116: court-policy pressure that already reads as policy tone, document direction, county-gate posture, and public interpretation should now surface a narrow local response affordance.

The target player-facing read is:

> This policy pressure has arrived through documents, county posture, and public reading; the player can lightly continue the local document/report path, but OfficeAndCareer and PublicLifeAndRumor still own the result.

This remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` remains a fact propagation tool, not the design body.

## Scope In

- Reuse the existing Chain 8 path:
  `WorldSettlements.CourtAgendaPressureAccumulated -> OfficeAndCareer.PolicyWindowOpened -> OfficeAndCareer.PolicyImplemented -> PublicLifeAndRumor`.
- Reuse existing command names and routes where possible:
  `PressCountyYamenDocument`, `RedirectRoadReport`, `PostCountyNotice`, `DispatchRoadReport`.
- Add policy-process affordance/readback language on governance, office, docket, and public-life command surfaces:
  `政策回应入口`, `文移续接选择`, `县门轻催`, `递报改道`, `公议降温只读回`, `不是本户硬扛朝廷后账`.
- Keep command resolution inside `OfficeAndCareer` for county document/report handling.
- Keep public interpretation in `PublicLifeAndRumor` and projected read models.
- Update focused tests and docs.

## Scope Out

- No Court module.
- No full court engine, faction AI, full policy economy, event pool, or scripted court process.
- No dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, response cooldown ledger, or policy closure ledger.
- No new player omnipotent court button and no direct emperor/court control.
- No Application calculation of policy success.
- No UI/Unity derivation of court-policy outcomes.
- No parsing of `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, or god-controller path.

## Affected Modules

- `OfficeAndCareer`: owns the bounded county document/report command resolution when policy-process pressure is visible through existing office scalar state.
- `PublicLifeAndRumor`: continues to own notice visibility, street reading, dispatch pressure, and public interpretation.
- `WorldSettlements`: remains the court agenda / mandate pressure source only.
- `SocialMemoryAndRelations`: remains a later-month durable residue reader from structured aftermath only; no same-month durable write is added.
- `Application`: routes, assembles, and projects command affordances/readbacks only.
- `Zongzu.Presentation.Unity`: copies projected fields only.

## Save/Schema Impact

Target impact: none.

No persisted state, root schema version, module schema version, migration, save manifest, or feature-pack membership should change. The implementation must reuse existing `OfficeAndCareer` state fields and existing runtime read-model fields.

If implementation proves a new persisted field is required, stop and document the schema/migration impact before adding it.

## Determinism Risk

Low. The new path should use deterministic existing scalar fields in `OfficeCareerState` / `JurisdictionAuthoritySnapshot` and stable command routing. No randomness, wall-clock access, IO, broad fanout, or scheduler reordering is intended.

## Milestones

1. Confirm v109-v116 owner lanes and existing command/readback code.
2. Add local response affordance readback in Application using structured snapshots only.
3. Make `OfficeAndCareer` resolve existing document/report commands against visible policy-process pressure when no order aftermath exists.
4. Add focused tests for affordance projection, owner-lane command resolution, no prose parsing, no schema drift, and Unity copy-only display.
5. Update topology, boundary, schema, UI, simulation, acceptance, audit, and skill-rationalization docs.
6. Run focused validation, `dotnet build Zongzu.sln --no-restore`, `git diff --check`, and `dotnet test Zongzu.sln --no-build`.

## Tests To Add/Update

- Integration:
  - court agenda pressure opens a policy window and projects first-layer local response affordances.
  - `PressCountyYamenDocument` / `RedirectRoadReport` can resolve through `OfficeAndCareer` from policy-process pressure without order aftermath.
  - unaffected settlements remain untouched.
- Architecture:
  - no Application/UI/Unity authority drift.
  - no summary/prose parsing.
  - no forbidden managers/controllers, no `PersonRegistry` expansion, no new schema without docs/tests.
- SocialMemory:
  - same-month policy local response does not write durable residue.
  - future readers must not parse projection prose.
- Unity presentation:
  - shell copies projected court-policy local-response command/readback fields only.

## Rollback / Fallback Plan

If the command-resolution layer proves too thick for this pass, keep only projection-side affordance guidance and explicitly mark owner-command resolution as deferred. If any required durable state appears, stop before adding it and replace this plan with a schema-aware follow-up.

## Open Questions

- Whether `PressCountyYamenDocument` should be the primary suggested command ahead of `RedirectRoadReport` whenever court-policy process pressure is visible and there is no order aftermath. Initial plan: yes, because it is the closest existing county-gate document continuation.
- Whether later SocialMemory residue should distinguish order-response office residue from policy-local-response office residue. Initial plan: no new durable category in v117-v124; reuse existing structured office receipt fields and keep durable residue changes out of scope.

## Evidence Log

- 2026-04-27: Confirmed v109-v116 owner lanes and existing court-policy readback fields before adding the local-response layer.
- 2026-04-27: Implemented Application command affordance/readback guidance from structured `JurisdictionAuthoritySnapshot` / `SettlementPublicLifeSnapshot` fields only.
- 2026-04-27: Implemented `OfficeAndCareer` handling for existing `PressCountyYamenDocument` and `RedirectRoadReport` commands when policy-process pressure is visible through existing office scalar state and no order aftermath is required.
- 2026-04-27: Added focused Office, integration, architecture, and Unity presentation tests covering command resolution, owner-lane projection, no prose parsing, same-month SocialMemory non-write, schema neutrality, and copy-only Unity display.
- 2026-04-27: Updated topology, boundary, integration, schema, simulation, UI, acceptance, audit, and skill-rationalization docs. Schema/migration impact remains none.
- 2026-04-27: Validation passed: `dotnet build Zongzu.sln --no-restore`; focused Office/integration/architecture/Unity tests; `git diff --check`; `dotnet test Zongzu.sln --no-build`.
