# FirstPassPresentationShellTests Split

## Goal
- Split the oversized `FirstPassPresentationShellTests.cs` fixture into smaller `partial` files without changing projections, assertions, or test data.

## Scope
- `tests/Zongzu.Presentation.Unity.Tests/FirstPassPresentationShellTests.cs`

## Plan
1. Keep the baseline hall, family, and lineage projection checks together.
2. Separate governance and public-life surface tests from hall-docket/debug/warfare surface tests.
3. Move the bundle factory into a dedicated helper partial and verify the presentation test project plus the full solution.

## Determinism And Save Compatibility
- Test-only structure cleanup.
- No authoritative simulation, schema, or save behavior changes.
