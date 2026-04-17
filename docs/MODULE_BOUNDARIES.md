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

## 1. WorldSettlements
### Owns
- regions, settlements, roads, route conditions
- local prosperity/security/environment indicators
- institution registry and baseline condition

### Public queries
- settlement security
- route status
- institution availability
- local pressure indicators

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
- lineage graph
- marriage links and household branch membership
- inheritance state
- clan policies / house rules
- branch split/merge state

### Public queries
- heirs
- marriage eligibility
- branch relations
- clan prestige projection

### Accepts commands
- arrange marriage
- designate heir policy
- redistribute household support
- approve/suppress branch split actions where allowed

### Emits events
- `MarriageArranged`
- `BirthRegistered`
- `DeathRegistered`
- `BranchSplit`
- `HeirStatusChanged`

### Does not own
- exam progress
- shop inventories
- official appointments
- bandit camp state

## 3. PopulationAndHouseholds
### Owns
- commoner households
- labor pools
- tenant/farmhand/shophand states
- migration pressure
- household livelihood pressure

### Public queries
- labor supply
- commoner distress
- militia/levy potential
- migration risk
- household pressure summaries

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

### Public queries
- asset summaries
- route dependency
- cashflow pressure
- market position projections

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
- office authority
- career track status
- official influence projections

### Public queries
- office authority tier
- appointment status
- current jurisdictional leverage

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
- war battle plans

## 8. OrderAndBanditry
### Owns
- security pressure beyond baseline settlement state
- outlaw/bandit pathways and camps
- black-route pressure
- suppression / recruitment / disorder escalation state

### Public queries
- bandit threat
- outlaw route pressure
- suppression demand
- local disorder projections

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

### Public queries
- available force pools
- readiness
- command capacity projection
- local conflict traces

### Accepts commands
- hire guards
- mobilize clan militia where permitted
- prepare convoy/escort
- suppress or restrain local retaliation

### Emits events
- `ConflictResolved`
- `CommanderWounded`
- `ForceReadinessChanged`
- `MilitiaMobilized`

### Does not own
- war campaigns
- office authority internals
- clan prestige internals

## 10. WarfareCampaign
### Owns
- campaign boards
- fronts and routes
- mobilized campaign forces
- campaign plans, battle phases, supply lines, morale in campaign scope

### Public queries
- campaign status
- route and front summaries
- commander fit and supply state
- campaign aftermath summaries

### Accepts commands
- choose campaign objective
- assign commander
- set stance/strategy
- allocate supply
- authorize mobilization where permitted

### Emits events
- `CampaignStarted`
- `CampaignWon`
- `CampaignLost`
- `SupplyCollapsed`
- `CommanderKilled`
- `RegionDevastated`

### Does not own
- family tree
- trade ledgers
- grievance internals
- direct settlement baseline data

## 11. NarrativeProjection
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
- `FamilyCore` may query `SocialMemoryAndRelations`, not mutate it
- `TradeAndIndustry` may query `WorldSettlements` and `OrderAndBanditry`
- `OfficeAndCareer` may query `EducationAndExams` and `SocialMemoryAndRelations`
- `WarfareCampaign` may query `ConflictAndForce`, `WorldSettlements`, `OfficeAndCareer`
- no module is allowed to “just update” another module’s internal data
