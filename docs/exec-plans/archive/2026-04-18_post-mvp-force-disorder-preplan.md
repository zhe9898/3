## Goal
- capture the first structural seam for post-MVP black-route / black-market depth and warfare depth without implementing those systems yet

## Scope in
- clarify which existing modules future disorder and war features extend
- document hard integration constraints for `TradeAndIndustry`, `OrderAndBanditry`, `ConflictAndForce`, and `WarfareCampaign`

## Scope out
- no implementation
- no schema changes
- no new modules in code

## Affected docs
- `POST_MVP_SCOPE.md`
- `CONFLICT_AND_FORCE.md`
- M3 interaction hardening notes as needed

## Save/schema impact
- none in this planning-only slice

## Determinism risk
- none directly

## Notes
- black-route / black-market depth should extend `OrderAndBanditry` plus `TradeAndIndustry`, not create a detached economy lane
- warfare should extend `ConflictAndForce` into `WarfareCampaign`, not create a second force model
- future black-route depth should treat local force response activation as queryable pressure relief only; order/disorder remains owned by `OrderAndBanditry`
- future warfare depth should promote explicit local force posture into campaign-owned mobilization state through commands/events rather than by reusing settlement force state as a hidden battle board

## Checklist before implementation
- define black-route ledgers versus ordinary trade ledgers
- define which disorder outcomes stay settlement-local and which escalate into regional/campaign scope
- define command/event handoff from local force posture into campaign mobilization
- define acceptance tests for black-route suppression, failed suppression, and campaign aftermath before adding rules
