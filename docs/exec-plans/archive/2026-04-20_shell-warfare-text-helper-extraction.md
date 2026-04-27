## Goal
- extract low-risk warfare surface text helpers into a dedicated presentation-local helper
- keep `WarfareCampaignShellAdapter` focused on warfare surface mapping without changing campaign ordering or player-facing semantics

## Scope in
- add a helper that owns campaign-surface text normalization, mobilization window labels, front summary text, mobilization summary text, and mobilization office/force summary text
- reconnect `WarfareCampaignShellAdapter` through the helper
- preserve current campaign board ordering, campaign summaries, and warfare receipt wording
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new warfare fields
- no wording rewrites
- no changes to regional-profile heuristics or campaign board ordering

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add warfare text helper
2. reconnect `WarfareCampaignShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing warfare shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures warfare surface composition, move only the local text glue back into `WarfareCampaignShellAdapter` and keep warfare behavior unchanged

## Result
- added `WarfareCampaignTextAdapter` as a presentation-local helper for campaign-surface text normalization, mobilization window labels, front summary text, and mobilization force/office summary text
- reconnected `WarfareCampaignShellAdapter` through the helper for warfare receipts, campaign board text fields, and mobilization signal text fields
- preserved campaign board ordering, warfare summaries, and existing warfare wording

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
