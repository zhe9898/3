## Goal
- extract active warfare/campaign shell summary logic out of `FirstPassPresentationShell`
- keep hall, desk, and warfare-board wording behavior stable while reducing shell-composer density

## Scope in
- move active regional warfare surface composition into a dedicated presentation adapter
- move great-hall warfare summary and aftermath-docket summary into the same adapter
- move desk-settlement campaign summary and aftermath summary into the same adapter
- add focused presentation coverage for regional warfare and aftermath summaries

## Scope out
- no changes to `Zongzu.Contracts`
- no authority or application behavior changes
- no command routing changes
- no new shell object grammar fields in shared contracts
- no cleanup of older unused warfare helper variants in this slice

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none in authority
- adapter reads existing deterministic warfare, settlement, office, and notification projections only

## Milestones
1. extract active warfare/campaign shell summary logic into a dedicated adapter file
2. wire great hall, desk sandbox, and warfare surface composition through the adapter
3. keep the existing shell output shape unchanged
4. add focused presentation coverage for regional warfare and aftermath summaries
5. run focused tests and solution verification

## Tests to add/update
- `Zongzu.Presentation.Unity.Tests`
- solution build/test verification

## Rollback / fallback plan
- if wording diverges after extraction, restore the inline call sites and compare adapter outputs against the previous shell summaries
- if a later cleanup wants to remove older unused warfare helper variants, do that as a separate slice instead of mixing it into this extraction

## Result notes
- active warfare/campaign shell summary logic now has a dedicated presentation-layer owner
- `FirstPassPresentationShell` now delegates hall, desk, and warfare-board warfare summaries through the adapter
- focused shell coverage now locks regional warfare and aftermath projection behavior

## Verification
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
