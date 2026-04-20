## Goal

Extend `PublicLifeAndRumor.Lite` from a monthly-cadence slice into a county-public-life v3 slice with explicit public venues, competing information channels, and bounded player responses routed through existing owning modules.

## Scope in

- bump `PublicLifeAndRumor` schema from `2` to `3`
- add public-life-owned venue/channel descriptors such as:
  - dominant venue code
  - documentary weight
  - verification cost
  - market rumor flow
  - courier risk
  - channel summary
- keep those descriptors authoritative inside `PublicLifeAndRumor` only
- add a thin `PublicLife` player-command surface that exposes bounded responses through existing owners:
  - `张榜晓谕` via `OfficeAndCareer`
  - `遣吏催报` via `OfficeAndCareer`
  - `催护一路` via `OrderAndBanditry`
  - `请族老出面` via `FamilyCore`
- project public-life affordances and receipts into the desk sandbox without moving rules into UI
- add save migration for `PublicLifeAndRumor` `2 -> 3`

## Scope out

- no full prefecture / circuit map
- no new standalone prefecture-state or imperial-state module
- no detached “society manager” that writes foreign state
- no new warfare / temple / guild / granary feature pack in this slice
- no authority rules inside `Presentation.Unity`

## Affected modules

- `Zongzu.Modules.PublicLifeAndRumor`
- `Zongzu.Application`
- `Zongzu.Contracts`
- `Zongzu.Presentation.Unity`
- bounded command routing into:
  - `Zongzu.Modules.OfficeAndCareer`
  - `Zongzu.Modules.OrderAndBanditry`
  - `Zongzu.Modules.FamilyCore`

## Save/schema impact

- `PublicLifeAndRumor` schema `2 -> 3`
- new public-life-owned venue/channel descriptors become part of save roundtrip
- no new save namespaces
- no root schema change
- no office / order / family schema bump unless later implementation proves it necessary

## Determinism risk

- low to medium
- venue/channel descriptors must derive only from deterministic month inputs plus published query state
- bounded response commands must mutate only owning-module state and stay deterministic for the same seed + same command sequence

## Milestones

1. Add v3 `PublicLifeAndRumor` state/query fields plus `2 -> 3` migration.
2. Extend public-life authority refresh to compute venue/channel competition.
3. Add thin public-life command names and route them through `FamilyCore`, `OfficeAndCareer`, and `OrderAndBanditry`.
4. Project public-life affordances/receipts into read models and desk shell.
5. Add/update tests, then run focused and solution-level verification.

## Tests to add/update

- `PublicLifeAndRumorModuleTests`
  - v3 venue/channel descriptors populate deterministically
  - county seat and market-town public surfaces diverge in expected ways
- `SaveMigrationPipelineTests`
  - `PublicLifeAndRumor` `2 -> 3` migration backfills v3 fields
- `M2LiteIntegrationTests`
  - public-life affordances/receipts appear only when owning modules are enabled
  - public-life commands route through owners and leave deterministic receipts
- `FirstPassPresentationShellTests`
  - desk settlement public-life summaries include channel/venue wording
  - desk settlement public-life command affordances / receipts project correctly

## Rollback / fallback plan

- if public-life command routing causes too much owner churn, keep the v3 venue/channel state but reduce the slice to read-only presentation
- if schema migration proves noisy, backfill conservative defaults and defer richer descriptor tuning to a follow-up

## Open questions

- whether `催护一路` should later migrate from an order-owned quick response into a richer force / escort coordination slice
- whether `张榜晓谕` and `遣吏催报` should eventually preserve explicit last-command code in office-owned state instead of being inferred from current task traces
