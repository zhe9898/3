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
- the current scheduler now performs a deterministic bounded post-simulation event drain before `NarrativeProjection` runs
- each drain round processes only fresh events emitted since the previous watermark, preserving stable module order and preventing replay of the whole month event list
- handler-emitted follow-on events may trigger downstream handlers in the same month only within the bounded drain cap; if a chain cannot finish within the cap, the remaining pressure must be carried as traceable state rather than hidden recursion

## Pressure-chain topology freeze

Current Renzong thin chains are indexed in `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`.

When changing one of those chains:
- update the index in the same change as code or spec edits
- keep the chain's pressure locus explicit before any local mutation
- keep broad pressures such as frontier, court, regime, disaster, or imperial rhythm from fanning out to every jurisdiction unless the rule explicitly says it is realm-wide
- document whether a same-month follow-on uses the bounded scheduler drain or deliberately carries pressure into the next month
- preserve the distinction between a thin topology proof and the full social chain

## Projection rules
- projections are read models
- projections may be cached
- projections are rebuilt from authoritative state
- projections are not a backdoor write channel
- read-model snapshots may expose read-only traversal helpers such as lane/item lookup when they only normalize visible data access; they must not grow a second composition or ordering layer beyond the already-built projection payload
- `PersonDossiers` are a presentation-layer join over `PersonRegistry`, `FamilyCore`, `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, and optional `SocialMemoryAndRelations` queries. They are not a person master table, do not expand `PersonRegistry`, do not own commands/events, and must be rebuilt from source projections each read.

## Historical-process integration rules

Historical people, reforms, wars, policies, and great trends must use the same Query / Command / DomainEvent channels as ordinary simulation.

They must not enter as:
- UI-side writes
- narrative-triggered authority changes
- hidden global script state
- one-off year triggers that bypass module ownership

They should enter as:
- upstream pressure in the owning modules
- named-person or faction pressure exposed through queries
- policy windows represented as commands, events, or module-owned state
- structured diffs that describe local implementation and backlash
- projection-only notices that explain why a trend is visible now

The player may carry or bend a great trend only through valid influence channels: household, lineage, market, education, yamen, public-life, force, office, or later court-facing seams.
That influence must still resolve through module-owned state and deterministic command/event handling.
At later scale, the same rule applies to rebellion, polity formation, succession struggle, usurpation, restoration, and dynasty repair: no direct timeline rewrite, but earned counterfactual history is allowed when modules own the pressure, force, legitimacy, logistics, and memory state.

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

## Current command-routing seam

`ModuleRunner<TState>` now exposes a module-owned command seam through `HandleCommand(ModuleCommandHandlingScope<TState>)`. `GameSimulation.IssueModuleCommand(...)` builds the normal query registry, delegates the request to the owning module, and refreshes the replay hash only after an accepted mutation.

`PlayerCommandService` is now dispatch glue over a shared player-command catalog: it resolves existing `PlayerCommandRequest` names to module/surface metadata, reports disabled-module rejection, and does not own consequence formulas for the migrated command slices.

Partial closure note (2026-04-24): the public-life order verbs `EscortRoadReport`, `FundLocalWatch`, `SuppressBanditry`, `NegotiateWithOutlaws`, and `TolerateDisorder` now route through the shared module command seam into `OrderAndBanditryModule.HandleCommand(...)`, so their state changes, receipts, refusals, and one-month carryover are owned by `OrderAndBanditry`. `PlayerCommandService` resolves catalog metadata, reports disabled-module rejection, and delegates through `GameSimulation.IssueModuleCommand(...)`; it must not own order-rule effects.

Until the general seam exists, any command route not yet moved into its owning module remains transitional.  Application services may route, gather query-derived modifiers, and adapt results, but new closure work should keep authority effects in the owning module rather than adding new Application-owned rule layers.  UI and projection code must remain read-only.

## Current M2-lite integration notes
- Renzong chain-1 is currently a **real scheduler thin slice with a first household-profile thickening**, not the full tax/corvee society chain: `WorldSettlements.TaxSeasonOpened` drains through `PopulationAndHouseholds.HouseholdDebtSpiked` and settlement-scoped `OfficeAndCareer.YamenOverloaded` into matching-settlement `PublicLifeAndRumor` street-talk heat. `PopulationAndHouseholds` now converts tax season into debt pressure through existing multi-dimensional household state: livelihood exposure, land visibility, grain/cash buffer, labor/dependency load, debt/distress fragility, and interaction terms. The household handler accepts settlement scope when present and preserves the existing symbolic global thin signal until `WorldSettlements` emits precise settlement tax events; Office must carry the resolved settlement and official metadata forward so public-life does not fan one yamen receipt out to unrelated settlements. The full chain still needs formal household-grade/tax-kind formulas, client/tenant rent cascade, market cash squeeze, long memory, precise settlement tax events, and richer jurisdiction capacity formulas.
- Renzong chain-2 is currently a **real scheduler thin slice with a first household-subsistence thickening**, not the full grain/famine economy chain: `WorldSettlements.SeasonPhaseAdvanced(Harvest)` drains through `TradeAndIndustry.GrainPriceSpike` into `PopulationAndHouseholds.HouseholdSubsistencePressureChanged`, with off-scope settlement protection. `TradeAndIndustry` now carries structured grain-market metadata (`oldPrice`, `currentPrice`, `priceDelta`, `supply`, `demand`), while `PopulationAndHouseholds` decides household distress from a multi-dimensional profile: price pressure, grain-store buffer, livelihood market dependency, labor/dependency load, existing debt/distress fragility, and interaction terms. The full chain still needs formal yield ratio, disaster inputs, granary security, route risk, migration/death pressure, order-route insecurity, and long public/memory residue.
- `EducationAndExams.Lite` currently reads only `WorldSettlements`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `EducationAndExams.Lite` owns study progress, tutor quality, exam attempts, outcomes, and explanation text; it does not write family prestige or office state directly
- `EducationAndExams.Lite` emits `ExamPassed` / `ExamFailed` / `StudyAbandoned` with `EntityKey = personId` (added 2026-04-23). `ExamPassed` now carries credential metadata (`examTier`, score, study progress, academy prestige, tutor quality, clan support, favor/shame, stress) so downstream handlers do not parse `Summary`.
- `FamilyCore` now **consumes** `ExamPassed` via `HandleEvents` (thin slice + first credential-prestige thickening, 2026-04-23): looks up the person's clan, computes `Prestige` / `MarriageAllianceValue` deltas from event metadata plus family-owned clan/person state, and emits `ClanPrestigeAdjusted` with structured cause/source/person/delta/profile metadata.
- Downstream modules must treat `ClanPrestigeAdjusted` metadata as cause-specific: exam-pass adjustments are structured; warfare, marriage, or other prestige causes must add equivalent metadata before consumers depend on them.
- `FamilyCore` does **not** yet consume `ExamFailed` or `StudyAbandoned`; `OfficeAndCareer` does **not** yet consume `ExamPassed`; `SocialMemoryAndRelations` now records exam pass/fail/study-abandonment as scoped aspiration/shame pressure and memory residue, but the full office waiting-list / public-life exam projection chain remains deferred.
- `SocialMemoryAndRelations` now owns the pressure-tempering kernel (schema `3`): xun/month passes combine family lineage pressure, household distress/debt/migration, optional trade debt/reputation, mourning/care/funeral pressure, support, mediation, and FamilyCore personality traits into clan climate and person tempering ledgers. It emits only terminal social-memory receipts after owned state changes and does not feed its own receipts back into authority recursion.
- Social-memory event handlers must stay scoped. Trade shock and family branch events target `ClanId`; exam and death events target `PersonId` and resolve clan through `IFamilyCoreQueries`; settlement or campaign events must carry settlement/entity metadata before memory handlers may mutate. Unscoped legacy events remain readable by projections but must not create broad social-memory writes.
- `OfficeAndCareer` now **consumes** `WorldSettlementsEventNames.ImperialRhythmChanged` for the chain-4 thin slice + first amnesty execution thickening (2026-04-23): it reads `IWorldSettlementsQueries.GetCurrentSeason().Imperial.AmnestyWave`, emits one settlement-scoped `OfficeAndCareer.AmnestyApplied` per jurisdiction only when the wave is newly above threshold, carries structured execution metadata (`AmnestyWave`, authority tier, jurisdiction leverage, clerk dependence, petition backlog, administrative task load), and persists `LastAppliedAmnestyWave` to avoid repeating the same amnesty when other imperial axes change.
- `OrderAndBanditry` now **consumes** `OfficeAndCareer.AmnestyApplied` for the same chain-4 slice: it mutates only the matching settlement's `DisorderPressure` through an order-owned amnesty-disorder profile using the office metadata plus local disorder soil (`DisorderPressure`, `BanditThreat`, `BlackRoutePressure`, `CoercionRisk`, `RoutePressure`, suppression buffers), then emits `OrderAndBanditry.DisorderSpike` with cause/profile metadata when the settlement crosses the public-spike threshold. `OrderAndBanditry.DisorderLevelChanged` remains the fuller periodic summary event for later expansion.
- Renzong chain-5 is currently a **real scheduler thin slice with a first frontier-supply household-profile thickening**, not the full frontier/war economy chain: `WorldSettlements.FrontierStrainEscalated` drains through `OfficeAndCareer.OfficialSupplyRequisition` into `PopulationAndHouseholds.HouseholdBurdenIncreased` when household distress crosses the receipt threshold. `OfficeAndCareer` now converts the frontier event into structured supply-execution metadata (`FrontierPressure`, severity, quota pressure, docket pressure, clerk distortion, authority buffer) and leaves office-owned task/backlog pressure on the matching jurisdiction. `PopulationAndHouseholds` then computes household burden from that metadata plus existing household dimensions: livelihood exposure, grain/tool/shelter buffer, labor/dependency load, debt/distress fragility, migration pressure, and interaction terms. The full chain still needs frontier sector allocation, WarfareCampaign mobilization, ConflictAndForce readiness, TradeAndIndustry market diversion, formal quota/cash formulas, public-life military-burden projection, and long memory residue.
- `WorldSettlements.FrontierStrainEscalated` is settlement-scoped in this slice: `EntityKey` must be a `SettlementId`, not a global `frontier` token. `OfficeAndCareer` must emit `OfficialSupplyRequisition` only for a matching jurisdiction and must not fan one frontier signal out to all jurisdictions.
- `WorldSettlements` schema `8` persists `LastDeclaredFrontierStrainBand` as a module-owned watermark so the same active frontier band is not re-declared every month. Future multi-frontier work should replace this thin-slice scalar with per-sector/per-settlement watermarks before adding simultaneous fronts or recurring quota cadence.
- `PopulationAndHouseholds.HouseholdBurdenIncreased` is a downstream receipt event for threshold-crossing household burden; it must be declared in `PublishedEvents` and must carry structured cause/source/settlement/profile metadata rather than asking later modules to parse narrative summaries.
- Renzong chain-6 is currently a **real scheduler thin slice with a first disaster-disorder profile thickening**, not the full disaster-relief society chain: `WorldSettlements.DisasterDeclared` drains through `OrderAndBanditry.DisorderSpike` into `PublicLifeAndRumor` street-talk heat, with off-scope settlement protection. `OrderAndBanditry` now computes disorder delta from disaster metadata plus local order soil (`DisorderPressure`, `BanditThreat`, `BlackRoutePressure`, `CoercionRisk`, route rupture / retaliation / implementation drag) and suppression buffers (`SuppressionRelief`, `RouteShielding`, `ResponseActivationLevel`, `AdministrativeSuppressionWindow`), then emits `DisorderSpike` with profile metadata when the threshold is crossed.
- `WorldSettlements.DisasterDeclared` must carry rule-readable metadata (`cause`, `disasterKind`, `severity`, `floodRisk`, `embankmentStrain`) and `OrderAndBanditry` must use that metadata rather than parsing `Summary`; the current slice implements flood only, and the current thickening still remains inside Order-owned state.
- `WorldSettlements` schema `7` persists `LastDeclaredFloodDisasterBand` as a module-owned watermark so the same active flood band is not re-declared every month. Future multi-locus disasters should replace this thin-slice scalar with a per-disaster/per-settlement watermark before adding drought, locust, epidemic, or simultaneous flood fronts.
- `OrderAndBanditry.DisorderSpike` now carries cause/profile metadata (`corvee`, `amnesty`, or `disaster`) so `PublicLifeAndRumor` can project cause-appropriate guidance without hard-coding every disorder spike as corvee pressure. The full chain-6 still needs relief decisions, household subsistence / migration, market panic, route insecurity, SocialMemory residue, and public legitimacy.
- Renzong chain-7 is currently a **real scheduler thin slice with a first clerk-capture profile thickening**, not the full official-clerk-execution chain: `OfficeAndCareer.ClerkCaptureDeepened` is emitted only on a newly crossed settlement clerk-capture edge and drains into scoped `PublicLifeAndRumor` heat. `OfficeAndCareer` now emits structured clerk-capture profile metadata (`ClerkDependence`, `PetitionBacklog`, `AdministrativeTaskLoad`, `PetitionPressure`, authority / leverage buffer, capture pressure components). `PublicLifeAndRumor` uses that metadata to scale street-talk heat while preserving the legacy +12 fallback for older/simple events. `OfficeAndCareer` schema `7` persists `ActiveClerkCaptureSettlementIds` plus later refusal-response trace fields as module-owned office state; repeated high clerk dependence must not re-emit until the condition clears, and off-scope settlements must stay untouched.
- Renzong chain-8 is currently a **real scheduler thin slice with a first policy-window profile thickening**, not the full court-agenda/policy-dispatch chain: `WorldSettlements.CourtAgendaPressureAccumulated` remains court/global input, but `OfficeAndCareer` must allocate it to one selected court-facing jurisdiction before emitting `PolicyWindowOpened`. Selection is office-owned and uses mandate deficit, authority tier, jurisdiction leverage, petition signal, administrative task load, clerk dependence, and petition backlog; the emitted event carries those profile components as metadata. The handler must not fan one court pressure event into all known jurisdictions, and high local drag can prevent a window even under low mandate confidence.
- Renzong chain-9 is currently a **real scheduler thin slice with a first defection-risk profile thickening**, not the full regime-recognition/compliance chain: `WorldSettlements.RegimeLegitimacyShifted` updates office-owned `OfficialDefectionRisk`, then `OfficeAndCareer.OfficeDefected` is emitted only as a receipt after one highest-risk appointed official actually loses appointment eligibility. The risk profile uses mandate deficit, demotion pressure, clerk pressure, petition pressure, reputation strain, and authority/reputation buffer; a low mandate signal alone must not defect a well-buffered official. Downstream household, market, public-life, memory, and force reactions remain deferred until their owning modules receive a fuller regime-pressure contract.
- Chain-8 / chain-9 imperial pressure must be explicitly seeded or moved by an imperial/court owner. A default `WorldSettlements` state uses neutral `MandateConfidence = 70`; an uninitialized world must not generate court crisis or regime defection by accident.
- `TradeAndIndustry.Lite` currently reads only `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces
- `TradeAndIndustry.Lite` owns clan trade cash/debt state, market pressure, route pressure, outcomes, and explanation text; it does not write household or clan internals directly
- `PublicLifeAndRumor.Lite` now reads `WorldSettlements`, `PopulationAndHouseholds`, `TradeAndIndustry`, `OrderAndBanditry`, optional `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` through query interfaces only
- `PublicLifeAndRumor.Lite` owns settlement public pulse only: street-talk heat, market bustle, notice visibility, road-report lag, prefecture-dispatch pressure, public legitimacy, dominant-venue wording, monthly cadence / crowd-mix wording, venue-channel competition metrics, and channel-line wording for notice / street talk / road report / prefecture pressure / contention
- `PublicLifeAndRumor.Lite` may now also compress office-owned `AdministrativeTaskLoad` and `ClerkDependence` into day-facing short-band county-gate heat, notice drift, and prefecture-pressure bias through the existing office query seam; month-end readable diffs and events remain public-life-owned and month-bound
- `WorldSettlements` now owns settlement tier / node rank at schema `2`; presentation and public-life projections must not invent county / market / village rank in UI-only code
- M2 and later manifests may enable `PublicLifeAndRumor` as an additive county-public-life layer without changing ownership of household, office, trade, force, or clan state
- great-hall and desk-sandbox public-life summaries must be rebuilt from `IPublicLifeAndRumorQueries` through the presentation bundle only; UI remains read-only
- monthly cadence labels such as fair days, county-gate docket pressure, or road-report bustle must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel descriptors such as documentary weight, market-rumor flow, verification cost, and courier risk must remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- public-life channel wording such as what the posted notice claims, what street talk says, how road reports differ, and how prefecture dispatch presses downward must also remain `PublicLifeAndRumor`-owned descriptors rather than being synthesized inside shell code
- bounded public-life responses may surface as read-only command affordances / receipts on hall or desk nodes, but command resolution must still route through `OfficeAndCareer`, `OrderAndBanditry`, or `FamilyCore` rather than `PublicLifeAndRumor`
- both M2-lite modules emit deterministic domain events and keep outcome explanations derived from queryable state plus kernel RNG only
- `NarrativeProjection` currently reads only the shared `WorldDiff` and `DomainEvent` streams plus its own saved history; it does not emit authority events or write foreign module state
- `NarrativeProjection` may use `FamilyCore` death pressure phrases carried in death diffs, such as `承祧缺口1阶` or `承祧缺口3阶`, to shape family-facing next-step guidance; this remains projection text and must not become a hidden funeral, inheritance, or command-resolution lane
- when a `DeathByViolence` source event targets a clan `PersonId`, `NarrativeProjection` may pull the matching `FamilyCore` death-pressure diff into the notice trace and may also create an ancestral-hall diff notice; both remain downstream projection and do not cause authority state changes
- bounded narrative-history trimming may preserve the latest notification per source module before trimming older overflow, so cross-module visibility stays readable without creating a second authority channel
- the current first-pass presentation shell consumes a read-model bundle only; it does not reference simulation modules directly and does not resolve commands or authority rules inside UI code
- the application-layer presentation bundle may also compose `HouseholdSocialPressureSnapshot` and `PlayerInfluenceFootprintSnapshot` from existing household, family, trade, education, public-life, order, office, and warfare projections; this is runtime-only visibility, not a stored route tag, player class, module key, or authority shortcut
- that influence footprint must distinguish the player's anchor household from observed household pressure: the anchor can carry local agency summaries, while outside households stay readable but not directly commandable unless a real social touchpoint exists

