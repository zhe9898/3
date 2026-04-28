# Commoner Social Position Readback V389-V396

## Intent

V389-V396 adds the first runtime readback surface for commoner / social-position interpretation on person dossiers.

This is a projection/readback layer over existing owner snapshots. It does not implement a class engine, promote/demote people, resolve zhuhu/kehu conversion, add a commoner route, or simulate every person as a career actor.

## Baseline

- `main` at `08153a3 Document commoner social position preflight`.
- V381-V388 documented the future-lane contract.
- Existing `PersonDossierSnapshot.SocialPositionLabel` already joins structured owner projections from family, household, education, trade, office, and social memory context.

## Implementation Scope

- Add `PersonDossierSnapshot.SocialPositionReadbackSummary`.
- Build the readback in `PresentationReadModelBuilder.PersonDossiers` from structured owner snapshots only.
- Copy the field into `PersonDossierViewModel`, the Unity shell mirror, and the lineage shell adapter.
- Let the focused person status ledger display the projected readback when present.
- Add focused integration, presentation, and architecture coverage.

## Non-Goals

- No full class engine.
- No direct promote/demote command.
- No zhuhu/kehu conversion state.
- No office-service, trade-attachment, clerk, artisan, merchant, tenant, or gentry route.
- No class/social-position/personnel/movement/focus/scheduler ledger.
- No persisted state, schema bump, migration, save manifest change, or projection cache.
- No Application class authority.
- No UI/Unity authority.
- No `PersonRegistry` expansion beyond identity and `FidelityRing`.
- No prose parsing of `DomainEvent.Summary`, person dossier labels, mobility text, public-life lines, receipt prose, notification prose, docs text, or social-position readback text.

## Target Schema / Migration

Target schema/migration impact: none.

The new field is runtime read-model / ViewModel projection only. If future work needs persisted status drift, conversion state, route history, durable residue, or a projection cache, stop and write a schema/migration plan first.

## Validation Plan

- `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "FullyQualifiedName~PersonRegistryIntegrationTests|FullyQualifiedName~SocialMobilityFidelityRingIntegrationTests"`
- `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-restore --filter "FullyQualifiedName~FirstPassPresentationShellTests|FullyQualifiedName~ViewModelJsonRoundTripTests"`
- Focused architecture guard.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Milestones

- [x] Add runtime read-model and Unity-facing field.
- [x] Add focused integration and presentation tests.
- [x] Add docs and architecture guard.
- [x] Run validation lane.

## Completion Notes

- Runtime impact is limited to `PersonDossierSnapshot.SocialPositionReadbackSummary`, Unity-facing ViewModels, and copy-only presentation adapter plumbing.
- The readback is built from structured owner snapshots already assembled for person dossiers; it does not parse labels, prose, summaries, receipts, public-life lines, notifications, or docs text.
- `PersonRegistry` remains identity / `FidelityRing` only. No commoner class engine, personnel movement engine, zhuhu/kehu conversion, route ledger, or promotion/demotion command was added.
- Schema and migration impact is explicitly none: no persisted fields, module schema version bump, root save version bump, save manifest change, migration, or projection cache.
- Validation passed:
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "FullyQualifiedName~PersonRegistryIntegrationTests|FullyQualifiedName~SocialMobilityFidelityRingIntegrationTests"`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-restore --filter "FullyQualifiedName~FirstPassPresentationShellTests|FullyQualifiedName~ViewModelJsonRoundTripTests"`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Commoner_social_position_readback_v389_v396_must_copy_structured_dossier_projection_without_class_authority"`
  - `dotnet build Zongzu.sln --no-restore`
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`
