# Goal

Extend the first post-MVP black-route authority slice so local-force response no longer reads as one flat penalty. `OrderAndBanditry` should now own the difference between roads being actively protected and suppression provoking backlash, while `TradeAndIndustry` continues to react through queries only.

# Scope in / out

## In scope
- keep black-route pressure owned by `OrderAndBanditry`
- keep gray-route / illicit-ledger state owned by `TradeAndIndustry`
- preserve office-mediated paper-compliance and implementation-drag fields already added to `OrderAndBanditry`
- add order-owned `RouteShielding` and `RetaliationRisk` summaries so force response can hand off two distinct outcomes
- let `TradeAndIndustry` read those summaries only through `IBlackRoutePressureQueries`
- keep the ownership split explicit:
  - `OrderAndBanditry` owns pressure, coercion, suppression relief, response activation mirrors, paper compliance, implementation drag, route shielding, retaliation risk, suppression windows, and escalation bands
  - `TradeAndIndustry` owns shadow price, diversion share, illicit margin, blocked shipments, seizure risk, diversion bands, route constraints, and per-route blockage mirrors
- update built-in migrations, persistence coverage, integration coverage, and docs for the new order schema version

## Out of scope
- no UI or presentation work
- no new module key or detached `BlackRoute` save namespace
- no outlaw camp simulation or actor-level black-market pack
- no new cross-module write path from force, office, or trade into order
- no new detached order surface; bounded public-life order verbs may deepen on the same settlement lane

# Affected modules

- `Zongzu.Modules.OrderAndBanditry`
- `Zongzu.Modules.TradeAndIndustry`
- `Zongzu.Contracts`
- `Zongzu.Application`
- `Zongzu.Persistence`
- `tests/Zongzu.Modules.OrderAndBanditry.Tests`
- `tests/Zongzu.Modules.TradeAndIndustry.Tests`
- `tests/Zongzu.Integration.Tests`
- `tests/Zongzu.Persistence.Tests`

# Save/schema impact

- bump `OrderAndBanditry` schema from `3` to `4`, then from `4` to `5` for explicit intervention receipts, then from `5` to `6` for one-month intervention follow-through state
- keep `TradeAndIndustry` schema at `3`
- preserve built-in default migration steps:
  - `OrderAndBanditry` `1 -> 2 -> 3 -> 4 -> 5 -> 6`
  - `TradeAndIndustry` `1 -> 2 -> 3`
- no root schema bump
- no new module key
- black-route data must remain inside the `OrderAndBanditry` and `TradeAndIndustry` envelopes only

# Determinism risk

- monthly pressure updates must keep stable settlement iteration order
- trade may react only through read-only queries and emitted events, not direct mutation
- `RouteShielding` and `RetaliationRisk` backfills must stay conservative so legacy saves remain loadable
- force-response refinement must not create hidden same-month write loops across `ConflictAndForce`, `OrderAndBanditry`, and `TradeAndIndustry`

# Milestones

1. Extend order-owned state and query seams with route-shielding and retaliation-risk summaries.
2. Feed those summaries into deterministic monthly pressure rules in `OrderAndBanditry`.
3. Make `TradeAndIndustry` distinguish shielding relief from retaliation backlash while keeping ownership local.
4. Register the schema `3 -> 4 -> 5 -> 6` migration and update persistence coverage.
5. Extend the same public-life order lane with explicit bounded intervention receipts and additional order verbs.
6. Collapse public-life order receipts onto the explicit last-intervention path and prevent duplicate desk-surface receipts for the same settlement/module lane.
7. Let recent order interventions carry one monthly follow-through inside `OrderAndBanditry`, with `TradeAndIndustry` reacting through queries only.
8. Surface the resulting carryover / shielding / backlash split through runtime-only diagnostics and hotspot summaries.
9. Let bounded order interventions read governance-lite office reach through queries so immediate command strength no longer assumes perfect local execution.
10. Project the same governance-lite execution reach back into public-life order affordances as read-only execution outlook, without adding new authority state.
11. Let recent order-intervention carryover feed back into next-month office burden through query seams only, while `OfficeAndCareer` still owns all resulting task / petition / leverage state.
12. Project that next-month office aftermath back onto public-life order receipts as read-only execution context, without creating a second command lane.
13. Extend runtime-only observability / hotspot summaries so order-linked office aftermath becomes visible as read-only administrative follow-through.
14. Add an application-layer governance-lane projection that joins public-life, order, and office summaries for the same settlement without creating module-owned synthetic state.
15. Let that governance lane nominate one read-only next-step public-life affordance from the existing command projections, without inventing a second command-resolution path.
16. Derive one bundle-level lead governance focus from those governance lanes so future hall surfaces can read a single monthly docket headline without re-sorting in UI.
17. Derive one bundle-level governance docket from that lead governance focus plus same-settlement notification context, still as read-only application projection rather than UI-owned sorting logic.
18. Let that governance docket also carry one same-settlement recent handling receipt from existing command projections, still as read-only application join rather than a second command ledger.
19. Let that governance docket also derive one hall-ready current phase label/summary from governance pressure plus existing receipts/notifications, still as read-only application projection rather than workflow authority state.
20. Derive one read-only `HallDocketStack` from family lifecycle, governance, and warfare-aftercare projections, with one lead item plus secondary items for future hall surfaces.
21. Add neutral ordering/provenance fields to each `HallDocketStack` item so future hall adapters can explain rank/source without importing UI object grammar into shared contracts.
22. Update docs and run full build/test verification.

# Tests to add/update

- `OrderAndBanditry` module tests for activated-response shielding, calm no-leak behavior, and retaliation-risk presence
- `TradeAndIndustry` module tests proving shielding and retaliation are read as different pressures
- integration tests proving the new fields remain inside `OrderAndBanditry` while trade still reacts through queries only
- persistence tests proving legacy `OrderAndBanditry` schema `1` saves migrate through `2 -> 3 -> 4 -> 5 -> 6`
- preflight migration tests proving the black-route slice still stays inside the `OrderAndBanditry` and `TradeAndIndustry` envelopes

# Rollback / fallback plan

- if same-month force-response refinement becomes too noisy, keep the fields but lower their contribution thresholds
- if migration reconstruction proves too speculative, backfill conservative zero-or-derived defaults and let later months rebuild richer pressure
- if trade-side comparison tests become flaky, keep the ownership split and narrow assertions to monotonic route-risk / diversion differences

# Open questions

- how far later bounded commands should write directly into route shielding / retaliation, versus continuing to influence them only through force/order/office upstream state
- which runtime-only diagnostics best expose the difference between protected traffic and backlash-heavy suppression during larger stress sweeps
