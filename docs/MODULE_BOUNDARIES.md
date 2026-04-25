# MODULE_BOUNDARIES

This document defines authoritative module boundaries.

## Boundary rules
For every module, define:
- owned state
- public queries/projections
- accepted commands
- emitted domain events
- upstream reads
- explicit non-responsibilities

For the full person-data ownership model, see `PERSON_OWNERSHIP_RULES.md`.
For the design rule that historical figures, reforms, wars, and great trends enter as pressure rather than rail scripts, see `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`.

## Historical process is not currently a hidden module

Named historical people and great trends may appear in design, projections, seed data, and later feature packs, but they do not justify bypassing module ownership.

Until a dedicated historical-process pack exists:
- office and appointment pressure belongs in `OfficeAndCareer`
- exam and scholar pressure belongs in `EducationAndExams`
- fiscal, tax, route, granary, and settlement pressure belongs in `WorldSettlements`, `TradeAndIndustry`, or `PopulationAndHouseholds` as appropriate
- public legitimacy, edicts, reform rumor, and street interpretation belong in `PublicLifeAndRumor` / `NarrativeProjection` as projection or public-life state
- war, frontier, and supply pressure belongs in `ConflictAndForce` / `WarfareCampaign`
- memories, faction labels, obligations, and shame belong in `SocialMemoryAndRelations`

A future historical-process pack may own high-level trend windows, named-figure pressure, and scenario chronology, but it must still integrate through Query / Command / DomainEvent and must not become a global script runner.

## 0. PersonRegistry (Kernel layer, identity-only)
### Owns
- person identity anchors: PersonId, display name, birth date, gender
- life stage (Infant / Child / Youth / Adult / Elder / Deceased)
- alive/dead status
- fidelity ring assignment (Core / Local / Regional)

PersonRegistry is **identity-only**. It exists so that all modules can reference the same person by PersonId without any module becoming a "person master table". It does not hold mutable domain state.

### Design guardrail
If a proposed field answers "what is this person doing / feeling / capable of / related to" rather than "does this person exist and are they alive", it does not belong here. It belongs in a domain module.

### Public queries
- `IPersonRegistryQueries`: lookup by PersonId, lookup by fidelity ring, alive filter

### Accepts commands
- none from player
- internal: Create, MarkDeceased, PromoteFidelityRing, DemoteFidelityRing

### Emits events
- `PersonCreated`
- `PersonDeceased`
- `FidelityRingChanged`

### Does not own
- personality, abilities, social position, health details, kinship, activity, or any domain-specific state
- these belong to the respective domain modules (see `PERSON_OWNERSHIP_RULES.md`)

### Time contract
- `month`: age progression, life-stage checks

### Schema note
- independent schema namespace, expected to be extremely stable and rarely migrated

## 1. WorldSettlements
### Owns
- regions, settlements, roads, route conditions
- local prosperity/security/environment indicators
- institution registry and baseline condition
- settlement tier / node rank used by downstream projections
- season-band declaration watermarks such as flood-disaster and frontier-strain bands

### Public queries
- settlement security
- route status
- institution availability
- local pressure indicators
- settlement tier and parent-node relationship

### Accepts commands
- mostly none from player in MVP
- limited local investment/support commands later

### Emits events
- `SettlementSecurityChanged`
- `RouteDisrupted`
- `InstitutionOpenedOrClosed`
- `HarvestPressureChanged`
- settlement-scoped pressure facts such as `WorldSettlements.DisasterDeclared` and `WorldSettlements.FrontierStrainEscalated`

### Does not own
- family tree
- household economy details
- exam outcomes
- trade ledgers
- grievances

## 2. FamilyCore
### Owns
- lineage graph (clan membership, branch position per PersonId)
- marriage links and household branch membership
- inheritance state
- clan policies / house rules
- branch split/merge state
- lineage-conflict pressure, mediation momentum, and branch-favor / relief-sanction pressure
- marriage-alliance pressure/value, heir security, reproductive pressure, and mourning load
- care burden, funeral debt, remedy confidence, and charity-obligation pressure inside the clan namespace
- last family-command receipts that remain family-owned state
- family-owned public-life refusal response trace for clan explanation / household guarantee repair, including v8 quiet elder explanation / guarantee-avoidance countermoves
- personality traits per clan member: ambition, prudence, loyalty, sociability
- clan-scoped kinship references: spouseId, childrenIds, fatherId, motherId (only for persons who are or were clan members; FamilyCore does not track kinship for non-clan persons such as unaffiliated commoners, bandits, or officials from other lineages)

### Public queries
- heirs
- marriage eligibility
- branch relations
- clan prestige projection
- lineage-conflict and mediation projection
- personality traits per clan member PersonId
- clan-scoped kinship view (only for persons who are or were clan members; not a global kinship registry)
- family-owned public-life refusal response aftermath fields for readback and later SocialMemory adjustment

### Accepts commands
- `ArrangeMarriage`
- `DesignateHeirPolicy`
- `SupportNewbornCare`
- `SetMourningOrder`
- `SupportSeniorBranch`
- `OrderFormalApology`
- `PermitBranchSeparation`
- `SuspendClanRelief`
- `InviteClanEldersMediation`
- `InviteClanEldersPubliclyBroker`
- `AskClanEldersExplain` /请族老解释 for public-life refusal response when family standing can repair or contain shame

