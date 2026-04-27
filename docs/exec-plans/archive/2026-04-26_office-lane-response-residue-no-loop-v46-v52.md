# Office-Lane Response / Residue / No-Loop Closure v46-v52

## Goal

Continue after v38-v45 by closing the Office owner-lane readback loop itself.

This pass is projection/readback guidance over existing `OfficeAndCareer` structured fields, public-life read models, and later-month `SocialMemoryAndRelations` residue. It is not a new command system, not an event pool, not a county-yamen formula, not clerk AI, not a policy ledger, and not a thick household or county economy. Zongzu remains a rule-driven command / aftermath / social-memory / readback loop; `DomainEvent` remains one deterministic fact propagation tool, not the design body.

## Version Slices

- v46: Office existing-entry readback explains whether the next entry is `押文催县门`, `改走递报`, `轻催文移`, or `冷却观察`.
- v47: Office receipt closure projects whether the Office after-hand is `已收口`, `仍拖`, `转硬`, or `放置`.
- v48: Office no-loop guard says Office/yamen after-hands stay in the Office lane and must not回压本户.
- v49: Office residue follow-up reads later-month structured SocialMemory residue and names cool/continue/switch/wait guidance without parsing prose.
- v50: Regime/official wavering closure stays Office-owned: official wavering, clerk drag, county face, and document delay do not return to the ordinary household line.
- v51: Unity shell and office surface copy projected Office-lane closure fields only.
- v52: Audit lock: docs and architecture tests confirm no schema, no summary parsing, no authority drift.

## Scope In

- Add runtime read-model fields to governance/office projections for Office-lane entry, receipt closure, residue follow-up, and no-loop guard.
- Populate those fields from existing structured snapshots such as `PetitionOutcomeCategory`, `AdministrativeTaskLoad`, `PetitionBacklog`, `ClerkDependence`, `JurisdictionLeverage`, `LastRefusalResponseCommandCode`, `LastRefusalResponseOutcomeCode`, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State`.
- Surface the fields through governance lane/docket and Office/Unity ViewModels.
- Extend command affordance and receipt readback text with those projected fields.
- Add focused tests proving Office-lane closure appears after v38-v45 readback and owner-response receipts, with same-month SocialMemory neutrality and later-month residue readback.

## Scope Out

- No persisted state.
- No schema bump, root save version bump, migration, save manifest change, or persisted projection cache.
- No county-yamen workflow object, owner-lane ledger, receipt-status ledger, outcome ledger, cooldown ledger, follow-up ledger, household target field, or PersonRegistry expansion.
- No Application command-result authority. Application may compose read models from existing structured snapshots only.
- No UI/Unity calculation of Office outcomes, owner-lane validity, command resolution, or SocialMemory writes.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastPetitionOutcome`, or `LastExplanation` as rule input.

## Ownership Boundaries

- `OfficeAndCareer` owns county-yamen implementation, document landing, clerk drag, official wavering/defection, and Office owner-lane response traces.
- `PublicLifeAndRumor` owns public-facing heat and county-gate/map projection of already-resolved office facts.
- `SocialMemoryAndRelations` owns durable shame/fear/favor/grudge/obligation residue during later monthly advancement only.
- `OrderAndBanditry` remains owner of route pressure, road watch, disorder, and banditry repair.
- `FamilyCore` remains owner of sponsor/clan elder/guarantee repair.
- `PopulationAndHouseholds` remains owner only of low-power home-household response traces.
- Unity copies projected fields; it does not query modules or infer rules.

## Save / Schema Impact

No persisted shape changes are planned.

- `OfficeAndCareer` remains schema `7`.
- `PublicLifeAndRumor` remains schema `4`.
- `SocialMemoryAndRelations` remains schema `3`.
- No root save version change.
- No migration.
- No save manifest update.

If any implementation needs persisted policy, owner-lane, receipt, outcome, cooldown, follow-up, household-target, social-memory, or projection state, stop before adding it and create a schema/migration plan with owning module schema bump, migration, save roundtrip tests, legacy migration tests, `DATA_SCHEMA.md`, and `SCHEMA_NAMESPACE_RULES.md` updates.

## Determinism Risk

Low. The pass reads deterministic module snapshots and structured SocialMemory cause keys, uses fixed ordering, and adds runtime projection strings. It must not depend on UI text, event summaries, wall-clock time, random choices, or receipt prose.

## Milestones

1. Create this ExecPlan.
2. Add Office-lane closure runtime fields to governance/read-model contracts.
3. Populate entry, receipt closure, residue follow-up, and no-loop guard from structured snapshots.
4. Copy the fields through Unity-facing ViewModels/adapters.
5. Add focused integration, architecture, and Unity presentation tests.
6. Update docs with v46-v52 boundary and no-schema/no-migration evidence.
7. Run build, focused tests, `git diff --check`, full solution tests.
8. Commit and push `codex/office-lane-closure-v46-v52`.

## Acceptance Notes

- After v38-v45 Office/yamen readback, governance and Office surfaces should say what existing Office entry is useful next.
- After an Office owner-lane response receipt, readback should say whether the Office after-hand收口, still拖,转硬, or放置.
- Office closure should explicitly say not to回压本户 once the after-account belongs to Office.
- Later SocialMemory residue may be shown only from structured residue snapshots and stable cause keys.
- Unity must display projected Office-lane closure fields only.

## Evidence Checklist

- [x] ExecPlan created
- [x] Office-lane closure read-model fields added
- [x] Office entry / receipt / residue / no-loop projections added
- [x] Unity copy-only fields added
- [x] focused tests added
- [x] docs updated
- [x] no schema/migration impact documented
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] commit and push

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration tests passed: `OfficeCourtRegimePressureChainTests` and `PublicLifeOrderActorCountermoveRuleDrivenTests` (10 tests).
- Focused architecture tests passed: `ProjectReferenceTests` suite (35 tests).
- Focused Unity presentation tests passed: `FirstPassPresentationShellTests` (29 tests).
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed across the solution.
