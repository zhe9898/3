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
- `OfficeAndCareer.Lite` now follows a Northern Song-inspired office funnel: local-exam success opens recommendation / waiting pressure first, then attached yamen service or appointment depending on backing and openings
- `OfficeAndCareer.Lite` now includes bounded appointment pressure, clerk dependence, promotion/demotion pressure, administrative task assignment, and petition handling inside the office-owned namespace
- current lite office leverage may feed disorder or local-force modules only through read-only queries; it does not grant direct writes into those modules
- the lighter office v2.1 slice still surfaces administrative-task tier, petition-outcome category, and authority-trajectory wording as read-only descriptors only; the later queue/dependence lift moves save schema to `3` but still does not add authority UI

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

Current implemented slice:
- the repository now ships the first black-route authority slice: pressure stays owned by `OrderAndBanditry`, while illicit/gray-route ledgers stay owned by `TradeAndIndustry`
- the current slice already persists paper-compliance, implementation drag, route-shielding, retaliation-risk, administrative suppression-window, escalation-band, seizure-risk, diversion-band, route-constraint, and trace summaries inside those two module envelopes only
- default loaders now migrate legacy `OrderAndBanditry` schema `1` saves to schema `2 -> 3 -> 4`, and legacy `TradeAndIndustry` schema `1` saves to schema `2 -> 3`, without changing the enabled-module or module-key set
- no standalone `BlackRoute` module key or save namespace is active in current manifests

Next-step checklist:
- deepen how later commands, office pressure, and diagnostics should interpret the same route-shielding / retaliation-risk fields without adding a detached outlaw pack
- decide which developer-only hotspot and payload diagnostics best reveal black-route growth during larger stress runs

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
