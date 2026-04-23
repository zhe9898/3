# SaveMigrationPipelineTests Split

## Goal
- Split the oversized `SaveMigrationPipelineTests.cs` file into smaller `partial` test slices without changing module boundaries, namespaces, assertions, or migration behavior.

## Scope
- `tests/Zongzu.Persistence.Tests/SaveMigrationPipelineTests.cs`

## Plan
1. Partition the test fixture by migration concern: core pipeline/report coverage, legacy upgrade paths, governance and warfare migrations, and black-route/public-life migrations.
2. Move helper methods and legacy fixture types into a dedicated helper partial while preserving the same `SaveMigrationPipelineTests` type.
3. Verify the persistence test project and the full solution after the split.

## Determinism And Save Compatibility
- No simulation rules change.
- No schema or migration logic changes.
- This task is test-only structure cleanup, so save compatibility risk stays unchanged.
