# ACCEPTANCE_TESTS

## Global acceptance rules
Every release line must pass:
- deterministic replay tests
- save roundtrip tests
- invariant tests
- feature-manifest compatibility tests
- module boundary tests where practical
- scheduler cadence tests proving deterministic day-level authority steps can run before month-end consolidation without changing module ownership rules, including safe batching/skipping of quiet spans when implemented
- player-facing cadence tests or reviews proving day-level motion remains internal by default: the shell exposes one normal monthly review / command window, summarizes day movement as trends, traces, urgency, or xun-labeled calendar bands, and reserves interrupt windows for urgent irreversible thresholds only
- playable-loop acceptance for any gameplay slice: visible pressure leads to readable leverage, a bounded command, module-owned resolution, receipt/refusal/residue, and a changed next-month read; read models alone are not enough
- perspective acceptance for player-facing shell changes: the player reads and acts from a home-household seat, while people are emotional/tactical entry points; the shell must not imply fixed-person RPG identity or clan-god control
- relationship-chain acceptance for expanded player power: clan, locality, office, force, or court-facing leverage must remain mediated by concrete kin, debt, document, favor, office, public-face, or force-carrier chains rather than free global control
- immersion-protection review for player-facing shell changes: no slider-wall main interaction, no profession-label identity model, no daily/xun report spam, object-anchored pressure, and delayed receipt/residue feedback instead of bare score bumps
- imperial-pressure presentation review: imperial pressure appears through object / institution carriers such as edicts, postings, yamen documents, tax / corvee writs, mourning markers, amnesty proclamations, appointment notices, and border dispatches; no player-facing emperor button, edict editor, or unearned court-control surface
- history-calibration review: Renzong-era data may seed initial institutional pressure, named carriers, and world tone, but no fixed-date historical rail may bypass deterministic rule resolution or player-earned counterfactual outcomes
- pressure-chain topology review: any Renzong pressure chain marked implemented must appear in `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` with locus, cadence, repetition guard, receipt/projection, proof test, and remaining full-chain debt

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
- Renzong chain-1 thin slice must prove real scheduler drain from `TaxSeasonOpened` to `HouseholdDebtSpiked`, `YamenOverloaded`, and public-life heat; focused handler tests must also prove the household tax burden profile uses multiple existing household dimensions, carries structured cause / source / settlement / debt / profile metadata, protects an off-scope settlement when a numeric settlement scope is supplied, and preserves the current symbolic global thin signal until `WorldSettlements` emits precise local tax events. This acceptance does not imply the full tax/corvee society chain is complete.
- Renzong chain-2 thin slice must prove real scheduler drain from harvest phase to `GrainPriceSpike` and local `HouseholdSubsistencePressureChanged`, including an off-scope settlement negative assertion; focused handler tests must also prove grain-price pressure uses multiple existing household dimensions, carries structured grain-market and household subsistence-profile metadata, and still keeps off-scope households untouched. This acceptance does not imply the full famine/market/route/memory chain is complete
- Renzong chain-3 thin slice must prove real scheduler drain from `ExamPassed` to `ClanPrestigeAdjusted`; focused handler tests must also prove exam prestige uses credential metadata plus family-owned clan/person state, carries structured exam-prestige metadata, and leaves off-scope clans untouched. This acceptance does not imply the full education / office waiting list / public-life exam projection / favor-shame memory chain is complete
- Renzong chain-5 thin slice must prove real scheduler drain from settlement-scoped `FrontierStrainEscalated` to matching-jurisdiction `OfficialSupplyRequisition` and household burden, including an off-scope jurisdiction/household negative assertion, office-side supply execution metadata, household-owned burden-profile metadata, `HouseholdBurdenIncreased` receipt emission, and repeated-frontier declaration suppression; this acceptance does not imply the full frontier/war economy chain is complete
- Renzong chain-4 thin slice must prove real scheduler drain from `ImperialRhythmChanged` through `AmnestyApplied` into `DisorderSpike`, structured office execution metadata on `AmnestyApplied`, order-owned amnesty-disorder profile metadata on `DisorderSpike`, off-scope settlement protection, and no Summary-driven amnesty rule; this acceptance does not imply the full mourning / succession / appointment rhythm / public legitimacy chain is complete
- Renzong chain-6 thin slice must prove real scheduler drain from `DisasterDeclared` to `DisorderSpike` and public-life heat, including an off-scope settlement negative assertion, metadata-only rule handling, order-owned disaster-disorder profile metadata, suppression-buffer absorption of moderate disasters, and repeated-disaster declaration suppression; this acceptance does not imply the full disaster-relief / market / migration / memory chain is complete
- Renzong chain-7 thin slice must prove real scheduler drain from `ClerkCaptureDeepened` to scoped public-life heat, including an off-scope settlement negative assertion, structured clerk-capture profile metadata, profile-scaled public heat, and repeated-clerk-capture declaration suppression; this acceptance does not imply the full official-clerk-execution chain is complete
- Renzong chain-8 thin slice must prove real scheduler handling of `CourtAgendaPressureAccumulated` into exactly one allocated `PolicyWindowOpened` when multiple jurisdictions exist, with structured policy-window profile metadata and a negative case where local clerk/backlog/task drag absorbs the court signal below threshold; this acceptance does not imply the full court-agenda / policy-dispatch chain is complete
- Renzong chain-9 thin slice must prove real scheduler handling of `RegimeLegitimacyShifted` into exactly one highest-risk `OfficeDefected` receipt after office-owned state mutation, while lower-risk appointed officials remain untouched; focused handler coverage must assert structured defection profile metadata and a buffered-official negative case; this acceptance does not imply the full regime-recognition / compliance chain is complete
- pressure-tempering kernel acceptance must prove `SocialMemoryAndRelations` combines multiple existing pressure dimensions into clan climate, shapes adult person tempering through `FamilyCore` personality traits, handles scoped trade/exam/death/family events without touching off-scope clans, publishes terminal `EmotionalPressureShifted` / `PressureTempered` receipts, migrates legacy social-memory schema `2 -> 3`, and receives at least one upstream event through the real monthly scheduler drain
- explicit default MVP bootstraps/loaders keep `PublicLifeAndRumor`, `OrderAndBanditry`, `ConflictAndForce`, `OfficeAndCareer`, and `WarfareCampaign` disabled unless a non-MVP path is explicitly selected
- explicit default MVP bootstraps remain deterministic across a 20-year headless run
- MVP preview artifacts can also stretch the default bootstrap across a 10-year family-lifecycle run while keeping hall, family council, and notification guidance aligned on the same next family action
- notifications trace back to `WorldDiff` entries
- notification history retention stays bounded, preserves the latest notice per source module, and trims the remaining oldest notices first
- runtime-only debug traces reset after save/load and do not affect compatibility
- first-pass shell composes from read-model bundles only
- presentation bundle surfaces runtime-only `PersonDossiers` from identity, family, household, education, clan-trade, office, and optional social-memory queries; registry-only persons still appear, missing optional module data falls back without failure, and shell adapters read those dossiers from the bundle rather than module state
- lineage/person-inspector ViewModels can expose one focused person from those dossiers as a portrait-scroll / kinship-thread read surface without new authority rules, module reads, or save compatibility impact
- transient shell selection may request a focused person by id, but the lineage shell must choose only from existing `PersonDossiers` and fall back without authority reads or mutations
- presentation bundle surfaces household social pressure and player influence footprint as read-only projections; stable M2 can distinguish the player's anchor household local agency from observed household pressure while disabled office, order, and force paths remain absent or watch-only
- great hall and desk-sandbox public-life summaries compose from `PublicLifeAndRumor` read models only and remain read-only
- venue-channel public-life summaries compose from `PublicLifeAndRumor` read models only and remain read-only
- public-life surfaces can show how榜文、街谈、路报、州牒 differ without UI inventing new authority logic or private state
- player-command affordances and receipts also compose from read-model bundles only
- player-command execution routes through the shared module command seam: Application selects the owning module and disabled-path rejection, while command formulas and authoritative mutation live in module resolvers
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
  - `PublicLifeAndRumor` day-facing short-band passes may distinguish hot and calm yamen surfaces through office task-load / clerk-dependence queries without emitting short-band diffs/events or writing office state
  - legacy `PublicLifeAndRumor` schema `1 -> 2 -> 3 -> 4` saves migrate through the default loaders and backfill cadence, venue-channel, plus channel-contention descriptors conservatively enough to keep current M2+ paths loadable
