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
- last family-command receipts that remain family-owned state
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

### Accepts commands
- `ArrangeMarriage`
- `DesignateHeirPolicy`
- `SupportSeniorBranch`
- `OrderFormalApology`
- `PermitBranchSeparation`
- `SuspendClanRelief`
- `InviteClanEldersMediation`
- `InviteClanEldersPubliclyBroker`

> Note: `RedistributeHouseholdSupport` is not yet implemented in the active command surface.

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
- summary pools: labor pool, marriage pool (population statistics), migration pool

### Public queries
- labor supply
- commoner distress
- militia/levy potential
- migration risk
- household pressure summaries
- household membership per PersonId
- health resilience and health status per PersonId
- person activity per PersonId
- marriage pool statistics per settlement (eligible counts, match difficulty)

### Accepts commands
- hire labor
- adjust tenancy burden
- relief/support where allowed

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

## 4. SocialMemoryAndRelations
### Owns
- relationship edges
- favor/debt/shame/fear records
- memory records
- clan narrative promotion
- grudge escalation state

### Public queries
- relation summaries
- grudge pressure
- obligation/favor summaries
- public vs private memory projections

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

### Does not own
- direct conflict resolution
- exam or trade state
- office appointments

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

### Accepts commands
- pursue posting
- resign/refuse
- petition via office channels
- deploy legal/administrative leverage where allowed

### Emits events
- `OfficeGranted`
- `OfficeLost`
- `OfficeTransfer`
- `AuthorityChanged`

### Does not own
- school results
- family tree
- trade ledgers
- local disorder pressure
- local force pools
- war battle plans
- foreign order state, black-route pressure, or intervention carryover directly

## 8. OrderAndBanditry
### Owns
- security pressure beyond baseline settlement state
- outlaw/bandit pathways and camps
- black-route pressure
- suppression / recruitment / disorder escalation state
- coercion-risk, suppression-relief, response-activation mirrors, paper-compliance visibility, implementation drag, route-shielding summaries, retaliation-risk summaries, administrative suppression windows, escalation bands, and intervention-receipt traces

### Public queries
- bandit threat
- outlaw route pressure
- black-route pressure summaries for trade-risk handoff
- paper-compliance, implementation-drag, route-shielding, and retaliation-risk summaries for office/trade read models
- black-route suppression-window and escalation-band summaries
- suppression demand
- local disorder projections
- last intervention command / label / summary / outcome for public-life read models

### Accepts commands
- fund local watch
- suppress
- negotiate in limited later cases
- tolerate/ignore at cost

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
- these commands are currently staged through a thin application-routed warfare-intent service
- the service may write only `WarfareCampaign`-owned directive state; it may not mutate `ConflictAndForce`, `OfficeAndCareer`, or settlement state directly

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
- `FamilyCore` may query `PersonRegistry` and `SocialMemoryAndRelations`, not mutate them
- `TradeAndIndustry` may query `WorldSettlements` and `OrderAndBanditry`
- `OrderAndBanditry` may query `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, `TradeAndIndustry`, `OfficeAndCareer`, and `ConflictAndForce`
- `ConflictAndForce` may query `WorldSettlements`, `PopulationAndHouseholds`, `FamilyCore`, `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, and `TradeAndIndustry`
- `OfficeAndCareer` may query `EducationAndExams`, `SocialMemoryAndRelations`, and optional `OrderAndBanditry`
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
