# MVP Default Path Freeze

## Goal
Freeze a real code-level MVP path so the repo has one explicit default line that matches `MVP_SCOPE.md` instead of relying on broader M2/M3-plus paths.

## Why now
- the repo already contains optional conflict, governance, warfare, and public-life slices
- MVP closeout needs one clearly named path for preview, acceptance, and later playable validation
- previews and future polish should point at the default MVP line first, not the broader additive paths

## Scope in
- add explicit `CreateMvpBootstrap` / `LoadMvp`
- add explicit `CreateMvpModules`
- keep the default MVP path limited to:
  - `WorldSettlements`
  - `FamilyCore`
  - `PopulationAndHouseholds`
  - `SocialMemoryAndRelations`
  - `EducationAndExams.Lite`
  - `TradeAndIndustry.Lite`
  - `NarrativeProjection`
- repoint the MVP family-lifecycle preview scenario to that path
- add integration tests for:
  - enabled-module freeze
  - 20-year deterministic replay on the default MVP path

## Scope out
- no removal of optional/additive paths
- no new gameplay rules
- no new schema migrations
- no changes to public-life, governance, conflict, or warfare authority logic

## Acceptance impact
- MVP closeout can now point to a named bootstrap path instead of an implied one
- preview artifacts can be regenerated from the actual default MVP path
- optional packs stay explicitly outside that path unless a non-MVP bootstrap is chosen

## Verification
- `dotnet build Zongzu.sln -c Debug`
- `dotnet test Zongzu.sln -c Debug --no-build -m:1`
- `dotnet run --project .\tools\Zongzu.MvpPreviewRunner\Zongzu.MvpPreviewRunner.csproj`
