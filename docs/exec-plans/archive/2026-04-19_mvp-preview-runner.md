# MVP Preview Runner

## Goal
Create a lightweight, rerunnable preview artifact so the current MVP family-lifecycle slice can be inspected without a full Unity project.

The preview should let a teammate:
- run one command
- see a before/after family-lifecycle effect
- read hall / family council / notice output
- regenerate the same markdown snapshot later

## Scope in
- add a reusable preview scenario in application code for the current MVP family-lifecycle path
- add a tiny console runner under `tools/`
- generate a markdown snapshot under `content/generated/`
- wire the tool into the solution so normal builds catch breakage
- add one integration test around the preview scenario
- update `README.md` with the rerun command

## Scope out
- no new gameplay rules
- no Unity project scaffolding
- no screenshots or rendered images
- no save schema changes
- no new player authority

## Affected modules
- `Zongzu.Application`
- `Zongzu.Presentation.Unity`
- `Zongzu.Integration.Tests`
- `README.md`
- `content/generated/`

## Preview shape
- bootstrap the default MVP path
- warm the world a small number of months
- capture a shell snapshot
- issue one bounded family lifecycle command
- capture the immediate shell snapshot
- advance one more month
- capture the downstream shell snapshot
- render the three snapshots into markdown

## Determinism and compatibility
- preview uses existing deterministic bootstraps only
- preview runner must not mutate save schema or compatibility surfaces
- generated markdown is an artifact, not authority data

## Tests to add/update
- integration test that the preview scenario returns:
  - non-empty before/after bundles
  - an accepted family lifecycle command
  - a matching receipt in the post-command bundle
  - readable hall / family-council / notification output

## Verification
- `dotnet build Zongzu.sln -c Debug`
- `dotnet test Zongzu.sln -c Debug --no-build -m:1`
- `dotnet run --project .\tools\Zongzu.MvpPreviewRunner\Zongzu.MvpPreviewRunner.csproj`
