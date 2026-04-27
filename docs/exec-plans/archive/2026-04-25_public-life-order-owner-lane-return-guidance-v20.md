# Public-Life Order Closure v20: External After-Account Owner-Lane Return Guidance

## Status

Implemented and validated on `codex/public-life-order-leverage-v3`.

## Framing

This pass continues the v12-v19 public-life order closure work in thin-chain mode. It adds projected owner-lane return guidance after ordinary home-household responses have already produced receipts and follow-up affordances.

The design remains a rule-driven command / aftermath / social-memory / readback loop. It is not an event-chain, event pool, new command system, or thick household economy. `DomainEvent` remains one fact propagation tool, not the design object.

v20 is projection/readback guidance only: the player-facing surface should read as “本户这头只能缓到这里；真正要补巡丁 / 催县门 / 请族老解释，还得走对应 lane.”

## Why This Slice

v18 receipts show what the household locally eased, what got squeezed, and which external after-account remained. v19 follow-up affordances show repeat / switch / cooldown pressure for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`.

The remaining thin-chain gap is ownership clarity. The player can see that the household is not a universal repair lane, but the current readback still needs to place the remaining after-account back onto the owner lanes:

- 巡丁、路匪、路面误读、route pressure repair -> `OrderAndBanditry`
- 县门未落地、文移拖延、胥吏续拖 -> `OfficeAndCareer`
- 族老解释、本户担保、宗房脸面 -> `FamilyCore`
- shame/fear/favor/grudge/obligation durable residue -> `SocialMemoryAndRelations`

## Scope

In scope:

- Add projection-only owner-lane return guidance to home-household local response affordances and receipts.
- Use text tokens including `外部后账归位`, `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修`.
- Shape guidance from existing structured command / household snapshot fields and existing projected read-model context.
- Keep `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` ownership unchanged.
- Prove Unity shell copies projected fields only.
- Prove `SocialMemoryAndRelations` does not parse owner-lane guidance prose, receipt prose, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- Update docs and acceptance evidence with explicit no-schema-impact notes.

Out of scope:

- No new persisted state.
- No schema bump or migration.
- No cooldown ledger, owner-lane ledger, household target field, route AI, runner faction model, county-yamen formula, or thick household economy.
- No new `WorldManager`, `PersonManager`, `CharacterManager`, or god controller.
- No expansion of `PersonRegistry`.
- No Application / UI / Unity command outcome computation.
- No UI or Unity social-memory writes.

## Boundary Rules

- `OrderAndBanditry` remains owner of order repair, road watch, road-bandit pressure, road misread repair, and route pressure repair.
- `OfficeAndCareer` remains owner of county-yamen催办, document landing, and clerk-delay handling.
- `FamilyCore` remains owner of elder explanation, household guarantee repair, and lineage face.
- `PopulationAndHouseholds` remains owner only of home-household low-power response and household labor / debt / distress / migration trace.
- `SocialMemoryAndRelations` does not process commands. It reads structured aftermath during later month advancement and writes durable shame / fear / favor / grudge / obligation residue into its own state.
- Unity may copy projected affordance / receipt fields only. It must not query modules, compute owner lanes, infer validity, or write social memory.

## Save And Schema Impact

No persisted shape changes are expected in v20.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root schema change.
- No migration.
- No save manifest update.
- No save roundtrip or legacy migration test is required for v20 unless implementation discovers a real persisted-state need.

If v20 discovers that owner-lane guidance cannot be projected from existing structured fields, implementation must stop before adding state and document the owning module schema bump, migration, save roundtrip tests, legacy migration tests, `DATA_SCHEMA.md`, and `SCHEMA_NAMESPACE_RULES.md` impact.

## Determinism Risk

Low. The intended implementation is projection-only and derives strings from already deterministic snapshots and structured command codes. No scheduler cadence, random selection, persisted state, module authority order, save manifest, or monthly advancement path changes are planned.

## Milestones

1. Create this v20 ExecPlan.
2. Add a projection-only owner-lane return guidance helper in the home-household local response read-model builder.
3. Join the guidance into affordance / receipt fields where the player already reads short-term consequence and follow-up pressure.
4. Add integration assertions proving v19 home-household response readback now includes owner-lane return guidance for at least Order lane and Office or Family lane.
5. Keep command-time mutation confined to `PopulationAndHouseholds` and same-month `SocialMemoryAndRelations` untouched.
6. Add architecture tests guarding projection-only behavior, no summary parsing, no forbidden manager names, no `PersonRegistry` expansion, and no schema drift without migration docs/tests.
7. Add Unity presentation tests proving shell copy-only display of projected owner-lane guidance.
8. Update boundary, relationship, schema, simulation, UI, acceptance, skill-matrix, and alignment docs with explicit v20 no-schema-impact evidence.
9. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused integration / architecture / Unity presentation tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`
10. Commit and push the branch.

## Acceptance Notes

- Home-household receipts and next projected affordances should say which local pressure was eased or squeezed and which external after-account returns to an owner lane.
- Owner-lane return guidance must be projected by the read-model layer, not calculated by UI / Unity.
- Guidance must not be derived by parsing `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- `SocialMemoryAndRelations` must not parse `外部后账归位`, `该走巡丁`, `该走县门`, `该走族老`, `本户不能代修`, or receipt prose.
- Ordinary home-household response remains a low-power self-household lane, not a universal repair lane.

## Rollback Path

Remove the projection helper, text joins, v20 tests, and v20 doc notes. Since no persisted state is planned, rollback should not require migration changes.

## Implementation Evidence

- `PresentationReadModelBuilder.PlayerCommands.HomeHouseholdLocalResponse.cs` now builds `HouseholdExternalOwnerLaneReturnGuidance` from existing `HouseholdPressureSnapshot` context and structured local response command code.
- The projection joins `外部后账归位`, `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` into home-household local response affordance / receipt execution, leverage, and readback fields.
- Integration tests prove a v19-style `遣少丁递信` response is still `PopulationAndHouseholds`-owned at command time, same-month `SocialMemoryAndRelations` remains untouched, and the next projected affordance / receipt carries owner-lane return guidance for Order plus Office / Family lanes.
- Architecture tests guard that the projection helper exists, that it does not add owner-lane or cooldown ledgers / hidden household targets, and that the SocialMemory reader does not parse owner-lane guidance tokens, summaries, or receipt prose.
- Unity presentation tests prove the shell copies projected owner-lane guidance fields only.
- Boundary, integration, relationship, schema, namespace, simulation, UI, acceptance, skill-matrix, and alignment docs now record v20 as no-schema-impact readback enrichment.

## Validation Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` passed: 9 tests.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~ProjectReferenceTests"` passed: 26 tests before full-suite rebuild context; after full-suite run, architecture project reports 27 total tests.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~FirstPassPresentationShellTests"` passed: 27 focused tests.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed: full solution, including 125 integration tests.

## Schema / Migration Evidence

No persisted state was added.

- `PopulationAndHouseholds` remains schema `3`.
- `SocialMemoryAndRelations` remains schema `3`.
- `OrderAndBanditry` remains schema `9`.
- `OfficeAndCareer` remains schema `7`.
- `FamilyCore` remains schema `8`.
- No root save version, module namespace, migration, save manifest update, cooldown ledger, owner-lane ledger, household target field, or legacy migration test was required for v20.
