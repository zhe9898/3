## Goal
- move the first documented `xun` behavior into real runtime ownership for `SocialMemoryAndRelations`
- let short-band fear, shame, favor, and feud temperature drift inside the month while keeping memory recording and readable clan narrative month-bound

## Scope in
- add bounded `RunXun` behavior for `SocialMemoryAndRelations`
- update short-band social pressure from family, household, and optional trade state
- keep memory creation, event emission, and public narrative wording month-bound
- add focused module coverage for xun behavior

## Scope out
- no schema bump
- no new memory kinds
- no xun narrative text rewrite
- no xun diff/event output
- no UI or hall wording work

## Affected modules
- `Zongzu.Modules.SocialMemoryAndRelations`
- `Zongzu.Modules.SocialMemoryAndRelations.Tests`
- this exec-plan note

## Save/schema impact
- no root schema bump
- no module schema bump
- xun behavior remains deterministic runtime evolution over existing owned state

## Determinism risk
- low to medium
- xun pulse should stay arithmetic and query-first
- no foreign writes
- month-end narrative wording and memory recording must remain the main readable social output

## Milestones
1. define bounded `SocialMemoryAndRelations` xun pulse over grudge / fear / shame / favor
2. add focused xun module tests
3. run build/test verification

## Tests to add/update
- `Zongzu.Modules.SocialMemoryAndRelations.Tests`
- `dotnet build / test` verification against full solution

## Rollback / fallback plan
- if xun social drift blurs month-end readability, keep only fear / grudge in xun and move shame / favor back to month
- if optional trade linkage feels too coupled, keep xun social drift on family + household only

## Open questions
- when to expose xun social drift into public-life hotspot staging without producing rumor spam
- when to let same-month family crisis visibility pull from this social layer rather than month-end only

## Result notes
- `SocialMemoryAndRelations` now owns a bounded deterministic `RunXun` pulse over short-band `grudge / fear / shame / favor` drift
- `shangxun` now reacts to household distress, migration, support reserve, branch tension, and mediation by nudging fear and feud temperature
- `zhongxun` now reacts to prestige, branch-favor pressure, relief sanction pressure, and optional trade standing by nudging shame and favor balance
- `xiaxun` now lets late-month migration risk, separation pressure, inheritance pressure, and support capacity harden or cool social heat before month-end consolidation
- xun social drift remains projection-silent in this slice: no xun memory creation, no xun event emission, and no xun public narrative rewrite
- the first implementation was corrected to keep test boundaries clean: social tests read `ITradeAndIndustryQueries` through a stub instead of adding a hard project reference to the trade module

## Verification
- `dotnet test .\tests\Zongzu.Modules.SocialMemoryAndRelations.Tests\Zongzu.Modules.SocialMemoryAndRelations.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
