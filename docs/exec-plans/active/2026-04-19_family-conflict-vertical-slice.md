## Goal
Add the first family-conflict vertical slice so lineage disputes, mediation, favoritism, separation pressure, and relief sanctions can surface through existing family/social-memory modules and the read-only ancestral-hall shell.

## Scope in
- Extend `FamilyCore` owned state with lite family-conflict pressure, command receipts, and migration support.
- Route a first set of family commands through the existing application command seam.
- Let `SocialMemoryAndRelations` react to family-conflict pressure through read-only family queries.
- Surface family conflict summaries, command affordances, and receipts in the lineage / ancestral-hall read-only shell.
- Add narrative titles / next-step wording for family-conflict events.
- Update tests and docs for boundaries, save schema, and player-facing behavior.

## Scope out
- No new standalone family-conflict module.
- No adult micro-agency overhaul, marriage overhaul, or inheritance-law simulation pack.
- No direct force escalation /械斗 slice in this pass.
- No authority rules in UI.
- No detached petition or black-route work.

## Affected modules
- `Zongzu.Modules.FamilyCore`
- `Zongzu.Modules.SocialMemoryAndRelations`
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Application`
- `Zongzu.Contracts`
- `Zongzu.Presentation.Unity`
- save migration tests, integration tests, docs

## Save/schema impact
- `FamilyCore` schema will move from `1` to `2`.
- New lineage-conflict fields remain owned by `FamilyCore`.
- Built-in `1 -> 2` migration backfills neutral defaults for legacy saves.
- Player-command targeting and family read models remain runtime/read-only and are not new authoritative namespaces.

## Determinism risk
- Low if command routing stays synchronous and state changes remain fully module-owned.
- Family commands must not introduce randomness.
- Social-memory reaction must depend only on current authoritative state.

## Milestones
1. Add ExecPlan and map family / social-memory / projection seams.
2. Extend `FamilyCore` state, queries, and migration path for lineage-conflict lite fields.
3. Route the first family commands through `PlayerCommandService`.
4. Surface family summaries, command affordances, and receipts in the lineage shell.
5. Add family conflict event/projection wording and social-memory fallout.
6. Update tests, docs, build, and test verification.

## Tests to add/update
- `FamilyCoreModuleTests` for lineage-conflict drift and command-owned outcomes.
- `SocialMemoryAndRelationsModuleTests` for family-conflict pressure affecting narrative/grudge memory.
- `NarrativeProjectionModuleTests` for family-conflict event titles / next steps.
- `M2LiteIntegrationTests` for family command affordances/receipts and read-only lineage shell behavior.
- `SaveMigrationPipelineTests` and save roundtrip coverage for `FamilyCore` `1 -> 2`.

## Rollback / fallback plan
- If cross-module fallout grows too wide, land only `FamilyCore` state + command receipts + lineage shell and defer social-memory projection to a follow-up.
- If command targeting complicates the shared read-model too much, keep the target limited to clan IDs while leaving office/warfare targeting unchanged.
