# Court Policy Process Readback v93-v100

Date: 2026-04-26
Branch: `codex/court-policy-process-readback-v93-v100`

## Intent

Continue the global skeleton after v85-v92 by making the existing thin court-policy process legible on player-facing governance and public-life surfaces. When court agenda pressure opens a policy window, and that window becomes county/yamen implementation plus public interpretation, the player should read the process as court pressure -> policy window ->文移到达 ->县门执行 ->公议读法, not as an Office-only receipt and not as ordinary households carrying imperial after-accounting.

This is projection/readback guidance over existing structured snapshots. It is not a new command system, not an event pool, not a thick court engine, not a policy formula, not a dispatch ledger, and not a second Application rule layer. `DomainEvent` remains one fact-propagation tool, not the design body.

## Scope

- Reuse the existing thin chain:
  - `WorldSettlements.CourtAgendaPressureAccumulated`
  - `OfficeAndCareer.PolicyWindowOpened`
  - `OfficeAndCareer.PolicyImplemented`
  - `PublicLifeAndRumor` structured dispatch/public interpretation snapshots
- Add projected court-policy process readback fields to governance, owner-lane docket, Office-facing view models, and Unity shell copies:
  - `朝议压力读回`
  - `政策窗口读回`
  - `文移到达读回`
  - `县门执行承接读回`
  - `公议读法读回`
  - `Court后手不直写地方`
  - `Office/PublicLife分读`
  - `不是本户也不是县门独吞朝廷后账`
  - `Court-policy防回压`
- Build wording from structured `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` fields only.
- Unity shell displays projected court-policy fields only.

## Non-Goals

- No new persisted state.
- No schema or migration impact.
- No court module, court AI, faction AI, memorial queue state, dispatch ledger, cooldown ledger, owner-lane ledger, household target field, policy closure ledger, or policy success formula.
- No Application-owned command-result calculation.
- No UI/Unity calculation of court-policy, county execution, or public interpretation outcomes.
- No UI/Unity writes to `SocialMemoryAndRelations`.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastAdministrativeTrace`, `LastPetitionOutcome`, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, or SocialMemory summary prose.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

If a new persisted field appears necessary, stop before implementing it and document schema namespace, migration, save manifest, and acceptance-test impact first.

## Version Slices

- v93: ExecPlan and schema-neutral court-policy process target.
- v94: Contract/read-model fields expose projected court-policy readback summaries.
- v95: Application projection composes court-policy process readback from structured Office/PublicLife snapshots.
- v96: Governance/public-life docket separates court-policy process from Office-only and household after-accounts.
- v97: Unity shell copies projected court-policy fields only.
- v98: Integration and Unity tests prove process readback and copy-only display.
- v99: Architecture tests guard no authority drift, no summary parsing, no forbidden managers, no PersonRegistry expansion, and no schema drift.
- v100: Docs, evidence, full validation, commit, and push.

## Ownership

- `WorldSettlements` owns mandate/court pressure facts and source pressure accumulation.
- `OfficeAndCareer` owns policy windows, county/yamen implementation, officials, clerks, administrative load, and petition outcomes.
- `PublicLifeAndRumor` owns public notice/dispatch/public interpretation fields.
- `PopulationAndHouseholds` owns ordinary household local response only; it does not absorb court-policy after-accounting.
- `SocialMemoryAndRelations` owns durable residue only through later monthly progression from structured aftermath and cause keys.
- `Application` routes and assembles projected read models; it may select from structured snapshots but does not compute policy outcomes.
- `Unity` copies projected readback fields and renders them; it does not derive court-policy results from notifications or prose.

## Schema / Migration Impact

Target impact: none.

This pass uses existing module state, existing event metadata, and existing query snapshots. Added read-model and Unity view-model strings are runtime projection fields only. No root save version, module schema version, migration, manifest, serialized payload shape, persisted projection cache, or ledger should change.

## Evidence Targets

- `dotnet build Zongzu.sln --no-restore`
- Focused integration tests for court-policy process readback and module isolation.
- Focused architecture guard for schema-neutral owner-lane discipline.
- Focused Unity presentation test for projected court-policy display.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Evidence Checklist

- [x] ExecPlan created.
- [x] Contract/read-model court-policy projection fields added.
- [x] Application court-policy process readback implemented from existing structured fields.
- [x] Unity court-policy display changed to projected snapshot use.
- [x] Focused tests added/updated.
- [x] Required docs updated.
- [x] No schema/migration impact documented.
- [x] Build passed.
- [x] Focused tests passed.
- [x] `git diff --check` passed.
- [x] Full solution tests passed.
- [ ] Commit and push completed.

## Evidence Results

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration tests passed: `OfficeCourtRegimePressureChainTests`.
- Focused architecture tests passed: `Office_yamen_readback_spine_must_stay_projection_only_and_schema_neutral` and `Unity_office_yamen_readback_must_copy_projected_fields_only`.
- Focused Unity presentation test passed: `Compose_CopiesOfficeYamenReadbackSpineWithoutShellAuthority`.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
