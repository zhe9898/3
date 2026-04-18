# POST_MVP_SCOPE

This document defines additive release lines after MVP.

## Post-MVP rule
Post-MVP features must extend the same:
- monthly tick
- save identity system
- command model
- module integration rules
- spatial shell
- explanation pipeline

## Post-MVP feature packs

### Governance pack
- `OfficeAndCareer`
- richer institution leverage
- office-mediated authority
- title-based resource access
- office-family tension

Current lite note:
- `OfficeAndCareer.Lite` is now active through a dedicated governance-lite bootstrap path as the first formal authority layer after local exams
- `OfficeAndCareer.Lite` now includes bounded promotion/demotion pressure, administrative task assignment, and petition handling inside the office-owned namespace
- current lite office leverage may feed disorder or local-force modules only through read-only queries; it does not grant direct writes into those modules
- the lighter office v2.1 slice now surfaces administrative-task tier, petition-outcome category, and authority-trajectory wording as read-only descriptors only; it does not add new authority UI or a schema `3` transition

### Order and disorder pack
- `OrderAndBanditry` full
- outlaw formation and suppression
- black-route pressure
- local armed disorder ecology
- optional limited recruitment and bargaining via gray channels

Integration seam:
- black-route / black-market depth must extend `OrderAndBanditry` and `TradeAndIndustry` through queries, events, and owned ledgers
- no future black-market system may bypass trade balances or write directly into trade-owned state
- black-route pressure should keep treating `ConflictAndForce` response activation as an input projection, not as a foreign state it can rewrite

Current preflight note:
- the repository now reserves black-route contracts only as query seams: pressure stays conceptually owned by `OrderAndBanditry`, while illicit/gray-route ledgers stay conceptually owned by `TradeAndIndustry`
- current preflight pressure/ledger contracts also reserve administrative suppression-window, escalation-band, seizure-risk, and diversion-band summaries for later rollout
- no standalone `BlackRoute` module key or save namespace is active in current manifests

Pre-implementation checklist:
- define which disorder fields stay in `OrderAndBanditry` and which ledgers stay in `TradeAndIndustry`
- define how black-route pressure becomes trade risk without adding direct writes
- define how local force response can suppress, redirect, or fail against black-route pressure through queries/events only
- define which runtime-only hotspot and migration diagnostics should stay visible to developers during black-route rollout without becoming player-facing authority
- define which runtime-only scale or payload-footprint summaries best reveal black-route growth pressure during stress runs

### Force pack
- `ConflictAndForce` full
- retainers, clan militia, escorts, force pools
- authority, mobilization, command capacity, supply foundations
- richer conflict aftermath

### Warfare pack
- `WarfareCampaign`
- campaign sandbox board
- routes, fronts, supply lines, battle plans
- battle intention, commander personality distortion, morale and pursuit
- war remains downstream of the living world

Integration seam:
- warfare must build on `ConflictAndForce` authority, mobilization, command-capacity, and supply foundations
- campaign systems may not invent a second independent force-ownership model
- future campaign mobilization should consume explicit force-response posture as a precursor signal, then promote owned campaign state inside `WarfareCampaign`

Current lite note:
- the repository now has an active `WarfareCampaign.Lite` slice behind a dedicated campaign-enabled bootstrap/load path
- the active lite implementation owns bounded campaign boards, mobilization signals, supply/morale/front summaries, command-fit wording, commander summaries, bounded route descriptors, aftermath summaries, and application-routed directive descriptors inside `WarfareCampaign`
- current mobilization signals carry order-support, office-authority-tier, administrative-leverage, petition-backlog, mobilization-window, and office-coordination summaries as upstream precursors only
- the current lite aftermath slice now also pushes bounded downstream consequences into trade, order, office, and clan-memory modules through event handlers only
- the current lite projection slice now also lets warfare notices and read-only hall / desk / campaign-board surfaces summarize merit claims, blame memorials, relief dockets, and local cleanup pressure without adding authority UI or a new schema
- legacy campaign-enabled saves now upgrade through a built-in `WarfareCampaign` schema `1 -> 2 -> 3` migration without introducing a new module key
- stable M2/M3/governance-lite paths still keep `WarfareCampaign` disabled, so the warfare-lite slice remains additive rather than contaminating earlier lines

Pre-implementation checklist:
- define command/event handoff from settlement force posture into campaign mobilization
- define what remains settlement-owned in `ConflictAndForce` versus campaign-owned in `WarfareCampaign`
- define campaign aftermath events before implementing any battle-resolution depth
- define which campaign stress diagnostics remain runtime-only and how they surface in the debug shell without entering save compatibility
- define which runtime-only scale/payload diagnostics indicate campaign-board complexity growth before any player-facing UX ships

### Regional expansion pack
- multiple regions
- migration, long-distance kin ties
- multi-settlement projections
- broader trade and order pressure

### Presentation polish pack
- richer room states
- weddings, funerals, banquets
- more visitors, letters, and ambient life
- more powerful analytics overlays

## Pathway expansion
Post-MVP may deepen:
- exams into school networks, patronage, rank ladders
- trade into guilds, caravan chains, goods categories, finance complexity
- office into career tracks, appointments, formal authority
- outlaw/banditry into camps, black routes, coercive pressure, limited negotiation and suppression arcs
- war into campaign-scale desk-sandbox play

## Post-MVP anti-goals
Do not:
- replace abstract conflict with unit micro
- replace desk-sandbox war with RTS battle maps
- fork the save model
- introduce a separate “outlaw game mode”
- turn pathways into isolated trees detached from family/commoner/world systems
