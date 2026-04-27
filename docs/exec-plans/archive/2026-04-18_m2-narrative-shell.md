# Goal

Finish the remaining M2 slice by adding `NarrativeProjection` plus a first-pass presentation shell that renders read models for family, exams, trade, and notifications without introducing any new authority rules into UI code.

# Scope in / out

## In scope
- add `Zongzu.Modules.NarrativeProjection`
- generate notifications from `WorldDiff` and `DomainEvent` only
- add read-model query contracts for the narrative projection
- integrate the narrative module into the M2 bootstrap/load path
- add an application-side read-model bundle builder for presentation
- add a first-pass `Zongzu.Presentation.Unity` shell that composes view models from projections only
- add tests for diff traceability, shell read-only boundaries, and updated M2 save/integration coverage
- update docs for schema, integration, and M2 acceptance expectations

## Out of scope
- Unity scene authoring, prefabs, or runtime input plumbing
- player command execution from the shell
- authority rule changes in family, education, trade, or social modules
- `OfficeAndCareer`, `OrderAndBanditry`, `ConflictAndForce`, or `WarfareCampaign`
- rich narrative templating, localization, or visitor cutscenes

# Affected modules

- `Zongzu.Kernel`
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Presentation.Unity`
- integration, persistence, narrative, and presentation tests
- docs for schema / integration / acceptance notes

# Save/schema impact

- add `NarrativeProjection` save namespace with schema version `1`
- add notification ID allocation to kernel state
- extend the M2 save profile from six to seven module namespaces
- persist derived notification history only inside the `NarrativeProjection` namespace

# Determinism risk

- notification generation must derive only from ordered `WorldDiff` and `DomainEvent` streams
- narrative projection must not consume extra RNG
- shell view-model composition must remain pure and read-only
- adding the narrative module must preserve deterministic replay hashes for identical seeds and inputs

# Milestones

1. Add `NarrativeProjection` contracts, state model, and module runner in the projection phase.
2. Integrate the narrative module into the M2 bootstrap/load path and notification ID allocation.
3. Add an application read-model bundle builder that exposes only projection-safe data to presentation.
4. Add the first-pass presentation shell project with great hall, lineage, desk sandbox, and notification-center view models.
5. Expand tests and docs so M2 closes with projection traceability and UI authority-boundary coverage.

# Tests to add/update

- module test proving notifications are built from `WorldDiff` and `DomainEvent` traces
- integration test proving M2 notifications can be traced back to diff entries after simulation advance
- updated M2 deterministic replay and save roundtrip tests with the narrative module enabled
- presentation-shell test proving composition uses read-model bundles only
- presentation-shell boundary test proving the presentation project does not reference authority modules directly

# Rollback / fallback plan

- if richer notification grouping becomes noisy, keep one deterministic notice per event/module for now
- if a full shell gets too broad, keep the first pass at view-model composition only and defer concrete Unity scenes
- if save growth becomes noisy, trim notification history to a bounded recent window

# Open questions

- whether later shells should persist read/unread UI state inside `NarrativeProjection` or in a separate presentation-only namespace
- whether future conflict/campaign packs should reuse the same notification trace shape or extend it with richer source metadata
