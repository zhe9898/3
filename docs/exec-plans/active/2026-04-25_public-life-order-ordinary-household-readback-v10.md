# Public-Life Order Closure v10: Ordinary Household Pressure Readback & Minimum Play Surface

## Status

Completed - 2026-04-25

## Framing

This plan extends the existing public-life/order closure into ordinary-household visibility. It is still a rule-driven command / residue / social-memory / response / readback loop, not an event-chain and not an event-pool design. `DomainEvent` remains one deterministic fact-propagation tool after module rules resolve; it is not the design center.

v5-v9 already prove command residue, bounded response affordances, owner-module response traces, SocialMemory-owned durable residue, actor countermove readback, and Unity projection-copy behavior. v10 answers the design gap raised after v9: ordinary households are part of this same line. They should read the after-account through livelihood, road fear, runner/porter misunderstanding, labor strain, yamen delay, and local rumor pressure without becoming a new god-control surface.

## Goal

When `添雇巡丁` or `严缉路匪` leaves refused / partial / response residue, ordinary household pressure read models should explain how that aftermath lands on commoner households:

- night-road fear and route pressure;
- runner / porter / watchman misread;
- household labor, debt, and migration strain;
- yamen document delay and clerk drag;
- bounded response affordances already projected from owner modules.

The player should see that ordinary households are affected and indirectly reachable, while command resolution remains in `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`.

## Scope

In scope:

- Add a projection-only ordinary-household signal for public-life/order residue.
- Let `HouseholdSocialPressureSnapshot` expose that ordinary household readback from structured order / office / family response aftermath.
- Let Desk Sandbox settlement pressure copy the projected household readback.
- Add integration proof that refused / partial residue projects ordinary-household after-account without mutating `PopulationAndHouseholds`.
- Add Unity/presentation proof that shell displays projected household readback only.
- Update docs and acceptance criteria.

Out of scope:

- New persisted fields, module schema bumps, migrations, or save manifest changes.
- New ordinary-household command ownership.
- UI/Application/Unity computation of command effectiveness.
- Parsing `DomainEvent.Summary`, receipt summaries, memory summaries, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- PersonRegistry expansion.
- New WorldManager / PersonManager / CharacterManager / god controller.

## Ownership

- `PopulationAndHouseholds` owns commoner household authoritative state.
- `OrderAndBanditry` owns refused / partial public-life order traces and order-owned response traces.
- `OfficeAndCareer` owns yamen催办 / 文移落地 / 胥吏拖延 response traces.
- `FamilyCore` owns 族老解释 / 本户担保 response traces.
- `SocialMemoryAndRelations` owns durable shame / fear / favor / grudge / obligation residue only.
- Application projection reads structured snapshots and assembles read models.
- Unity shell copies projected fields only.

## Save And Schema Impact

No save/schema impact expected.

v10 adds runtime read-model constants and projection text only:

- `HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue`
- `HouseholdSocialDriftKeys.PublicOrderAftermath`

No module-owned persisted state is added. Therefore no module schema bump, migration, save roundtrip, or legacy migration test is required. If this changes, implementation must pause and update `DATA_SCHEMA.md`, `SCHEMA_NAMESPACE_RULES.md`, migrations, and persistence tests before landing.

## Milestones

1. Add v10 ExecPlan and record no-save-impact guard.
2. Add household pressure signal / drift for public-life order residue.
3. Wire Desk Sandbox projection context and settlement pressure summary to copied household readback.
4. Add integration proof for partial watch residue -> ordinary household after-account projection.
5. Add presentation proof that Unity shell displays projected household readback.
6. Update docs / acceptance evidence.
7. Run:
   - `dotnet build Zongzu.sln --no-restore`
   - focused integration and presentation tests
   - focused architecture guards
   - `git diff --check`
   - `dotnet test Zongzu.sln --no-build`

## Acceptance Proof Targets

- v5 refused / partial residue produces an ordinary-household `PublicLifeOrderResidue` signal in Month N+1 read models.
- Projection reads structured order / office / family aftermath fields only.
- Projection does not mutate `PopulationAndHouseholds`, `OrderAndBanditry`, `FamilyCore`, `OfficeAndCareer`, or `SocialMemoryAndRelations`.
- Ordinary household readback includes route fear, runner/watch misunderstanding, labor/debt/migration strain, and yamen delay where projected.
- Bounded response affordances remain owner-module commands; ordinary households are visible pressure carriers, not command owners.
- Unity shell displays projected household pressure readback only.
- No persisted state or schema change is introduced.

## Evidence

- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- Focused integration test passed: `PublicLifeOrderRefusalResponseRuleDrivenTests`, including `PartialWatchResidue_ProjectsOrdinaryHouseholdAfterAccountWithoutPopulationMutation`.
- Focused Unity/presentation tests passed: `Compose_ProjectsOrdinaryHouseholdOrderResiduePressureWithoutShellAuthority` and `Compose_ProjectsActorCountermoveReadbackWithoutShellAuthority`.
- Focused architecture guard passed: `Ordinary_household_order_residue_projection_must_use_structured_after_account_fields`, plus the existing manager/god-controller and PersonRegistry guards through full architecture tests.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
- Save/schema impact remains none: v10 adds runtime read-model constants and projection readback only, with no persisted fields, no module schema version bump, and no migration.
