# MVP Ten-Year Preview And Guidance Audit

## Goal
Extend the MVP preview runner so teammates can inspect a longer family-lifecycle slice and verify that hall, family council, and notification guidance keep pointing at the same bounded family action.

## Scope in
- add a ten-year MVP preview scenario on top of the explicit default MVP bootstrap
- generate a second markdown artifact with yearly checkpoints
- audit family-lifecycle guidance alignment across hall / family council / notification center
- add integration coverage for the ten-year run
- update `README.md`

## Scope out
- no new authority rules
- no new UI authority
- no non-MVP module enablement
- no save schema changes

## Affected modules
- `Zongzu.Application`
- `Zongzu.Presentation.Unity`
- `Zongzu.Integration.Tests`
- `README.md`
- `content/generated/`

## Preview shape
- use the explicit default MVP bootstrap
- run one bounded family command per month when an enabled lifecycle affordance exists
- capture the initial shell state
- capture yearly checkpoints for ten in-world years
- render a separate markdown artifact with:
  - yearly family summary
  - lead notice
  - family council summary
  - latest command receipt
  - guidance alignment audit

## Guidance audit rule
- choose the same primary family lifecycle affordance that the shell uses
- ensure hall summary, family council summary, clan lifecycle tile, and family lifecycle notifications all point at that same command label
- if alignment breaks, report it in the generated artifact and fail the integration test

## Determinism and compatibility
- preview remains a read-only artifact over the deterministic MVP bootstrap
- no save schema or compatibility surfaces change
- the audit validates projection consistency, not world authority

## Verification
- `dotnet build Zongzu.sln -c Debug`
- `dotnet test Zongzu.sln -c Debug --no-build -m:1`
- `dotnet run --project .\tools\Zongzu.MvpPreviewRunner\Zongzu.MvpPreviewRunner.csproj`
