Result: completed on 2026-04-18. Added warfare-aftermath fallout for civilian livelihood, settlement prosperity/security, and clan standing through module-owned handlers only, with focused module/integration coverage and no save-schema bump.

## Goal
Extend `WarfareCampaign.Lite` aftermath so it also lands in civilian livelihood, settlement prosperity/security, and clan reputation/support, while keeping all writes inside the owning modules and preserving the campaign-level sandbox model.

## Scope in / out
### In
- Let `PopulationAndHouseholds` react to settlement-targeted warfare aftermath through owned household and settlement-pressure state.
- Let `WorldSettlements` react to warfare aftermath through owned prosperity/security state only.
- Let `FamilyCore` react to warfare aftermath through owned clan prestige/support state only.
- Add/update tests for deterministic handler behavior and boundary discipline.
- Update docs for integration, boundaries, acceptance, and current fallout coverage.

### Out
- No new save schemas if existing fields are sufficient.
- No direct writes from `WarfareCampaign` into household, settlement, or clan state.
- No detached war mini-game, tactical map, or authority UI.

## Affected modules
- `Zongzu.Modules.PopulationAndHouseholds`
- `Zongzu.Modules.WorldSettlements`
- `Zongzu.Modules.FamilyCore`
- `Zongzu.Integration.Tests`
- docs: `MODULE_BOUNDARIES.md`, `MODULE_INTEGRATION_RULES.md`, `SIMULATION.md`, `ACCEPTANCE_TESTS.md`, `CONFLICT_AND_FORCE.md`, `POST_MVP_SCOPE.md`

## Save/schema impact
- Prefer no root schema bump.
- Prefer no module schema bump.
- Use existing owned fields plus diffs/events only.

## Determinism risk
- Low to medium.
- Risks:
  - handler ordering drift across newly-reactive early modules
  - over-applying the same warfare bundle inside multiple household/clan loops
- Controls:
  - build settlement bundles in stable order
  - compute bounded deterministic deltas from `IWarfareCampaignQueries`
  - rebuild population summaries after household fallout updates

## Milestones
1. Add handler support in `PopulationAndHouseholds`, `WorldSettlements`, and `FamilyCore`.
2. Add module tests for livelihood, prosperity, and reputation fallout.
3. Extend campaign-enabled integration assertions.
4. Update docs and verify build/tests.

## Tests to add/update
- `PopulationAndHouseholdsModuleTests`
- `WorldSettlementsModuleTests`
- `FamilyCoreModuleTests`
- `M2LiteIntegrationTests`

## Rollback / fallback plan
- If one fallout line proves too noisy, keep the event contract and remove only that module's handler while leaving others intact.