> Note: `RedistributeHouseholdSupport` is not yet implemented in the active command surface.

Current routing note:
- family commands are resolved by `FamilyCoreCommandResolver` inside `Zongzu.Modules.FamilyCore`
- `PlayerCommandService` remains thin module-selection glue for this slice and must not own family consequence formulas
- the resolver may read `PersonRegistry` and `SocialMemoryAndRelations` query snapshots, but may mutate only `FamilyCore` state and receipt fields
- for public-life refusal response, the resolver may also read `OrderAndBanditry` refusal/partial residue through queries; v8 monthly family actor countermoves may read structured `SocialMemoryAndRelations` response residue. Both paths may mutate only `FamilyCore` response trace and family pressure fields, not Order or SocialMemory.

### Emits events
- `MarriageAllianceArranged`
- `BirthRegistered`
- `ClanMemberDied` (FamilyCore reports death of clan members only; other modules report death in their own domain via domain-specific events such as `DeathByIllness`, `DeathByViolence`; PersonRegistry consolidates into `PersonDeceased`)
- `HeirSecurityWeakened`
- `LineageDisputeHardened`
- `LineageMediationOpened`
- `BranchSeparationApproved`

### Does not own
- person identity anchors (owned by PersonRegistry)
- life stage or alive/dead status (owned by PersonRegistry)
- literacy, martial ability, commercial sense, health resilience (owned by respective domain modules; see `PERSON_OWNERSHIP_RULES.md`)
- social position labels (computed by projection layer)
- person activity (owned by PopulationAndHouseholds)
- exam progress
- shop inventories
- official appointments
- bandit camp state

## 3. PopulationAndHouseholds
### Owns
- commoner households
- household membership per PersonId (which person belongs to which household)
- livelihood type per household member (Smallholder / Tenant / HiredLabor / Artisan / PettyTrader / Boatman / DomesticServant / YamenRunner / SeasonalMigrant / Vagrant)
- health resilience per household member (the `healthResilience` ability value)
- health status per household member (Healthy / Ill / Injured / Disabled / Dying + illnessMonths)
- person activity per household member (Idle / Farming / Migrating / Laboring / ...)
- labor pools
- tenant/farmhand/shophand states
- migration pressure
- household livelihood pressure
- home-household local response traces for public-life/order after-accounts, including `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`
- summary pools: labor pool, marriage pool (population statistics), migration pool

