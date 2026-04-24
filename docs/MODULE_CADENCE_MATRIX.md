# MODULE_CADENCE_MATRIX

This document defines the default time-cadence contract for authoritative modules.
It complements:
- `SIMULATION.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `MODULE_BOUNDARIES.md`
- `ENGINEERING_RULES.md`
- `EXTENSIBILITY_MODEL.md`

Use this file when deciding whether a rule should resolve at `day`, `month`, `seasonal`, or command-resolution cadence.

## Cadence bands

### `day`
Use for near-life pressure, short-lived local drift, due dates, travel or message delay, illness, debt timing, and high-frequency state that players may later read as lived momentum.
It is an internal authoritative cadence, not a default player-facing turn.
Day outputs should normally become trend, pressure, hotspot, or month-end explanation; only urgent threshold crossings should request an interrupt window.

### `xun / calendar band`
Use `shangxun / zhongxun / xiaxun` only as almanac wording, UI grouping, projection summary, or loose schedule-window label.
It is not the preferred bottom-level authority grid and must not require three routine player turns.

### `month`
Use for consolidation, structured diffs, major household review, command intake, and formal projection.

### `seasonal`
Use for slower bands such as harvest outcome, broad exam heat, major war posture, and climate-weighted institutional or regional shifts.

## Default module cadence contract

### Kernel and scheduler
- `day`
- `month`

The kernel owns the day calendar, deterministic ordering, command staging, batching/skipping of quiet spans, and month-end finalization.

### PersonRegistry (Kernel layer)
- `month`

Runs age progression and life-stage checks at the start of each month (Phase 0), before due day steps begin. Listens for death events at any time to update alive/dead status. Does not run at routine `day` cadence. See `PERSON_OWNERSHIP_RULES.md`.

### WorldSettlements
- `day`
- `month`
- `seasonal`

Runs route friction, local heat, ferries, notices-in-motion, and pressure accumulation at `day`; settles broader settlement state and trend summary at `month`; uses `seasonal` for slower structural bands.

### PopulationAndHouseholds
- `day`
- `month`

Runs livelihood strain, food burn, labor strain, support stress, petty debt pressure, short illness drift, and household motion at `day`; settles month-end deltas, births, deaths, and consolidated household review at `month`.

### FamilyCore
- `day`
- `month`

Runs immediate household and branch tension, support choices already in flight, and near-family drift at `day`; resolves formal family review, inheritance-sensitive consolidation, and house-style policy effects at `month`.

### SocialMemoryAndRelations
- `day`
- `month`

Runs rumor heat, insult carry, obligation friction, and low-latency social drift at `day`; consolidates durable memory, status change, and relationship narrative summaries at `month`.

### EducationAndExams
- `month`
- `seasonal`

Uses `month` for study progress, access, funding continuity, and institutional pull; uses `seasonal` for exam climate, formal rounds, and slower academic opportunity changes.

### TradeAndIndustry
- `day`
- `month`

Runs petty trade movement, local market heat, route blockage impact, and shop-floor strain at `day`; settles account review, sustained profitability, and structured business diffs at `month`.

### PublicLifeAndRumor
- `day`
- `month`

Runs venue heat, street talk, market bustle, notice drift, road-report motion, and county-gate pressure at `day`; settles cadence labels, crowd mix, venue/channel summaries, and month-end public contention wording at `month`.

### OfficeAndCareer
- `day`
- `month`

Runs paper delay, office pressure, petition queue drift, and local yamen temperature at `day`; settles appointments, reputation movement, and formal office/career state changes at `month`.

### ConflictAndForce
- `day`
- `month`

Runs hotspot escalation, escort strain, guard posture, and local coercive motion at `day`; settles larger force-state changes and consolidated conflict aftermath at `month`.

### OrderAndBanditry
- `day`
- `month`

Runs disorder flare, intimidation, patrol weakness, and opportunistic gray movement at `day`; settles wider order temperature, organized threat drift, and month-end safety deltas at `month`.

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
- day-facing trend visibility only through projections, with optional xun-labeled summaries

Presentation does not own authoritative simulation state. It may preview day-driven movement only through read models, trends, staged visible change, and almanac-style labels.

## Anti-patterns

Do not:
- make every module run at `day` just because high frequency feels more alive
- make `day` or `xun` a routine player turn or require the player to advance three sub-turns every month
- hide authoritative day-level state in presentation-only caches
- let a module invent a private cadence that the scheduler cannot inspect
- let projection become an alternate simulation layer

## One-line summary

`day` is for lived pressure, `month` is for review and consolidation, `seasonal` is for slower structural climate, and `xun` is a readable calendar band rather than the bottom authority grid.