- legacy `OrderAndBanditry` schema `1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8 -> 9` saves migrate through the default loaders and backfill black-route pressure, paper reach, shielding / retaliation, empty intervention-receipt fields, clamped one-month intervention-follow-through state, structured public-life order outcome/refusal/partial trace fields, and public-life refusal response trace fields conservatively enough to keep current M3+ paths loadable
- legacy `PopulationAndHouseholds` schema `2 -> 3` saves migrate through the default loaders and backfill home-household local response trace fields conservatively enough to keep current M2+ paths loadable
- legacy `WorldSettlements` schema `1` saves migrate through schema `7` through the default loaders, backfilling settlement tiers and the chain-6 flood-disaster declaration watermark conservatively enough to continue load on current M2+ paths
- legacy `SocialMemoryAndRelations` schema `2` saves migrate to schema `3`, backfilling clan emotional climates from existing narratives and keeping `GameDate` fields valid for save/load roundtrip

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
- public-life order closure must prove a full playable-thin loop: Month N public-life/order pressure appears on a settlement surface, the player issues one bounded order command, `OrderAndBanditry` resolves accepted/partial/refused outcome and receipt state, and Month N+1 governance/order readback changes without UI authority or cross-module mutation
- public-life order social-memory residue must prove Month N `添雇巡丁` or `严缉路匪` mutates only order-owned command/carryover/refusal-trace state, Month N+1 `SocialMemoryAndRelations` reads structured order aftermath and writes only social-memory memory/narrative/climate state, and the durable residue appears in read models and shell readback
- governance-lite office runs may also convert recent order-intervention carryover into office-owned backlog / petition / task-load fallout on the next month, but only by reading order queries and without writing order state back
- those same public-life order receipts may also surface a read-only office-aftermath execution summary when next-month jurisdiction traces still carry that order command’s follow-through
- runtime-only interaction-pressure and hotspot summaries may also surface that same order-linked office aftermath as read-only administrative-task / backlog context without entering save compatibility
- the application-layer presentation bundle may also surface a read-only settlement governance-lane summary that joins public-life, order, and office fallout for the same settlement without introducing module-owned synthetic state
- that same governance-lane summary may now also surface a read-only day-facing public-momentum summary so hall / desk projections can see whether a county gate is tightening or still has room to buffer, without introducing module-owned cadence state
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
- legacy `OrderAndBanditry` schema `1` saves migrate to schema `2`, then `3`, then `4`, then `5`, then `6`, then `7`, then `8`, then `9`, backfilling black-route pressure first, paper-compliance / implementation-drag fields second, route-shielding / retaliation-risk fields third, intervention receipts fourth, intervention-follow-through bounds fifth, structured outcome/refusal/partial trace fields sixth, and refusal response trace fields seventh inside the same envelope only
- legacy `TradeAndIndustry` schema `1` saves migrate to schema `2` and then `3`, backfilling gray-route ledgers first and route-level blockage / seizure mirrors second inside the same envelope only
- campaign aftermath can push deterministic owned-state fatigue / escort-strain fallout into `ConflictAndForce` without cross-module writes
- `ConflictAndForce` campaign-fatigue fallout must reduce only conflict-owned readiness / command / escort posture and recover through later monthly passes
- post-MVP black-route migration tests stay inside `OrderAndBanditry` and `TradeAndIndustry` module envelopes and do not create a standalone module key
- stable M2/M3 bootstraps remain isolated from `OfficeAndCareer` unless the governance-lite path is selected
- legacy `FamilyCore` saves migrate through current schema `8` through the default loaders without changing enabled-module or module-envelope key sets
- family command handling resolves through `FamilyCoreCommandResolver` and mutates only `FamilyCore` directly; `SocialMemoryAndRelations` may be read for deterministic pressure-tempering friction and may react later through family queries, not same-command cross-module writes
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
- missing `SocialMemoryAndRelations` query access must remain neutral for family command resolution, while high volatility / bitterness should make reconciliation weaker and trust / restraint should strengthen apology or mediation
- shared command-resolution helpers may provide banding, profile-factor text, clamping, and delta adjustment, but domain ownership and command effects must remain in the owning module resolver rather than Application or a universal decision engine
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
- legacy governance-lite office saves migrate from schema `1` through schema `7` without changing enabled-module or module-envelope key sets
- legacy governance-lite office saves reconstruct v2-only task/petition/service descriptors first, then backfill queue pressure, clerk dependence, amnesty de-duplication, office post / waiting-list state, clerk-capture watermarks, official-defection risk, and public-life refusal response trace fields conservatively enough to continue replay on the current schema path
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
- warfare intent commands (`DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, `WithdrawToBarracks`) resolve through `WarfareCampaignCommandResolver` and may update warfare-owned directive state only, without mutating upstream module state
- a unified player-command service may dispatch bounded family, office, order, and warfare intents to their owning module command handlers only; disabled module paths must not leak their commands into the shell
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

## Playable closure v2 acceptance - 2026-04-24
- `chain1-public-life-order-v2` must prove Month N public-life/order pressure is visible, a bounded public-life order command routes to `OrderAndBanditry`, the owner module accepts or refuses, and Month N+1 governance/order readback changes through projections only.
- Acceptance must include a disabled-module fallback: when `OrderAndBanditry` is not enabled, public-life order commands refuse safely, produce no order receipt, and do not add an order save envelope.
- Presentation acceptance must include projection-absent fallback: desk/great-hall public-life surfaces remain stable and expose no synthesized order affordances or receipts.
- Architecture acceptance must guard against WorldManager/PersonManager/CharacterManager/god-controller drift, UI authority drift, application/presentation mutation paths inside modules, and `PersonRegistry` expansion beyond identity fields.
- Save/schema result for this v2 chain: no save/schema impact.

## Playable closure v3 leverage acceptance - 2026-04-24
- `chain1-public-life-order-leverage-v3` must prove Month N public-life/order pressure exposes a visible leverage explanation before command issue, the bounded order command still resolves in `OrderAndBanditry`, and Month N+1 governance/order readback carries the leverage/cost/residue explanation forward.
- Integration acceptance must show affordance leverage, receipt cost, and next-month governance docket readback for an accepted public-life order command without adding a new command, event, save field, or cross-module write path.
- Presentation acceptance must show Unity shell adapters copy projected leverage/cost/readback fields and fall back safely when those projections are absent; Unity must not compute authority formulas.
- Relationship acceptance for this pass is projection-only: durable favor, shame, debt, fear, grudge, or obligation state remains future `SocialMemoryAndRelations` work unless a later task explicitly adds owner-state schema and migration.
- Save/schema result for this v3 chain: no persisted state impact; the only schema documentation change is runtime read-model shape.

## Playable closure v4 social-memory residue acceptance - 2026-04-25
- `public-life-order-social-memory-residue-v4` must prove Month N public-life/order command resolution remains inside `OrderAndBanditry`, while Month N+1 durable obligation, favor, shame, fear, or grudge residue is persisted only by `SocialMemoryAndRelations`.
- Integration acceptance must show the full rule-driven loop: accepted `添雇巡丁` or `严缉路匪`, structured order aftermath query, SocialMemory-owned memory/climate/narrative residue, public-life receipt readback, governance readback, and shell visibility.
- Save acceptance must show the new residue entries roundtrip through the existing SocialMemory schema `3`; because no new persisted field is added, no `3 -> 4` migration is expected for this pass.
- Architecture acceptance must guard against `DomainEvent.Summary` parsing, Application/UI/Unity social-memory writes, manager/god-controller drift, and `PersonRegistry` expansion.

## Playable closure v5 refusal-residue acceptance - 2026-04-25
- `public-life-order-refusal-residue-v5` must prove this is a rule-driven command / aftermath / social-memory readback loop, not an event-centered or event-pool design.
- Order acceptance must cover accepted, partial, and refused `添雇巡丁` / `严缉路匪` paths and prove command time mutates only `OrderAndBanditry` state.
- SocialMemory acceptance must prove same-month command resolution does not write SocialMemory, while Month N+1 refused or partial aftermath writes durable `Memories`, `ClanNarratives`, and `ClanEmotionalClimates` residue only inside `SocialMemoryAndRelations`.
- Read-model acceptance must prove public-life receipts, governance lanes/dockets, family-facing `SocialMemories`, and shell surfaces expose `县门未落地`, `地方拖延`, `后账仍在`, and `社会记忆读回` from projections only.
- Save acceptance must prove `OrderAndBanditry` schema `7 -> 8` migration and save/load preservation of structured refusal trace plus SocialMemory refusal residue; SocialMemory remains schema `3`.
- Architecture acceptance must guard against `LastInterventionSummary` / `DomainEvent.Summary` parsing, Application/UI/Unity social-memory writes, forbidden manager/god-controller names, and `PersonRegistry` expansion.

## Playable closure v6 refusal-response acceptance - 2026-04-25
- `public-life-order-refusal-response-v6` must prove this is a rule-driven command / residue / social-memory / response loop, not an event-centered or event-pool design.
- Read-model acceptance must prove v5 refusal / partial residue projects Month N+1 bounded response affordances for the public-life, governance, and family-facing surfaces.
- Command acceptance must prove `补保巡丁`, `赔脚户误读`, and `暂缓强压` mutate only `OrderAndBanditry`; `押文催县门` and `改走递报` mutate only `OfficeAndCareer`; `请族老解释` mutates only `FamilyCore` at command time.
- Same-month acceptance must prove response command handling does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance must prove Month N+2 reads structured response aftermath and adjusts only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`, without parsing `DomainEvent.Summary`, receipt text, or `LastInterventionSummary`.
- Outcome acceptance must cover at least two paths among `Repaired`, `Contained`, `Escalated`, and `Ignored`; focused tests should include both a repair/containment path and an escalation or ignored path.
- Projection acceptance must prove public-life receipts, governance lane / docket, family-facing readback, and shell fields expose whether the后账 was repaired, temporarily contained, worsened, or left aside, plus projected shame/fear/favor/grudge/obligation changes.
- Unity acceptance must prove shell adapters display projected response readback only and never query simulation modules or compute response outcome.
- Save acceptance must prove `OrderAndBanditry` `8 -> 9`, `OfficeAndCareer` `6 -> 7`, and `FamilyCore` `7 -> 8` migrations plus save/load preservation of response trace fields; `SocialMemoryAndRelations` remains schema `3`.
- Architecture acceptance must guard boundary drift, summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, and Application/UI/Unity writes to SocialMemory.

## Playable closure v7 residue-decay / repeat-friction acceptance - 2026-04-25
- `public-life-order-residue-decay-friction-v7` must prove the response afterlife remains a rule-driven command / residue / social-memory / response loop, not an event-centered or event-pool design.
- SocialMemory acceptance must prove response memories recorded in Month N+2 are skipped for same-month drift, then later soften or harden only through `SocialMemoryAndRelations` updates to existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- At least one repaired path must show declining response memory weight and projected `后账渐平` readback.
- At least one escalated or ignored path must show hardening response memory weight and projected `后账转硬` or equivalent hardening readback.
- Repeat-friction acceptance must prove later `OrderAndBanditry`, `OfficeAndCareer`, and/or `FamilyCore` commands read structured SocialMemory response cause keys and weights, mutate only their owning module at command time, and do not write SocialMemory.
- Projection acceptance must prove public-life / governance / family readback can expose the later social residue state from read models only.
- Save/schema acceptance: no new persisted fields, no schema bump, and no migration are expected for v7; it reuses SocialMemory schema `3` and existing v6 owner response trace fields.
- Architecture acceptance must guard against parsing social-memory summary prose, receipt prose, `LastRefusalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary` for drift or repeat friction.

## Playable closure v8 actor-countermove / passive back-pressure acceptance - 2026-04-25
- `public-life-order-actor-countermove-v8` must prove the response afterlife remains a rule-driven command / residue / social-memory / response loop, not an event-centered, event-pool, or autonomous-manager design.
- Actor-countermove acceptance must prove existing SocialMemory response residue can trigger a later monthly owner-module countermove without a new player command.
- Soft-path acceptance must cover a repaired or contained residue case such as `巡丁自补保`, proving `OrderAndBanditry` mutates only order-owned route pressure and existing response trace fields.
- Hard-path acceptance must cover an escalated or ignored residue case such as `胥吏续拖`, proving `OfficeAndCareer` mutates only office-owned clerk/docket pressure and existing response trace fields.
- Structured-read acceptance must prove actor countermoves read `SocialMemoryEntrySnapshot.CauseKey`, outcome marker, `Weight`, `State`, `SourceClanId`, and `OriginDate`, skip current-month memories, and do not parse summaries or receipt prose.
- Readback acceptance must prove public-life receipts, governance lanes, family-facing surfaces, and Unity projection adapters can show actor-countermove aftermath only from projected fields.
- Save/schema acceptance: no new persisted fields, no schema bump, and no migration are expected for v8; it reuses SocialMemory schema `3` and existing v6 owner response trace fields.

## Playable closure v9 actor-countermove readback hardening acceptance - 2026-04-25
- `public-life-order-actor-countermove-readback-v9` must prove all three owner modules have soft and hard actor-countermove coverage across v8/v9 without adding an actor manager, event pool, UI authority, or new persisted fields.
- Minimum playable response-loop acceptance must prove visible refusal/partial residue leads to at least three bounded response affordances with projected availability, leverage, cost, execution, and next-readback text; hidden command codes alone are insufficient.
- SocialMemory acceptance must prove actor traces are owner-state facts first: Order/Office traces are read on the following monthly pass, while Family traces may be read in the same scheduler pass through module order and response carryover, without parsing summaries or using UI timers.
- Presentation acceptance must prove Unity/shell readback copies projected actor-countermove receipts only, including order, office, and family labels/result text, and does not query modules or compute effectiveness.
- Save/schema acceptance: no schema bump, migration, or new persisted fields are expected for v9.

## Playable closure v10 ordinary-household readback acceptance - 2026-04-25
- `public-life-order-ordinary-household-readback-v10` must prove ordinary households are part of the same rule-driven command / residue / social-memory / response / readback loop, not a separate event-chain, event-pool, or household-control subsystem.
- Read-model acceptance must prove Month N refused / partial `添雇巡丁` or `严缉路匪` residue projects a Month N+1 `HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue` signal for affected ordinary households.
- Boundary acceptance must prove the projection reads structured order / office / family aftermath fields and does not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- Command acceptance remains unchanged: response affordances are still owned by `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`; ordinary households carry pressure readback and indirect leverage context, but they do not own a new command surface.
- Projection acceptance must show route fear, runner/watch misunderstanding, household labor/debt/migration strain, and yamen delay where projected, while leaving `PopulationAndHouseholds` state unmutated by read-model composition.
- Unity acceptance must prove Desk Sandbox settlement pressure displays the projected ordinary-household after-account only from `PresentationReadModelBundle.HouseholdSocialPressures`.
- Save/schema acceptance: v10 adds runtime read-model constants only and introduces no persisted fields, schema bump, migration, or save roundtrip requirement.

## Playable closure v11 ordinary-household play-surface acceptance - 2026-04-25
- `public-life-order-ordinary-household-play-surface-v11` must prove ordinary-household pressure becomes a costed response choice surface, while staying inside the same rule-driven command / residue / social-memory / response / readback loop.
- Read-model acceptance must prove Month N+1 response affordances for `补保巡丁`, `赔脚户误读`, `押文催县门`, `改走递报`, `暂缓强压`, or `请族老解释` can display which ordinary household is carrying the visible后账.
- Choice-surface acceptance must prove response affordances expose projected availability, leverage, cost, owner-module execution, and next-readback text that mention the affected household, without adding household-owned order commands.
- Command acceptance must prove issuing a response mutates only `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore` at command time, and does not mutate `PopulationAndHouseholds` or same-month `SocialMemoryAndRelations`.
- Receipt acceptance must prove the response receipt carries ordinary-household readback after owner-module resolution, while durable residue remains for later `SocialMemoryAndRelations` monthly handling.
- Unity acceptance must prove Desk Sandbox public-life command affordances and receipts copy projected ordinary-household response text only.
- Architecture acceptance must guard against summary parsing, UI/Application outcome computation, forbidden manager/god-controller names, `PersonRegistry` expansion, and household-control drift.
- Save/schema acceptance: v11 is runtime projection enrichment only and introduces no persisted fields, command request shape change, schema bump, migration, or save roundtrip requirement.

## Playable closure v12 home-household local response acceptance - 2026-04-25
- `public-life-order-home-household-local-response-v12` must prove ordinary-household pressure now has a first low-power home-household command loop, while staying rule-driven and avoiding event-pool / event-centered authority.
- Read-model acceptance must prove v5 refusal / partial residue plus v10/v11 household pressure projects Month N+1 affordances for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` from projected read models only.
- Command acceptance must prove issuing a local response mutates only `PopulationAndHouseholds` household labor, debt, distress, migration, and `LastLocalResponse*` / `LocalResponseCarryoverMonths` fields at command time.
- Same-month acceptance must prove the local response does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`; those modules still own their own public-order, yamen, family, and durable social-memory accounts.
- Outcome acceptance must cover at least two household local response paths among `Relieved`, `Contained`, `Strained`, and `Ignored`, including a successful relief/containment path and an eating-cost/strained path.
- Projection acceptance must prove public-life / family-facing / household readback exposes the local response result and household cost (`本户已缓`, `本户暂压`, `本户吃紧`, or `本户放置`) without implying county order or social memory was repaired by the household.
- Unity acceptance must prove shell adapters display projected home-household affordances and receipts only, never query `PopulationAndHouseholds`, select hidden household targets, or compute response outcome.
- Save/schema acceptance must prove `PopulationAndHouseholds` schema `2 -> 3` migration plus save/load preservation of local response trace fields. `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` schema versions do not change in v12.
- Architecture acceptance must guard boundary drift, summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, UI/Application outcome computation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v13 home-household social-memory readback acceptance - 2026-04-25
- `public-life-order-home-household-social-memory-v13` must prove the v12 local response loop continues into Month N+2 SocialMemory-owned residue without becoming an event-chain, event-pool, or UI-owned rule layer.
- Command-time acceptance remains strict: issuing `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信` mutates only `PopulationAndHouseholds` and does not mutate same-month `SocialMemoryAndRelations`.
- SocialMemory acceptance must prove Month N+2 reads structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`, then writes only existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- Path acceptance must cover at least two local response outcomes, including a relief/favor path and a strained/debt or shame path.
- Projection acceptance must prove home-household receipts expose the resulting SocialMemory readback while still saying the household response did not repair county order, yamen, or family authority.
- Unity acceptance must prove shell adapters copy projected receipt readback only and do not compute SocialMemory residue or parse summaries.
- Save/schema acceptance: v13 adds no persisted fields, no schema bump, and no migration; existing v12 `PopulationAndHouseholds` schema `3` save/load proof remains sufficient for the local response trace fields.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v14 home-household repeat-friction acceptance - 2026-04-25
- `public-life-order-home-household-repeat-friction-v14` must prove v13 SocialMemory residue can influence a later local household response as bounded local friction without becoming a thick social formula or UI-owned rule path.
- Command acceptance must prove `PopulationAndHouseholds` reads structured `SocialMemoryEntrySnapshot` cause keys and weights, then mutates only household labor, debt, distress, migration, and `LastLocalResponse*` fields.
- Same-command acceptance must prove the repeat response does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Path acceptance must cover at least one relieved/favor support path and one strained/debt drag path.
- Projection acceptance must prove local response affordances and receipts expose `旧账记忆` / `社会记忆读回` hints from read models only.
- Unity acceptance must prove shell adapters copy projected hints and do not compute local response effectiveness.
- Save/schema acceptance: v14 adds no persisted fields, no schema bump, no migration, no command target shape change, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v15 common-household response texture acceptance - 2026-04-25
- `public-life-order-common-household-response-texture-v15` must prove ordinary household state gives the local response lane readable play texture without becoming thick household-class simulation.
- Command acceptance must prove `PopulationAndHouseholds` derives texture from existing household fields (`DebtPressure`, `LaborCapacity`, `Distress`, `MigrationRisk`, `DependentCount`, `LaborerCount`, and `Livelihood`) and mutates only household labor, debt, distress, migration, and `LastLocalResponse*` fields.
- Same-command acceptance must prove texture-adjusted commands do not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Path acceptance must cover at least two texture paths, including debt-heavy compensation and labor-thin night restriction / road messaging.
- Projection acceptance must prove local response affordances and receipts expose `本户底色` hints from read models only, while UI/Application/Unity do not compute final response effectiveness.
- Unity acceptance must prove shell adapters copy projected `本户底色` fields and do not query modules or resolve outcomes.
- Save/schema acceptance: v15 adds no persisted fields, no schema bump, no migration, no command target shape change, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v16 home-household response capacity acceptance - 2026-04-25
- `public-life-order-home-household-response-capacity-v16` must prove the v12-v15 home-household lane now shows a projected `回应承受线` before thick household rules are added.
- Read-model acceptance must prove Month N+1 local response affordances expose bearable / risky / unfit capacity for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` from existing household fields only.
- Path acceptance must cover at least a debt-over-line compensation path, a labor-floor road-message path, and a migration-high night-travel path that remains useful when avoidance is the only local move.
- Command acceptance must prove issuing a capacity-shaped local response mutates only `PopulationAndHouseholds` at command time and can resolve as `Strained` without mutating `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Projection acceptance must prove affordances and receipts expose `回应承受线`, `承受线代价`, and `承受线读回` from read models only, while Application/UI/Unity do not compute final command outcome.
- Unity acceptance must prove shell adapters copy projected capacity fields and do not query modules, select hidden household targets, resolve outcomes, or write SocialMemory.
- Save/schema acceptance: v16 adds no persisted fields, no schema bump, no migration, no command target shape change, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v17 home-household response tradeoff forecast acceptance - 2026-04-25
- `public-life-order-home-household-response-tradeoff-v17` must prove the v12-v16 home-household lane now shows projected `取舍预判` before thick household rules are added.
- Read-model acceptance must prove Month N+1 local response affordances expose `预期收益`, `反噬尾巴`, and `外部后账` for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` from existing household fields only.
- Choice acceptance must prove the three options read differently: night travel favors migration / night-road relief, runner compensation favors calming `脚户误读` while risking `新欠账`, and road messaging favors `路情` clarity while spending `丁力`.
- Command acceptance must prove issuing a tradeoff-shaped local response mutates only `PopulationAndHouseholds` at command time without mutating `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- Projection acceptance must prove affordances and receipts expose `取舍预判`, `预期收益`, `反噬尾巴`, `外部后账`, and `取舍读回` from read models only, while Application/UI/Unity do not compute final command outcome.
- Unity acceptance must prove shell adapters copy projected tradeoff forecast fields and do not query modules, select hidden household targets, resolve outcomes, or write SocialMemory.
- Save/schema acceptance: v17 adds no persisted fields, no schema bump, no migration, no command target shape change, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v18 home-household short-term consequence readback acceptance - 2026-04-25
- `public-life-order-home-household-short-term-readback-v18` must prove the v12-v17 home-household lane now shows receipt-side `短期后果` after a local household response resolves, before thick household rules are added.
- Projection acceptance must prove receipts expose `缓住项`, `挤压项`, and `仍欠外部后账` for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` from existing projected household fields and structured `LastLocalResponse*` codes only.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time without mutating `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- SocialMemory acceptance must prove the later reader does not parse `短期后果`, `缓住项`, `挤压项`, `仍欠外部后账`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected short-term consequence receipt fields and do not query modules, select hidden household targets, resolve outcomes, or write SocialMemory.
- Save/schema acceptance: v18 adds no persisted fields, no schema bump, no migration, no command target shape change, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v19 home-household follow-up affordance acceptance - 2026-04-25
- `public-life-order-home-household-follow-up-affordance-v19` must prove the v12-v18 home-household lane now shows projected repeat/switch/cooldown hints on the next local response surface, before thick household rules are added.
- Projection acceptance must prove affordances expose `续接提示`, `换招提示`, `冷却提示`, and `续接读回` for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` from existing projected household fields and structured `LastLocalResponse*` codes only.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time without mutating `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, or `PersonRegistry`.
- SocialMemory acceptance must prove the later reader does not parse `续接提示`, `换招提示`, `冷却提示`, `续接读回`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected follow-up affordance fields and do not query modules, select hidden household targets, resolve outcomes, write SocialMemory, or maintain a cooldown ledger.
- Save/schema acceptance: v19 adds no persisted fields, no schema bump, no migration, no command target shape change, no cooldown ledger, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, and Application/UI/Unity writes to SocialMemory.

## Playable closure v20 owner-lane return guidance acceptance - 2026-04-25
- `public-life-order-owner-lane-return-guidance-v20` must prove the v12-v19 home-household lane now returns unresolved external after-accounts to their owning lanes, before thick household or county rules are added.
- Projection acceptance must prove receipts and the next affordance/readback expose `外部后账归位`, `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` from existing projected household fields and structured `LastLocalResponse*` codes only.
- Coverage acceptance must include at least one Order lane direction for 巡丁/路匪/route pressure and one Office or Family lane direction for 县门/文移/胥吏 or 族老/担保.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time and same-month handling does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance must prove the later reader does not parse `外部后账归位`, `该走巡丁`, `该走县门`, `该走族老`, `本户不能代修`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected owner-lane guidance fields and do not query modules, compute owner lanes, resolve outcomes, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- Save/schema acceptance: v20 adds no persisted fields, no schema bump, no migration, no command target shape change, no cooldown ledger, no owner-lane ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Playable closure v21 owner-lane return surface readback acceptance - 2026-04-26
- `public-life-order-owner-lane-return-surface-readback-v21` must prove the v20 owner-lane return guidance now appears on the lane-facing read surfaces that own the next action, without becoming a new command system or event pool.
- Projection acceptance must prove Office/Governance surfaces expose `该走县门/文移 lane` and Family-facing surfaces expose `该走族老/担保 lane` from existing projected household fields and structured `LastLocalResponse*` codes only.
- Coverage acceptance must keep Order lane visibility for 巡丁/路匪/route pressure while also proving at least one Office/Governance and one Family-facing owner-lane return surface.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time and same-month handling does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`.
- SocialMemory acceptance must prove the later reader does not parse Office/Governance or Family-facing owner-lane guidance, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters display projected Office/Governance and Family owner-lane guidance only and do not query modules, compute owner lanes, resolve outcomes, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- Save/schema acceptance: v21 adds no persisted fields, no schema bump, no migration, no command target shape change, no cooldown ledger, no owner-lane ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Playable closure v22 owner-lane handoff entry readback acceptance - 2026-04-26
- `public-life-order-owner-lane-handoff-entry-readback-v22` must prove v21 owner-lane return surfaces can also name projected `承接入口` labels for existing owner-lane affordances without becoming a command queue, event pool, or recommendation ledger.
- Projection acceptance must prove Order, Office/Governance, and Family surfaces expose lane-specific existing command-entry wording such as `添雇巡丁`, `押文催县门`, and `请族老解释` from existing projected household fields and structured `LastLocalResponse*` codes only.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time and same-month handling does not mutate `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`.
- SocialMemory acceptance must prove the later reader does not parse `承接入口`, owner-lane guidance prose, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected `承接入口` text only and do not query modules, navigate/execute commands from prose, compute owner lanes, resolve outcomes, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- Save/schema acceptance: v22 adds no persisted fields, no schema bump, no migration, no command target shape change, no command queue, no cooldown ledger, no owner-lane ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Playable closure v23 owner-lane receipt status readback acceptance - 2026-04-26
- `public-life-order-owner-lane-receipt-status-readback-v23` must prove v22 owner-lane surfaces can also show projected `归口状态` when an existing owner lane already has a structured response trace.
- Projection acceptance must prove Order, Office/Governance, and Family surfaces can expose `已归口到巡丁/路匪 lane`, `已归口到县门/文移 lane`, and `已归口到族老/担保 lane` from existing `HouseholdPressureSnapshot.LastLocalResponse*` plus owner-module `LastRefusalResponse*` fields only.
- Meaning acceptance must prove `已归口` is not "社会其他人接手" and not automatic repair: projected copy must include `归口不等于修好` and `仍看 owner lane 下月读回`.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time; later owner-lane responses mutate only `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`; same-month handling does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance must prove the later reader does not parse `归口状态`, `已归口到巡丁`, `已归口到县门`, `已归口到族老`, `归口不等于修好`, `仍看 owner lane 下月读回`, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected `归口状态` text only and do not query modules, compute归口, resolve outcomes, write SocialMemory, maintain an owner-lane or receipt-status ledger, or invent a hidden household target.
- Save/schema acceptance: v23 adds no persisted fields, no schema bump, no migration, no command target shape change, no command queue, no cooldown ledger, no owner-lane ledger, no receipt-status ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Playable closure v24 owner-lane outcome reading guidance acceptance - 2026-04-26
- `public-life-order-owner-lane-outcome-reading-guidance-v24` must prove v23 owner-lane surfaces can also show projected `归口后读法` after an existing owner lane has a structured outcome code.
- Projection acceptance must prove readback exposes `已修复：先停本户加压` and at least one non-repaired reading such as `恶化转硬：别让本户代扛` or `暂压留账：仍看本 lane 下月` from existing owner-module `LastRefusalResponseOutcomeCode` values only.
- Meaning acceptance must prove ordinary home-household response is not a universal repair lane: `归口后读法` tells the player how to read the owner-lane result after归口, while actual road-watch / yamen-document / elder-guarantee handling still belongs to `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time; later owner-lane responses mutate only `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`; same-month handling does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance must prove the later reader does not parse `归口后读法`, `已修复：先停本户加压`, `暂压留账`, `恶化转硬`, `放置未接`, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected `归口后读法` text only and do not query modules, compute owner-lane outcome meaning, write SocialMemory, maintain an outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- Save/schema acceptance: v24 adds no persisted fields, no schema bump, no migration, no command target shape change, no command queue, no cooldown ledger, no owner-lane ledger, no receipt-status ledger, no outcome ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Playable closure v25 owner-lane social-residue readback acceptance - 2026-04-26
- `public-life-order-owner-lane-social-residue-readback-v25` must prove v24 owner-lane surfaces can also show projected `社会余味读回` after the later monthly SocialMemory pass has visible structured residue.
- Projection acceptance must prove readback exposes at least one Order lane residue such as `后账渐平` and at least one Office or Family lane residue such as `后账转硬` or `后账暂压留账`, using existing `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane response command/outcome fields only.
- Meaning acceptance must prove ordinary home-household response is not a universal repair lane: `社会余味读回` says how SocialMemory is now settling, holding, hardening, or souring the owner-lane after-account; it is not a prompt to make the home household repair Order, Office, Family, or SocialMemory.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time; later owner-lane responses mutate only `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`; same-month owner-lane response handling does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance must prove no reader parses `社会余味读回`, owner-lane guidance prose, SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected `社会余味读回` text only and do not query modules, compute residue, write SocialMemory, maintain a SocialMemory / outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- Save/schema acceptance: v25 adds no persisted fields, no schema bump, no migration, no command target shape change, no command queue, no cooldown ledger, no owner-lane ledger, no receipt-status ledger, no outcome ledger, no SocialMemory ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Playable closure v26 owner-lane social-residue follow-up guidance acceptance - 2026-04-26
- `public-life-order-owner-lane-social-residue-followup-v26` must prove v25 owner-lane surfaces can also show projected `余味冷却提示`, `余味续接提示`, or `余味换招提示` after visible `社会余味读回`.
- Projection acceptance must prove at least two directions: an Order lane repaired / `后账渐平` path exposes `余味冷却提示`, and an Office or Family lane contained/escalated/ignored path exposes `余味续接提示` or `余味换招提示`, using existing `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and owner-lane response command/outcome fields only.
- Meaning acceptance must prove ordinary home-household response is not a universal follow-up lane: v26 wording tells the player whether to cool down, lightly continue in the owner lane, switch owner-lane tactic, or wait for a better entry; it is not a prompt to force the home household to repair Order, Office, Family, or SocialMemory.
- Command acceptance remains strict: issuing the local response mutates only `PopulationAndHouseholds` at command time; later owner-lane responses mutate only `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`; same-month owner-lane response handling does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance must prove no reader parses `余味冷却提示`, `余味续接提示`, `余味换招提示`, `社会余味读回`, owner-lane guidance prose, SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`; durable residue still comes from structured aftermath fields.
- Unity acceptance must prove shell adapters copy projected `余味冷却提示` / `余味续接提示` / `余味换招提示` text only and do not query modules, compute follow-up validity, write SocialMemory, maintain a follow-up / SocialMemory / outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- Save/schema acceptance: v26 adds no persisted fields, no schema bump, no migration, no command target shape change, no command queue, no cooldown ledger, no owner-lane ledger, no receipt-status ledger, no outcome ledger, no follow-up ledger, no SocialMemory ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Playable closure v27-v30 owner-lane closure acceptance - 2026-04-26
- `public-life-order-owner-lane-affordance-echo-v27` must prove existing owner-lane affordances can show projected `现有入口读法` with `建议冷却`, `可轻续`, `建议换招`, or `等待承接口` without changing command availability or routing.
- `public-life-order-owner-followup-receipt-closure-v28` must prove owner-lane receipts can show projected `后手收口读回` such as `已收口`, `仍留账`, `转硬待换招`, or `未接待承口` from existing owner outcome codes and matching SocialMemory cause keys only.
- `public-life-order-owner-lane-no-loop-guard-v29` must prove projected `闭环防回压` keeps stale guidance from pointing back to the home household. Repaired/cooling paths must read as `后账已收束`, `旧提示仅作读回`, and `不重复追本户`.
- `public-life-order-owner-lane-v30-closure-audit` must prove v20-v30 remain a thin projection/readback closure arc, not a new event pool, command system, household repair lane, or persisted ledger.
- SocialMemory acceptance must prove no reader parses `现有入口读法`, `后手收口读回`, `闭环防回压`, owner-lane guidance prose, SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`.
- Unity acceptance must prove shell adapters copy v27-v30 projected text only and do not query modules, compute follow-up or closure validity, write SocialMemory, maintain a stale-guidance / follow-up / SocialMemory / outcome / owner-lane / receipt-status ledger, or invent a hidden household target.
- Save/schema acceptance: v27-v30 add no persisted fields, no schema bump, no migration, no command target shape change, no command queue, no cooldown ledger, no owner-lane ledger, no receipt-status ledger, no outcome ledger, no stale-guidance ledger, no follow-up ledger, no SocialMemory ledger, no household target field, and no new SocialMemory field.
- Architecture acceptance must guard summary parsing, forbidden manager/god-controller names, `PersonRegistry` expansion, foreign state mutation, Application/UI/Unity authority drift, and no new schema without migration docs/tests.

## Backend event contract health v32 acceptance - 2026-04-26
- Ten-year simulation diagnostics must classify emitted-but-unconsumed and declared-but-not-emitted DomainEvent contract debt rather than leaving it as unlabelled noise.
- Classification acceptance must include `ProjectionOnlyReceipt`, `FutureContract`, `DormantSeededPath`, and `AcceptanceTestGap`; `AlignmentBug` remains an explicit future category, not an implicit assumption.
- Diagnostic key acceptance must prevent false debt caused by double module prefixes such as `OfficeAndCareer.OfficeAndCareer.*`.
- Architecture acceptance remains strict: v32 must not add event-pool authority, new gameplay rules, command surfaces, persisted state, schema bump, migration, manager/controller layers, `PersonRegistry` expansion, Application/UI/Unity authority, or summary parsing.
- Save/schema acceptance: v32 diagnostic classifications are runtime/test evidence only and add no persisted fields, no module envelope, no root/module schema version change, no migration, and no save roundtrip change.

## Backend event contract health v33 acceptance - 2026-04-26
- Ten-year simulation diagnostics must fail when current emitted-without-authority-consumer or declared-but-not-emitted `DomainEvent` debt remains `Unclassified`.
- Focused integration acceptance must prove the no-unclassified gate rejects a synthetic unclassified debt entry and the 120-month campaign health report passes with current debt explicitly classified.
- Architecture acceptance must prove the gate lives in diagnostics/tests, does not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`, and does not create Application/UI/Unity authority.
- Save/schema acceptance: v33 diagnostic gate is runtime/test evidence only and adds no persisted fields, no module envelope, no root/module schema version change, no migration, and no save roundtrip change.

## Backend event contract health v34 acceptance - 2026-04-26
- Ten-year simulation diagnostics must show event-contract classification readback with a structured `owner=<module>` lane and an `evidence=<doc/test backlink>` for visible classified debt.
- Focused integration acceptance must prove emitted-without-authority-consumer and declared-but-not-emitted examples include owner/evidence backlinks while preserving the v33 no-unclassified gate.
- Architecture acceptance must prove owner/evidence backlinks live in diagnostics/tests, do not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`, and do not create Application/UI/Unity authority.
- Save/schema acceptance: v34 evidence backlinks are runtime/test diagnostics only and add no persisted fields, no module envelope, no root/module schema version change, no migration, no event-health ledger, and no save roundtrip change.

## Backend canal-window Trade/Order v35 acceptance - 2026-04-26
- `WorldSettlements.CanalWindowChanged` must be consumed by both `TradeAndIndustry` and `OrderAndBanditry` through declared `ConsumedEvents`, so the ten-year event-contract health run no longer treats it as emitted-without-authority-consumer debt.
- Focused module acceptance must prove a closed canal window adjusts only water/canal-exposed Trade state and only water/canal-exposed Order state, with comparable off-scope settlements unchanged.
- Architecture acceptance must prove the handlers use structured canal-window metadata plus `IWorldSettlementsQueries`, not `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- Save/schema acceptance: v35 adds no persisted fields, no module envelope, no root/module schema version change, no migration, no canal ledger, no owner-lane ledger, no UI/Unity authority, and no save roundtrip change.

## Backend household-family burden v36 acceptance - 2026-04-26
- `FamilyCore` must declare and consume `PopulationAndHouseholds.HouseholdDebtSpiked`, `PopulationAndHouseholds.HouseholdSubsistencePressureChanged`, and `PopulationAndHouseholds.HouseholdBurdenIncreased` through the scheduler event-drain seam.
- Focused module acceptance must prove sponsor-clan targeting via `IPopulationAndHouseholdsQueries.GetRequiredHousehold(...)` and `SponsorClanId`, with off-scope clans unchanged and no-sponsor households no-op.
- Integration acceptance must prove a real monthly tax-season burden can drain into sponsor-clan `FamilyCore` pressure in the same month.
- Architecture acceptance must prove the handler uses structured event/entity/metadata and population query snapshots only, not `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, `LastLocalResponseSummary`, `PopulationAndHouseholdsState`, foreign mutable state, Application/UI/Unity authority, forbidden manager names, or `PersonRegistry` expansion.
- SocialMemory acceptance must prove v36 same-month handling does not mutate `SocialMemoryAndRelations`; durable residue remains a later structured read/write concern.
- Save/schema acceptance: v36 adds no persisted fields, no module envelope, no root/module schema version change, no migration, no relief ledger, no sponsor-lane ledger, no household target field, no SocialMemory field, no UI/Unity authority, and no save roundtrip change.

## Backend office/yamen implementation drag v37 acceptance - 2026-04-26
- `OfficeAndCareer` must declare `PolicyImplemented` as a published event and `PolicyWindowOpened` as a consumed event.
- Focused module acceptance must prove `PolicyWindowOpened` can resolve to dragged, captured, and paper-compliance implementation outcomes while mutating only matching-settlement `OfficeAndCareer` state; off-scope jurisdictions must remain unchanged.
- Integration acceptance must prove the real monthly scheduler can drain `CourtAgendaPressureAccumulated -> PolicyWindowOpened -> PolicyImplemented` in the same month through bounded fresh-event rounds.
- Architecture acceptance must prove the handler uses structured `PolicyWindow*` metadata, `EntityKey` / `SettlementId`, and office-owned state only, not `DomainEvent.Summary`, receipt prose, response summaries, foreign mutable state, Application/UI/Unity authority, forbidden manager names, or `PersonRegistry` expansion.
- SocialMemory acceptance: v37 same-month handling does not write `SocialMemoryAndRelations`; durable shame/fear/favor/grudge/obligation residue remains a later structured read/write concern if added.
- Save/schema acceptance: v37 adds no persisted fields, no module envelope, no root/module schema version change, no migration, no policy ledger, no yamen workflow state, no owner-lane ledger, no SocialMemory field, no UI/Unity authority, and no save roundtrip change.

## Backend office/yamen readback spine v38-v45 acceptance - 2026-04-26
- Public-life acceptance: after `OfficeAndCareer.PolicyImplemented`, `PublicLifeAndRumor` must project county-gate readback such as `县门执行读回`, `OfficeAndCareer lane`, and `本户不能代修` from structured metadata, not from event summaries.
- Governance/read-model acceptance: governance lanes/docket and office affordances must expose `OfficeImplementationReadbackSummary`, `OfficeNextStepReadbackSummary`, `RegimeOfficeReadbackSummary`, `CanalRouteReadbackSummary`, and `ResidueHealthSummary` as runtime projections.
- SocialMemory acceptance: same-month implementation handling must not mutate `SocialMemoryAndRelations`; the later monthly SocialMemory pass may write `OfficePolicyImplementationResidue` only from structured `JurisdictionAuthoritySnapshot` fields.
- UI/Unity acceptance: shell and Unity ViewModels may display projected office/yamen readback fields only. They must not query modules, compute implementation effectiveness, infer owner lanes, parse receipt/event prose, or write SocialMemory.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, and no new schema without migration docs/tests.
- Save/schema acceptance: v38-v45 add no persisted fields, no module envelope, no root/module schema version change, no migration, no policy/yamen/owner-lane/cooldown ledger, no household target field, and no save roundtrip change.

## Backend office-lane closure v46-v52 acceptance - 2026-04-26
- Governance/read-model acceptance: after v38-v45 Office/yamen readback, governance lanes, dockets, office surfaces, and relevant receipts expose runtime-only `OfficeLaneEntryReadbackSummary`, `OfficeLaneReceiptClosureSummary`, `OfficeLaneResidueFollowUpSummary`, and `OfficeLaneNoLoopGuardSummary`.
- Projection acceptance: readback must include Office-lane cues such as `Office承接入口`, `该走县门/文移 lane`, `Office后手收口读回`, `Office余味续接读回`, `Office闭环防回压`, and `本户不再代修` from structured `JurisdictionAuthoritySnapshot` fields, existing Office response trace codes, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State` only.
- Meaning acceptance: ordinary home-household response remains a low-power local response surface. It can ease/strain the home household, but it cannot repair county-yamen/document/clerk delay, route pressure, family guarantee face, or durable social residue.
- Command acceptance remains strict: command-time local household response mutates only `PopulationAndHouseholds`; Office owner-lane response/implementation mutates only `OfficeAndCareer`; same-month handling does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance: later residue may read structured Office aftermath and cause keys only. It must not parse `Office承接入口`, `Office后手收口读回`, `Office余味续接读回`, `Office闭环防回压`, `本户不再代修`, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, or `DomainEvent.Summary`.
- UI/Unity acceptance: shell and Unity ViewModels copy projected Office-lane closure fields only. They must not query modules, compute Office closure, infer owner-lane validity, maintain ledgers, parse prose, or write SocialMemory.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, and no new schema without migration docs/tests.
- Save/schema acceptance: v46-v52 add no persisted fields, no module envelope, no root/module schema version change, no migration, no policy/yamen/owner-lane/receipt-status/outcome/cooldown/follow-up ledger, no household target field, and no save roundtrip change.
- Focused proof: `OfficeCourtRegimePressureChainTests`, `PublicLifeOrderActorCountermoveRuleDrivenTests`, `ProjectReferenceTests`, and `FirstPassPresentationShellTests` cover Office closure projection, later residue readback, architecture guards, and Unity copy-only behavior.

## Backend Family-lane closure v53-v60 acceptance - 2026-04-26
- Read-model acceptance: after v52 owner-lane readback, public-life, family-facing, governance/docket, desk settlement, and relevant receipts expose runtime-only `FamilyLaneEntryReadbackSummary`, `FamilyElderExplanationReadbackSummary`, `FamilyGuaranteeReadbackSummary`, `FamilyHouseFaceReadbackSummary`, `FamilyLaneReceiptClosureSummary`, `FamilyLaneResidueFollowUpSummary`, and `FamilyLaneNoLoopGuardSummary`.
- Projection acceptance: readback must include Family-lane cues such as `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, `Family闭环防回压`, and `不是普通家户再扛` from structured `ClanSnapshot`, `HouseholdPressureSnapshot`, `SponsorClanId`, existing Family response trace codes, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State` only.
- Meaning acceptance: ordinary home-household response remains a low-power local response surface. It can ease/strain the home household, but it cannot repair clan elder explanation, household guarantee, lineage-house face, sponsor-clan pressure, Office/Order aftermath, or durable social residue.
- Command acceptance remains strict: command-time local household response mutates only `PopulationAndHouseholds`; Family owner-lane response mutates only `FamilyCore`; same-month local household response does not mutate `SocialMemoryAndRelations`.
- SocialMemory acceptance: later residue may read structured Family aftermath and cause keys only. It must not parse `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family闭环防回压`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, receipt prose, projected Family prose, or `DomainEvent.Summary`.
- UI/Unity acceptance: shell and Unity ViewModels copy projected Family-lane closure fields only. They must not query modules, compute Family closure, infer guarantee success, maintain ledgers, parse prose, or write SocialMemory.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, no `WorldManager` / `PersonManager` / `CharacterManager` path, and no new schema without migration docs/tests.
- Save/schema acceptance: v53-v60 add no persisted fields, no module envelope, no root/module schema version change, no migration, no Family closure / guarantee / owner-lane / receipt-status / outcome / cooldown / follow-up ledger, no household target field, and no save roundtrip change.
- Focused proof: `PublicLifeOrderActorCountermoveRuleDrivenTests`, `ProjectReferenceTests`, and `FirstPassPresentationShellTests` cover Family closure projection, no summary parsing, architecture guards, and Unity copy-only behavior; local household command/no-SocialMemory guarantees remain covered by `PublicLifeOrderRefusalResponseRuleDrivenTests`.

## Backend Family relief choice v61-v68 acceptance - 2026-04-26
- Command acceptance: `GrantClanRelief` must be cataloged and advertised by `FamilyCore`, resolve inside `FamilyCore`, and mutate only existing family-owned support/charity/branch/relief/mediation pressure fields plus existing conflict receipt fields.
- Projection acceptance: Family-facing affordances and receipts must expose `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, and `不是普通家户再扛` as projected readback. UI/Unity must copy projected command fields only.
- Meaning acceptance: ordinary home-household response remains a low-power `PopulationAndHouseholds` surface. It can ease/strain the household, but it cannot repair Family relief, lineage-house face, sponsor-clan pressure, or durable social residue.
- Command-time isolation acceptance: `GrantClanRelief` must not mutate `PopulationAndHouseholds` and must not write `SocialMemoryAndRelations` during the same command.
- SocialMemory acceptance: later residue may read structured Family aftermath only. It must not parse `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, `不是普通家户再扛`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, receipt prose, projected Family prose, or `DomainEvent.Summary`.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, no relief/charity/owner-lane ledger, no household target field, and no new schema without migration docs/tests.
- Save/schema acceptance: v61-v68 add no persisted fields, no module envelope, no root/module schema version change, no migration, no relief / charity / guarantee / Family closure / owner-lane / receipt-status / outcome / cooldown / follow-up ledger, no household target field, and no save roundtrip change.
- Focused proof: `FamilyCoreCommandResolverTests`, `FamilyReliefChoiceCommandTests`, `ProjectReferenceTests`, and `FirstPassPresentationShellTests` cover FamilyCore ownership, command-time isolation, architecture guards, and Unity copy-only behavior.

## Backend Force/Campaign/Regime owner-lane readback v69-v76 acceptance - 2026-04-26
- Read-model acceptance: governance lanes, owner-lane docket, warfare affordances, and campaign receipts expose runtime-only `WarfareLaneEntryReadbackSummary`, `ForceReadinessReadbackSummary`, `CampaignAftermathReadbackSummary`, `WarfareLaneReceiptClosureSummary`, `WarfareLaneResidueFollowUpSummary`, and `WarfareLaneNoLoopGuardSummary`.
- Projection acceptance: readback must include `军务承接入口`, `Force承接读回`, `战后后账读回`, `军务后手收口读回`, `军务余味续接读回`, `军务闭环防回压`, `不是普通家户硬扛`, and `不是把军务后账误读成县门/Order后账` from structured campaign/force/office/clan/SocialMemory snapshots only.
- Meaning acceptance: ordinary home-household response remains a low-power `PopulationAndHouseholds` surface. It can ease/strain households, but it cannot repair force readiness, campaign aftermath, military order, regime coordination, or durable social residue.
- SocialMemory acceptance: later residue may read structured campaign/force aftermath only. It must not parse projected military readback text, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, receipt prose, SocialMemory summary prose, or `DomainEvent.Summary`.
- UI/Unity acceptance: shell and Unity ViewModels copy projected Force/Campaign/Regime closure fields only. They must not query modules, compute closure, infer Office/Order fallback, maintain ledgers, parse prose, or write SocialMemory.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, no force/campaign closure ledger, no household target field, and no new schema without migration docs/tests.
- Save/schema acceptance: v69-v76 add no persisted fields, no module envelope, no root/module schema version change, no migration, no force/campaign closure / owner-lane / cooldown / follow-up ledger, no household target field, and no save roundtrip change.
- Focused proof: `M2LiteIntegrationTests`, `ProjectReferenceTests`, and `FirstPassPresentationShellTests` cover campaign/force/governance projection, architecture guards, and Unity copy-only behavior.

## Backend Warfare directive choice depth v77-v84 acceptance - 2026-04-26
- Command acceptance: `DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, and `WithdrawToBarracks` resolve through `WarfareCampaignCommandResolver` and mutate only existing `WarfareCampaign` directive fields.
- Read-model acceptance: warfare affordances and campaign receipts expose `军令选择读回` plus the appropriate `案头筹议选择`, `点兵加压选择`, `粮道护持选择`, or `归营止损选择` wording alongside v69-v76 closure guidance.
- Meaning acceptance: `WarfareCampaign拥有军令`; the military choice is not county-yamen paperwork, not an Order/public-life repair, and not an ordinary home-household line.
- SocialMemory acceptance: no reader parses `军令选择读回`, directive-choice prose, `LastDirectiveTrace`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`.
- UI/Unity acceptance: shell and Unity ViewModels copy projected directive-choice readback only. They must not query modules, compute directive success, infer owner lanes, parse prose, or write SocialMemory.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, no directive/owner-lane ledger, no household target field, and no new schema without migration docs/tests.
- Save/schema acceptance: v77-v84 add no persisted fields, no module envelope, no root/module schema version change, no migration, no directive / force / campaign / owner-lane / cooldown / follow-up ledger, no household target field, and no save roundtrip change.

## Backend Warfare aftermath docket readback v85-v92 acceptance - 2026-04-26
- Read-model acceptance: `PresentationReadModelBundle` exposes existing `AftermathDocketSnapshot` values as `CampaignAftermathDockets`, and warfare affordances / campaign receipts / governance lanes compose `战后案卷读回` into `CampaignAftermathReadbackSummary`.
- Projection acceptance: readback must include `记功簿读回`, `劾责状读回`, `抚恤簿读回`, `清路札读回`, `WarfareCampaign拥有战后案卷`, `战后案卷不是县门/Order代算`, `不是普通家户补战后`, and `军务案卷防回压` from structured docket lists only.
- Meaning acceptance: the military aftermath docket is not county-yamen paperwork, not an Order/public-life repair, and not an ordinary home-household repair line.
- SocialMemory acceptance: no reader parses `战后案卷读回`, docket readback prose, `DocketSummary`, `LastDirectiveTrace`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`.
- UI/Unity acceptance: shell and Unity ViewModels display projected aftermath docket snapshots/fields only. They must not query modules, infer docket contents from notifications/event traces/settlement stats, parse prose, or write SocialMemory.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, no aftermath/relief/route-repair/owner-lane ledger, no household target field, and no new schema without migration docs/tests.
- Save/schema acceptance: v85-v92 add no persisted fields, no module envelope, no root/module schema version change, no migration, no aftermath / relief / route-repair / force / campaign / owner-lane / cooldown / follow-up ledger, no household target field, and no save roundtrip change.
- Focused proof: `CampaignAftermathDocketReadback_ProjectsWarfareOwnedDocketWithoutOfficeOrderBackfill`, `Warfare_aftermath_docket_readback_must_stay_projection_only_and_schema_neutral`, and `Compose_ProjectsRegionalWarfareAndAftermathIntoHallDeskAndCampaignBoard` cover Application projection, architecture guards, and Unity projected-docket display.

## Backend court-policy process readback v93-v100 acceptance - 2026-04-26
- Read-model acceptance: governance lanes, focus, dockets, Office jurisdiction surfaces, great hall governance summaries, and desk settlement nodes expose runtime-only `CourtPolicyEntryReadbackSummary`, `CourtPolicyDispatchReadbackSummary`, `CourtPolicyPublicReadbackSummary`, and `CourtPolicyNoLoopGuardSummary`.
- Projection acceptance: readback must include `朝议压力读回`, `政策窗口读回`, `文移到达读回`, `县门执行承接读回`, `公议读法读回`, `Court后手不直写地方`, `Office/PublicLife分读`, `不是本户也不是县门独吞朝廷后账`, and `Court-policy防回压` from structured `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` fields only.
- Meaning acceptance: court-policy after-accounting is not ordinary household repair, not a county-yamen-only receipt, not an Office/public-life prose parser, and not a new court engine.
- SocialMemory acceptance: no reader parses court-policy projection prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, receipt prose, or `DomainEvent.Summary`.
- UI/Unity acceptance: shell and Unity ViewModels display projected court-policy fields only. They must not query modules, compute policy success, infer ownership from notice/dispatch prose, maintain ledgers, parse prose, or write SocialMemory.
- Architecture acceptance: guard no Application/UI/Unity authority drift, no summary parsing, no forbidden manager/god-controller names, no `PersonRegistry` expansion, no court/policy/dispatch/owner-lane ledger, no household target field, and no new schema without migration docs/tests.
- Save/schema acceptance: v93-v100 add no persisted fields, no module envelope, no root/module schema version change, no migration, no court module, no dispatch / policy closure / owner-lane / cooldown / follow-up ledger, no household target field, and no save roundtrip change.
- Focused proof: `OfficeCourtRegimePressureChainTests`, `Office_yamen_readback_spine_must_stay_projection_only_and_schema_neutral`, `Unity_office_yamen_readback_must_copy_projected_fields_only`, and `Compose_CopiesOfficeYamenReadbackSpineWithoutShellAuthority` cover Application projection, architecture guards, SocialMemory non-parsing, and Unity copy-only display.

## Backend thin-chain closeout audit v101-v108 acceptance - 2026-04-26
- Closeout acceptance: the current Renzong thin-chain skeleton is documented as closed through v100 only as thin topology/readback evidence: source pressure, owner module, structured event/query seam, scheduler drain or delayed residue path, repetition guard, off-scope/no-touch boundary where applicable, player-facing readback, UI/Unity copy-only display, no-summary-parsing guard, and no-save/no-schema evidence.
- Non-overclaim acceptance: the closeout must explicitly preserve full-chain debt. It must not claim completed thick household registration, tax/corvee, famine, relief economy, full court process, office factions, campaign logistics, regime recognition, canal politics, durable residue depth, or recovery/decay tuning.
- Architecture acceptance: no production command, event, query, read-model, ViewModel, Unity adapter, scheduler, persistence, module-rule, manager/god-controller, `PersonRegistry`, Application rule-layer, or UI authority change is introduced by the audit.
- Save/schema acceptance: v101-v108 adds no persisted fields, no module envelope, no root/module schema version change, no migration, no save manifest change, no ledger, no household target field, and no save roundtrip change.
- Focused proof: `Thin_chain_closeout_audit_must_document_v100_without_claiming_full_chain_completion` guards the permanent docs, schema-neutral note, no-summary-parsing boundary, and full-chain debt distinction.
