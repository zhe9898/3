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

## Court-policy suggested action guard v165-v172 boundary note

Chain 8 v165-v172 keeps v157-v164 docket guard ownership and adds only a projected suggested-action prompt guard:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the current policy window, county execution, document/report aftermath, and implementation posture.
- `PublicLifeAndRumor` owns notice/report public interpretation and street-reading texture.
- `SocialMemoryAndRelations` owns the durable `office.policy_local_response...` records that make the old reading visible.
- Application may assemble `建议动作防误读` from structured guard eligibility plus existing projected affordance/no-loop text only; it must not change command ranking or calculate policy success.
- Unity may copy governance, office, docket, desk, and great-hall projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/memory-pressure/public-reading/public-follow-up/docket/suggested-action ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

## Court-policy follow-up docket guard v157-v164 boundary note

Chain 8 v157-v164 keeps v149-v156 public follow-up ownership and adds only a projected docket/no-loop guard:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the current policy window, county execution, and document/report aftermath.
- `PublicLifeAndRumor` owns notice/report public interpretation and street-reading texture.
- `SocialMemoryAndRelations` owns the durable `office.policy_local_response...` records that make the old reading visible.
- Application may assemble `政策后手案牍防误读` from existing structured snapshots only.
- Unity may copy governance, office, docket, and desk projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/memory-pressure/public-reading/public-follow-up/docket ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

## Court-policy public follow-up cue v149-v156 boundary note

Chain 8 v149-v156 keeps v141-v148 public-reading ownership and adds only a projected follow-up cue:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the current policy window, county execution, and document/report aftermath.
- `PublicLifeAndRumor` owns notice/report public interpretation, street-reading texture, and the public meaning of whether to cool, lightly continue, change method, or wait for an opening.
- `SocialMemoryAndRelations` owns the durable `office.policy_local_response...` records that make the old reading visible.
- Application may assemble `政策公议后手提示` from existing structured snapshots only.
- Unity may copy public-life command and governance projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/memory-pressure/public-reading/public-follow-up ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

## Court-policy public-reading echo v141-v148 boundary note

Chain 8 v141-v148 keeps v133-v140 memory-pressure ownership and adds only a projected public-reading echo:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the current policy window and county document/report aftermath.
- `PublicLifeAndRumor` owns notice/report public interpretation and street-reading texture.
- `SocialMemoryAndRelations` owns the existing durable `office.policy_local_response...` records.
- Application may assemble `政策公议旧读回` from structured memory cause/type/weight and current Office/PublicLife snapshots; it must not calculate policy success or write state.
- Unity may copy public-life command and governance projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/memory-pressure/public-reading ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

## Court-policy memory-pressure readback v133-v140 boundary note

Chain 8 v133-v140 keeps v125-v132 durable residue ownership and adds only a projected old-residue pressure readback:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the current policy window and county document/report aftermath.
- `PublicLifeAndRumor` owns public interpretation, notice visibility, and street reading.
- `SocialMemoryAndRelations` owns the existing durable `office.policy_local_response...` records.
- Application may assemble `政策旧账回压读回` from structured memory cause/type/weight and current Office/PublicLife snapshots; it must not calculate policy success or write state.
- Unity may copy projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/memory-pressure ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

## Court-policy social-memory echo v125-v132 boundary note

