# Goal

Implement the remaining M1 substrate modules, `PopulationAndHouseholds` and `SocialMemoryAndRelations`, so the M1 authority layer contains all four foundational modules with deterministic monthly behavior, save support, and boundary-safe integration.

# Scope in / out

## In scope
- add `Zongzu.Modules.PopulationAndHouseholds`
- add `Zongzu.Modules.SocialMemoryAndRelations`
- add query contracts and module registration updates needed for these modules
- extend the bootstrap world so all four M1 modules participate
- add/expand tests for:
  - deterministic replay
  - save roundtrip with all M1 modules
  - household pressure invariants
  - grudge persistence over time
  - disabled-module boundary behavior

## Out of scope
- `EducationAndExams`
- `TradeAndIndustry`
- `NarrativeProjection`
- `OrderAndBanditry`
- `ConflictAndForce`
- `WarfareCampaign`
- UI / Unity implementation
- player command resolution beyond existing command descriptors
- post-MVP campaign simulation

# Affected modules

- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Modules.PopulationAndHouseholds`
- `Zongzu.Modules.SocialMemoryAndRelations`
- `Zongzu.Integration.Tests`
- module-specific test projects

# Save/schema impact

- add initial module save namespaces for:
  - `PopulationAndHouseholds`
  - `SocialMemoryAndRelations`
- establish initial schema version `1` for both modules
- bootstrap saves created before these modules existed remain loadable if the feature manifest leaves them disabled
- no root schema bump planned

# Determinism risk

- cross-module reads must stay query-only
- new modules must iterate households, clans, and memories in stable order
- grudge drift and livelihood pressure must use only kernel RNG and stable thresholds
- save serialization must preserve all four module namespaces without key drift

# Milestones

1. Add new module contracts, projects, and solution wiring.
2. Implement `PopulationAndHouseholds` with deterministic household pressure and published queries/events.
3. Implement `SocialMemoryAndRelations` with deterministic clan narrative/grudge drift fed by family and household context.
4. Update bootstrap, persistence expectations, and boundary validation for four-module M1 worlds.
5. Add and run focused module tests plus expanded integration/save tests.

# Tests to add/update

- deterministic 12-month replay equality with all four M1 modules enabled
- save roundtrip for four-module bootstrap world
- household distress and migration risk stay within invariant bounds
- grudge pressure persists over multiple years instead of resetting instantly
- disabled modules remain absent from save state when the manifest leaves them off

# Rollback / fallback plan

- keep population/social-memory behavior coarse and deterministic if richer lifecycle logic threatens boundary clarity
- prefer clan-level narrative/grudge tracking over prematurely detailed per-person social simulation
- if query coupling becomes unclear, reduce integration to fewer published snapshots rather than adding direct references

# Open questions

- when M2 starts, whether `SocialMemoryAndRelations` should react to explicit domain events or continue with pure phase-based drift until the event-handling pass is expanded
- whether household sponsorship by clan should remain a light optional link or become a broader institutional support model later
