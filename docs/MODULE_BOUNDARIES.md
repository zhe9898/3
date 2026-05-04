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

## Social mobility scale-budget guard v269-v276 boundary note

V269-V276 adds no new runtime boundary. It hardens the existing fidelity-ring boundary so later social/personnel flow cannot drift into global per-person simulation:
- `PopulationAndHouseholds` owns household livelihood, membership activity, and labor/marriage/migration pools.
- `PersonRegistry` owns only identity, life-stage/death anchors, and existing `FidelityRing` assignment.
- `SocialMemoryAndRelations` owns later durable memory residue only when a future pass declares structured aftermath input and schema impact.
- Application may assemble `FidelityScaleSnapshot`, `SettlementMobilitySnapshot`, person dossiers, and runtime diagnostics, but it may not decide authority, promotion policy, or movement outcomes.
- UI and Unity may display projected mobility/fidelity fields only.

The four precision bands are: player household / close orbit as named detail; player influence footprint or active pressure as selective detail; active chain region as structured pools and settlement summaries; distant world as pressure summaries. This pass adds no schema, migration, movement/social/focus/scheduler ledger, manager/controller path, `PersonRegistry` domain expansion, or prose parser.

## Social mobility influence readback v277-v284 boundary note

V277-V284 adds runtime readback fields but no new authority boundary:
- `PopulationAndHouseholds` remains the owner of livelihood/activity/pools and migration pressure.
- `PersonRegistry` remains the owner of identity and existing `FidelityRing` assignment only.
- `SocialMemoryAndRelations` remains future durable-residue owner only; no same-month durable movement residue is written here.
- Application assembles `InfluenceFootprintReadbackSummary` and `ScaleBudgetReadbackSummary` from structured snapshots as projection text only.
- Unity copies the projected fields into shell ViewModels and summaries; it does not compute precision bands or movement outcomes.

Future readers must not parse these new readback strings as authority. The pass adds no schema, migration, command surface, movement/social/focus/scheduler ledger, manager/controller path, `PersonRegistry` domain expansion, or UI rule path.

## Social mobility boundary closeout v285-v292 boundary note

V285-V292 adds no new runtime boundary. It closes v213-v284 as a first-layer substrate and keeps the current split:
- `PopulationAndHouseholds` owns household livelihood, member activity, and labor/marriage/migration pools.
- `PersonRegistry` owns identity, life-stage/death anchors, and existing `FidelityRing` assignment only.
- `SocialMemoryAndRelations` may become the durable movement-residue owner only in a future planned pass with structured input and schema analysis.
- Application assembles projections, diagnostics, and closeout evidence only.
- UI and Unity copy projected mobility/fidelity/readback fields only.

The closeout is not a complete society engine, migration economy, personnel command layer, dormant-stub model, or whole-world person simulation. It adds no schema, migration, command/social/movement/focus/scheduler/personnel ledger, projection cache, manager/controller path, `PersonRegistry` domain expansion, or prose parser.

## Personnel command preflight v293-v300 boundary note

V293-V300 adds no new command boundary. It records the gate a future personnel-flow command must pass:
- `PopulationAndHouseholds` may own household livelihood, migration risk, household activity, and local household response.
- `PersonRegistry` may only own identity, life-stage/death anchors, and existing `FidelityRing` assignment; `ChangeFidelityRing` is a readability/fidelity command, not a movement command.
- `FamilyCore` may own kin/lineage intent when a future command concerns family authority or household support.
- `OfficeAndCareer` may own office-service, appointment, clerk, or document-mediated personnel pressure when a future command concerns yamen/office reach.
- `WarfareCampaign` may own campaign manpower posture when a future command concerns mobilization or return through campaign lanes.
- Application routes commands only; UI/Unity copy affordances and receipts only.

No future personnel command may begin as an Application rule, UI action, Unity drag/drop, global manager, or `PersonRegistry` domain expansion. V293-V300 adds no schema, migration, direct move/transfer/summon/assign-person command, command/movement/personnel/assignment/focus/scheduler ledger, projection cache, manager/controller path, or prose parser.

## Personnel flow command readiness v301-v308 boundary note

V301-V308 adds one runtime read-model/ViewModel field, `PersonnelFlowReadinessSummary`, to existing player-command affordances and receipts. It does not add a command boundary:
- `PopulationAndHouseholds` remains the owner of local household response and migration pressure.
- `PersonRegistry` remains identity plus existing `FidelityRing` only.
- Application fills the field from already-projected household snapshots and existing command names; it does not select people or resolve movement.
- Unity copies the field through command ViewModels only.

The field may say `人员流动预备读回`, `近处细读`, and `远处汇总`, but those are readback words only. V301-V308 adds no schema, migration, direct move/transfer/summon/assign-person command, command/movement/personnel/assignment/focus/scheduler ledger, projection cache, manager/controller path, `PersonRegistry` domain expansion, or prose parser.

## Personnel flow surface echo v309-v316 boundary note

V309-V316 adds a runtime command-surface echo field, `PlayerCommandSurfaceSnapshot.PersonnelFlowReadinessSummary`. It is assembled from structured affordance/receipt personnel-flow readiness fields after the owning command surfaces have been projected.

`Application` may count/read non-empty structured readiness fields and produce `人员流动命令预备汇总`; it must not choose people, rank movement candidates, inspect `PersonRegistry` for movement decisions, parse `ReadbackSummary`, or calculate migration success. `Zongzu.Presentation.Unity` may append the projected echo to Great Hall mobility readback; Unity shell code remains display-only.

The echo adds no module boundary, schema, migration, direct personnel command, movement/personnel/surface-echo ledger, projection cache, manager/controller path, `PersonRegistry` domain expansion, or prose parser.

## Personnel flow readiness closeout v317-v324 boundary note

V317-V324 is docs/tests only. It closes v293-v316 as a first command-readiness layer and does not add a boundary, command route, owner lane, schema, migration, or runtime state.

Future deeper personnel-flow work still requires a fresh owner-lane plan before adding movement state, assignment state, office-service rules, campaign-manpower rules, durable SocialMemory residue, or a new persisted schema.

## Personnel flow owner-lane gate v325-v332 boundary note

V325-V332 adds a runtime projection field, `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary`. It names the current and future owner lanes without opening a new command boundary.

The current readable lane remains `PopulationAndHouseholds` home-household response. `FamilyCore` kin mediation, `OfficeAndCareer` document/service pressure, and `WarfareCampaign` manpower posture are named only as future owner-lane plans. `PersonRegistry` remains identity/FidelityRing only.

Application may assemble the gate from structured command affordance/receipt fields; UI/Unity may display it. No module may treat the gate as movement permission, assignment authority, ledger membership, or durable SocialMemory residue.

## Personnel flow desk gate echo v333-v340 boundary note

V333-V340 adds no module boundary. It lets `Zongzu.Presentation.Unity` append the projected owner-lane gate to a Desk Sandbox settlement node only when the command surface already has local public-life personnel-flow readiness affordances or receipts.

The desk echo may enumerate structured command surfaces by settlement. It must not parse prose, infer hidden targets, query simulation modules directly, mutate state, or create a desk-local ledger.

## Personnel flow desk gate containment v341-v348 boundary note

V341-V348 adds no module boundary. It proves that a global `PersonnelFlowOwnerLaneGateSummary` is not a settlement-wide rule by itself.

Desk Sandbox must keep the gate local to settlements that have structured public-life `PersonnelFlowReadinessSummary` affordances or receipts. Quiet or distant settlements remain mobility pool summaries and do not inherit personnel-flow owner-lane text from another node.

Application still assembles projected command fields only. V341-V348 adds no schema, migration, command route, movement/personnel/desk-gate containment ledger, prose parser, UI authority, Unity authority, or `PersonRegistry` expansion.

## Personnel flow gate closeout v349-v356 boundary note

V349-V356 adds no boundary and closes no future owner lane. It closes only the current readback layer around personnel-flow gate visibility.

`PopulationAndHouseholds` remains the only current readable personnel-flow lane through household response and migration pressure. `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` remain future owner-lane plans until separate rules are designed.

Application, UI, and Unity remain projection/copy surfaces. V349-V356 adds no command route, movement resolver, owner-lane-gate ledger, schema, migration, prose parser, or `PersonRegistry` expansion.

## Personnel flow future owner-lane preflight v357-v364 boundary note

V357-V364 adds no boundary. It documents the boundary that a future personnel-flow lane must cross before implementation.

`FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` may later own their own personnel-flow effects only through separately planned module-owned commands and state/rule contracts. They may not borrow the v325-v356 gate as movement authority.

Any future lane must name owner state, accepted command, target scope, no-touch boundary, cardinality, deterministic cap/order, schema impact, cadence, projection/readback, and tests before code opens the path.

## Commoner social position preflight v381-v388 boundary note

V381-V388 adds no boundary. It documents the gate for later commoner / class-position mobility work over the existing social-mobility substrate.

`PopulationAndHouseholds` currently owns household livelihood, distress, labor/activity pressure, migration risk, and labor/marriage/migration pools. `PersonRegistry` remains identity, life-stage/death anchors, and existing `FidelityRing` only. `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, and `SocialMemoryAndRelations` may later contribute owner-lane pressure carriers only through separately planned module-owned rules.

Future commoner social-position drift must name owner state, accepted command or monthly rule, target scope, no-touch boundary, hot path, expected cardinality, deterministic cap/order, schema impact, cadence, projection/readback, and tests before code opens the path.

V381-V388 is not a full class engine, not a direct promote/demote command, not zhuhu/kehu conversion state, not a global per-person career simulation, and not a class/social-position/personnel/movement/focus/scheduler ledger. It adds no schema, migration, Application authority, UI/Unity authority, prose parser, manager/controller path, or `PersonRegistry` expansion.

## Commoner social position readback v389-v396 boundary note

V389-V396 adds a runtime read-model/ViewModel field only: `PersonDossierSnapshot.SocialPositionReadbackSummary`.

`PresentationReadModelBuilder.PersonDossiers` may assemble this readback from structured `FamilyCore`, `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, and `SocialMemoryAndRelations` snapshots already used by the dossier. `PersonRegistry` still owns only identity, life-stage/death anchors, and `FidelityRing`; it does not gain household, education, office, trade, memory, class, or status state.

Application assembles the readback and Unity copies it. Neither layer may parse `SocialPositionLabel`, readback prose, mobility text, public-life lines, receipt prose, notification prose, docs text, or `DomainEvent.Summary` to infer status movement. V389-V396 adds no schema, migration, promote/demote command, zhuhu/kehu conversion state, route resolver, ledger, projection cache, manager/controller path, or `PersonRegistry` expansion.

## Regime legitimacy readback v253-v260 boundary note

Chain 9 v253-v260 keeps the existing regime legitimacy pressure path inside current owner lanes:
- `WorldSettlements` owns the mandate/regime pressure source and `RegimeLegitimacyShifted` fact.
- `OfficeAndCareer` owns official risk, appointment mutation, county/yamen posture, and `OfficeDefected`.
- `PublicLifeAndRumor` owns public interpretation, notice visibility, dispatch texture, street heat, and `公议向背读法`.
- `SocialMemoryAndRelations` writes no same-month durable residue for this pass; any future durable residue must read structured aftermath and declare its own schema/rule impact.
- Application routes, assembles, and projects structured read models only.
- Unity and shell adapters copy projected fields only.

The readback tokens `天命摇动读回`, `去就风险读回`, `官身承压姿态`, `公议向背读法`, `仍由Office/PublicLife分读`, `不是本户替朝廷修合法性`, and `不是UI判定归附成败` are first-layer projection/readback wording over existing owner facts. V253-V260 is not a full regime engine, not a Court module, not faction AI, not an event pool, and not a legitimacy/defection/owner-lane/cooldown ledger. It adds no schema field, migration, `PersonRegistry` expansion, Application rule authority, UI/Unity authority, or manager/god-controller path.

## Regime legitimacy readback closeout v261-v268 boundary note

V261-V268 adds no new runtime boundary. It documents that the v253-v260 Chain 9 readback branch is closed only as a first-layer owner-lane readback:
- `WorldSettlements` remains the mandate/regime pressure source.
- `OfficeAndCareer` remains the owner of official risk, appointment mutation, and county/yamen posture.
- `PublicLifeAndRumor` remains the owner of public interpretation and street reading.
- `SocialMemoryAndRelations` remains out of same-month durable residue for this branch.
- Application remains projection assembly only.
- Unity remains copy-only presentation.

Future regime-recognition, public allegiance, ritual legitimacy, force backing, rebellion-to-polity, dynasty-cycle, or durable regime-memory work must declare a new owner/state/schema plan. The closeout adds no full regime engine, faction AI, Court module, ledger, schema, migration, `PersonRegistry` expansion, Application rule authority, UI/Unity authority, or manager/god-controller path.

## Court-policy first rule-density closeout audit v197-v204 boundary note

The v109-v196 first rule-density closeout audit v197-v204 is documentation/test governance over the existing Chain 8 branch. It does not add runtime authority:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` remains the owner of policy windows, county execution, document/report aftermath, and implementation posture.
- `PublicLifeAndRumor` remains the owner of public interpretation, notice visibility, road-report texture, and street reading.
- `SocialMemoryAndRelations` remains the owner of later durable `office.policy_local_response...` residue.
- Application remains a route / assemble / project layer over structured owner-lane facts.
- Unity remains a projected-field copy layer.

This closeout says v109-v196 is closed as a first rule-density branch only. It is not the full court engine, not a court-agenda / policy-dispatch completion claim, and not new court process state, appointment slate, dispatch arrival, or downstream household/market/public consequence rules. It adds no Court module, event pool, dispatch/policy/court-process/owner-lane/cooldown/docket/receipt/public-life-receipt-echo ledger, schema field, migration, Application rule authority, UI/Unity authority, `PersonRegistry` expansion, or manager/god-controller path.

## Court-policy public-life receipt echo v189-v196 boundary note

Chain 8 v189-v196 keeps v181-v188 receipt/docket ownership and adds only a projected public-life command echo:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the policy window, county execution, document/report aftermath, and implementation posture.
- `PublicLifeAndRumor` owns notice/report public interpretation, street reading, and public-life command surface texture.
- `SocialMemoryAndRelations` owns the durable `office.policy_local_response...` records that make the old reading visible.
- Application may assemble `公议回执回声防误读` from structured SocialMemory cause data and current PublicLife scalars, but it does not calculate policy success or turn a receipt into a new command.
- Unity copies projected `LeverageSummary` / `ReadbackSummary` only.

No Court module, court engine, event pool, dispatch/policy/court-process/owner-lane/cooldown/docket/receipt/public-life-receipt-echo ledger, schema field, migration, Application rule authority, UI/Unity authority, `PersonRegistry` expansion, or manager/god-controller path is added.

## Court-policy receipt-docket consistency guard v181-v188 boundary note

Chain 8 v181-v188 keeps v173-v180 suggested-receipt ownership and adds only a projected governance/docket consistency guard:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the current policy window, county execution, document/report aftermath, and implementation posture.
- `PublicLifeAndRumor` owns notice/report public interpretation and street-reading texture.
- `SocialMemoryAndRelations` owns the durable `office.policy_local_response...` records that make the old reading visible.
- Application may assemble `回执案牍一致防误读` from structured SocialMemory cause-key data plus current PublicLife scalars only; it must not calculate a fresh policy result, receipt result, or docket authority.
- Unity may copy governance, office, docket, desk, and great-hall projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/memory-pressure/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

## Court-policy suggested receipt guard v173-v180 boundary note

Chain 8 v173-v180 keeps v165-v172 suggested-action ownership and adds only a projected command-receipt guard:
- `WorldSettlements` remains only the court agenda / mandate pressure source.
- `OfficeAndCareer` owns the current policy window, county execution, document/report aftermath, and implementation posture.
- `PublicLifeAndRumor` owns notice/report public interpretation and street-reading texture.
- `SocialMemoryAndRelations` owns the durable `office.policy_local_response...` records that make the old reading visible.
- Application may assemble `建议回执防误读` from structured SocialMemory cause-key data plus current PublicLife scalars only; it must not calculate a fresh policy result or create receipt authority.
- Unity may copy receipt, governance, office, docket, desk, and great-hall projected fields only.

This pass adds no Court module, full court engine, dispatch/policy/court-process/owner-lane/cooldown/memory-pressure/public-reading/public-follow-up/docket/suggested-action/suggested-receipt ledger, schema field, migration, manager/god-controller path, or `PersonRegistry` expansion. It must not parse `DomainEvent.Summary`, memory summary prose, receipt prose, public-life prose, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastAdministrativeTrace`, `LastPetitionOutcome`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.

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
## 2026-04-28 social mobility fidelity ring v213-v244 note
- `PopulationAndHouseholds` owns household livelihood drift, household-member activity synchronization, and settlement labor/marriage/migration pool rebuilding. These are first-layer social/person movement rules, not Application or UI calculations.
- `PersonRegistry` owns identity and the existing `FidelityRing` only. Its `ChangeFidelityRing` command may move a hot person between precision rings, but it may not store household, livelihood, movement, clan, office, memory, or capability facts.
- `SocialMemoryAndRelations` writes no same-month durable movement residue in this pass. Later residue must read structured aftermath from owner modules, never movement/readback prose.
- `Application` builds `FidelityScaleSnapshot`, `SettlementMobilitySnapshot`, and person dossier readbacks from queries. `Zongzu.Presentation.Unity`, shared ViewModels, and the Unity shell copy projected fields only.
- V213-V244 adds no persisted fields, schema bump, migration, social-mobility ledger, movement ledger, focus ledger, manager/controller layer, or `PersonRegistry` expansion.

