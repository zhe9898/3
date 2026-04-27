# Family Death Succession Pressure Profile

## Goal
Tighten the pre-MVP lineage lifecycle substrate by making death produce differentiated, deterministic pressure instead of one fixed aftermath.

This is still foundation work, not a full funeral or inheritance system. The point is to make death visibly continue into mourning, funeral debt, succession anxiety, marriage pressure, and branch dispute pressure.

## Scope in
- keep the existing `FamilyCore` death path and event names
- add a small internal death-impact profile owned by `FamilyCore`
- distinguish child death, ordinary adult or elder death, and current-heir death
- make current-heir death heavier when there is no adult heir-eligible successor
- surface the pressure factors in `LastLifecycleTrace`
- add module tests proving the same death produces different aftermath depending on successor availability
- update acceptance notes for the death-to-succession pressure chain

## Scope out
- no new persisted fields
- no schema bump
- no full funeral workflow
- no adoption, widow guardianship, property division, grave maintenance, or social-memory edge writes yet
- no UI authority logic
- no new player commands
- no balancing pass

## Affected modules
- `src/Zongzu.Modules.FamilyCore`
- `tests/Zongzu.Modules.FamilyCore.Tests`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- no root schema bump
- no `FamilyCore` schema bump
- existing schema `7` fields are reused

## Determinism risk
- low
- no RNG draws are added
- death-impact profiles are pure functions of current clan state and living family signals

## Milestones
1. Add this ExecPlan and lock the narrow pre-MVP boundary.
2. Add internal `FamilyDeathImpactProfile` helper functions.
3. Route `TryResolveClanDeath` through the profile.
4. Add successor-sensitive death tests.
5. Update acceptance notes.
6. Run targeted FamilyCore tests.

## Verification
- `dotnet build .\src\Zongzu.Modules.FamilyCore\Zongzu.Modules.FamilyCore.csproj -c Debug --no-restore`
- `dotnet test .\tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj -c Debug`
- `dotnet build .\src\Zongzu.Application\Zongzu.Application.csproj -c Debug --no-restore`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --filter "FullyQualifiedName~MvpFamilyLifecyclePreview|FullyQualifiedName~PlayerCommandService_RoutesFamilyLifecycleIntent|FullyQualifiedName~PlayerCommandService_RoutesMourningOrderIntent|FullyQualifiedName~PlayerCommandService_HeirPolicyProfile"`
- scoped `git diff --check`

## Verification result
- `FamilyCore` build passed with 0 warnings and 0 errors
- `FamilyCore` tests passed: 14/14
- application build passed with 0 warnings and 0 errors
- targeted family/lifecycle integration tests passed: 5/5
- scoped `git diff --check` passed for the touched files; only existing line-ending normalization warnings were reported
