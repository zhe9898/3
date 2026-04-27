# Goal

Implement `EducationAndExams.Lite` and `TradeAndIndustry.Lite` on top of the existing M1 substrate so that the monthly simulation supports lite exam and trade pathways with deterministic state updates, save compatibility, and explainable outcomes.

# Scope in / out

## In scope
- add `Zongzu.Modules.EducationAndExams`
- add `Zongzu.Modules.TradeAndIndustry`
- add query contracts and bootstrap data for both lite modules
- connect both modules into the existing monthly simulation order
- add deterministic tests for exam and trade outcomes
- expand save roundtrip and integration tests to cover the six-module M2-lite world
- update docs where schema/integration details need to reflect the new module state

## Out of scope
- `NarrativeProjection`
- `Presentation.Unity`
- `OfficeAndCareer`
- `OrderAndBanditry`
- `ConflictAndForce`
- `WarfareCampaign`
- full office progression from exam success
- full trade networks, guilds, caravans across multiple regions
- command-resolution UI surfaces

# Affected modules

- `Zongzu.Kernel`
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Modules.EducationAndExams`
- `Zongzu.Modules.TradeAndIndustry`
- `Zongzu.Integration.Tests`
- module-specific test projects
- docs for schema / integration notes

# Save/schema impact

- add initial module save namespaces for:
  - `EducationAndExams`
  - `TradeAndIndustry`
- establish initial schema version `1` for both modules
- extend the bootstrap save manifest for the M2-lite profile
- preserve clean loading when the M2 modules are disabled in the feature manifest

# Determinism risk

- exam attempts and trade outcomes must use only kernel RNG
- execution order inside the shared upward-mobility/economy phase must stay fixed
- outcome explanations must derive from deterministic inputs rather than ad hoc text-only logic
- save serialization must preserve six module namespaces with stable replay hashes

# Milestones

1. Add contracts, project wiring, and bootstrap hooks for the two M2-lite modules.
2. Implement `EducationAndExams.Lite` with study state, academy capacity, exam attempts, pass/fail outcomes, and explanation traces.
3. Implement `TradeAndIndustry.Lite` with clan trade state, market/route pressure, profit/loss/debt outcomes, and explanation traces.
4. Extend bootstrap data and save coverage for a six-module M2-lite world.
5. Add and run module, integration, and persistence tests for deterministic replay, explainability, and disabled-module boundaries.

# Tests to add/update

- deterministic multi-month replay test with the six-module M2-lite world
- save roundtrip test covering all six module namespaces
- exam outcome test proving pass/fail results carry explanation text
- trade outcome test proving profit/loss results carry explanation text
- disabled-module test proving education/trade state stays absent when the feature manifest leaves them off

# Rollback / fallback plan

- keep exam progression single-stage and local if richer rank ladders threaten scope
- keep trade local and route-light if broader market modeling threatens clarity
- prefer readable internal explanations over premature narrative projection output

# Open questions

- whether M2-lite should stop at internal explanation state or also emit domain-event traces intended for a future narrative-projection module
- whether trade should stay clan-centric in lite scope or add household-scale petty commerce in a follow-up slice
