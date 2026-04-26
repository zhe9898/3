# Office/Yamen Readback Spine v38-v45

## Goal

Fold v38 through v45 into one thin backend closure pass after v37 `OfficeAndCareer.PolicyImplemented`.

This pass is projection/readback guidance plus bounded monthly residue. It is not a new command system, event pool, county-yamen formula, clerk AI, policy ledger, owner-lane ledger, cooldown ledger, or thick household economy. Zongzu remains a rule-driven command / aftermath / social-memory / readback loop; `DomainEvent` remains one deterministic fact propagation tool, not the design body.

## Version Slices

- v38: public-life and governance readback for office/yamen policy implementation aftermath.
- v39: later-month `SocialMemoryAndRelations` residue from structured office implementation pressure; no same-month social-memory write from the command/event drain.
- v40: office-lane next affordance guidance for pressing county documents, redirecting reports, cooling, or observing.
- v41: regime/office defection readback, still projected from existing official risk and structured defection metadata.
- v42: canal/route/public-map readback using existing trade routes, public-life road lag, and order route pressure.
- v43: family-facing relief/sponsor readback only as owner-lane guidance; no thick household economy.
- v44: residue-health and no-dead-end readback guards so pressure does not only rise invisibly.
- v45: Unity shell copies projected fields only; no Unity module queries, command rules, or social-memory writes.

## Scope In

- Let `PublicLifeAndRumor` consume structured `OfficeAndCareer.PolicyImplemented` facts and project them into existing public-life fields.
- Let `PublicLifeAndRumor` consume structured `OfficeAndCareer.OfficeDefected` facts when a settlement id is available, projecting public office/regime anxiety into existing fields.
- Add governance read-model fields for office implementation readback, next-step guidance, regime office readback, canal/route readback, and residue-health readback.
- Add office affordance/readback text to existing player-command projections using structured snapshots (`PetitionOutcomeCategory`, pressure/load/backlog/clerk/leverage fields, route snapshots, social-memory snapshots).
- Add monthly SocialMemory residue from office/yamen implementation pressure by reading `JurisdictionAuthoritySnapshot` fields, not event summaries or receipt prose.
- Copy new projected fields through Unity-facing presentation view models/adapters.
- Add focused tests and architecture guards.

## Scope Out

- No new persisted state.
- No schema bump, root save version bump, migration, save manifest change, or persisted projection cache.
- No county-yamen workflow object, implementation ledger, owner-lane ledger, cooldown ledger, household target field, or PersonRegistry expansion.
- No Application command-result authority. Application may compose read models from existing structured snapshots only.
- No UI/Unity calculation of owner lanes, implementation effectiveness, command validity, or social memory.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastPetitionOutcome`, or `LastExplanation` as rule input.

## Ownership Boundaries

- `OfficeAndCareer` owns county-yamen implementation, document landing, clerk drag, and official defection state.
- `PublicLifeAndRumor` owns public-facing heat and county-gate/map projection of already-resolved office facts.
- `SocialMemoryAndRelations` owns durable shame/fear/favor/grudge/obligation residue during later monthly advancement only.
- `TradeAndIndustry` remains owner of trade routes; v42 only reads existing route snapshots for public-map guidance.
- `OrderAndBanditry` remains owner of route pressure, road watch, disorder, and banditry repair.
- `FamilyCore` remains owner of sponsor/clan elder/guarantee repair. v43 only points the player back to that lane.
- Unity copies projected fields; it does not query modules or infer rules.

## Save / Schema Impact

No persisted shape changes are planned.

- `OfficeAndCareer` remains schema `7`.
- `PublicLifeAndRumor` remains schema `4`.
- `SocialMemoryAndRelations` remains schema `3`.
- No root save version change.
- No migration.
- No save manifest update.

If any implementation needs persisted policy, owner-lane, cooldown, route, social-memory, or projection state, stop before adding it and create a schema/migration plan with owning module schema bump, migration, save roundtrip tests, legacy migration tests, `DATA_SCHEMA.md`, and `SCHEMA_NAMESPACE_RULES.md` updates.

## Determinism Risk

Low to medium. The pass reads deterministic module snapshots and structured event metadata, uses fixed ordering, and mutates only owner modules. The added SocialMemory monthly residue must be idempotent by stable cause key and must not depend on UI text, summaries, wall-clock time, or random choices.

## Milestones

1. Create this ExecPlan.
2. Add `PolicyImplemented` and settlement-scoped `OfficeDefected` public-life readback.
3. Add governance read-model summaries for v38-v44.
4. Add office-lane affordance/readback guidance.
5. Add monthly SocialMemory office implementation residue from structured snapshots.
6. Add Unity/view-model copy-only fields.
7. Add focused PublicLife, integration, SocialMemory, architecture, and Unity presentation tests.
8. Update docs with no-schema/no-migration evidence and module-boundary notes.
9. Run build, focused tests, `git diff --check`, full solution tests.
10. Commit and push `codex/office-yamen-readback-v38-v45`.

## Acceptance Notes

- After v37 implementation drag, public-life/governance surfaces should show whether the county gate reads the result as rapid landing, paper compliance, yamen drag, or clerk capture.
- Governance lane/docket should say whether the next useful player-facing entry is pressing county documents, redirecting report paths, cooling, or observing.
- Regime/office readback should be visible without creating a regime formula or defection AI.
- Canal/route guidance should use existing route/order/public-life pressure and remain a projection, not a canal economy.
- Family-facing text should make clear that family relief or elder explanation belongs to `FamilyCore`, not the ordinary household local response lane.
- SocialMemory residue must appear on later monthly advancement from structured office pressure, not from same-month UI/receipt/event summary parsing.
- Unity must display only projected fields.

## Evidence Checklist

- [x] ExecPlan created
- [x] public-life readback added
- [x] governance/read-model summaries added
- [x] office affordance guidance added
- [x] SocialMemory monthly residue added
- [x] Unity copy-only presentation added
- [x] focused tests added
- [x] docs updated
- [x] no schema/migration impact documented
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [ ] commit and push

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused tests passed:
  - `Zongzu.Modules.PublicLifeAndRumor.Tests` policy implementation readback.
  - `Zongzu.Modules.SocialMemoryAndRelations.Tests` office policy implementation residue.
  - `Zongzu.Integration.Tests` office implementation -> governance readback -> next-month SocialMemory residue.
  - `Zongzu.Architecture.Tests` office/yamen projection-only, structured-reader, and Unity copy-only guards.
  - `Zongzu.Presentation.Unity.Tests` projected office/yamen readback field copy.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.

## Implementation Evidence

- `PublicLifeAndRumor` now consumes structured `OfficeAndCareer.PolicyImplemented` and settlement-scoped `OfficeAndCareer.OfficeDefected` readback without parsing event summaries.
- Application governance read models now expose office implementation, next-step, regime/office, route-map, and residue-health summaries as runtime projection fields.
- `SocialMemoryAndRelations` writes `OfficePolicyImplementationResidue` only in a later monthly pass from structured `JurisdictionAuthoritySnapshot` fields and stable cause keys.
- Unity ViewModels/adapters copy projected fields only.
- Schema impact remains none: no new persisted fields, root/module schema bump, migration, ledger, household target field, or save-manifest change.