### Public queries
- labor supply
- commoner distress
- militia/levy potential
- migration risk
- household pressure summaries
- projected ordinary-household public-life/order after-account readback may join household pressure with Order / Office / Family traces, but this remains an Application read-model projection and does not make `PopulationAndHouseholds` own order commands or response traces
- projected ordinary-household public-life/order response choice text may join that same pressure with existing public-life command affordances / receipts, but this remains Application read-model enrichment and does not add household command ownership or a household target to command requests
- projected home-household local response aftermath fields for readback: `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, `LastLocalResponseTraceCode`, `LastLocalResponseSummary`, and `LocalResponseCarryoverMonths`
- household membership per PersonId
- health resilience and health status per PersonId
- person activity per PersonId
- marriage pool statistics per settlement (eligible counts, match difficulty)

### Accepts commands
- hire labor
- adjust tenancy burden
- relief/support where allowed
- `RestrictNightTravel` /暂缩夜行 for local household road-risk avoidance
- `PoolRunnerCompensation` /凑钱赔脚户 for local porter/runner misread containment
- `SendHouseholdRoadMessage` /遣少丁递信 for household-scoped road-message clarification

Current routing note:
- these home-household local response commands are resolved by `PopulationAndHouseholdsCommandResolver` inside `Zongzu.Modules.PopulationAndHouseholds`
- the resolver may select an affected household from settlement / optional clan scope, but it may mutate only population-owned household labor, debt, distress, migration, and local response trace fields
- it does not repair `OrderAndBanditry` refusal authority, `OfficeAndCareer` yamen/document landing, `FamilyCore` elder explanation, or `SocialMemoryAndRelations` durable residue
- v14 repeat friction may read structured `SocialMemoryEntrySnapshot` cause keys and weights for the same household through `ISocialMemoryAndRelationsQueries`, but it still mutates only population-owned household state and must not parse memory summaries or write SocialMemory state
- v15 common-household response texture may derive local command cost and outcome pressure from existing population-owned household fields such as debt, labor, distress, migration risk, dependents, laborers, and livelihood. This remains `PopulationAndHouseholds` command-time logic and adds no new household target, schema field, or foreign authority.
- v16 response capacity affordance may project `回应承受线` and may add command-time capacity summary tails from those same existing household fields. It does not add persisted capacity state, does not change command targeting, and does not let Application / UI / Unity compute final local response outcomes.
- v17 response tradeoff forecast may project `取舍预判` / `预期收益` / `反噬尾巴` / `外部后账` and may add command-time tradeoff summary tails from those same existing household fields. It does not add persisted tradeoff state, does not change command targeting, and does not let Application / UI / Unity compute final local response outcomes.
- v18 short-term consequence readback may project receipt-side `短期后果：缓住项` / `挤压项` / `仍欠外部后账` from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponse*` codes. It does not add persisted consequence state, does not change command targeting, and does not let Application / UI / Unity compute final local response outcomes.
- v19 follow-up affordance readback may project `续接提示` / `换招提示` / `冷却提示` / `续接读回` from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponse*` codes. It does not add a cooldown ledger, repeated-response counter, persisted consequence state, or command targeting change.

### Emits events
- `HouseholdDebtSpiked`
- `MigrationStarted`
- `TenantFlight`
- `LaborShortage`
- `LivelihoodCollapsed`
- `DeathByIllness` (health-owned death cause; PersonRegistry listens to consolidate into `PersonDeceased`)

### Does not own
- clan lineage
- school rank
- trade route ownership
- official careers
- order repair, county-yamen催办, elder explanation, durable social-memory residue, or public-life rumor authority

## 4. SocialMemoryAndRelations
### Owns
- relationship edges
- favor/debt/shame/fear records
- memory records
- clan narrative promotion
- grudge escalation state
- clan emotional climate: fear, shame, grief, anger, obligation, hope, trust, restraint, hardening, bitterness, volatility
- person pressure tempering for clan-linked adults, keyed by `PersonId`

### Public queries
- relation summaries
- grudge pressure
- obligation/favor summaries
- public vs private memory projections
- clan emotional climate snapshots
- person pressure-tempering snapshots by person or clan

### Accepts commands
- apologize
- compensate
- restrain retaliation
- publicly honor or shame
- mediation choices

### Emits events
- `GrudgeEscalated`
- `GrudgeSoftened`
- `FavorIncurred`
- `DebtOfHonorCreated`
- `ClanNarrativeUpdated`
- `SocialMemoryAndRelations.EmotionalPressureShifted`
- `SocialMemoryAndRelations.PressureTempered`

### Upstream reads / event inputs
- reads `FamilyCore` clan pressure and personality traits through `IFamilyCoreQueries`
- reads sponsored household pressure through `IPopulationAndHouseholdsQueries`
- reads optional clan trade pressure through `ITradeAndIndustryQueries`
- reads optional `OrderAndBanditry` public-life order aftermath through `IOrderAndBanditryQueries` when turning recent accepted, partial, or refused `添雇巡丁`, `严缉路匪`, or related order carryover into owner-owned obligation, fear, shame, favor, or grudge residue
- reads structured public-life refusal response aftermath through `IOrderAndBanditryQueries`, optional `IOfficeAndCareerQueries`, and optional `IFamilyCoreQueries`; it may use response command / outcome / trace codes, but must not parse `DomainEvent.Summary`, receipt summaries, or `LastInterventionSummary`
- later-month public-life response residue drift is SocialMemory-owned: it may adjust only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`, reusing existing schema `3` fields such as memory weight, cause key, lifecycle state, narrative pressures, and climate axes
- reads structured home-household local response aftermath through `IPopulationAndHouseholdsQueries`; it may use `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`, but must not parse `LastLocalResponseSummary` or treat a household local response as county order / yamen / family repair
- v8 owner-module actor countermoves may read SocialMemory snapshots, but `SocialMemoryAndRelations` still owns only durable residue. It does not resolve route-watch, yamen, clerk, elder, or household-guarantee countermoves.
- consumes scoped trade shock, exam, death, marriage, branch, heir, and warfare events to mutate only its own climate, memory, narrative, and tempering state

### Does not own
- direct conflict resolution
- exam or trade state
- office appointments
- household distress, market price, education progress, family lineage, force posture, public-life heat, or order carryover state
- public-life refusal response command resolution or response authority traces

## 5. EducationAndExams
### Owns
- study state
- academy/school attendance state
- tutor/school relationships in this domain
- exam attempts and outcomes

### Public queries
- study progress
- exam eligibility
- school capacity
- scholarly reputation projection

### Accepts commands
- fund study
- hire tutor
- redirect educational support
- withdraw from study

### Emits events
- `ExamPassed`
- `ExamFailed`
- `StudyAbandoned`
- `TutorSecured`

### Does not own
- family prestige internals
- office rank internals
- wealth ledgers

## 6. TradeAndIndustry
### Owns
- estates, shops, caravans, route commitments
- inventories/cashflow/debt in this domain
- local trade relations
- business obligations
- gray-route / illicit ledgers
- shadow-price, diversion share, blocked-shipment, and seizure-risk summaries for trade-owned settlements
- active-route blocked-shipment / seizure mirrors plus route-constraint labels and traces

### Public queries
- asset summaries
- route dependency
- cashflow pressure
- market position projections
- gray-route ledger summaries
- diversion-band / seizure-risk / shadow-price projections
- route-level blockage / seizure / constraint summaries

### Accepts commands
- open/close shop
- expand/cut trade route
- borrow/invest
- appoint manager
- convoy/hire escort later

### Emits events
- `TradeProspered`
- `TradeLossOccurred`
- `TradeDebtDefaulted`
- `CaravanDelayed`
- `RouteBusinessBlocked`

