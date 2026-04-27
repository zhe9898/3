# Person Dossier Read-Model Slice

## Goal
Add a minimal read-only person dossier projection so the lineage/family shell can answer "who is this person?" by composing existing distributed person facts.

## Scope In
- Add a contracts-level `PersonDossierSnapshot` read model.
- Add `PresentationReadModelBundle.PersonDossiers`.
- Compose dossiers in `PresentationReadModelBuilder` from existing query seams:
  - `IPersonRegistryQueries`
  - `IFamilyCoreQueries`
  - `ISocialMemoryAndRelationsQueries`
- Surface dossiers in the Unity-facing lineage shell through ViewModels and adapters.
- Add targeted integration and presentation tests.
- Update docs to record projection-only ownership and acceptance.

## Scope Out
- No `PersonManager`, `CharacterManager`, or person master-table module.
- No new authoritative person state.
- No PersonRegistry scope expansion beyond identity anchors.
- No command eligibility, authority formula, inheritance rule, marriage rule, or death rule.
- No `DomainEvent.Summary` parsing.
- No new save namespace, schema version, migration, or feature manifest membership.
- No direct Unity or adapter access to simulation modules.

## Affected Modules and Docs
- Contracts:
  - `src/Zongzu.Contracts/ReadModels/PersonDossierReadModels.cs`
  - `src/Zongzu.Contracts/ReadModels/PresentationReadModelBundle.cs`
- Application:
  - `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.cs`
  - `src/Zongzu.Application/PresentationReadModelBuilder/PresentationReadModelBuilder.PersonDossiers.cs`
- Unity presentation:
  - `src/Zongzu.Presentation.Unity/Adapters/Family/LineageShellAdapter.cs`
  - `src/Zongzu.Presentation.Unity/ProjectionContexts/FamilyProjectionContext.cs`
  - `src/Zongzu.Presentation.Unity.ViewModels/Family/LineageSurfaceViewModel.cs`
  - `src/Zongzu.Presentation.Unity.ViewModels/Family/PersonDossierViewModel.cs`
  - `src/Zongzu.Presentation.Unity.ViewModels/Family/PersonInspectorViewModel.cs`
  - `src/Zongzu.Presentation.Unity.ViewModels/Shared/PresentationShellSelectionViewModel.cs`
  - `unity/Zongzu.UnityShell/Assets/Scripts/ReadModels/ViewModels/Family/LineageSurfaceViewModel.cs`
  - `unity/Zongzu.UnityShell/Assets/Scripts/ReadModels/ViewModels/Family/PersonDossierViewModel.cs`
  - `unity/Zongzu.UnityShell/Assets/Scripts/ReadModels/ViewModels/Family/PersonInspectorViewModel.cs`
  - `unity/Zongzu.UnityShell/Assets/Scripts/ReadModels/ViewModels/Shared/PresentationShellSelectionViewModel.cs`
- Tests:
  - `tests/Zongzu.Integration.Tests/PersonRegistryIntegrationTests.cs`
  - `tests/Zongzu.Presentation.Unity.Tests/*`
- Docs:
  - `docs/UI_AND_PRESENTATION.md`
  - `docs/MODULE_INTEGRATION_RULES.md`
  - `docs/ACCEPTANCE_TESTS.md`
  - `docs/DATA_SCHEMA.md`

## Query / Command / DomainEvent Impact
- Query impact: read-only composition over existing query interfaces.
- Command impact: none.
- DomainEvent impact: none.
- Projection impact: new runtime-only dossier payload in the presentation bundle.

## Save / Schema Impact
- No root schema change.
- No module schema change.
- No migration required.
- `PersonDossiers` are rebuilt read models and are not persisted as an authoritative namespace.

## Determinism Impact
- No authoritative simulation mutation.
- No RNG use.
- Sorting must be stable: living/core/local first, then clan/name/person id.
- Replay hash should remain unaffected except for existing debug save export behavior.

## Unity / Presentation Boundary Impact
- Unity shell receives `PersonDossierViewModel` only from `PresentationReadModelBundle`.
- Adapters do not call module queries or infer rules.
- Lineage surface gains read-only person dossier list while clan tiles remain clan-level.

## Milestones
- [x] Confirm context and live code facts.
- [x] Add contracts read model and bundle field.
- [x] Add application builder partial and wire it into `BuildForM2`.
- [x] Add lineage ViewModel and adapter projection.
- [x] Add a minimal focused person-inspector ViewModel on the lineage surface.
- [x] Add transient focused-person selection into first-pass shell composition.
- [x] Add integration and presentation tests.
- [x] Update docs.
- [x] Run targeted and full verification commands.

## Tests To Run
- `git diff --check`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter PersonRegistryIntegrationTests /p:UseSharedCompilation=false`
- `dotnet test Zongzu.sln --no-restore /p:UseSharedCompilation=false`

## Rollback / Fallback Plan
- If composition cannot safely read optional family or social-memory queries, keep registry-only dossiers and document optional-source absence in `SourceModuleKeys`.
- If Unity shell composition needs a narrower first slice, keep dossiers on the lineage surface only and leave family council detail for a follow-up.
- Revert this slice by removing the new read model, bundle field, builder partial, ViewModel field, adapter mapping, tests, and docs. No save migration cleanup is required.

## Open Questions
- Whether later slices should add education, household, office, trade, or force person facts into the dossier projection.
- Whether a dedicated person inspector surface should get its own ViewModel once Unity host scene work begins.
- Whether dossier ordering should later become player-focus aware using influence footprint and information reach.
