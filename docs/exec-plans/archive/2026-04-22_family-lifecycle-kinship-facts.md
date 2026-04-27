# Family Lifecycle Kinship Facts

## Goal
Turn the current family lifecycle slice from clan-level pressure changes into concrete person-linked facts where the code already has the right state hooks:
- autonomous marriage must bind real spouses
- birth must preserve parent links and add care burden
- death must leave mourning plus funeral debt

This is a narrow kernel slice. It should make the monthly loop more believable without opening a full marriage market, dowry system, widowhood system, adoption law, or household economy.

## Scope in
- make `TryArrangeAutonomousMarriage` pick a living unmarried adult and create a registered spouse when no cross-clan match has already resolved the pressure
- keep spouse links inside `FamilyCore` and identity creation through `PersonRegistry` command surface
- make birth visibly increase `CareLoad` in addition to existing newborn registration and parent linking
- make death visibly increase `FuneralDebt` in addition to existing mourning, succession, and cause-specific death event behavior
- add focused `FamilyCore` tests for spouse links, parent links / care burden, and funeral debt

## Scope out
- no schema bump unless new persisted fields are introduced
- no new module
- no Unity scene work
- no UI authority logic
- no full affinal network, bridewealth, dowry, widowhood, remarriage, adoption, concubinage, or ritual-law simulator in this pass
- no player command redesign

## Affected modules
- `src/Zongzu.Modules.FamilyCore`
- `tests/Zongzu.Modules.FamilyCore.Tests`
- `docs/DATA_SCHEMA.md`
- `docs/MODULE_BOUNDARIES.md`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- no root schema bump
- no `FamilyCore` schema bump expected
- `SpouseId`, `FatherId`, `MotherId`, `ChildrenIds`, `CareLoad`, and `FuneralDebt` already exist in schema `7`
- existing saves missing these fields remain covered by current default-value behavior and migration/backfill paths

## Determinism risk
- low to medium
- new spouse creation allocates one `PersonId`, so it must run only from stable clan/person ordering
- no extra random draws should be added
- birth should not occur in the same month as a newly arranged marriage in this slice, so one monthly tick does not collapse courtship, spouse entry, and childbirth into one step

## Milestones
1. Add this ExecPlan and lock the narrow rule boundary.
2. Implement person-linked autonomous marriage and one-month birth deferral after new marriage.
3. Add care burden on birth and funeral debt on death.
4. Update focused tests and docs.
5. Run `FamilyCore` tests and, if fast, a targeted integration test.

## Tests to add/update
- `RunMonth_ArrangesMarriage_WhenAlliancePressureRunsHigh`
  - now asserts a concrete registered spouse and reciprocal `SpouseId`
- `RunMonth_RegistersBirth_WhenMarriageAndReproductivePressureAlign`
  - now asserts newborn parent links / parent child lists and `CareLoad`
- `RunMonth_RegistersDeath_AndWeakensHeirSecurity_WhenLifecycleThresholdsAreMet`
  - now asserts `FuneralDebt`
- `RunMonth_InfantDeath_EmitsDeathByIllness_AndRaisesReproductivePressure`
  - now asserts smaller child funeral burden

## Rollback / fallback plan
- if spouse creation makes bootstrap identity timing too noisy, keep the spouse-link requirement but restrict autonomous marriage to already-seeded candidate spouses
- if care / funeral pressure creates too much monthly drift, keep one-time birth/death deltas only and defer decay rules to a later family-household slice

## Open questions
- should future marriage-in spouses remain `DependentKin`, or should affinal entrants get a more precise branch position
- should funeral debt decay by ritual calendar, cash/grain support, or a bounded player command such as `议定丧次`
- should birth care burden later split into milk/nursing, grain ration, and elder-child sibling care, or stay as one clan-owned pressure band

## Result notes
- autonomous marriage now creates a concrete registered spouse, writes reciprocal `SpouseId`, and defers birth until a later monthly tick
- birth already registered newborn identity and parent links; this slice now also raises clan-owned `CareLoad`
- death already wrote canonical death through `PersonRegistry` and cause-specific events; this slice now also raises clan-owned `FuneralDebt`
- FamilyCore person creation now skips already-used `PersonId` values before registering births or marriage-in spouses, protecting unit seeds and future migrated saves from identity collisions

## Verification
- `dotnet test .\tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj -c Debug`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --filter "FullyQualifiedName~PersonRegistryIntegrationTests"`
