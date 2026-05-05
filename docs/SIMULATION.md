# SIMULATION

This document defines the authoritative simulation cadence and monthly scheduler.

For the higher-level design rule that the world moves first, projection comes after diffs, and the player only acts late in the monthly cycle, see `RULES_DRIVEN_LIVING_WORLD.md`.
For the rule that precision is allocated by focus ring rather than by a flat status ladder, see `SIMULATION_FIDELITY_MODEL.md`.
For the per-module time contract, see `MODULE_CADENCE_MATRIX.md`.
For the living-world structure that this loop serves, see `LIVING_WORLD_DESIGN.md`.
For the development route and phase ownership, see `GAME_DEVELOPMENT_ROADMAP.md`.
For modern game-engineering standards, see `MODERN_GAME_ENGINEERING_STANDARDS.md`.

## Core cadence rule

Zongzu should not use a flat one-jump monthly heartbeat for all forms of life.

Use this cadence instead:

- `month` is the outer review shell
- `day` is the target inner authority atom for lived pressure, travel, illness, delay, debt timing, and local motion
- `xun` (`early / mid / late month`) is a calendar label, projection grouping, or explanation band, not the preferred bottom-level authority grid

Non-negotiable player-facing rule:
- `day` is an authority cadence, not the normal player turn cadence
- the player should not click through daily turns as routine play
- the shell should not expose `early / mid / late month` as mandatory sub-turns either
- normal review, interpretation, and command happen at the monthly shell
- only urgent red-band or irreversible items may interrupt the month, and those interrupts must be narrow response windows rather than a daily alert treadmill

In plain terms:
- the player still receives a monthly review and major command window
- the world below that review can move by day
- quiet days may be skipped or batched when no due rule, threshold, command, journey, illness, debt, office delay, or interrupt is pending
- low-level life does not have to wait a whole month to strain, worsen, travel, or surface

This keeps the design aligned with:
- living household pressure
- delayed but readable causality
- bounded player intervention
- ordinary people, debt, illness, labor, and rumor feeling alive instead of being crushed into one large monthly jump

## Current regime legitimacy readback v253-v260 note

The v253-v260 Chain 9 readback uses the existing scheduler cadence and bounded event drain. A low mandate/regime pressure fact may emit `WorldSettlements.RegimeLegitimacyShifted`; `OfficeAndCareer` may resolve one highest-risk `OfficeDefected` mutation in the same month; `PublicLifeAndRumor` may read that structured fact into matching-settlement public interpretation. Application then projects `天命摇动读回`, `去就风险读回`, `官身承压姿态`, and `公议向背读法`.

This adds no scheduler phase, event pool, full regime engine, faction AI, Court module, ledger, schema field, migration, same-month SocialMemory durable residue, or UI/Unity authority. The simulation proof should cover same-month pressure flow, off-scope non-inheritance, structured metadata/read-model use, no prose parsing, and the boundary that the home household is not repairing legitimacy: `不是本户替朝廷修合法性`.

## Current regime legitimacy readback closeout v261-v268 note

The v261-v268 closeout adds no simulation cadence and no runtime rule. It records that the existing v253-v260 Chain 9 path is closed only for first-layer readback: one mandate/regime pressure edge, one office-owned defection mutation, matching public-life interpretation, governance projection, and Unity copy-only display.

This closeout must not be used as a scheduler phase, recurring demand, regime-recognition formula, public allegiance calculation, durable memory write, faction AI, event pool, ledger, schema field, migration, or UI/Unity authority. Future deeper regime work must declare owner state, cadence, fanout, recovery/decay, save/schema impact, and validation separately.

## Current court-policy first rule-density closeout audit v197-v204 note

The v109-v196 first rule-density closeout audit v197-v204 uses no new simulation cadence. It records that Chain 8 has a closed first-layer branch across policy process texture, local Office/PublicLife response, later SocialMemory residue, next-window memory readback, public-reading echo, public follow-up cue, docket guard, suggested-action guard, suggested-receipt guard, receipt-docket consistency, and public-life receipt echo.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, court process state, appointment slate, dispatch arrival, downstream household/market/public consequence rule, or UI/Unity authority. It must not mutate SocialMemory during projection, create a cooldown account, reopen policy state, calculate policy success, treat court-policy readback as Order/Office authority, or parse memory summary prose, receipt prose, public-life prose, affordance prose, docket prose, or `DomainEvent.Summary`.

## Current court-policy public-life receipt echo v189-v196 note

The v189-v196 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if public-life notice/report commands are visible for the same settlement, the projection layer may show `公议回执回声防误读` from structured SocialMemory cause outcome data plus current PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, public-life receipt echo ledger, or UI/Unity authority. It must not mutate SocialMemory during projection, create a cooldown account, reopen policy state, calculate policy success, treat public-life command readback or receipt readback as Order/Office authority, or parse memory summary prose, receipt prose, public-life prose, affordance prose, docket prose, or `DomainEvent.Summary`.

## Current court-policy receipt-docket consistency guard v181-v188 note

The v181-v188 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if governance/docket surfaces are visible for the same settlement, the projection layer may show `回执案牍一致防误读` from structured SocialMemory cause outcome data plus current PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, receipt-docket ledger, or UI/Unity authority. It must not mutate SocialMemory during projection, create a cooldown account, reopen policy state, calculate policy success, treat the docket or receipt as Order/Office authority, or parse memory summary prose, receipt prose, public-life prose, affordance prose, docket prose, or `DomainEvent.Summary`.

## Current court-policy suggested receipt guard v173-v180 note

The v173-v180 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if a command receipt is already visible for the same settlement, the projection layer may show `建议回执防误读` from structured SocialMemory cause outcome data plus current PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, suggested receipt ledger, or UI/Unity authority. It must not mutate SocialMemory during projection, create a cooldown account, reopen policy state, calculate policy success, treat the receipt as Order/Office authority, or parse memory summary prose, receipt prose, public-life prose, affordance prose, or `DomainEvent.Summary`.

## Current court-policy suggested action guard v165-v172 note

The v165-v172 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if the governance docket already selects a projected affordance, the projection layer may show `建议动作防误读` from structured SocialMemory cause outcome data plus current PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, suggested-action ranking rule, or UI/Unity authority. It must not mutate SocialMemory during projection, create a cooldown account, reopen policy state, calculate policy success, treat the prompt as Order/Office authority, or parse memory summary prose, receipt prose, public-life prose, affordance prose, or `DomainEvent.Summary`.

## Current court-policy follow-up docket guard v157-v164 note

The v157-v164 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if governance/docket surfaces are visible, the projection layer may show `政策后手案牍防误读` from structured SocialMemory cause outcome data plus current PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, or UI/Unity authority. It must not mutate SocialMemory during projection, create a cooldown account, reopen policy state, calculate policy success, treat the guard as Order/Office authority, or parse memory summary prose, receipt prose, public-life prose, or `DomainEvent.Summary`.

## Current court-policy public follow-up cue v149-v156 note

The v149-v156 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if public-life notice/report command surfaces are visible, the projection layer may show `政策公议后手提示` from structured SocialMemory cause outcome data plus current PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, or UI/Unity authority. It must not mutate SocialMemory during projection, create a cooldown account, reopen policy state, calculate policy success, or parse memory summary prose, receipt prose, public-life prose, or `DomainEvent.Summary`.

## Current court-policy public-reading echo v141-v148 note

The v141-v148 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if public-life notice/report command surfaces are visible, the projection layer may show `政策公议旧读回` from structured SocialMemory cause/type/weight plus current Office/PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, or UI/Unity authority. It must not mutate SocialMemory during projection, reopen policy state, calculate public-reading success, or parse memory summary prose, receipt prose, public-life prose, or `DomainEvent.Summary`.

## Current court-policy memory-pressure readback v133-v140 note

The v133-v140 Chain 8 readback uses no new simulation cadence. A later month may already have produced `office.policy_local_response...` residue; if a new policy window is visible, the projection layer may show `政策旧账回压读回` from structured SocialMemory cause/type/weight plus current Office/PublicLife snapshots.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, or UI/Unity authority. It must not mutate SocialMemory during projection, reopen policy state, or parse memory summary prose, receipt prose, public-life prose, or `DomainEvent.Summary`.

## Current court-policy social-memory echo v125-v132 note

The v125-v132 Chain 8 echo uses the existing monthly cadence. Month N player command handling may write `OfficeAndCareer` structured local-response aftermath, but it must not write durable SocialMemory residue in the same command step. A later `SocialMemoryAndRelations.RunMonth` pass may read structured `JurisdictionAuthoritySnapshot` response fields and write existing memory/narrative/climate records under `office.policy_local_response...`.

This adds no scheduler phase, event pool, Court module, ledger, schema field, migration, or UI/Unity authority. The simulation proof should cover same-month neutrality, later-month residue, deterministic ordering by settlement/clan, and no prose parsing from receipts, public-life lines, or `DomainEvent.Summary`.

## Time layers

### Day authority step

Use `day` as the target inner unit of lived time.

This is where the game should resolve:
- food consumption and immediate livelihood strain
- short labor allocation and labor shortage
- petty trade movement and local market heat
- route interruption, ferry irregularity, and messenger delay
- rumor spread and local notice drift
- illness worsening or short-term recovery
- small debt pressure, rent pressure, and support strain
- local disorder flare, escort strain, and hotspot activation

The day step is not a full player turn.
It is the world's internal breathing rhythm.
Most day-level movement should be absorbed into trend state, pressure accumulation, hotspot staging, and later month-end explanation.
If a day-level event becomes visible immediately, it must be because it crossed a threshold such as death, flight, violence, office seizure, route collapse, disaster impact, or another irreversible / time-sensitive state.

### Xun / calendar band

`shangxun / zhongxun / xiaxun` may still exist as:
- almanac wording
- UI grouping
- projection summary
- schedule-window label for rules that want "early month" or "late month" flavor

It should not force every local process into three equal authoritative slices.
When an exact due date matters, use day-level timing.
When precision does not matter, summarize by month or by a named window.

### Monthly shell

The month remains the main review and settlement shell.

This is where the game should emphasize:
- consolidated diffs
- family and branch review
- major policy and support choices
- formal notices, hall review, and council prompts
- marriage advancement or stalling
- inherited pressure handoff into the next month

### Seasonal and annual bands

Use longer bands for slower-moving structures such as:
- harvest outcome
- major exam moments
- broad tax burden climate
- war posture and campaign fatigue that outlasts one month
- major disaster bands
- large legitimacy and policy climate shifts

These longer bands may still leak short-term pressure into day-level motion, xun-labeled summaries, and monthly play.

## Monthly shell with day-level inner motion

This section describes authoritative scheduler order.
It is not a UI mandate to expose daily player turns.
The UI should normally present one monthly review, with day-level motion summarized as trends, cause traces, and urgent exceptions.

### Phase 0: prepare month
- current date known
- `PersonRegistry` runs age progression and life-stage checks (so all modules in Phase 1 see current life stage)
- staged player commands from the previous review validated
- enabled feature manifest loaded
- projection caches invalidated as needed

### Phase 1: run due day steps

Each month contains day-level authority steps.
The scheduler may skip or batch quiet day spans when no module has a due rule, command receipt, travel arrival, illness threshold, debt date, office deadline, route window, or interrupt candidate.

Each processed day should run the same deterministic sub-order.

#### Day Phase A: local world conditions
`WorldSettlements`
- update short-band environment, route friction, safety, and local availability
- refresh current settlement heat and access constraints

#### Day Phase B: household and livelihood motion
`PopulationAndHouseholds`
- apply subsistence use, labor strain, wage or rent pressure, migration drift, and worker instability
- resolve whether pressure remains ambient, becomes household-visible, or escalates

#### Day Phase C: family and support motion
`FamilyCore`
- resolve near-term support burden, household dependency strain, branch assistance pressure, and urgent kin disruption
- urgent clan-scoped births and household crises may surface here instead of waiting for month-end
- FamilyCore may respond to death events emitted by other modules (e.g. `DeathByIllness` from PopulationAndHouseholds, `DeathByViolence` from ConflictAndForce) by triggering inheritance and mourning logic; it does not own the death cause itself (see `PERSON_OWNERSHIP_RULES.md`)

