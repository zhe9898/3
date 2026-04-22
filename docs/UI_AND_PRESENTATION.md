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
5. macro sandbox / regional pressure board
6. desk sandbox settlement view
7. ledgers, memorials, and route reports
8. notice tray / notification center
9. conflict result vignette
10. campaign board surface later

## UI architecture rules
- use view models or read models
- presentation reads projections/query services
- presentation sends commands
- presentation does not hold authoritative logic

## Wording lanes
- detailed lane rules, ownership, and authoring workflow live in `WRITING_AND_COPY_GUIDELINES.md`
- player-facing in-world surfaces should use theme-appropriate wording for the setting rather than modern product-dashboard language
- for the current baseline, county, office, and campaign wording should default to a Northern Song-inspired tone: hall, yamen, memorial, route, grain line, gate, ferry, and posted notice rather than generic fantasy bureaucracy
- bootstrap and fixture-fed place / household labels should enter the player-facing shell already grounded as county, market, wharf, ferry, academy, and household names from the same Northern Song-inspired register; do not rely on late-stage replacement of English placeholder seeds
- system actions and general usability labels may stay modern and plain when clarity matters, for example save/load, confirm/cancel, sorting, filtering, and settings
- development and diagnostics surfaces should stay modern, explicit, and engineering-friendly; do not rewrite debug, migration, schema, payload, or inspector language into faux-historical prose
- do not mix the two lanes on one surface: a hall / desk / warfare board should not read like a dashboard, and a debug inspector should not read like a memorial

