# TradeAndIndustryModuleTests Split

## Goal
- Split the oversized `TradeAndIndustryModuleTests.cs` fixture into smaller `partial` files without changing queries, assertions, or module behavior.

## Scope
- `tests/Zongzu.Modules.TradeAndIndustry.Tests/TradeAndIndustryModuleTests.cs`

## Plan
1. Separate xun cadence and stable-market month tests from order-pressure and gray-ledger tests.
2. Keep campaign spillover and intervention carryover tests together because they share the same stub query surface.
3. Move query stubs into a helper partial and verify the trade test project plus the full solution.

## Determinism And Save Compatibility
- Test-only structure cleanup.
- No rule, schema, or save compatibility changes.
