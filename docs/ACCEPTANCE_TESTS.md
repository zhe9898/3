# ACCEPTANCE_TESTS

## Global acceptance rules
Every release line must pass:
- deterministic replay tests
- save roundtrip tests
- invariant tests
- feature-manifest compatibility tests
- module boundary tests where practical
- scheduler cadence tests proving three deterministic `xun` pulses can run before month-end consolidation without changing module ownership rules
- player-facing cadence tests or reviews proving xun pulses remain internal by default: the shell exposes one normal monthly review / command window, summarizes xun motion as trends or traces, and reserves interrupt windows for urgent irreversible thresholds only
- immersion-protection review for player-facing shell changes: no slider-wall main interaction, no profession-label identity model, no xun report spam, object-anchored pressure, and delayed receipt/residue feedback instead of bare score bumps
- imperial-pressure presentation review: imperial pressure appears through object / institution carriers such as edicts, postings, yamen documents, tax / corvee writs, mourning markers, amnesty proclamations, appointment notices, and border dispatches; no player-facing emperor button, edict editor, or unearned court-control surface
- history-calibration review: Renzong-era data may seed initial institutional pressure, named carriers, and world tone, but no fixed-date historical rail may bypass deterministic rule resolution or player-earned counterfactual outcomes

## Phase M0
- 12-month empty/minimal world replay equality
- save root manifest validation
- module registration order deterministic
- module cadence declarations are explicit and scheduler-visible rather than hidden inside private module logic

## Phase M1
- no impossible family states
- household membership invariants hold
- commoner pressure can rise/fall deterministically
- grudges can persist across multiple years

