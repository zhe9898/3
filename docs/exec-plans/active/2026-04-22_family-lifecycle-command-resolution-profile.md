# Family Lifecycle And Conflict Command Resolution Profile

## Goal
Give the existing family lifecycle and house-branch conflict commands a small, inspectable numeric skeleton so they behave like bounded intents instead of fixed stat edits.

This is not a final balance table. It is a first profile layer that lets the same command read current pressure bands and produce different deterministic deltas.

## Scope in
- route `议亲定婚` through a profile based on marriage pressure, affinal weakness, support reserve, clan standing, mourning drag, heir fragility, and branch pressure
- route `议定承祧` through a profile based on candidate stability, heir fragility, inheritance pressure, branch pressure, and mediation momentum
- route `拨粮护婴` through a profile based on infant count, `SupportReserve`, `CareLoad`, `RemedyConfidence`, `MourningLoad`, and branch pressure
- route `议定丧次` through a profile based on `MourningLoad`, `FuneralDebt`, `Prestige`, `MediationMomentum`, `SupportReserve`, heir fragility, and branch pressure
- route branch-conflict commands such as `偏护嫡支`, `责令赔礼`, `准其分房`, `停其接济`, `请族老调停`, and `请族老出面` through deterministic pressure profiles
- keep command writes confined to `FamilyCore`-owned state through the existing temporary application command seam
- surface the profile factors in lifecycle/conflict traces and receipts so the command remains explainable
- add integration coverage proving profile bands change results
- update module command metadata so `SupportNewbornCare` and `SetMourningOrder` are listed as `FamilyCore` accepted commands
- add a thin shared `CommandResolution` layer for band scoring, profile-factor text, 0..100 clamping, and shifted delta helpers without moving domain rules into a universal engine

## Scope out
- no new persisted fields
- no schema bump
- no UI authority logic
- no full balancing pass
- no config table migration yet
- no new command surface
- no shared inheritance hierarchy or central decision engine

## Affected modules
- `src/Zongzu.Application`
- `src/Zongzu.Modules.FamilyCore`
- `tests/Zongzu.Integration.Tests`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- no root schema bump
- no `FamilyCore` schema bump
- only existing schema `7` fields are read or written
- command receipts remain derived from existing `LastLifecycle*` and `LastConflict*` fields

## Determinism risk
- low
- no RNG draws are added
- profile results are pure functions of current clan state and infant count
- repeated seed + same command should keep the same replay hash behavior

## Milestones
1. Add this ExecPlan and lock the narrow command-resolution boundary.
2. Add a `FamilyLifecycleResolutionProfile` helper for marriage, heir, newborn-care, and mourning-order commands.
3. Add a `FamilyConflictResolutionProfile` helper for branch-conflict commands.
4. Replace fixed deltas in those command cases with profile deltas.
5. Update accepted-command metadata.
6. Add tests for profile-sensitive outcomes and richer lifecycle/conflict receipts.
7. Run targeted application / integration tests.

## Tests to add/update
- `PlayerCommandService_RoutesFamilyIntent_AndSurfacesFamilyReceipts`
  - now asserts clan-elder mediation reads branch-conflict pressure bands and produces a richer conflict receipt
- `PlayerCommandService_ClanEldersMediationProfile_RespondsToConflictBands`
  - compares calm vs heated branch conflicts and proves the same mediation command lands differently
- `PlayerCommandService_RoutesFamilyLifecycleIntent_AndSurfacesRicherLifecycleReceipts`
  - now asserts marriage pressure falls, affinal value rises, support reserve is spent, and the receipt explains the pressure band
- `PlayerCommandService_MarriageProfile_RespondsToPressureBands`
  - compares strained vs prepared clans and proves the same marriage command lands differently
- `PlayerCommandService_HeirPolicyProfile_RespondsToCandidateBands`
  - compares young vs adult heir candidates and proves candidate stability changes command outcomes
- `PlayerCommandService_RoutesNewbornCareIntent_AndSurfacesInfantFollowUp`
  - now asserts care load falls, support reserve is spent, charity obligation rises, and the receipt explains the pressure band
- `PlayerCommandService_NewbornCareProfile_RespondsToPressureBands`
  - compares strained vs prepared clans and proves the same command lands differently
- `PlayerCommandService_RoutesMourningOrderIntent_AndSurfacesMourningFollowUp`
  - now asserts mourning load, funeral debt, and inheritance pressure fall through the profile

## Rollback / fallback plan
- if the first profile feels too strong, keep the helper seam and reduce band deltas without touching command routing
- if command logic grows too large in Application, move this profile wholesale into `FamilyCore` once the planned module command-handling seam exists
- if balancing needs designer iteration, lift the band thresholds and delta constants into validated config after the MVP command loop is stable

## Result notes
- `议亲定婚` now reads marriage pressure, affinal weakness, support reserve, clan standing, mourning drag, heir fragility, and branch pressure before applying alliance-value lift, pressure relief, reproductive pressure, heir-security lift, support cost, and possible branch backlash
- `议定承祧` now reads candidate stability, heir fragility, inheritance pressure, branch pressure, and mediation momentum before applying heir-security floor/lift, inheritance relief, mediation momentum, and possible branch backlash
- `拨粮护婴` now reads infant count, support reserve, care burden, remedy confidence, mourning drag, and branch pressure before applying support cost / care relief / heir-security lift / reproductive-pressure relief / obligation changes
- `议定丧次` now reads mourning load, funeral debt, prestige, mediation momentum, support reserve, heir fragility, and branch pressure before applying ritual relief / debt relief / inheritance relief / support cost
- branch-conflict commands now read house-branch pressure, separation pressure, old favoritism grievance, inheritance pressure, relief sanction pressure, support reserve, mediation momentum, and prestige before applying command-specific pressure shifts
- lifecycle and conflict receipts now include the pressure-band explanation used by the profile
- `FamilyCore` accepted-command metadata now lists `SupportNewbornCare` and `SetMourningOrder`
- shared command-resolution helpers now live under `src/Zongzu.Application/CommandResolution`; Family lifecycle/conflict uses them for band/factor text and PublicLife order commands use them for office-reach delta adjustment

## Verification
- `dotnet build .\src\Zongzu.Application\Zongzu.Application.csproj -c Debug --no-restore`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --filter "FullyQualifiedName~PlayerCommandService_RoutesFamilyIntent|FullyQualifiedName~PlayerCommandService_ClanEldersMediationProfile|FullyQualifiedName~PlayerCommandService_RoutesFamilyLifecycleIntent|FullyQualifiedName~PlayerCommandService_MarriageProfile|FullyQualifiedName~PlayerCommandService_HeirPolicyProfile|FullyQualifiedName~PlayerCommandService_RoutesNewbornCareIntent|FullyQualifiedName~PlayerCommandService_NewbornCareProfile|FullyQualifiedName~PlayerCommandService_RoutesMourningOrderIntent"`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --filter "FullyQualifiedName~GovernanceLocalConflict_PublicLifeCommands|FullyQualifiedName~GovernanceLocalConflict_OrderInterventionScalesAgainstOfficeReach"`
- `dotnet test .\tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj -c Debug`

## Verification result
- application build passed with 0 warnings and 0 errors
- targeted family command-resolution integration tests passed: 8/8
- shared PublicLife command-resolution regression tests passed: 2/2
- `FamilyCore` module tests passed: 13/13
- scoped `git diff --check` passed for the touched command-resolution files; only existing line-ending normalization warnings were reported