Chain 8 v125-v132 keeps the same owner lanes while adding a delayed SocialMemory echo for court-policy local response:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns policy windows, county document/report command aftermath, and structured response fields such as `LastRefusalResponseCommandCode`, outcome, trace, and carryover.
- `PublicLifeAndRumor` owns public interpretation, notice visibility, and street reading.
- `SocialMemoryAndRelations` may write later durable residue from structured Office aftermath into existing memory/narrative/climate records.
- Application may route, assemble, and project structured memory cause/type/weight; it must not calculate policy success.
- Unity may copy projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/social-memory ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, receipt prose, public-life prose, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

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
- v24-v30 projected outcome/social-residue/follow-up/closure reading may explain family outcomes as `已修复`, `暂压留账`, `恶化转硬`, `放置未接`, later `社会余味读回`, `余味冷却提示` / `余味续接提示` / `余味换招提示`, `现有入口读法`, `后手收口读回`, and `闭环防回压`, but those words are readback only; 族老公开解释、本户担保修复、宗房脸面 handling remains `FamilyCore` authority.

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
- v20-v30 owner-lane return/status/outcome/social-residue/follow-up/closure readback may project `外部后账归位`, `承接入口`, `归口状态`, `归口后读法`, `社会余味读回`, `余味冷却提示` / `余味续接提示` / `余味换招提示`, `现有入口读法`, `后手收口读回`, and `闭环防回压` from existing household local response fields, existing owner-module response traces, and existing structured SocialMemory response residue. This does not make `PopulationAndHouseholds` own order repair, county-yamen催办, elder explanation, durable SocialMemory residue, or any owner-lane / receipt-status / outcome / follow-up ledger.

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
- v24-v30 `归口后读法` / `社会余味读回` / `余味冷却提示` / `余味续接提示` / `余味换招提示` / `现有入口读法` / `后手收口读回` / `闭环防回压` remain projection text. `SocialMemoryAndRelations` must read structured Order / Office / Family response aftermath and home-household aftermath through queries, not parse `归口后读法`, `社会余味读回`, `余味冷却提示`, `余味续接提示`, `余味换招提示`, `现有入口读法`, `后手收口读回`, `闭环防回压`, `已修复：先停本户加压`, `暂压留账`, `恶化转硬`, or `放置未接` prose.
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
- policy-window implementation drag outcome, including rapid, dragged, captured, or paper-compliance resolution through existing office/yamen fields
- county/yamen execution side of the court-policy process after a policy window opens
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
- current policy implementation aftermath only after `OfficeAndCareer` has written its own task/backlog/clerk state
- policy-window and county-execution signals for court-policy process readback
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
- v24-v30 projected outcome/social-residue/follow-up/closure reading may explain office outcomes as `已修复`, `暂压留账`, `恶化转硬`, `放置未接`, later `社会余味读回`, `余味冷却提示` / `余味续接提示` / `余味换招提示`, `现有入口读法`, `后手收口读回`, and `闭环防回压`, but those words are readback only; county-yamen催办, 文移落地, and 胥吏拖延 handling remains `OfficeAndCareer` authority.

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
- `PolicyImplemented`
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
- court-policy wording state, county workflow ledgers, policy implementation ledgers, or UI-computed yamen effectiveness

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
- v24-v30 projected outcome/social-residue/follow-up/closure reading may explain order outcomes as `已修复`, `暂压留账`, `恶化转硬`, `放置未接`, later `社会余味读回`, `余味冷却提示` / `余味续接提示` / `余味换招提示`, `现有入口读法`, `后手收口读回`, and `闭环防回压`, but those words are readback only; road watch, route-pressure repair, runner misread repair, and patrol/order after-account handling remain `OrderAndBanditry` authority.

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
- public interpretation side of court-policy process readback, including notice visibility, dispatch pressure, documentary weight, verification cost, courier risk, and public legitimacy

### Public queries
- public-life snapshots by settlement
- street-talk / market-buzz / notice-visibility summaries
- county-gate / road-report / prefecture-pressure summaries
- dominant venue labels and public-trace summaries
- monthly cadence labels and cadence summaries for hall / desk read models
- venue-channel summaries and channel-pressure metrics for hall / desk read models
- explicit channel-line wording for notice, street talk, road report, prefecture pressure, and contention
- structured public-life channel metrics for court-policy process readback; consumers may read the fields, not parse the channel-line prose

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

## 2026-04-25 playable closure v20 owner-lane return guidance note
- `public-life-order-owner-lane-return-guidance-v20` adds projected `外部后账归位` guidance to home-household local response affordances and receipts.
- The guidance is derived only from existing projected household fields and structured `LastLocalResponse*` codes. It does not parse `LastLocalResponseSummary`, receipt prose, `DomainEvent.Summary`, or `LastInterventionSummary`.
- `OrderAndBanditry` still owns 巡丁/路匪/路面误读/route pressure repair; `OfficeAndCareer` still owns 县门未落地/文移拖延/胥吏续拖; `FamilyCore` still owns 族老解释/本户担保/宗房脸面; `SocialMemoryAndRelations` still owns durable shame/fear/favor/grudge/obligation residue.
- `PopulationAndHouseholds` remains only the low-power home-household response owner for labor, debt, distress, migration, and local response trace. Ordinary household response is not a universal repair line.
- UI and Unity may copy `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` projected fields only; they must not compute owner lanes or query modules.
- v20 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, owner-lane ledger, household target field, or command-target shape change.

