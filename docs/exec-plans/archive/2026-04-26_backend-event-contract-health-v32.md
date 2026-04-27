# Backend Contract Health v32: DomainEvent Contract Classification

## Goal
- Add an explicit health classification for `DomainEvent` contract debt that appears in the ten-year diagnostic report.
- Make emitted-but-unconsumed and declared-but-not-emitted events readable as one of: projection-only receipt, future contract, dormant seeded path, acceptance-test gap, or alignment bug.
- Keep `DomainEvent` as a fact-propagation tool after rules resolve. This is not an event-pool design and not the design body of a pressure chain.

## Scope In
- Improve the existing ten-year health report/readback so event contract debt is categorized instead of printed as an undifferentiated list.
- Normalize diagnostic event keys so module-prefixed event names are not double-prefixed in the report.
- Add focused tests proving current known contract-health debt has explicit classifications.
- Update docs that describe module integration, acceptance criteria, schema impact, and topology freeze rules.

## Scope Out
- No new gameplay command, player affordance, projection wording, or public-life owner-lane behavior.
- No new pressure formula, route repair rule, county-yamen rule, household economy rule, SocialMemory residue writer, or AI behavior.
- No event pool, event-chain core loop, or broad fanout.
- No `WorldManager`, `PersonManager`, `CharacterManager`, god controller, or `PersonRegistry` expansion.
- No parsing of `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`.

## Affected Modules
- Runtime modules: none targeted for authority-rule changes.
- `tests/Zongzu.Integration.Tests`: ten-year simulation health diagnostics and focused contract-health tests.
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
- V32 adds no persisted state, no module schema bump, no root save-version bump, no migration, no save-manifest membership, and no save roundtrip behavior.
- If the work uncovers a need for persisted event-health state or ledgers, stop and write the owning module schema/migration impact before implementation.

## Determinism Risk
- Low. V32 changes diagnostics and tests only.
- The ten-year simulation still advances through the existing deterministic scheduler and modules.
- Diagnostic classification reads already exposed `PublishedEvents`, `ConsumedEvents`, and emitted event keys; it does not mutate state or feed back into simulation.

## Milestones
1. Add this ExecPlan and record the no-schema target.
2. Add canonical diagnostic event-key formatting and contract-health classifications.
3. Add focused tests for classification coverage and duplicate-prefix prevention.
4. Update docs and acceptance criteria with v32 evidence.
5. Run build, focused integration/architecture tests, `git diff --check`, and full no-build solution tests.
6. Commit and push `codex/public-life-contract-health-v32`.

## Tests To Add / Update
- Focused integration tests:
  - current known emitted-but-unconsumed contract debt has explicit classification;
  - current known declared-but-not-emitted contract debt has explicit classification;
  - module-prefixed event names are not double-prefixed by diagnostics.
- Focused architecture/doc tests if needed to guard no summary parsing and no authority drift.
- Validation commands:
  - `dotnet build Zongzu.sln --no-restore`
  - focused integration / architecture tests
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`

## Rollback / Fallback Plan
- If classification causes noisy or unstable health output, keep the canonical-key fix and move the classification table behind a narrower focused diagnostic helper.
- If tests show actual authority alignment bugs rather than diagnostic debt, do not patch rules opportunistically in v32; document the bug and open a later owner-module ExecPlan.
- If any persisted state appears necessary, stop and convert this plan into a schema/migration plan before code changes.

## Open Questions
- Whether future work should turn some `future contract` entries into real consumed-event handlers. V32 only classifies that debt.
- Whether the ten-year health report should become a generated artifact in CI. V32 keeps it as test output and focused assertions.

## Evidence Checklist
- [x] ExecPlan created
- [x] diagnostic contract classifications added
- [x] focused classification tests added
- [x] docs updated
- [x] no schema/migration impact documented
- [x] `dotnet build Zongzu.sln --no-restore`
- [x] focused integration / architecture tests
- [x] focused Unity presentation tests
- [x] 120-month health report rerun with classified contract debt
- [x] `git diff --check`
- [x] `dotnet test Zongzu.sln --no-build`
- [x] branch committed and pushed
