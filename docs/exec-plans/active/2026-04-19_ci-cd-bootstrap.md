Result: in progress on 2026-04-19. Add repository-level GitHub Actions CI/CD so this modular-monolith .NET workspace has an explicit build/test gate and a minimal release-artifact path.

## Goal
Add GitHub Actions workflows that verify the repo on push/PR and produce a manual-or-tagged release artifact bundle without changing authoritative simulation behavior.

## Scope in / out
### In
- Add `.github/workflows` for CI and minimal CD artifact publication.
- Make CI explicitly run solution build, integration tests, and the remaining test projects.
- Upload test-result artifacts and release binary artifacts from Actions.
- Document the new repository workflows in `README.md`.

### Out
- No gameplay or module-boundary changes.
- No deployment to an external runtime or store.
- No packaging assumptions beyond build artifacts, because the repo is not yet a full Unity project or Windows app bundle.

## Affected modules
- repo workflows:
  - `.github/workflows/*`
- docs:
  - `README.md`

## Save/schema impact
- None.

## Determinism risk
- Low.
- Risks:
  - CI only proving compilation while missing integration gates.
  - workflow duplication making failures noisy or slow.
- Controls:
  - run `Zongzu.Integration.Tests` as an explicit stage.
  - run remaining test projects after the integration gate.
  - keep workflows build-only / artifact-only with no runtime rule changes.

## Milestones
1. Inspect solution/test layout and choose the CI test split.
2. Add CI workflow for restore, build, integration tests, remaining tests, and result upload.
3. Add minimal CD workflow that publishes release binaries as a GitHub artifact on tag or manual dispatch.
4. Update docs and verify the repo still builds/tests locally.

## Tests to add/update
- No product tests added.
- Local verification:
  - `dotnet build Zongzu.sln -c Debug`
  - `dotnet test Zongzu.sln -c Debug --no-build`

## Rollback / fallback plan
- If the split test workflow proves too brittle, collapse CI to a simpler solution-level `dotnet test` gate first, then reintroduce per-project separation later.
