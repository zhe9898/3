## Goal
Lock the repo's default historical grounding to a Northern Song-inspired baseline and correct `OfficeAndCareer.Lite` so office access feels like waiting, recommendation, yamen attachment, and paperwork dependence rather than a straight exam-to-office ladder.

## Scope in
- Add a documented `Northern Song-inspired` grounding note for the current player-facing baseline without turning the repo into a hard-dynasty reenactment.
- Rework `OfficeAndCareer` lite career entry so local-exam success opens a candidate queue rather than immediate appointment.
- Add bounded office-owned pressure for waiting / recommendation and yamen-clerk dependence.
- Update governance-lite read models and shell summaries only as needed to explain the revised pathway.
- Add or update tests for deterministic queue progression, migration, and governance-lite presentation.
- Update docs for baseline, boundaries, schema, and acceptance expectations.

## Scope out
- No dynasty-wide full reskin of every module.
- No exact Northern Song title law or administrative geography simulation.
- No new standalone governance or clerk module.
- No changes to warfare, trade, or family authority rules beyond office-owned wording or query effects already required by the office slice.
- No UI-side authority.

## Affected modules
- `Zongzu.Modules.OfficeAndCareer`
- `Zongzu.Application`
- `Zongzu.Contracts`
- `Zongzu.Presentation.Unity`
- `Zongzu.Persistence.Tests`
- `Zongzu.Integration.Tests`
- grounding/spec docs

## Save/schema impact
- `OfficeAndCareer` schema will move from `2` to `3`.
- New office-owned fields will capture pre-appointment pressure and clerk/yamen dependence inside the office namespace.
- Built-in migration will upgrade `OfficeAndCareer` `1 -> 2 -> 3` for governance-lite loads.
- Root save schema remains unchanged.

## Determinism risk
- Low if appointment progression remains purely state-derived and uses only deterministic random already available in module execution context.
- Migration backfill must use stable inference only.
- Presentation updates must remain read-only and derive from snapshots, not authoritative logic.

## Milestones
1. Add ExecPlan and lock the Northern Song-inspired baseline in core docs.
2. Extend `OfficeAndCareer` state/snapshots for appointment pressure and clerk dependence.
3. Replace direct exam-to-appointment flow with queue / recommendation / yamen-attachment progression.
4. Add migration path and update governance-lite shell summaries.
5. Update tests, docs, build, and verification.

## Tests to add/update
- `OfficeAndCareerModuleTests` for queued candidates, attached-yamen progression, and explainable appointment.
- `SaveMigrationPipelineTests` and `SaveRoundtripTests` for `OfficeAndCareer` `2 -> 3`.
- `M2LiteIntegrationTests` for governance-lite office summaries and determinism under the revised pathway.
- Presentation tests only where office read-model wording or candidate summaries change.

## Rollback / fallback plan
- If schema `3` proves too heavy for this slice, keep the Northern Song grounding docs and queue logic but encode it with existing fields only, then defer explicit clerk dependence to a follow-up.
- If presentation adjustments sprawl, keep shell changes minimal and let the office explanations/read models carry most of the new flavor.
