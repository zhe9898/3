# Public-Life Order Closure v53-v60: Family Owner-Lane Closure Readback

## Goal

Continue after v46-v52 by closing the Family owner-lane readback loop after public-life/order external after-accounts have already been projected back toward `FamilyCore`.

This pass is projection/readback guidance over existing structured `FamilyCore`, `PopulationAndHouseholds`, `SocialMemoryAndRelations`, public-life, and owner-lane read-model snapshots. It is not a new command system, not an event pool, not a thick Family rule model, not clan elder AI, not a branch-faction system, not a guarantee formula, and not a household repair lane. Zongzu remains a rule-driven command / aftermath / social-memory / readback loop; `DomainEvent` remains one deterministic fact propagation tool, not the design body.

The player-facing read should become:

> This is not asking the ordinary home household to carry the after-account again. Clan elder explanation, household guarantee, lineage-house face, and sponsor-clan pressure must be read back in the Family lane.

## Version Slices

- v53: Family lane entry guidance shows `Family承接入口` when existing local household aftermath and sponsor-clan structure point the after-account back to `FamilyCore`.
- v54: Clan elder explanation readback projects `族老解释读回` from existing Family-owned structured traces.
- v55: Household guarantee / lineage-house face readback projects `本户担保读回` and `宗房脸面读回` from existing Family-owned structured traces.
- v56: Family receipt closure projects `Family后手收口读回` without adding a receipt ledger.
- v57: Later SocialMemory residue readback projects `Family余味续接读回` from structured SocialMemory snapshots only.
- v58: No-loop guidance projects `Family闭环防回压` and `不是普通家户再扛` so stale after-accounts do not point back to `PopulationAndHouseholds`.
- v59: Unity shell and family/public-life/gov surfaces copy projected Family closure fields only.
- v60: Audit lock documents no schema/migration impact, no summary parsing, no manager/controller drift, and no `PersonRegistry` expansion.

## Scope In