#### Day Phase D: local social drift
`SocialMemoryAndRelations`
- update short-band obligation, fear, shame, rumor attachment, and feud temperature
- promote or cool memories when pressure was actually carried during the day

#### Day Phase E: enabled local and upward modules
Enabled modules run in deterministic order for day-visible local pressure:
1. `EducationAndExams`
2. `TradeAndIndustry`
3. `PublicLifeAndRumor`
4. `OfficeAndCareer` if enabled
5. `ConflictAndForce` if enabled
6. `OrderAndBanditry` if enabled
7. `WarfareCampaign` if enabled

Use this pass for:
- route and price movement
- venue heat, street talk, notice drift, and road-report motion
- minor office or yamen temperature shifts
- local-force readiness strain
- order response lag
- visible campaign spillover

Do not force every slow structure to fully resolve inside a single day.
The day step should handle due short-band pressure, not replace the monthly shell.

#### Day Phase F: receipts and hotspot staging
- capture short-band receipts
- mark hotspot activation
- stage urgent items for month-end review
- allow only bounded interrupt-worthy items to request immediate visibility

### Phase 2: month-end domain event handling
- modules emit monthly event snapshots after all due day steps for the month are complete
- deterministic event queue snapshot processed before projection
- handlers update only owning module state
- the active handler seam runs after authority modules finish their monthly pass and before `NarrativeProjection` builds notices
- follow-on events may reach projection, but they do not trigger uncontrolled recursive month expansion
- `SocialMemoryAndRelations` may consume scoped trade, exam, death, branch, marriage, and warfare events here; it mutates only memory/climate/tempering state and emits terminal social-memory receipts such as `EmotionalPressureShifted` or `PressureTempered`

Current M3 local-conflict note:
- `ConflictAndForce.Lite` may refresh force posture before `OrderAndBanditry.Lite` reads same-month response support
- only activated local-conflict response state may feed same-month order relief; calm or standing-but-untriggered posture stays visible but does not leak relief
- `ConflictAndForce.Lite` still reads only published query state and does not mutate `OrderAndBanditry` directly
- `OfficeAndCareer.Lite`, when enabled through the governance-lite path, now runs ahead of conflict/order so jurisdiction leverage can be read as bounded same-month administrative support without direct writes
- `SocialMemoryAndRelations` currently runs before `OrderAndBanditry` in the monthly module order, so it may read the prior public-life order carryover once, persist social residue inside its own state, and leave Order to decay or clear its carryover later in the same month
- `PopulationAndHouseholds` runs before `SocialMemoryAndRelations`, so v13 home-household local response residue reads structured local response command/outcome/trace fields after the population pass and relies on SocialMemory cause-key de-duplication rather than UI timers or summary parsing
- v14 home-household repeat friction is command-time and query-driven: when the player issues a later local response, `PopulationAndHouseholds` reads already-persisted SocialMemory snapshots and mutates only its own household state, so no scheduler shortcut or same-command SocialMemory write is introduced
- v15 common-household response texture is also command-time and population-owned: the resolver derives debt/labor/distress/migration texture from existing household state and mutates only the local household response cost/outcome fields. It adds no scheduler step, no same-command SocialMemory write, and no UI-owned rule path.
- v16 home-household response capacity is projection-time plus command-time only: read models derive `回应承受线` from existing household fields, while `PopulationAndHouseholds` resolves any issued local command inside its own namespace. It adds no scheduler step, no same-command SocialMemory write, and no thick household rule loop.
- v17 home-household response tradeoff forecast is projection-time plus command-time only: read models derive `取舍预判` / `预期收益` / `反噬尾巴` / `外部后账` from existing household fields, while `PopulationAndHouseholds` resolves any issued local command inside its own namespace. It adds no scheduler step, no same-command SocialMemory write, and no thick household rule loop.
- v18 home-household short-term consequence readback is receipt projection only: read models derive `短期后果：缓住项` / `挤压项` / `仍欠外部后账` from existing household fields and structured local response codes after the owning command has resolved. It adds no scheduler step, no same-command SocialMemory write, and no thick household rule loop.
- v19 home-household follow-up affordance readback is projection-time only: read models derive `续接提示` / `换招提示` / `冷却提示` / `续接读回` from existing household fields and structured local response codes for the next command window. It adds no scheduler step, no same-command SocialMemory write, no cooldown ledger, and no thick household rule loop.
- v20 owner-lane return guidance is projection-time only: read models derive `外部后账归位`, `该走巡丁/路匪 lane`, `该走县门/文移 lane`, `该走族老/担保 lane`, and `本户不能代修` from existing household fields and structured local response codes. It adds no scheduler step, no same-command SocialMemory write, no owner-lane ledger, and no thick household rule loop.
- v21 owner-lane surface readback is projection-time only: read models copy that owner-lane return guidance into Office/Governance and Family-facing surfaces from existing structured household response fields. It adds no scheduler step, no same-command SocialMemory write, no owner-lane ledger, no household target field, and no thick household rule loop.
- v22 owner-lane handoff entry readback is projection-time only: read models append `承接入口` labels for existing owner-lane affordances without adding a scheduler step, command queue, command target, or persisted ledger.
- v23 owner-lane receipt status readback is projection-time only: read models append `归口状态` from existing owner-module structured response traces plus existing household local-response structure. `已归口` is not "社会其他人接手" and not automatic repair; it adds no scheduler step, no same-command SocialMemory write, no receipt-status ledger, no owner-lane ledger, and no thick household rule loop.
- v24 owner-lane outcome reading guidance is projection-time only: read models append `归口后读法` from existing owner-module outcome codes. It explains whether the owner-lane result reads as `已修复`, `暂压留账`, `恶化转硬`, or `放置未接`, but it adds no scheduler step, no same-command SocialMemory write, no outcome ledger, no receipt-status ledger, no owner-lane ledger, and no thick household rule loop.
- v25 owner-lane social-residue readback is projection-time only: read models append `社会余味读回` from existing SocialMemory response residue after the later monthly pass. It may show `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸`, but it adds no scheduler step, no same-command SocialMemory write, no SocialMemory ledger, no outcome ledger, no receipt-status ledger, no owner-lane ledger, and no thick household rule loop.
- v26 owner-lane social-residue follow-up guidance is projection-time only: read models append `余味冷却提示`, `余味续接提示`, or `余味换招提示` from existing SocialMemory response residue and owner-lane outcome traces after the later monthly pass. It may tell the player to let the owner lane and SocialMemory cool, lightly continue in the owner lane, switch owner-lane tactic, or wait for a better entry, but it adds no scheduler step, no same-command SocialMemory write, no follow-up ledger, no SocialMemory ledger, no outcome ledger, no receipt-status ledger, no owner-lane ledger, and no thick household rule loop.
- v27-v30 owner-lane closure is projection-time only: read models append `现有入口读法`, `后手收口读回`, and `闭环防回压` from existing SocialMemory response residue and owner-lane outcome traces. These cues prevent stale readback from pushing the player back to the home household, but add no scheduler step, no same-command SocialMemory write, no stale-guidance ledger, no follow-up ledger, no SocialMemory ledger, no outcome ledger, no receipt-status ledger, no owner-lane ledger, and no thick household rule loop.
- v37 office/yamen implementation drag uses the existing bounded month-end event drain: `WorldSettlements.CourtAgendaPressureAccumulated` can emit `OfficeAndCareer.PolicyWindowOpened`, and a later fresh-event drain round lets `OfficeAndCareer` convert that policy window into `OfficeAndCareer.PolicyImplemented` in the same month. This remains office-owned authority, mutates only existing office/yamen fields, and adds no scheduler phase, event pool, policy ledger, yamen workflow state, or UI rule path.
- v46-v52 Office-lane closure is projection-time only: read models append `Office承接入口`, `Office后手收口读回`, `Office余味续接读回`, and `Office闭环防回压` from existing Office snapshots, owner-response trace codes, and later structured SocialMemory cause keys. These cues keep county-yamen/document/clerk后手 in the Office lane and off the home household, but add no scheduler step, no same-command SocialMemory write, no policy/yamen ledger, no owner-lane ledger, no receipt-status ledger, no outcome ledger, no cooldown/follow-up ledger, and no thick household or county rule loop.
- v53-v60 Family-lane closure is projection-time only: read models append `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, `Family闭环防回压`, and `不是普通家户再扛` from existing Family snapshots, sponsor-clan household reads, owner-response trace codes, and later structured SocialMemory cause keys. These cues keep clan elder explanation, household guarantee, lineage-house face, and sponsor-clan pressure in the Family lane and off the ordinary home household, but add no scheduler step, no same-command SocialMemory write, no Family closure ledger, no guarantee ledger, no owner-lane ledger, no receipt-status ledger, no outcome ledger, no cooldown/follow-up ledger, and no thick household or clan rule loop.
- `ConflictAndForce.Lite` may also carry campaign-fatigue and escort-strain fallout across months; those penalties recover during its own owned pass

### Phase 3: month-end diff generation
Structured diff records created for:
- people
- households
- clans
- settlements
- institutions
- conflicts and campaigns if enabled

Diffs should be able to distinguish:
- day-local fluctuation
- month-level consolidation
- long-band structural change

### Phase 4: month-end projection and narrative
`NarrativeProjection`
- urgent / consequential / background grouping
- letters, reports, rumors, council prompts
- explanation trails
- projection may summarize day accumulation rather than dumping every day as separate spam

### Phase 5: player review and command
The player acts through bounded commands:
- issue commands
- mark watch items
- adjust policies
- choose local interventions
- no direct rewrite of world state

The normal rule remains:
- player review is monthly
- player action is monthly
- ordinary day steps are not separate player turns
- the player should usually choose after seeing the month-end projection, not after every internal day-level movement

Current public-life/order v3 note:
- home-household leverage, cost, and readback for public-life order commands are read at the monthly shell from current projections before command issue, then read again after the next monthly pass through command receipts and governance/order docket projections
- this does not make `xun` a lower authority grid and does not add a daily player turn; the authority mutation still belongs to the owning module command resolver and later monthly module passes

Current public-life/order v4 note:
- Month N accepted order commands such as `添雇巡丁` or `严缉路匪` mutate only `OrderAndBanditry` command receipt and carryover state at command time
- Month N+1 `SocialMemoryAndRelations` reads structured order aftermath through `IOrderAndBanditryQueries`, writes durable memory/narrative/climate residue, and exposes it to read models before projection

Current public-life/order v5 note:
- Month N public-life/order commands may resolve as accepted, partial, or refused inside `OrderAndBanditry`; the module owns the structured outcome/refusal/partial/trace codes and any carryover.
- Same-month command resolution must not mutate `SocialMemoryAndRelations`; Month N+1 SocialMemory reads prior-month structured Order trace before Order clears the carryover later in the monthly pass.
- The loop is rule-driven command / aftermath / social-memory readback, not an event-chain or event-pool design; `DomainEvent.Summary` and receipt prose are not rule input.
- this is a rule-driven monthly residue path over query-visible state, not an event pool and not `DomainEvent.Summary` parsing; UI and Unity receive only the projected residue/readback text

Current public-life/order v6 note:
- Month N refused or partial `添雇巡丁` / `严缉路匪` residue becomes a Month N+1 projected response surface, not an automatic authority mutation.
- The response command resolves in the owner module: `OrderAndBanditry` for road-watch / route-pressure repair, `OfficeAndCareer` for county-yamen / document-route repair, and `FamilyCore` for elder explanation / home-household guarantee repair.
- Same-month response command handling mutates only the owner module response trace and related owner pressure fields; it must not mutate `SocialMemoryAndRelations`.
- Month N+2 `SocialMemoryAndRelations` reads structured response aftermath from query snapshots and adjusts durable memory/narrative/climate residue. It must not parse `DomainEvent.Summary`, receipt text, `LastInterventionSummary`, or UI readback strings.
- Family response carryover remains visible long enough for SocialMemory because `FamilyCore` runs before `SocialMemoryAndRelations` in the current monthly order; duplicate memory writes are guarded by SocialMemory's existing cause checks rather than by UI timers.
- The projected readback may say whether the后账 was repaired, contained, worsened, or left aside, but the command result itself remains owner-owned state.

Current public-life/order v7 note:
- after Month N+2 response residue exists, later monthly SocialMemory passes may make it `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸` by changing only SocialMemory-owned memory weight, clan narrative pressure, and clan emotional climate.
- SocialMemory skips current-month response memories for drift, so recording the response residue and aging/hardening it remain separate deterministic month steps.
- later owner-module commands may read structured SocialMemory response memories and local clan scope as repeat-friction inputs. Order repair, yamen催办, and族老解释 still resolve in their owner module and do not write SocialMemory at command time.
- v7 adds no new persisted fields or migration; the cadence proof is over existing SocialMemory schema `3` plus v6 owner response trace fields.

Current public-life/order v8 note:
- after response residue exists, later monthly owner-module rules may read structured SocialMemory response memories and resolve small actor countermoves without a new player command. Order owns route-watch / runner self-movement, Office owns yamen / clerk docket movement, and Family owns elder / household-guarantee movement.
- actor countermoves skip memories whose `OriginDate` is the current month, and they read cause keys, outcome markers, weights, lifecycle state, source clan, and origin date rather than summaries.
- `OrderAndBanditry` and `OfficeAndCareer` currently run after `SocialMemoryAndRelations`, so their actor countermove traces are read by SocialMemory on the following monthly pass. `FamilyCore` runs before `SocialMemoryAndRelations`, so family actor traces are owner-state facts first and may be read in the same scheduler pass when response carryover remains visible; SocialMemory duplicate-cause guards keep this deterministic without UI timers.
- v8 adds no scheduler phase, no manager/controller layer, no persisted fields, and no migration. Application, UI, and Unity may only display projected readback.

Current public-life/order v9/v10/v11/v12 note:
- v9 hardens the same response afterlife by proving soft and hard actor-countermove readbacks plus minimum playable response affordances; it still adds no scheduler phase, manager/controller, or persisted fields.
- v10 adds ordinary-household visibility as a projection/readback layer: Month N refused / partial residue can become a Month N+1 household social-pressure signal showing night-road fear, runner/watch misunderstanding, labor/debt/migration strain, and yamen delay.
- ordinary-household v10 readback does not make `PopulationAndHouseholds` resolve public-life order commands and does not mutate household state during projection. The response commands still resolve in `OrderAndBanditry`, `OfficeAndCareer`, or `FamilyCore`, while SocialMemory residue remains owned by `SocialMemoryAndRelations`.
- v11 turns that same projected ordinary-household pressure into costed response choice and receipt text on the existing public-life response affordances. The projection may name the affected household, show the tradeoff, and say which owner module will resolve the response, but it does not add a household command target or compute whether the response works.
- v10/v11 add runtime read-model constants / projection enrichment only and no save/schema migration.
- v12 adds the first home-household local response commands after that projection: `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信`. They resolve through `PopulationAndHouseholds` at command time, mutate only household labor/debt/distress/migration plus local response trace fields, and still do not repair Order / Office / Family / SocialMemory aftermath.
- v12 same-month handling does not mutate `SocialMemoryAndRelations`; any durable shame/fear/favor/grudge/obligation residue remains SocialMemory-owned. Save impact is `PopulationAndHouseholds` schema `2 -> 3` with a local migration.
- v16 adds response-capacity readback on top of v12-v15: `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` can show bearable / risky / unfit `回应承受线` before command issue, and receipts can copy the resulting capacity readback. This remains a rule-driven command / aftermath / social-memory readback loop, not an event-pool design.
- v17 adds tradeoff forecast readback on top of v12-v16: `暂缩夜行`, `凑钱赔脚户`, and `遣少丁递信` can show expected benefit, recoil tail, and external-afteraccount boundary before command issue, and receipts can copy the resulting tradeoff readback. This remains a rule-driven command / aftermath / social-memory readback loop, not an event-pool design.
- v18 adds short-term consequence readback on top of v12-v17: receipts can say what was locally eased (`缓住项`), what got squeezed (`挤压项`), and which外部后账 remains outside household authority. This remains a rule-driven command / aftermath / social-memory readback loop, not an event-pool design.
- v19 adds follow-up affordance readback on top of v12-v18: the next projected local response surface can say whether repeating or switching a home-household move is续接, 换招, or should cool down. This remains a rule-driven command / aftermath / social-memory readback loop, not an event-pool design.
- v20 adds owner-lane return guidance on top of v12-v19: receipts and the next projected local response surface can say that Order, Office, Family, or SocialMemory still own the external after-account. This remains a rule-driven command / aftermath / social-memory readback loop, not an event-pool design.
- v21 carries owner-lane return guidance into Office/Governance and Family-facing readback surfaces on top of v20. This remains projection/readback guidance over existing state, not a new command system, event pool, or persisted ledger.
- v22 adds projected `承接入口` labels on top of v21 so those surfaces can point back to existing owner-lane command affordances. This does not add a scheduler step, command queue, command target, persisted ledger, or outcome calculation.
- v23 adds projected `归口状态` on top of v22 so those surfaces can say when an existing owner lane has already received a structured response trace. This remains projection/readback guidance, not a social takeover, new scheduler step, event pool, persisted ledger, or outcome calculation.
- v24 adds projected `归口后读法` on top of v23 so those surfaces can say how to read the existing owner-lane outcome after归口. This remains projection/readback guidance, not a new command system, event pool, persisted ledger, or outcome calculation.
- v25 adds projected `社会余味读回` on top of v24 so those surfaces can say whether the later SocialMemory pass has made the owner-lane residue read as `后账渐平`, `后账暂压留账`, `后账转硬`, or `后账放置发酸`. This remains projection/readback guidance, not a new command system, event pool, persisted ledger, or outcome/residue calculation.
- v26 adds projected `余味冷却提示` / `余味续接提示` / `余味换招提示` on top of v25 so those surfaces can say how to read the visible social residue as cool-down, owner-lane continuation, owner-lane tactic switch, or waiting. This remains projection/readback guidance, not a new command system, event pool, persisted ledger, or follow-up calculation.
- v27-v30 add projected `现有入口读法`, `后手收口读回`, and `闭环防回压` on top of v26 so owner-lane affordances and receipts read as a closure surface rather than a loop back to the home household. This remains projection/readback guidance, not a new command system, event pool, persisted ledger, or follow-up calculation.
- v32 adds backend event-contract health classification to the ten-year diagnostics. It classifies `DomainEvent` contract debt after simulation has run; it does not affect scheduler order, command resolution, module state, projection authority, or save/schema compatibility.
- v33 adds a no-unclassified gate to that same ten-year diagnostic pass. The assertion runs after simulation has produced runtime event counts and does not affect scheduler order, command resolution, module state, projection authority, or save/schema compatibility.
- v34 adds owner/evidence backlinks to that same diagnostic readback. The report formats `owner=<module>` from structured event keys and `evidence=<doc/test backlink>` from classification kind after simulation has run; it does not affect scheduler order, command resolution, module state, projection authority, UI/Unity authority, or save/schema compatibility.
- v35 turns `WorldSettlements.CanalWindowChanged` into a thin same-month owner-lane handoff. The bounded fresh-event drain lets `TradeAndIndustry` and `OrderAndBanditry` consume the structured canal-window fact and mutate only their own existing water/route pressure fields. This is still not an event pool, not a thick canal economy, and not a UI/Unity rule path.

Optional exception:
- extremely urgent red-band items may open a narrow interrupt-style response window
- these should stay rare and should not turn the game into a daily alert treadmill
- an interrupt should offer only the command surface justified by the crisis, then return to the monthly shell

### Phase 6: finalize month
- replay hash checkpoint
- autosave opportunity
- advance date

## Why this order exists

The world should not wait for the player to make it move.
But low-level life also should not wait a full month before it can bend, break, spread, or ask for help.

This structure gives:
- monthly readability
- sub-month life rhythm
- delayed but legible consequence
- room for poor, ordinary, or exposed starts to feel alive without getting deleted by one giant monthly settlement jump

## Diff requirements

Every month should still be able to answer:
- what changed?
- who changed?
- why did it change?
- what was a day-local fluctuation versus a true monthly shift?
- what can the player still do?

## Projection requirements

Narrative projection should not expose every day as equally noisy output.

Projection should usually:
- compress repeated day-level strain into one readable month-end summary
- surface only the day-level events that crossed an urgency or irreversibility threshold
- keep background day-level motion visible through trend wording, route heat, notices, almanac bands, and hotspot surfaces

## MVP minimum monthly heartbeat

For the MVP, the simulation must prove it can run the following minimal cycle deterministically:

```
Month N (example: 1100-03)
├── Prepare month
│   ├── PersonRegistry: age and life-stage preparation
│   └── staged command receipts, due timers, and feature manifest are loaded
├── Day authority steps, with quiet spans skipped or batched
│   ├── WorldSettlements: route reliability, ferry friction, safety, local availability
│   ├── PopulationAndHouseholds: subsistence, labor strain, illness progression, petty debt
│   ├── FamilyCore: support burden, urgent kin disruption, branch assistance pressure
│   ├── SocialMemoryAndRelations: rumor temperature, obligation, fear, shame, grudge heat
│   ├── enabled local modules: route price, public-life heat, yamen delay, conflict hotspot
│   └── receipts and hotspot staging
├── Month-end
│   ├── Domain event snapshot
│   ├── Diff generation (people, households, clans, settlements, institutions)
│   ├── NarrativeProjection: urgent / consequential / background grouping
│   └── Player review window opens
├── Player command phase
│   ├── Bounded household-seat commands issued through concrete channels
│   ├── Command receipts staged for Month N+1
│   └── Optional: mark watch items
└── Month finalization
    ├── Replay hash checkpoint
    ├── Autosave
    └── Advance date → Month N+1
