# Goal

Bootstrap the M0 modular spine and the first M1 slice for `WorldSettlements` and `FamilyCore` only, using a deterministic pure-C# authority stack with save/version scaffolding and test coverage for determinism, save roundtrip, and module boundary invariants.

# Scope in / out

## In scope
- repository skeleton under `src/` and `tests/`
- solution/projects for kernel, contracts, scheduler, application shell, persistence shell
- module registration and feature manifest support
- deterministic monthly scheduler spine
- save root and module envelope serialization
- first two authoritative modules:
  - `WorldSettlements`
  - `FamilyCore`
- automated tests for:
  - deterministic replay equality
  - save roundtrip
  - module boundary invariants

## Out of scope
- `PopulationAndHouseholds`
- `SocialMemoryAndRelations`
- `EducationAndExams`
- `TradeAndIndustry`
- `NarrativeProjection`
- all post-MVP modules
- Unity presentation implementation
- full M1 simulation richness such as births/deaths/household pressure beyond the minimum skeleton needed to prove architecture

# Affected modules

- `Zongzu.Kernel`
- `Zongzu.Contracts`
- `Zongzu.Scheduler`
- `Zongzu.Application`
- `Zongzu.Persistence`
- `Zongzu.Modules.WorldSettlements`
- `Zongzu.Modules.FamilyCore`
- `Zongzu.Integration.Tests`
- module-specific unit test projects

# Save/schema impact

- introduce initial root save structure with:
  - `RootSchemaVersion`
  - feature manifest
  - kernel state
  - module state dictionary
- introduce first module namespaces:
  - `WorldSettlements`
  - `FamilyCore`
- establish initial module schema versions at `1`
- no migration work yet because this is the initial bootstrap

# Determinism risk

- scheduler phase order must be fixed and test-covered
- module registration order must be explicit rather than reflection-driven
- authoritative randomness must go through kernel RNG only
- serialization roundtrip must preserve module registration/module-state identity without relying on unordered maps for execution

# Milestones

1. Create solution and project skeleton matching the modular monolith layout.
2. Implement kernel primitives, contracts, and deterministic scheduler interfaces.
3. Implement persistence/application shells with root save and feature manifest support.
4. Add `WorldSettlements` and `FamilyCore` with minimal owned state, registration, monthly advancement hooks, and event-safe boundaries.
5. Add determinism, save roundtrip, and module boundary tests; run full build and test pass.

# Tests to add/update

- kernel determinism test for repeatable replay hash across identical runs
- scheduler test for deterministic module registration and monthly execution order
- persistence roundtrip test for root save + module envelopes
- integration test proving the first two modules can advance a minimal world for 12 months deterministically
- boundary invariant tests proving:
  - modules only save into their own namespace
  - module descriptors expose only owned commands/events/state keys
  - disabled modules are absent from the save manifest/state dictionary by default

# Rollback / fallback plan

- keep the implementation limited to empty/minimal deterministic state if richer simulation threatens schedule
- prefer stubbed application/persistence adapters over speculative gameplay logic
- if a module feature introduces boundary ambiguity, remove the feature and keep only registration/state ownership proof

# Open questions

- whether to use MessagePack immediately or start with JSON-backed persistence shell while keeping serializer abstraction stable
- whether `FamilyCore` should include any seeded people/clans in bootstrap fixtures or stay empty except in tests
