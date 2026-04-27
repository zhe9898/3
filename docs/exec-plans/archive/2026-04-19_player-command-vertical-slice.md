## Goal
Add the first playable player-command vertical slice so the current hall / desk / campaign shell can expose bounded, application-routed commands and immediately readable command receipts without moving authority rules into UI.

## Scope in
- Add a thin application-routed player command service for the current enabled office and warfare slices.
- Expose read-only player command affordances and recent command receipts through the presentation read-model bundle.
- Surface those affordances and receipts in the existing hall / office / warfare shell only.
- Add tests that prove UI remains read-only, commands stay routed through application services, and disabled module paths do not leak commands.
- Update docs for player-command boundaries and acceptance coverage.

## Scope out
- No new save namespace or root schema change.
- No detached command queue subsystem.
- No treasury, family, or trade authority commands in this slice.
- No black-route rules.
- No Unity scene / prefab implementation beyond the current read-model shell.

## Affected modules
- `Zongzu.Application`
- `Zongzu.Contracts`
- `Zongzu.Presentation.Unity`
- `OfficeAndCareer`
- `WarfareCampaign`
- tests and docs

## Save/schema impact
- No authoritative save-schema expansion is planned.
- Command affordances and receipts stay inside read-only presentation/export contracts and are rebuilt from current authoritative state.
- Office command handling should reuse existing office-owned fields and jurisdiction rebuilds rather than adding new saved fields.

## Determinism risk
- Low if command routing stays synchronous and avoids new randomness.
- Same input command on the same simulation state must yield the same authoritative result.
- No UI-side logic may alter command availability or outcomes.

## Milestones
1. Add ExecPlan and map current office / warfare command seams.
2. Implement a unified application-routed player command service for the first office + warfare commands.
3. Export read-only command affordances and receipts in the presentation bundle.
4. Surface those affordances and receipts in the shell with player-facing wording appropriate to the setting.
5. Add integration/presentation acceptance tests.
6. Update docs and run build/test verification.

## Tests to add/update
- Integration test for command affordance visibility on campaign-enabled / governance-enabled paths.
- Integration test for routed office command results reflected in read models.
- Integration test for routed warfare command results reflected in read models through the unified service.
- Presentation test proving command surfaces consume read models only.
- Acceptance doc updates for disabled-module command hiding and read-only UI command boundaries.

## Rollback / fallback plan
- If unified routing causes scope creep, keep warfare on its existing service and land office routing plus read-model affordances first.
- If player-facing receipts require too much wording churn, fall back to a thinner receipt summary built from existing traces while keeping the service and affordance wiring.

## Open questions
- Keep the first office slice at petition review + administrative leverage only, or also surface relief disbursement wording in this pass?
- Whether the hall should summarize command availability directly, or leave commands scoped to office / warfare surfaces only.
