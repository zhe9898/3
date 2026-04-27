## Goal
- move the next documented `xun` behaviors into real runtime ownership for `PopulationAndHouseholds` and `FamilyCore`
- keep the month-end hall review readable while letting household and near-kin pressure actually breathe inside the month

## Scope in
- add bounded `RunXun` behavior for `PopulationAndHouseholds`
- add bounded `RunXun` behavior for `FamilyCore`
- keep xun passes projection-silent in this slice
- keep month-end family and household review as the main readable output
- add focused module coverage for xun behavior

## Scope out
- no schema bump
- no new player command surfaces
- no same-month interrupt windows
- no lifecycle re-timing of births, deaths, or marriages into xun yet
- no integration-test-only hall wording changes

## Affected modules
- `Zongzu.Modules.PopulationAndHouseholds`
- `Zongzu.Modules.FamilyCore`
- module tests for both modules
- this exec-plan note

## Save/schema impact
- no root schema bump
- no module schema bump
- xun behavior remains deterministic runtime evolution over existing owned state

## Determinism risk
- low to medium
- xun pulses should stay deterministic and mostly arithmetic, not add hidden clocks or foreign writes
- month-end output must remain the primary readable family/household summary

## Milestones
1. add deterministic livelihood xun pulse for `PopulationAndHouseholds`
2. add deterministic near-family xun pulse for `FamilyCore`
3. add/update focused module tests
4. run build/test verification

## Tests to add/update
- `Zongzu.Modules.PopulationAndHouseholds.Tests`
- `Zongzu.Modules.FamilyCore.Tests`
- `Zongzu.Modules.NarrativeProjection.Tests`
- `Zongzu.Integration.Tests`

## Rollback / fallback plan
- if xun household drift makes month-end too volatile, keep only one bounded xun pulse dimension per module and remove the rest
- if family xun pressure blurs lifecycle readability, keep support/tension drift in xun and leave lifecycle timing fully month-bound

## Open questions
- when to move urgent household shocks such as acute illness, infant risk, or support collapse into explicit xun-visible hotspots
- when to let family lifecycle events request narrow red-band same-month visibility without turning the game into alert spam

## Result notes
- `PopulationAndHouseholds` now owns a bounded deterministic `RunXun` livelihood pulse over existing distress / debt / labor / migration fields, and still keeps readable diff/event output month-bound
- `FamilyCore` now owns a bounded deterministic `RunXun` near-kin pulse over support reserve, branch tension, inheritance pressure, separation pressure, and reproductive pressure, while births / deaths / marriages remain month-bound in this slice
- both xun passes stay projection-silent in this slice: no xun diff spam and no xun domain-event spam
- the slice exposed one regression in `NarrativeProjection` retention: FIFO-only trimming could evict the latest warfare notification after family / household month-end output became denser
- retention is now still bounded, but preserves the latest notification per source module before trimming older overflow, so cross-module hall visibility survives longer runs without introducing a second authority path
- long-run diagnostics were re-verified after that retention fix; the local-conflict 240-month sweep now needs a slightly higher peak save-payload ceiling (`51000`) while growth ceiling remains unchanged

## Verification
- `dotnet test .\tests\Zongzu.Modules.PopulationAndHouseholds.Tests\Zongzu.Modules.PopulationAndHouseholds.Tests.csproj -c Debug`
- `dotnet test .\tests\Zongzu.Modules.FamilyCore.Tests\Zongzu.Modules.FamilyCore.Tests.csproj -c Debug`
- `dotnet test .\tests\Zongzu.Modules.NarrativeProjection.Tests\Zongzu.Modules.NarrativeProjection.Tests.csproj -c Debug`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --filter "FullyQualifiedName~CampaignSandboxBootstrap_ActivatesWarfareCampaignAndSurfacesReadOnlyBoard"`
- `dotnet test .\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --filter "FullyQualifiedName~DiagnosticsHarness_RunMany_TracksTwoHundredFortyMonthBudgetForLocalConflictSlice"`