### Does not own
- marriages
- exam outcomes
- office assignments
- bandit camp internals

## 7. OfficeAndCareer
### Owns
- appointments
- candidate-queue / waiting pressure before appointment
- clerk / yamen dependence inside the office pathway
- office authority
- career track status
- promotion / demotion pressure
- administrative task assignment
- petition backlog / petition outcomes
- jurisdiction-level clerk dependence and administrative task load
- clerk-capture edge watermarks for office-owned escalation receipts
- official defection risk before office-owned appointment loss
- office-owned public-life refusal response trace for county-yamen催办, 文移落地, and 胥吏拖延 outcomes
- official influence projections

### Public queries
- office authority tier
- appointment status
- current appointment pressure
- current clerk dependence
- current jurisdictional leverage
- current petition pressure
- current petition backlog
- current administrative task tier and stable task label
- petition outcome category plus latest petition outcome trace
- promotion / demotion pressure labels and authority-trajectory summary
- current official defection risk when governance-lite exposes regime pressure
- office-owned public-life refusal response aftermath fields for governance docket readback

### Accepts commands
- pursue posting
- resign/refuse
- petition via office channels
- deploy legal/administrative leverage where allowed
- post county notice
- dispatch road report
- `PressCountyYamenDocument` /押文催县门 for county-yamen landing or clerk-delay escalation
- `RedirectRoadReport` /改走递报 for bounded document-route repair

Current routing note:
- these commands are resolved by `OfficeAndCareerCommandResolver` inside `Zongzu.Modules.OfficeAndCareer`
- office public-life verbs may update only office-owned jurisdiction, petition, and trace state; order, family, trade, or public-life heat must move later through queries, events, or projections
- public-life refusal response commands may read `OrderAndBanditry` structured residue through queries, and v7/v8 repeat-friction or actor countermove logic may read `FamilyCore` local clan scope plus `SocialMemoryAndRelations` response residue weights, but the response outcome and trace they write are owned by `OfficeAndCareer`

### Emits events
- `OfficeGranted`
- `OfficeLost`
- `OfficeTransfer`
- `AuthorityChanged`
- `YamenOverloaded`
- `AmnestyApplied`
- `OfficialSupplyRequisition`
- `ClerkCaptureDeepened`
- `PolicyWindowOpened`
- `OfficeDefected`

### Does not own
- school results
- family tree
- trade ledgers
- local disorder pressure
- local force pools
- war battle plans
- foreign order state, black-route pressure, or intervention carryover directly
- family or social-memory residue, including shame/fear/favor/grudge/obligation records

## 8. OrderAndBanditry
### Owns
- security pressure beyond baseline settlement state
- outlaw/bandit pathways and camps
- black-route pressure
- suppression / recruitment / disorder escalation state
- coercion-risk, suppression-relief, response-activation mirrors, paper-compliance visibility, implementation drag, route-shielding summaries, retaliation-risk summaries, administrative suppression windows, escalation bands, intervention-receipt traces, order-owned refusal response traces, and v8 route-watch / runner actor countermove traces

### Public queries
- bandit threat
- outlaw route pressure
- black-route pressure summaries for trade-risk handoff
- paper-compliance, implementation-drag, route-shielding, and retaliation-risk summaries for office/trade read models
- black-route suppression-window and escalation-band summaries
- suppression demand
- local disorder projections
- last intervention command / label / summary / outcome plus structured outcome/refusal/partial/trace codes for public-life read models
- structured intervention aftermath such as black-route pressure, coercion risk, implementation drag, route shielding, and retaliation risk for next-month readback by office, trade, social-memory, and presentation projections
- order-owned response aftermath fields (`LastRefusalResponseCommandCode`, `LastRefusalResponseOutcomeCode`, `LastRefusalResponseTraceCode`, `ResponseCarryoverMonths`) for Month N+2 SocialMemory reads and shell readback

### Accepts commands
- `EscortRoadReport` /催护一路 for limited route-report and travel protection
- `FundLocalWatch` /添雇巡丁
- `SuppressBanditry` /严缉路匪
- `NegotiateWithOutlaws` /遣人议路 in limited local cases
- `TolerateDisorder` /暂缓穷追 at cost
- `RepairLocalWatchGuarantee` /补保巡丁 for road-watch guarantee repair
- `CompensateRunnerMisread` /赔脚户误读 for runner/carrier misread repair
- `DeferHardPressure` /暂缓强压 for route-pressure containment

Current routing note:
- these public-life order commands are currently routed by `PlayerCommandService`, but resolution lives in `OrderAndBanditryModule.HandlePublicLifeCommand`
- the application layer may pass query-derived office-reach modifiers into that resolver; it must not write office, trade, public-life, force, or family state directly

Current routing note:
- these commands are resolved by `OrderAndBanditryCommandResolver` inside `Zongzu.Modules.OrderAndBanditry`
- the resolver may read `OfficeAndCareer` jurisdiction authority through queries to shape administrative reach, but it may mutate only order-owned pressure, carryover, and receipt state

