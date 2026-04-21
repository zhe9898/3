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

## Current command-routing gap (documented, to be closed)

`ModuleRunner<TState>` currently does **not** expose a command-handling interface (no `HandleCommand` / `ExecuteCommand` method).  Therefore the thin player-command services in `Zongzu.Application` (`PlayerCommandService`, `WarfareCampaignCommandService`) currently retrieve module state via `GameSimulation.GetMutableModuleState<T>()` and mutate it directly.

This is a **known structural gap**, not an intended permanent design.  The plan is:
1. add a command-handling seam to `ModuleRunner<TState>` (or a parallel `ICommandHandler<TState>` contract);
2. move command resolution logic out of `PlayerCommandService` and into the owning modules;
3. make `GetMutableModuleState` truly internal-only for bootstrapping and testing.

Until that seam exists, the Application services act as a temporary rule layer.  All direct state mutations are confined to Application-layer command services and do not leak into UI or projection code.

## Current M2-lite integration notes
- `EducationAndExams.Lite` currently reads only `WorldSettlements`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `EducationAndExams.Lite` owns study progress, tutor quality, exam attempts, outcomes, and explanation text; it does not write family prestige or office state directly
- `TradeAndIndustry.Lite` currently reads only `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `TradeAndIndustry.Lite` owns clan trade cash/debt state, market pressure, route pressure, outcomes, and explanation text; it does not write household or clan internals directly
- `PublicLifeAndRumor.Lite` now reads `WorldSettlements`, `PopulationAndHouseholds`, `TradeAndIndustry`, `OrderAndBanditry`, optional `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces only
- `PublicLifeAndRumor.Lite` owns settlement public pulse only: street-talk heat, market bustle, notice visibility, road-report lag, prefecture-dispatch pressure, public legitimacy, dominant-venue wording, monthly cadence / crowd-mix wording, venue-channel competition metrics, and channel-line wording for notice / street talk / road report / prefecture pressure / contention
- `PublicLifeAndRumor.Lite` may now also compress office-owned `AdministrativeTaskLoad` and `ClerkDependence` into xun-only county-gate heat, notice drift, and prefecture-pressure bias through the existing office query seam; month-end readable diffs and events remain public-life-owned and month-bound
- `WorldSettlements` now owns settlement tier / node rank at schema `2`; presentation and public-life projections must not invent county / market / village rank in UI-only code
- M2 and later manifests may enable `PublicLifeAndRumor` as an additive county-public-life layer without changing ownership of household, office, trade, force, or clan state
- great-hall and desk-sandbox public-life summaries must be rebuilt from `IPublicLifeAndRumorQueries` through the presentation bundle only; UI remains read-only
- monthly cadence labels such as fair days, county-gate docket pressure, or road-report bustle must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel descriptors such as documentary weight, market-rumor flow, verification cost, and courier risk must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel wording such as what the posted notice claims, what street talk says, how road reports differ, and how prefecture dispatch presses downward must also remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- bounded public-life responses may surface as read-only command affordances / receipts on hall or desk nodes, but command resolution must still route through `OfficeAndCareer`, `OrderAndBanditry`, or `FamilyCore` rather than `PublicLifeAndRumor`
- both M2-lite modules emit deterministic domain events and keep outcome explanations derived from queryable state plus kernel RNG only
- `NarrativeProjection` currently reads only the shared `WorldDiff` and `DomainEvent` streams plus its own saved history; it does not emit authority events or write foreign module state
- bounded narrative-history trimming may preserve the latest notification per source module before trimming older overflow, so cross-module visibility stays readable without creating a second authority channel
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
- those runtime-only observability summaries may now also include order-owned intervention-carryover counts plus shielding-vs-backlash dominance and hotspot fields, still without creating any write path back into authority state
- those observability summaries are derived after authority simulation and never become a backdoor write channel
- save loading now passes through an explicit migration seam with registrable root/module migration hooks, same-version pass-through, and explicit failure when no path is registered

