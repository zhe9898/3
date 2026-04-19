# MODULE_INTEGRATION_RULES

This document defines exactly how modules may interact.

## Allowed integration channels

### 1. Query
Read-only access to projections or query services.
Use for:
- security pressure lookups
- school availability
- route conditions
- force pool summaries
- grudge pressure summaries

### 2. Command
Intent routed to the owning module.
Use for:
- arrange marriage
- fund study
- expand trade route
- suppress banditry
- mobilize militia
- start campaign

Commands do not guarantee success.
They trigger module-owned resolution.

### 3. DomainEvent
Structured “something happened” outputs.
Use for:
- exam passed
- caravan raided
- branch split
- bandit group formed
- campaign lost

Subscribers may update **their own** state only.

## Forbidden integration
- direct object references to foreign mutable state
- module A changing module B’s private collections
- UI writing into module state
- text templates causing authority changes
- ad hoc global singleton service with cross-module write access

## Event handling rules
- event queues are deterministic
- event ordering is documented by scheduler phase
- handlers must be side-effect limited to owned state
- if a handler needs additional data, it must query published projections
- the current scheduler now performs a thin deterministic post-simulation event-handling pass on a stable event snapshot before `NarrativeProjection` runs
- handler-emitted follow-on events may enrich projection and diagnostics in the same month, but they do not recursively trigger another handler sweep that month

## Projection rules
- projections are read models
- projections may be cached
- projections are rebuilt from authoritative state
- projections are not a backdoor write channel

## Integration review checklist
Before approving a cross-module change:
- Who owns the state being changed?
- Could this be an event instead of a direct write?
- Could this be a query instead of a reference?
- Is the foreign state only being read through projections?
- Is save/schema impact documented?
- Does this keep the feature pack additive?

## Canonical bad example
“ExamPassed directly increments Clan.Prestige and creates OfficeRank.”

Why bad:
- exam module is mutating family and office internals

Correct approach:
- `EducationAndExams` emits `ExamPassed`
- `FamilyCore` handles the event to update its own prestige state
- `OfficeAndCareer` handles the event to open or advance office eligibility

## Current M2-lite integration notes
- `EducationAndExams.Lite` currently reads only `WorldSettlements`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `EducationAndExams.Lite` owns study progress, tutor quality, exam attempts, outcomes, and explanation text; it does not write family prestige or office state directly
- `TradeAndIndustry.Lite` currently reads only `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `TradeAndIndustry.Lite` owns clan trade cash/debt state, market pressure, route pressure, outcomes, and explanation text; it does not write household or clan internals directly
- `PublicLifeAndRumor.Lite` now reads `WorldSettlements`, `PopulationAndHouseholds`, `TradeAndIndustry`, `OrderAndBanditry`, optional `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces only
- `PublicLifeAndRumor.Lite` owns settlement public pulse only: street-talk heat, market bustle, notice visibility, road-report lag, prefecture-dispatch pressure, public legitimacy, dominant-venue wording, monthly cadence / crowd-mix wording, venue-channel competition metrics, and channel-line wording for notice / street talk / road report / prefecture pressure / contention
- `WorldSettlements` now owns settlement tier / node rank at schema `2`; presentation and public-life projections must not invent county / market / village rank in UI-only code
- M2 and later manifests may enable `PublicLifeAndRumor` as an additive county-public-life layer without changing ownership of household, office, trade, force, or clan state
- great-hall and desk-sandbox public-life summaries must be rebuilt from `IPublicLifeAndRumorQueries` through the presentation bundle only; UI remains read-only
- monthly cadence labels such as fair days, county-gate docket pressure, or road-report bustle must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel descriptors such as documentary weight, market-rumor flow, verification cost, and courier risk must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel wording such as what the posted notice claims, what street talk says, how road reports differ, and how prefecture dispatch presses downward must also remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- bounded public-life responses may surface as read-only command affordances / receipts on hall or desk nodes, but command resolution must still route through `OfficeAndCareer`, `OrderAndBanditry`, or `FamilyCore` rather than `PublicLifeAndRumor`
- both M2-lite modules emit deterministic domain events and keep outcome explanations derived from queryable state plus kernel RNG only
- `NarrativeProjection` currently reads only the shared `WorldDiff` and `DomainEvent` streams plus its own saved history; it does not emit authority events or write foreign module state
- the current first-pass presentation shell consumes a read-model bundle only; it does not reference simulation modules directly and does not resolve commands or authority rules inside UI code