## 2026-04-26 playable closure v21 owner-lane surface readback note
- `public-life-order-owner-lane-return-surface-readback-v21` carries v20 owner-lane return guidance into lane-facing read surfaces.
- Application projections may place `该走县门/文移 lane` on Office/Governance affordance and docket readback, and `该走族老/担保 lane` on Family-facing affordances, using only existing `HouseholdPressureSnapshot.LastLocalResponseCommandCode`, `LastLocalResponseOutcomeCode`, `LocalResponseCarryoverMonths`, settlement, and sponsor-clan fields.
- Public-life Order affordances may echo `该走巡丁/路匪 lane` when a local household response already exists, but the actual route-watch / road-bandit / route-pressure repair remains `OrderAndBanditry` authority.
- `PopulationAndHouseholds` still owns only the low-power household response trace. It does not become a repair owner for county order, yamen paperwork, clan elder explanation, or durable social-memory residue.
- UI and Unity remain copy-only. They may display the projected owner-lane guidance in public-life, Office/Governance, and Family surfaces, but must not compute owner-lane validity, query modules, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- v21 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, owner-lane ledger, household target field, or command-target shape change.

## 2026-04-26 playable closure v22 owner-lane handoff entry readback note
- `public-life-order-owner-lane-handoff-entry-readback-v22` adds projected `承接入口` wording to the v20-v21 owner-lane guidance.
- Application projections may name existing owner-lane affordance labels such as `添雇巡丁`, `押文催县门`, or `请族老解释`, but this is readback text only and does not create a new command system, queue, ranking authority, or command target.
- Ownership remains unchanged: `OrderAndBanditry` owns route-watch / road-bandit / route-pressure repair, `OfficeAndCareer` owns county-yamen / document / clerk drag, `FamilyCore` owns elder explanation / guarantee face, `PopulationAndHouseholds` owns only low-power household response traces, and `SocialMemoryAndRelations` writes durable residue only from structured aftermath.
- UI and Unity may copy `承接入口` from projected fields only. They must not compute owner lanes, query modules, write SocialMemory, maintain an owner-lane ledger, or invent a hidden household target.
- v22 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, owner-lane ledger, household target field, command queue, or command-target shape change.

## 2026-04-26 playable closure v23 owner-lane receipt status readback note
- `public-life-order-owner-lane-receipt-status-readback-v23` adds projected `归口状态` wording after a home-household local response when an existing owner lane already has a structured response trace.
- This is not "社会其他人接手": `已归口` means the after-account has returned to the owning lane's readback, not that repair is automatic or handled by a new actor system.
- Application projections may read `SettlementDisorderSnapshot.LastRefusalResponse*`, `JurisdictionAuthoritySnapshot.LastRefusalResponse*`, or `ClanSnapshot.LastRefusalResponse*` alongside existing `HouseholdPressureSnapshot.LastLocalResponse*` fields. They must not parse `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, or `DomainEvent.Summary`.
- Ownership remains unchanged: `OrderAndBanditry` owns route-watch / road-bandit / route-pressure response status, `OfficeAndCareer` owns county-yamen / document / clerk-drag response status, `FamilyCore` owns elder explanation / guarantee-face response status, and `SocialMemoryAndRelations` writes durable residue only from structured aftermath during its later pass.
- UI and Unity may copy `已归口到巡丁/路匪 lane`, `已归口到县门/文移 lane`, `已归口到族老/担保 lane`, `归口不等于修好`, and `仍看 owner lane 下月读回` from projected fields only. They must not compute归口, query modules, write SocialMemory, maintain a ledger, or invent a hidden household target.
- v23 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, owner-lane ledger, receipt-status ledger, household target field, command queue, or command-target shape change.

## 2026-04-26 playable closure v25 owner-lane social-residue readback note
- `public-life-order-owner-lane-social-residue-readback-v25` adds projected `社会余味读回` wording after the later monthly SocialMemory pass has made owner-lane response residue visible.
- Application projections may match existing structured SocialMemory response residue using `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, `OriginDate`, and current owner-lane command/outcome trace fields. They must not parse SocialMemory summary prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, receipt prose, `LastInterventionSummary`, or `DomainEvent.Summary`.
- Ownership remains unchanged: `SocialMemoryAndRelations` owns durable shame/fear/favor/grudge/obligation residue; `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` own their lane outcomes; `PopulationAndHouseholds` remains only the low-power home-household response owner and is not a universal repair line.
- UI and Unity may copy `社会余味读回`, `后账渐平`, `后账暂压留账`, `后账转硬`, `后账放置发酸`, and `不是本户再修` from projected fields only. They must not compute residue, query modules, write SocialMemory, maintain a ledger, or invent a hidden household target.
- v25 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, household target field, command queue, or command-target shape change.

