# Backend Office/Yamen Implementation Drag Thin Chain v37

## Goal

- Deepen the next priority from `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`: court and county pressure should pass through Office/Yamen documents, clerks, backlog, and local leverage before it reads as implementation.
- Keep Zongzu rules-driven. `DomainEvent` records and transfers resolved facts after owner rules run; it is not an event pool and not the design body.
- Let `OfficeAndCareer` convert an existing `PolicyWindowOpened` fact into an office-owned implementation outcome using existing jurisdiction / career fields.

## Scope In

- `OfficeAndCareer` publishes the already-declared `OfficeAndCareer.PolicyImplemented` fact.
- `OfficeAndCareer` consumes its own `PolicyWindowOpened` fact through the deterministic month-end drain.
- The implementation profile uses existing structured metadata and existing office state:
  - `AuthorityTier`
  - `JurisdictionLeverage`
  - `ClerkDependence`
  - `PetitionPressure`
  - `PetitionBacklog`
  - `AdministrativeTaskLoad`
  - `PolicyWindow*` metadata
- `OfficeAndCareer` mutates only existing office-owned fields on matching-settlement appointed officials, such as `CurrentAdministrativeTask`, `AdministrativeTaskLoad`, `PetitionBacklog`, `PetitionPressure`, `ClerkDependence`, `JurisdictionLeverage`, `PromotionMomentum`, `DemotionPressure`, `LastPetitionOutcome`, and `LastExplanation`.
- Tests prove implementation outcome directions, off-scope no-touch, same-month scheduler drain, metadata-only rules, and no schema/version change.

## Scope Out

- No thick county-yamen formula, clerk AI, memorial queue, court-process state, county office workflow, policy ledger, owner-lane ledger, cooldown ledger, or UI command lane.
- No new persisted state, module schema bump, root save bump, migration, save-manifest change, or persisted projection cache.
- No Application/UI/Unity authority; no Unity module queries; no UI calculation of yamen implementation effectiveness.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

## Affected Modules

- `OfficeAndCareer`: owns policy-window implementation drag and response through existing office state.
- `Zongzu.Contracts`: may add runtime metadata constants only; no persisted fields.
- `tests`: focused `OfficeAndCareer`, scheduler integration, and architecture guards.
- `docs`: topology, boundaries, schema/no-migration, simulation, UI/presentation, acceptance, audit, and skill matrix evidence.

## Save / Schema Impact

- V37 targets no persisted shape change. `OfficeAndCareer` remains schema `7`.
- New metadata constants, if added, are runtime `DomainEvent` payload keys only. They are not save fields, indexes, ledgers, workflow state, or module envelopes.
- If implementation requires a new implementation ledger, policy state, docket workflow object, owner-lane ledger, or persisted cache, stop and convert this into a schema/migration plan before code changes.

## Determinism Risk

- Low. The handler uses event type/entity/metadata, matching-settlement office state, deterministic sorting, fixed profile formulas, and existing scheduler drain order.
- It does not use wall-clock time, random choices, summary text, UI state, external services, or cross-module direct mutation.

## Milestones

1. Add this ExecPlan with no-schema target.
2. Add `OfficeAndCareer.PolicyImplemented` to published events and `PolicyWindowOpened` to consumed events.
3. Implement matching-settlement policy implementation profile and owner-state mutation inside `OfficeAndCareer`.
4. Add focused module tests for dragged, captured / paper-compliance directions and off-scope no-touch.
5. Add real scheduler proof that court agenda drains into policy window and policy implementation in the same month.
6. Add architecture guards for no summary parsing, no schema drift, and no Application/UI authority.
7. Update docs and run validation.
8. Commit and push `codex/backend-office-yamen-drag-chain-v37`.

## Tests To Add / Update

- `PolicyWindowOpened_DraggedImplementation_UpdatesMatchingOfficeStateAndEmitsPolicyImplemented`
- `PolicyWindowOpened_HighClerkCapture_EmitsCapturedImplementation`
- `PolicyWindowOpened_OffScopeSettlement_DoesNotMutateOtherJurisdiction`
- `Chain8_RealScheduler_CourtAgendaDrainsIntoOnePolicyImplementation`
- Architecture guard: Office policy implementation handler uses structured metadata / office state and must not parse `DomainEvent.Summary`, receipt prose, or response summaries.
- Event-contract health classification updated for `OfficeAndCareer.PolicyImplemented` if the diagnostic debt table sees it as emitted without downstream authority consumers or declared but not emitted.

## Rollback / Fallback Plan

- Revert the `OfficeAndCareer` handler and test additions.
- Keep `PolicyImplemented` as declared future contract if implementation cannot stay inside existing Office state.
- If new persisted state becomes necessary, stop this plan and create a schema/migration ExecPlan before any state-bearing code is committed.

## Open Questions

- Whether a later pass should let `PublicLifeAndRumor` consume `PolicyImplemented` for public-facing heat. V37 keeps that downstream public projection out of scope unless tests reveal an existing required contract.

## Evidence Checklist

- [x] ExecPlan created
- [x] PolicyImplemented published / PolicyWindowOpened consumed
- [x] office-owned implementation handler added
- [x] focused OfficeAndCareer tests passed
- [x] focused scheduler integration test passed
- [x] focused architecture test passed
- [x] docs updated
- [x] no schema/migration impact documented in docs
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [ ] commit and push
