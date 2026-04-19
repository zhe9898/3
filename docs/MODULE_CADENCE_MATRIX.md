# MODULE_CADENCE_MATRIX

This document defines the default time-cadence contract for authoritative modules.
It complements:
- `SIMULATION.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `MODULE_BOUNDARIES.md`
- `ENGINEERING_RULES.md`
- `EXTENSIBILITY_MODEL.md`

Use this file when deciding whether a rule should resolve at `xun`, `month`, or `seasonal` cadence.

## Cadence bands

### `xun`
Use for near-life pressure, short-lived local drift, and high-frequency state that players may later read as lived momentum.

### `month`
Use for consolidation, structured diffs, major household review, command intake, and formal projection.

### `seasonal`
Use for slower bands such as harvest outcome, broad exam heat, major war posture, and climate-weighted institutional or regional shifts.

## Default module cadence contract

### Kernel and scheduler
- `xun`
- `month`

The kernel owns the pulse calendar, deterministic ordering, command staging, and month-end finalization.

### PersonRegistry (Kernel layer)
- `month`

Runs age progression and life-stage checks at the start of each month (Phase 0), before xun pulses begin. Listens for death events at any time to update alive/dead status. Does not run at `xun` cadence. See `PERSON_OWNERSHIP_RULES.md`.

### WorldSettlements
- `xun`
- `month`
- `seasonal`

Runs route friction, local heat, ferries, notices-in-motion, and pressure accumulation at `xun`; settles broader settlement state and trend summary at `month`; uses `seasonal` for slower structural bands.

### PopulationAndHouseholds
- `xun`
- `month`

Runs livelihood strain, food burn, labor strain, support stress, petty debt pressure, short illness drift, and household motion at `xun`; settles month-end deltas, births, deaths, and consolidated household review at `month`.

### FamilyCore
- `xun`
- `month`

Runs immediate household and branch tension, support choices already in flight, and near-family drift at `xun`; resolves formal family review, inheritance-sensitive consolidation, and house-style policy effects at `month`.

### SocialMemoryAndRelations
- `xun`
- `month`

Runs rumor heat, insult carry, obligation friction, and low-latency social drift at `xun`; consolidates durable memory, status change, and relationship narrative summaries at `month`.

### EducationAndExams
- `month`
- `seasonal`

Uses `month` for study progress, access, funding continuity, and institutional pull; uses `seasonal` for exam climate, formal rounds, and slower academic opportunity changes.

### TradeAndIndustry
- `xun`
- `month`

Runs petty trade movement, local market heat, route blockage impact, and shop-floor strain at `xun`; settles account review, sustained profitability, and structured business diffs at `month`.

### PublicLifeAndRumor
- `xun`
- `month`

Runs venue heat, street talk, market bustle, notice drift, road-report motion, and county-gate pressure at `xun`; settles cadence labels, crowd mix, venue/channel summaries, and month-end public contention wording at `month`.

### OfficeAndCareer
- `xun`
- `month`

Runs paper delay, office pressure, petition queue drift, and local yamen temperature at `xun`; settles appointments, reputation movement, and formal office/career state changes at `month`.

### ConflictAndForce
- `xun`
- `month`

Runs hotspot escalation, escort strain, guard posture, and local coercive motion at `xun`; settles larger force-state changes and consolidated conflict aftermath at `month`.

### OrderAndBanditry
- `xun`
- `month`

Runs disorder flare, intimidation, patrol weakness, and opportunistic gray movement at `xun`; settles wider order temperature, organized threat drift, and month-end safety deltas at `month`.

### WarfareCampaign
- `month`
- `seasonal`

Uses `month` for campaign pressure, route-scale posture, and war-facing opportunity climate; uses `seasonal` for slower mobilization, fatigue bands, and major strategic shifts.

### NarrativeProjection
- `month`

Builds the main readable review shell at month-end. Rare urgent interrupts may surface between reviews, but projection does not become a second authoritative simulation layer.

### Presentation.Unity
- continuous render
- month-facing review shell
- xun-facing trend visibility only through projections

Presentation does not own authoritative simulation state. It may preview xun-driven movement only through read models, trends, and staged visible change.

## Anti-patterns

Do not:
- make every module run at `xun` just because high frequency feels more alive
- hide authoritative `xun` state in presentation-only caches
- let a module invent a private cadence that the scheduler cannot inspect
- let projection become an alternate simulation layer

## One-line summary

`xun` is for lived pressure, `month` is for review and consolidation, and `seasonal` is for slower structural climate.
