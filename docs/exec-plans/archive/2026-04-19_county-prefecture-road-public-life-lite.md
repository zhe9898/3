## Goal

Implement a thin Northern Song-inspired `PublicLifeAndRumor.Lite` slice that makes the desk sandbox feel like a living county society rather than a pile of private household numbers. The slice should spatialize `乡里 / 镇市 / 县城 / 州府牒报 / 路报` pressure without taking ownership away from existing modules.

## Why now

- the repo already has lineage, population, market, office, disorder, conflict, and campaign-lite layers
- the user wants a living social world, not only route families or clan-only simulation
- the ancient-China grounding pass shows that public life must surface through markets, yamen gates, ferries, inns, notices, rumor, and county/prefecture pressure rather than staying inside household or office math

## Scope

### 1. World-settlement grounding
- upgrade `WorldSettlements` to schema `2`
- add a lightweight settlement tier so player-facing nodes can distinguish:
  - `乡里`
  - `镇市`
  - `县城`
  - `州府`
- keep tier ownership in `WorldSettlements`; do not let presentation invent node rank

### 2. New module: `PublicLifeAndRumor.Lite`
- own settlement-level public pulse only:
  - street-talk heat
  - market/town bustle
  - notice visibility
  - road-report lag
  - prefecture dispatch pressure
  - public legitimacy
  - dominant public venue label / trace
- read-only upstream inputs:
  - `WorldSettlements`
  - `PopulationAndHouseholds`
  - `TradeAndIndustry`
  - `OrderAndBanditry`
  - `OfficeAndCareer`
  - `SocialMemoryAndRelations`
- no cross-module state writes
- optional same-month warfare fallout through event handling is allowed only if the module updates its own public-pulse state

### 3. Read models and shell
- export public-life read models through `PresentationReadModelBuilder`
- add a public-life summary to the great hall and settlement nodes in the desk sandbox
- keep the UI read-only

### 4. Narrative hooks
- add public-life notification titles and next-step wording in `NarrativeProjection`

## Non-goals

- no detached prefecture map
- no new authority UI
- no full granary / temple / guild pack yet
- no direct county-office commands in this slice

## Save / determinism notes

- `WorldSettlements` gains a built-in `1 -> 2` migration that backfills settlement tiers conservatively
- `PublicLifeAndRumor` is a new module namespace at schema `1`
- old saves remain loadable with the new module disabled
- new M2+ bootstraps may enable the new module
- simulation remains deterministic because public-pulse evolution derives from stable queries plus kernel RNG only

## Tests

- module tests for bounded public-pulse evolution and event handling
- integration test for bootstrap/read-model/shell public-life summaries
- save roundtrip coverage for new namespace plus world tier migration

## Docs to update

- `docs/MODULE_BOUNDARIES.md`
- `docs/MODULE_INTEGRATION_RULES.md`
- `docs/SCHEMA_NAMESPACE_RULES.md`
- `docs/DATA_SCHEMA.md`
- `docs/UI_AND_PRESENTATION.md`
- `docs/ACCEPTANCE_TESTS.md`
