Result: completed on 2026-04-18. Added a thin deterministic handler seam before projection, settlement-targeted warfare aftermath events, owned-state downstream reactions in trade/order/office/social modules, and replay/integration coverage without changing save schema.

## Goal
Implement `WarfareCampaign.Lite v4` so campaign aftermath produces deterministic downstream consequences for the living world through a thin event-handling seam, while keeping warfare campaign-level, read-only in presentation, and free of unit micro.

## Scope in / out
### In
- Add a deterministic post-simulation event-handling seam that runs before `NarrativeProjection`.
- Let `WarfareCampaign` emit settlement-targeted aftermath events for campaign pressure, strained supply, and war-aftermath review.
- Let downstream modules update only their own state from warfare aftermath:
  - `TradeAndIndustry`
  - `OrderAndBanditry`
  - `OfficeAndCareer`
  - `SocialMemoryAndRelations`
- Surface the resulting downstream traces through existing projections/notifications.
- Add/update tests for determinism, boundary discipline, and campaign-enabled save/load coverage.

### Out
- No tactical map, unit micro, or detached battle game.
- No new player-facing warfare authority UI.
- No new regional terrain/climate authority module.
- No standalone black-route implementation.

## Affected modules
- `Zongzu.Contracts`
- `Zongzu.Scheduler`
- `Zongzu.Modules.WarfareCampaign`
- `Zongzu.Modules.TradeAndIndustry`
- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.OfficeAndCareer`
- `Zongzu.Modules.SocialMemoryAndRelations`
- `Zongzu.Modules.NarrativeProjection`
- tests under warfare/integration/persistence
- docs: boundaries, integration, schema, simulation, acceptance, post-MVP, UI/presentation if projection wording changes

## Save/schema impact
- Prefer no root schema bump.
- Prefer no module schema bump if downstream aftermath can live inside existing owned fields and explanation traces.
- Domain-event shape may gain non-persisted targeting metadata for deterministic event handling.
- Save compatibility notes must explicitly state that the new event-handling seam is runtime-only and not part of save surface.

## Determinism risk
- Medium.
- Risks:
  - event ordering drift if handler pass iterates unstable collections
  - duplicated aftermath application if event snapshots are not bounded per month
  - narrative seeing the wrong event set if handler pass runs after projection
- Controls:
  - snapshot domain events before handler pass
  - run handlers in stable module order
  - keep handler updates limited to module-owned state
  - verify multi-seed replay parity on campaign-enabled path

## Milestones
1. Add thin event-handling seam and stable domain-event targeting metadata.
2. Emit settlement-scoped warfare aftermath events from `WarfareCampaign`.
3. Apply downstream consequences in trade/order/office/social modules.
4. Extend projection/tests/docs and verify build/test.

## Tests to add/update
- Scheduler/event-handling tests proving handlers run before `NarrativeProjection`.
- `WarfareCampaign` tests proving aftermath events carry settlement targeting.
- Integration tests proving campaign-enabled runs push explainable downstream effects into trade/order/office/social without cross-module writes.
- Deterministic replay tests for campaign-enabled path.
- Save roundtrip tests proving no save-surface regression for campaign-enabled manifests.

## Rollback / fallback plan
- If the handler seam proves too invasive, keep the seam but restrict v4 to warfare-owned aftermath summaries only and defer downstream state reactions.
- If a downstream reaction destabilizes replay or boundaries, remove that module’s handler and leave the warfare event contract in place for a later slice.

## Open questions
- Whether downstream handlers should emit their own follow-on events in the same month or only record diffs plus owned-field updates.
- Whether campaign aftermath should influence `PopulationAndHouseholds` in a later slice once commoner war burdens are ready.
