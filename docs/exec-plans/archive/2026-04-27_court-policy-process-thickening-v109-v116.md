# Court Policy Process Thickening V109-V116

## Goal

Add the first rule-density layer for Chain 8 court-policy process readback:

`WorldSettlements.CourtAgendaPressureAccumulated -> OfficeAndCareer.PolicyWindowOpened -> OfficeAndCareer.PolicyImplemented -> PublicLifeAndRumor`

This pass moves the player-facing surfaces from thin readback guidance toward a visible process: court pressure has policy tone, document direction, county-gate implementation posture, and public interpretation. It remains a rule-driven command / aftermath / social-memory / readback loop. `DomainEvent` records are fact propagation tools, not the design body.

## Non-Goals

- No full Court module, court engine, faction AI, or full policy economy.
- No event pool, dispatch ledger, policy ledger, court-process ledger, owner-lane ledger, or cooldown ledger.
- No Application-layer policy success calculation.
- No UI or Unity derivation of court-policy outcomes.
- No parsing of `DomainEvent.Summary`, receipt prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, or `LastLocalResponseSummary`.
- No `PersonRegistry` expansion.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or equivalent authority object.

## Owner Lanes

- `WorldSettlements` owns the court agenda / mandate pressure source.
- `OfficeAndCareer` owns policy window selection, county-gate execution, document handoff, and implementation posture.
- `PublicLifeAndRumor` owns public interpretation, notice visibility, and street reading.
- `SocialMemoryAndRelations` may only write durable residue during later monthly advancement from structured aftermath.
- `Zongzu.Application` routes, assembles, and projects existing structured fields.
- `Zongzu.Presentation.Unity` copies projected fields only.

## Schema And Migration

Target impact: none.

This pass must not add persisted module fields or raise module schema versions. It may reuse existing runtime metadata, query snapshots, read models, and projection text fields. If implementation requires new persisted state, stop before making that change and document the required schema, namespace, migration, save compatibility, and tests.

Current expected schema versions stay:

- `WorldSettlements` schema 8
- `OfficeAndCareer` schema 7
- `PublicLifeAndRumor` schema 4
- `SocialMemoryAndRelations` schema 3

## Implementation Plan

1. Create this ExecPlan and keep it as the evidence anchor for v109-v116.
2. Reuse `PolicyImplemented` structured metadata and existing `JurisdictionAuthoritySnapshot` / `SettlementPublicLifeSnapshot` scalar fields.
3. Thickening tokens to surface through existing court-policy readback fields:
   - `政策语气读回`
   - `文移指向读回`
   - `县门承接姿态`
   - `公议承压读法`
   - `朝廷后手仍不直写地方`
   - `不是本户硬扛朝廷后账`
4. Keep `Application` readback helpers projection-only.
5. Let `PublicLifeAndRumor` convert structured `PolicyImplemented` metadata into public interpretation prose and scalar pressure shifts inside its owner lane.
6. Keep Unity unchanged unless tests reveal a copy-only field gap.

## Validation Plan

- Focused integration proof:
  - court agenda pressure opens one policy window;
  - policy implementation produces structured first-layer process readback;
  - public-life reads structured public interpretation.
- Architecture guards:
  - no Application/UI/Unity authority drift;
  - no summary/prose parsing;
  - no forbidden manager/god-controller names;
  - no `PersonRegistry` expansion;
  - no new schema without migration docs/tests.
- SocialMemory guards:
  - same-month handling does not write durable residue;
  - future residue reader does not parse projection prose.
- Unity presentation proof:
  - shell copies projected court-policy thickening fields only.
- Final commands:
  - `dotnet build Zongzu.sln --no-restore`
  - focused tests for integration, architecture, SocialMemory, and Unity presentation
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`

## Evidence Log

- 2026-04-27: Read required skill workflows and local references for game design, architecture boundaries, pressure chains, save/schema, simulation validation, UI shell, content authoring, ancient China grounding, and Unity shell.
- 2026-04-27: Read required repo docs plus active v93-v108 ExecPlans.
- 2026-04-27: Initial code map found existing structured `PolicyImplemented` metadata and public-life scalar readback, so the intended implementation remains schema-neutral.
- 2026-04-27: Implemented Application court-policy first-layer readback helpers from structured `JurisdictionAuthoritySnapshot` / `SettlementPublicLifeSnapshot` fields only.
- 2026-04-27: Implemented PublicLifeAndRumor owner-lane policy-process wording from structured `PolicyImplemented` metadata, preserving existing `县门执行读回` / `本户不能代修` compatibility while adding v109-v116 tokens.
- 2026-04-27: Added integration, architecture, SocialMemory, PublicLife, and Unity presentation assertions for process readback, no prose parsing, no same-month durable residue, schema neutrality, and copy-only presentation.
- 2026-04-27: Updated required permanent docs and recorded schema/migration impact as none.
- 2026-04-27: Validation passed: `dotnet build Zongzu.sln --no-restore`; focused integration / architecture / SocialMemory / Unity tests; `git diff --check`; `dotnet test Zongzu.sln --no-build`.
