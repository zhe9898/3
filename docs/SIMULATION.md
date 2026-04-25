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
