## Goal
- extract great hall data precomputation into a dedicated presentation-local helper
- reduce `GreatHallShellAdapter` setup noise without changing hall wording, counts, or lead-notice behavior

## Scope in
- add a helper that precomputes lead clan, lead notification, education/governance counts, public-life lead, and hall-docket lead state
- reconnect `GreatHallShellAdapter` through the helper
- keep great hall wording, governance append, and secondary docket behavior unchanged
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new hall fields
- no wording rewrites
- no ordering changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add great hall projection context helper
2. reconnect `GreatHallShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing great hall shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures great hall composition, move only the precomputed selections back into `GreatHallShellAdapter` and keep hall behavior unchanged

## Result
- added `GreatHallProjectionContext` as a presentation-local helper that precomputes the great hall lead clan, lead notification, education/trade/governance counts, hottest public-life settlement, and hall-docket lead state
- reconnected `GreatHallShellAdapter` through the helper without changing hall wording, governance append behavior, or secondary docket hydration

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