## Family-conflict vertical slice notes
- `FamilyCore` now owns lineage-conflict pressure, mediation momentum, branch-favor pressure, relief-sanction pressure, and last family-command receipts inside the family namespace
- `FamilyCore` schema `8` also owns marriage-alliance pressure/value, heir security, reproductive pressure, mourning load, care load, funeral debt, remedy confidence, charity obligation, clan-scoped spouse/parent/child links, last lifecycle-command receipts, and public-life refusal-response trace fields inside the same namespace
- `FamilyCore` may use `PersonRegistry` command interfaces only for identity-shaped writes when birth, marriage-in spouse creation, or death requires a canonical person anchor; all lineage facts and lifecycle pressures remain `FamilyCore`-owned
- `FamilyCore` may consume `DeathByViolence` from conflict / order / warfare producers only as a lineage-pressure bridge: it parses the event `EntityKey` as `PersonId`, reads identity facts through `PersonRegistry`, updates only clan-owned mourning / inheritance / branch / heir-security pressures, and must not emit a second cause-specific death event from that handler
- family command routing now uses the shared module command seam: `PlayerCommandService` selects the `FamilyCore` module, while `FamilyCoreCommandResolver` owns family consequence formulas and mutates only `FamilyCore` state
- `FamilyCoreCommandResolver` may read `PersonRegistry` queries for identity-only facts such as alive / age when choosing candidates; it must not mutate `PersonRegistry` except through already authorized birth/death identity paths
- `FamilyCoreCommandResolver` may read `SocialMemoryAndRelations` clan climate and adult pressure-tempering snapshots as deterministic command-friction inputs; missing SocialMemory queries remain neutral
- `SocialMemoryAndRelations` may read those family-conflict fields through queries only; it may not be written by the player-command service or by the family command resolver
- bounded family intents such as branch favor, formal apology, branch separation, relief suspension, elder mediation, marriage arrangement, heir designation, newborn care, and mourning order must route into `FamilyCore` through the shared module command seam
- the family-council shell now reads clan conflict summaries, clan narratives, family affordances, and family receipts from read models only
- built-in default loaders now migrate legacy `FamilyCore` schemas through current schema `7` without changing enabled-module or envelope-key sets

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
- `OrderAndBanditry.Lite` now owns the corresponding public-life order command resolver for `EscortRoadReport`, `FundLocalWatch`, `SuppressBanditry`, `NegotiateWithOutlaws`, and `TolerateDisorder`; the application layer supplies only the settlement id, command name/label, and query-derived office-reach modifiers
- `OrderAndBanditry.Lite` may now also read office-owned jurisdiction leverage, clerk dependence, administrative task load, and petition pressure as bounded administrative-reach inputs; it still may not write office state
- those bounded order verbs may now scale their immediate effect against the same office query seam when governance-lite is enabled; when office is disabled or no jurisdiction is present, the order lane stays on its neutral local baseline
- those same public-life order affordances may now also project a read-only execution outlook from the office query seam so hall or desk surfaces can tell whether a command is likely to land as a full push or only half-arrive; UI still does not resolve authority
- `OfficeAndCareer.Lite` may now also read recent order-intervention carryover plus black-route pressure summaries through order queries only; it converts that aftermath into office-owned task load, petition backlog, petition pressure, clerk dependence, and leverage drift on the next monthly pass, without writing any order state back
- those same public-life order receipts may now also surface a read-only office-aftermath execution summary when the next-month jurisdiction trace still carries the same order command label; the receipt remains a projection and does not create a new write lane
- `SocialMemoryAndRelations` may now read the same query-visible public-life order aftermath during its next monthly pass and persist owner-owned residue such as watch obligation, protection favor, crackdown fear, public shame, or grudge pressure inside social-memory state only
- runtime-only observability and hotspot summaries may now also surface that same order-linked office aftermath as read-only administrative task/backlog context, but those fields remain derived after simulation and never become authority state
- the application-layer presentation bundle may now also project a read-only settlement governance lane by joining public-life, order, and office snapshots for the same settlement; this derived summary remains outside all module-owned authority state
- that same governance lane may also nominate one current public-life lead affordance as a read-only next-step prompt, but the prompt must be selected from the existing player-command projections rather than a second hidden command-resolution path
- that same governance lane may now also carry a read-only day-facing public-momentum summary derived from current public-life pressure plus office task-load / clerk-drag projections; it remains projection-only and does not create governance-owned cadence state
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
- event observability must classify each emitted-but-unconsumed or declared-but-not-yet-emitted event as one of: projection-only receipt, future contract, dormant source, or alignment bug; a pressure-chain slice is not complete until that classification is documented or tested
- `OrderAndBanditry.Lite` remains available through an order-enabled M3 bridge bootstrap path
- the current public-life order lane already supports `催护一路`, `添雇巡丁`, `严缉路匪`, `遣人议路`, and `暂缓穷追` through thin `PlayerCommandService` routing into the `OrderAndBanditry` resolver; later work should deepen consequences through the same lane rather than invent a second order surface
- `ConflictAndForce.Lite` is available through a conflict-enabled M3 local-conflict bootstrap path and remains absent from active M2 manifests

