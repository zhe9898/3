## Goal
- harden the governance-lite office slice with stronger determinism, migration, and enabled/disabled path checks
- integrate `OfficeAndCareer` into the current first-pass presentation shell as a read-only surface only

## Scope in
- office read-model export in application code
- first-pass shell read-only governance summaries
- governance-lite determinism and migration regression coverage
- office-enabled vs office-disabled presentation/boundary coverage
- docs and acceptance updates for read-only office presentation

## Scope out
- no new authority rules beyond the existing office-v2 slice
- no player command UI for office actions
- no `WarfareCampaign` rules
- no black-route or black-market rules

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Presentation.Unity`
- integration / presentation / persistence tests
- UI / acceptance docs

## Save/schema impact
- no new schema change planned
- no root schema bump planned
- governance-lite presentation should consume existing office schema `2` data only through queries

## Determinism risk
- low
- presentation is read-only, but governance-lite regression coverage will now prove longer replay stability and legacy office migration equivalence

## Milestones
1. surface office read models in the application export bundle
2. compose a read-only office surface in the first-pass shell without introducing authority behavior
3. add governance hardening coverage for determinism, migration replay, and office enabled/disabled paths
4. update docs and verify build/test

## Tests to add/update
- `M2LiteIntegrationTests`
- `FirstPassPresentationShellTests`
- `SaveMigrationPipelineTests`
- docs / acceptance updates

## Rollback / fallback plan
- if the office surface feels too large for the current shell, keep the exported read models and reduce the shell to summary-only governance strings
- if a migration replay test proves brittle, keep the migration unit coverage and replace the replay test with shorter deterministic save/load parity

## Completion notes
- `PresentationReadModelBuilder` now exports office careers and jurisdiction snapshots when governance-lite is enabled, while stable M2/M3 paths continue to surface empty office read models.
- `FirstPassPresentationShell` now composes a read-only office surface plus governance summaries in the great hall and desk sandbox without introducing any authority logic.
- hardening coverage now includes governance-lite multi-seed determinism, legacy office-schema replay migration, and office-enabled vs office-disabled presentation path assertions.

## Verification
- `& 'C:\Program Files\dotnet\dotnet.exe' build 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.Presentation.Unity.Tests\Zongzu.Presentation.Unity.Tests.csproj' -c Debug --no-build`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj' -c Debug --no-build`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj' -c Debug --no-build`
- `& 'C:\Program Files\dotnet\dotnet.exe' test 'E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln' -c Debug --no-build`

## Save compatibility notes
- no root schema changed in this slice.
- no module schema changed in this slice; presentation now consumes existing office schema `2` data through queries only.
- stable M2/M3 saves still remain office-free by default, and their presentation path must therefore remain office-empty rather than synthesizing governance state.