- Add runtime read-model fields for Family lane closure guidance in the public-life/order owner-lane and family-facing readback surfaces.
- Populate those fields from existing structured snapshots, such as `ClanSnapshot`, `HouseholdPressureSnapshot`, existing Family owner response traces, sponsor-clan IDs, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State`.
- Surface projected guidance with tokens such as:
  - `Family承接入口`
  - `族老解释读回`
  - `本户担保读回`
  - `宗房脸面读回`
  - `Family后手收口读回`
  - `Family余味续接读回`
  - `Family闭环防回压`
  - `不是普通家户再扛`
- Keep ordinary household local response visible as low-power easing/strain in `PopulationAndHouseholds`.
- Keep durable shame/favor/grudge/obligation residue in `SocialMemoryAndRelations`, written only by later structured monthly read/write logic.
- Add focused integration, architecture, and Unity presentation tests proving the projection-only closure.

## Scope Out

- No persisted state.
- No schema bump, root save version bump, migration, save manifest change, or persisted projection cache.
- No cooldown ledger, owner-lane ledger, Family closure ledger, household target field, guarantee ledger, receipt-status ledger, outcome ledger, command queue, or repeated-response counter.
- No thick clan economy, clan elder AI, lineage-house faction system, or complete guarantee formula.
- No Application command-result authority. Application may route, assemble, and project from existing structured snapshots only.
- No UI/Unity calculation of Family owner-lane归口, guarantee success, clan elder interpretation, lineage-house face repair, or SocialMemory residue.
- No UI/Unity writes to `SocialMemoryAndRelations`.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, SocialMemory summary prose, or Family projection prose.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

## Affected Modules

- `Zongzu.Application`: projection/read-model assembly only. It may add runtime projected fields and read existing structured snapshots, but it must not calculate command outcomes or mutate authority.
- `FamilyCore`: remains owner of clan elder explanation, household guarantee, lineage-house face, and `SponsorClanId` pressure.
- `PopulationAndHouseholds`: remains owner only of ordinary home-household low-power local response state and traces.
- `SocialMemoryAndRelations`: remains owner of durable shame/favor/grudge/obligation residue during later monthly advancement.
- `Zongzu.Presentation.Unity.ViewModels` and `Zongzu.Presentation.Unity`: copy projected fields only.
- Tests and docs: update focused integration, architecture, Unity presentation, schema/no-migration, simulation, boundary, UI, and acceptance evidence.

## Save / Schema Impact

No persisted shape changes are planned.

- `FamilyCore` remains at its current module schema version.
- `PopulationAndHouseholds` remains at its current module schema version.
- `SocialMemoryAndRelations` remains at its current module schema version.
- No root save version change.
- No migration.
- No save manifest update.

If implementation requires a new Family closure ledger, guarantee ledger, owner-lane ledger, household target field, persisted projection cache, SocialMemory field, or other persisted authority, stop before adding it and create a schema/migration plan with owning module schema bump, migration, save roundtrip tests, legacy migration tests, `DATA_SCHEMA.md`, and `SCHEMA_NAMESPACE_RULES.md` updates.

## Determinism Risk

Low. The pass reads deterministic module snapshots and structured SocialMemory cause keys, uses fixed ordering, and adds runtime projection strings. It must not depend on UI text, event summaries, wall-clock time, random choices, receipt prose, or local household response summary prose.

## Milestones

1. Create this ExecPlan.
2. Inspect v46-v52 Office-lane closure implementation and existing Family owner-lane readback fields.
3. Add Family-lane closure runtime fields to read-model contracts and projection helpers.
4. Populate Family entry, clan elder explanation, household guarantee, lineage-house face, receipt closure, residue follow-up, and no-loop guidance from structured snapshots.
5. Copy the fields through Unity-facing ViewModels/adapters.
6. Add focused integration, architecture, and Unity presentation tests.
7. Update docs with v53-v60 boundary and no-schema/no-migration evidence.
8. Run build, focused tests, `git diff --check`, and full solution tests.
9. Commit and push `codex/family-lane-closure-v53-v60`.

## Tests To Add / Update

- Family-facing projected readback after v52 contains Family lane closure guidance.
- At least two directions are covered:
  - clan elder explanation direction with `族老解释读回`;
  - household guarantee / lineage-house face direction with `本户担保读回` and `宗房脸面读回`.
- Command-time local household response still mutates only `PopulationAndHouseholds`.
- Same-month local household response does not mutate `SocialMemoryAndRelations`.
- SocialMemory readers do not parse `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family闭环防回压`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- Unity shell displays projected Family closure fields only.
- Architecture tests guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, and no new schema without migration docs/tests.

## Rollback / Fallback Plan

If Family closure guidance cannot be derived from existing structured snapshots, keep v52 as the stable baseline and stop with a schema/migration impact note. Do not smuggle Family closure state into Application, UI, Unity, SocialMemory prose, or a hidden ledger.

## Open Questions

- None blocking. Default implementation should mirror the v46-v52 Office-lane closure shape, with Family-specific ownership and wording.

## Evidence Checklist

- [x] ExecPlan created
- [x] Family-lane closure read-model fields added
- [x] Family entry / clan elder / guarantee / face / receipt / residue / no-loop projections added
- [x] Unity copy-only fields added
- [x] focused tests added
- [x] docs updated
- [x] no schema/migration impact documented
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] commit and push

## Evidence Notes

- `PlayerCommandReadModels`, `GovernanceReadModels`, Unity command/settlement ViewModels, and adapters now carry runtime-only Family closure fields.
- `PresentationReadModelBuilder` projects `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, `Family闭环防回压`, and `不是普通家户再扛` from structured Family/household/SocialMemory snapshots only.
- Governance Family-lane readback selects clans with structured Family owner traces before prestige fallback, so owner-lane dockets do not misread Family后账 as Office/Order后账.
- Schema/migration impact remains none: no persisted fields, no schema bump, no migration, no save manifest change, no Family closure/guarantee/owner-lane ledger, and no household target field.
- Focused tests run so far:
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter PublicLifeOrderActorCountermoveRuleDrivenTests`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter FirstPassPresentationShellTests`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter ProjectReferenceTests`
- Final validation run:
  - `dotnet build Zongzu.sln --no-restore`
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`
- Commit / push:
  - committed and pushed `codex/family-lane-closure-v53-v60`
