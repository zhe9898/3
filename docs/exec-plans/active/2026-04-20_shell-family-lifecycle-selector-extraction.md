## Goal
- extract family lifecycle affordance selection and priority rules into a dedicated presentation-local helper
- keep family wording in `FamilyShellAdapter` while making lifecycle command choice easier to maintain
- verify the recent build-lock warnings with a clean serial solution pass

## Scope in
- add a shared helper for selecting the lead and per-clan lifecycle affordance
- move lifecycle command filtering and priority ranking out of `FamilyShellAdapter`
- reconnect `FamilyShellAdapter` through the new helper without changing shell wording
- run a serial solution build/test pass and record whether file-lock warnings are intrinsic or verification noise

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new shell fields
- no family wording rewrite
- no command behavior changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor and verification pass

## Milestones
1. add dedicated family lifecycle command selector helper
2. reconnect `FamilyShellAdapter` and keep existing shell tests green
3. run serial solution verification and record the lock-warning finding

## Tests to add/update
- no new focused test required if existing shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper hides family-specific behavior, move only the selection logic back into `FamilyShellAdapter` and keep the verification finding

## Result
- added `FamilyLifecycleCommandSelector` to own family lifecycle affordance filtering and priority ranking
- rewired `FamilyShellAdapter` so lifecycle prompt wording stays local while lead/per-clan affordance choice now flows through the shared selector
- kept existing family shell wording and field shape unchanged
- verified the recent build-lock warnings with a clean serial pass; with no lingering `testhost`, `dotnet build .\Zongzu.sln -c Debug -m:1` completed with `0 warning / 0 error`, so the earlier `MSB3026` bursts were verification-time process overlap rather than an intrinsic repo build failure

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
- `Get-Process testhost -ErrorAction SilentlyContinue`
- `dotnet build .\Zongzu.sln -c Debug -m:1`
- `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build`
