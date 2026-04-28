# Social Mobility Boundary Closeout V285-V292

## Goal

Close the v213-v284 social mobility / personnel-flow substrate as a first-layer loop:

**near detail, pressure-selected local detail, active-region pools, distant pressure summary.**

This pass is an audit closeout. It records that the current branch is readable and bounded, but it is not a complete society engine, not a full migration economy, not world-scale per-person simulation, and not a player command layer for moving people directly.

## Scope In

- Document the closeout boundary across v213-v284:
  - v213-v244: first fidelity-ring / mobility substrate.
  - v245-v252: first-layer closeout audit.
  - v269-v276: scale-budget guard.
  - v277-v284: influence / scale-budget readback fields.
- Add an architecture guard proving the closeout remains docs/tests governance only.
- Record the stable owner split:
  - `PopulationAndHouseholds` owns livelihood, activity, labor pools, marriage pools, and migration pools.
  - `PersonRegistry` owns identity and existing `FidelityRing` assignment only.
  - `SocialMemoryAndRelations` remains future durable movement-residue owner only.
  - Application assembles projections and diagnostics.
  - UI/Unity copy projected fields only.
- Record explicit future debt for deeper social mobility, migration, personnel flow, dormant stubs, durable movement residue, direct personnel commands, bounded player commands, and cross-region personnel flow.

## Scope Out

- No production rule change.
- No new social mobility engine.
- No full personnel-flow or migration economy.
- No direct player command for moving people.
- No whole-world per-person monthly simulation.
- No dormant-stub demotion loop.
- No durable SocialMemory movement residue.
- No new persisted state, schema version, root save version, migration, manifest change, projection cache, or serialized module payload change.
- No movement, social-mobility, focus, scheduler, command, or personnel ledger.
- No Application command outcome calculation.
- No UI/Unity authority.
- No prose parsing from readback fields, person dossier text, settlement mobility text, receipt prose, notification text, or `DomainEvent.Summary`.

## Affected Modules

- `docs/*`: topology, design audit, boundaries, integration rules, schema docs, simulation docs, fidelity model, UI/presentation, acceptance, and skill alignment notes.
- `tests/Zongzu.Architecture.Tests`: closeout guard.

No runtime module, scheduler, projection, ViewModel, Unity adapter, persistence, schema, or manifest code is planned.

## Save / Schema Impact

Target impact: none.

This pass is docs/tests only. If implementation requires persisted state, stop and split a schema/migration plan before coding.

## Determinism / Performance Risk

No runtime determinism or performance risk is expected because this pass changes no scheduler, module, projection, save, or Unity implementation code.

The risk being guarded is future drift: deeper personnel-flow work must name hot path, expected cardinality, deterministic cap/order, owner module, schema impact, no-touch boundary, and validation lane before adding rule density.

## Milestones

1. Create this ExecPlan and confirm the no-schema target.
2. Update docs with v285-v292 closeout wording and future debt.
3. Add an architecture guard for boundary/no-authority/no-schema drift.
4. Run focused architecture/build/diff/full validation.
5. Archive this plan with validation evidence after merge readiness.

## Tests To Add / Update

- Architecture guard proving v285-v292 documents the v213-v284 closeout, future debt, no production rule change, no schema/migration, no ledger/cache/manager, no prose parsing, no Application/UI/Unity authority, and no `PersonRegistry` domain expansion.

## Rollback / Fallback

If the guard becomes too broad, narrow it to the stable invariants: `PopulationAndHouseholds` owns pools, `PersonRegistry` remains identity/fidelity only, social memory residue is future work, distant society remains summarized, and no production manager/ledger/schema drift is introduced.

## Implementation Evidence

- Added v285-v292 closeout wording to topology, design audit, module boundaries, integration rules, schema docs, simulation docs, fidelity model, UI/presentation docs, acceptance tests, and skill rationalization matrix.
- Added `Social_mobility_boundary_closeout_v285_v292_must_document_first_layer_only_without_schema_or_authority_drift` to architecture tests.
- No production module, scheduler, projection, ViewModel, Unity adapter, persistence, schema, or manifest code changed.
- Save/schema result: none. The pass is docs/tests only.

## Validation Evidence

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "FullyQualifiedName~Social_mobility_boundary_closeout_v285_v292"` passed: 1/1.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings / 0 errors.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed: 0 failed, full solution green.

## Closure

Completed and ready to archive. V285-V292 closes v213-v284 only as a first-layer mobility/personnel-flow substrate. It does not add runtime authority, schema, migration, direct personnel command ownership, or a complete society engine.
