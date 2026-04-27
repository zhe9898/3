## Goal
- document the stabilized shell-layer structure inside `Zongzu.Presentation.Unity`
- make the composer / adapter / projection-context boundaries explicit for future work

## Scope in
- add lightweight README/convention notes for `Zongzu.Presentation.Unity`
- document intended dependency direction and folder responsibilities
- keep code behavior unchanged
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new fields or surface behavior
- no wording rewrites

## Affected modules
- `Zongzu.Presentation.Unity`

## Save/schema impact
- none

## Determinism risk
- none
- documentation-only structure pass

## Milestones
1. add shell-layer README/convention notes
2. document composer / adapter / projection-context rules
3. run verification and record result

## Tests to add/update
- no new tests required
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the added notes feel redundant, keep only the project-level README and remove folder-level copies without changing code

## Result
- added lightweight README/convention notes for `Zongzu.Presentation.Unity`, `Adapters/`, and `ProjectionContexts/`
- documented the intended composer / adapter / projection-context boundaries and dependency direction inside the project itself
- added a small composer-role note to `FirstPassPresentationShell` so the code entrypoint matches the written conventions

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
