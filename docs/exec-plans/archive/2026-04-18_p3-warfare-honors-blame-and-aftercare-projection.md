Result: completed on 2026-04-18. Added warfare-aftercare contextual notices plus read-only hall / desk / campaign-board docket summaries for merit, blame, relief, and cleanup pressure, without changing authority rules or save schemas.

## Goal
Make warfare-lite aftermath read more like a Chinese-ancient campaign docket by projecting `merit / setback-blame / relief-aftercare` into `NarrativeProjection` and the read-only hall / desk / campaign-board shell, without adding new authority rules.

## Scope in / out
### In
- Enrich `NarrativeProjection` warfare notices with contextual traces pulled from same-settlement fallout diffs.
- Reframe warfare-aftercare notices around honors, blame, relief, and local recovery wording.
- Surface read-only aftermath docket summaries in:
  - great hall
  - desk sandbox settlement nodes
  - warfare campaign boards
- Add/update tests for projection trace quality and read-only shell summaries.
- Update UI / acceptance / post-MVP docs for the new projection layer.

### Out
- No new authoritative warfare rules.
- No new module key or save schema bump.
- No authority UI commands, tactical battle map, or unit micro.
- No direct writes between modules beyond the existing deterministic event-handling seam.

## Affected modules
- `Zongzu.Modules.NarrativeProjection`
- `Zongzu.Presentation.Unity`
- `Zongzu.Integration.Tests`
- docs: `UI_AND_PRESENTATION.md`, `ACCEPTANCE_TESTS.md`, `POST_MVP_SCOPE.md`

## Save/schema impact
- No root schema bump.
- No module schema bump.
- All new signals stay projection-only or shell-only.

## Determinism risk
- Low.
- Risks:
  - projection ordering drifting when warfare notices pull extra contextual traces
  - shell summaries inferring too much from optional notifications
- Controls:
  - keep contextual trace selection deterministic by stable module/entity ordering
  - treat missing notices as empty read-only surfaces, never as implicit state

## Milestones
1. Add warfare-aftercare contextual trace selection in `NarrativeProjection`.
2. Add hall / desk / campaign-board aftermath docket summaries in the read-only shell.
3. Extend projection / shell / integration tests.
4. Update docs and verify build/tests.

## Tests to add/update
- `NarrativeProjectionModuleTests`
- `FirstPassPresentationShellTests`
- `M2LiteIntegrationTests`

## Rollback / fallback plan
- If the new wording proves noisy, keep the contextual traces but collapse the new summaries back to simpler generic aftermath wording.