## Playable closure v2 integration note - 2026-04-24
- `chain1-public-life-order-v2` proves the public-life order lane as pressure -> order-owned state -> projection -> shell affordance -> bounded command -> order-owned receipt/refusal -> next-month governance/order readback.
- Application routing may still pass the existing public-life order command into `OrderAndBanditryModule.HandlePublicLifeCommand`, but it must not compute the order outcome itself; office reach is supplied as query-derived modifiers only.
- Projection joins may show office aftermath tied to the prior order receipt, but no projection, notification, or shell adapter may parse `DomainEvent.Summary` or mutate module state.
- When `OrderAndBanditry` is disabled, public-life order commands must return a refusal and leave save/module projection state unchanged.

## Playable closure v3 leverage note - 2026-04-24
- `chain1-public-life-order-leverage-v3` deepens the same lane with read-only household leverage, cost, and next-month readback text on affordances, receipts, and the governance docket.
- The projection may read existing family prestige/support, social-memory clan narrative, office reach, trade-route exposure, public-life heat, and order pressure snapshots from the presentation bundle. These joins remain runtime-only and do not create a new query interface, command, domain event, module state, or save field.
- The leverage text explains what the home-household is spending, such as lineage face, yamen/document reach, cash/watch labor, mediation, trade exposure, or tolerated ground risk. It is not an authority formula and must not be parsed by later modules.