## Phase M2
- six-module replay remains deterministic across repeated seeds
- 60-month replay remains deterministic across multiple representative seeds
- headless diagnostics harness can run 120 months and report bounded notification growth plus save payload sizes
- diagnostics harness and debug shell expose aligned runtime metrics for diff entries, domain events, notifications, and save payload bytes
- M2 save roundtrip preserves all enabled module namespaces including `NarrativeProjection`
- M2 and later save roundtrip preserves the `PublicLifeAndRumor` namespace whenever the active manifest enables the county-public-life slice
- legacy M0-M1 saves load through the M2 loader when newer modules remain disabled
- explicit root schema mismatches are rejected at load time
- explicit module schema mismatches are rejected at load time
- exam outcomes explainable
- trade outcomes explainable
- disabled M2-lite modules stay absent from save output
- Renzong chain-1 thin slice must prove real scheduler drain from `TaxSeasonOpened` to `HouseholdDebtSpiked`, `YamenOverloaded`, and public-life heat; this acceptance does not imply the full tax/corvee society chain is complete
- Renzong chain-2 thin slice must prove real scheduler drain from harvest phase to `GrainPriceSpike` and local `HouseholdSubsistencePressureChanged`, including an off-scope settlement negative assertion; this acceptance does not imply the full famine/market/route/memory chain is complete
- Renzong chain-5 thin slice must prove real scheduler drain from settlement-scoped `FrontierStrainEscalated` to matching-jurisdiction `OfficialSupplyRequisition` and household burden, including an off-scope jurisdiction/household negative assertion, `HouseholdBurdenIncreased` receipt emission, and repeated-frontier declaration suppression; this acceptance does not imply the full frontier/war economy chain is complete
- Renzong chain-6 thin slice must prove real scheduler drain from `DisasterDeclared` to `DisorderSpike` and public-life heat, including an off-scope settlement negative assertion, metadata-only rule handling, and repeated-disaster declaration suppression; this acceptance does not imply the full disaster-relief / market / migration / memory chain is complete
- Renzong chain-7 thin slice must prove real scheduler drain from `ClerkCaptureDeepened` to scoped public-life heat, including an off-scope settlement negative assertion and repeated-clerk-capture declaration suppression; this acceptance does not imply the full official-clerk-execution chain is complete
- Renzong chain-8 thin slice must prove real scheduler handling of `CourtAgendaPressureAccumulated` into exactly one allocated `PolicyWindowOpened` when multiple jurisdictions exist; this acceptance does not imply the full court-agenda / policy-dispatch chain is complete
- Renzong chain-9 thin slice must prove real scheduler handling of `RegimeLegitimacyShifted` into exactly one highest-risk `OfficeDefected` receipt after office-owned state mutation, while lower-risk appointed officials remain untouched; this acceptance does not imply the full regime-recognition / compliance chain is complete
- explicit default MVP bootstraps/loaders keep `PublicLifeAndRumor`, `OrderAndBanditry`, `ConflictAndForce`, `OfficeAndCareer`, and `WarfareCampaign` disabled unless a non-MVP path is explicitly selected
- explicit default MVP bootstraps remain deterministic across a 20-year headless run
- MVP preview artifacts can also stretch the default bootstrap across a 10-year family-lifecycle run while keeping hall, family council, and notification guidance aligned on the same next family action
- notifications trace back to `WorldDiff` entries
- notification history retention stays bounded, preserves the latest notice per source module, and trims the remaining oldest notices first
- runtime-only debug traces reset after save/load and do not affect compatibility
- first-pass shell composes from read-model bundles only
- presentation bundle surfaces runtime-only `PersonDossiers` from identity, family, and optional social-memory queries; registry-only persons still appear, missing optional family/social-memory data falls back without failure, and shell adapters read those dossiers from the bundle rather than module state
- lineage/person-inspector ViewModels can expose one focused person from those dossiers as a portrait-scroll / kinship-thread read surface without new authority rules, module reads, or save compatibility impact
- transient shell selection may request a focused person by id, but the lineage shell must choose only from existing `PersonDossiers` and fall back without authority reads or mutations
- presentation bundle surfaces household social pressure and player influence footprint as read-only projections; stable M2 can distinguish the player's anchor household local agency from observed household pressure while disabled office, order, and force paths remain absent or watch-only
- great hall and desk-sandbox public-life summaries compose from `PublicLifeAndRumor` read models only and remain read-only
- venue-channel public-life summaries compose from `PublicLifeAndRumor` read models only and remain read-only
- public-life surfaces can show how榜文、街谈、路报、州牒 differ without UI inventing new authority logic or private state
- player-command affordances and receipts also compose from read-model bundles only
- bounded public-life affordances / receipts such as `张榜晓谕`, `遣吏催报`, `催护一路`, `添雇巡丁`, `严缉路匪`, `遣人议路`, `暂缓穷追`, and `请族老出面` appear on settlement nodes only when their owning modules project them; UI still does not resolve authority rules
- stable M2 and later paths may surface family-council command affordances and receipts from `FamilyCore` read models only
- family-council surfaces remain read-only and must not resolve lineage-conflict authority inside UI code
- debug panel exposes seed, enabled modules, recent traces, inspectors, and invariant/warning output without authority writes
- debug panel can surface bootstrap vs save-load origin plus runtime-only migration summaries without changing authority state
- debug panel can surface runtime-only scale summaries and top payload modules without changing authority state
- debug panel can surface runtime-only pressure distribution and payload-summary headlines without changing authority state
- debug panel can regroup the same runtime-only diagnostics into `Scale`, `Pressure`, `Hotspots`, `Migration`, and `Warnings` sections without changing authority state
- presentation projects do not reference authority modules directly
- notifications trace back to diffs
- UI shell can display all required surfaces without authority leakage
- player-facing shell and notification wording avoid modern dashboard/workflow jargon on hall / desk / office / warfare surfaces, while system/debug/migration wording remains modern and engineering-clear
- player-facing authoritative diff/event summaries avoid leftover English dashboard/workflow phrasing at source and do not depend on shell-only normalization to become setting-appropriate
  - county-public-life summaries can surface street talk, county-gate crowding, market bustle, road-report lag, and prefecture pressure without introducing authority UI
  - county-public-life summaries can also surface monthly cadence and crowd mix on hall / desk nodes without introducing authority UI
  - county-public-life summaries can also surface official-notice, street-talk, road-report, prefecture-dispatch, and contention wording on hall / desk nodes without introducing authority UI
  - `PublicLifeAndRumor` xun passes may now distinguish hot and calm yamen surfaces through office task-load / clerk-dependence queries without emitting xun diffs/events or writing office state
  - legacy `PublicLifeAndRumor` schema `1 -> 2 -> 3 -> 4` saves migrate through the default loaders and backfill cadence, venue-channel, plus channel-contention descriptors conservatively enough to keep current M2+ paths loadable