### Emits events
- `BanditThreatRaised`
- `OutlawGroupFormed`
- `SuppressionSucceeded`
- `RouteUnsafeDueToBanditry`

### Does not own
- family lineage
- authoritative trade balances
- force pools
- campaign maps
- county-yamen催办, 文移落地, or 胥吏拖延 response traces owned by `OfficeAndCareer`
- clan explanation / home-household guarantee response traces owned by `FamilyCore`
- durable shame/fear/favor/grudge/obligation residue owned by `SocialMemoryAndRelations`

## 9. ConflictAndForce
### Owns
- local force pools: retainers, guards, clan militia, escorts
- local conflict resolution
- injuries and losses tied to conflict
- force-readiness, command capacity basics, supply basics for local conflict
- persistent campaign-fatigue, escort-strain, and campaign-fallout traces for local force recovery

### Public queries
- available force pools
- readiness
- command capacity projection
- explicit response activation / order-support projection
- local conflict traces

### Accepts commands
- hire guards
- mobilize clan militia where permitted
- prepare convoy/escort
- suppress or restrain local retaliation

### Emits events
- `ConflictResolved`
- `CommanderWounded`
- `DeathByViolence` (conflict-owned death cause; PersonRegistry listens to consolidate into `PersonDeceased`)
- `ForceReadinessChanged`
- `MilitiaMobilized`

### Does not own
- war campaigns
- office authority internals
- clan prestige internals

## 10. WarfareCampaign
### Owns
- campaign boards
- bounded front pressure, supply state, and morale state in campaign scope
- mobilization signals derived into campaign-owned state
- campaign aftermath summaries and campaign-board labeling
- command-fit wording, commander summaries, and bounded route descriptors for the campaign board
- campaign-intent descriptors such as active directive code/label/summary and last directive trace

### Public queries
- campaign status
- route and front summaries
- anchor settlement and campaign-board summary
- mobilization signals derived from upstream local-force posture
- mobilization window labels, command-fit summaries, directive summaries, and office-coordination trace summaries
- order-support, office-authority-tier, administrative-leverage, and petition-backlog precursors
- supply state
- campaign aftermath summaries

### Accepts commands
- draft campaign plan
- commit mobilization
- protect supply line
- withdraw to barracks

Current routing note:
- these commands are resolved by `WarfareCampaignCommandResolver` inside `Zongzu.Modules.WarfareCampaign`
- the resolver may write only `WarfareCampaign`-owned directive state; it may not mutate `ConflictAndForce`, `OfficeAndCareer`, or settlement state directly

### Emits events
- `CampaignMobilized`
- `CampaignPressureRaised`
- `CampaignSupplyStrained`
- `CampaignAftermathRegistered`

Current lite note:
- warfare events now carry settlement-targeting metadata so downstream handlers can update only their own state without parsing narrative strings

### Does not own
- family tree
- trade ledgers
- grievance internals
- direct settlement baseline data

## 11. PublicLifeAndRumor
### Owns
- settlement-level public pulse
- street-talk heat
- market bustle / county-gate crowding
- posted-notice visibility
- road-report lag
- prefecture-dispatch pressure
- public-legitimacy and dominant-venue summaries
- monthly cadence / crowd-mix descriptors for public spaces
- documentary weight / verification cost / market-rumor flow / courier risk
- venue-channel competition summaries for county gates, market streets, ferries, inns, and road nodes
- official-notice / street-talk / road-report / prefecture-dispatch wording
- contention summaries that explain where declared order and lived order pull apart

### Public queries
- public-life snapshots by settlement
- street-talk / market-buzz / notice-visibility summaries
- county-gate / road-report / prefecture-pressure summaries
- dominant venue labels and public-trace summaries
- monthly cadence labels and cadence summaries for hall / desk read models
- venue-channel summaries and channel-pressure metrics for hall / desk read models
- explicit channel-line wording for notice, street talk, road report, prefecture pressure, and contention

### Accepts commands
- none authoritative in the current lite slice
- public-life response verbs such as `张榜晓谕`, `遣吏催报`, `催护一路`, `添雇巡丁`, `严缉路匪`, `遣人议路`, `暂缓穷追`, and `请族老出面` must still route through `OfficeAndCareer`, `OrderAndBanditry`, or `FamilyCore`; `PublicLifeAndRumor` may project them but must not own or resolve them

### Emits events
- `StreetTalkSurged`
- `CountyGateCrowded`
- `MarketBuzzRaised`
- `RoadReportDelayed`
- `PrefectureDispatchPressed`

### Does not own
- household debt or labor
- trade ledgers or route ownership
- office appointments or petition state
- force posture or campaign state
- clan memory internals

## 12. NarrativeProjection
### Owns
- notifications
- letters
- rumors
- reports
- council agenda surfaces
- explanation formatting

### Public queries
- current notification sets
- trace/explanation entries
- report projections

### Accepts commands
- mark read/unread
- pin/watch items
- filter/report preferences

### Emits events
- none authoritative for simulation

### Does not own
- any authoritative world state

