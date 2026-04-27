## Goal
- extract desk sandbox settlement lookup/index preparation into a dedicated presentation-local helper
- reduce `DeskSandboxShellAdapter` setup noise without changing node wording or hydration behavior

## Scope in
- add a helper that precomputes ordered settlements and per-settlement lookups used by desk sandbox projection
- reconnect `DeskSandboxShellAdapter` through the helper
- keep settlement-node summary wording, governance append, and public-life hydration behavior unchanged
- run presentation/full verification

## Scope out
- no changes to `Zongzu.Contracts`
- no changes to `Zongzu.Application`
- no new desk fields
- no wording rewrites
- no node-order changes

## Affected modules
- `Zongzu.Presentation.Unity`
- `Zongzu.Presentation.Unity.Tests`

## Save/schema impact
- none

## Determinism risk
- none
- presentation-only refactor

## Milestones
1. add desk sandbox projection context helper
2. reconnect `DeskSandboxShellAdapter`
3. run presentation/full verification and record result

## Tests to add/update
- no new focused test required if existing desk shell coverage remains green
- keep `Presentation.Unity` and full-solution coverage green

## Rollback / fallback plan
- if the helper obscures desk-node composition, move only the lookups back into `DeskSandboxShellAdapter` and keep the adapter behavior unchanged

## Result
- added `DeskSandboxProjectionContext` as a presentation-local helper that precomputes ordered settlements and per-settlement lookup/index access for desk sandbox projection
- reconnected `DeskSandboxShellAdapter` through the helper without changing node wording, hall-agenda hydration, governance append, or desk public-life hydration behavior
- fixed the two compile seams introduced during extraction by restoring the required `System` and `Zongzu.Kernel` imports in the new helper

## Verification
- `dotnet build .\src\Zongzu.Presentation.Unity\Zongzu.Presentation.Unity.csproj -c Debug -m:1`
- `dotnet test .\tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj -c Debug`