## 2026-04-28 social mobility fidelity ring closeout v245-v252 note
- V245-V252 is a closeout audit over the v213-v244 substrate. It adds no new production boundary and does not widen the `PersonRegistry` command surface beyond identity/fidelity-ring assignment.
- The branch is closed only as first-layer rule density: `PopulationAndHouseholds` can keep livelihood/activity/pools coherent, `PersonRegistry` can move an existing fidelity ring, and projection surfaces can explain why near/hot people are readable while regional people remain pooled.
- Future demotion, dormant stubs, durable movement residue, richer migration economics, and class-mobility formulas must still name their owning module and schema impact before implementation.
- UI, Unity, and Application must not turn this closeout into a complete society engine, per-person world simulation, saved projection cache, movement/social/focus ledger, or prose-parsing rule path.

## Personnel flow future lane surface v365-v372 boundary note
- `PopulationAndHouseholds` remains the current owner of local household response and personnel-flow readiness readback. The new surface field does not move authority into `FamilyCore`, `OfficeAndCareer`, or `WarfareCampaign`.
- `PlayerCommandSurfaceSnapshot.PersonnelFlowFutureOwnerLanePreflightSummary` is runtime read-model text only. It records that future Family/Office/Warfare personnel-flow lanes still need separate owner-module command/rule/state/schema/validation plans.
- `Application` may assemble the preflight from structured affordance/receipt readiness fields; it may not calculate movement success, choose people, or parse prose.
- `Zongzu.Presentation.Unity` may copy the projected field into Great Hall mobility readback only. Desk Sandbox locality remains governed by existing settlement-local readiness checks.
- V365-V372 adds no persisted state, schema bump, migration, direct personnel command, movement resolver, future-lane-surface ledger, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Personnel flow future lane closeout v373-v380 boundary note
- V373-V380 adds no new production boundary. It audits v357-v372 as planning/readback evidence only.
- `PopulationAndHouseholds` remains the current readable local-response lane; `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` remain future owner lanes until each has its own command/rule/state/schema/validation plan.
- `Application` remains projection/assembly only. `Zongzu.Presentation.Unity` remains copy-only. `PersonRegistry` remains identity/FidelityRing only.
- Later Family/Office/Warfare personnel-flow depth must not reuse this closeout as authority to move, assign, summon, dispatch, or allocate people.
- V373-V380 adds no persisted state, schema bump, migration, direct personnel command, movement resolver, future-lane-closeout ledger, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Social position owner-lane keys v397-v404 boundary note
- `PersonRegistry` remains identity / `FidelityRing` only; `SocialPositionSourceModuleKeys` does not give it household, class, office, movement, or memory authority.
- `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` remain the owners of the structured snapshots that may support the social-position readback.
- `Application` may assemble the runtime source-key list from already-loaded snapshots. It must not parse social-position prose, rank people, promote/demote people, resolve zhuhu/kehu conversion, or create a second class-rule layer.
- `Zongzu.Presentation.Unity` and the Unity shell mirror copy the projected list only.
- V397-V404 adds no persisted state, schema bump, migration, social-position ledger, class engine, command route, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Social position readback closeout v405-v412 boundary note
- V405-V412 adds no new production boundary. It closes v381-v404 as evidence that commoner / social-position readback is legible without becoming a class engine.
- `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` remain separate owner lanes. A future rule pass must choose one owner lane before changing status, route, residue, or fidelity.
- `Application` remains projection/assembly only. `Zongzu.Presentation.Unity` remains copy-only. `PersonRegistry` remains identity/FidelityRing only.
- Later commoner status depth must not reuse this closeout as authority to promote, demote, convert zhuhu/kehu status, assign office service, attach trade work, or write durable social-position residue.
- V405-V412 adds no persisted state, schema bump, migration, class/social-position closeout ledger, command route, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Social position scale budget v413-v420 boundary note
- `PersonRegistry` still owns identity / `FidelityRing` only. `SocialPositionScaleBudgetReadbackSummary` reads an existing ring; it does not change precision, select people, or store class/status facts.
- `Application` may assemble this runtime readback from existing `FidelityRing` and structured source keys. It must not parse social-position prose or calculate status movement.
- `Zongzu.Presentation.Unity` and the Unity shell mirror copy/display the projected field only.
- Future status depth that changes precision, source cardinality, or durable residue must open a new owner-lane schema/rule plan.
- V413-V420 adds no persisted state, schema bump, migration, class/social-position/scale-budget ledger, command route, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Social position regional scale guard v421-v428 boundary note
- V421-V428 adds no new production boundary. It verifies that `FidelityRing.Regional` reads as regional summary through the existing person dossier projection.
- `PersonRegistry` still owns identity / `FidelityRing` only and gains no status, class, route, or regional-detail authority.
- `Application` remains projection/assembly only; the guard must not become a hidden selection pass or precision policy.
- `Zongzu.Presentation.Unity` remains copy-only and gains no rule for choosing regional people.
- V421-V428 adds no persisted state, schema bump, migration, class/social-position/scale-budget ledger, command route, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Social position scale closeout v429-v436 boundary note
- V429-V436 adds no new production boundary. It closes v381-v428 as first-layer social-position visibility evidence only.
- `PersonRegistry` remains identity / `FidelityRing` only. It still does not own household status, class movement, office service, trade attachment, memory residue, or regional person selection.
- `PopulationAndHouseholds`, `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, `FamilyCore`, and `SocialMemoryAndRelations` remain separate owner lanes. Future status depth must choose one owner lane before changing state, route, residue, precision, or target cardinality.
- `Application` remains projection/assembly only. `Zongzu.Presentation.Unity` and the Unity shell mirror remain copy-only.
- V429-V436 adds no persisted state, schema bump, migration, class/social-position/scale-budget/closeout ledger, command route, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Commoner status owner-lane preflight v437-v444 boundary note
- V437-V444 adds no production boundary. It recommends `PopulationAndHouseholds` as the first future owner lane for commoner status drift because that module already owns household livelihood/activity/pools and pressure carriers.
- `PersonRegistry` remains identity / `FidelityRing` only and must not store commoner status drift, zhuhu/kehu conversion, office-service status, trade attachment, or durable residue.
- `Application` may only route/assemble future projections once an owner lane exists; it must not become a status resolver.
- `Zongzu.Presentation.Unity` and the Unity shell must not infer status movement, pick regional people, or present a class browser from current readbacks.
- V437-V444 adds no persisted state, schema bump, migration, owner-lane ledger, class/social-position/commoner-status module, command route, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Fidelity scale budget preflight v445-v452 boundary note
- V445-V452 adds no production boundary. It documents "near detail, far summary" as the required scale budget before any future commoner/personnel/social-position depth raises precision.
- `PersonRegistry` remains identity / `FidelityRing` only. It must not become a global person scanner, regional selector, class/status authority, or movement engine.
- `PopulationAndHouseholds` and other owner lanes may only deepen detail in future work after declaring target scope, hot path, touched counts, deterministic cap/order, cadence, schema impact, and validation.
- `Application` remains route/assembly/projection only. `Zongzu.Presentation.Unity` and the Unity shell copy projected fields only and must not infer hidden people, status movement, or far-region detail.
- V445-V452 adds no persisted state, schema bump, migration, scale-budget ledger, fidelity-budget ledger, selector ledger, class/social-position/commoner-status module, command route, scheduler sweep, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Household mobility dynamics explanation v453-v460 boundary note

- `PopulationAndHouseholds` remains the owner of household livelihood, distress, debt, labor, land, grain, migration, and settlement pool carriers.
- `HouseholdSocialPressureSnapshot.MobilityDynamicsExplanationSummary` and `MobilityDynamicsDimensionKeys` are runtime read-model fields over existing signals. They do not add state, change monthly rules, or create a household-mobility ledger.
- `PersonRegistry` remains identity / `FidelityRing` only and gains no commoner status, household mobility, class, route, or target-selection authority.
- `Application` may assemble the explanation from structured snapshots. It must not parse summaries, receipts, notification prose, mobility text, social-position text, docs text, or `DomainEvent.Summary`.
- `Zongzu.Presentation.Unity` and the Unity shell may copy the projected explanation to Desk Sandbox only. They must not infer class/status movement, choose people, raise fidelity, or scan regional society.
- V453-V460 adds no persisted state, schema bump, migration, mobility/class/status ledger, selector, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Household mobility dynamics closeout v461-v468 boundary note

- V461-V468 adds no new production boundary. It closes v453-v460 as first-layer household mobility explanation evidence only.
- Later migration, commoner status, route-history, office-service, trade-attachment, durable residue, or direct movement depth must not reuse this closeout as authority to move, assign, convert, select, or promote people.
- `PopulationAndHouseholds` remains the owner of household livelihood, labor, debt, grain, land, migration, and pool carriers. `PersonRegistry` remains identity / `FidelityRing` only.
- Application, UI, and Unity may keep showing projected explanation fields, but they must not parse those fields, calculate movement eligibility, create hidden target selection, or scan distant people.
- V461-V468 adds no persisted state, schema bump, migration, household-mobility/movement/route-history/status/class/selector/closeout ledger, command route, module, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Household mobility owner-lane preflight v469-v476 boundary note

- V469-V476 adds no production boundary. It gates a future household mobility runtime rule before any movement, route, status, selector, or durable residue authority is added.
- `PopulationAndHouseholds` is the default first owner lane because household livelihood, activity, distress, debt, labor, grain, land, migration pressure, and pool carriers already live inside that namespace.
- A future owner-lane pass must name owned state, cadence, target scope, hot path, touched counts, deterministic cap/order, no-touch boundary, schema impact, projection fields, and validation before implementation.
- `PersonRegistry` remains identity / `FidelityRing` only. It must not store household mobility, commoner status, class drift, route history, target selection, office-service status, trade attachment, or durable residue.
- Application remains route/assembly/projection only. UI, `Zongzu.Presentation.Unity`, and the Unity shell copy projected fields only and must not infer movement, scan distant people, or promote far summaries into hidden targets.
- V469-V476 adds no persisted state, schema bump, migration, owner-lane ledger, household-mobility ledger, movement ledger, route-history ledger, selector ledger, module, command route, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Household mobility preflight closeout v485-v492 boundary note

- V485-V492 adds no new production boundary. It closes v469-v476 as a future-rule gate, not as runtime household mobility authority.
- Later household movement, route history, status drift, selector state, durable residue, or fidelity promotion must not reuse this closeout as authority to move, assign, convert, select, promote, or remember households.
- `PopulationAndHouseholds` remains the default first future owner lane because the relevant household pressure carriers already live there. `PersonRegistry` remains identity / `FidelityRing` only.
- Application, UI, and Unity may keep showing existing projected household mobility explanation fields, but they must not parse those fields, calculate movement eligibility, create hidden target selection, or scan distant people.
- V485-V492 adds no persisted state, schema bump, migration, household-mobility/movement/route-history/status/class/selector/preflight/closeout ledger, command route, module, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Household mobility runtime rules-data readiness v501-v508 boundary note

- V501-V508 adds no new production boundary. It records a first runtime rule readiness map and hardcoded extraction map only.
- `PopulationAndHouseholds` remains the future first runtime owner lane because household livelihood, member activity, distress, debt, labor, grain, land, migration risk, local-response carryover, settlement summaries, and labor/marriage/migration pools already live in that namespace.
- A future rule should be monthly-first. Xun-band household pressure remains existing cadence/projection context until a later plan proves a runtime need.
- The documented target scope is player-near households, pressure-hit local households, active-region pools, and distant summaries. Quiet households, off-scope settlements, distant pooled society, `PersonRegistry`, Application, UI, and Unity remain no-touch.
- Future fanout must declare household/pool/settlement caps, deterministic ordering, and stable tie-break priority before runtime behavior changes.
- Current hardcoded thresholds, weights, caps, recovery/decay rules, ordering rules, regional assumptions, era/scenario assumptions, and pool limits are candidates for owner-consumed authored rules-data, not permission for Application/UI/Unity calculation or runtime plugin loading.
- V501-V508 adds no persisted state, schema bump, migration, rules-data loader, runtime plugin marketplace, household-mobility/movement/route-history/status/class/selector/readiness ledger, command route, movement authority, module, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Household mobility rules-data contract preflight v509-v516 boundary note

- V509-V516 adds no production boundary. It defines a future rules-data contract and validator preflight only.
- The future consumer remains `PopulationAndHouseholds`; Application, UI, Unity, `PersonRegistry`, and other modules may not calculate household mobility outcomes from rules-data.
- Required future contract traits are stable ids, schema/version, deterministic ordering, default fallback, readable validation errors, owner-consumed use only, no UI/Application authority, and no arbitrary script/plugin execution.
- Future parameter categories are threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and deterministic tie-break priorities.
- The current repo has no reusable runtime rules-data/content/config pattern, so this pass adds no rules-data loader, default file, validator implementation, or config-backed runtime rule.
- V509-V516 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, validator, runtime plugin marketplace, arbitrary script surface, runtime assembly load, reflection-heavy loader, household-mobility/movement/route-history/status/class/selector/contract ledger, command route, movement authority, module, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## Household mobility default rules-data skeleton v517-v524 boundary note

- V517-V524 adds no production boundary. It defines a future default rules-data skeleton shape only.
- The future consumer remains `PopulationAndHouseholds`; the skeleton is not authority state and does not enter save.
- Future skeleton fields are `ruleSetId`, `schemaVersion`, `ownerModule`, `defaultFallbackPolicy`, `parameterGroups`, `validationResult`, and deterministic declaration order.
- Parameter groups are threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and tie-break priorities.
- Because the current repo has no reusable runtime rules-data/content/config pattern, this pass adds no `content/rules-data`, rules-data loader, default file, validator implementation, or config-backed runtime rule.
- V517-V524 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, validator, runtime plugin marketplace, arbitrary script surface, runtime assembly load, reflection-heavy loader, household-mobility/movement/route-history/status/class/selector/default-skeleton ledger, command route, movement authority, module, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## PopulationAndHouseholds first hardcoded rule extraction v525-v532 boundary note

- V525-V532 extracts one existing `PopulationAndHouseholds` cap into owner-consumed rules data. It does not create a new module boundary.
- `PopulationHouseholdMobilityRulesData` is consumed only by `PopulationAndHouseholds`; Application, UI, Unity, `PersonRegistry`, and other modules do not read it to calculate outcomes.
- The extracted parameter is the focused member promotion fanout cap. Default remains 2, and deterministic household-id then person-id ordering remains unchanged.
- This is not a household movement authority path, route-history model, migration economy, selector, class/status engine, loader, `content/rules-data`, runtime plugin marketplace, or prose parser.
- V525-V532 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, household-mobility/movement/route-history/status/class/selector/extraction ledger, command route, movement authority, module, manager/controller path, `PersonRegistry` expansion, or UI/Unity authority.

## PopulationAndHouseholds first mobility runtime rule v533-v540 boundary note

- V533-V540 keeps household mobility authority inside `PopulationAndHouseholds`; it does not create a new household mobility, household movement, migration economy, route-history, selector, status, or class module.
- The monthly owner rule reads existing owner state and summaries: `MigrationPools`, livelihood, distress, debt, labor, grain, land, and migration risk.
- The rule writes only existing owner fields: `MigrationRisk`, `IsMigrating`, pool summaries, and the existing `MigrationStarted` event if the existing threshold is crossed.
- Application, UI, Unity, `PersonRegistry`, other modules, docs text, projection prose, receipt text, public-life lines, and `DomainEvent.Summary` do not calculate target eligibility or outcomes.
- V533-V540 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, movement command, route-history model, movement ledger, owner-lane ledger, cooldown ledger, selector watermark, target-cardinality state, durable residue, runtime plugin marketplace, arbitrary script surface, reflection-heavy loader, or UI/Unity authority.

## Household mobility first runtime rule closeout v541-v548 boundary note

- V541-V548 is closeout only. It does not add another `PopulationAndHouseholds` rule path, module, command, query, event, loader, or presentation field.
- The first runtime rule remains inside `PopulationAndHouseholds` and remains limited to existing `MigrationRisk`, `IsMigrating`, `MigrationPools`, and the existing `MigrationStarted` threshold receipt.
- Application, UI, Unity, `PersonRegistry`, other modules, docs text, projection prose, receipt text, public-life lines, and `DomainEvent.Summary` still do not calculate eligibility, fanout, or outcomes.
- Any future move from pressure nudge to household movement, route history, recovery/decay depth, persistent cooldown, selector, or target-cardinality state requires a separate boundary plan and schema decision before code.
- V541-V548 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, movement command, route-history model, movement ledger, owner-lane ledger, cooldown ledger, selector watermark, target-cardinality state, durable residue, runtime plugin marketplace, arbitrary script surface, reflection-heavy loader, or UI/Unity authority.

## Household mobility runtime rule health evidence v549-v556 boundary note

- V549-V556 is diagnostics/readiness evidence only. It does not add another `PopulationAndHouseholds` rule path, module, command, query, event, loader, presentation field, or scheduler phase.
- The first runtime rule remains inside `PopulationAndHouseholds` and remains limited to existing `MigrationRisk`, `IsMigrating`, `MigrationPools`, and the existing `MigrationStarted` threshold receipt.
- Future widening must first document touched household/pool/settlement counts, deterministic cap/order, same-seed replay proof, no-touch boundaries, pressure-band interpretation, and hot-path/cardinality notes.
- Application, UI, Unity, `PersonRegistry`, other modules, docs text, projection prose, receipt text, public-life lines, and `DomainEvent.Summary` still do not calculate eligibility, fanout, health classification, or outcomes.
- V549-V556 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, movement command, route-history model, movement ledger, owner-lane ledger, cooldown ledger, selector watermark, target-cardinality state, durable residue, runtime plugin marketplace, arbitrary script surface, reflection-heavy loader, long-run saturation tuner, performance optimization path, or UI/Unity authority.

## Household mobility runtime widening gate v557-v564 boundary note

- V557-V564 is preflight only. It does not add another `PopulationAndHouseholds` rule path, widen fanout, change caps, change recovery/decay formulas, add counters, add caches, or add a scheduler phase.
- The first runtime rule remains inside `PopulationAndHouseholds` and remains limited to existing `MigrationRisk`, `IsMigrating`, `MigrationPools`, and the existing `MigrationStarted` threshold receipt.
- Future widening must declare owner state, target scope, current/proposed touched counts, deterministic order/caps, no-touch boundaries, pressure-band interpretation, schema decision, validation lane, and whether performance evidence is claimed.
- Application, UI, Unity, `PersonRegistry`, other modules, docs text, projection prose, receipt text, public-life lines, and `DomainEvent.Summary` still do not calculate eligibility, fanout, touched counts, health classification, performance status, or outcomes.
- V557-V564 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, movement command, route-history model, movement ledger, owner-lane ledger, cooldown ledger, selector watermark, target-cardinality state, touch-count state, diagnostic state, performance cache, durable residue, runtime plugin marketplace, arbitrary script surface, reflection-heavy loader, long-run saturation tuner, performance optimization path, or UI/Unity authority.

## Household mobility runtime touch-count proof v565-v572 boundary note

- V565-V572 is focused test evidence only. It does not add another `PopulationAndHouseholds` rule path, widen fanout, change caps, change recovery/decay formulas, add counters, add caches, or add a scheduler phase.
- The first runtime rule remains inside `PopulationAndHouseholds` and remains limited to existing `MigrationRisk`, `IsMigrating`, `MigrationPools`, and the existing `MigrationStarted` threshold receipt.
- The new owner test proves the current default fixture touches exactly two eligible households in one selected active pool and leaves the lower-priority pressure-hit household, quiet household, and lower-priority active pool untouched.
- Application, UI, Unity, `PersonRegistry`, other modules, docs text, projection prose, receipt text, public-life lines, and `DomainEvent.Summary` still do not calculate eligibility, fanout, touched counts, health classification, performance status, or outcomes.
- V565-V572 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, movement command, route-history model, movement ledger, owner-lane ledger, cooldown ledger, selector watermark, target-cardinality state, persisted touch-count state, diagnostic state, performance cache, durable residue, runtime plugin marketplace, arbitrary script surface, reflection-heavy loader, long-run saturation tuner, performance optimization path, or UI/Unity authority.

## Household mobility rules-data fallback matrix v573-v580 boundary note

- V573-V580 is focused fallback evidence only. It does not add a rules-data loader, default file, plugin surface, another `PopulationAndHouseholds` rule path, fanout widening, counters, caches, or scheduler phase.
- Existing `PopulationHouseholdMobilityRulesData` remains owner-consumed by `PopulationAndHouseholds`; Application, UI, Unity, `PersonRegistry`, docs text, projection prose, receipt text, public-life lines, and `DomainEvent.Summary` do not calculate validation fallback or outcomes.
- Malformed runtime threshold/cap/delta values fall back to defaults, and owner-result evidence requires malformed rules-data to match the default monthly signature.
- V573-V580 adds no persisted state, schema bump, migration, rules-data file, rules-data loader, movement command, route-history model, movement ledger, owner-lane ledger, cooldown ledger, selector watermark, target-cardinality state, persisted touch-count state, diagnostic state, performance cache, durable residue, runtime plugin marketplace, arbitrary script surface, reflection-heavy loader, long-run saturation tuner, performance optimization path, or UI/Unity authority.
## Household mobility runtime threshold no-touch v581-v588 boundary note

V581-V588 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule respects active-pool threshold blocking. The pass adds only tests/docs and keeps selection, ordering, fanout, threshold fallback, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, cache, or `PersonRegistry` expansion is introduced.
## Household mobility runtime zero-cap no-touch v589-v596 boundary note

V589-V596 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule respects zero fanout caps. The pass adds only tests/docs and keeps cap blocking, selection, ordering, fanout, threshold fallback, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, cache, or `PersonRegistry` expansion is introduced.
## Household mobility runtime zero-risk-delta no-touch v597-v604 boundary note

V597-V604 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule respects zero risk delta. The pass adds only tests/docs and keeps risk-delta blocking, selection, ordering, fanout, threshold fallback, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, cache, or `PersonRegistry` expansion is introduced.
## Household mobility runtime candidate-filter no-touch v605-v612 boundary note

V605-V612 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule respects candidate filtering. The pass adds only tests/docs and keeps candidate filtering, selection, ordering, fanout, threshold fallback, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, cache, or `PersonRegistry` expansion is introduced.
## Household mobility runtime tie-break no-touch v613-v620 boundary note

V613-V620 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule respects deterministic score tie-break ordering. The pass adds only tests/docs and keeps candidate scoring, lower-household-id tie-break selection, cap application, fanout, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, ordering ledger, cache, or `PersonRegistry` expansion is introduced.
## Household mobility runtime pool tie-break no-touch v621-v628 boundary note

V621-V628 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule respects deterministic active-pool tie-break ordering. The pass adds only tests/docs and keeps pool outflow ordering, lower-settlement-id tie-break selection, settlement cap application, fanout, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, pool ordering ledger, cache, or `PersonRegistry` expansion is introduced.
## Household mobility runtime score-ordering no-touch v629-v636 boundary note

V629-V636 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule respects deterministic candidate score ordering before household-id tie-break. The pass adds only tests/docs and keeps candidate scoring, cap application, fanout, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, score-order ledger, cache, or `PersonRegistry` expansion is introduced.

## Household mobility runtime pool-priority no-touch v637-v644 boundary note

V637-V644 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule applies active-pool priority before cross-pool household score comparison. The pass adds only tests/docs and keeps pool priority, settlement cap application, candidate scoring, fanout, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, pool-priority ledger, cross-pool score ledger, cache, or `PersonRegistry` expansion is introduced.

## Household mobility runtime per-pool cap no-touch v645-v652 boundary note

V645-V652 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule applies household cap limits inside each selected active pool. The pass adds only tests/docs and keeps pool selection, per-pool cap application, candidate scoring, fanout, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, per-pool cap ledger, global household cap ledger, cache, or `PersonRegistry` expansion is introduced.

## Household mobility runtime threshold-event no-touch v653-v660 boundary note

V653-V660 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule emits the existing threshold receipt only for a selected household that crosses the existing migration-started threshold. The pass adds only tests/docs and keeps threshold crossing, event emission, structured metadata, fanout, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, threshold-event ledger, event-routing ledger, migration-started selector state, cache, or `PersonRegistry` expansion is introduced.

## Household mobility runtime event-metadata no-prose v661-v668 boundary note

V661-V668 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule's selected threshold event is interpreted through structured metadata rather than prose. The pass adds only tests/docs and keeps metadata meaning, threshold crossing, event emission, fanout, and no-touch behavior inside the owning module.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, event-metadata ledger, prose-parsing ledger, event-routing ledger, migration-started selector state, cache, or `PersonRegistry` expansion is introduced.

## Household mobility runtime event-metadata replay v669-v676 boundary note

V669-V676 is a `PopulationAndHouseholds` owner-lane proof that the first household mobility runtime rule's selected threshold-event metadata signature is stable under same-seed replay. The pass adds only tests/docs and keeps replay comparison, metadata meaning, threshold crossing, event emission, fanout, and no-touch behavior inside owner tests.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, schema field, event-replay state, event-metadata ledger, event-routing ledger, migration-started selector state, cache, or `PersonRegistry` expansion is introduced.

## Household mobility runtime threshold extraction v677-v684 boundary note

V677-V684 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's selected-household `MigrationStarted` event threshold is now read from `PopulationHouseholdMobilityRulesData` with default 80.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, threshold-extraction state, event-routing ledger, validation ledger, cache, candidate filter retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime candidate-floor extraction v685-v692 boundary note

V685-V692 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's candidate migration-risk floor is now read from `PopulationHouseholdMobilityRulesData` with default 55.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, candidate-floor state, event-routing ledger, validation ledger, cache, high-risk filter retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime score-weight extraction v693-v700 boundary note

V693-V700 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's migration-risk score weight is now read from `PopulationHouseholdMobilityRulesData` with default 4.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, score-weight state, event-routing ledger, validation ledger, cache, score formula retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime labor-floor extraction v701-v708 boundary note

V701-V708 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's labor-capacity pressure floor is now read from `PopulationHouseholdMobilityRulesData` with default 60.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, labor-floor state, event-routing ledger, validation ledger, cache, labor model retune, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime grain-floor extraction v709-v716 boundary note

V709-V716 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's grain-store pressure floor is now read from `PopulationHouseholdMobilityRulesData` with default 25.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, grain-floor state, event-routing ledger, validation ledger, cache, grain economy retune, grain pressure divisor extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime land-floor extraction v717-v724 boundary note

V717-V724 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's land-holding pressure floor is now read from `PopulationHouseholdMobilityRulesData` with default 20.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, land-floor state, event-routing ledger, validation ledger, cache, land economy retune, land pressure divisor extraction, class/status engine, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime grain-divisor extraction v725-v732 boundary note

V725-V732 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's grain-store pressure divisor is now read from `PopulationHouseholdMobilityRulesData` with default 2 and validation range 1..16.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, grain-divisor state, event-routing ledger, validation ledger, cache, grain economy retune, land pressure divisor extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime land-divisor extraction v733-v740 boundary note

V733-V740 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's land-holding pressure divisor is now read from `PopulationHouseholdMobilityRulesData` with default 2 and validation range 1..16.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, land-divisor state, event-routing ledger, validation ledger, cache, land economy retune, grain pressure divisor extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime candidate-ceiling extraction v741-v748 boundary note

V741-V748 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's high-risk candidate ceiling is now read from `PopulationHouseholdMobilityRulesData` with default 80 and validation range 1..100.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, candidate-ceiling state, event-routing ledger, validation ledger, cache, migration-started event threshold retune, candidate floor retune, trigger threshold extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime distress-trigger extraction v749-v756 boundary note

V749-V756 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's distress trigger threshold is now read from `PopulationHouseholdMobilityRulesData` with default 60 and validation range 0..100.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, distress-trigger state, event-routing ledger, validation ledger, cache, debt/labor/grain/land/livelihood trigger extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime debt-trigger extraction v757-v764 boundary note

V757-V764 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's debt-pressure trigger threshold is now read from `PopulationHouseholdMobilityRulesData` with default 60 and validation range 0..100.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, debt-trigger state, event-routing ledger, validation ledger, cache, distress/labor/grain/land/livelihood trigger extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime labor-trigger extraction v765-v772 boundary note

V765-V772 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's labor-capacity trigger ceiling is now read from `PopulationHouseholdMobilityRulesData` with default 45 and validation range 0..100.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, labor-trigger state, event-routing ledger, validation ledger, cache, debt/distress/grain/land/livelihood trigger extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime grain-trigger extraction v773-v780 boundary note

V773-V780 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's grain-store trigger floor is now read from `PopulationHouseholdMobilityRulesData` with default 25 and validation range 0..100.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, grain-trigger state, event-routing ledger, validation ledger, cache, labor/debt/distress/land/livelihood trigger extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime land-trigger extraction v781-v788 boundary note

V781-V788 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's land-holding trigger floor is now read from `PopulationHouseholdMobilityRulesData` with default 15 and validation range 0..100.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, land-trigger state, event-routing ledger, validation ledger, cache, grain/labor/debt/distress/livelihood trigger extraction, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime livelihood-trigger extraction v789-v796 boundary note

V789-V796 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's trigger livelihood list is now read from `PopulationHouseholdMobilityRulesData` with default `[SeasonalMigrant, HiredLabor]` and deterministic validation.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, livelihood-trigger state, event-routing ledger, validation ledger, cache, class/status engine, score formula expansion, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime livelihood-score extraction v797-v804 boundary note

V797-V804 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's livelihood score weights are now read from `PopulationHouseholdMobilityRulesData` with defaults `SeasonalMigrant=18`, `HiredLabor=10`, and `Tenant=6`; unmatched livelihoods still score `0`.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, livelihood-score state, event-routing ledger, validation ledger, cache, class/status engine, trigger retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime pressure-score extraction v805-v812 boundary note

V805-V812 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's distress/debt score weights are now read from `PopulationHouseholdMobilityRulesData` with defaults `Distress=1` and `DebtPressure=1`.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, pressure-score state, event-routing ledger, validation ledger, cache, class/status engine, trigger retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime migration-status extraction v813-v820 boundary note

V813-V820 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's migration status threshold is now read from `PopulationHouseholdMobilityRulesData` with default `80`.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, migration-status state, event-routing ledger, validation ledger, cache, class/status engine, event-threshold retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime migration-risk clamp extraction v821-v828 boundary note

V821-V828 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's migration-risk clamp bounds are now read from `PopulationHouseholdMobilityRulesData` with default `0..100`.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, risk-clamp state, event-routing ledger, validation ledger, cache, class/status engine, risk-delta retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime tie-break priority extraction v829-v836 boundary note

V829-V836 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's deterministic pool and household tie-break priorities are now read from `PopulationHouseholdMobilityRulesData` with default settlement-id and household-id ascending behavior.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, tie-break ledger, ordering ledger, event-routing ledger, validation ledger, cache, class/status engine, ordering retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime unmatched livelihood score extraction v837-v844 boundary note

V837-V844 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's unmatched-livelihood score fallback is now read from `PopulationHouseholdMobilityRulesData` with default `0` behavior.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, unmatched-livelihood ledger, ordering ledger, event-routing ledger, validation ledger, cache, class/status engine, score formula retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime pressure contribution floor extraction v845-v852 boundary note

V845-V852 is a `PopulationAndHouseholds` owner-lane hardcoded extraction: the first household mobility runtime rule's non-negative labor/grain/land pressure contribution floor is now read from `PopulationHouseholdMobilityRulesData` with default `0` behavior.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, pressure-contribution ledger, ordering ledger, event-routing ledger, validation ledger, cache, class/status engine, score formula retune, fanout widening, or `PersonRegistry` expansion is introduced.

## Household mobility runtime extraction closeout v853-v860 boundary note

V853-V860 closes the first `PopulationAndHouseholds` household mobility runtime rule hardcoded extraction track. The remaining inline guards are control-flow boundaries rather than rules-data authority: no-op cap/delta checks, empty-state checks, changed-flow, threshold crossing, and boolean candidate composition stay inside the owner method.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, closeout ledger, ordering ledger, event-routing ledger, validation ledger, cache, class/status engine, fanout widening, file split, or `PersonRegistry` expansion is introduced.

## PopulationAndHouseholds runtime rule file split v861-v868 boundary note

V861-V868 splits the first household mobility runtime rule into `PopulationAndHouseholdsModule.MobilityRuntime.cs` while keeping it inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, command/event/query seams, scheduler cadence, and rules-data consumption remain unchanged.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, file-split state, ordering ledger, event-routing ledger, validation ledger, cache, class/status engine, fanout widening, or `PersonRegistry` expansion is introduced.

## PopulationAndHouseholds membership focus file split v869-v876 boundary note

V869-V876 splits membership synchronization and hot-household focus promotion helpers into `PopulationAndHouseholdsModule.MembershipFocus.cs` while keeping them inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, command/event/query seams, scheduler cadence, and focused-member promotion cap consumption remain unchanged.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, membership-focus split state, focus ledger, event-routing ledger, validation ledger, cache, class/status engine, fanout widening, or `PersonRegistry` expansion is introduced.

## PopulationAndHouseholds pool rebuild file split v877-v884 boundary note

V877-V884 splits settlement summary and labor/marriage/migration pool rebuild helpers into `PopulationAndHouseholdsModule.PoolRebuild.cs` while keeping them inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, command/event/query seams, scheduler cadence, and pool formulas remain unchanged.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, pool-rebuild split state, pool ledger, event-routing ledger, validation ledger, cache, class/status engine, fanout widening, or `PersonRegistry` expansion is introduced.

## PopulationAndHouseholds query surface file split v885-v892 boundary note

V885-V892 splits the private `PopulationQueries` implementation and clone helpers into `PopulationAndHouseholdsModule.Queries.cs` while keeping them inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, query registration, command/event/query seams, scheduler cadence, and snapshot copy behavior remain unchanged.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, query-surface split state, query ledger, event-routing ledger, validation ledger, cache, class/status engine, fanout widening, or `PersonRegistry` expansion is introduced.

## PopulationAndHouseholds pressure profile file split v893-v900 boundary note

V893-V900 splits private grain, tax-season, and official-supply pressure profile helpers into `PopulationAndHouseholdsModule.PressureProfiles.cs` while keeping them inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, command/event/query seams, scheduler cadence, metadata fallback values, and formulas remain unchanged.

## PopulationAndHouseholds event dispatch file split v901-v908 boundary note

V901-V908 splits private trade-shock, world-pulse, family-branch, grain-price, tax-season, and official-supply event-dispatch/application helpers into `PopulationAndHouseholdsModule.EventDispatch.cs` while keeping them inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, event subscriptions, emitted metadata, scheduler cadence, deterministic ordering, and household pressure authority remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains event-dispatch authority.

## PopulationAndHouseholds livelihood drift file split v909-v916 boundary note

V909-V916 splits private monthly livelihood drift helpers into `PopulationAndHouseholdsModule.LivelihoodDrift.cs` while keeping them inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, monthly cadence, household traversal, livelihood thresholds, emitted receipts at call sites, and household pressure authority remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains livelihood-drift authority.

## PopulationAndHouseholds monthly pulse file split v917-v924 boundary note

V917-V924 splits private xun/month pulse helpers into `PopulationAndHouseholdsModule.MonthlyPulse.cs` while keeping them inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, xun/month cadence, household traversal, debt/labor/migration delta thresholds, migration status fallback, and household pressure authority remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains monthly-pulse or migration-status authority.

## PopulationAndHouseholds warfare aftermath file split v925-v932 boundary note

V925-V932 splits private warfare-campaign aftermath handling into `PopulationAndHouseholdsModule.WarfareAftermath.cs` while keeping it inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, event handling order, settlement-scoped household traversal, campaign delta formulas, emitted receipts, and household pressure authority remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains warfare-aftermath or campaign-delta authority.

## PopulationAndHouseholds health lifecycle file split v933-v940 boundary note

V933-V940 splits private monthly health lifecycle handling into `PopulationAndHouseholdsModule.HealthLifecycle.cs` while keeping it inside the same private `PopulationAndHouseholdsModule` partial class. Module ownership, monthly call order, person-id membership traversal, health thresholds, illness-month caps, `PersonRegistry` command seam, emitted receipts, and dependent-count adjustment remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains health-lifecycle, illness-death, or mortality-formula authority.

## PopulationAndHouseholds monthly pressure rules-data extraction v941-v948 boundary note

V941-V948 extracts monthly prosperity/security/clan-relief/drift thresholds into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` monthly owner pass. Module ownership, monthly cadence, household traversal, pressure mutation, event emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains monthly-pressure or rules-data outcome authority.

