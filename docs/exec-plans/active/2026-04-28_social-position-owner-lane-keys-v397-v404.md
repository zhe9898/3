# Social Position Owner-Lane Keys V397-V404

## Intent

V397-V404 adds structured source-module keys for the commoner / social-position readback introduced in v389-v396.

The goal is to let player-facing surfaces, tests, and future readers know which existing owner-lane snapshots supported the social-position readback without parsing `SocialPositionLabel`, `SocialPositionReadbackSummary`, notification prose, receipt prose, or any other display text.

## Baseline

- `main` at `d2a4f60 Add commoner social position readback`.
- V381-V388 documented commoner / social-position future-lane boundaries.
- V389-V396 added runtime `PersonDossierSnapshot.SocialPositionReadbackSummary` and Unity copy-only presentation.
- Existing `SourceModuleKeys` identifies the full dossier source set; this pass adds a narrower `SocialPositionSourceModuleKeys` list for the social-position readback.

## Implementation Scope

- Add `PersonDossierSnapshot.SocialPositionSourceModuleKeys`.
- Build the list from structured owner snapshots already available to `PresentationReadModelBuilder.PersonDossiers`.
- Copy the list through `PersonDossierViewModel`, the Unity shell mirror, and the lineage shell adapter.
- Add integration, presentation, JSON round-trip, and architecture coverage.

## Non-Goals

- No full class engine.
- No class/social-position/personnel/movement ledger.
- No zhuhu/kehu conversion state.
- No promote/demote command.
- No new module.
- No `PersonRegistry` expansion beyond identity and `FidelityRing`.
- No UI/Unity authority or source inference.
- No parsing of `DomainEvent.Summary`, labels, readback summaries, notification prose, receipt prose, docs text, or localization-facing copy.

## Target Schema / Migration

Target schema/migration impact: none.

The new field is runtime read-model / ViewModel projection only. If future work needs persisted route history, social-position drift, durable conversion state, or a projection cache, stop and write a schema/migration plan first.

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

## Completion Notes

- Runtime impact is limited to `PersonDossierSnapshot.SocialPositionSourceModuleKeys`, Unity-facing ViewModels, and copy-only presentation adapter plumbing.
- The key list is built from structured owner snapshots already assembled for person dossiers; it does not parse `SocialPositionLabel`, `SocialPositionReadbackSummary`, notification prose, receipt prose, docs text, or `DomainEvent.Summary`.
- `PersonRegistry` remains identity / `FidelityRing` only. No class engine, social-position engine, route history, zhuhu/kehu conversion state, promote/demote command, source-key ledger, or durable residue was added.
- Schema and migration impact is explicitly none: no persisted fields, module schema version bump, root save version bump, save manifest change, migration, or projection cache.
- Validation passed:
  - `dotnet test tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj --no-restore --filter "FullyQualifiedName~PersonRegistryIntegrationTests"`
  - `dotnet test tests\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj --no-restore --filter "FullyQualifiedName~FirstPassPresentationShellTests|FullyQualifiedName~ViewModelJsonRoundTripTests"`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Social_position_owner_lane_keys_v397_v404_must_be_structured_projection_not_prose_parser"`
  - `dotnet build Zongzu.sln --no-restore`
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`
