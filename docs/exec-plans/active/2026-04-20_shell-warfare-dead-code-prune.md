## Goal
- remove warfare/campaign shell helper code from `FirstPassPresentationShell` after the adapter extraction
- keep `Zongzu.Presentation.Unity` behavior unchanged while making the shell composer smaller and less misleading

## Scope in
- delete dead warfare summary helpers now owned by `WarfareCampaignShellAdapter`
- delete stale nested helper types in `FirstPassPresentationShell` that are no longer needed after the prune
- keep focused shell coverage green

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no authority, command, or projection behavior changes
- no shell wording changes beyond preserving current adapter output

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only dead code removal after adapter extraction

## Milestones
1. identify dead warfare/campaign helpers still resident in `FirstPassPresentationShell`
2. prune them without touching live shell call sites
3. keep focused presentation coverage green
4. run build and no-build solution verification

## Tests to add/update
- no new behavior tests required
- keep `Zongzu.Presentation.Unity.Tests` and solution verification green

## Rollback / fallback plan
- if pruning reveals a live dependency, restore only the minimal helper and move that logic into the adapter in a follow-up slice

## Result
- removed the stale warfare/campaign helper block and no-longer-used nested helper types from `FirstPassPresentationShell`
- restored the still-live `BuildNotificationCenter` seam after the bulk prune exposed it as a shared shell dependency
- cleaned the remaining decompiler-style nullability and unused-variable noise in `FirstPassPresentationShell`, bringing `Zongzu.Presentation.Unity` back to `0 warning / 0 error`
- verification passed:
  - `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
  - `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
  - `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
