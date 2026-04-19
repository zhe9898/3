## Goal
- extract governance summary adapter logic out of `FirstPassPresentationShell`
- keep the shell composer thinner while preserving current great-hall and desk governance wording behavior

## Scope in
- move great-hall governance summary selection and momentum merge into a dedicated presentation adapter
- move desk-settlement governance summary momentum merge into the same adapter
- keep all new logic inside `Zongzu.Presentation.Unity`
- rely on existing presentation tests to confirm unchanged behavior

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to application ordering or governance projections
- no authority, simulation, or save changes
- no new shell object grammar fields

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none in authority
- adapter reads existing deterministic governance projections only

## Milestones
1. extract governance summary adapter logic into a dedicated file
2. wire great hall and desk settlement composition through the adapter
3. remove duplicated governance helper methods from the shell composer
4. run focused presentation tests
5. run solution build/test verification

## Tests to add/update
- existing `Zongzu.Presentation.Unity.Tests` governance coverage should continue to pass
- solution build/test verification

## Rollback / fallback plan
- if wording diverges after extraction, restore the governance helper methods inline and compare against the adapter implementation before retrying
- if future shell work needs richer governance composition, extend the adapter rather than growing `FirstPassPresentationShell` again

## Result notes
- governance summary assembly now has a single owning file inside the Unity presentation layer
- `FirstPassPresentationShell` no longer owns lane selection or public-momentum merge details for governance summaries
- no behavior or shared contract shape changed

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