## Playable closure v4 social-memory residue note - 2026-04-25
- `public-life-order-social-memory-residue-v4` chooses a rule-driven query-visible aftermath seam rather than a command-time event path: `OrderAndBanditry` stores the accepted public-life order receipt/carryover, and `SocialMemoryAndRelations` reads that structured state on the following monthly pass.
- The SocialMemory pass must use fields such as `LastInterventionCommandCode`, `InterventionCarryoverMonths`, `RouteShielding`, `RetaliationRisk`, `BlackRoutePressure`, `CoercionRisk`, and `ImplementationDrag`; it must not parse `DomainEvent.Summary` or command receipt prose.
- The write boundary is strict: Order writes order state, SocialMemory writes social-memory state, and Application/UI/Unity only join read models for `社会记忆读回`.
- Because the residue uses existing SocialMemory schema v3 records, this pass does not add a new module state field, migration, command, feature pack, `PersonRegistry` field, or manager layer.

## Playable closure v5 refusal-residue note - 2026-04-25
- `public-life-order-refusal-residue-v5` extends the same rule-driven loop to partial and refused `添雇巡丁` / `严缉路匪`: Order resolution writes structured outcome/refusal/partial/trace codes, not just receipt prose.
- `OrderAndBanditry` is the only owner of refusal / partial authority classification. Application may pass office-reach modifiers from queries, but it may not compute `县门未落地`, `地方拖延`, or refusal causes.
- `SocialMemoryAndRelations` must consume `LastInterventionOutcomeCode`, `LastInterventionRefusalCode`, `LastInterventionPartialCode`, `RefusalCarryoverMonths`, and pressure fields through `IOrderAndBanditryQueries`; it must not parse `LastInterventionSummary`, `LastInterventionOutcome`, or `DomainEvent.Summary`.
- Governance, public-life, family, and Unity shell readback may display refusal residue only through `PresentationReadModelBundle.SocialMemories`, command receipts, and governance summaries.
- Save impact is limited to `OrderAndBanditry` schema `7 -> 8`; SocialMemory residue continues to use schema `3`.