## 2026-04-26 playable closure v26 owner-lane social-residue follow-up guidance note
- `public-life-order-owner-lane-social-residue-followup-v26` adds projected `余味冷却提示` / `余味续接提示` / `余味换招提示` wording after v25 `社会余味读回`.
- Application projections may derive the cue only from existing structured owner-lane outcome codes plus matching `SocialMemoryEntrySnapshot.CauseKey`, `State`, `Weight`, and `OriginDate`. They must not parse SocialMemory summary prose, owner-lane guidance prose, receipt prose, `LastRefusalResponseSummary`, `LastLocalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- Ownership remains unchanged: cooling, light continuation, tactic switch, or waiting are player-facing readback cues over existing owner lanes. `OrderAndBanditry`, `OfficeAndCareer`, and `FamilyCore` still own their commands and outcomes; `SocialMemoryAndRelations` still owns durable residue; `PopulationAndHouseholds` remains only the low-power home-household response lane.
- UI and Unity may copy `余味冷却提示`, `余味续接提示`, `余味换招提示`, `继续降温`, `别回压本户`, and `不要从本户硬补` from projected fields only. They must not compute follow-up validity, query modules, write SocialMemory, maintain a follow-up ledger, or invent a hidden household target.
- v26 adds no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, follow-up ledger, SocialMemory ledger, household target field, command queue, or command-target shape change.

## 2026-04-26 playable closure v27-v30 owner-lane closure audit note
- v27 `public-life-order-owner-lane-affordance-echo` adds projected `现有入口读法` over existing owner-lane affordances. It may say `建议冷却`, `可轻续`, `建议换招`, or `等待承接口`, but it does not enable, disable, rank, or route commands.
- v28 `public-life-order-owner-followup-receipt-closure` adds projected `后手收口读回` to owner-lane receipts. It may say `已收口`, `仍留账`, `转硬待换招`, or `未接待承口`, but it is not a persisted receipt-status ledger.
- v29 `public-life-order-owner-lane-no-loop-guard` adds projected `闭环防回压` so stale guidance does not point back at the home household after the owner lane has closed or hardened.
- v30 records that v20-v30 are one thin projection/readback closure arc. `OrderAndBanditry`, `OfficeAndCareer`, `FamilyCore`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` keep the same ownership split; Application only projects structured fields and Unity copies DTO fields.
- v27-v30 add no persisted fields, schema bump, migration, `PersonRegistry` expansion, manager/controller layer, cooldown ledger, owner-lane ledger, receipt-status ledger, outcome ledger, follow-up ledger, SocialMemory ledger, household target field, command queue, or command-target shape change.

## 2026-04-26 backend event contract health v32 note
- V32 is a diagnostic/readback classification pass for `DomainEvent` contract health. It does not move authority across module boundaries.
- Emitted-but-unconsumed and declared-but-not-emitted events may be classified as projection-only receipts, future contracts, dormant seeded paths, acceptance-test gaps, alignment bugs, or unclassified debt.
- `PublishedEvents` / `ConsumedEvents` remain module-owned contract declarations; the classification table in integration tests is evidence about current debt, not a new runtime registry or owner-lane ledger.
- Diagnostics may normalize event keys for display, but no Application/UI/Unity layer may compute module authority from those labels or parse `DomainEvent.Summary`.
- V32 adds no persisted fields, schema bump, migration, command system, projection surface, `PersonRegistry` expansion, or manager/controller layer.

## 2026-04-26 backend event contract health v33 note
- V33 adds a no-unclassified diagnostic gate over current ten-year event-contract debt. The gate reads emitted event keys, module `PublishedEvents`, and diagnostic consumer counts; it does not mutate module state or route commands.
- Current emitted-without-authority-consumer and declared-but-not-emitted debt must classify as projection-only receipt, future contract, dormant seeded path, acceptance-test gap, or alignment bug before the health report is used as evidence.
- This remains a test/readback boundary guard. It is not an event pool, not an event-chain design body, not an authority registry, and not a UI/Unity command or projection surface.
- V33 adds no persisted fields, schema bump, migration, command system, projection surface, `PersonRegistry` expansion, or manager/controller layer.

## 2026-04-26 backend event contract health v34 note
- V34 adds diagnostic owner/evidence backlinks to classified event-contract debt. `owner=<module>` comes from the structured event key/module prefix; `evidence=<doc/test backlink>` comes from the classification kind.
- The backlinks are developer diagnostics only. They do not move authority across modules, do not create an owner-lane registry, and do not let Application/UI/Unity compute command outcomes or owner lanes.
- V34 adds no persisted fields, schema bump, migration, command system, projection surface, event-health ledger, `PersonRegistry` expansion, or manager/controller layer.