## Dependency guidance
- all modules may query `PersonRegistry` for identity, life stage, alive status, and fidelity ring
- `FamilyCore` may query `PersonRegistry` and `SocialMemoryAndRelations`; it may call the narrow `PersonRegistry` identity command surface for person creation/death, but must not directly mutate foreign module state
- `TradeAndIndustry` may query `WorldSettlements` and `OrderAndBanditry`
- `OrderAndBanditry` may query `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, `TradeAndIndustry`, `OfficeAndCareer`, and `ConflictAndForce`
- `ConflictAndForce` may query `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, and `TradeAndIndustry`
- `OfficeAndCareer` may query `EducationAndExams`, `SocialMemoryAndRelations`, optional `OrderAndBanditry`, and `FamilyCore` when it needs local clan scope for structured public-life response friction or v8 yamen / clerk actor countermoves
- `WarfareCampaign` may query `ConflictAndForce`, `WorldSettlements`, `OfficeAndCareer`
- `PublicLifeAndRumor` may query `WorldSettlements`, `PopulationAndHouseholds`, `TradeAndIndustry`, `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations`
- `TradeAndIndustry`, `OrderAndBanditry`, `OfficeAndCareer`, and `SocialMemoryAndRelations` may react to settlement-targeted `WarfareCampaign` events during the handler pass, but only by updating their own owned state
- `ConflictAndForce` may also react to settlement-targeted `WarfareCampaign` aftermath events, but only by updating its own fatigue, escort strain, readiness, command-capacity, and fallout-trace state
- `PopulationAndHouseholds` may react to settlement-targeted `WarfareCampaign` aftermath events, but only by updating household distress, debt, labor, migration, and rebuilt settlement summaries in its own namespace
- `WorldSettlements` may react to settlement-targeted `WarfareCampaign` aftermath events, but only by updating settlement security/prosperity inside its own namespace
- `FamilyCore` may react to settlement-targeted `WarfareCampaign` aftermath events, but only by updating clan prestige/support inside its own namespace
- `PublicLifeAndRumor` may also react to settlement-targeted `WarfareCampaign` aftermath events, but only by updating its own public-pulse summaries and public-trace state
- black-route depth must stay split across `OrderAndBanditry` pressure and `TradeAndIndustry` ledgers; it must not grow a detached module namespace
- no module is allowed to "just update" another module's internal data

## 2026-04-24 playable closure v2 note
- `chain1-public-life-order-v2` keeps public-life order resolution owned by `OrderAndBanditry`; `PublicLifeAndRumor` supplies visibility, `OfficeAndCareer` supplies optional reach/readback through queries, and Unity/presentation surfaces remain projection-only.
- The playable-thin chain uses existing `SettlementDisorderState.LastIntervention*` and `InterventionCarryoverMonths` state for receipt/residue/readback; no new module namespace, manager, or save/schema field is introduced.
- Disabled or absent `OrderAndBanditry` must refuse public-life order commands safely instead of creating a fallback authority path in Application or UI.

## 2026-04-24 playable closure v3 leverage note
- `chain1-public-life-order-leverage-v3` adds runtime-only leverage / cost / readback projections for the same public-life order lane. The projection may join existing public-life, order, office, family, social-memory, and trade snapshots, but it must not write any module state.
- `OrderAndBanditry` remains the only owner of public-life order command resolution and intervention receipt/carryover state. Family, office, trade, and social-memory context is explanatory readback, not a same-command write path.
- Durable obligation, favor, shame, fear, or grudge records still belong to `SocialMemoryAndRelations`; v3 does not add a shortcut residue store in Application, Unity, or `PersonRegistry`.

## 2026-04-25 playable closure v4 social-memory residue note
- `public-life-order-social-memory-residue-v4` keeps public-life order command resolution inside `OrderAndBanditry`, then applies a rule-driven SocialMemory monthly residue rule over structured next-month order aftermath.
- `OrderAndBanditry` still mutates only order-owned settlement pressure, receipt, and carryover state. It does not write memory, favor, shame, fear, obligation, or grudge records.
- `SocialMemoryAndRelations` may create public-order residue only inside its own `Memories`, `ClanNarratives`, and `ClanEmotionalClimates` state. It must use query-visible order fields or structured metadata, never `DomainEvent.Summary` parsing.
- Application, presentation, and Unity may expose `社会记忆读回` from projected SocialMemory read models only; they must not author, save, or repair social memory state.

## 2026-04-25 playable closure v5 refusal-residue note
- `public-life-order-refusal-residue-v5` keeps the same rule-driven command / aftermath / social-memory readback loop and explicitly rejects event-chain or event-pool design language.
- `OrderAndBanditry` now owns structured `accepted`, `partial`, and `refused` authority trace fields for public-life/order commands, including refusal and partial reason codes plus refusal carryover.
- `SocialMemoryAndRelations` consumes those structured Order query fields on the next monthly pass and may write only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`; it must not parse `DomainEvent.Summary`, `LastInterventionSummary`, or receipt prose.
- Application, governance read models, public-life receipts, family-facing SocialMemory readback, and Unity shell surfaces may only copy projected refusal/partial residue such as `县门未落地`, `地方拖延`, and `后账仍在`.

## 2026-04-25 playable closure v7 residue-decay / repeat-friction note
- `public-life-order-residue-decay-friction-v7` keeps the response afterlife inside the rule-driven command / residue / social-memory / response loop; it is not an event-pool or event-centered authority path.
- `SocialMemoryAndRelations` owns later-month softening or hardening of public-life response residue by adjusting only existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` may read structured response memories through `ISocialMemoryAndRelationsQueries` and local clan scope through `IFamilyCoreQueries` where needed, then mutate only their own command/pressure/trace state.
- No module may parse social-memory summary prose, receipt prose, `LastRefusalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary` to compute repeat friction.
- v7 adds no persisted fields; SocialMemory remains schema `3`, with owner response traces still held by the v6 owning modules.

