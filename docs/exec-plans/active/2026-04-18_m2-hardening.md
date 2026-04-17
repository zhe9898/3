# Goal

Harden the completed M2 stack so it remains diagnosable and stable over longer runs by formalizing notification retention, exposing read-only debug inspectors, extending deterministic coverage, and documenting save/debug behavior clearly.

# Scope in / out

## In scope
- formalize the `NarrativeProjection` notification retention policy
- expose a read-only debug snapshot for development-facing presentation/debug panels
- store the latest non-authoritative monthly result in application memory for debug trace inspection
- extend deterministic integration coverage to longer multi-seed runs
- add tests for notification retention, debug snapshot composition, and long-run M2 replay stability
- update docs to reflect retention, debug inspectors, and non-persisted debug traces

## Out of scope
- new authority modules
- new player command resolution
- final Unity scene or prefab work
- performance micro-optimization beyond bounded projection history
- post-MVP warfare, office, or outlaw systems

# Affected modules

- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Presentation.Unity`
- integration, narrative, and presentation tests
- docs for schema / acceptance / UI notes

# Save/schema impact

- no new authoritative module namespaces
- no root schema bump
- debug snapshot data remains read-only and non-persisted
- notification history keeps its current module namespace but now has an explicit bounded-retention policy

# Determinism risk

- latest-month debug traces must not feed back into replay hash or save content
- long-run tests must verify equal replay hashes across repeated seeded runs
- notification trimming must be deterministic and preserve newest notices only

# Milestones

1. Add a read-only debug snapshot contract for presentation and application diagnostics.
2. Persist the latest month result in application memory and export debug inspectors from the read-model builder.
3. Formalize and test narrative notification retention behavior.
4. Extend shell view models with a debug panel fed only by read models.
5. Add longer-run deterministic coverage and update docs.

# Tests to add/update

- narrative retention test proving older notifications are trimmed at the retention limit
- integration test proving debug snapshot contains seed, enabled modules, inspectors, and recent traces
- multi-seed 60-month deterministic replay test for M2
- presentation-shell test proving debug panel composition remains read-only

# Rollback / fallback plan

- if the debug snapshot feels too heavy, keep module inspectors and trace lists minimal rather than removing the diagnostics seam
- if latest-month result storage causes confusion, keep it explicitly non-persisted and reset on load/bootstrap
- if retention policy feels too low, adjust the bound without changing the overall projection shape

# Open questions

- whether future runtime builds should gate debug snapshot export behind an explicit development flag
- whether older notification history should eventually be archived into coarser monthly summaries instead of simple trimming
