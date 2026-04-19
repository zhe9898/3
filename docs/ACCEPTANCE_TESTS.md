# ACCEPTANCE_TESTS

## Global acceptance rules
Every release line must pass:
- deterministic replay tests
- save roundtrip tests
- invariant tests
- feature-manifest compatibility tests
- module boundary tests where practical

## Phase M0
- 12-month empty/minimal world replay equality
- save root manifest validation
- module registration order deterministic

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
- explicit default MVP bootstraps/loaders keep `PublicLifeAndRumor`, `OrderAndBanditry`, `ConflictAndForce`, `OfficeAndCareer`, and `WarfareCampaign` disabled unless a non-MVP path is explicitly selected
- explicit default MVP bootstraps remain deterministic across a 20-year headless run
- MVP preview artifacts can also stretch the default bootstrap across a 10-year family-lifecycle run while keeping hall, family council, and notification guidance aligned on the same next family action
- notifications trace back to `WorldDiff` entries
- notification history retention stays bounded and trims oldest notices first
- runtime-only debug traces reset after save/load and do not affect compatibility
- first-pass shell composes from read-model bundles only
- great hall and desk-sandbox public-life summaries compose from `PublicLifeAndRumor` read models only and remain read-only
- venue-channel public-life summaries compose from `PublicLifeAndRumor` read models only and remain read-only
- public-life surfaces can show how榜文、街谈、路报、州牒 differ without UI inventing new authority logic or private state
- player-command affordances and receipts also compose from read-model bundles only
- bounded public-life affordances / receipts such as `张榜晓谕`, `遣吏催报`, `催护一路`, and `请族老出面` appear on settlement nodes only when their owning modules project them; UI still does not resolve authority rules
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
- legacy `PublicLifeAndRumor` schema `1 -> 2 -> 3 -> 4` saves migrate through the default loaders and backfill cadence, venue-channel, plus channel-contention descriptors conservatively enough to keep current M2+ paths loadable
- legacy `WorldSettlements` schema `1` saves migrate to schema `2` through the default loaders and backfill settlement tiers conservatively enough to continue load on current M2+ paths

## Phase M3
- active M2 bootstraps remain isolated from `OrderAndBanditry` and `ConflictAndForce`
- order-enabled M3 bridge saves/load keep `OrderAndBanditry` enabled while `ConflictAndForce` remains disabled
- conflict-enabled M3 lite runs can complete 240-month multi-seed diagnostics sweeps without runtime metrics escaping into save compatibility
- order-only stress and local-conflict stress bootstraps preserve settlement parity while keeping module activation surfaces distinct
- `OrderAndBanditry.Lite` produces traceable disorder/route-pressure diffs and notifications
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
- route insecurity affects trade through `OrderAndBanditry` queries or events, not direct mutation
- `ConflictAndForce.Lite` affects `OrderAndBanditry` through queries or events, not direct mutation
- local conflict resolution deterministic
- legacy M3 local-conflict saves can migrate `ConflictAndForce` schema `1` to `2` to `3` through the default local-conflict loader
- migrated legacy M3 stress saves continue deterministic replay against the current-schema local-conflict loader
- migrated local-conflict loads can surface runtime-only migration steps and current hotspots through the read-only debug shell
- migration reports preserve enabled-module and module-envelope key sets unless an explicit migration says otherwise
- migration preparation leaves source save data unchanged while reporting consistency status on the prepared copy
- campaign aftermath can push deterministic owned-state fatigue / escort-strain fallout into `ConflictAndForce` without cross-module writes
- `ConflictAndForce` campaign-fatigue fallout must reduce only conflict-owned readiness / command / escort posture and recover through later monthly passes
- post-MVP black-route migration tests stay inside `OrderAndBanditry` and `TradeAndIndustry` module envelopes and do not create a standalone module key
- stable M2/M3 bootstraps remain isolated from `OfficeAndCareer` unless the governance-lite path is selected
- legacy `FamilyCore` schema `1` saves migrate to schema `2` and then `3` through the default loaders without changing enabled-module or module-envelope key sets
- family command handling mutates only `FamilyCore` directly; `SocialMemoryAndRelations` may react later through family queries, not same-command cross-module writes
- family lifecycle commands surface in family read models and hall / council projections without adding any authority logic to UI
- family lifecycle events such as `议亲定婚`, `门内添丁`, `门内举哀`, and `承祧未稳` should project dedicated ancestral-hall-facing notice text rather than generic module fallback titles
- follow-up family commands such as `拨粮护婴` and `议定丧次` should remain bounded `FamilyCore` writes and surface as read-only receipts on hall / council projections
- family lifecycle notices should also carry concrete next-step guidance about襁褓护持, 口粮与乳哺, 丧次祭次, and承祧名分 when relevant traces indicate those pressures
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
- legacy governance-lite office saves migrate from schema `1` to `2` to `3` without changing enabled-module or module-envelope key sets
- legacy governance-lite office saves reconstruct v2-only task/petition/service descriptors first, then backfill queue pressure and clerk dependence conservatively enough to continue replay on the current schema path
- `OrderAndBanditry` and `ConflictAndForce` may read office leverage only through queries, not direct mutation
- first-pass presentation may surface office appointments, task tiers, petition categories, petition backlog, and promotion/demotion pressure summaries only when governance-lite is enabled, and must remain office-empty on stable M2/M3 paths
- governance-lite presentation may surface bounded office command affordances and recent office command receipts only when governance-lite is enabled, and must remain command-empty on stable M2/M3 paths

### Order/banditry pack
- outlaw/banditry state can be enabled without schema collisions
- route insecurity affects trade via queries/events, not direct mutation
- black-route preflight contracts keep pressure and ledger ownership split across `OrderAndBanditry` and `TradeAndIndustry`

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

## Boundary tests
At integration level, verify:
- no module writes foreign namespace
- projections remain read-only
- disabled feature packs load clean defaults
