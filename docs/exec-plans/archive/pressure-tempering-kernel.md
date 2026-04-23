# Pressure Tempering Kernel

> Status: Complete
> Started: 2026-04-23
> Skills: `zongzu-game-design`, `zongzu-ancient-china`

## Goal

Build the first authoritative "pressure tempering / emotional social drive" kernel for Zongzu as a rules-driven living-world subsystem. The goal is not a mood event pool and not a player-facing stat layer. It should let repeated material, lineage, death, trade, exam, and warfare pressures leave durable emotional residue in social memory, and let that residue become queryable pressure for later adult autonomy, command friction, public projection, and clan memory.

The kernel must satisfy the repo truth that the world advances first, state changes drive projection, and people respond through bounded motives such as fear, shame, grief, anger, obligation, hope, trust, restraint, and exhaustion rather than pure spreadsheet optimization.

## Scope In

- Extend `SocialMemoryAndRelations` as the owning module for emotional residue and pressure-tempering state.
- Keep `FamilyCore` personality traits as read-only inputs through `IFamilyCoreQueries`.
- Use existing population, trade, family, exam, death, and warfare state/events as pressure inputs.
- Add query/read-model surfaces for clan-level emotional climate and individual pressure tempering.
- Add deterministic formulas that:
  - transform repeated pressure into durable tempering/residue,
  - cool pressure over time,
  - keep public clan narrative, private memory, and personal motive state distinct,
  - preserve adult autonomy by making pressure influence future decisions instead of guaranteeing compliance.
- Add DomainEvent receipts only after `SocialMemoryAndRelations` mutates its own state.
- Update schema/version/migration docs and tests.

## Scope Out

