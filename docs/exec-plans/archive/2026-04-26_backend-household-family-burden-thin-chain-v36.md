# Backend Household -> Family Burden Thin Chain v36

## Goal

- Deepen the next priority from `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`: household tax/grain/frontier burden should begin to press family lifecycle pressure, without becoming a thick household economy or clan relief system.
- Keep Zongzu rules-driven. `DomainEvent` carries facts after owner rules resolve; it is not an event pool or design body.
- Let `FamilyCore` read structured `PopulationAndHouseholds` aftermath and mutate only existing family-owned pressure fields for the sponsor clan.

## Scope In

- `FamilyCore` consumes these existing `PopulationAndHouseholds` events:
  - `HouseholdDebtSpiked`
  - `PopulationAndHouseholds.HouseholdSubsistencePressureChanged`
  - `PopulationAndHouseholds.HouseholdBurdenIncreased`
- `FamilyCore` resolves the affected clan through `IPopulationAndHouseholdsQueries.GetRequiredHousehold(...).SponsorClanId`.
- `FamilyCore` mutates only existing owned fields such as `CharityObligation`, `SupportReserve`, `BranchTension`, `ReliefSanctionPressure`, and `LastLifecycleTrace/Outcome`.
- Follow-on receipts, if thresholded, use existing `FamilyCore` event names and structured metadata.
- Tests prove sponsor-clan targeting, off-scope clan no-touch, no summary parsing, same-month scheduler drain, and no schema/version changes.

## Scope Out

- No new command system, relief ledger, charity ledger, household target field, sponsor-lane ledger, cooldown ledger, family economy subsystem, county formula, or population/family manager.
- No new persisted state, module schema bump, root save bump, migration, save-manifest change, or persisted projection cache.
- No Application/UI/Unity authority; no Unity module queries; no UI calculation of family pressure.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or household local-response prose.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.

## Affected Modules

- `PopulationAndHouseholds`: remains owner of household distress/debt/labor/migration and emits structured household aftermath events. No code ownership moves out of the module.
- `FamilyCore`: owns the sponsor-clan pressure response after household burden facts arrive.
- `Zongzu.Contracts`: may add runtime metadata constants only; no persisted fields.
- `tests`: focused `FamilyCore`, scheduler integration, and architecture guards.
- `docs`: topology, boundaries, schema/no-migration, simulation, relationships, UI/presentation, and acceptance evidence.

## Save / Schema Impact

- V36 targets no persisted shape change. `FamilyCore` remains schema `8`; `PopulationAndHouseholds` remains schema `3`.
- New metadata constants, if added, are runtime event payload keys only. They are not saved fields, ledgers, indexes, or module envelopes.
- If implementation requires a new family relief ledger, household target field, sponsor-lane ledger, or saved aftermath cache, stop and convert this into a schema/migration plan before code changes.

## Determinism Risk

- Low. The handler uses event entity keys, structured metadata, query snapshots, existing module state, deterministic sorting, and fixed profile formulas.
- It does not use wall-clock time, random choices, summary text, UI state, external services, or cross-module direct mutation.

## Milestones

1. Add this ExecPlan with no-schema target.
2. Add `FamilyCore` consumed-event declarations for existing Population household burden receipts.
3. Implement sponsor-clan-only family pressure adjustment from structured household snapshots and metadata.
4. Add focused module tests for debt/subsistence/burden directions and off-scope/no-sponsor cases.
5. Add scheduler integration proof that household burden can drain into FamilyCore in the same month.
6. Add architecture guards for no summary parsing and no schema/version drift.
7. Update docs and run validation.
8. Commit and push `codex/backend-household-family-burden-chain-v36`.

## Tests To Add / Update

- `HouseholdDebtSpiked_PressesSponsorClanOnly`
- `HouseholdSubsistencePressureChanged_UsesStructuredHouseholdSnapshot`
- `HouseholdBurdenIncreased_OffScopeClanDoesNotChange`
- `Chain1_RealScheduler_TaxSeasonDebtDrainsIntoSponsorFamilyPressure`
- Architecture guard: `FamilyCore` household burden handler must use `IPopulationAndHouseholdsQueries`, event type/entity/metadata, and must not parse `DomainEvent.Summary` or household prose.
- Event-contract health classification must treat any new FamilyCore receipt behavior as owner-readback evidence, not runtime authority.

## Evidence Checklist

- [x] ExecPlan created
- [x] FamilyCore consumed-event declarations added
- [x] sponsor-clan handler added
- [x] focused FamilyCore tests passed: `dotnet test tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj --no-build --filter HouseholdBurden`
- [x] focused scheduler integration test passed: `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter Chain1_RealMonthlyScheduler_DrainsTaxSeasonIntoYamenAndPublicLife`
- [x] focused stress-budget check passed after minimal current-ceiling calibration: `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter DiagnosticsHarness_StressBootstrap_TracksHeavierLocalConflictInteractionBudget`
- [x] focused architecture test passed: `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter Family_household_burden_handoff_must_use_structured_population_queries_only`
- [x] docs updated
- [x] no schema/migration impact documented in docs
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture tests
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] commit and push
