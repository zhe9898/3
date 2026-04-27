# Public-Life Order Closure v11: Ordinary Household Play Surface & Costed Response Choice

## Status

Completed - 2026-04-25

## Framing

This plan extends v10 ordinary-household readback into a minimum playable response surface. It remains a rule-driven command / residue / social-memory / response / readback loop, not an event-chain and not an event-pool design. `DomainEvent` remains only one deterministic fact-propagation tool after module rules resolve.

v10 proved that ordinary households are on the same public-life/order line as `添雇巡丁` and `严缉路匪` residue. v11 makes that visible pressure playable by attaching ordinary-household stakes to the existing bounded response choices, without creating household-owned order commands.

## Goal

When Month N refusal / partial residue projects a Month N+1 ordinary-household `PublicLifeOrderResidue` readback, the public-life response choices should also show:

- which ordinary household is visibly carrying the after-account;
- what the player can indirectly spend through the existing response command;
- what cost / tradeoff is being accepted;
- what next-month readback should be watched;
- which owner module will actually resolve the command.

This turns the household line from "readable" into "judgable": the player can decide whether to spend road-watch guarantee, yamen document pressure, family elder face, compensation, delay, or redirected report reach on the visible after-account.

## Scope

In scope:

- Use projected `HouseholdSocialPressures` to enrich existing public-life refusal-response affordances.
- Enrich response receipts with the same ordinary-household readback after the command resolves.
- Keep all response commands routed through the existing owning modules.
- Let Unity shell copy the already-projected affordance / receipt text only.
- Add integration, presentation, and architecture proof.
- Update docs and acceptance criteria.

Out of scope:

- New ordinary-household command ownership.
- Adding `HouseholdId` to `PlayerCommandRequest`.
- New persisted response fields, schema bumps, migrations, or save manifests.
- Application / UI / Unity computation of response effectiveness.
- Parsing `DomainEvent.Summary`, receipt summaries, memory summaries, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- PersonRegistry expansion.
- New WorldManager / PersonManager / CharacterManager / god controller.

## Ownership

- `PopulationAndHouseholds` owns ordinary household state.
- `OrderAndBanditry` owns order repair, road watch, runner / porter misread, route-pressure repair, and order response traces.
- `OfficeAndCareer` owns county-yamen催办 / 文移落地 / 胥吏拖延 response traces.
- `FamilyCore` owns 族老解释 / 本户担保 response traces.
- `SocialMemoryAndRelations` owns durable shame / fear / favor / grudge / obligation residue only.
- Application projection may join `HouseholdSocialPressures` with existing response affordance / receipt projections, but may not calculate outcomes or mutate state.
- Unity shell copies projected fields only.

## Save And Schema Impact

No save/schema impact expected.

v11 adds runtime projection enrichment only. It does not add module-owned state, persisted fields, schema versions, migrations, save membership, or new command request fields. If implementation requires persisted targeting later, the task must pause for `DATA_SCHEMA.md`, `SCHEMA_NAMESPACE_RULES.md`, migrations, and save/load tests.

## Milestones

1. Add v11 ExecPlan and no-save-impact guard.
2. Add projection helper that selects a visible ordinary-household public-life/order pressure from `HouseholdSocialPressures`.
3. Enrich existing response affordances with ordinary-household target, leverage, cost, and readback text.
4. Enrich existing response receipts with projected household readback after owning-module command resolution.
5. Add integration proof that response choice surfaces include the ordinary household while command-time mutation remains owner-module only.
6. Add Unity/presentation proof that shell copies projected ordinary-household response choice / receipt text only.
7. Add architecture guard against summary parsing and boundary drift.
8. Update docs / acceptance evidence.
9. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused integration, presentation, and architecture tests
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`

## Acceptance Proof Targets

- v5 refused / partial residue plus v10 ordinary-household pressure projects Month N+1 response affordances with household-specific costed choice text.
- The response affordance comes from projected read models and does not make UI compute effectiveness.
- Issuing a response command mutates only the owning module at command time.
- Same-month response does not mutate `PopulationAndHouseholds` or `SocialMemoryAndRelations`.
- Response receipts include ordinary-household readback after owner-module resolution.
- Unity shell displays projected response choice / receipt text only.
- No persisted state or schema change is introduced.

## Evidence

- Added runtime-only ordinary-household response-surface enrichment in `PresentationReadModelBuilder.PlayerCommands.OrdinaryHouseholdSurface.cs`.
- Existing public-life refusal-response affordances now expose projected ordinary-household target, leverage, cost, owner-module, and next-month readback text when `HouseholdSocialPressures` contains `PublicLifeOrderResidue`.
- Existing response receipts now carry the same ordinary-household projected readback after the owning module resolves the command.
- Integration proof added to `PublicLifeOrderRefusalResponseRuleDrivenTests`: Month N+1 response choice names the ordinary household and its cost/readback, issuing `RepairLocalWatchGuarantee` mutates only the owning command surface at command time, and same-month response does not mutate `PopulationAndHouseholds` or `SocialMemoryAndRelations`.
- Unity presentation proof added to `FirstPassPresentationShellTests.GovernanceAndPublicLife`: the shell copies projected response affordance / receipt fields only.
- Architecture proof added to `ProjectReferenceTests`: v11 helper may read `HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue` and player-command projection snapshots, and may not parse summaries, issue commands, touch mutable module state, mutate social memory, or depend on `PopulationAndHouseholdsState`.
- Save/schema impact stayed at none: no module state, schema version, migration, save manifest, or command request field was added.
- Validation passed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~PartialWatchResidue_ProjectsOrdinaryHouseholdAfterAccountWithoutPopulationMutation|FullyQualifiedName~PublicLifeOrderRefusalResponseRuleDrivenTests"` (4 passed)
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~Compose_ProjectsOrdinaryHouseholdOrderResiduePressureWithoutShellAuthority"` (1 passed)
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "FullyQualifiedName~Ordinary_household_response_play_surface_must_use_projected_household_pressures_only|FullyQualifiedName~Ordinary_household_order_residue_projection_must_use_structured_after_account_fields|FullyQualifiedName~Forbidden_manager_or_god_controller_names_are_not_introduced|FullyQualifiedName~PersonRecord_must_remain_identity_only"` (3 passed)
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`
