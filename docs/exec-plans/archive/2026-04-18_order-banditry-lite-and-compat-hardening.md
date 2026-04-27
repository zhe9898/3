# Goal

Advance the post-M2 stabilization path by extending runtime diagnostics for longer multi-seed runs, turning the save migration seam into a registrable pipeline, and implementing the first authority slice of `OrderAndBanditry.Lite` without breaking current module boundaries.

# Scope in / out

## In scope
- extend diagnostics harness coverage from single-run reporting to multi-seed, longer-horizon budget sweeps
- keep diagnostics runtime-only and outside the save compatibility surface
- add registrable root/module migration hooks with explicit chaining/failure behavior
- add tests for registered migration execution and for longer diagnostics sweeps
- implement first-pass `OrderAndBanditry.Lite` authority rules, events, and diffs
- let `TradeAndIndustry.Lite` consume `OrderAndBanditry` projections through queries only when the module is enabled
- add bootstrap/load helpers for an order-enabled M3 slice
- update docs for performance budgets, migration registration, and `OrderAndBanditry` integration

## Out of scope
- full `ConflictAndForce.Lite`
- office, diplomacy, black-market, or warfare implementation
- Unity scene/prefab interaction work
- real content-authored migration catalogues beyond the new registration seam

# Affected modules

- `Zongzu.Application`
- `Zongzu.Persistence`
- `Zongzu.Contracts`
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.TradeAndIndustry`
- integration, persistence, and order-module tests
- docs for schema, module integration, acceptance, and implementation status

# Save/schema impact

- no root schema bump
- no module schema bump for existing M2 modules
- `OrderAndBanditry` remains schema version `1`, but transitions from placeholder no-op behavior to owned authority state
- save loading gains registrable migration hooks while preserving same-version pass-through behavior
- diagnostics sweep outputs remain runtime-only and non-persisted

# Determinism risk

- multi-seed diagnostics must derive from deterministic simulation only
- registered migrations must apply in a stable, explicit order
- `OrderAndBanditry` must use stable iteration order and only query foreign state
- `TradeAndIndustry` must not mutate `OrderAndBanditry` state directly when consuming route insecurity projections

# Milestones

1. Extend diagnostics harness with multi-seed long-run reporting and budget evaluation.
2. Add a registrable save migration pipeline with root/module migration registration and chaining.
3. Add tests for diagnostics sweeps, registered migrations, and same-version compatibility.
4. Implement `OrderAndBanditry.Lite` authority rules and wire `TradeAndIndustry.Lite` to read order pressure through queries.
5. Add M3 order-enabled bootstrap/load helpers, integration tests, and documentation updates.

# Tests to add/update

- diagnostics tests for 240-month multi-seed runs and budget summaries
- persistence tests for registered root/module migration chains and explicit no-path failures
- save roundtrip tests for order-enabled saves
- module tests for `OrderAndBanditry.Lite` state updates, event emission, and explanation text
- integration tests proving order pressure appears in diffs/notifications and influences trade through queries only

# Rollback / fallback plan

- if multi-seed budget reporting becomes noisy, keep the sweep report minimal and expose only peak/final/growth metrics
- if migration registration grows too broad, preserve registration plus explicit failure and defer richer migration contexts
- if the first `OrderAndBanditry` slice becomes too large, keep it to settlement disorder + route pressure and defer outlaw actor detail

# Open questions

- whether future migration handlers should receive richer typed contexts or remain envelope-based
- whether `OrderAndBanditry.Lite` should create settlement disorder records lazily on first tick or eagerly during bootstrap
- how much of the eventual `ConflictAndForce` interface should be anticipated in current order-side events

# Completion notes

- diagnostics now support multi-seed 240-month sweep reporting and budget evaluation while remaining runtime-only.
- save loading now supports registrable chained root/module migrations in addition to same-version pass-through and explicit no-path failure.
- `OrderAndBanditry.Lite` now owns settlement disorder pressure and route insecurity in an order-enabled M3 bootstrap path.
- `TradeAndIndustry.Lite` now consumes order pressure through queries only when `OrderAndBanditry` is enabled.
- verification completed with `dotnet build Zongzu.sln -c Debug` and `dotnet test Zongzu.sln -c Debug --no-build`.
