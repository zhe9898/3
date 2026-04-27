# OfficeAndCareerModuleTests Split

## Goal
- Split the oversized `OfficeAndCareerModuleTests.cs` fixture into smaller `partial` files without changing namespaces, assertions, stubs, or module behavior.

## Scope
- `tests/Zongzu.Modules.OfficeAndCareer.Tests/OfficeAndCareerModuleTests.cs`

## Plan
1. Separate appointment and promotion month tests from xun-cadence tests.
2. Keep cross-module pressure and campaign aftermath tests together because they share the same query stub surface.
3. Move query stub helpers into a dedicated helper partial and verify the module test project plus the full solution.

## Determinism And Save Compatibility
- Test-only structure cleanup.
- No rule, schema, or save behavior changes.
