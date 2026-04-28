# Social Mobility Influence Readback V277-V284

## Goal

Add the next small player-facing layer after v269-v276: surfaces should not only show that social mobility exists, but also why a person or settlement is being read as close detail, selective pressure detail, active-region pool, or distant summary.

This pass keeps the existing v213-v244 owner rules and adds readback fields only.

## Scope In

- Add runtime read-model fields for influence / scale-budget readback:
  - `FidelityScaleSnapshot.InfluenceFootprintReadbackSummary`
  - `SettlementMobilitySnapshot.ScaleBudgetReadbackSummary`
  - `PersonDossierSnapshot.InfluenceFootprintReadbackSummary`
- Copy the new person-dossier field into Unity-facing ViewModels.
- Show settlement scale-budget readback in desk mobility summaries and fidelity influence readback in great hall mobility summaries.
- Add focused integration, architecture, and Unity presentation tests.
- Update docs with explicit no-schema/no-authority evidence.

## Scope Out

- No new social mobility engine.
- No new player command.
- No migration economy.
- No durable SocialMemory movement residue.
- No promotion/demotion formula beyond existing v213-v244 behavior.
- No new persisted state, schema bump, root save version, migration, manifest change, or projection cache.
- No movement/social/focus/scheduler ledger.
- No Application command outcome calculation.
- No UI/Unity authority.
- No prose parsing from person dossier text, settlement mobility text, `DomainEvent.Summary`, receipt prose, or notification text.

## Affected Modules

- `Zongzu.Contracts`: runtime read-model DTO fields only.
- `Zongzu.Application`: projection/readback assembly from existing structured snapshots.
- `Zongzu.Presentation.Unity.ViewModels`: copy-only DTO field.
- `Zongzu.Presentation.Unity`: copy/project existing read-model fields into shell summaries.
- `tests/*`: focused integration, architecture, and Unity presentation checks.
- `docs/*`: boundary and acceptance evidence.

## Save / Schema Impact

Target impact: none.

Runtime read-model and ViewModel fields are not persisted module state. If implementation requires persisted state or migration, stop and split a schema plan before coding.

## Determinism / Performance Risk

Low. The readback derives from already-built snapshots using stable counts and ordering. It must not add global scans in scheduler hot paths, mutable caches, or UI frame-time rule work.

## Milestones

1. Create this ExecPlan and confirm no schema target.
2. Add runtime read-model fields and Application readback builders.
3. Copy fields into Unity-facing ViewModels/adapters.
4. Add focused tests and architecture guards.
5. Update docs and close with validation evidence.

## Tests To Add / Update

- Integration test assertions for influence/scale-budget readback on fidelity scale, settlement mobility, and person dossier.
- Unity presentation round-trip/adapter assertions proving fields copy through without calculation.
- Architecture guard proving v277-v284 remains read-model/presentation only, schema-neutral, and free of managers/ledgers/prose parsing.

## Rollback / Fallback

If any field proves too noisy, keep the contract field and simplify the text to a stable module-owner readback. Do not move the rule into UI, Application command routing, or persisted state.

## Implementation Evidence

- Added `InfluenceFootprintReadbackSummary` to `FidelityScaleSnapshot` and `PersonDossierSnapshot`.
- Added `ScaleBudgetReadbackSummary` to `SettlementMobilitySnapshot`.
- `PresentationReadModelBuilder` now composes the new readback fields from existing structured person, household, fidelity-ring, and population pool snapshots.
- Great hall, desk, lineage, and person dossier Unity-facing surfaces copy or include the projected fields only.
- Added focused integration, Unity JSON, and architecture proof.
- Updated topology, design audit, module boundaries, integration rules, schema docs, simulation docs, fidelity model, UI/presentation docs, acceptance tests, and skill matrix.
- Save/schema result: none. The pass adds runtime read-model/ViewModel fields only.

## Validation Evidence

- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-build --filter "FullyQualifiedName~SocialMobilityFidelityRingIntegrationTests"` passed: 1/1.
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-build --filter "FullyQualifiedName~ViewModelJsonRoundTripTests"` passed: 3/3.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "FullyQualifiedName~Social_mobility_influence_readback_v277_v284"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings / 0 errors.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed: 0 failed, full solution green.

## Closure

Completed and ready to archive. V277-V284 is a projection/readback layer over existing owner rules; it does not add runtime authority, schema, migration, command ownership, or a complete society engine.