## M3 local-conflict transition notes
- `OrderAndBanditry.Lite` now owns settlement disorder pressure, route insecurity pressure, suppression demand, explanation text, plus black-route pressure / coercion / paper-compliance / implementation-drag / route-shielding / retaliation-risk / suppression-window summaries inside its own namespace
- `OrderAndBanditry.Lite` reads `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, optional `TradeAndIndustry`, optional `OfficeAndCareer`, and optional `ConflictAndForce` projections only
- `OrderAndBanditry.Lite` converts activated local guard, escort, readiness, and militia posture into slower disorder escalation and lower suppression demand through queries only
- `OrderAndBanditry.Lite` also collapses local-force response into two order-owned handoff signals: route shielding when patrol or escort pressure really protects traffic, and retaliation risk when suppression invites backlash
- `OrderAndBanditry.Lite` now also owns explicit public-life intervention receipts for bounded order verbs on the same settlement lane: command code/label plus short summary/outcome text
- `OrderAndBanditry.Lite` now also owns a one-month intervention follow-through window on that same lane; recent `催护一路` / `添雇巡丁` / `严缉路匪` / `遣人议路` / `暂缓穷追` effects must decay inside order-owned state rather than via UI timers or direct trade writes
- `OrderAndBanditry.Lite` may now also read office-owned jurisdiction leverage, clerk dependence, administrative task load, and petition pressure as bounded administrative-reach inputs; it still may not write office state
- those bounded order verbs may now scale their immediate effect against the same office query seam when governance-lite is enabled; when office is disabled or no jurisdiction is present, the order lane stays on its neutral local baseline
- those same public-life order affordances may now also project a read-only execution outlook from the office query seam so hall or desk surfaces can tell whether a command is likely to land as a full push or only half-arrive; UI still does not resolve authority
- `OfficeAndCareer.Lite` may now also read recent order-intervention carryover plus black-route pressure summaries through order queries only; it converts that aftermath into office-owned task load, petition backlog, petition pressure, clerk dependence, and leverage drift on the next monthly pass, without writing any order state back
- those same public-life order receipts may now also surface a read-only office-aftermath execution summary when the next-month jurisdiction trace still carries the same order command label; the receipt remains a projection and does not create a new write lane
- runtime-only observability and hotspot summaries may now also surface that same order-linked office aftermath as read-only administrative task/backlog context, but those fields remain derived after simulation and never become authority state
- the application-layer presentation bundle may now also project a read-only settlement governance lane by joining public-life, order, and office snapshots for the same settlement; this derived summary remains outside all module-owned authority state
- that same governance lane may also nominate one current public-life lead affordance as a read-only next-step prompt, but the prompt must be selected from the existing player-command projections rather than a second hidden command-resolution path
- that same governance lane may now also carry a read-only xun-facing public-momentum summary derived from current public-life pressure plus office task-load / clerk-drag projections; it remains projection-only and does not create governance-owned cadence state
- the application-layer presentation bundle may also derive one lead governance focus from those settlement governance lanes so hall surfaces can consume a single monthly docket headline without re-sorting inside UI
- that same application-layer bundle may also derive one read-only governance docket from the selected governance focus plus same-settlement notifications, but governance focus remains primary and notifications only decorate why-now / what-next context after authority has already resolved
- that same read-only governance docket may also pull one same-settlement recent handling receipt from the existing command projections, so hall surfaces can read why-now / what-was-done / what-next without inventing a second command ledger
- that same governance docket may also derive a read-only current phase such as `待即刻应对`, `已下处置`, or `案后收束` from governance-lane pressure plus existing receipts/notifications; this phase remains an application join and does not become module-owned workflow state
- the application layer may also derive one read-only `HallDocketStack` from family lifecycle, governance, and warfare-aftercare projections, with one lead item plus secondary items; the stack remains a projection-only ordering seam and may not become a hidden authority scheduler
- each `HallDocketStack` item may also expose neutral ordering/provenance fields such as ordering summary plus source projection/module keys, but those fields must remain shell-agnostic and may not hard-code hall object grammar, paper types, trays, seals, or other presentation objects into shared contracts
- `OrderAndBanditry.Lite` may now also read trade-owned gray-route ledgers after trade executes in the same month, but only through `IBlackRouteLedgerQueries`
- `TradeAndIndustry.Lite` now reads `OrderAndBanditry` projections when enabled and converts that pressure into route risk plus gray-route ledger updates through queries only
- `TradeAndIndustry.Lite` now distinguishes order-owned route shielding from retaliation risk instead of treating all response pressure as the same trade-side penalty
- `TradeAndIndustry.Lite` may react to recent order-intervention follow-through only by reading `IOrderAndBanditryQueries`; it must not persist or decrement foreign carryover state itself
- `TradeAndIndustry.Lite` now also owns active-route blockage / seizure mirrors and route-constraint traces inside its own namespace
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
- the current public-life order lane already supports `催护一路`, `添雇巡丁`, `严缉路匪`, `遣人议路`, and `暂缓穷追` through `PlayerCommandService`; later work should deepen consequences through the same lane rather than invent a second order surface
- `ConflictAndForce.Lite` is available through a conflict-enabled M3 local-conflict bootstrap path and remains absent from active M2 manifests

## Governance-lite notes
- `OfficeAndCareer.Lite` now owns office appointments, authority tier, candidate waiting pressure, clerk dependence, service progression, administrative tasks, petition backlog/outcomes, jurisdiction leverage, petition pressure, jurisdiction task load, and explanation text inside its own namespace
- `OfficeAndCareer.Lite` now reads `EducationAndExams`, `SocialMemoryAndRelations`, and optional `OrderAndBanditry` projections only
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
- built-in `WarfareCampaign` migration now upgrades schema `1 -> 2 -> 3 -> 4` by backfilling labels, route descriptors, directive descriptors, and campaign phasing plus aftermath dockets without changing enabled-module or envelope-key sets
- current warfare-lite aftermath now propagates into `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `TradeAndIndustry`, `OrderAndBanditry`, `OfficeAndCareer`, and `SocialMemoryAndRelations` through the event-handling seam only
- those downstream modules update only their own prosperity, livelihood, prestige, ledger, pressure, petition, or memory state; none of them write back into `WarfareCampaign`

## Post-MVP preflight seam notes
- black-route depth now has explicit preflight query seams: pressure snapshots stay aligned with `OrderAndBanditry`, while gray-route / illicit ledger snapshots stay aligned with `TradeAndIndustry`
- the first active authority slice now persists paper-compliance, implementation-drag, route-shielding, retaliation-risk, administrative suppression-window, escalation-band, seizure-risk, and diversion-band fields inside those two owned modules only
- no standalone `BlackRoute` module key or save namespace should be introduced; future black-route migrations must stay inside the `OrderAndBanditry` and `TradeAndIndustry` module envelopes