```

### MVP determinism check
A 20-year headless run with the same seed must produce:
- Identical replay hash at month 240
- No divergence in `PersonRegistry` population count > 2%
- No impossible kinship states (incest, negative ages, orphaned minors without guardian)
- Valid save/load roundtrip at years 5, 10, 15, 20

### MVP command window
The player has a bounded command window each month. The MVP must support at least these command types:
- Family lifecycle: `议定承祧`, `议定丧次`, `议亲定婚`, `拨粮护婴`
- Trade/household: `GuaranteeDebt`, `FundStudy`, `EscortRoute`, `InvestEstate`
- Public/local: `PetitionYamen`, `RecommendPerson`, `Endure`

Each command is validated against:
1. **Authority check** — Does the player have the required influence (prestige, credit, office, lineage position)?
2. **Precondition check** — Does the world state satisfy the command's requirements?
3. **Cost reservation** — Are resources available (cash, grain, labor, reputation)?
4. **Autonomy check** — For commands targeting other households or adults, does the target accept or resist?
5. **Resolution** — Deterministic outcome based on world state + command + small bounded randomness (seeded)
6. **Receipt** — Structured receipt emitted as domain event, surfaced in projection next month

Commands that fail any of checks 1–4 produce a `CommandRejected` receipt with explanation trace, not a silent failure.

## Scheduler extension rule

A new module must declare:
- whether it runs at day cadence, monthly cadence, seasonal cadence, command resolution, or a declared combination
- which phase it participates in
- what events it emits
- what events it handles
- what projections it publishes

No module may insert ad hoc hidden execution outside the scheduler.

## Current backend household-family burden v36 note

- V36 uses the existing scheduler event-drain seam: `PopulationAndHouseholds` emits household burden facts, and `FamilyCore` consumes them in the bounded same-month fresh-event pass.
- The path is `HouseholdDebtSpiked` / `HouseholdSubsistencePressureChanged` / `HouseholdBurdenIncreased` -> `IPopulationAndHouseholdsQueries.GetRequiredHousehold(...)` -> sponsor-clan `FamilyCore` pressure fields.
- This is not a new scheduler phase, event pool, command system, or hidden recurring demand formula. It is a thin same-month handoff from structured household aftermath into existing family lifecycle pressure.
- Determinism depends on event type/entity/metadata, query snapshots, and fixed formulas only. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, or local-response summaries.

## Current backend office/yamen readback v38-v45 note

- V38-V45 keeps the v37 scheduler shape: court pressure may resolve into `OfficeAndCareer.PolicyImplemented`, then public-life and governance readbacks are projected from structured facts and snapshots.
- `PublicLifeAndRumor` consumes fresh office facts during the event-handling drain and updates existing public-life projection state. This is not a new scheduler phase, event pool, county formula, or clerk AI.
- `SocialMemoryAndRelations` writes office/yamen residue only on a later monthly pass from structured `JurisdictionAuthoritySnapshot` fields; same-month policy implementation handling must not mutate SocialMemory.
- Determinism depends on event type/entity/metadata, query snapshots, fixed ordering, and stable cause keys. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, `LastInterventionSummary`, or `LastLocalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for county-yamen, document drag, clerk delay, route pressure, or durable social residue.

