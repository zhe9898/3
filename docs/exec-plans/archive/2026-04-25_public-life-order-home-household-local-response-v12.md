# Public-Life Order Closure v12: Home-Household Local Response Play Surface

## Status

Completed - 2026-04-25

## Framing

This plan thickens the v11 ordinary-household play surface into a first real home-household response loop. It is still a rule-driven command / residue / household pressure / readback loop, not an event-chain and not an event-pool design. `DomainEvent` remains a deterministic fact propagation tool after owning-module rules resolve; it is not the design body.

The key product correction is: ordinary households are not merely a readback decoration on public-life order residue. The player seat begins from a home household, so the first playable layer after `添雇巡丁` / `严缉路匪` refusal or partial residue should let the home household make bounded, low-power choices that affect its own labor, debt, distress, and road risk without repairing the county, order, family, or social-memory accounts by fiat.

## Goal

When Month N refusal / partial public-life order residue projects Month N+1 ordinary-household pressure, the Month N+1 surface should expose a small home-household local response affordance:

- 暂缩夜行: keep household members off risky night roads, lowering migration/road exposure at a labor and debt cost;
- 凑钱赔脚户: spend cash and favors to contain porter/runner misread around the household, lowering distress at a debt cost;
- 遣少丁递信: send a household runner to clarify road news, trading labor for slightly clearer local safety.

Issuing one of these commands must mutate only `PopulationAndHouseholds` at command time. The result is stored as structured household local response aftermath and projected back into public-life / family-facing / household readback surfaces. Social memory is not mutated in the same month and remains owned by `SocialMemoryAndRelations`.

## Scope

In scope:

- Add `PopulationAndHouseholds` command ownership for the three home-household local response commands.
- Persist structured household local response traces in `PopulationHouseholdState`.
- Bump `PopulationAndHouseholds` schema from v2 to v3.
- Add v2 -> v3 migration and save/load proof.
- Project bounded response affordances from read models, not UI rules.
- Add public-life receipts that show local response aftermath and household cost.
- Update influence footprint / shell readback so "本户" has a real command surface.
- Add integration, save, migration, presentation, and architecture tests.
- Update schema, namespace, boundary, integration, simulation, UI, and acceptance docs.

Out of scope:

- Repairing `OrderAndBanditry` refusal / partial authority trace through household commands.
- Handling county-yamen催办 / 文移落地 / 胥吏拖延 in `PopulationAndHouseholds`.
- Handling 族老公开解释 / 本户担保修复 in `PopulationAndHouseholds`.
- Writing `SocialMemoryAndRelations` in the same-month command.
- Adding `HouseholdId` to `PlayerCommandRequest` before the shell has a stable household-target grammar.
- Letting Application, UI, or Unity calculate response effectiveness.
- Parsing `DomainEvent.Summary`, receipt summaries, `LastInterventionSummary`, `LastRefusalResponseSummary`, or narrative prose.
- Adding WorldManager / PersonManager / CharacterManager / god-controller code.
- Expanding `PersonRegistry`.

## Touched Modules

- `Zongzu.Modules.PopulationAndHouseholds`: command resolver, local response state, schema v3 projection, monthly carryover decay, queries.
- `Zongzu.Contracts`: player command constants and household query snapshot fields.
- `Zongzu.Application`: command catalog, projection affordances, receipts, save migration registration.
- `Zongzu.Presentation.Unity`: presentation tests prove shell copies projected fields only; no authority code.
- Docs and tests as listed below.

## Ownership

- `PopulationAndHouseholds` owns home-household local response command traces and local pressure mutations.
- `OrderAndBanditry` continues to own refusal / partial authority trace and public order repair.
- `OfficeAndCareer` continues to own yamen document pressure, clerk delay, and public office response traces.
- `FamilyCore` continues to own clan elder explanation, public shame relief, and guarantee repair.
- `SocialMemoryAndRelations` continues to own durable shame / fear / favor / grudge / obligation residue, and does not process player commands here.
- Application and Unity only route commands and copy projected fields.

## Save And Schema Impact

Schema impact is intentional.

- `PopulationAndHouseholds` module schema: v2 -> v3.
- New persisted `PopulationHouseholdState` fields:
  - `LastLocalResponseCommandCode`
  - `LastLocalResponseCommandLabel`
  - `LastLocalResponseOutcomeCode`
  - `LastLocalResponseTraceCode`
  - `LastLocalResponseSummary`
  - `LocalResponseCarryoverMonths`
