# Player Command Projection Selector Alignment

## Goal
Reduce repeated player-command projection selector logic in application read-model code so the same family lifecycle heuristics do not drift across hall-docket assembly, preview scenarios, and tests.

This pass should keep the current architecture intact:
- command authority still lives in module-owned `HandleCommand(...)`
- application read models may own projection heuristics and ordering
- selector logic should be shared at the projection layer instead of copied into every consumer
- no UI or test should become a shadow owner of player-command prioritization rules

## Scope in / out
In scope:
- identify duplicated player-command affordance/receipt selection logic in `PresentationReadModelBuilder` and preview helpers
- extract shared projection-side selector helpers for family lifecycle affordances/receipts
- opportunistically slim repeated surface-filtering code where it is obviously the same shape
- update preview/test code to rely on projection outputs or shared projection helpers instead of re-implementing ordering logic

Out of scope:
- no new player commands
- no new command labels or route metadata
- no module authority changes
- no save/schema changes
- no shell layout changes

## Touched code / docs
- `src/Zongzu.Application/PresentationReadModelBuilder/*`
- `src/Zongzu.Application/MvpFamilyLifecyclePreviewScenario.cs`
- `tests/Zongzu.Integration.Tests/*`
- `docs/exec-plans/active/*`

## Query / Command / DomainEvent notes
- no new queries
- no new commands
- no new domain events
- projection-only consolidation; authority and scheduler behavior stay unchanged

## Determinism
No intended simulation change. This is read-model and preview selector consolidation only.

## Save compatibility
None expected.

## Milestones
- [x] Audit selector duplication across hall-docket, governance/warfare affordance picking, preview scenario, and tests.
- [x] Extract shared projection selector helpers and rewire family lifecycle consumers to them.
- [x] Replace duplicated test-side lifecycle ordering logic with assertions against shared projection outputs.
- [x] Run focused and full validation, then record residual projection risk.

## Tests
- integration coverage for preview scenario still passes after selector consolidation
- hall-docket / preview alignment remains intact for family lifecycle suggested commands
- full solution build/test passes

## Risks
- projection selectors are intentionally heuristic and may evolve; the extraction should share current logic without pretending those heuristics became domain rules
- tests should stop copying lifecycle priority formulas directly, but still need enough coverage to catch accidental ordering drift in visible outputs

## Outcome
- added `PresentationReadModelBuilder.PlayerCommands.Selection.cs` as a projection-local home for shared family lifecycle selection plus reusable surface-filter helpers
- rewired family hall-docket, governance, warfare, and MVP preview consumers to those helpers so application-visible command selection no longer drifts across duplicated switches
- added `ProjectionSelectorAlignmentTests` and hardened the existing command-seam receipt test to use an explicit accepted warfare command path instead of relying on incidental affordance availability
- removed dead preview-side lifecycle selector helpers after the shared projection selector became the single visible-ordering source

## Validation
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~ProjectionSelectorAlignmentTests|FullyQualifiedName~CommandSeamIntegrationTests|FullyQualifiedName~MvpFamilyLifecyclePreviewScenario"`
- `git diff --check`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

## Residual Risk
- family lifecycle command ordering is still an application projection heuristic keyed partly off family-facing trace text; that is acceptable for hall/preview selection, but if ordering needs structured reuse beyond projections it should move to explicit read-model fields rather than more string inspection
