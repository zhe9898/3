# Public-Life Order Closure v25: Owner-Lane Outcome Social Residue Readback

## Goal
- Add projected/readback guidance for owner-lane public-life order outcomes after the later SocialMemory monthly pass has made durable residue visible.
- Keep the player-facing loop clear: the home-household response can only缓住/暂压/吃紧/放置 its own side; owner-lane commands can repair/contain/escalate/ignore their lane; the following month can show whether `SocialMemoryAndRelations` has begun to leave `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸`.
- This is not a new command system, not an event pool, not an “事件链”, and not a thick household economy / yamen formula / runner faction / patrol AI pass. DomainEvent remains one factual propagation tool, not the design body.

## Scope In
- Application projection/read-model wording that appends `社会余味读回` to existing owner-lane return status when matching structured SocialMemory response residue is visible.
- Existing SocialMemory residue de-duplication may preserve distinct same-month owner-lane residues by `CauseKey` so an Order repaired residue and a Family repaired residue do not collapse into one record just because they share the same memory kind.
- Public-life, governance docket, and family-facing surfaces should copy projected text so the player can read:
  - owner lane got the后账
  - current outcome reading remains owner-lane owned
  - durable shame/fear/favor/grudge/obligation residue is now `SocialMemoryAndRelations` aftermath, not a reason to re-use the home household as a universal repair path.
- Tests for Order lane and Office/Family lane readback, same-month no SocialMemory mutation, no summary/prose parsing, Unity shell copy-only behavior, and architecture guardrails.

## Scope Out
- No new player commands.
- No new owner-lane ledger, cooldown ledger, household target field, or repeated-response ledger.
- No new SocialMemory rule formulas or durable memory namespace.
- No command resolution in Application, UI, or Unity.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, or SocialMemory summary prose.
- No expansion of `PersonRegistry`.

## Affected Modules
- `Zongzu.Application`: projects owner-lane social-residue readback from existing structured fields (`SocialMemoryEntrySnapshot.CauseKey`, `Kind`, `State`, `Weight`, `OriginDate`, and owner-lane command/outcome traces).
- `Zongzu.Modules.SocialMemoryAndRelations`: keeps existing memory schema and residue formulas, while de-duplicating same-month memory writes by `CauseKey` as well as kind so different owner lanes retain distinct structured aftermath.
- `Zongzu.Presentation.Unity`: remains copy-only through existing view-model fields; tests assert the shell displays projected text without calculating lane validity.
- Docs and tests only for boundary and acceptance alignment.

## Ownership Notes
- `OrderAndBanditry` still owns order repair, road watch, route pressure repair, and route/banditry owner-lane command outcomes.
- `OfficeAndCareer` still owns county-yamen催办, document landing, and clerk-drag handling.
- `FamilyCore` still owns clan elder explanation, household guarantee repair, and lineage face repair.
- `PopulationAndHouseholds` still owns home-household low-power responses and household labor/debt/distress/migration traces.
- `SocialMemoryAndRelations` still does not process commands. It reads structured owner-lane aftermath during monthly simulation and writes only its own memory/narrative/climate residue.

## Save / Schema Impact
- Target impact: none.
- v25 adds projection/readback guidance and a same-schema SocialMemory de-duplication correction only. It must not add persisted fields, bump module schema versions, alter manifests, or add migrations.
- Existing SocialMemory records remain in schema `3`; this can preserve more distinct same-month `order.public_life.response.*` records when the `CauseKey` differs, but the record shape, namespace, migration path, and save envelope do not change.
- If implementation discovers a genuine need for new persisted state, stop before coding it and document the owning module schema bump, migration, roundtrip tests, legacy migration tests, `DATA_SCHEMA.md`, and `SCHEMA_NAMESPACE_RULES.md` impact.

## Determinism Risk
- Low. Projection reads already-built snapshots after deterministic simulation state exists, and SocialMemory cause-key de-duplication remains deterministic.
- No new randomness, scheduler cadence, event handling, save serialization shape, or cross-module authoritative mutation is introduced.

## Milestones
1. Read v24 code/docs and owner-lane/SocialMemory projection seams.
2. Add projection helper for `社会余味读回` using structured SocialMemory residue only.
3. Thread projected guidance into public-life, governance docket, and family-facing surfaces without UI/Unity inference.
4. Add/extend integration, architecture, and Unity presentation tests.
5. Update docs with v25 no-schema/no-migration impact and the “home household is not a universal repair lane” boundary.
6. Run build, focused tests, `git diff --check`, full `dotnet test --no-build`, then commit and push.

## Evidence Checklist
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture / Unity presentation tests:
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "Name=HomeHouseholdLocalResponse_CapacityLineShapesAffordanceAndCommandOutcome"`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Owner_lane_return_surface_readback_must_stay_projection_only"`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "Name=Compose_ProjectsOwnerLaneReturnGuidanceInOfficeAndFamilySurfacesWithoutShellAuthority"`
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] docs updated
- [x] schema/migration impact explicitly documented as no field/schema/envelope/migration impact
- [x] branch committed and pushed
