## Goal
- remove recurring validation-noise from lingering repo-scoped `testhost` processes
- provide one clean serial verification entrypoint for the solution

## Scope in
- add a repo-local PowerShell verification script
- wait for or stop lingering repo-scoped `testhost` processes before solution build
- run solution build and `--no-build` test serially through the script
- update top-level verification guidance

## Scope out
- no changes to simulation code
- no changes to contracts, save schema, or module boundaries
- no changes to test semantics

## Affected modules
- repo tooling
- docs

## Save/schema impact
- none

## Determinism risk
- none
- tooling-only change

## Milestones
1. add clean verification script
2. update verification guidance
3. run script and record result

## Tests to add/update
- no new unit tests
- verify by running the script successfully

## Rollback / fallback plan
- if the script proves too aggressive, keep the serial build/test steps and remove only the lingering-process cleanup logic

## Result
- added [Verify-CleanSolution.ps1](E:/zongzu_codex_spec_modular_rebuilt/tools/Verify-CleanSolution.ps1) as a repo-local clean verification entrypoint
- the script waits for repo-scoped `testhost` processes to exit, force-stops only lingering repo-scoped copies when needed, then runs serial solution `build` and `test --no-build`
- updated [README.md](E:/zongzu_codex_spec_modular_rebuilt/README.md) so the preferred verification path points to the clean script first, while preserving the raw `dotnet build` and `dotnet test` commands below it

## Verification
- `powershell -ExecutionPolicy Bypass -File .\tools\Verify-CleanSolution.ps1`
- result: `dotnet build .\Zongzu.sln -c Debug -m:1` completed with `0 warning / 0 error`
- result: `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build` passed `173/173`
- follow-up raw check: `dotnet test .\Zongzu.sln -c Debug -m:1 --no-build` then `dotnet build .\Zongzu.sln -c Debug -m:1`
- result: follow-up raw `build` also completed with `0 warning / 0 error`
