## Goal

Extend `PublicLifeAndRumor.Lite` from a static county-public-pulse slice into a monthly-cadence slice so `乡里 / 镇市 / 县门 / 路报 / 州牒` do not read the same every month.

## Why now

- the county-public-life slice already exists, but without cadence it still risks feeling like a fixed dashboard
- the user explicitly wants a living county society where public surfaces change with month rhythm rather than only with pressure totals
- the `zongzu-ancient-china` grounding pass points toward fairs, posted notices, road reports, county gates, and crowd mix as the right public-life layer for a Northern Song-inspired county slice

## Scope

### 1. Public-life authority state
- bump `PublicLifeAndRumor` schema from `1` to `2`
- add owned cadence descriptors per settlement:
  - `MonthlyCadenceCode`
  - `MonthlyCadenceLabel`
  - `CrowdMixLabel`
  - `CadenceSummary`
- keep these fields inside `PublicLifeAndRumor` only; no other module owns public-calendar wording

### 2. Migration
- add a built-in `1 -> 2` migration for `PublicLifeAndRumor`
- backfill cadence fields conservatively for legacy saves
- keep old saves loadable when the module is enabled later

### 3. Presentation
- great hall public-life summary should surface the current monthly cadence and crowd mix of the hottest node
- desk sandbox settlement public-life summary should surface cadence before route/street-talk phrasing
- UI remains read-only

### 4. Tests
- module test for cadence variation across months
- shell test that cadence appears on hall/desk surfaces
- migration/save coverage for `PublicLifeAndRumor` schema `1 -> 2`

## Non-goals

- no new county command verbs in this slice
- no new temple/guild/granary authority pack yet
- no detached prefecture map

## Save / determinism notes

- cadence fields are authoritative `PublicLifeAndRumor` state and therefore part of save roundtrip
- cadence wording still derives deterministically from month, settlement tier, and current query inputs
- presentation only reads cadence through the read-model bundle and does not synthesize public calendar state