## Playable closure v6 refusal-response note - 2026-04-25
- `public-life-order-refusal-response-v6` completes the post-refusal account as a rule-driven response loop, not an event-centered or event-pool design: Month N refusal/partial residue is projected in Month N+1, a bounded response affordance is selected from the read model, the owning module resolves the response, and Month N+2 `SocialMemoryAndRelations` reads structured aftermath.
- Response command ownership is split by actual authority: `OrderAndBanditry` owns `补保巡丁`, `赔脚户误读`, and `暂缓强压`; `OfficeAndCareer` owns `押文催县门` and `改走递报`; `FamilyCore` owns `请族老解释`.
- Response aftermath must be one of structured outcome codes `Repaired`, `Contained`, `Escalated`, or `Ignored`, with module-owned trace codes. No Application service, projection, UI, or Unity adapter may compute whether the response worked.
- `SocialMemoryAndRelations` reads only structured response fields such as `LastRefusalResponseCommandCode`, `LastRefusalResponseOutcomeCode`, `LastRefusalResponseTraceCode`, and `ResponseCarryoverMonths`; it writes only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- Governance lanes, public-life receipts, family surfaces, and Unity shell objects may show whether the后账 was repaired, contained, worsened, or left alone only through projected readbacks. Unity must copy projection fields and must not query modules.
- Save impact is module-local: `OrderAndBanditry` `8 -> 9`, `OfficeAndCareer` `6 -> 7`, and `FamilyCore` `7 -> 8`; `SocialMemoryAndRelations` remains schema `3`.
- `public-life-order-residue-decay-friction-v7` extends the same rule-driven response loop after Month N+2: `SocialMemoryAndRelations` may soften repaired residue, carry contained obligation, or harden escalated/ignored residue by adjusting existing memory weight, clan narrative pressure, and clan emotional climate only.
- Later `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` commands may read structured `SocialMemoryEntrySnapshot` response cause keys and weights as bounded repeat-friction inputs, then mutate only their own owner state. They must not parse social-memory summary prose, receipt prose, or `DomainEvent.Summary`.
- v7 adds no persisted fields and no migration. It reuses SocialMemory schema `3` and the v6 owner response trace fields.
- `public-life-order-actor-countermove-v8` adds a later monthly passive back-pressure layer: `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` may read structured SocialMemory response memories and local clan scope, skip current-month memories, and resolve small deterministic actor countermoves inside their own monthly rules.
- Order actor movement owns route-watch / runner / road-tail outcomes such as `巡丁自补保` or `脚户误读反噬`; Office actor movement owns yamen / clerk / document outcomes such as `县门自补落地` or `胥吏续拖`; Family actor movement owns elder explanation / household guarantee outcomes such as `族老自解释` or `族老避羞`.
- v8 countermoves mutate only the owning module's pressure and existing response trace fields. `SocialMemoryAndRelations` does not resolve those countermoves; it may later read the structured owner aftermath through queries and write only SocialMemory-owned residue.
- v8 adds no persisted fields and no migration. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- `public-life-order-ordinary-household-readback-v10` adds a projection-only ordinary-household readback layer. Application may join `HouseholdPressureSnapshot` with structured Order / Office / Family public-life aftermath to build `HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue`, but it must not mutate `PopulationAndHouseholds`, compute response outcomes, or create a new household command owner.
- Desk Sandbox / Unity may copy that household readback from `PresentationReadModelBundle.HouseholdSocialPressures` only. This readback can explain night-road fear, runner/watch misunderstanding, labor/debt/migration strain, and yamen delay, while command resolution remains owner-module and SocialMemory residue remains SocialMemory-owned.
- v10 adds runtime read-model constants only and no persisted fields, schema bump, or migration.
- `public-life-order-ordinary-household-play-surface-v11` may enrich existing public-life response affordances / receipts with a selected ordinary-household stake from `PresentationReadModelBundle.HouseholdSocialPressures`. This is read-model enrichment only: it may expose costed choice and next-readback text, but it must not add a `HouseholdId` command target, compute response effectiveness, mutate `PopulationAndHouseholds`, or write SocialMemory.
- Unity may display those enriched `PlayerCommandAffordanceSnapshot` / `PlayerCommandReceiptSnapshot` fields only. It must not query modules, rescore households, or decide repaired / contained / escalated / ignored outcomes.
- v11 adds no persisted fields, command request shape change, schema bump, or migration.
- `public-life-order-home-household-local-response-v12` adds a narrow population-owned local response surface: `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` route through the shared player-command catalog into `PopulationAndHouseholdsModule.HandleCommand(...)`.
- v12 command-time mutation is limited to `PopulationAndHouseholds` household labor, debt, distress, migration, and `LastLocalResponse*` / `LocalResponseCarryoverMonths` fields. It must not mutate `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `SocialMemoryAndRelations`, `PublicLifeAndRumor`, or `PersonRegistry`.
- v12 projections may expose those home-household affordances and receipts when v5/v10/v11 residue is visible, but projection/UI/Unity code must not compute outcome effectiveness and must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- v12 save impact is module-local: `PopulationAndHouseholds` schema `2 -> 3`; Order remains schema `9`, Office schema `7`, Family schema `8`, and SocialMemory schema `3`.
- `public-life-order-home-household-social-memory-v13` lets `SocialMemoryAndRelations` read the structured v12 local-response aftermath on the later monthly pass and convert `Relieved`, `Contained`, `Strained`, or `Ignored` into SocialMemory-owned residue.
- v13 SocialMemory writes are limited to existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`; the reader must not parse `LastLocalResponseSummary`, receipt prose, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- v13 projection may append the resulting SocialMemory readback to home-household local response receipts, but Application / UI / Unity still do not compute outcome effectiveness or write social memory. v13 adds no persisted state, schema bump, or migration.
- `public-life-order-home-household-repeat-friction-v14` lets `PopulationAndHouseholds` read structured v13 SocialMemory residue when resolving later local household responses. `Relieved` can supply small local support, `Contained` can leave obligation drag, and `Strained` / `Ignored` can add debt or labor drag.
- v14 command-time mutation remains limited to `PopulationAndHouseholds`; `SocialMemoryAndRelations` is read through queries only and is not mutated by the command resolver.
- v14 projections may display the existing SocialMemory hint on local response affordances / receipts, but Application / UI / Unity must not compute the command outcome. v14 adds no persisted state, schema bump, or migration.
- `public-life-order-common-household-response-texture-v15` adds a small population-owned texture profile to the same local household response lane. The resolver reads only existing household pressure fields (`DebtPressure`, `LaborCapacity`, `Distress`, `MigrationRisk`, `DependentCount`, `LaborerCount`, `Livelihood`) and writes only population-owned household cost/outcome trace.
- v15 Application projections may display `本户底色` hints from `HouseholdPressureSnapshot` so the player can distinguish debt-heavy compensation, labor-thin night restriction / road messaging, distress-heavy face pressure, and migration-prone avoidance. These hints are read-model text only; UI and Unity must not query modules or compute final response effectiveness.
- v15 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-response-capacity-v16` adds a projected `回应承受线` to those same three local household responses. Application may derive the affordance enabled/unfit hint from existing `HouseholdPressureSnapshot` fields, but it still does not compute final outcomes or mutate state.
- v16 command-time capacity consequences remain population-owned: `PopulationAndHouseholds` may mark over-line debt or floor-level labor as a strained local aftermath and may append a capacity summary tail, while `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, and `PersonRegistry` remain untouched at command time.
- v16 Unity / shell behavior is copy-only for projected availability, cost, readback, and receipt fields. v16 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-response-tradeoff-v17` adds projected `取舍预判` to those same three local household responses. Application may derive expected benefit, recoil tail, and external-afteraccount boundary from existing `HouseholdPressureSnapshot` fields, but it still does not compute final outcomes or mutate state.
- v17 command-time tradeoff summaries remain population-owned: `PopulationAndHouseholds` may append command summary tails for the local tradeoff, while `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, `SocialMemoryAndRelations`, and `PersonRegistry` remain untouched at command time.
- v17 Unity / shell behavior is copy-only for projected `取舍预判`, `预期收益`, `反噬尾巴`, `外部后账`, and receipt fields. v17 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-short-term-readback-v18` adds projected receipt-side short-term consequence readback after a local household response resolves. Application may derive `缓住项`, `挤压项`, and `仍欠外部后账` from existing `HouseholdPressureSnapshot` fields plus structured `LastLocalResponse*` codes, but it still does not compute final outcomes or mutate state.
- v18 does not add command-time mutation beyond the existing `PopulationAndHouseholds` local response resolver. `SocialMemoryAndRelations` continues to read only structured aftermath on the later monthly pass and must not parse `短期后果`, `缓住项`, `挤压项`, `仍欠外部后账`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- v18 Unity / shell behavior is copy-only for projected receipt fields. v18 adds no persisted state, command request shape change, schema bump, or migration.
- `public-life-order-home-household-follow-up-affordance-v19` adds projected follow-up hints to local response affordances after a prior local response. Application may derive repeat/switch/cooldown hints from existing `HouseholdPressureSnapshot` fields plus structured `LastLocalResponse*` codes, but it still does not compute final outcomes or mutate state.
- v19 does not add command-time mutation beyond the existing `PopulationAndHouseholds` local response resolver. `SocialMemoryAndRelations` must not parse `续接提示`, `换招提示`, `冷却提示`, `续接读回`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- v19 Unity / shell behavior is copy-only for projected affordance fields. v19 adds no persisted state, command request shape change, schema bump, cooldown ledger, repeated-response counter, or migration.
- `public-life-order-owner-lane-return-guidance-v20` adds projected `外部后账归位` guidance to local response affordances and receipts after a v12-v19 home-household response. Application may join this guidance from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponse*` codes, but it still does not compute final outcomes or mutate state.
- v20 explicitly returns external after-accounts to owner lanes: `OrderAndBanditry` for 巡丁/路匪/route pressure repair, `OfficeAndCareer` for 县门/文移/胥吏续拖, `FamilyCore` for 族老解释/本户担保/宗房脸面, and `SocialMemoryAndRelations` for later durable shame/fear/favor/grudge/obligation residue.
- v20 does not add command-time mutation beyond the existing `PopulationAndHouseholds` local response resolver. `SocialMemoryAndRelations` must not parse `外部后账归位`, `该走巡丁`, `该走县门`, `该走族老`, `本户不能代修`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- v20 Unity / shell behavior is copy-only for projected fields. v20 adds no persisted state, command request shape change, schema bump, cooldown ledger, owner-lane ledger, household target field, or migration.
- `public-life-order-owner-lane-return-surface-readback-v21` carries the same owner-lane return projection into Office/Governance and Family-facing surfaces. Application may join it from existing household read models, but it must not parse receipts, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v21 keeps ownership unchanged: Order repairs road-watch / route-pressure, Office handles county-yamen / document / clerk drag, Family handles elder explanation / guarantee face, and SocialMemory writes durable residue only on its later structured read.
- v21 Unity / shell behavior remains copy-only for projected fields. v21 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, household target field, or new SocialMemory field.
- `public-life-order-owner-lane-handoff-entry-readback-v22` adds projected `承接入口` text to the same owner-lane guidance. It may name existing command labels such as `添雇巡丁`, `押文催县门`, or `请族老解释`, but it does not create a new command system, command queue, recommendation ledger, or routing layer.
- v22 keeps the same no-summary-parsing rule: Application may derive the guidance from existing `HouseholdPressureSnapshot` structured local-response fields only; SocialMemory must not parse `承接入口`, receipt prose, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v22 Unity / shell behavior remains copy-only for projected fields. v22 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-receipt-status-readback-v23` adds projected `归口状态` text when the relevant owner lane already has structured public-life refusal-response fields. This is owner-lane归口 status, not "社会其他人接手" and not automatic repair.
- v23 may join existing `HouseholdPressureSnapshot.LastLocalResponse*` with `SettlementDisorderSnapshot.LastRefusalResponse*`, `JurisdictionAuthoritySnapshot.LastRefusalResponse*`, or `ClanSnapshot.LastRefusalResponse*`. It must not parse `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v23 Unity / shell behavior remains copy-only for projected fields. v23 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-outcome-reading-guidance-v24` adds projected `归口后读法` text from existing owner-lane outcome codes. It may render `已修复：先停本户加压`, `暂压留账：仍看本 lane 下月`, `恶化转硬：别让本户代扛`, or `放置未接：仍回 owner lane`, but it does not calculate the outcome.
- v24 may join existing `HouseholdPressureSnapshot.LastLocalResponse*` with structured owner snapshots and `LastRefusalResponseOutcomeCode` only. It must not parse `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v24 Unity / shell behavior remains copy-only for projected fields. v24 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-social-residue-readback-v25` adds projected `社会余味读回` text after existing SocialMemory response residue is visible. It may render `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸` from structured outcome codes and the matching active SocialMemory response cause key, but it does not calculate residue or mutate SocialMemory.
- v25 may join existing `HouseholdPressureSnapshot.LastLocalResponse*`, owner-lane `LastRefusalResponseCommandCode` / `LastRefusalResponseOutcomeCode`, and `SocialMemoryEntrySnapshot.CauseKey` / `State` / `Weight` / `OriginDate` only. It must not parse SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- `SocialMemoryAndRelations` remains the only writer of durable social residue. Its same-month memory de-duplication now includes `CauseKey`, so distinct owner-lane after-accounts with the same memory kind stay separate without creating a new ledger or namespace.
- v25 Unity / shell behavior remains copy-only for projected fields. v25 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-social-residue-followup-v26` adds projected `余味冷却提示`, `余味续接提示`, and `余味换招提示` text after existing v25 `社会余味读回` is visible. It may tell the player to cool down, lightly continue in the owner lane, switch owner-lane tactic, or wait for a better owner-lane entry, but it does not create a new follow-up command system.
- v26 may join existing `HouseholdPressureSnapshot.LastLocalResponse*`, owner-lane `LastRefusalResponseCommandCode` / `LastRefusalResponseOutcomeCode`, and `SocialMemoryEntrySnapshot.CauseKey` / `State` / `Weight` / `OriginDate` only. It must not parse `余味冷却提示`, `余味续接提示`, `余味换招提示`, SocialMemory summary prose, owner-lane guidance prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- v26 Unity / shell behavior remains copy-only for projected fields. v26 adds no persisted state, command request shape change, schema bump, migration, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, follow-up ledger, household target field, command queue, or new SocialMemory field.
- `public-life-order-owner-lane-affordance-echo-v27` adds projected `现有入口读法` text to existing owner-lane affordance readback. It may say `建议冷却`, `可轻续`, `建议换招`, or `等待承接口`, but command availability and command routing still come from the existing owner modules and catalog.
- `public-life-order-owner-followup-receipt-closure-v28` adds projected `后手收口读回` text to owner-lane receipts from existing outcome codes and matching SocialMemory cause keys. It must not parse receipt prose, `LastRefusalResponseSummary`, SocialMemory summary prose, or `DomainEvent.Summary`.
- `public-life-order-owner-lane-no-loop-guard-v29` adds projected `闭环防回压` text so old guidance remains readback and does not point back to the home household. It adds no stale-guidance ledger, cooldown ledger, repeated-response counter, or command target.
- `public-life-order-owner-lane-v30-closure-audit` records v20-v30 as a complete projection/readback closure arc. v27-v30 Unity / shell behavior remains copy-only for projected fields and adds no persisted state, command request shape change, schema bump, migration, owner-lane ledger, receipt-status ledger, outcome ledger, follow-up ledger, SocialMemory ledger, household target field, command queue, or new SocialMemory field.
- `backend-event-contract-health-v32` classifies ten-year diagnostic DomainEvent contract debt as `ProjectionOnlyReceipt`, `FutureContract`, `DormantSeededPath`, `AcceptanceTestGap`, `AlignmentBug`, or `Unclassified`. This is diagnostic/readback evidence only: it does not add event-pool authority, module rules, persisted state, migration, command surfaces, or projection wording.
- v32 diagnostics may normalize event keys for readability, but they must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or projection text. Any unclassified event contract debt must be documented before being used as proof that a pressure-chain slice is healthy or broken.

