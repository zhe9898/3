# Social Mobility Scale Budget Guard V269-V276

## Goal

Close the first gap after v213-v252 by making the scale-budget rule explicit:

**near people can become detailed; distant society stays alive through summaries and pools; the world must not be simulated person-by-person everywhere.**

This is a governance and architecture guard pass over the existing social mobility fidelity-ring substrate. It does not add a new mobility system.

## Scope In

- Document that v213-v244 already implemented the first social mobility / fidelity-ring substrate.
- Add a v269-v276 guard for the four precision bands:
  - player household / close orbit: named detail
  - player influence footprint / active pressure: selective detail
  - active chain region: structured pools and settlement summaries
  - distant world: pressure summaries, not per-person hard simulation
- Add architecture test coverage that future code must not introduce an all-world person-simulation manager, global movement ledger, hidden scheduler ledger, or `PersonRegistry` domain expansion.
- Record no save/schema impact.

## Scope Out

- No production rule change.
- No new social mobility engine.
- No full migration economy.
- No per-person whole-world simulation.
- No dormant-stub demotion loop.
- No durable SocialMemory movement residue.
- No new persisted state, schema version, root save version, migration, manifest change, cache, ledger, or manager/controller layer.
- No Application, UI, or Unity authority.
- No prose parsing from person dossier summaries, `DomainEvent.Summary`, receipt text, notification text, or readback prose.

## Affected Modules

- `docs/*`: design, boundary, schema, simulation, UI, acceptance, and skill alignment notes.
- `tests/Zongzu.Architecture.Tests`: scale-budget guard test.

No runtime module code is planned.

## Save / Schema Impact

Target impact: none.

This pass is docs/tests only. If implementation requires persisted state, stop and split a schema/migration plan before coding.

## Determinism / Performance Risk

No runtime determinism or performance risk is expected because no scheduler, module, projection, save, or Unity code should change.

The risk being guarded is future drift: a later mobility or personnel-flow branch must name expected cardinality, deterministic ordering, cap strategy, and owner module before it can deepen simulation fidelity.

## Milestones

1. Create this ExecPlan and confirm the no-schema target.
2. Update docs with the v269-v276 scale-budget guard.
3. Add an architecture test for no whole-world per-person simulation drift.
4. Run focused architecture/build/diff/full validation.
5. Archive this plan with validation evidence after merge readiness.

## Tests To Add / Update

- Architecture guard proving v269-v276 is docs/tests governance only, documents the four precision bands, keeps v213-v244 as the implementation substrate, and forbids whole-world person-simulation managers/ledgers in production source.

## Rollback / Fallback

If the guard becomes too broad, narrow it to the stable invariants: `PersonRegistry` remains identity/fidelity only, `PopulationAndHouseholds` owns pools, distant society uses summaries, and no production manager/ledger/schema drift is introduced.

## Implementation Evidence

- Added v269-v276 scale-budget wording to topology, design audit, module boundaries, integration rules, schema docs, simulation docs, fidelity model, UI/presentation docs, acceptance tests, and skill rationalization matrix.
- Added `Social_mobility_scale_budget_guard_v269_v276_must_prevent_whole_world_person_simulation_drift` to architecture tests.
- No production module, scheduler, projection, ViewModel, Unity adapter, persistence, schema, or manifest code changed.
- Save/schema result: none. The pass is docs/tests only.

## Validation Evidence

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "FullyQualifiedName~Social_mobility_scale_budget_guard_v269_v276"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings / 0 errors.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed: 0 failed, full solution green.

## Closure

Completed and ready to archive. V269-V276 closes only the scale-budget guard over the existing fidelity substrate; it does not add runtime authority, schema, migration, or a complete society engine.
