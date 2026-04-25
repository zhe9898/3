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