## 2026-04-25 playable closure v8 actor-countermove / passive back-pressure note
- `public-life-order-actor-countermove-v8` adds small deterministic monthly actor movement after response residue exists; it remains part of the rule-driven command / residue / social-memory / response loop, not an event-pool, event-chain, or autonomous-manager design.
- `OrderAndBanditry` owns route-watch and runner countermoves such as `巡丁自补保` or `脚户误读反噬`; `OfficeAndCareer` owns yamen and clerk countermoves such as `县门自补落地` or `胥吏续拖`; `FamilyCore` owns elder and household-guarantee countermoves such as `族老自解释` or `族老避羞`.
- Those modules may read structured `SocialMemoryEntrySnapshot.CauseKey`, `Weight`, `State`, `SourceClanId`, and `OriginDate`, skip current-month response memories, and mutate only their own existing pressure and response trace fields.
- `SocialMemoryAndRelations` does not resolve actor countermoves and does not write Order, Office, Family, PublicLife, Governance, Population, or PersonRegistry state. It may later read structured owner aftermath and adjust only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- v8 adds no persisted fields, schema bump, migration, manager/controller layer, or `PersonRegistry` expansion.

## 2026-04-25 playable closure v10 ordinary-household readback note
- `public-life-order-ordinary-household-readback-v10` keeps ordinary households in the same rule-driven command / residue / social-memory / response / readback loop without adding an event pool or new command owner.
- `PopulationAndHouseholds` still owns commoner household pressure state only. It does not own `添雇巡丁`, `严缉路匪`, response repair, yamen催办, elder explanation, or durable SocialMemory residue.
- Application projections may join `HouseholdPressureSnapshot` with structured `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` aftermath snapshots to expose `PublicLifeOrderResidue` household pressure readback.
- Unity and shell adapters may copy that projected household readback into Desk Sandbox settlement pressure, but they must not query modules, parse summaries, compute effectiveness, mutate household state, or create a new household-control surface.
- v10 adds runtime read-model constants only and no persisted fields, schema bump, migration, or `PersonRegistry` expansion.

## 2026-04-25 playable closure v11 ordinary-household play-surface note
- `public-life-order-ordinary-household-play-surface-v11` makes the v10 household readback playable by attaching visible household stakes to existing response affordances and receipts.
- `PopulationAndHouseholds` remains the ordinary-household state owner. It still does not resolve order repair, county-yamen催办, family explanation, response traces, or social residue.
- Application projections may select a visible `HouseholdSocialPressureSignalKeys.PublicLifeOrderResidue` household and append that household's stakes to `PlayerCommandAffordanceSnapshot` / `PlayerCommandReceiptSnapshot` leverage, cost, execution, and readback text.
- The selected household is a projected stake, not an authoritative command target. `PlayerCommandRequest` remains settlement / optional clan scoped, and owning modules still resolve the response.
- v11 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, or household-control surface.

## 2026-04-25 playable closure v12 home-household local response note
- `public-life-order-home-household-local-response-v12` turns the ordinary-household stake into a first low-power home-household command surface without making commoner households own county order repair.
- `PopulationAndHouseholds` now owns only local household-seat responses: `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`. These commands can shift household labor, debt, distress, migration risk, and local response trace fields.
- `OrderAndBanditry` still owns refused / partial order authority and route-watch repair; `OfficeAndCareer` still owns county-yamen/document/clerk response; `FamilyCore` still owns elder explanation and household guarantee; `SocialMemoryAndRelations` still owns durable shame/fear/favor/grudge/obligation residue.
- Application projections may expose bounded home-household affordances only from projected read models. UI and Unity may copy the resulting projected affordance / receipt fields only, and must not query modules or compute local response effectiveness.
- v12 bumps `PopulationAndHouseholds` schema from `2` to `3`, adds same-namespace migration and save roundtrip proof, and adds no `PersonRegistry` expansion, manager/controller layer, or `HouseholdId` command target.

## 2026-04-25 playable closure v13 home-household social-memory readback note
- `public-life-order-home-household-social-memory-v13` keeps the same rule-driven command / residue / social-memory / response loop and adds only the later SocialMemory readback layer for v12 local responses.
- `SocialMemoryAndRelations` reads structured `PopulationAndHouseholds` local-response aftermath and writes only existing `Memories`, `ClanNarratives`, and `ClanEmotionalClimates` records. It does not resolve `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信`, and it does not repair order, yamen, family, public-life, population, or registry state.
- Application may join the resulting `SocialMemoryEntrySnapshot` onto home-household local response receipts as projected readback. Unity and shell adapters may copy that receipt text only; they must not compute durable residue, query modules, or parse summaries.
- v13 adds only additive `SocialMemoryKinds` constants and no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, or command-target shape change.