## 2026-04-26 backend canal window v35 note
- `WorldSettlements` owns `CanalWindow` state and publishes `WorldSettlements.CanalWindowChanged` with structured before/after metadata.
- `TradeAndIndustry` may consume that fact to update only existing trade-owned market risk, route risk, blocked shipment, seizure-risk, and black-route ledger fields for water/canal-exposed settlements.
- `OrderAndBanditry` may consume that fact to update only existing order-owned route pressure, black-route pressure, disorder pressure, suppression demand, and route-shielding fields for water/canal-exposed settlements.
- V35 handlers choose locus through `IWorldSettlementsQueries`; they must not parse `DomainEvent.Summary`, receipt prose, `LastInterventionSummary`, or `LastLocalResponseSummary`, and they do not create persisted canal/owner-lane state.

## 2026-04-26 backend household-family burden v36 note
- `PopulationAndHouseholds` still owns household debt, distress, labor, migration, and local-response traces. Its household burden events are structured facts, not permission for another module to rewrite household state.
- `FamilyCore` may consume `PopulationAndHouseholds.HouseholdDebtSpiked`, `PopulationAndHouseholds.HouseholdSubsistencePressureChanged`, and `PopulationAndHouseholds.HouseholdBurdenIncreased` through `IPopulationAndHouseholdsQueries.GetRequiredHousehold(...)`, then target only the household `SponsorClanId`.
- The v36 handler mutates only existing family-owned fields: sponsor-clan charity obligation, support reserve, branch tension, relief sanction pressure, and lifecycle trace/outcome readback.
- `SocialMemoryAndRelations` is not part of the same-month mutation path. Durable shame/fear/favor/grudge/obligation residue remains its own later structured read/write concern.
- V36 adds no persisted fields, schema bump, migration, relief ledger, sponsor-lane ledger, household target field, manager/controller layer, `PersonRegistry` expansion, Application/UI/Unity authority, or summary parsing.