## Current first-pass implementation note
- the current repository implements a first-pass shell as view-model composition rather than final Unity scenes
- `Zongzu.Presentation.Unity` consumes a read-model bundle exported by application code
- family, exam, trade, settlement, governance-office, warfare-campaign, and notice panels are composed from projections only
- the shell should treat the map stack as sandboxes at multiple scales rather than a modern flat map: a macro sandbox for route / prefecture / regional pressure and a desk sandbox for county-scale lived pressure
- great hall and desk sandbox may now also surface county-public-life summaries such as street talk, county-gate crowding, market bustle, road-report lag, and prefecture pressure from read models only
- the current shell now also carries a first thin player-command layer through read-only affordances and receipts for office and warfare slices; it may show what can be ordered and what was last acknowledged, but command resolution still lives outside UI
- the current shell now also carries a read-only family-council surface, exposing lineage-conflict summaries, clan-memory wording, family command affordances, and recent receipts without adding authority UI
- stable M2 and later paths may now surface bounded family commands such as elder mediation, branch favor, apology, relief suspension, and branch separation when `FamilyCore` projects them
- the same family-council surface may now also expose thin marriage / heir / mourning lifecycle wording such as `议亲定婚`, `拨粮护婴`, `议定承祧`, `议定丧次`, `承祧未稳`, and `门内举哀`, but those remain `FamilyCore` read models rather than UI-owned rules
- the great hall family line may now also summarize whether a clan is pressed by婚议, 承祧, or 举哀, and that wording must still come from family read models rather than UI-side rule inference
- family lifecycle receipts may now read as household dispositions such as聘财轻重, 入谱定名, and 丧服护持 instead of generic success text, but those phrases still come from `FamilyCore` state/read models rather than UI-owned logic
- great hall and family-council lifecycle summaries may now also carry a read-only directional prompt such as `眼下宜先议定承祧` or `眼下宜先议定丧次`; the shell may choose which bounded family command to highlight from projected affordances, but it still may not resolve or mutate family state inside UI code
- when the lead notice itself is a family-lifecycle notice, the great hall lead-notice guidance and notification-center `WhatNext` text should align to the same read-only directional prompt already chosen from projected lifecycle affordances
- family death notices may now distinguish adult-successor deaths from severe承祧 gaps: the former should lead with丧次 / 祭次 and stabilizing the new承祧, while the latter should lead with议定承祧 and压住房支后议; this is notice / ancestral-hall guidance only, not a full funeral workflow
- the read-only office surface now exposes current appointment, current administrative task, petition backlog, petition outcome category, and promotion/demotion pressure wording without introducing any authority controls
- the read-only office surface may now also expose bounded command affordances such as petition review and administrative leverage only when governance-lite is enabled
- the read-only public-life surface now exposes county-public-life summaries on the hall and settlement nodes only; it does not resolve notices, rumors, or county pressure inside UI code
- the read-only public-life surface may now also expose monthly cadence and crowd-mix wording such as fair days, docket-choked county gates, or road-report bustle on the hall and settlement nodes, but that wording still comes from `PublicLifeAndRumor` read models only
- the read-only public-life surface may now also expose venue-channel summaries such as榜示分量、市语流势、查验周折、递报险数 on settlement nodes, but those numbers and summaries still come from `PublicLifeAndRumor` read models only
- the read-only public-life surface may now also expose who says what:榜文如何写、街谈如何传、路报如何失真、州牒如何压下来, and that contention wording must still come from `PublicLifeAndRumor` read models only
- hall / desk settlement nodes may now show bounded public-life affordances and receipts such as `张榜晓谕`, `遣吏催报`, `催护一路`, and `请族老出面`, but those remain read-only UI projections; authority resolution still lives in the owning application/domain service
- the read-only warfare surface now exposes campaign boards, mobilization windows, front labels, command-fit labels, commander summaries, active directive label/summary/trace, bounded route summaries, supply-line summaries, office coordination traces, and aftermath summaries without introducing any authority controls
- the read-only warfare surface may now also expose bounded command affordances such as plan drafting, mobilization, supply-line protection, and barracks withdrawal only when the campaign-enabled path is active
- the read-only warfare surface now also derives board-environment labels, board-surface labels, marker summaries, and atmosphere summaries from front pressure, supply state, morale state, active directive, and route posture so the campaign sandbox no longer reads like one static board
- the read-only warfare surface now also derives regional-profile labels and regional backdrop summaries from existing settlement security/prosperity plus local route naming signals, so a water-linked market county, a hill-route pass, and a flat inland county do not read like the same board
- the read-only warfare surface now also surfaces aftermath-docket summaries for the hall, settlement nodes, and campaign board, so `记功簿`, `劾责状`, `抚恤簿`, and `清路札` can appear as projections only
- player-facing hall / desk / office / warfare wording should avoid modern dashboard or workflow jargon; use hall, yamen, memorial, route, grain-line, petition, and campaign-board language instead
- player-facing public-life wording should read like lived county society: market lanes, county gates, posted notices, ferries, inns, and road reports rather than analytics or dashboard prose
- warfare-lite wording should read like a Chinese-ancient desk-sandbox board: `军务沙盘`, `前线`, `粮道`, `军心`, `号令`, with directives such as `发檄点兵` and `催督粮道`
- player-facing authoritative summaries should be authored in final in-world language at source where practical; do not rely on shell-only replacement passes for module diffs, domain-event summaries, or surfaced state explanations that will later feed notices
- if legacy or fixture-fed English phrases still enter the warfare shell, normalize them at the read-only presentation boundary before they reach hall, desk, or campaign-board surfaces; do not preserve `Registrar`, `campaign board`, `docket traffic`, or escort-logistics prose on player-facing panels
- development-facing debug panels are also composed from read-only projections, now grouped for faster scanning into `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings`, and they should keep modern engineering wording
- authoritative simulation state stays inside application/module layers
- command affordances and receipts in the shell are rebuilt from current authoritative state only; they do not authorize, queue, or resolve commands inside UI code
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
- a minimal Unity host shell now also lives at `/unity/Zongzu.UnityShell`; treat it as the scene/asset workspace for hand-built UI, while authoritative simulation and read-model composition remain in `src/`
