## Goal
- route the first-pass great hall lead notice through the neutral `HallDocket` projection when available
- keep notification-center behavior unchanged so hall prioritization and notification listing do not collapse into one lane

## Scope in
- update `FirstPassPresentationShell` so great-hall lead title/guidance prefers `HallDocket.LeadItem`
- keep fallback to raw notifications when the hall docket lead is absent
- preserve notification-center composition from notifications only
- add focused presentation tests
- update acceptance notes

## Scope out
- no contract changes
- no authority changes
- no hall-specific object grammar fields
- no notification sorting changes in application

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`
- `docs/ACCEPTANCE_TESTS.md`

## Save/schema impact
- none

## Determinism risk
- none in authority
- adapter reads existing deterministic projections only

## Milestones
1. identify first-pass great-hall lead notice join point
2. prefer `HallDocket.LeadItem` for title/guidance
3. keep notification-center on raw notifications
4. add focused shell coverage
5. run build/test verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if the hall-docket lead wording proves too dense, keep the title from `HallDocket` but fall back to notification guidance
- if a bundle path does not supply a meaningful `HallDocket` lead, preserve the old notification-first behavior on that path

## Result notes
- great-hall lead notice now prefers the read-only hall-docket lead item when present
- lead guidance is assembled from hall-docket guidance/phase/handling text, with notification fallback preserved
- notification-center items remain notification-driven and are not replaced by hall-docket items

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