## PopulationAndHouseholds grain price signal rules-data extraction v949-v956 boundary note

V949-V956 extracts grain-price shock signal fallback and clamp values into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` grain-price event handling path. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains grain-shock, pressure-profile, or rules-data outcome authority.

## PopulationAndHouseholds grain price pressure clamp extraction v957-v964 boundary note

V957-V964 extracts grain-price price-pressure clamp bounds into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` grain-price pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains grain-price-pressure, pressure-profile, or rules-data outcome authority.

## PopulationAndHouseholds grain price level band extraction v965-v972 boundary note

V965-V972 extracts grain-price level threshold/score bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` grain-price pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains grain-price-level, pressure-profile, or rules-data outcome authority.

## PopulationAndHouseholds grain price jump band extraction v973-v980 boundary note

V973-V980 extracts grain-price jump threshold/score bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` grain-price pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains grain-price-jump, pressure-profile, or rules-data outcome authority.

## PopulationAndHouseholds grain price market tightness band extraction v981-v988 boundary note

V981-V988 extracts grain-price market tightness threshold/score bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` grain-price pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains market-tightness, pressure-profile, or rules-data outcome authority.

## PopulationAndHouseholds subsistence market dependency extraction v989-v996 boundary note

V989-V996 extracts subsistence market dependency livelihood score weights into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains market-dependency, pressure-profile, or rules-data outcome authority.

Application, presentation, and Unity layers remain projection/copy surfaces only. No migration engine, movement command, route-history state, runtime plugin loader, rules-data file, schema field, pressure-profile split state, pressure-profile ledger, event-routing ledger, validation ledger, cache, class/status engine, fanout widening, or `PersonRegistry` expansion is introduced.
## PopulationAndHouseholds subsistence labor capacity extraction v997-v1004 boundary note

V997-V1004 extracts subsistence labor-capacity threshold score bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, dependent-count pressure, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains subsistence-labor, pressure-profile, rules-data, or household mobility outcome authority.

## PopulationAndHouseholds subsistence dependent count extraction v1005-v1012 boundary note

V1005-V1012 extracts subsistence dependent-count threshold score bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, final labor clamp, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains dependent-count, pressure-profile, rules-data, or household mobility outcome authority.

## PopulationAndHouseholds subsistence labor clamp extraction v1013-v1020 boundary note

V1013-V1020 extracts the final subsistence labor pressure clamp into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, labor-capacity/dependent-count scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains subsistence-labor, pressure-profile, rules-data, or household mobility outcome authority.

## PopulationAndHouseholds subsistence grain buffer extraction v1021-v1028 boundary note

V1021-V1028 extracts subsistence grain-buffer pressure bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, price/market/labor/fragility/interaction scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains grain-buffer, pressure-profile, rules-data, or household mobility outcome authority.

## PopulationAndHouseholds subsistence fragility distress extraction v1029-v1036 boundary note

V1029-V1036 extracts subsistence fragility distress bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, debt/migration/fragility-clamp scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains fragility-distress, pressure-profile, rules-data, or household mobility outcome authority.

## PopulationAndHouseholds subsistence fragility debt extraction v1037-v1044 boundary note

V1037-V1044 extracts subsistence fragility debt bands into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, distress/migration/fragility-clamp scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains fragility-debt, pressure-profile, rules-data, or household mobility outcome authority.

## PopulationAndHouseholds subsistence fragility migration extraction v1045-v1052 boundary note

V1045-V1052 extracts the subsistence fragility migration threshold/score/fallback into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, distress/debt/fragility-clamp scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains fragility-migration, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence fragility clamp extraction v1053-v1060 boundary note

V1053-V1060 extracts the subsistence fragility final clamp floor/ceiling into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, distress/debt/migration scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains fragility-clamp, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence interaction grain shortage extraction v1061-v1068 boundary note

V1061-V1068 extracts the subsistence interaction grain-shortage floor/ceiling into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, interaction boost/relief scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains interaction-grain-shortage, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence interaction cash-need extraction v1069-v1076 boundary note

V1069-V1076 extracts the subsistence interaction cash-need boost score into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, grain-shortage window, debt/resilience interaction scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains interaction-cash-need, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence interaction debt-boost extraction v1077-v1084 boundary note

V1077-V1084 extracts the subsistence interaction debt pressure threshold and boost score into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, grain-shortage window, cash-need interaction scoring, resilience interaction scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains interaction-debt, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence interaction resilience relief extraction v1085-v1092 boundary note

V1085-V1092 extracts the subsistence interaction resilience relief thresholds and score into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, grain-shortage window, cash-need interaction scoring, debt interaction scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains interaction-resilience, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence interaction clamp extraction v1093-v1100 boundary note

V1093-V1100 extracts the subsistence interaction pressure clamp floor/ceiling into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` subsistence pressure helper. Module ownership, event scope, household traversal, subsistence pressure mutation, event emission, grain-shortage window, cash-need interaction scoring, debt interaction scoring, resilience interaction scoring, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains interaction-clamp, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence event threshold extraction v1101-v1108 boundary note

