# Goal
Run a pure diagnostic ten-year health check against the current all-module skeleton and report whether the simulated society shows living pressure, long memory, and cross-module consequence flow.

# Scope In / Out
In:
- add a temporary/integration-test diagnostic runner under `tests/`
- initialize a campaign-enabled all-module sandbox
- sample 120 monthly ticks for event flow, settlement trends, person registry, and social memory
- report findings in the active thread

Out:
- no `src/` business-code changes
- no rule tuning
- no schema, save, or module-boundary changes
- no refactor recommendations in the final design diagnosis

# Affected Modules
- Runtime observation only across all enabled modules:
  - PersonRegistry
  - WorldSettlements
  - FamilyCore
  - PopulationAndHouseholds
  - SocialMemoryAndRelations
  - EducationAndExams
  - TradeAndIndustry
  - PublicLifeAndRumor
  - OfficeAndCareer
  - OrderAndBanditry
  - ConflictAndForce
  - WarfareCampaign
  - NarrativeProjection

# Save / Schema Impact
None. The diagnostic uses existing bootstrap/save paths and does not add module state or migrations.

# Determinism Risk
Low. The test runs the normal deterministic scheduler and only observes state after each month.

# Milestones
1. Add a diagnostic test runner that wraps modules to count event handling.
2. Run 120 months on a campaign-enabled, multi-settlement sandbox.
3. Capture event, trend, person, and social-memory output.
4. Summarize game-design health gaps in the thread.

# Tests To Add / Update
- Add `TenYearSimulationHealthCheckTests.cs` under integration tests.
- Run only the new test for the report.

# Rollback / Fallback Plan
Delete the diagnostic test file and this ExecPlan if the probe should remain temporary-only.

# Open Questions
- Whether future health checks should become a stable budget test or remain a manual design diagnostic.
