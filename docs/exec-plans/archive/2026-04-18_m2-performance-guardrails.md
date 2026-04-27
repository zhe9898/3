# Goal

Add a headless diagnostics/performance harness and stronger save compatibility guardrails so the hardened M2 stack can be exercised over longer runs, inspected for bounded growth, and verified against legacy/additive save scenarios.

# Scope in / out

## In scope
- add a headless M2 diagnostics harness for long-run metric capture
- record per-month counts for diffs, domain events, notifications, and save payload size
- add save compatibility tests for legacy M0-M1 loads under M2, runtime-only debug trace reset, and schema mismatch rejection
- add integration coverage proving diagnostics metrics stay bounded by the current projection retention policy
- update docs to describe the new diagnostics seam and additive save compatibility expectations

## Out of scope
- wall-clock performance thresholds or benchmark gating
- new gameplay modules
- runtime profiling UI or charts
- schema migrations for incompatible saves
- Unity scene work

# Affected modules

- `Zongzu.Application`
- integration and persistence tests
- docs for acceptance and schema/save notes

# Save/schema impact

- no new save namespaces
- no root schema bump
- diagnostics harness data remains runtime-only and non-persisted
- additive compatibility must continue to allow older saves to load when newly introduced modules remain disabled in the feature manifest

# Determinism risk

- diagnostics collection must not mutate authoritative state or replay hash
- save-size sampling must read exported saves only
- legacy-save tests must prove M2 loaders ignore disabled future modules cleanly

# Milestones

1. Add a diagnostics/performance report type and harness in application code.
2. Add integration tests for long-run bounded metrics and diagnostics sampling.
3. Add persistence tests for runtime-only debug trace reset, legacy M0-M1 load under M2, and schema mismatch rejection.
4. Update docs for additive compatibility and diagnostics expectations.

# Tests to add/update

- integration test for a 120-month M2 diagnostics harness run with bounded notification growth
- persistence test proving M0-M1 saves load through the M2 loader when new modules are off
- persistence test proving runtime-only debug traces reset after save/load
- persistence test proving schema mismatches are rejected explicitly

# Rollback / fallback plan

- if monthly save-size sampling is too noisy, keep only max/final payload sizes
- if long-run diagnostics reports feel too heavy, keep the samples but bound them to the simulated months rather than accumulating extra derived projections
- if legacy-save coverage exposes incompatibilities, keep M2 additive and defer any breaking schema changes until migrations exist

# Open questions

- whether future phases should promote diagnostics harness output into a dedicated developer CLI or automation
- whether payload-size guardrails should later become automated regression thresholds or stay observational only