- No new global `WorldManager`.
- No event-pool gameplay loop.
- No UI authority rules.
- No player command buttons for directly editing emotion.
- No full mental-health model or modern therapy language in player-facing surfaces.
- No hard-coded historical rail.
- No cross-module writes into `FamilyCore`, `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, `OrderAndBanditry`, `ConflictAndForce`, `WarfareCampaign`, or `PublicLifeAndRumor`.
- No full command-resolution rework. This slice exposes query seams and receipts; later commands may consume them.

## Affected Modules

- `Zongzu.Contracts`
  - new pressure-tempering snapshot contracts,
  - new SocialMemory event constants,
  - new memory subtype / emotional axis enums if needed,
  - query additions on `ISocialMemoryAndRelationsQueries`.
- `Zongzu.Modules.SocialMemoryAndRelations`
  - schema bump from `2` to `3`,
  - state additions for clan emotional climate and person tempering,
  - xun/month formulas,
  - event handlers for currently thin memory entry points,
  - query implementation.
- `Zongzu.Application`
  - register SocialMemory schema `2 -> 3` migration.
- Tests
  - focused `SocialMemoryAndRelations` formula / handler / query tests,
  - save migration tests,
  - integration scheduler test for a real pressure event flowing into SocialMemory.
- Docs
  - `DATA_SCHEMA.md`,
  - `SCHEMA_NAMESPACE_RULES.md`,
  - `MODULE_BOUNDARIES.md`,
  - `MODULE_INTEGRATION_RULES.md`,
  - `RELATIONSHIPS_AND_GRUDGES.md`,
  - `SIMULATION.md`,
  - `ACCEPTANCE_TESTS.md`.

## Schema / Save Impact

- `SocialMemoryAndRelations` module schema bumps from `2` to `3`.
- Additive fields only:
  - clan emotional climate / tempering state list,
  - person emotional tempering state list,
  - optional last trace / last updated dates.
- Migration `2 -> 3` initializes empty lists and backfills clan climate from existing `ClanNarrativeState` pressures conservatively.
- No root schema bump.
- Runtime DomainEvent metadata remains non-save state.

## Query / Command / DomainEvent

### Queries

- Extend `ISocialMemoryAndRelationsQueries` with:
  - `GetClanEmotionalClimate(ClanId)`,
  - `GetClanEmotionalClimates()`,
  - `FindPersonTempering(PersonId)`,
  - `GetPersonTemperingsByClan(ClanId)`.

Queries are read-only and clone module-owned state.

### Commands

- No new commands in this slice.
- Existing social commands (`Apologize`, `Compensate`, `RestrainRetaliation`, `PubliclyHonorOrShame`) remain declared but not broadened into a new authority lane.

### DomainEvent

New SocialMemory-owned receipts only after state mutation:

- `SocialMemoryAndRelations.PressureTempered`
- `SocialMemoryAndRelations.EmotionalPressureShifted`

Existing compatibility events remain unprefixed.

## Determinism

- No wall clock, filesystem, network, random IO, or unordered dictionary iteration in authority paths.
- All formula iteration ordered by typed id value.
- Event handling filters by event type, module key, `EntityKey`, and metadata before mutation.
- No recursive event emission loops; new receipts are terminal social-memory facts and not consumed by `SocialMemoryAndRelations` itself.
- Formula clamping is explicit and integer-only.

## Historical / Design Grounding

- Use pressure as historically grounded social force, not modern mood sim:
  - shame / face from public exposure, failed duty, relief refusal, exam failure,
  - fear from death, disorder, war, illness, route insecurity,
  - grief from child loss, elder death, violent death,
  - obligation / trust from relief, marriage alliance, protection, mediation,
  - anger / grudge from violent harm, debt breach, abandonment, coercion,
  - hope / aspiration from study progress, exam success, social recovery.
- The owning locus is clan/person, not "all society".
- Broad imperial or disaster pressure must be mediated through existing module events or queries before SocialMemory mutates.

## Milestones

### Milestone 1 - Contracts, Schema, and ExecPlan

Status: Complete

- Add contracts for emotional axes / snapshots.
- Add SocialMemory event constants.
- Add SocialMemory schema `3` state and migration.
- Register migration.
- Update schema docs and namespace docs.

### Milestone 2 - Xun / Month Pressure-Tempering Formulas

Status: Complete

- Implement deterministic clan climate and person tempering updates.
- Read `FamilyCore` personality traits through queries.
- Read sponsored household pressure and clan trade pressure.
- Convert repeated pressure into durable restraint / hardening / bitterness / trust / volatility.
- Keep xun quiet by default; month can emit receipts when thresholds cross.

### Milestone 3 - Event Pressure Inputs

Status: Complete

- Fill currently thin SocialMemory event dispatches for:
  - trade shock,
  - violent death,
  - branch / heir events,
  - marriage alliance,
  - exam pass/fail/study abandonment if contract metadata can identify clan/person.
- Filter scope before mutation.
- Emit SocialMemory receipts only after owned state changes.

### Milestone 4 - Query, Projection-Ready Read Models, and Tests

Status: Complete

- Add focused tests for:
  - formulas use multiple dimensions,
  - personality affects personal tempering,
  - event handlers mutate only scoped clan/person,
  - receipts are registered in `PublishedEvents`,
  - migration preserves key sets and initializes defaults.
- Add at least one real scheduler integration test proving upstream pressure can reach SocialMemory in the same month without manual handler wiring.

### Milestone 5 - Docs, Validation, Commit, Push, PR

Status: Complete

- Update architecture/product docs listed above.
- Run `dotnet build`.
- Run relevant `dotnet test`.
- Run broader tests if time and dependencies allow.
- Mark this ExecPlan complete with result / residual risk.
- Commit, push, and update the existing PR.

## Tests To Add / Update

- `tests/Zongzu.Modules.SocialMemoryAndRelations.Tests`
  - schema / query / formula / event handler tests.
- `tests/Zongzu.Persistence.Tests`
  - SocialMemory schema `2 -> 3` migration test.
- `tests/Zongzu.Integration.Tests`
  - scheduler pressure-to-memory test using real modules and seeded state.
- Existing architecture / boundary tests should continue to pass.

## Rollback / Fallback Plan

- If formulas prove too broad, keep schema/query contracts and reduce formulas to clan climate only.
- If person tempering creates excessive coupling, leave person state sparse and only update persons explicitly referenced by events or clan membership.
- If integration test needs a module not available in default manifests, use an explicit feature manifest in the test and document it.
- If migration blocks build, keep schema at `2` and expose derived read-only snapshots only, then open a follow-up plan for persisted tempering.

## Open Questions

- None blocking at start. The current docs already identify `SocialMemoryAndRelations` as owner of memory, grudge, shame, fear, favor, and public/private residue, with `FamilyCore` personality as read-only input.

## Progress Log

- 2026-04-23: Read `zongzu-game-design`, `zongzu-ancient-china`, mandatory repo docs, `PLANS.md`, and exec-plan instructions. Code facts confirm SocialMemory schema `2`, existing clan narratives / memories / dormant stubs, FamilyCore personality query, xun/month cadence, and deterministic event drain.
- 2026-04-23: Implemented SocialMemory schema `3`, emotional-axis / snapshot contracts, query additions, pressure-tempering receipts, module state, migration registration, deterministic xun/month formulas, and scoped event pressure handlers.
- 2026-04-23: Added focused SocialMemory tests for multidimensional pressure, personality-shaped tempering, scoped trade shock, exam aspiration memory, and published receipts. Added real scheduler test for `ExamPassed` draining into SocialMemory and migration coverage for schema `2 -> 3`.
- 2026-04-23: Fixed a save-compatibility bug found by migration tests: persisted `GameDate` fields in climate/tempering records now have valid defaults and migration normalization.
- 2026-04-23: Updated schema, namespace, boundary, integration, relationship, simulation, and acceptance docs. Long-run diagnostics budgets were raised with explicit SocialMemory schema-v3 payload rationale.
- 2026-04-23: Validation passed: `dotnet build Zongzu.sln -p:UseSharedCompilation=false`; focused SocialMemory tests; Persistence tests; Integration tests; and full `dotnet test Zongzu.sln --no-build -p:UseSharedCompilation=false`.

## Result

Complete. The pressure-tempering kernel is now an authoritative `SocialMemoryAndRelations` subsystem rather than a projection-only concept. It persists clan emotional climate and adult person tempering, exposes read-only queries, reacts to scoped upstream events, emits terminal social-memory receipts, and migrates legacy social-memory saves from schema `2` to `3`.

## Residual Risk / Follow-Up

- Trade events currently need scoped `EntityKey` / metadata to enter social-memory pressure safely. The handler deliberately ignores unscoped legacy trade events to avoid broad false writes.
- `PressureTempered` / `EmotionalPressureShifted` are terminal receipts today. Future command-resolution work can read them through queries, but must avoid consuming them as a new event-pool loop.
- The long-run diagnostics payload budget increased because schema `3` persists bounded per-clan/per-person ledgers. The budget was updated with explicit rationale; future scale work should watch payload growth as fidelity rings expand.