- Migration: initialize strings to empty and clamp carryover to 0..1.
- Save/load roundtrip: a local response command remains visible after load.
- Legacy migration proof: v2 household state upgrades to v3 with initialized local response fields.

## Determinism Risk

Low. Command resolution uses current household state, settlement/clan targeting already present in `PlayerCommandRequest`, deterministic ordering, and bounded arithmetic. No UI or Application randomization is added. Replay hash changes only when the owning module accepts a command, consistent with existing command flow.

## Milestones

1. Add v12 ExecPlan and schema-impact statement.
2. Add contract constants and query snapshot fields for home-household local response.
3. Add `PopulationAndHouseholds` command resolver and command handling.
4. Add schema v3 state fields, projection migration, bootstrapper migration, and monthly carryover decay.
5. Add projected affordances / receipts and influence-footprint readback.
6. Add integration proof for command-time ownership and same-month social-memory isolation.
7. Add save roundtrip / legacy migration tests.
8. Add Unity shell copy-only and architecture boundary tests.
9. Update docs and acceptance evidence.
10. Run:
    - `dotnet build Zongzu.sln --no-restore`
    - focused tests
    - `git diff --check`
    - `dotnet test Zongzu.sln --no-build`

## Tests

- v5 refusal / partial residue plus v11 household pressure projects Month N+1 home-household local response affordances.
- Issuing a home-household local response mutates only `PopulationAndHouseholds` at command time.
- Same-month local response does not mutate `SocialMemoryAndRelations`.
- At least two response paths are covered, including contained and strained outcomes.
- Read models expose structured local response aftermath and adjusted household pressure.
- Public-life / family-facing / household readback includes the local response result and cost.
- Unity shell displays projected public-life command/receipt fields only.
- Save/load preserves local response trace.
- Migration upgrades old `PopulationAndHouseholds` v2 state to v3.
- Architecture tests guard boundary drift, summary parsing, forbidden manager/god-controller names, and PersonRegistry expansion.

## Rollback Path

If the command loop proves too wide for v12, revert the command constants, `PopulationAndHouseholds` schema v3 fields, migration, projection helpers, and tests together. Because this plan bumps a module schema, rollback must also remove the migration registration and restore docs to v2.

## Evidence

Completed 2026-04-25:

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration proof passed:
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PartialWatchResidue_ProjectsHomeHouseholdLocalResponse_AndCommandMutatesOnlyPopulation"`: 1 passed.
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~Player_command_contract_matches_live_command_owner_modules|FullyQualifiedName~M2Bundle_SurfacesLivingSocietyPressureAndInfluenceFootprint"`: 2 passed.
- Focused save / migration proof passed:
  - `dotnet test tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj --no-build --filter "FullyQualifiedName~SaveCodec_RoundtripPreservesHomeHouseholdLocalResponseTrace|FullyQualifiedName~LoadM2_DefaultMigrationPipeline_UpgradesLegacyPopulationAndHouseholdsSchemaV2"`: 2 passed.
- Focused Unity projection proof passed:
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~Compose_ProjectsHomeHouseholdLocalResponsePublicLifeFieldsWithoutShellAuthority"`: 1 passed.
- Focused architecture proof passed:
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Home_household_local_response_commands_must_stay_population_owned|FullyQualifiedName~Home_household_local_response_projection_must_copy_projected_fields_only"`: 2 passed.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed; the solution test run completed with all projects green.

Schema / migration evidence:

- `PopulationAndHouseholds` now reports module schema version `3`.
- `SimulationBootstrapper` registers `PopulationAndHouseholds` `2 -> 3` migration.
- Save roundtrip preserves `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, `LastLocalResponseTraceCode`, `LastLocalResponseSummary`, and `LocalResponseCarryoverMonths`.
- Legacy migration initializes local response strings and clamps `LocalResponseCarryoverMonths` to `0..1`.

Boundary evidence:

- `RestrictNightTravel`, `PoolRunnerCompensation`, and `SendHouseholdRoadMessage` route through the shared command catalog to `PopulationAndHouseholds`.
- Command-time mutation is limited to population-owned household pressure and local response trace fields.
- Same-month command handling leaves `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` unchanged.
- Projection and Unity tests prove the shell copies projected affordance / receipt fields only.