## Family-conflict vertical slice notes
- `FamilyCore` now owns lineage-conflict pressure, mediation momentum, branch-favor pressure, relief-sanction pressure, and last family-command receipts inside the family namespace
- `FamilyCore` schema `3` also owns marriage-alliance pressure/value, heir security, reproductive pressure, mourning load, and last lifecycle-command receipts inside the same namespace
- `SocialMemoryAndRelations` may read those family-conflict fields through queries only; it may not be written by the player-command service
- a thin player-command service may now route bounded family intents such as branch favor, formal apology, branch separation, relief suspension, elder mediation, marriage arrangement, and heir designation into `FamilyCore` only
- the family-council shell now reads clan conflict summaries, clan narratives, family affordances, and family receipts from read models only
- built-in default loaders now migrate `FamilyCore` schema `1 -> 2 -> 3` without changing enabled-module or envelope-key sets

## Current observability and migration notes
- diagnostics harness reports and presentation debug snapshots now align on the same runtime-only metrics: diff entries, domain events, notifications, and save payload bytes
- diagnostics harness now also supports multi-seed long-run sweeps plus explicit budget evaluation, still as runtime-only reporting
- diagnostics harness now also records per-module diff/event activity peaks for local-conflict slices as runtime-only reporting
- diagnostics harness now also records runtime-only local-conflict interaction pressure such as activated responses, supported order settlements, and high suppression-demand settlements
- diagnostics harness may now also surface top hotspot settlements by joining `OrderAndBanditry` and `ConflictAndForce` state after simulation; those hotspot summaries remain runtime-only
- those observability summaries are derived after authority simulation and never become a backdoor write channel
- save loading now passes through an explicit migration seam with registrable root/module migration hooks, same-version pass-through, and explicit failure when no path is registered

## M3 local-conflict transition notes
- `OrderAndBanditry.Lite` now owns settlement disorder pressure, route insecurity pressure, suppression demand, and explanation text inside its own namespace
- `OrderAndBanditry.Lite` reads `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, optional `TradeAndIndustry`, optional `OfficeAndCareer`, and optional `ConflictAndForce` projections only
- `OrderAndBanditry.Lite` converts activated local guard, escort, readiness, and militia posture into slower disorder escalation and lower suppression demand through queries only
- `OrderAndBanditry.Lite` may now also read office-owned jurisdiction leverage as bounded administrative relief; it still may not write office state
- `TradeAndIndustry.Lite` now reads `OrderAndBanditry` projections when enabled and converts that pressure into route risk through queries only
- `ConflictAndForce.Lite` now owns settlement guard, retainer, militia, escort, readiness, command-capacity, and local conflict trace state inside its own namespace
- `ConflictAndForce.Lite` now also owns persistent campaign-fatigue, escort-strain, and campaign-fallout trace state inside the same namespace
- `ConflictAndForce.Lite` reads `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, `OrderAndBanditry`, optional `OfficeAndCareer`, and optional `TradeAndIndustry` projections only
- `ConflictAndForce.Lite` now runs before `OrderAndBanditry.Lite` inside the active local-conflict slice so same-month response activation can feed order support without direct writes
- `ConflictAndForce.Lite` now persists explicit response activation/support state instead of relying on trace-text inference alone
- `ConflictAndForce.Lite` may now also consume settlement-targeted warfare aftermath events, but only to update its own fatigue / escort-strain / readiness fallout state
- warfare aftermath fallout now persists in `ConflictAndForce.Lite` for later monthly recovery; it does not write back into `WarfareCampaign`, `OrderAndBanditry`, or office state
- calm or standing-but-untriggered `ConflictAndForce.Lite` posture must not leak support, escort relief, or militia relief into `OrderAndBanditry` until the response is actually activated by local-conflict pressure
- `ConflictAndForce.Lite` emits deterministic readiness and local-conflict events while still updating only its own state
- `OrderAndBanditry.Lite` remains available through an order-enabled M3 bridge bootstrap path
- `ConflictAndForce.Lite` is available through a conflict-enabled M3 local-conflict bootstrap path and remains absent from active M2 manifests

