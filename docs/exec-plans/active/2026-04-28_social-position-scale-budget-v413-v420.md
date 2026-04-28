# Social Position Scale Budget V413-V420

## Intent

V413-V420 adds a runtime scale-budget readback for commoner / social-position person dossiers.

The goal is to make the player-facing dossier say the rule the design already follows: near people can receive structured owner-lane detail, while distant society remains pooled summary. This keeps social-position visibility readable without implying all-world per-person class simulation.

## Baseline

- `main` at `aeff30e Close social position readback layer`.
- V381-V412 closed the first commoner / social-position readback layer.
- Existing person dossiers already carry `SocialPositionReadbackSummary` and `SocialPositionSourceModuleKeys`.

## Implementation Scope

- Add `PersonDossierSnapshot.SocialPositionScaleBudgetReadbackSummary`.
- Build the readback from existing `FidelityRing` and structured social-position source keys.
- Copy the field through `PersonDossierViewModel`, the Unity shell mirror, and `LineageShellAdapter`.
- Display the projected field in the focused person status ledger by concatenating projected fields only.
- Add focused integration, presentation, JSON round-trip, architecture, and docs evidence.

## Non-Goals

- No full class engine.
- No fidelity-ring mutation.
- No promote/demote command.
- No zhuhu/kehu conversion state.
- No office-service, trade-attachment, clerk, artisan, merchant, tenant, or gentry route.
- No class/social-position/personnel/movement/focus/scheduler/scale-budget ledger.
- No persisted state, schema bump, migration, save manifest change, or projection cache.
- No Application, UI, or Unity authority.
- No `PersonRegistry` expansion beyond identity and `FidelityRing`.
- No prose parsing of `DomainEvent.Summary`, `SocialPositionLabel`, `SocialPositionReadbackSummary`, `SocialPositionSourceModuleKeys`, notification prose, receipt prose, public-life lines, mobility text, or docs text.

## Target Schema / Migration

Target schema/migration impact: none.

The new field is runtime read-model / ViewModel projection only. If future work needs persisted precision-band state, status drift, conversion state, route history, durable residue, or a projection cache, stop and write a schema/migration plan first.

## Validation Plan

- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "FullyQualifiedName~PersonRegistryIntegrationTests"`
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-restore --filter "FullyQualifiedName~FirstPassPresentationShellTests|FullyQualifiedName~ViewModelJsonRoundTripTests"`
- Focused architecture guard.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Milestones

- [x] Add runtime read-model and Unity-facing field.
- [x] Add focused integration and presentation tests.
- [x] Add docs and architecture guard.
- [x] Run final validation lane.

## Completion Evidence

- Added `PersonDossierSnapshot.SocialPositionScaleBudgetReadbackSummary` as a runtime read-model field built from existing `FidelityRing` plus structured social-position source keys.
- Copied the projected field through Unity-facing ViewModels and `LineageShellAdapter`; Unity remains copy-only.
- Updated the v397-v404 owner-lane source-key architecture guard so the reused `socialPositionSourceKeys` local remains accepted as structured projection rather than prose parsing.
- Schema / migration impact: none. No persisted state, schema version, migration, save manifest, or `PersonRegistry` authority field changed.

Validation completed on 2026-04-28:

- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "FullyQualifiedName~PersonRegistryIntegrationTests"`
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-restore --filter "FullyQualifiedName~FirstPassPresentationShellTests|FullyQualifiedName~ViewModelJsonRoundTripTests"`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Social_position_scale_budget_v413_v420_must_read_existing_fidelity_without_precision_or_class_authority"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-build --filter "Name=Social_position_owner_lane_keys_v397_v404_must_be_structured_projection_not_prose_parser|Name=Social_position_scale_budget_v413_v420_must_read_existing_fidelity_without_precision_or_class_authority"`
- `dotnet test Zongzu.sln --no-build`