V1101-V1108 extracts the grain-price subsistence event distress crossing threshold into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` event dispatch helper. Module ownership, event scope, household traversal, subsistence pressure mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains subsistence-event-threshold, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds subsistence distress delta clamp extraction v1109-v1116 boundary note

V1109-V1116 extracts the grain-price subsistence distress delta clamp into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` pressure profile helper. Module ownership, event scope, household traversal, subsistence pressure mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains subsistence-delta-clamp, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds tax season debt delta clamp extraction v1117-v1124 boundary note

V1117-V1124 extracts the tax-season debt delta clamp into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` tax-season pressure profile helper. Module ownership, event scope, deterministic household traversal, debt-pressure mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains tax-debt-delta-clamp, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds tax season debt spike threshold extraction v1125-v1132 boundary note

V1125-V1132 extracts the tax-season debt spike event threshold into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` event dispatch helper. Module ownership, event scope, deterministic household traversal, debt-pressure mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains tax-debt-spike-threshold, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds official supply distress delta clamp extraction v1133-v1140 boundary note

V1133-V1140 extracts the official-supply distress delta clamp into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` official-supply pressure profile helper. Module ownership, event scope, deterministic household traversal, burden mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains official-supply-distress-delta-clamp, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds official supply debt delta clamp extraction v1141-v1148 boundary note

V1141-V1148 extracts the official-supply debt delta clamp into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` official-supply pressure profile helper. Module ownership, event scope, deterministic household traversal, burden mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains official-supply-debt-delta-clamp, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds official supply labor drop clamp extraction v1149-v1156 boundary note