## Governance-lite notes
- `OfficeAndCareer.Lite` now owns office appointments, authority tier, candidate waiting pressure, clerk dependence, service progression, administrative tasks, petition backlog/outcomes, jurisdiction leverage, petition pressure, and explanation text inside its own namespace
- `OfficeAndCareer.Lite` currently reads only `EducationAndExams` and `SocialMemoryAndRelations` projections
- the new governance-lite bootstrap path enables `OfficeAndCareer` without mutating the stable M2 or M3 manifests
- a thin player-command service may now route bounded office intents such as petition review or administrative leverage into `OfficeAndCareer` only; it may not write family, trade, order, or force state directly
- governance-lite now treats local-exam success as entry into a bounded office funnel rather than direct appointment: recommendation, waiting for openings, and attached yamen service stay office-owned and deterministic
- the lighter office v2.1 slice now exposes petition-outcome category, administrative-task tier, promotion/demotion labels, and authority-trajectory wording as derived read-model/query fields only; it does not add new office-owned save fields
- office influence stays bounded: downstream modules may read leverage or petition pressure, but only `OfficeAndCareer` mutates office appointments and jurisdiction authority
- governance-lite loads now include built-in `OfficeAndCareer` `1 -> 2 -> 3` migration for legacy office saves
- legacy office migration now reconstructs missing service/task/petition details first, then backfills queue pressure and clerk dependence conservatively for schema `3`

## Campaign-lite integration notes
- `WarfareCampaign.Lite` is now active only through the dedicated campaign-enabled bootstrap/load path; stable M2/M3/governance-lite paths remain warfare-free
- `WarfareCampaign.Lite` owns campaign boards, campaign aftermath summaries, and mobilization-signal snapshots inside its own namespace
- `WarfareCampaign.Lite` reads `ConflictAndForce`, `WorldSettlements`, and optional `OfficeAndCareer` projections only
- `WarfareCampaign.Lite` does not write force posture, office leverage, or settlement baselines back into upstream modules
- campaign read models now flow through `IWarfareCampaignQueries`, using local-force posture plus office coordination as upstream signals rather than inventing a second force-ownership model
- the active warfare-lite slice currently emits bounded campaign events only: mobilization, front-pressure escalation, supply strain, and aftermath registration
- warfare-lite events now also carry settlement-targeting metadata so downstream handlers can update their own state without parsing narrative strings
- current warfare-lite contracts already carry order-support, office-authority-tier, administrative-leverage, petition-backlog, mobilization-window, command-fit, route-flow, and office-coordination precursor fields so later campaign depth can stay query-first
- the current board-depth refinement persists commander summaries plus bounded route descriptors inside `WarfareCampaign` rather than synthesizing them in UI-only code
- a thin application-routed warfare-intent service now stages `DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, and `WithdrawToBarracks` into `WarfareCampaign`-owned directive state only
- the current player-command vertical slice may expose those same warfare directives as read-only affordances and receipts in presentation, but the routing still stays in application services and the writes still stay inside `WarfareCampaign`
- current warfare-lite state now persists active directive code/label/summary and last directive trace inside the warfare namespace instead of inventing a cross-module command ledger
- built-in `WarfareCampaign` migration now upgrades schema `1 -> 2 -> 3` by backfilling labels, route descriptors, and directive descriptors without changing enabled-module or envelope-key sets
- current warfare-lite aftermath now propagates into `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `TradeAndIndustry`, `OrderAndBanditry`, `OfficeAndCareer`, and `SocialMemoryAndRelations` through the event-handling seam only
- those downstream modules update only their own prosperity, livelihood, prestige, ledger, pressure, petition, or memory state; none of them write back into `WarfareCampaign`

## Post-MVP preflight seam notes
- black-route depth now has explicit preflight query seams: pressure snapshots stay aligned with `OrderAndBanditry`, while gray-route / illicit ledger snapshots stay aligned with `TradeAndIndustry`
- current black-route preflight contracts now also reserve administrative suppression-window, escalation-band, seizure-risk, and diversion-band fields without activating any gameplay rules
- no standalone `BlackRoute` module key or save namespace should be introduced; future black-route migrations must stay inside the `OrderAndBanditry` and `TradeAndIndustry` module envelopes