- legacy `OrderAndBanditry` schema `1 -> 2 -> 3 -> 4 -> 5 -> 6` saves migrate through the default loaders and backfill black-route pressure, paper reach, shielding / retaliation, empty intervention-receipt fields, plus clamped one-month intervention-follow-through state conservatively enough to keep current M3+ paths loadable
- legacy `WorldSettlements` schema `1` saves migrate through schema `7` through the default loaders, backfilling settlement tiers and the chain-6 flood-disaster declaration watermark conservatively enough to continue load on current M2+ paths

## Phase M3
- active M2 bootstraps remain isolated from `OrderAndBanditry` and `ConflictAndForce`
- order-enabled M3 bridge saves/load keep `OrderAndBanditry` enabled while `ConflictAndForce` remains disabled
- conflict-enabled M3 lite runs can complete 240-month multi-seed diagnostics sweeps without runtime metrics escaping into save compatibility
- order-only stress and local-conflict stress bootstraps preserve settlement parity while keeping module activation surfaces distinct
- `OrderAndBanditry.Lite` produces traceable disorder/route-pressure diffs and notifications
- `OrderAndBanditry.Lite` also exposes black-route pressure, coercion risk, paper compliance, implementation drag, route shielding, retaliation risk, suppression-window, escalation-band, and pressure-trace queries without creating a new module key
- `TradeAndIndustry.Lite` also exposes gray-route / illicit-ledger summaries such as shadow-price, diversion share, blocked shipments, seizure risk, and diversion-band labels through its own namespace only
- `TradeAndIndustry.Lite` also mirrors settlement-level blockage / seizure pressure onto active-route summaries so route queries can report per-route blockage without moving ownership out of trade
- `TradeAndIndustry.Lite` distinguishes protective route shielding from backlash retaliation when reading order-owned pressure snapshots
- recent bounded order interventions can echo into the next monthly pass through order-owned carryover state, and trade may only react by reading the corresponding order queries
- bounded public-life order interventions may also scale against office-owned jurisdiction reach when governance-lite is enabled, while order-only paths remain neutral
- those same public-life order affordances may also surface a read-only office-aware execution summary without introducing UI-owned authority logic
- governance-lite office runs may also convert recent order-intervention carryover into office-owned backlog / petition / task-load fallout on the next month, but only by reading order queries and without writing order state back
- those same public-life order receipts may also surface a read-only office-aftermath execution summary when next-month jurisdiction traces still carry that order command’s follow-through
- runtime-only interaction-pressure and hotspot summaries may also surface that same order-linked office aftermath as read-only administrative-task / backlog context without entering save compatibility
- the application-layer presentation bundle may also surface a read-only settlement governance-lane summary that joins public-life, order, and office fallout for the same settlement without introducing module-owned synthetic state
- that same governance-lane summary may now also surface a read-only xun-facing public-momentum summary so hall / desk projections can see whether a county gate is tightening or still has room to buffer, without introducing module-owned cadence state
- that same governance-lane summary may also surface one read-only next-step public-life prompt, but only by selecting from the existing projected affordances for that settlement rather than inventing a second command lane
- the application-layer presentation bundle may also derive one read-only lead governance focus from those governance lanes so future hall surfaces can consume a single monthly governance docket without re-sorting in UI
- `ConflictAndForce.Lite` produces traceable readiness/conflict diffs and conflict-vignette notifications
- `ConflictAndForce.Lite` can synchronize activated same-month force support into `OrderAndBanditry.Lite` without direct mutation
- calm `ConflictAndForce.Lite` posture does not leak support into `OrderAndBanditry.Lite`
- standing-but-untriggered `ConflictAndForce.Lite` posture does not leak escort or militia relief into `OrderAndBanditry.Lite`
- runtime-only diagnostics can report per-module local-conflict activity peaks without entering save compatibility
- runtime-only diagnostics can report multi-settlement local-conflict interaction pressure peaks without entering save compatibility
- runtime-only diagnostics can report suppression-demand and pressure-distribution summaries without entering save compatibility
- runtime-only diagnostics can identify named hotspot settlements in large local-conflict stress runs without entering save compatibility
- runtime-only diagnostics budgets can constrain interaction-pressure counts and hotspot-score ceilings during long stress sweeps
- runtime-only diagnostics can report scale summaries and top payload modules during long stress sweeps without entering save compatibility
- runtime-only diagnostics can also report order-intervention carryover counts plus shielding-vs-backlash dominance without entering save compatibility
- runtime-only hotspot summaries can also surface order-owned carryover / shielding / retaliation fields without becoming authority state
- route insecurity affects trade through `OrderAndBanditry` queries or events, not direct mutation
- `ConflictAndForce.Lite` affects `OrderAndBanditry` through queries or events, not direct mutation
- local conflict resolution deterministic
- legacy M3 local-conflict saves can migrate `ConflictAndForce` schema `1` to `2` to `3` through the default local-conflict loader
- migrated legacy M3 stress saves continue deterministic replay against the current-schema local-conflict loader
- migrated local-conflict loads can surface runtime-only migration steps and current hotspots through the read-only debug shell
- migration reports preserve enabled-module and module-envelope key sets unless an explicit migration says otherwise
- migration preparation leaves source save data unchanged while reporting consistency status on the prepared copy
- legacy `OrderAndBanditry` schema `1` saves migrate to schema `2`, then `3`, then `4`, then `5`, then `6`, backfilling black-route pressure first, paper-compliance / implementation-drag fields second, route-shielding / retaliation-risk fields third, intervention receipts fourth, and intervention-follow-through bounds fifth inside the same envelope only
- legacy `TradeAndIndustry` schema `1` saves migrate to schema `2` and then `3`, backfilling gray-route ledgers first and route-level blockage / seizure mirrors second inside the same envelope only
- campaign aftermath can push deterministic owned-state fatigue / escort-strain fallout into `ConflictAndForce` without cross-module writes
- `ConflictAndForce` campaign-fatigue fallout must reduce only conflict-owned readiness / command / escort posture and recover through later monthly passes
- post-MVP black-route migration tests stay inside `OrderAndBanditry` and `TradeAndIndustry` module envelopes and do not create a standalone module key
- stable M2/M3 bootstraps remain isolated from `OfficeAndCareer` unless the governance-lite path is selected
- legacy `FamilyCore` saves migrate through current schema `7` through the default loaders without changing enabled-module or module-envelope key sets
- family command handling mutates only `FamilyCore` directly; `SocialMemoryAndRelations` may react later through family queries, not same-command cross-module writes
- family lifecycle commands surface in family read models and hall / council projections without adding any authority logic to UI
- autonomous marriage resolution must bind concrete spouse links before births can occur; newly arranged marriages do not also produce a same-month birth
- birth registration must preserve parent links / parent child lists and increase clan-owned care burden
- death registration must preserve cause-specific death events while increasing clan-owned funeral debt
- death aftermath must distinguish child death, ordinary adult / elder death, and current-heir death; a current-heir death with no adult successor must create heavier inheritance, branch, separation, and marriage pressure than one with an adult successor
- external violent / warlike deaths targeting a clan `PersonId` must enter the same `FamilyCore` death-pressure profile without FamilyCore re-emitting a duplicate cause-specific death event; adult-successor and no-adult-successor heir deaths must still produce different pressure bands
- a current-heir death with no adult successor must produce an end-to-end readable loop: family notice / ancestral-hall guidance points to `议定承祧`, the bounded command writes a receipt, the deceased heir is not re-selected, and the following month still exposes readable lifecycle state
- death aftermath pressure must project into notice and ancestral-hall guidance so adult-successor deaths point first toward `议定丧次` / stabilizing the new承祧, while no-adult-successor or only-young-heir gaps point first toward `议定承祧` and branch containment; this remains guidance only, not a full funeral or inheritance system
- bounded lifecycle commands such as `议亲定婚`, `议定承祧`, `拨粮护婴`, and `议定丧次` must resolve through deterministic pressure profiles rather than fixed deltas, reading only `FamilyCore`-owned lifecycle state and surfacing the relevant pressure bands in receipts
- house-branch conflict commands such as `偏护嫡支`, `责令赔礼`, `准其分房`, `停其接济`, `请族老调停`, and `请族老出面` must also resolve through deterministic pressure profiles rather than fixed deltas, reading only `FamilyCore`-owned conflict state and surfacing the relevant pressure bands in receipts
- shared command-resolution helpers may provide banding, profile-factor text, clamping, and delta adjustment, but domain ownership and command effects must remain in the owning module/application resolver rather than a universal decision engine
- family lifecycle events such as `议亲定婚`, `门内添丁`, `门内举哀`, and `承祧未稳` should project dedicated ancestral-hall-facing notice text rather than generic module fallback titles
- follow-up family commands such as `拨粮护婴` and `议定丧次` should remain bounded `FamilyCore` writes and surface as read-only receipts on hall / council projections
- family lifecycle notices should also carry concrete next-step guidance about襁褓护持, 口粮与乳哺, 丧次祭次, and承祧名分 when relevant traces indicate those pressures
- family death lifecycle notices should carry different next-step guidance when traces show `承祧缺口1阶` versus `承祧缺口3阶`, and the notification center should append the same projected lifecycle prompt used by the great hall
- great hall and family-council lifecycle summaries may also surface a read-only `眼下宜先...` prompt for the next bounded family command, but that prompt must be selected from projected affordances rather than inferred as UI-owned authority
- lead lifecycle notices in the great hall and notification center should align to that same projected `眼下宜先...` prompt whenever the top notice is a family-lifecycle notice, without moving any command-resolution logic into UI
- conflict aftermath affects owned modules only through events
- no tactical micro inputs exist