V1149-V1156 extracts the official-supply labor drop clamp into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` official-supply pressure profile helper. Module ownership, event scope, deterministic household traversal, burden mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains official-supply-labor-drop-clamp, pressure-profile, rules-data, migration outcome, or household mobility outcome authority.

## PopulationAndHouseholds official supply migration delta clamp extraction v1157-v1164 boundary note

V1157-V1164 extracts the official-supply migration delta clamp into `PopulationHouseholdMobilityRulesData` while keeping consumption inside the private `PopulationAndHouseholdsModule` official-supply pressure profile helper. Module ownership, event scope, deterministic household traversal, burden mutation, metadata emission, and projection downstream behavior remain unchanged.

No Application, presentation, Unity, persistence, or `PersonRegistry` boundary gains official-supply-migration-delta-clamp, pressure-profile, rules-data, movement, route-history, or household mobility outcome authority.
### PopulationAndHouseholds official supply burden event threshold extraction v1165-v1172 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of the official-supply burden event distress threshold. The extracted `DefaultOfficialSupplyBurdenEventDistressThreshold = 80` is a schema-neutral rules-data parameter, not module state, not public presentation authority, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate or store official-supply burden outcomes. No movement command, route-history module, migration economy, class/status engine, loader, runtime plugin marketplace, or persisted ledger is introduced.

### PopulationAndHouseholds official supply signal fallback clamp extraction v1173-v1180 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply signal fallback values. The extracted defaults are schema-neutral rules-data parameters, not module state, not public presentation authority, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate or store official-supply signal fallback outcomes. No movement command, route-history module, migration economy, class/status engine, loader, runtime plugin marketplace, or persisted ledger is introduced.

### PopulationAndHouseholds official supply signal normalization clamp extraction v1181-v1188 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply signal normalization clamps. The extracted bounds are schema-neutral rules-data parameters, not module state, not public presentation authority, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate or store official-supply signal normalization outcomes. No movement command, route-history module, migration economy, class/status engine, loader, runtime plugin marketplace, or persisted ledger is introduced.

### PopulationAndHouseholds official supply livelihood exposure extraction v1189-v1196 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply livelihood exposure scores and land visibility bands. The extracted values are schema-neutral rules-data parameters, not module state, not public presentation authority, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate or store official-supply livelihood exposure outcomes. No movement command, route-history module, migration economy, class/status engine, loader, runtime plugin marketplace, or persisted ledger is introduced.

### PopulationAndHouseholds official supply resource buffer extraction v1197-v1204 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply resource buffer grain/tool/shelter bands. The extracted values are schema-neutral rules-data parameters, not module state, not public presentation authority, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate or store official-supply resource buffer outcomes. No movement command, route-history module, migration economy, class/status engine, loader, runtime plugin marketplace, or persisted ledger is introduced.

### PopulationAndHouseholds official supply labor pressure extraction v1205-v1212 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply labor capacity bands, dependent count bands, and dependent/labor ratio bonus. The extracted values are schema-neutral rules-data parameters, not module state, not public presentation authority, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate or store official-supply labor pressure outcomes. No movement command, route-history module, migration economy, class/status engine, loader, runtime plugin marketplace, or persisted ledger is introduced.

### PopulationAndHouseholds official supply liquidity pressure extraction v1213-v1220 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply liquidity grain strain bands, cash-need score, tool drag threshold, and debt drag bands. The extracted values are schema-neutral rules-data parameters, not module state, not public presentation authority, and not a cross-module command surface.

### PopulationAndHouseholds official supply fragility pressure extraction v1221-v1228 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply fragility pressure distress bands, debt bands, migration-risk threshold, shelter-drag threshold, fallback scores, and clamp. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune official-supply fragility outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

### PopulationAndHouseholds official supply interaction pressure extraction v1229-v1236 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply interaction pressure livelihood conditions, thresholds, boost scores, resilience relief score, fallback scores, and clamp. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune official-supply interaction outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

### PopulationAndHouseholds official supply distress delta formula extraction v1237-v1244 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply distress delta formula divisors and component weights. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune official-supply distress delta outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

### PopulationAndHouseholds official supply debt delta formula extraction v1245-v1252 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply debt delta formula divisors, interaction floor, and component weights. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune official-supply debt delta outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

### PopulationAndHouseholds official supply labor drop formula extraction v1253-v1260 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply labor drop formula divisors, labor-pressure floor, and component weight. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune official-supply labor drop outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

### PopulationAndHouseholds official supply migration delta formula extraction v1261-v1268 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of official-supply migration delta formula divisors and fragility threshold/boost. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune official-supply migration delta outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

Application, UI, Unity, and `PersonRegistry` must not calculate or store official-supply liquidity pressure outcomes. No movement command, route-history module, migration economy, class/status engine, loader, runtime plugin marketplace, shared cash-need predicate retune, or persisted ledger is introduced.

### PopulationAndHouseholds tax season registration visibility extraction v1269-v1276 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of tax-season registration visibility livelihood exposure scores, land visibility bands, fallback scores, and clamp. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune tax-season registration visibility outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

### PopulationAndHouseholds tax season liquidity pressure extraction v1277-v1284 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of tax-season liquidity pressure grain bands, cash-need livelihood scores, tool-drag threshold/score, fallback scores, and clamp. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune tax-season liquidity outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.

### PopulationAndHouseholds tax season labor pressure extraction v1285-v1292 boundary note

`PopulationAndHouseholds` remains the owner and sole consumer of tax-season labor capacity bands, dependent count bands, dependent/labor ratio bonus, fallback scores, and clamp. The extracted values are schema-neutral in-memory `PopulationHouseholdMobilityRulesData` defaults, not module state, not public presentation authority, not a runtime rules-data file, and not a cross-module command surface.

Application, UI, Unity, and `PersonRegistry` must not calculate, store, parse, or retune tax-season labor outcomes. No movement command, route-history state, migration economy, class/status engine, runtime plugin loader, content/config namespace, prose parser, pressure-profile ledger, or persisted field is introduced.
