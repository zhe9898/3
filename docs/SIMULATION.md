# SIMULATION

This document defines the authoritative simulation cadence and monthly scheduler.

For the higher-level design rule that the world moves first, projection comes after diffs, and the player only acts late in the monthly cycle, see `RULES_DRIVEN_LIVING_WORLD.md`.
For the rule that precision is allocated by focus ring rather than by a flat status ladder, see `SIMULATION_FIDELITY_MODEL.md`.
For the per-module time contract, see `MODULE_CADENCE_MATRIX.md`.

## Core cadence rule

Zongzu should not use a flat one-jump monthly heartbeat for all forms of life.

Use this cadence instead:

- `month` is the outer review shell
- `xun` (`early / mid / late month`) is the inner living pulse

In plain terms:
- the player still receives a monthly review and major command window
- the world below that review breathes in three smaller beats
- low-level life does not have to wait a whole month to strain, worsen, travel, or surface

This keeps the design aligned with:
- living household pressure
- delayed but readable causality
- bounded player intervention
- ordinary people, debt, illness, labor, and rumor feeling alive instead of being crushed into one large monthly jump

## Time layers

### Xun pulse

Use `shangxun / zhongxun / xiaxun` as the inner pulse of lived time.

This is where the game should resolve:
- food consumption and immediate livelihood strain
- short labor allocation and labor shortage
- petty trade movement and local market heat
- route interruption, ferry irregularity, and messenger delay
- rumor spread and local notice drift
- illness worsening or short-term recovery
- small debt pressure, rent pressure, and support strain
- local disorder flare, escort strain, and hotspot activation

The xun pulse is not a full player turn.
It is the world's internal breathing rhythm.

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

These longer bands may still leak short-term pressure into xun and monthly play.

## Monthly shell with three xun pulses

### Phase 0: prepare month
- current date known
- `PersonRegistry` runs age progression and life-stage checks (so all modules in Phase 1 see current life stage)
- staged player commands from the previous review validated
- enabled feature manifest loaded
- projection caches invalidated as needed

### Phase 1: run three xun pulses

Each month contains:
1. `上旬`
2. `中旬`
3. `下旬`

Each xun pulse should run the same deterministic sub-order.

#### Xun Phase A: local world conditions
`WorldSettlements`
- update short-band environment, route friction, safety, and local availability
- refresh current settlement heat and access constraints

#### Xun Phase B: household and livelihood pulse
`PopulationAndHouseholds`
- apply subsistence use, labor strain, wage or rent pressure, migration drift, and worker instability
- resolve whether pressure remains ambient, becomes household-visible, or escalates

#### Xun Phase C: family and support pulse
`FamilyCore`
- resolve near-term support burden, household dependency strain, branch assistance pressure, and urgent kin disruption
- urgent clan-scoped births and household crises may surface here instead of waiting for month-end
- FamilyCore may respond to death events emitted by other modules (e.g. `DeathByIllness` from PopulationAndHouseholds, `DeathByViolence` from ConflictAndForce) by triggering inheritance and mourning logic; it does not own the death cause itself (see `PERSON_OWNERSHIP_RULES.md`)

#### Xun Phase D: local social drift
`SocialMemoryAndRelations`
- update short-band obligation, fear, shame, rumor attachment, and feud temperature
- promote or cool memories when pressure was actually carried during the pulse

#### Xun Phase E: enabled local and upward modules
Enabled modules run in deterministic order for xun-visible local pressure:
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

Do not force every slow structure to fully resolve inside a single xun pulse.
The pulse should handle short-band pressure, not replace the monthly shell.

#### Xun Phase F: pulse receipts and hotspot staging
- capture short-band receipts
- mark hotspot activation
- stage urgent items for month-end review
- allow only bounded interrupt-worthy items to request immediate visibility

### Phase 2: month-end domain event handling
- modules emit monthly event snapshots after the third xun closes
- deterministic event queue snapshot processed before projection
- handlers update only owning module state
- the active handler seam runs after authority modules finish their monthly pass and before `NarrativeProjection` builds notices
- follow-on events may reach projection, but they do not trigger uncontrolled recursive month expansion

Current M3 local-conflict note:
- `ConflictAndForce.Lite` may refresh force posture before `OrderAndBanditry.Lite` reads same-month response support
- only activated local-conflict response state may feed same-month order relief; calm or standing-but-untriggered posture stays visible but does not leak relief
- `ConflictAndForce.Lite` still reads only published query state and does not mutate `OrderAndBanditry` directly
- `OfficeAndCareer.Lite`, when enabled through the governance-lite path, now runs ahead of conflict/order so jurisdiction leverage can be read as bounded same-month administrative support without direct writes
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
- xun-local fluctuation
- month-level consolidation
- long-band structural change

### Phase 4: month-end projection and narrative
`NarrativeProjection`
- urgent / consequential / background grouping
- letters, reports, rumors, council prompts
- explanation trails
- projection may summarize pulse accumulation rather than dumping all three xun as separate spam

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

Optional exception:
- extremely urgent red-band items may open a narrow interrupt-style response window
- these should stay rare and should not turn the game into a daily alert treadmill

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
- what was a pulse fluctuation versus a true monthly shift?
- what can the player still do?

## Projection requirements

Narrative projection should not expose all three xun as equally noisy output.

Projection should usually:
- compress repeated pulse strain into one readable month-end summary
- surface only the xun events that crossed an urgency or irreversibility threshold
- keep background xun motion visible through trend wording, route heat, notices, and hotspot surfaces

## Scheduler extension rule

A new module must declare:
- whether it runs at xun cadence, monthly cadence, or both
- which phase it participates in
- what events it emits
- what events it handles
- what projections it publishes

No module may insert ad hoc hidden execution outside the scheduler.
