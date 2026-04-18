# UI_AND_PRESENTATION

This document defines information hierarchy, screens, and interaction responsibilities.

## Information hierarchy
### Tier 1: urgent
Needs immediate player attention:
- death, inheritance, core marriage, legal emergency, critical conflict, campaign collapse

### Tier 2: consequential
Important soon:
- branch tension
- exam result
- trade route loss
- rising outlaw pressure
- local security decline

### Tier 3: background
Useful but non-blocking:
- rumors
- local market drift
- distant kin information
- ambient reports

## Core screens / surfaces
1. great hall / main hall report surface
2. lineage surface
3. person inspector
4. household/clan inspector
5. desk sandbox settlement view
6. ledgers, memorials, and route reports
7. notice tray / notification center
8. conflict result vignette
9. campaign board surface later

## UI architecture rules
- use view models or read models
- presentation reads projections/query services
- presentation sends commands
- presentation does not hold authoritative logic

## Wording lanes
- player-facing in-world surfaces should use theme-appropriate wording for the setting rather than modern product-dashboard language
- system actions and general usability labels may stay modern and plain when clarity matters, for example save/load, confirm/cancel, sorting, filtering, and settings
- development and diagnostics surfaces should stay modern, explicit, and engineering-friendly; do not rewrite debug, migration, schema, payload, or inspector language into faux-historical prose
- do not mix the two lanes on one surface: a hall / desk / warfare board should not read like a dashboard, and a debug inspector should not read like a memorial

## Current first-pass implementation note
- the current repository implements a first-pass shell as view-model composition rather than final Unity scenes
- `Zongzu.Presentation.Unity` consumes a read-model bundle exported by application code
- family, exam, trade, settlement, governance-office, warfare-campaign, and notice panels are composed from projections only
- the read-only office surface now exposes current appointment, current administrative task, petition backlog, petition outcome category, and promotion/demotion pressure wording without introducing any authority controls
- the read-only warfare surface now exposes campaign boards, mobilization windows, front labels, command-fit labels, commander summaries, active directive label/summary/trace, bounded route summaries, supply-line summaries, office coordination traces, and aftermath summaries without introducing any authority controls
- the read-only warfare surface now also derives board-environment labels, board-surface labels, marker summaries, and atmosphere summaries from front pressure, supply state, morale state, active directive, and route posture so the campaign sandbox no longer reads like one static board
- the read-only warfare surface now also derives regional-profile labels and regional backdrop summaries from existing settlement security/prosperity plus local route naming signals, so a water-linked market county, a hill-route pass, and a flat inland county do not read like the same board
- the read-only warfare surface now also surfaces aftermath-docket summaries for the hall, settlement nodes, and campaign board, so `记功簿`, `劾责状`, `抚恤簿`, and `清路札` can appear as projections only
- player-facing hall / desk / office / warfare wording should avoid modern dashboard or workflow jargon; use hall, yamen, memorial, route, grain-line, petition, and campaign-board language instead
- warfare-lite wording should read like a Chinese-ancient desk-sandbox board: `军务沙盘`, `前线`, `粮道`, `军心`, `号令`, with directives such as `发檄点兵` and `催督粮道`
- if legacy or fixture-fed English phrases still enter the warfare shell, normalize them at the read-only presentation boundary before they reach hall, desk, or campaign-board surfaces; do not preserve `Registrar`, `campaign board`, `docket traffic`, or escort-logistics prose on player-facing panels
- development-facing debug panels are also composed from read-only projections, now grouped for faster scanning into `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings`, and they should keep modern engineering wording
- authoritative simulation state stays inside application/module layers
- governance-lite office read models remain optional: when `OfficeAndCareer` is disabled, the shell must surface a read-only "no office projection" equivalent rather than synthesizing office data
- warfare-lite read models remain optional: when `WarfareCampaign` is disabled, the shell must surface `暂无军务沙盘投影。` rather than synthesizing campaign data

## Explainability rule
Every major visible outcome needs:
- what happened
- affected actors
- why it happened
- what the player can do next

## Debug UI minimum
Development builds must expose:
- current seed
- current date
- replay hash
- enabled feature packs
- diff traces
- module state inspectors
- current interaction-pressure summary
- pressure-distribution summary
- runtime scale summary
- payload-summary headline
- named hotspot summary for active local-conflict runs
- top module payload footprint summary
- bootstrap vs save-load origin and any runtime migration steps
- migration consistency summary/warnings
- warning/invariant list
- campaign traces when warfare pack is enabled

Current repository note:
- the first-pass shell now carries a read-only debug panel whose `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings` sections reorganize the same runtime-only diagnostics into scan-friendly developer buckets
- latest-month debug traces, pressure/scale summaries, hotspot summaries, payload headlines/footprints, and migration summaries are runtime diagnostics only and are not part of save compatibility
