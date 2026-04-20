## Goal
- extract hall-docket shell adapter logic out of `FirstPassPresentationShell`
- keep behavior unchanged while making the shell composer thinner and easier to extend

## Scope in
- move great-hall hall-docket lead and secondary-docket mapping into a dedicated presentation adapter
- move desk-settlement hall-agenda mapping into the same dedicated adapter
- keep all new logic inside `Zongzu.Presentation.Unity`
- verify existing shell behavior through focused presentation tests and full solution verification

## Scope out
- no changes to `Zongzu.Contracts`
- no application ordering or projection changes
- no authority or simulation changes
- no new shell object grammar fields

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none in authority
- adapter reads existing deterministic `HallDocket` projections only

## Milestones
1. extract hall-docket shell mapping into a dedicated adapter file
2. update `FirstPassPresentationShell` to compose through the adapter only
3. remove duplicated hall-docket helper methods from the shell composer
4. run focused presentation tests
5. run solution build/test verification

## Tests to add/update
- no new behavior cases required; existing `Zongzu.Presentation.Unity.Tests` coverage should continue to pass
- solution build/test verification

## Rollback / fallback plan
- if the adapter split causes shell regressions, inline the adapter calls back into `FirstPassPresentationShell` without changing read-model shape
- if the adapter grows across unrelated surfaces, split it again by surface rather than re-expanding the shell composer

## Result notes
- `FirstPassPresentationShell` now delegates hall-docket lead, secondary, and desk-agenda mapping to `HallDocketShellAdapter`
- hall-docket presentation logic now has one owning file inside the Unity presentation layer
- no shared contract or ordering behavior changed

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