## Current backend office-lane closure v46-v52 note

- V46-V52 keeps the same scheduler shape: no new phase, event pool, Office loop, or same-command SocialMemory write is introduced.
- Application projections read structured `JurisdictionAuthoritySnapshot` fields, existing Office response trace codes, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State` after owning modules have already resolved their state.
- Governance, docket, office, and receipt readback may show `Office承接入口`, `Office后手收口读回`, `Office余味续接读回`, and `Office闭环防回压`, but those strings are runtime guidance only.
- Determinism depends on query snapshots, fixed ordering, and stable cause keys. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, `LastPetitionOutcome`, `LastExplanation`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for Office, Order, Family, or SocialMemory after-accounts.

## Current backend Family-lane closure v53-v60 note

- V53-V60 keeps the same scheduler shape: no new phase, event pool, Family loop, or same-command SocialMemory write is introduced.
- Application projections read structured `ClanSnapshot`, `HouseholdPressureSnapshot`, `SponsorClanId`, existing Family response trace codes, and structured `SocialMemoryEntrySnapshot.CauseKey` / `Weight` / `State` after owning modules have already resolved their state.
- Public-life, family-facing, governance, docket, and receipt readback may show `Family承接入口`, `族老解释读回`, `本户担保读回`, `宗房脸面读回`, `Family后手收口读回`, `Family余味续接读回`, and `Family闭环防回压`, but those strings are runtime guidance only.
- Determinism depends on query snapshots, fixed ordering, and stable cause keys. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, projected Family prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for clan elder explanation, household guarantee, lineage-house face, sponsor-clan pressure, or SocialMemory after-accounts.

## Current backend Family relief choice v61-v68 note

- V61-V68 adds one command-time FamilyCore resolution, `GrantClanRelief`, and no scheduler phase, event pool, Family loop, or same-command SocialMemory write.
- The command reads the targeted clan and deterministic integer fields already owned by `FamilyCore`; it updates existing support/charity/branch/relief/mediation pressure fields and existing conflict receipt fields only.
- Application projections may show `Family救济选择读回`, `接济义务读回`, `宗房余力读回`, and `不是普通家户再扛` after the owning module state is available. Those strings are runtime guidance only.
- Determinism depends on command target, query snapshots, fixed ordering, and fixed formulas. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, projected Family prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for Family relief, lineage-house face, sponsor-clan pressure, or SocialMemory after-accounts.

## Current backend Force/Campaign/Regime owner-lane readback v69-v76 note

- V69-V76 keeps the same scheduler shape: no new phase, event pool, event-chain body, campaign loop, or same-command SocialMemory write is introduced.
- Application projections read structured campaign, force, office, clan, and SocialMemory snapshots after owning modules have already resolved their state.
- Governance, owner-lane docket, warfare affordances, and campaign receipts may show `军务承接入口`, `Force承接读回`, `战后后账读回`, `军务后手收口读回`, `军务余味续接读回`, and `军务闭环防回压`, but those strings are runtime guidance only.
- Determinism depends on query snapshots, fixed ordering, and stable cause keys. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, projected military prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for campaign aftermath, force readiness, military order, regime coordination, or SocialMemory after-accounts.

## Current backend Warfare directive choice depth v77-v84 note

- V77-V84 keeps command-time resolution inside the existing `WarfareCampaign` module. No scheduler phase, event pool, campaign loop, tactical layer, or same-command SocialMemory write is introduced.
- `DraftCampaignPlan`, `CommitMobilization`, `ProtectSupplyLine`, and `WithdrawToBarracks` update only existing active directive fields and `LastDirectiveTrace`; `ConflictAndForce`, `OfficeAndCareer`, `PopulationAndHouseholds`, and `SocialMemoryAndRelations` are not mutated by that command.
- Application projections may show `军令选择读回`, `案头筹议选择`, `点兵加压选择`, `粮道护持选择`, `归营止损选择`, `WarfareCampaign拥有军令`, and `军务选择不是县门文移代打` after the owning module state is available. Those strings are runtime guidance only.
- Determinism depends on command target, existing campaign/mobilization snapshots, fixed ordering, and fixed formulas. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, projected military prose, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for military directives, campaign aftermath, force readiness, county paperwork, or SocialMemory after-accounts.

## Current backend Warfare aftermath docket readback v85-v92 note

- V85-V92 keeps the scheduler shape unchanged: no new phase, event pool, campaign loop, tactical layer, post-battle cleanup command, or same-command SocialMemory write is introduced.
- Existing `WarfareCampaign` monthly/projection logic owns aftermath docket creation. Application reads `AftermathDocketSnapshot` lists after the owning module state is available and projects `战后案卷读回` plus docket-count labels.
- Unity reads projected aftermath docket snapshots/fields only; it must not infer docket contents from notifications, event traces, settlement stats, receipt prose, or `DocketSummary`.
- Determinism depends on existing campaign state, structured docket lists, fixed ordering, and stable read-model assembly. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, projected military prose, `LastDirectiveTrace`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for campaign aftermath dockets, merits/blames, relief needs, route repairs, or SocialMemory after-accounts.

## Current backend court-policy process readback v93-v100 note

- V93-V100 keeps the scheduler shape unchanged: no new phase, event pool, court loop, policy formula, dispatch ledger, or same-command SocialMemory write is introduced.
- The existing same-month drain remains `CourtAgendaPressureAccumulated -> PolicyWindowOpened -> PolicyImplemented`, with `PublicLifeAndRumor` handling structured public readback afterward. Application reads existing `JurisdictionAuthoritySnapshot` and `SettlementPublicLifeSnapshot` values after owning modules resolve their state.
- Governance, docket, office, and desk surfaces may show `朝议压力读回`, `政策窗口读回`, `文移到达读回`, `县门执行承接读回`, `公议读法读回`, and `Court-policy防回压`, but those strings are runtime guidance only.
- Determinism depends on structured snapshots, fixed ordering, and stable read-model assembly. It must not use wall-clock time, random UI state, `DomainEvent.Summary`, receipt prose, `LastAdministrativeTrace`, `LastPetitionOutcome`, `OfficialNoticeLine`, `PrefectureDispatchLine`, `LastInterventionSummary`, `LastLocalResponseSummary`, or `LastRefusalResponseSummary`.
- The ordinary household line remains a bounded low-power local response surface. It is not a universal repair path for court-policy after-accounting, county-yamen implementation, public legitimacy, or SocialMemory after-accounts.

## Current thin-chain closeout audit v101-v108 note

- V101-V108 changes no scheduler behavior. It adds no phase, event drain, command cadence, projection pass, migration path, or runtime authority.
- The closeout means the v3-v100 thin-chain skeleton has enough scheduler/projection evidence to be treated as closed for topology and readback purposes. It does not mean full historical/social formulas are implemented.
- Future thickening must still name its cadence, repetition guard, pressure locus, owner module, no-touch boundary, and validation lane. The closeout must not be used as a shortcut for broad fanout, summary parsing, or UI-owned rule work.

## Current backend court-policy process thickening v109-v116 note

- V109-V116 keeps the v37/v93 scheduler shape: `CourtAgendaPressureAccumulated -> PolicyWindowOpened -> PolicyImplemented -> PublicLifeAndRumor` can drain through existing owner-lane handling, with no new phase, event pool, court loop, policy ledger, or same-command SocialMemory write.
- `OfficeAndCareer` resolves policy tone, document direction, and county-gate implementation posture from existing structured policy-window / implementation metadata and office state. `PublicLifeAndRumor` resolves public interpretation from structured `PolicyImplemented` metadata and public-life scalar state.
- Application may project `政策语气读回`, `文移指向读回`, `县门承接姿态`, `公议承压读法`, `朝廷后手仍不直写地方`, and `不是本户硬扛朝廷后账` only after owner modules have resolved. UI/Unity copy these fields only.
- SocialMemory durable residue remains later-month only and must read structured aftermath. The same-month court-policy process must not write SocialMemory, parse projection prose, or treat a household as carrying the court after-account.

## Current backend court-policy local response v117-v124 note

- V117-V124 keeps the same scheduler shape and adds no new phase, event pool, court loop, policy ledger, or same-command SocialMemory write.
- When existing office scalar pressure shows policy-process strain, `OfficeAndCareer` may resolve the reused `PressCountyYamenDocument` / `RedirectRoadReport` commands as local document/report continuations. This remains deterministic owner-lane command handling, not Application policy success calculation.
- Application may project `政策回应入口`, `文移续接选择`, `县门轻催`, `递报改道`, `公议降温只读回`, and `不是本户硬扛朝廷后账` only from structured snapshots and existing command affordance/receipt fields. UI/Unity copy these fields only.
- Same-month handling must not write durable SocialMemory residue. Future residue readers must use structured aftermath and must not parse court-policy projection prose or public-life notice/dispatch wording.
## Current backend social mobility fidelity ring v213-v244 note

- V213-V244 keeps the existing monthly/xun cadence. `PopulationAndHouseholds` updates household pressure, applies first-layer livelihood drift, synchronizes member activity, and rebuilds settlement labor/marriage/migration pools in deterministic order.
- Pressure-triggered near readback is bounded: a hot household can ask `PersonRegistry` to move a small number of regional members into `Local` fidelity, but the wider world remains summarized through pools.
- Application and Unity see only projections after owner modules resolve: `FidelityScaleSnapshot`, `SettlementMobilitySnapshot`, person dossier movement/fidelity readbacks, and debug scale counters.
- Determinism depends on stable `SettlementId`, `HouseholdId`, and `PersonId` ordering. No wall-clock state, UI state, summary parsing, ledger, or new scheduler phase is introduced.

## Current backend social mobility fidelity ring closeout v245-v252 note

- V245-V252 adds no scheduler step, cadence change, event-drain rule, runtime cache, or simulation formula. It is a closeout audit for the v213-v244 first-layer fidelity substrate.
- The closed branch proves a bounded monthly/xun path over existing population and registry state; it does not prove full per-person world simulation, full migration economy, demotion/dormant-stub handling, or durable movement residue.
- Future performance work must name touched counts, deterministic ordering, caps, cache invalidation, save/schema impact, and a validation lane before changing the fidelity loop.

## Current social mobility scale-budget guard v269-v276 note

- V269-V276 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, or projection field.
- It makes the simulation budget rule explicit for future work: close orbit can run as named detail, player influence or active pressure can promote bounded selective detail, active regions should usually use structured pools and settlement summaries, and the distant world should remain pressure-summary simulation rather than per-person hard ticks.
- Future mobility/personnel-flow branches must name hot path, expected cardinality, deterministic order, cap strategy, owner module, schema impact, and validation lane before increasing fidelity.

## Current social mobility influence readback v277-v284 note

- V277-V284 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, or persistence change.
- The new readbacks explain existing simulation scale: `InfluenceFootprintReadbackSummary` says why close/local/distant people are readable at that precision, and `ScaleBudgetReadbackSummary` says whether a settlement is named local detail, pressure-selected detail, active-region pool, or distant summary.
- Determinism depends only on already-built snapshots and stable projection ordering. Future readers must not parse these strings as input to scheduler, commands, SocialMemory, or UI decisions.

## Current social mobility boundary closeout v285-v292 note

- V285-V292 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, or persistence change.
- The closeout states that v213-v284 is bounded by four precision bands: close-orbit named detail, pressure-selected local detail, active-region structured pools, and distant pressure summary.
- Future mobility/personnel-flow work must name hot path, expected cardinality, deterministic order/cap, owner module, schema impact, no-touch boundary, and validation lane before adding rule density.
- Future readers must not parse readback strings, person dossier prose, settlement mobility text, notification prose, receipt prose, or `DomainEvent.Summary` as scheduler or command input.

## Current personnel command preflight v293-v300 note

- V293-V300 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, command route, or persistence change.
- Future personnel-flow commands must be bounded intents resolved by an owner module after it declares target scope, hot path, expected cardinality, deterministic order/cap, no-touch boundary, schema impact, and validation lane.
- Application must route only. UI/Unity must not compute movement success, target ranking, assignment outcome, or precision changes.
- Future readers must not parse command labels, readback strings, person dossier prose, settlement mobility text, notification prose, receipt prose, or `DomainEvent.Summary` as scheduler or command input.

## Current personnel flow command readiness v301-v308 note

- V301-V308 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, command route, or persistence change.
- The new `PersonnelFlowReadinessSummary` is projection-time readback over existing `PopulationAndHouseholds` household fields and local response command identity.
- It may explain near detail and far summary, but it does not promote/demote people, move people, choose personnel targets, write SocialMemory residue, or change `PersonRegistry`.
- UI/Unity must copy the field only and must not parse it to compute movement success.

## Current personnel flow surface echo v309-v316 note

- V309-V316 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, command route, or persistence change.
- `PlayerCommandSurfaceSnapshot.PersonnelFlowReadinessSummary` is projection-time echo over already projected command readiness fields.
- It may explain that personnel-flow command readiness exists on the current surface, but it does not promote/demote people, move people, choose personnel targets, write SocialMemory residue, or change `PersonRegistry`.
- UI/Unity must display the projected echo only and must not parse it to compute movement success.

## Current personnel flow readiness closeout v317-v324 note

- V317-V324 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, command route, or persistence change.
- It closes v293-v316 only as a first personnel-flow command-readiness layer.
- It does not complete migration, social mobility, office service, campaign manpower, or household relocation systems.
- Future personnel-flow work must still open a new owner-lane plan before adding state, schema, or rule density.

## Current personnel flow owner-lane gate v325-v332 note

- V325-V332 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, command route, or persistence change.
- `PlayerCommandSurfaceSnapshot.PersonnelFlowOwnerLaneGateSummary` is projection-time readback over structured command affordance/receipt metadata.
- It names current and future owner lanes, but it does not promote/demote people, move people, choose personnel targets, write SocialMemory residue, or change `PersonRegistry`.
- UI/Unity must display the projected gate only and must not parse it to compute movement success.

## Current personnel flow desk gate echo v333-v340 note

- V333-V340 adds no scheduler step, cadence change, event-drain rule, runtime cache, simulation formula, command, command route, or persistence change.
- Desk Sandbox displays the projected owner-lane gate only when the settlement has local structured personnel-flow readiness command affordances or receipts.
- It does not promote/demote people, move people, choose personnel targets, write SocialMemory residue, or change `PersonRegistry`.
- UI/Unity must display the projected local echo only and must not parse it to compute movement success.

## Current personnel flow desk gate containment v341-v348 note

- V341-V348 adds a negative containment proof around the desk echo. A global owner-lane gate summary does not make every settlement locally active.
- The simulation remains unchanged: only settlements with structured local public-life personnel-flow readiness affordances or receipts may show the desk gate echo.
- Quiet or distant settlements remain pool summaries. No monthly rule, movement resolver, person assignment path, migration economy, SocialMemory residue, schema, migration, ledger, or prose parser is added.

## Current personnel flow gate closeout v349-v356 note

- V349-V356 closes v325-v348 as a projection/readback layer only.
- The simulation still has no new scheduler step, cadence rule, event-drain rule, movement resolver, person assignment path, office-service lane, campaign-manpower lane, migration economy, SocialMemory residue, schema, migration, ledger, or prose parser.
- Future personnel-flow simulation depth must choose an owner lane and document state, cadence, cardinality, deterministic cap/order, schema impact, and validation before implementation.

## Current personnel flow future owner-lane preflight v357-v364 note

- V357-V364 adds no simulation behavior. It is a guard for future personnel-flow owner lanes.
- Future Family, Office, or Warfare personnel-flow rules must choose owner module state, accepted command, cadence, target scope, no-touch boundary, hot path, expected cardinality, deterministic cap/order, schema impact, projection/readback, and validation before implementation.
- The current simulation still has no office-service personnel movement, kin transfer, campaign-manpower allocation, direct assignment, movement resolver, migration economy, future-owner-lane ledger, or prose parser.

## Current personnel flow future lane surface v365-v372 note

- V365-V372 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, or persistence change.
- The new Great Hall readback is projection-time surface text over existing personnel-flow readiness affordance/receipt fields.
- It does not promote/demote people, move people, select targets, allocate office service, allocate campaign manpower, write SocialMemory residue, or change `PersonRegistry`.
- Future readers must not parse this surface text as scheduler, command, SocialMemory, UI, Unity, or owner-lane authority.

## Current personnel flow future lane closeout v373-v380 note

- V373-V380 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, or persistence change.
- It closes v357-v372 as preflight visibility only, not as a movement, assignment, office-service, kin-transfer, campaign-manpower, durable-residue, or migration simulation.
- Future personnel-flow simulation depth must choose one owner lane and document state, cadence, cardinality, deterministic cap/order, schema impact, and validation before implementation.

## Current commoner social position preflight v381-v388 note

- V381-V388 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, or persistence change.
- It records that commoner / class-position mobility must remain scale-budgeted: near households can be read through existing household/person dossier projections, while distant society remains summarized by pools and pressure carriers.
- Future commoner status drift must choose one owner lane and document pressure carrier, state, cadence, target scope, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation before implementation.
- It does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, or change `PersonRegistry`.

## Current commoner social position readback v389-v396 note

- V389-V396 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, or persistence change.
- It projects `SocialPositionReadbackSummary` on person dossiers from existing owner snapshots so nearby people read as socially situated without turning the world into a global per-person career simulation.
- The readback does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, or change `PersonRegistry`.
- Future status drift still needs one owner lane with state, cadence, target scope, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation before implementation.

## Current social position owner-lane keys v397-v404 note

- V397-V404 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, or persistence change.
- It projects `SocialPositionSourceModuleKeys` on person dossiers from existing owner snapshots so future surfaces can read source provenance without parsing social-position prose.
- The key list does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, or change `PersonRegistry`.
- Future status drift still needs one owner lane with state, cadence, target scope, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation before implementation.

## Current social position readback closeout v405-v412 note

- V405-V412 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, or persistence change.
- It closes v381-v404 as preflight/readback/source-key evidence only, not as commoner status drift or a global per-person career simulation.
- The closeout does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, or change `PersonRegistry`.
- Future status drift still needs one owner lane with state, cadence, target scope, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation before implementation.

## Current social position scale budget v413-v420 note

- V413-V420 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, precision mutation, or persistence change.
- It projects `SocialPositionScaleBudgetReadbackSummary` on person dossiers from existing `FidelityRing` and structured source keys, so the player can read "near detail, far summary" without assuming all-world per-person class simulation.
- The readback does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, or change `PersonRegistry`.
- Future status drift still needs one owner lane with state, cadence, target scope, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation before implementation.

## Current social position regional scale guard v421-v428 note

- V421-V428 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, precision mutation, or persistence change.
- It verifies the existing regional readback: a registry-only `FidelityRing.Regional` dossier remains `regional summary` and registry-only source.
- The guard does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, or change `PersonRegistry`.
- Future regional precision changes still need one owner lane with state, cadence, target scope, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation before implementation.

## Current social position scale closeout v429-v436 note

- V429-V436 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, precision mutation, selection rule, or persistence change.
- It closes v381-v428 as first-layer visibility evidence: future-lane contract, readback, source keys, scale budget, regional guard, and Unity copy-only evidence.
- The closeout does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, choose regional people, or change `PersonRegistry`.
- Future status depth still needs one owner lane with state, cadence, target scope, hot path, cardinality, deterministic cap/order, schema impact, projection fields, and validation before implementation.

## Current commoner status owner-lane preflight v437-v444 note

- V437-V444 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, precision mutation, selection rule, or persistence change.
- It recommends `PopulationAndHouseholds` as the first future runtime owner lane for commoner status depth because that module already owns household livelihood/activity/pools and pressure carriers.
- The preflight does not promote/demote people, resolve zhuhu/kehu conversion, select office service, assign trade attachment, write durable social-position residue, choose regional people, or change `PersonRegistry`.
- Future implementation still needs `PopulationAndHouseholds` state/cadence/target-scope/no-touch/hot-path/cardinality/cap-order/schema/projection/validation before code lands.

## Current fidelity scale budget preflight v445-v452 note

- V445-V452 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, precision mutation, selection rule, or persistence change.
- It documents the scale budget: close/pressure-hit actors may become readable through owner-laned projection; distant society stays summarized through pools and pressure carriers.
- The preflight does not promote/demote people, mutate fidelity rings, run a global person scan, choose regional people, resolve zhuhu/kehu conversion, or change `PersonRegistry`.
- Future implementation still needs target scope, hot path, touched counts, deterministic cap/order, cadence, schema impact, projection fields, validation, and no-touch boundary before code lands.

## Current household mobility dynamics explanation v453-v460 note

- V453-V460 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, fidelity mutation, selection rule, or persistence change.
- It projects a structured explanation from existing household social-pressure signals and `PopulationAndHouseholds` carriers so the player can read the current multi-dimensional mobility posture.
- The explanation does not promote/demote people, resolve zhuhu/kehu conversion, select office service, attach trade work, write durable social-position residue, choose regional people, or change `PersonRegistry`.
- Future mobility/status depth still needs owner state, cadence, target scope, hot path, touched counts, deterministic cap/order, schema impact, projection fields, validation, and no-touch boundary before code lands.

## Current household mobility dynamics closeout v461-v468 note

- V461-V468 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, fidelity mutation, movement selector, or persistence change.
- The closeout records that v453-v460 is complete only as a first household mobility explanation layer: existing household signals, deterministic dimension keys, projected explanation, and Desk Sandbox copy-only display.
- The closeout does not promote/demote people, resolve zhuhu/kehu conversion, select office service, attach trade work, write durable movement residue, choose regional people, create route history, or change `PersonRegistry`.
- Future mobility/status depth still needs owner state, cadence, target scope, hot path, touched counts, deterministic cap/order, schema impact, projection fields, validation, and no-touch boundary before code lands.

## Current household mobility owner-lane preflight v469-v476 note

- V469-V476 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, fidelity mutation, movement selector, route-history model, or persistence change.
- Future household mobility runtime depth should start from `PopulationAndHouseholds` unless a later ExecPlan proves another owner lane; that module already owns household livelihood, activity, distress, debt, labor, grain, land, migration pressure, and pool carriers.
- The future rule must declare cadence, target scope, hot path, touched counts, deterministic cap/order, no-touch boundary, schema impact, projection fields, and validation before code lands.
- The scale budget remains near detail, far summary: player-near and pressure-hit households may become readable through owner-laned rules, while distant society remains pool/settlement pressure summary until explicitly promoted.

## Current household mobility preflight closeout v485-v492 note

- V485-V492 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, fidelity mutation, movement selector, route-history model, durable residue, or persistence change.
- It closes v469-v476 as gate evidence only: owner-lane requirements are documented and guarded, but no household movement or migration economy rule is active.
- Future household mobility simulation depth must still choose one owner lane and document state, cadence, cardinality, deterministic cap/order, schema impact, no-touch boundary, projection fields, and validation before implementation.
- The closeout preserves near detail, far summary and does not promote distant society into per-person or per-household runtime detail.

## Current household mobility runtime rules-data readiness v501-v508 note

- V501-V508 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, movement selector, route-history model, durable residue, rules-data loader, or persistence change.
- It records a first runtime rule readiness map: future authority should be monthly-first, `PopulationAndHouseholds`-owned, deterministic, bounded by fanout, and visible through near detail / far summary.
- Existing xun behavior remains existing household-pressure cadence context. Xun-band grouping is not promoted into a new household movement authority path.
- Future implementation must cap and order candidate households/pools/settlements deterministically, leave quiet/off-scope/distant pooled society untouched, and stop for schema/migration review if persisted state is required.
- The hardcoded extraction map is documentation only: thresholds, weights, caps, recovery/decay rules, regional/era assumptions, and pool limits are candidates for later owner-consumed authored rules-data.

## Current household mobility rules-data contract preflight v509-v516 note

- V509-V516 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, movement selector, route-history model, durable residue, rules-data loader, validator implementation, or persistence change.
- It defines a future rules-data contract for owner-consumed household mobility parameters: stable ids, schema/version, deterministic ordering, default fallback, readable validation errors, no UI/Application authority, and no arbitrary script/plugin execution.
- Future parameter categories are threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and deterministic tie-break priorities.
- Because no reusable runtime rules-data/content/config pattern exists in this repo today, this pass does not add a default file or loader.
- The future consumer remains `PopulationAndHouseholds`; Application, UI, Unity, docs text, projection prose, public-life lines, and `DomainEvent.Summary` remain outside authority.

## Current household mobility default rules-data skeleton v517-v524 note

- V517-V524 adds no simulation behavior, scheduler step, cadence change, event-drain rule, runtime cache, command route, resolver, movement selector, route-history model, durable residue, default rules-data file, loader, validator implementation, or persistence change.
- It defines only the future default skeleton shape: `ruleSetId`, `schemaVersion`, `ownerModule`, `defaultFallbackPolicy`, `parameterGroups`, `validationResult`, and deterministic declaration order.
- Future parameter groups are threshold bands, pressure weights, regional modifiers, era/scenario modifiers, recovery/decay rates, fanout caps, and tie-break priorities.
- The skeleton does not enter save, does not change current hardcoded behavior, and does not create `content/rules-data`.
- The future consumer remains `PopulationAndHouseholds`; Application, UI, Unity, docs text, projection prose, public-life lines, and `DomainEvent.Summary` remain outside authority.

## Current household mobility first hardcoded rule extraction v525-v532 note

- V525-V532 changes runtime code shape but preserves behavior: the focused member promotion fanout cap is now owner-consumed from `PopulationHouseholdMobilityRulesData`, with the default still 2.
- It adds no scheduler step, cadence change, event-drain rule, command route, movement selector, route-history model, durable residue, rules-data loader, default file, or persistence change.
- Determinism is preserved by keeping the existing monthly pass, household-id grouping/order, person-id order, and same cap value.
- Malformed owner rules data falls back to the same default cap; no Application/UI/Unity path can calculate movement or target eligibility from the parameter.

## Current household mobility first runtime rule v533-v540 note

- V533-V540 adds a monthly owner rule after the existing `PopulationAndHouseholds` pool rebuild. It reads active `MigrationPools` and capped household candidates from existing state.
- Deterministic target selection is pool outflow pressure descending, settlement id, then household pressure score descending, household id.
- Bounded fanout defaults to one active pool and two households, with a one-point existing migration-risk nudge. Malformed monthly cap/threshold/delta data falls back deterministically.
- The rule creates no scheduler phase, command route, movement selector, route-history model, durable residue, migration economy, class/status engine, rules-data loader, default file, or persistence change.
- No Application/UI/Unity path calculates target eligibility or household mobility outcome; public shells may only display owner-projected fields.

## Current household mobility first runtime rule closeout v541-v548 note

- V541-V548 adds no scheduler phase, cadence change, runtime rule, event-drain rule, command route, movement selector, route-history model, durable residue, migration economy, class/status engine, rules-data loader, default file, or persistence change.
- The closeout records that V533-V540 remains the only first runtime rule in this track, and it stays monthly, owner-only, deterministic, and capped.
- Future household mobility expansion must declare whether it changes runtime behavior, projection, save/schema, fanout, cadence, or no-touch boundaries before implementation.
- Application/UI/Unity continue to display projected owner fields only and must not calculate target eligibility or household mobility outcome.

## Current household mobility runtime rule health evidence v549-v556 note

- V549-V556 adds no scheduler phase, cadence change, runtime rule, event-drain rule, command route, movement selector, route-history model, durable residue, migration economy, class/status engine, rules-data loader, default file, persistence change, long-run saturation tuning, or performance optimization.
- The health-evidence pass records the next gate before widening: touched household/pool/settlement counts, deterministic cap/order, same-seed replay, no-touch proof, pressure-band interpretation, and hot-path/cardinality notes.
- The V533-V540 rule remains monthly, owner-only, deterministic, and capped; the existing `MigrationStarted` event remains threshold evidence only.
- Application/UI/Unity continue to display projected owner fields only and must not calculate target eligibility, health classification, or household mobility outcome.

## Current household mobility runtime widening gate v557-v564 note

- V557-V564 adds no scheduler phase, cadence change, runtime rule, event-drain rule, command route, movement selector, route-history model, durable residue, migration economy, class/status engine, rules-data loader, default file, persistence change, counters, caches, long-run saturation tuning, or performance optimization.
- The widening gate requires a later implementation PR to name target scope, current/proposed touched household/pool/settlement counts, deterministic cap/order, same-seed replay, no-touch proof, pressure-band interpretation, schema decision, and validation lane before changing behavior.
- The V533-V540 rule remains monthly, owner-only, deterministic, and capped at the current defaults; the existing `MigrationStarted` event remains threshold evidence only.
- Application/UI/Unity continue to display projected owner fields only and must not calculate target eligibility, touched counts, health classification, performance status, or household mobility outcome.

## Current household mobility runtime touch-count proof v565-v572 note

- V565-V572 adds focused owner-test evidence, not a scheduler phase, cadence change, runtime rule, event-drain rule, command route, movement selector, route-history model, durable residue, migration economy, class/status engine, rules-data loader, default file, persistence change, counters, caches, long-run saturation tuning, or performance optimization.
- The test proves the current default fixture touches exactly two eligible households in one selected active pool while leaving the lower-priority selected-pool candidate, quiet household, and lower-priority active pool untouched.
- The proof derives touched counts in tests by comparing existing risk deltas against a zero-risk-delta baseline. Runtime state still has no touched-count field, diagnostic state, performance cache, or projection-owned counter.
- Application/UI/Unity continue to display projected owner fields only and must not calculate target eligibility, touched counts, health classification, performance status, or household mobility outcome.

## Current household mobility rules-data fallback matrix v573-v580 note

- V573-V580 adds focused fallback evidence, not a scheduler phase, cadence change, runtime rule, event-drain rule, command route, movement selector, route-history model, durable residue, migration economy, class/status engine, rules-data loader, default file, persistence change, counters, caches, long-run saturation tuning, or performance optimization.
- Malformed runtime threshold/cap/delta rules-data validates with readable errors and falls back to defaults.
- The owner-result proof requires malformed runtime rules-data to produce the same monthly signature as default rules-data, keeping fallback inside `PopulationAndHouseholds`.
- Application/UI/Unity continue to display projected owner fields only and must not calculate validation fallback, target eligibility, touched counts, health classification, performance status, or household mobility outcome.
## Current household mobility runtime threshold no-touch v581-v588 note

V581-V588 proves the existing first household mobility runtime rule remains bounded when active-pool thresholding blocks selection. The owner test compares the threshold-blocked run with a zero-risk-delta baseline, checks that fixture pools stay below the maximum threshold, and verifies no `Household mobility pressure` diff entries are produced.

No scheduler cadence, runtime formula, fanout cap, ordering rule, save schema, route-history model, or movement command changes.
## Current household mobility runtime zero-cap no-touch v589-v596 note

V589-V596 proves the existing first household mobility runtime rule remains bounded when zero fanout caps block selection. The owner test compares settlement-cap-blocked and household-cap-blocked runs with a zero-risk-delta baseline and verifies no `Household mobility pressure` diff entries are produced.

No scheduler cadence, runtime formula, fanout widening, ordering rule, save schema, route-history model, or movement command changes.
## Current household mobility runtime zero-risk-delta no-touch v597-v604 note

V597-V604 proves the existing first household mobility runtime rule remains bounded when zero risk delta blocks target mutation. The owner test compares a risk-delta-blocked run with a cap-blocked no-touch baseline and verifies no `Household mobility pressure` diff entries are produced.

No scheduler cadence, runtime formula, fanout widening, ordering rule, save schema, route-history model, or movement command changes.
## Current household mobility runtime candidate-filter no-touch v605-v612 note

V605-V612 proves the existing first household mobility runtime rule remains bounded when candidate filters exclude already-migrating/high-risk households and households below the candidate floor. The owner test verifies filtered households receive no `Household mobility pressure` diff while the remaining eligible candidate is selected deterministically.

No scheduler cadence, runtime formula, fanout widening, ordering rule, save schema, route-history model, or movement command changes.
## Current household mobility runtime tie-break no-touch v613-v620 note

V613-V620 proves the existing first household mobility runtime rule remains bounded when equal-score candidates reach the ordering step. The owner test verifies the tied lower household id receives the cap-one `Household mobility pressure` diff while the tied higher household id remains no-touch.

No scheduler cadence, runtime formula, fanout widening, ordering retune, score retune, save schema, route-history model, or movement command changes.
## Current household mobility runtime pool tie-break no-touch v621-v628 note

V621-V628 proves the existing first household mobility runtime rule remains bounded when equal-outflow active pools reach the ordering step. The owner test verifies the lower settlement id receives the cap-one pool pass while the tied higher settlement id and its households remain no-touch.

No scheduler cadence, runtime formula, fanout widening, pool ordering retune, threshold retune, save schema, route-history model, or movement command changes.
## Current household mobility runtime score-ordering no-touch v629-v636 note

V629-V636 proves the existing first household mobility runtime rule remains bounded when score ordering outranks lower household id under cap one. The owner test verifies the higher-score candidate receives the `Household mobility pressure` diff while the lower household id remains no-touch.

No scheduler cadence, runtime formula, fanout widening, score formula retune, candidate ordering retune, save schema, route-history model, or movement command changes.

## Current household mobility runtime pool-priority no-touch v637-v644 note

V637-V644 proves the existing first household mobility runtime rule remains bounded when active-pool priority is applied before cross-pool household score comparison. The owner test verifies a higher-scoring household in a lower-priority unselected pool receives no `Household mobility pressure` diff under settlement cap one.

No scheduler cadence, runtime formula, fanout widening, pool ordering retune, score formula retune, candidate ordering retune, threshold retune, save schema, route-history model, or movement command changes.

## Current household mobility runtime per-pool cap no-touch v645-v652 note

V645-V652 proves the existing first household mobility runtime rule remains bounded when household cap application is scoped inside each selected active pool. The owner test verifies settlement cap two and household cap one touch one deterministic household in each selected pool while lower-score households in those pools receive no `Household mobility pressure` diff.

No scheduler cadence, runtime formula, fanout widening, cap semantics retune, global household cap, pool ordering retune, score formula retune, candidate ordering retune, threshold retune, save schema, route-history model, or movement command changes.

## Current household mobility runtime threshold-event no-touch v653-v660 note

V653-V660 proves the existing first household mobility runtime rule keeps threshold-event emission bounded to selected crossing households. The owner test verifies a capped selected household crossing from 79 to 80 emits the existing `MigrationStarted` structured event, while unselected/off-cap households emit no threshold event and receive no `Household mobility pressure` diff.

No scheduler cadence, runtime formula, fanout widening, new event type, event routing change, threshold retune, cap semantics retune, save schema, route-history model, or movement command changes.

## Current household mobility runtime event-metadata no-prose v661-v668 note

V661-V668 proves the existing first household mobility runtime rule keeps threshold-event interpretation structured. The owner test verifies cause, settlement id, and household id are read from `Metadata`, while `Summary` remains prose and is not needed to recover authority ids or cause.

No scheduler cadence, runtime formula, fanout widening, new event type, event routing change, prose parser, threshold retune, cap semantics retune, save schema, route-history model, or movement command changes.

## Current household mobility runtime event-metadata replay v669-v676 note

V669-V676 proves the existing first household mobility runtime rule keeps selected threshold-event metadata replay-stable under same-seed owner runs. The owner test compares event type, entity key, cause, settlement id, household id, and downstream summary as test evidence only.

No scheduler cadence, runtime formula, fanout widening, new event type, event routing change, replay state, threshold retune, cap semantics retune, save schema, route-history model, or movement command changes.

## Current household mobility runtime threshold extraction v677-v684 note

V677-V684 changes the first household mobility runtime rule's threshold event check from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate filtering, deterministic ordering, fanout caps, event type, and default threshold behavior remain unchanged.

No scheduler cadence change, fanout widening, candidate filter retune, general migration-state retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime candidate-floor extraction v685-v692 note

V685-V692 changes the first household mobility runtime rule's low-risk candidate floor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, high-risk filtering, deterministic ordering, fanout caps, event type, and default floor behavior remain unchanged.

No scheduler cadence change, fanout widening, high-risk filter retune, general migration-state retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime score-weight extraction v693-v700 note

V693-V700 changes the first household mobility runtime rule's migration-risk score weight from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate filters, deterministic ordering, fanout caps, event type, and default score behavior remain unchanged.

No scheduler cadence change, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime labor-floor extraction v701-v708 note

V701-V708 changes the first household mobility runtime rule's labor-capacity pressure floor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate filters, deterministic ordering, fanout caps, event type, and default labor-pressure behavior remain unchanged.

No scheduler cadence change, labor model retune, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime grain-floor extraction v709-v716 note

V709-V716 changes the first household mobility runtime rule's grain-store pressure floor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate filters, deterministic ordering, fanout caps, event type, and default grain-pressure behavior remain unchanged.

No scheduler cadence change, grain economy retune, grain pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime land-floor extraction v717-v724 note

V717-V724 changes the first household mobility runtime rule's land-holding pressure floor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate filters, deterministic ordering, fanout caps, event type, and default land-pressure behavior remain unchanged.

No scheduler cadence change, land economy retune, land pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime grain-divisor extraction v725-v732 note

V725-V732 changes the first household mobility runtime rule's grain-store pressure divisor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate filters, deterministic ordering, fanout caps, event type, and default grain-pressure behavior remain unchanged.

No scheduler cadence change, grain economy retune, grain floor retune, land pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime land-divisor extraction v733-v740 note

V733-V740 changes the first household mobility runtime rule's land-holding pressure divisor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate filters, deterministic ordering, fanout caps, event type, and default land-pressure behavior remain unchanged.

No scheduler cadence change, land economy retune, land floor retune, grain pressure divisor extraction, score formula retune beyond literal extraction, fanout widening, filter retune, threshold retune, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime candidate-ceiling extraction v741-v748 note

V741-V748 changes the first household mobility runtime rule's high-risk candidate ceiling from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate floor, deterministic ordering, fanout caps, event type, and default high-risk no-touch behavior remain unchanged.

No scheduler cadence change, migration-started event threshold retune, candidate floor retune, trigger threshold extraction, score formula retune beyond literal extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime distress-trigger extraction v749-v756 note

V749-V756 changes the first household mobility runtime rule's distress trigger threshold from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default no-touch behavior remain unchanged.

No scheduler cadence change, distress economy retune, debt/labor/grain/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime debt-trigger extraction v757-v764 note

V757-V764 changes the first household mobility runtime rule's debt-pressure trigger threshold from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default no-touch behavior remain unchanged.

No scheduler cadence change, debt economy retune, distress/labor/grain/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime labor-trigger extraction v765-v772 note

V765-V772 changes the first household mobility runtime rule's labor-capacity trigger ceiling from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default no-touch behavior remain unchanged.

No scheduler cadence change, labor model retune, debt/distress/grain/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime grain-trigger extraction v773-v780 note

V773-V780 changes the first household mobility runtime rule's grain-store trigger floor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default no-touch behavior remain unchanged.

No scheduler cadence change, grain economy retune, labor/debt/distress/land/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime land-trigger extraction v781-v788 note

V781-V788 changes the first household mobility runtime rule's land-holding trigger floor from a naked literal to an owner-consumed rules-data getter. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default no-touch behavior remain unchanged.

No scheduler cadence change, land economy retune, grain/labor/debt/distress/livelihood trigger extraction, score formula retune beyond literal extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime livelihood-trigger extraction v789-v796 note

V789-V796 changes the first household mobility runtime rule's trigger livelihood pattern from naked enum matching to an owner-consumed rules-data list. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default candidate behavior remain unchanged.

No scheduler cadence change, livelihood engine retune, livelihood score weight extraction, score formula retune beyond literal extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime livelihood-score extraction v797-v804 note

V797-V804 changes the first household mobility runtime rule's livelihood score pattern from a naked enum switch to an owner-consumed rules-data weight list. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default selected-household ordering remain unchanged.

No scheduler cadence change, livelihood engine retune, trigger extraction, migration-risk score retune, pressure floor/divisor extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime pressure-score extraction v805-v812 note

V805-V812 changes the first household mobility runtime rule's distress/debt score pattern from implicit unit additions to owner-consumed rules-data weights. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default selected-household ordering remain unchanged.

No scheduler cadence change, pressure formula retune, livelihood engine retune, trigger extraction, migration-risk score retune, livelihood score retune, pressure floor/divisor extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime migration-status extraction v813-v820 note

V813-V820 changes the first household mobility runtime rule's migration status threshold from a hardcoded `80` helper value to an owner-consumed rules-data threshold. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default migrating-status behavior remain unchanged.

No scheduler cadence change, migration status retune, migration-started event threshold retune, candidate ceiling retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime migration-risk clamp extraction v821-v828 note

V821-V828 changes the first household mobility runtime rule's post-nudge migration-risk clamp from hardcoded `0..100` bounds to owner-consumed rules-data bounds. The monthly cadence, active-pool selection, candidate floor/ceiling, deterministic ordering, fanout caps, event type, and default risk-band behavior remain unchanged.

No scheduler cadence change, migration-risk retune, risk-delta retune, migration status retune, candidate filter retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime tie-break priority extraction v829-v836 note

V829-V836 changes the first household mobility runtime rule's active-pool and household tie-break priorities from inline ordering to owner-consumed rules-data priorities. The monthly cadence, active-pool selection, candidate scoring, fanout caps, event type, and default settlement-id/household-id ordering remain unchanged.

No scheduler cadence change, ordering retune, score formula retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime unmatched livelihood score extraction v837-v844 note

V837-V844 changes the first household mobility runtime rule's unmatched-livelihood score fallback from inline `0` behavior to owner-consumed rules-data. The monthly cadence, active-pool selection, matched livelihood weights, candidate filters, fanout caps, event type, and default unmatched fallback remain unchanged.

No scheduler cadence change, livelihood weight retune, score formula retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime pressure contribution floor extraction v845-v852 note

V845-V852 changes the first household mobility runtime rule's labor/grain/land pressure contribution floor from inline `0` behavior to owner-consumed rules-data. The monthly cadence, active-pool selection, pressure floors/divisors, matched livelihood weights, candidate filters, fanout caps, event type, and default non-negative contribution behavior remain unchanged.

No scheduler cadence change, pressure floor retune, divisor retune, score formula retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, or rules-data file is added.

## Current household mobility runtime extraction closeout v853-v860 note

V853-V860 changes no runtime simulation code. It closes the first household mobility runtime rule hardcoded extraction track and records that remaining inline controls are owner-method control flow, not authored rules-data knobs.

No scheduler cadence change, threshold retune, weight retune, floor/divisor retune, score formula retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, rules-data file, file split, or second household mobility runtime rule is added.

## Current population households runtime rule file split v861-v868 note

V861-V868 changes file layout only. The first household mobility runtime rule remains on the monthly owner cadence and uses the same candidate filters, score computation, deterministic tie-breaks, fanout caps, risk delta, clamps, and event threshold.

No scheduler cadence change, threshold retune, weight retune, floor/divisor retune, score formula retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, rules-data file, or second household mobility runtime rule is added.

## Current population households membership focus file split v869-v876 note

V869-V876 changes file layout only. Membership synchronization and hot-household focus promotion remain on existing owner call sites and use the same membership ordering, household grouping, focused-member promotion cap, and `PersonRegistry` command seam.

No scheduler cadence change, membership behavior change, fidelity-ring behavior change, promotion-cap retune, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, rules-data file, or second household mobility runtime rule is added.

## Current population households pool rebuild file split v877-v884 note

V877-V884 changes file layout only. Settlement summary and labor/marriage/migration pool rebuilds remain on existing owner call sites and use the same settlement grouping, household ordering, membership ordering, pool formulas, and `PersonRegistry` query seam.

No scheduler cadence change, pool behavior change, labor/marriage/migration formula retune, pool limit extraction, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, rules-data file, or second household mobility runtime rule is added.

## Current population households query surface file split v885-v892 note

V885-V892 changes file layout only. Query registration remains on the existing owner seam, and query snapshots still copy households, settlements, memberships, and pools with the same stable ordering.

No scheduler cadence change, query behavior change, snapshot field expansion, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, rules-data file, or second household mobility runtime rule is added.

## Current population households pressure profile file split v893-v900 note

V893-V900 changes file layout only. Grain subsistence, tax-season, and official-supply pressure profile helpers move into a partial file while event dispatch, emitted receipts, and call-site ordering remain unchanged.

## Current population households event dispatch file split v901-v908 note

V901-V908 changes file layout only. Trade-shock, grain-price, world-pulse, tax-season, family-branch, and official-supply event-dispatch/application helpers move into a partial file while `HandleEvents`, scheduler cadence, emitted receipts, metadata, and deterministic household ordering remain unchanged.

No event pool, movement command, route-history path, event-routing ledger, plugin loading, or schema state is introduced.

## Current population households livelihood drift file split v909-v916 note

V909-V916 changes file layout only. Monthly livelihood drift helpers move into a partial file while `RunMonth`, scheduler cadence, household traversal order, drift thresholds, baseline mapping, emitted receipts, and metadata remain unchanged.

No event pool, movement command, route-history path, livelihood-drift ledger, class/status engine, plugin loading, or schema state is introduced.

## Current population households monthly pulse file split v917-v924 note

V917-V924 changes file layout only. Xun/month pulse helpers move into a partial file while `RunXun`, `RunMonth`, scheduler cadence, household traversal order, delta thresholds, migration status fallback, emitted receipts, and metadata remain unchanged.

No event pool, movement command, route-history path, monthly-pulse ledger, migration-status ledger, class/status engine, plugin loading, or schema state is introduced.

## Current population households warfare aftermath file split v925-v932 note

V925-V932 changes file layout only. Warfare aftermath handling moves into a partial file while `HandleEvents`, scheduler cadence, event handling order, settlement-scoped household traversal, campaign delta formulas, emitted receipts, and metadata remain unchanged.

No event pool, movement command, route-history path, warfare-aftermath ledger, campaign-aftermath ledger, class/status engine, plugin loading, or schema state is introduced.

## Current population households health lifecycle file split v933-v940 note

V933-V940 changes file layout only. Health lifecycle handling moves into a partial file while `RunMonth`, scheduler cadence, monthly call order, person-id membership traversal, health thresholds, illness-month caps, deceased command seam, emitted receipts, and metadata remain unchanged.

No event pool, movement command, route-history path, health-lifecycle ledger, illness ledger, mortality ledger, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households monthly pressure rules-data extraction v941-v948 note

V941-V948 changes monthly pressure parameter ownership only. `RunMonth` now reads prosperity/security/clan-relief/drift thresholds from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, household traversal, owner-state mutation, emitted events, and default outcomes remain unchanged.

No event pool, movement command, route-history path, monthly-pressure ledger, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households grain price signal rules-data extraction v949-v956 note

V949-V956 changes grain-price signal parameter ownership only. The grain-price event path now reads metadata fallback and clamp values from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, grain-shock ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households grain price pressure clamp extraction v957-v964 note

V957-V964 changes grain-price price-pressure clamp ownership only. The grain-price event path now reads price-pressure clamp bounds from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, grain-price-pressure ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households grain price level band extraction v965-v972 note

V965-V972 changes grain-price level band ownership only. The grain-price event path now reads price-level threshold/score bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, grain-price-level ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households grain price jump band extraction v973-v980 note

V973-V980 changes grain-price jump band ownership only. The grain-price event path now reads price-jump threshold/score bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, grain-price-jump ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households grain price market tightness band extraction v981-v988 note

V981-V988 changes market tightness band ownership only. The grain-price event path now reads market-tightness threshold/score bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, market-tightness ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence market dependency extraction v989-v996 note

V989-V996 changes market dependency score ownership only. The grain-price subsistence path now reads livelihood-score weights from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, market-dependency ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

No scheduler cadence change, pressure formula change, metadata fallback change, fanout widening, new event type, event routing change, save schema, route-history model, movement command, loader, rules-data file, or second household mobility runtime rule is added.
## Current population households subsistence labor capacity extraction v997-v1004 note

V997-V1004 changes labor-capacity score ownership only. The grain-price subsistence path now reads labor-capacity pressure bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, dependent-count pressure, final labor clamp, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, subsistence-labor ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence dependent count extraction v1005-v1012 note

V1005-V1012 changes dependent-count score ownership only. The grain-price subsistence path now reads dependent-count pressure bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, labor-capacity bands, final labor clamp, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, dependent-count ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence labor clamp extraction v1013-v1020 note

V1013-V1020 changes final clamp ownership only. The grain-price subsistence path now reads subsistence labor pressure clamp floor/ceiling values from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, labor-capacity bands, dependent-count bands, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, subsistence-labor ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence grain buffer extraction v1021-v1028 note

V1021-V1028 changes grain-buffer score ownership only. The grain-price subsistence path now reads grain-store threshold bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, price/market/labor/fragility/interaction scoring, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, grain-buffer ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence fragility distress extraction v1029-v1036 note

V1029-V1036 changes fragility distress score ownership only. The grain-price subsistence path now reads distress threshold bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, debt/migration/fragility-clamp scoring, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, fragility-distress ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence fragility debt extraction v1037-v1044 note

V1037-V1044 changes fragility debt score ownership only. The grain-price subsistence path now reads debt threshold bands from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, distress/migration/fragility-clamp scoring, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, fragility-debt ledger, pressure-profile state, class/status engine, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence fragility migration extraction v1045-v1052 note

V1045-V1052 changes fragility migration contribution ownership only. The grain-price subsistence path now reads the migration risk threshold, pressure score, and fallback score from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, distress/debt/fragility-clamp scoring, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, fragility-migration ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence fragility clamp extraction v1053-v1060 note

V1053-V1060 changes fragility final-clamp ownership only. The grain-price subsistence path now reads clamp floor and ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, distress/debt/migration scoring, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, fragility-clamp ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence interaction grain shortage extraction v1061-v1068 note

V1061-V1068 changes interaction grain-shortage window ownership only. The grain-price subsistence path now reads grain-shortage floor and ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, cash-need boost, debt threshold boost, resilience relief, interaction clamp, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, interaction-grain-shortage ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence interaction cash-need extraction v1069-v1076 note

V1069-V1076 changes interaction cash-need boost ownership only. The grain-price subsistence path now reads cash-need boost score from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, grain-shortage window, debt threshold boost, resilience relief, interaction clamp, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, interaction-cash-need ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence interaction debt-boost extraction v1077-v1084 note

V1077-V1084 changes interaction debt boost ownership only. The grain-price subsistence path now reads debt pressure threshold and boost score from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, grain-shortage window, cash-need boost, resilience relief, interaction clamp, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, interaction-debt ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence interaction resilience relief extraction v1085-v1092 note

V1085-V1092 changes interaction resilience relief ownership only. The grain-price subsistence path now reads resilience relief thresholds and score from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, grain-shortage window, cash-need boost, debt boost, interaction clamp, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, interaction-resilience ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence interaction clamp extraction v1093-v1100 note

V1093-V1100 changes interaction clamp ownership only. The grain-price subsistence path now reads final interaction pressure clamp floor/ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, grain-shortage window, cash-need boost, debt boost, resilience relief, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, interaction-clamp ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence event threshold extraction v1101-v1108 note

V1101-V1108 changes the grain-price subsistence event threshold ownership only. The grain-price subsistence path now reads the distress crossing threshold from a validated `PopulationHouseholdMobilityRulesData` getter, while scheduler cadence, event scope, household traversal, subsistence pressure calculation, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, event-threshold ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households subsistence distress delta clamp extraction v1109-v1116 note

V1109-V1116 changes the grain-price subsistence distress delta clamp ownership only. The grain-price subsistence path now reads distress delta clamp floor/ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, household traversal, event threshold, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, delta-clamp ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households tax season debt delta clamp extraction v1117-v1124 note

V1117-V1124 changes the tax-season debt delta clamp ownership only. The tax-season path now reads debt delta clamp floor/ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, deterministic household traversal, debt spike threshold, tax component scoring, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, tax-debt-clamp ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households tax season debt spike threshold extraction v1125-v1132 note

V1125-V1132 changes the tax-season debt spike event threshold ownership only. The tax-season path now reads the debt crossing threshold from a validated `PopulationHouseholdMobilityRulesData` getter, while scheduler cadence, event scope, deterministic household traversal, tax debt delta clamp, tax component scoring, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, debt-threshold ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households official supply distress delta clamp extraction v1133-v1140 note

V1133-V1140 changes the official-supply distress delta clamp ownership only. The official-supply path now reads distress delta clamp floor/ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, deterministic household traversal, debt/labor/migration deltas, event threshold, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, official-supply-delta ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households official supply debt delta clamp extraction v1141-v1148 note

V1141-V1148 changes the official-supply debt delta clamp ownership only. The official-supply path now reads debt delta clamp floor/ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, deterministic household traversal, distress/labor/migration deltas, event threshold, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, official-supply-debt-delta ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households official supply labor drop clamp extraction v1149-v1156 note

V1149-V1156 changes the official-supply labor drop clamp ownership only. The official-supply path now reads labor drop clamp floor/ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, deterministic household traversal, distress/debt/migration deltas, event threshold, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, official-supply-labor-drop ledger, pressure-profile state, labor-market engine, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.

## Current population households official supply migration delta clamp extraction v1157-v1164 note

V1157-V1164 changes the official-supply migration delta clamp ownership only. The official-supply path now reads migration delta clamp floor/ceiling from validated `PopulationHouseholdMobilityRulesData` getters, while scheduler cadence, event scope, deterministic household traversal, distress/debt/labor deltas, event threshold, owner-state mutation, emitted metadata, and default outcomes remain unchanged.

No event pool, movement command, route-history path, official-supply-migration-delta ledger, pressure-profile state, class/status engine, migration economy, plugin loading, `PersonRegistry` expansion, or schema state is introduced.
### Current population households official supply burden event threshold extraction v1165-v1172 note

V1165-V1172 extracts the official-supply burden event distress threshold into deterministic owner-consumed rules-data. Default behavior remains unchanged: `HouseholdBurdenIncreased` still emits at the prior distress crossing threshold of `80`.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.

### Current population households subsistence cash-need extraction v1325-v1332 note

V1325-V1332 extracts the subsistence interaction cash-need livelihood list into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured grain-price shock handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.

### Current population households official supply liquidity cash-need extraction v1317-v1324 note

V1317-V1324 extracts the official-supply liquidity cash-need livelihood list into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.

### Current population households official supply liquidity pressure extraction v1213-v1220 note

V1213-V1220 extracts official-supply liquidity grain strain bands, cash-need score, tool drag threshold, debt drag bands, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, shared cash-need predicate retuning, or UI/Application authority.

### Current population households official supply fragility pressure extraction v1221-v1228 note

V1221-V1228 extracts official-supply fragility distress bands, debt bands, migration-risk threshold, shelter-drag threshold, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, official-supply interaction extraction, formula-divisor retuning, or UI/Application authority.

### Current population households official supply interaction pressure extraction v1229-v1236 note

V1229-V1236 extracts official-supply interaction livelihood conditions, thresholds, boost scores, resilience relief score, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, formula-divisor retuning, or UI/Application authority.

### Current population households official supply distress delta formula extraction v1237-v1244 note

V1237-V1244 extracts official-supply distress delta formula divisors and component weights into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, debt/labor/migration delta retuning, or UI/Application authority.

### Current population households official supply debt delta formula extraction v1245-v1252 note

V1245-V1252 extracts official-supply debt delta formula divisors, interaction floor, and component weights into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, distress/labor/migration delta retuning, or UI/Application authority.

### Current population households official supply labor drop formula extraction v1253-v1260 note

V1253-V1260 extracts official-supply labor drop formula divisors, labor-pressure floor, and component weight into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, distress/debt/migration delta retuning, or UI/Application authority.

### Current population households official supply migration delta formula extraction v1261-v1268 note

V1261-V1268 extracts official-supply migration delta formula divisors and fragility threshold/boost into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, distress/debt/labor delta retuning, or UI/Application authority.

### Current population households tax season registration visibility extraction v1269-v1276 note

V1269-V1276 extracts tax-season registration visibility livelihood exposure scores, land visibility bands, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured tax-season handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, tax liquidity/labor/fragility/interaction/debt-delta retuning, or UI/Application authority.

### Current population households tax season liquidity pressure extraction v1277-v1284 note

V1277-V1284 extracts tax-season liquidity grain pressure bands, cash-need livelihood scores, tool-drag threshold/score, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured tax-season handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, tax registration-visibility/labor/fragility/interaction/debt-delta retuning, or UI/Application authority.

### Current population households tax season labor pressure extraction v1285-v1292 note

V1285-V1292 extracts tax-season labor capacity bands, dependent count bands, dependent/labor ratio bonus, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured tax-season handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, tax registration-visibility/liquidity/fragility/interaction/debt-delta retuning, or UI/Application authority.

### Current population households tax season fragility pressure extraction v1293-v1300 note

V1293-V1300 extracts tax-season fragility distress bands, debt bands, shelter-drag threshold/score, migration-risk threshold/score, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured tax-season handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, tax registration-visibility/liquidity/labor/interaction/debt-delta retuning, or UI/Application authority.

### Current population households tax season interaction pressure extraction v1301-v1308 note

V1301-V1308 extracts tax-season interaction tenant pressure, land/labor pressure, cash-need livelihood pressure, resilience relief, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured tax-season handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, tax registration-visibility/liquidity/labor/fragility/debt-delta retuning, or UI/Application authority.

### Current population households tax season debt delta formula extraction v1309-v1316 note

V1309-V1316 extracts tax-season debt-delta base score and pressure weights into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured tax-season handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, tax registration-visibility/liquidity/labor/fragility/interaction/clamp retuning, or UI/Application authority.

### Current population households official supply labor pressure extraction v1205-v1212 note

V1205-V1212 extracts official-supply labor capacity bands, dependent count bands, dependent/labor ratio bonus, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.

### Current population households official supply signal normalization clamp extraction v1181-v1188 note

V1181-V1188 extracts official-supply signal normalization clamps into deterministic owner-consumed rules-data. Default behavior remains unchanged for out-of-range structured Office metadata.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.

### Current population households official supply signal fallback clamp extraction v1173-v1180 note

V1173-V1180 extracts official-supply signal metadata fallback values and the derived supply-pressure fallback clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for missing/partial structured Office metadata.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.

### Current population households official supply livelihood exposure extraction v1189-v1196 note

V1189-V1196 extracts official-supply livelihood exposure scores, land visibility bands, fallback scores, and exposure clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.

### Current population households official supply resource buffer extraction v1197-v1204 note

V1197-V1204 extracts official-supply resource buffer grain/tool/shelter bands, fallback scores, and clamp into deterministic owner-consumed rules-data. Default behavior remains unchanged for structured Office supply requisition handling.

This does not add household movement, route history, migration economy, class/status simulation, new cadence, new persisted state, prose parsing, or UI/Application authority.
