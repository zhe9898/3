# Backend Contract Health v33: No-Unclassified Debt Gate

## Goal
- Convert the v32 diagnostic event-contract classifications into a hard health gate: current emitted-without-authority-consumer and declared-but-not-emitted `DomainEvent` contract debt must not silently remain `Unclassified`.
- Keep the pass diagnostic/readback/test-only. It is not a new gameplay rule, event pool, command system, pressure formula, projection surface, or event-chain design.
- Preserve `DomainEvent` as one deterministic fact-propagation tool after module rules resolve, not the design body of a pressure chain.

## Scope In
- Add a ten-year health assertion that inspects current diagnostic event-contract debt and fails if any entry still classifies as `Unclassified`.
- Add focused coverage proving the gate rejects a synthetic unclassified contract debt entry.
- Keep v32 canonical event-key formatting and classification readback.
- Update docs that record diagnostics, topology evidence, schema impact, and acceptance criteria.

## Scope Out
- No new gameplay command, player affordance, UI text, Unity presentation behavior, or public-life owner-lane behavior.
- No new module event handlers, pressure formulas, county-yamen rules, household economy rules, route repair rules, SocialMemory writers, or AI behavior.
- No event pool, event-chain core loop, broad fanout, scheduler phase, manager/controller layer, or `PersonRegistry` expansion.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, projection wording, or SocialMemory summary text.

## Affected Modules
- Runtime modules: none targeted for authority-rule changes.
- `tests/Zongzu.Integration.Tests`: ten-year simulation health diagnostics and focused no-unclassified gate test.
- `tests/Zongzu.Architecture.Tests`: guard that event-contract diagnostics remain runtime-only and now include the no-unclassified gate.
- Documentation:
  - `docs/MODULE_INTEGRATION_RULES.md`
  - `docs/RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`
  - `docs/ACCEPTANCE_TESTS.md`
  - `docs/DESIGN_CODE_ALIGNMENT_AUDIT.md`
  - `docs/DATA_SCHEMA.md`
  - `docs/SCHEMA_NAMESPACE_RULES.md`
  - `docs/SIMULATION.md`
  - `docs/CODEX_SKILL_RATIONALIZATION_MATRIX.md`

## Save / Schema Impact
- Target impact: none.
- V33 adds no persisted state, no root save-version bump, no module schema bump, no migration, no save-manifest membership, no save roundtrip behavior, and no runtime diagnostic ledger.
- If implementation uncovers a need to persist event-health state or any owner-lane/event ledger, stop and document the owning module schema/migration impact before writing code.

## Determinism Risk
- Low. V33 adds assertions and diagnostic helpers around the existing deterministic ten-year run.
- The simulation still advances through the existing scheduler and module-owned rules.
- The gate reads `PublishedEvents`, emitted event keys, and diagnostic consumer counts after the run; it does not mutate state and does not feed back into simulation.

## Milestones
1. Add this ExecPlan with no-schema target.
2. Add a reusable diagnostic debt collector and no-unclassified assertion.
3. Call the no-unclassified assertion from the 120-month health report.
4. Add focused synthetic gate coverage and architecture guard coverage.
5. Update docs with v33 evidence and no schema/migration impact.
6. Run build, focused integration/architecture/Unity tests, `git diff --check`, and full no-build solution tests.
7. Commit and push `codex/backend-contract-health-v33`.

## Tests To Add / Update
- Integration:
  - the ten-year health report fails if current diagnostic debt contains an `Unclassified` entry;
  - synthetic unclassified debt is rejected by the same gate helper;
  - existing v32 classification and canonical event-key tests remain green.
- Architecture:
  - diagnostics remain runtime-only and contain no summary parsing;
  - the no-unclassified gate exists in diagnostics rather than Application/UI/Unity authority code.
- Validation commands:
  - `dotnet build Zongzu.sln --no-restore`
  - focused integration / architecture tests
  - focused Unity presentation tests
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`

## Rollback / Fallback Plan
- If the gate finds real current `Unclassified` debt, classify it explicitly as projection-only receipt, future contract, dormant seeded path, acceptance-test gap, or alignment bug before using the ten-year report as evidence.
- If the debt is an actual owner-rule bug, do not patch owner modules opportunistically in v33; document the failing contract and open a later owner-module ExecPlan.
- If any persisted state appears necessary, stop and convert this plan into a schema/migration plan before code changes.

## Open Questions
- Whether future CI should publish the full event-contract debt table as an artifact. V33 keeps it as test output plus hard assertions.
- Which current `FutureContract` entries should become real owner-module handlers in a later backend pass.

## Evidence Checklist
- [x] ExecPlan created
- [x] no-unclassified diagnostic gate added
- [x] focused synthetic gate test added
- [x] architecture guard updated
- [x] docs updated
- [x] no schema/migration impact documented
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture tests
- [x] focused Unity presentation tests
- [x] 120-month health report rerun with no unclassified contract debt
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] branch committed and pushed
