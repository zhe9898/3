# Person Dossier Depth Ladder

## Goal
Deepen the runtime-only `PersonDossiers` read model beyond the first identity/family/social-memory slice so the lineage surface can answer "who is this person?" through the existing multi-route society documents rather than through an MVP-only cut. The dossier remains a projection-layer join over module-owned queries.

## Scope In
- Add optional dossier lines for household placement, livelihood/activity/health, education/exam path, clan trade footing, office/career position, and dormant social-memory residue when those source modules are present.
- Keep every field read-only and rebuilt from existing query surfaces.
- Extend Unity-facing ViewModels and the lineage person inspector to carry the richer lines.
- Add integration and presentation tests that prove enabled modules enrich the dossier and missing modules still degrade safely.
- Update docs to state that the person dossier is the shell-facing join point for distributed person facts, not a person master table.

## Scope Out
- No `PersonManager`, `CharacterManager`, or authoritative all-person state.
- No new commands, command eligibility, inheritance/marriage/death inference, or domain formulas in Application/Unity.
- No new module state, save namespace, schema version, migration, feature manifest, or persistence changes.
- No direct Unity/module access. The shell reads only `PresentationReadModelBundle`.

## Touched Modules / Docs
- `Zongzu.Contracts`: `PersonDossierSnapshot` display/projection fields only.
- `Zongzu.Application`: `PresentationReadModelBuilder.PersonDossiers` optional query joins.
- `Zongzu.Presentation.Unity.ViewModels`: person dossier / inspector DTOs.
- `Zongzu.Presentation.Unity`: lineage adapter mapping.
- Tests: integration and presentation JSON/compose coverage.
- Docs: `PERSON_OWNERSHIP_RULES`, `UI_AND_PRESENTATION`, `MODULE_INTEGRATION_RULES`, `ACCEPTANCE_TESTS`, `DATA_SCHEMA`.

## Query / Command / DomainEvent Impact
- Query: reads existing `IPersonRegistryQueries`, `IFamilyCoreQueries`, `IPopulationAndHouseholdsQueries`, `IEducationAndExamsQueries`, `ITradeAndIndustryQueries`, `IOfficeAndCareerQueries`, and `ISocialMemoryAndRelationsQueries` when modules are enabled.
- Command: no new command.
- DomainEvent: no new event and no event-summary parsing.

## Determinism Impact
- No authority or scheduler changes.
- Read-model sorting remains deterministic by liveness, fidelity ring, clan/name/person id.
- Optional joins use stable dictionaries keyed by typed ids and stable ordered source lists.

## Save / Schema Impact
- No save/schema impact.
- `PersonDossiers` remain runtime-only presentation read models and do not enter module envelopes or the save root.

## Unity / Presentation Boundary Impact
- Unity-facing DTOs receive additional strings/ids already prepared by the read model.
- Adapters may select and display dossier lines but do not infer status, commands, or consequences.
- The lineage surface remains the first shell consumer; later hall/desk inspectors may reuse the same DTO.

## Milestones
- [x] M1: extend contracts with narrow optional dossier depth fields.
- [x] M2: build optional module indexes and fallback summaries in the read-model builder.
- [x] M3: map new fields into Unity ViewModels and person inspector lines.
- [x] M4: add integration and presentation serialization tests.
- [x] M5: update docs and verify.

## Tests To Run
- `git diff --check`
- `dotnet test tests/Zongzu.Presentation.Unity.Tests/Zongzu.Presentation.Unity.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Integration.Tests/Zongzu.Integration.Tests.csproj --no-restore --filter PersonRegistryIntegrationTests /p:UseSharedCompilation=false`
- `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore /p:UseSharedCompilation=false`
- `dotnet test Zongzu.sln --no-restore /p:UseSharedCompilation=false`

## Verification Result
- `git diff --check`: passed.
- `Zongzu.Architecture.Tests`: passed, 9 tests.
- `Zongzu.Presentation.Unity.Tests`: passed, 24 tests.
- `PersonRegistryIntegrationTests`: passed, 6 tests.
- `Zongzu.sln`: passed.

## Fallback Notes
- If a module lacks person-level queries, the dossier may summarize via the nearest existing clan/household/office projection only when the join is explicit and read-only.
- If an optional module is absent, fields must say no projection rather than throwing.
- If a query is enabled but has no matching person record, keep the registry-only dossier.