## 2026-04-25 playable closure v14 home-household repeat-friction note
- `public-life-order-home-household-repeat-friction-v14` lets later `PopulationAndHouseholds` local response commands read existing v13 SocialMemory residue as small local repeat friction.
- The reader uses only active `SocialMemoryEntrySnapshot` cause keys under `order.public_life.household_response.{HouseholdId}.`, structured outcome markers, and weights. It must not parse memory summary prose, command receipt prose, `LastLocalResponseSummary`, `DomainEvent.Summary`, `LastInterventionSummary`, or `LastRefusalResponseSummary`.
- `PopulationAndHouseholds` remains the command-time owner and may mutate only household labor, debt, distress, migration, and local response trace fields. It still does not repair order, yamen, family, public-life, registry, or SocialMemory state.
- v14 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, or command-target shape change.

## 2026-04-25 playable closure v15 common-household response texture note
- `public-life-order-common-household-response-texture-v15` keeps the v12-v14 home-household local response lane thin but makes ordinary household state matter in play.
- `PopulationAndHouseholds` derives a bounded texture profile from existing fields (`DebtPressure`, `LaborCapacity`, `Distress`, `MigrationRisk`, `DependentCount`, `LaborerCount`, and `Livelihood`) when resolving `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信`.
- The profile may make debt-heavy compensation more costly, labor-thin night restriction / road messaging more fragile, distress-heavy pressure more socially brittle, and migration-prone night restriction more useful. It still writes only population-owned household pressure and local response trace fields.
- Application projections may display `本户底色` hints from `HouseholdPressureSnapshot`, and Unity may copy those projected affordance / receipt fields only. UI and Unity must not compute final local response effectiveness.
- v15 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, or command-target shape change.

## 2026-04-25 playable closure v16 home-household response capacity note
- `public-life-order-home-household-response-capacity-v16` adds a projected `回应承受线` to the existing home-household local response lane.
- The capacity line is derived only from existing household read-model fields and existing command-time household state: debt pressure, labor capacity, distress, migration risk, dependents, laborers, and livelihood. It is not a new saved ledger.
- `PopulationAndHouseholds` may use the same existing fields to mark a forced local response as `Strained` and append a capacity summary tail, but it still writes only household labor, debt, distress, migration, and local response trace fields.
- Application projections may show whether `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信` is bearable, risky, or currently unfit. UI and Unity may copy those projected fields only and must not compute command outcome, query modules, or write SocialMemory.
- v16 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, or command-target shape change.

## 2026-04-25 playable closure v17 home-household response tradeoff forecast note
- `public-life-order-home-household-response-tradeoff-v17` adds projected `取舍预判` to the same home-household local response lane.
- The tradeoff forecast is derived only from existing household read-model fields and existing command-time household state: debt pressure, labor capacity, distress, migration risk, dependents, laborers, and livelihood. It is not a new saved ledger.
- `PopulationAndHouseholds` may append command-time tradeoff summary tails for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`, but it still writes only household labor, debt, distress, migration, and local response trace fields.
- Application projections may show expected benefit, recoil tail, and external-afteraccount boundary so the player can distinguish migration avoidance, runner compensation, and road-message choices. UI and Unity may copy those projected fields only and must not compute command outcome, query modules, or write SocialMemory.
- v17 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, or command-target shape change.

## 2026-04-25 playable closure v18 home-household short-term consequence readback note
- `public-life-order-home-household-short-term-readback-v18` adds receipt-side projected readback after a local household response resolves.
- The readback is derived only from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`. It is not a new saved ledger.
- Receipts may show `短期后果：缓住项`, `短期后果：挤压项`, and `短期后果：仍欠外部后账` for `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`, so the player can see what the household locally eased, what it squeezed, and which order/yamen/family/social-memory after-account remains outside household authority.
- `SocialMemoryAndRelations` must not parse those receipt strings; its Month N+2 local response residue continues to read only structured `PopulationAndHouseholds` aftermath fields and write only `Memories`, `ClanNarratives`, and `ClanEmotionalClimates`.
- v18 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, or command-target shape change.

## 2026-04-25 playable closure v19 home-household follow-up affordance note
- `public-life-order-home-household-follow-up-affordance-v19` adds projected follow-up hints to the next home-household local response affordance surface.
- The hints are derived only from existing `HouseholdPressureSnapshot` fields and structured `LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, and `LastLocalResponseTraceCode`. They are not a saved cooldown ledger or repeated-response counter.
- Affordances may show `续接提示`, `换招提示`, `冷却提示`, and `续接读回` so the player can see whether repeating or switching `暂缩夜行`, `凑钱赔脚户`, or `遣少丁递信` would be light follow-up, risky overpressure, or a local switch that still leaves外部后账.
- `SocialMemoryAndRelations` must not parse those affordance strings; its Month N+2 local response residue continues to read only structured `PopulationAndHouseholds` aftermath fields.
- v19 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, or command-target shape change.