## Governance-lite notes
- `OfficeAndCareer.Lite` now owns office appointments, authority tier, candidate waiting pressure, clerk dependence, service progression, administrative tasks, petition backlog/outcomes, jurisdiction leverage, petition pressure, jurisdiction task load, and explanation text inside its own namespace
- `OfficeAndCareer.Lite` now reads `EducationAndExams`, `SocialMemoryAndRelations`, and optional `OrderAndBanditry` projections only
- the new governance-lite bootstrap path enables `OfficeAndCareer` without mutating the stable M2 or M3 manifests
- bounded office intents such as petition review, administrative leverage, county notice, and road dispatch now route through `OfficeAndCareerCommandResolver`; they may not write family, trade, order, or force state directly
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
- `WarfareCampaignCommandResolver` now stages `DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, and `WithdrawToBarracks` into `WarfareCampaign`-owned directive state only
- the current player-command vertical slice may expose those same warfare directives as read-only affordances and receipts in presentation, but authoritative resolution stays in the module command seam and writes stay inside `WarfareCampaign`
- current warfare-lite state now persists active directive code/label/summary and last directive trace inside the warfare namespace instead of inventing a cross-module command ledger
- built-in `WarfareCampaign` migration now upgrades schema `1 -> 2 -> 3 -> 4` by backfilling labels, route descriptors, directive descriptors, and campaign phasing plus aftermath dockets without changing enabled-module or envelope-key sets
- current warfare-lite aftermath now propagates into `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `TradeAndIndustry`, `OrderAndBanditry`, `OfficeAndCareer`, and `SocialMemoryAndRelations` through the event-handling seam only
- those downstream modules update only their own prosperity, livelihood, prestige, ledger, pressure, petition, or memory state; none of them write back into `WarfareCampaign`

## Post-MVP preflight seam notes
- black-route depth now has explicit preflight query seams: pressure snapshots stay aligned with `OrderAndBanditry`, while gray-route / illicit ledger snapshots stay aligned with `TradeAndIndustry`
- the first active authority slice now persists paper-compliance, implementation-drag, route-shielding, retaliation-risk, administrative suppression-window, escalation-band, seizure-risk, and diversion-band fields inside those two owned modules only
- no standalone `BlackRoute` module key or save namespace should be introduced; future black-route migrations must stay inside the `OrderAndBanditry` and `TradeAndIndustry` module envelopes