## Post-MVP packs
### Office pack
- appointment authority affects only office-owned state directly
- family/trade changes happen through events and handlers
- governance-lite saves/load preserve the `OfficeAndCareer` namespace without leaking it into stable M2/M3 manifests
- `OfficeAndCareer.Lite` treats local-exam success as a bounded queue into office, not an automatic direct appointment
- eligible governance-lite candidates can remain in waiting / attached-yamen states deterministically before formal appointment
- `OfficeAndCareer.Lite` can grant explainable appointments and jurisdiction leverage deterministically once queue pressure and backing cross threshold
- governance-lite office service can progress through bounded promotion/demotion pressure, administrative tasks, and petition outcomes deterministically
- governance-lite office service may also absorb recent order-command aftermath into office-owned task load, petition backlog, petition pressure, and leverage drift through query seams only
- legacy governance-lite office saves migrate from schema `1` through schema `6` without changing enabled-module or module-envelope key sets
- legacy governance-lite office saves reconstruct v2-only task/petition/service descriptors first, then backfill queue pressure, clerk dependence, amnesty de-duplication, office post / waiting-list state, clerk-capture watermarks, and official-defection risk conservatively enough to continue replay on the current schema path
- governance-lite jurisdiction queries expose clerk dependence and administrative task load so order/trade slices can distinguish paper orders from actual local reach without direct office-state writes
- future office-depth projections distinguish credential, actual post, clerk dependence, patron / family pull, evaluation pressure, and memorial attack risk rather than treating official rank as automatic authority
- court-facing office context, if shown before the imperial pack exists, remains watch-only appointment rumor / reform talk / censor-pressure / dispatch wording and does not resolve court decisions inside `OfficeAndCareer`
- `OrderAndBanditry` and `ConflictAndForce` may read office leverage only through queries, not direct mutation
- first-pass presentation may surface office appointments, task tiers, petition categories, petition backlog, and promotion/demotion pressure summaries only when governance-lite is enabled, and must remain office-empty on stable M2/M3 paths
- governance-lite presentation may surface bounded office command affordances and recent office command receipts only when governance-lite is enabled, and must remain command-empty on stable M2/M3 paths
- the application-layer presentation bundle may also expose one read-only governance docket that joins the selected governance focus with same-settlement notification context, while still keeping notification ordering downstream of authority state
- that same read-only governance docket may also expose one same-settlement recent handling receipt from the existing command projections, so hall surfaces can read what was just done without adding any new authority-owned command ledger
- that same read-only governance docket may also expose a derived current phase label/summary from governance pressure plus existing receipts/notifications, without introducing module-owned workflow state or UI-owned sorting logic
- first-pass presentation adapters may also thread governance-lane public-momentum summaries into existing great-hall and desk-settlement governance summaries, while keeping fallback behavior on projection-absent paths and without adding new shell-owned authority fields
- the application-layer presentation bundle may also expose one read-only `HallDocketStack` whose lead item and secondary items are derived from existing family, governance, and warfare projections, while keeping ordering logic out of UI and out of authority state
- first-pass presentation adapters may also let great-hall lead title/guidance prefer the read-only `HallDocketStack.LeadItem`, while notification center remains notification-driven and projection-only
- first-pass presentation adapters may also expose read-only `HallDocketStack.SecondaryItems` as great-hall secondary-matter summaries, while keeping notification ordering and authority untouched
- first-pass presentation adapters may also reflect same-settlement `HallDocketStack` items back onto desk settlement nodes as read-only hall-agenda summaries, without changing application ordering or inventing new authority
- those same desk-settlement adapters may also expose the matched same-settlement hall-docket items as thin read-only agenda rows, while keeping ordering, ownership, and shell object grammar out of shared contracts
- those same desk-settlement adapters may also expose read-only hall-agenda counts and distinct lane keys so UI can mark which local nodes are already on the hall docket without recomputing hall ordering
- those same desk-settlement adapters may also expose whether the current settlement is the monthly lead hall-docket node, plus the lead lane key when it is, without inferring lead status from secondary items
- each read-only `HallDocketStack` item may also expose neutral ordering/provenance fields such as ordering summary plus source projection/module keys, while keeping shell object grammar outside shared contracts

