## Goal
- organize `Zongzu.Presentation.Unity` shell files into clearer adapter and projection-context groups
- keep namespaces, behavior, and public entrypoints unchanged

## Scope in
- move shell adapter helpers into an `Adapters/` folder
- move projection-context helpers into a `ProjectionContexts/` folder
- keep `FirstPassPresentationShell`, view models, and project file behavior unchanged
- run presentation/full verification

## Scope out
- no logic changes
- no namespace rewrites
- no file content rewrites beyond path moves
- no changes to tests other than compile path pickup through SDK defaults

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- file-organization only

## Milestones
1. move adapters into grouped folder
2. move projection contexts into grouped folder
3. run presentation/full verification and record result

## Tests to add/update
- no new focused tests required
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if path reorganization introduces unexpected compile or tooling issues, move files back without changing their content

## Result
- moved shell adapter helpers into `src/Zongzu.Presentation.Unity/Adapters/`
- moved projection-context helpers into `src/Zongzu.Presentation.Unity/ProjectionContexts/`
- kept namespaces, public entrypoints, and behavior unchanged while making the shell layer easier to scan and extend

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