## 2026-04-26 backend office/yamen readback v38-v45 note
- `OfficeAndCareer` remains the owner of county-yamen implementation, document landing, clerk drag, petition outcome category, and official defection risk. V38-V45 does not create a county formula, yamen workflow object, policy ledger, or clerk AI.
- `PublicLifeAndRumor` may consume structured `OfficeAndCareer.PolicyImplemented` and `OfficeAndCareer.OfficeDefected` facts to project county-gate heat, notice, dispatch, street-talk, and legitimacy readback inside its existing namespace. It must not parse `DomainEvent.Summary`, receipt prose, `LastPetitionOutcome`, or `LastExplanation`.
- `Application` may compose governance/read-model guidance from existing structured snapshots such as `JurisdictionAuthoritySnapshot`, public-life snapshots, trade-route snapshots, disorder snapshots, and SocialMemory snapshots. It may not mutate state or compute command results.
- `SocialMemoryAndRelations` may write later-month office/yamen residue from structured `JurisdictionAuthoritySnapshot` fields only. Same-month command/event handling still does not write SocialMemory.
- Unity and shell code may copy `OfficeImplementationReadbackSummary`, `OfficeNextStepReadbackSummary`, `RegimeOfficeReadbackSummary`, `CanalRouteReadbackSummary`, and `ResidueHealthSummary`; they must not query modules, infer owner lanes, or write SocialMemory.
- V38-V45 adds no persisted fields, schema bump, migration, policy ledger, yamen workflow state, owner-lane ledger, cooldown ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 backend office-lane closure v46-v52 note
- `OfficeAndCareer` remains the owner of county-yamen, document landing, clerk drag, official wavering, and Office response traces. V46-V52 adds readback guidance for the Office lane; it does not move Office repair into `PopulationAndHouseholds`, `PublicLifeAndRumor`, Application, UI, or Unity.
- `Application` may project `OfficeLaneEntryReadbackSummary`, `OfficeLaneReceiptClosureSummary`, `OfficeLaneResidueFollowUpSummary`, and `OfficeLaneNoLoopGuardSummary` from existing structured Office snapshots, owner-response trace codes, and structured SocialMemory cause keys. It may not parse summaries or compute command results.
- `PopulationAndHouseholds` remains only the low-power home-household response owner. Its local response can ease or strain the home household, but it cannot repair yamen/documents/clerk delay, route pressure, family guarantee face, or durable social residue.
- `SocialMemoryAndRelations` still writes durable shame/fear/favor/grudge/obligation residue only in its later monthly pass. It must not read Office-lane projection prose such as `Office承接入口`, `Office后手收口读回`, `Office余味续接读回`, `Office闭环防回压`, or `本户不再代修` as authority input.
- Unity and shell code may copy the projected Office-lane closure fields only. They must not query modules, compute Office closure, infer owner-lane validity, maintain any owner-lane or receipt ledger, or write SocialMemory.
- V46-V52 adds no persisted fields, schema bump, migration, policy ledger, yamen workflow state, owner-lane ledger, receipt-status ledger, outcome ledger, cooldown ledger, follow-up ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 backend Family-lane closure v53-v60 note
- `FamilyCore` remains the owner of clan elder explanation, household guarantee, lineage-house face, and `SponsorClanId` pressure. V53-V60 adds readback guidance for the Family lane; it does not move Family repair into `PopulationAndHouseholds`, `PublicLifeAndRumor`, Application, UI, or Unity.
- `Application` may project `FamilyLaneEntryReadbackSummary`, `FamilyElderExplanationReadbackSummary`, `FamilyGuaranteeReadbackSummary`, `FamilyHouseFaceReadbackSummary`, `FamilyLaneReceiptClosureSummary`, `FamilyLaneResidueFollowUpSummary`, and `FamilyLaneNoLoopGuardSummary` from existing structured Family, household, and SocialMemory snapshots. It may not parse summaries or compute command results.
- `PopulationAndHouseholds` remains only the ordinary home-household local-response owner. Its local response can show where a household has eased or become strained, but clan elders, household guarantee, lineage-house face, and sponsor-clan pressure return to the Family lane. This is why the player-facing readback says `不是普通家户再扛`.
- `SocialMemoryAndRelations` still writes durable shame/favor/grudge/obligation residue only in its later monthly pass. It must not read Family-lane projection prose such as `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, or `Family闭环防回压` as authority input.
- Unity and shell code may copy the projected Family-lane closure fields only. They must not query modules, compute Family closure, infer guarantee success, maintain any Family closure / owner-lane / receipt ledger, or write SocialMemory.
- V53-V60 adds no persisted fields, schema bump, migration, Family closure ledger, guarantee ledger, owner-lane ledger, receipt-status ledger, outcome ledger, cooldown ledger, follow-up ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 backend Family relief choice v61-v68 note
- `FamilyCore` owns `GrantClanRelief`. The command is a bounded sibling to existing Family conflict commands and resolves inside the module from existing clan fields only.
- The command may reduce `CharityObligation`, `ReliefSanctionPressure`, `BranchTension`, and `BranchFavorPressure`, spend `SupportReserve`, raise `MediationMomentum`, and write existing conflict outcome/trace fields. It must not mutate `PopulationAndHouseholds`, `SocialMemoryAndRelations`, `OrderAndBanditry`, `OfficeAndCareer`, or `PublicLifeAndRumor`.
- `Application` may route/catalog the command and project `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, and `不是普通家户再扛`. It may not compute whether the relief succeeds, choose hidden household targets, or maintain a relief/owner-lane ledger.
- `PopulationAndHouseholds` remains the ordinary home-household low-power response lane. `GrantClanRelief` is not a backdoor household repair command and does not write household local-response traces.
- `SocialMemoryAndRelations` remains the later durable residue owner. Same-command Family relief does not write memories; any later shame/favor/grudge/obligation pass must read structured aftermath, not projection or receipt prose.
- Unity and shell code may copy projected command affordance/receipt fields only. They must not query modules, compute Family relief, infer sponsor targeting, or write SocialMemory.
- V61-V68 adds no persisted fields, schema bump, migration, relief ledger, charity ledger, guarantee ledger, Family closure ledger, owner-lane ledger, cooldown ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 backend Force/Campaign/Regime owner-lane readback v69-v76 note
- `ConflictAndForce` remains the owner of force posture, readiness, escort strain, and campaign fatigue; `WarfareCampaign` remains the owner of campaign boards, directives, mobilization signals, fronts, and aftermath dockets; `OfficeAndCareer` remains the owner of office coordination and regime/yamen authority.
- V69-V76 adds only projected readback fields: `WarfareLaneEntryReadbackSummary`, `ForceReadinessReadbackSummary`, `CampaignAftermathReadbackSummary`, `WarfareLaneReceiptClosureSummary`, `WarfareLaneResidueFollowUpSummary`, and `WarfareLaneNoLoopGuardSummary`.
- `Application` may assemble those fields from structured campaign, force, office, clan, and SocialMemory snapshots. It must not compute campaign outcomes, resolve force readiness, choose military targets, parse prose, or maintain a force/campaign owner-lane ledger.
- `PopulationAndHouseholds` remains only the ordinary household local-response lane. Military aftermath can pressure households, but the after-account for force readiness, campaign board, military order, and regime coordination returns to Force/Campaign/Office lanes rather than becoming `不是普通家户硬扛`.
- `SocialMemoryAndRelations` may later read structured campaign/force aftermath and write durable shame/fear/favor/grudge/obligation residue in its own cadence. It must not parse projected strings, receipt prose, `LastLocalResponseSummary`, `LastRefusalResponseSummary`, `LastInterventionSummary`, or `DomainEvent.Summary`.
- Unity and shell code copy projected fields only. They must not query simulation modules, compute Force/Campaign closure, infer yamen/Order ownership from military after-accounts, maintain ledgers, or write SocialMemory.
- V69-V76 adds no persisted fields, schema bump, migration, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 backend Warfare directive choice depth v77-v84 note
- `WarfareCampaign` remains the sole owner of military directive choice, active directive code/label/summary, and `LastDirectiveTrace`. `ConflictAndForce` still owns force posture/readiness, `OfficeAndCareer` still owns official coordination, and `PopulationAndHouseholds` remains ordinary local household response only.
- V77-V84 adds no new command system: the existing `DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, and `WithdrawToBarracks` commands now read back as `军令选择读回` with `案头筹议选择`, `点兵加压选择`, `粮道护持选择`, or `归营止损选择`.
- `Application` may compose that directive-choice readback with v69-v76 closure guidance from structured `CampaignMobilizationSignalSnapshot` and `CampaignFrontSnapshot` values. It must not compute command success, force readiness, county paperwork substitutes, owner-lane closure, or durable residue.
- `Unity` copies the projected command/receipt `ReadbackSummary` and warfare lane fields only; it must not query modules, infer owner lanes, execute from prose, parse summaries, or write SocialMemory.
- V77-V84 adds no persisted fields, schema bump, migration, directive ledger, force/campaign closure ledger, owner-lane ledger, cooldown ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 backend Warfare aftermath docket readback v85-v92 note
- `WarfareCampaign` remains the owner of campaign aftermath dockets: merits, blames, relief needs, route repairs, and docket summary. `OfficeAndCareer` owns county paperwork, `OrderAndBanditry` owns public-order aftermath, and `PopulationAndHouseholds` remains ordinary local household response only.
- V85-V92 adds no new command system or aftermath formula. It exposes existing `AftermathDocketSnapshot` values through runtime read models and projects `战后案卷读回`, `记功簿读回`, `劾责状读回`, `抚恤簿读回`, `清路札读回`, `WarfareCampaign拥有战后案卷`, `战后案卷不是县门/Order代算`, `不是普通家户补战后`, and `军务案卷防回压`.
- `Application` may count structured docket lists and compose readback text. It must not parse `DocketSummary`, `LastDirectiveTrace`, receipt prose, `DomainEvent.Summary`, or SocialMemory prose, and must not compute campaign outcomes.
- `Unity` reads `CampaignAftermathDockets` and projected command/governance fields only. It must not infer merits, blame, relief, or route repair from notifications, event traces, settlement stats, or prose.
- V85-V92 adds no persisted fields, schema bump, migration, aftermath ledger, relief ledger, route-repair ledger, owner-lane ledger, cooldown ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 backend court-policy process readback v93-v100 note
- `WorldSettlements` remains the owner of court-pressure facts such as `CourtAgendaPressureAccumulated`; `OfficeAndCareer` remains the owner of `PolicyWindowOpened`, `PolicyImplemented`, and county/yamen implementation; `PublicLifeAndRumor` remains the owner of public notice, dispatch, and street interpretation.
- V93-V100 adds only projected readback fields: `CourtPolicyEntryReadbackSummary`, `CourtPolicyDispatchReadbackSummary`, `CourtPolicyPublicReadbackSummary`, and `CourtPolicyNoLoopGuardSummary`. The player-facing read should include `朝议压力读回`, `政策窗口读回`, `文移到达读回`, `县门执行承接读回`, `公议读法读回`, `Court后手不直写地方`, `Office/PublicLife分读`, `不是本户也不是县门独吞朝廷后账`, and `Court-policy防回压`.
- `Application` may assemble those fields from structured `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` values only. It must not parse `DomainEvent.Summary`, receipt prose, `LastAdministrativeTrace`, `LastPetitionOutcome`, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- `PopulationAndHouseholds` remains ordinary local household response only. A home household may be strained by policy fallout, but it does not carry court-policy after-accounting.
- `SocialMemoryAndRelations` may later read structured aftermath and cause keys for durable residue; it must not treat court-policy projection prose as an authority source.
- `Unity` and shell code copy projected court-policy fields only. They must not query simulation modules, compute policy success, infer yamen/public-life ownership from prose, maintain dispatch/policy ledgers, or write SocialMemory.
- V93-V100 adds no persisted fields, schema bump, migration, court module, dispatch ledger, policy closure ledger, owner-lane ledger, cooldown ledger, household target field, manager/controller layer, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-26 thin-chain closeout audit v101-v108 note
- V101-V108 closes the current thin-chain skeleton only. It does not move authority between modules and does not add a new module, rule layer, command system, event pool, or production read-model field.
- The closeout meaning is boundary evidence: each implemented thin chain has an owning source, module-owned mutation or projection lane, structured event/query seam, repetition guard, off-scope/no-touch proof where applicable, and a player-facing readback that keeps ordinary households, Family, Office, Order, Trade, Force/Campaign, Warfare, court-policy, and SocialMemory responsibilities separate.
- The full-chain formulas remain outside this closeout. If a future pass deepens taxes, relief, famine, court process, regime recognition, campaign logistics, canal politics, or durable memory, that pass must name the owner module and schema/rule impact separately.
- Application remains route/assemble only. UI and Unity remain projection display only. `PersonRegistry` remains identity-only.

## 2026-04-27 court-policy process thickening v109-v116 note
- `WorldSettlements` still owns only court agenda / mandate pressure source facts such as `CourtAgendaPressureAccumulated`; it does not own policy implementation, public interpretation, household fallout, or durable memory.
- `OfficeAndCareer` owns `PolicyWindowOpened`, county/yamen execution, document handoff, and implementation posture. The first rule-density readback may expose `政策语气读回`, `文移指向读回`, and `县门承接姿态`, but those meanings come from Office-owned structured policy-window / implementation state and metadata.
- `PublicLifeAndRumor` owns public interpretation, notice visibility, street reading, dispatch pressure, documentary weight, verification cost, courier risk, and public legitimacy. It may expose `公议承压读法` and `朝廷后手仍不直写地方` from structured `PolicyImplemented` metadata and public-life scalar state, not from prose parsing.
- `SocialMemoryAndRelations` remains a later-month durable residue owner only. Same-month court-policy handling must not write memory, and future residue readers must use structured aftermath rather than court-policy projection text.
- `Application` may route, assemble, and project from structured `JurisdictionAuthoritySnapshot` / `SettlementPublicLifeSnapshot` fields only. `Unity` copies projected fields only. V109-V116 adds no Court module, court engine, event pool, policy ledger, dispatch ledger, court-process ledger, owner-lane ledger, cooldown ledger, schema bump, migration, `PersonRegistry` expansion, or UI/Unity authority.

## 2026-04-27 court-policy local response v117-v124 note
- `WorldSettlements` remains only the court agenda / mandate pressure source. It does not own local response, policy implementation success, public interpretation, or durable memory.
- `OfficeAndCareer` owns the bounded local document/report continuation. `PressCountyYamenDocument` and `RedirectRoadReport` may resolve policy-process pressure through existing office scalar state and structured response fields, without requiring order aftermath and without creating a court ledger.
- `PublicLifeAndRumor` owns the public reading surface for `PostCountyNotice` / `DispatchRoadReport`; it does not calculate policy success and does not parse notice or dispatch prose as authority.
- `SocialMemoryAndRelations` remains a later-month durable residue reader from structured aftermath only. Same-month v117-v124 handling must not write durable residue, and future readers must not parse projection prose.
- `Application` may surface `政策回应入口`, `文移续接选择`, `县门轻催`, `递报改道`, `公议降温只读回`, and `不是本户硬扛朝廷后账` as projected guidance from structured snapshots only. `Unity` copies command/readback fields only. V117-V124 adds no Court module, full court engine, event pool, dispatch/policy/court-process/owner-lane/cooldown ledger, schema bump, migration, `PersonRegistry` expansion, or UI/Unity authority.
