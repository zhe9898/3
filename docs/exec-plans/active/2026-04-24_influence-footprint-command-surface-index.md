# Influence Footprint Command Surface Index

## Goal
Reduce repeated enabled-affordance surface scans in `PresentationReadModelBuilder.LivingSociety` by indexing enabled player-command surfaces once per bundle build and reusing that read-only projection data when assembling influence reaches.

This pass stays strictly application/projection side:
- no authority changes
- no player-command routing changes
- no save/schema changes
- no shell copy changes

## Scope in / out
In scope:
- add a projection-local helper for indexing enabled command surfaces
- rewire living-society influence reach assembly to use the shared enabled-surface index
- keep current visible behavior and summaries unchanged

Out of scope:
- no new reaches
- no new command affordances or receipts
- no module ownership changes
- no balancing or text changes

## Touched code / docs
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PlayerCommands.Selection.cs`
- `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.LivingSociety.cs`
- `docs/exec-plans/active/*`

## Query / Command / DomainEvent notes
- no query changes
- no command changes
- no domain event changes

## Determinism
No simulation change. Read-model assembly only.

## Save compatibility
None expected.

## Milestones
- [x] Add an enabled-surface projection helper alongside existing affordance/receipt selection helpers.
- [x] Rewire living-society influence reach assembly to use the indexed enabled surfaces instead of repeated scans.
- [x] Run focused and full validation, then record residual risk.

## Risks
- this should stay a small read-model optimization seam, not become a second command policy layer
- helper semantics must keep matching current enabled-affordance visibility rules

## Outcome
- added a projection-local enabled-surface index helper next to the existing affordance/receipt selection helpers
- rewired living-society influence reach assembly so family, yamen, public-life, order, and warfare reach surfaces reuse that indexed enabled-surface set instead of re-scanning affordances repeatedly
- removed the old living-society-local `HasEnabledCommand(...)` scan helper after the shared projection helper became the only visibility path

## Validation
- `git diff --check`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore -p:UseSharedCompilation=false -nodeReuse:false --filter "FullyQualifiedName~M2Bundle_SurfacesLivingSocietyPressureAndInfluenceFootprint"`
- `dotnet build Zongzu.sln -p:UseSharedCompilation=false -nodeReuse:false`
- `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false -nodeReuse:false`

## Residual Risk
- this is still intentionally a read-model-side enabled-surface cache; if later code needs richer command visibility than "surface has any enabled affordance", that should come from explicit projection metadata rather than growing more inferred policy here