### Order/banditry pack
- outlaw/banditry state can be enabled without schema collisions
- route insecurity affects trade via queries/events, not direct mutation
- black-route preflight contracts keep pressure and ledger ownership split across `OrderAndBanditry` and `TradeAndIndustry`
- black-route authority state remains split across `OrderAndBanditry` pressure and `TradeAndIndustry` ledgers even after default migrations run

### Force pack
- force pool math deterministic
- command cap and supply effects reproducible
- local conflict logs readable

### Warfare pack
- campaign-enabled warfare-lite saves/load preserve the `WarfareCampaign` namespace without leaking it into stable M2/M3/governance-lite manifests
- `WarfareCampaign.Lite` can derive deterministic campaign boards and mobilization signals from `ConflictAndForce`, `WorldSettlements`, and `OfficeAndCareer` through queries only
- `WarfareCampaign.Lite` presentation remains read-only and campaign-level
- legacy campaign-enabled schema `1` saves migrate through schema `2` into schema `3` without replay drift
- campaign boards surface command-fit, commander summary, and bounded route descriptors without adding unit-micro authority
- thin application-routed warfare intent commands (`DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, `WithdrawToBarracks`) may update warfare-owned directive state only, without mutating upstream module state
- a unified player-command service may route bounded office and warfare intents through application services only; disabled module paths must not leak their commands into the shell
- deterministic event-handling runs before `NarrativeProjection`, so projection can see warfare source events plus handler-emitted downstream follow-ons in the same month
- warfare events carry settlement-targeting metadata without extending save compatibility
- warfare read models surface directive label/summary/trace and Chinese-ancient desk-sandbox wording without moving authority rules into UI
- warfare read models also vary board-environment / board-surface / marker / atmosphere descriptors from campaign conditions so the campaign sandbox changes with front, supply, morale, directive, and route posture
- warfare read models also vary regional-profile / backdrop descriptors from existing settlement and route projections so different local environments do not present as one static battlefield shell
- campaign outcomes reproducible from seed + inputs
- war overlay remains campaign-level, no unit-micro authority
- campaign aftermath updates other modules only via events
- campaign aftermath can push deterministic owned-state consequences into `TradeAndIndustry`, `OrderAndBanditry`, `OfficeAndCareer`, and `SocialMemoryAndRelations` without cross-module writes
- campaign aftermath can also push deterministic owned-state fatigue / readiness fallout into `ConflictAndForce` without writing back into `WarfareCampaign`
- campaign aftermath can also push deterministic livelihood pressure into `PopulationAndHouseholds`, prosperity/security scars into `WorldSettlements`, and clan-standing fallout into `FamilyCore` without cross-module writes
- warfare-aftercare notifications can pull same-settlement fallout traces into `NarrativeProjection` so honors, blame, relief, and route-cleanup context remain explainable without changing authority ownership
- read-only hall / desk / campaign-board surfaces may summarize aftermath dockets from warfare plus downstream fallout projections only; they must not add authority UI or synthetic save state
- read-only office / warfare surfaces may show command affordances and recent receipts, but those surfaces must still not resolve authority inside UI code

### Imperial / dynasty-cycle pack
- imperial or court-facing state, if introduced, has an explicit owner such as a future `CourtAndThrone` / `WorldEvents` pack and does not live in UI or `NarrativeProjection`
- imperial rhythm such as accession, mourning, amnesty, succession uncertainty, mandate confidence, or court-time disruption affects local modules only through queries / commands / domain events
- imperial pressure remains mediated through office, public-life, world, warfare, market, household, memory, and projection surfaces until a court-facing pack explicitly grants higher influence reach
- court-process state, if introduced, models memorial queues, audience or council attention, agenda pressure, censor pressure, appointment slates, policy windows, and dispatch targets as owned state or projections under the court pack, not as UI cutscenes
- court-process outputs affect local modules only through appointments, policies, dispatches, rhythm bands, commands, queries, and domain events; they may not directly rewrite household, market, order, or settlement state
- regime-authority state, if introduced, models recognition, appointment reach, tax reach, grain-route reach, force backing, ritual claim, public belief, office defection, and local compliance rather than a single dynasty-name flag
- rebellion-to-polity escalation grows from protection failure, armed autonomy, force backing, grain-route control, office defection, public legitimacy, and memory pressure rather than a one-step event
- succession struggle, usurpation, restoration, and dynasty repair remain deterministic from seed + state + player commands
- player regime-scale agency resolves through bounded leverage: force, grain, logistics, office access, faction memory, public legitimacy, information reach, and succession conditions
- historical counterfactuals expose cause traces and residue; no timeline-editor command or global year-trigger stat rewrite is accepted
- imperial / dynasty-cycle pack saves/load preserve module-envelope ownership and can be disabled without corrupting lower-scope save paths

## Boundary tests
At integration level, verify:
- no module writes foreign namespace
- projections remain read-only
- disabled feature packs load clean defaults
