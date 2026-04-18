## Goal
- implement the first active `OfficeAndCareer.Lite` authority slice as the next bounded leverage layer after exams, trade, order, and local conflict
- keep the slice additive by introducing a dedicated post-MVP governance bootstrap path instead of mutating the stable M2/M3 paths

## Scope in
- `OfficeAndCareer` contracts, state, runner, and tests
- a post-MVP governance-lite bootstrap/load path in application code
- optional read-only integration from `ConflictAndForce` and `OrderAndBanditry` into office leverage queries
- docs and acceptance updates for module boundaries, schema, and post-MVP governance behavior

## Scope out
- no `WarfareCampaign` rules
- no final player-facing office UI
- no office command handling beyond declaring accepted commands
- no office-to-foreign direct writes

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Modules.OfficeAndCareer`
- `Zongzu.Application`
- `Zongzu.Modules.ConflictAndForce`
- `Zongzu.Modules.OrderAndBanditry`
- module, integration, and persistence tests
- governance / schema / integration docs

## Save/schema impact
- no root schema bump planned
- add active module namespace `OfficeAndCareer` at schema version `1`
- no migration step required for current active paths because governance-lite uses a new manifest path
- old M2/M3 saves must continue to load cleanly with `OfficeAndCareer` disabled

## Determinism risk
- medium
- `OfficeAndCareer` will participate in phase 5 and can influence downstream order/conflict calculations
- mitigate with:
  - focused module tests for office appointment and leverage behavior
  - order/conflict tests proving office influence arrives through queries only
  - integration and save-roundtrip verification on the new governance bootstrap

## Milestones
1. create the ExecPlan and align scope with the current post-MVP governance seam
2. add `OfficeAndCareer` contracts, state, and lite authority logic
3. wire a governance-lite bootstrap/load path without changing current M2/M3 paths
4. let order/conflict read office leverage through queries only
5. add tests, update docs, and verify build/test

## Tests to add/update
- `Zongzu.Modules.OfficeAndCareer.Tests`
- `ConflictAndForceModuleTests`
- `OrderAndBanditryModuleTests`
- `M2LiteIntegrationTests`
- `SaveRoundtripTests`
- docs / acceptance updates

## Rollback / fallback plan
- if downstream office influence feels too invasive, keep `OfficeAndCareer` active but remove the optional order/conflict reads for this slice
- if governance-lite bootstrap adds too much surface area, keep the module wired only through isolated module tests and a single integration bootstrap

## Completion notes
- `OfficeAndCareer.Lite` is now implemented as an active authority module with owned office-career state plus jurisdiction-authority state.
- the slice ships through a dedicated `CreateP1GovernanceLocalConflictBootstrap` / `LoadP1GovernanceLocalConflict` path so stable M2/M3 manifests remain unchanged.
- `OfficeAndCareer.Lite` currently reads only `EducationAndExams` and `SocialMemoryAndRelations`, then produces explainable appointment, authority-tier, leverage, and petition-pressure updates.
- `OrderAndBanditry.Lite` and `ConflictAndForce.Lite` now optionally read office jurisdiction leverage through queries only, using it as bounded administrative relief/support rather than direct foreign-state mutation.
- new coverage now spans module tests, downstream order/force boundary tests, integration bootstrap coverage, and save roundtrip for governance-lite.

## Verification
- `dotnet build E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Modules.OfficeAndCareer.Tests\Zongzu.Modules.OfficeAndCareer.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Modules.OrderAndBanditry.Tests\Zongzu.Modules.OrderAndBanditry.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Modules.ConflictAndForce.Tests\Zongzu.Modules.ConflictAndForce.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Integration.Tests\Zongzu.Integration.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\tests\Zongzu.Persistence.Tests\Zongzu.Persistence.Tests.csproj -c Debug --no-build`
- `dotnet test E:\zongzu_codex_spec_modular_rebuilt\Zongzu.sln -c Debug --no-build`

## Save compatibility notes
- no root schema changed in this slice.
- `OfficeAndCareer` is now an active module namespace at schema version `1`, but only through the dedicated governance-lite manifest path.
- stable M2 and M3 manifests remain office-free by default, so existing saves do not gain an `OfficeAndCareer` envelope implicitly.
- no new migration step was required for current active paths; governance-lite simply starts with owned default state when enabled.
