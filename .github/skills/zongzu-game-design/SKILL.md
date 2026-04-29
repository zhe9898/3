---
name: zongzu-game-design
description: Use when working on Zongzu's game design, especially rules-driven living-world structure, Northern Song/Renzong pressure chains, monthly and xun cadence, bounded player leverage, actor autonomy, command resolution, explainable causality, fidelity/scale budget, vertical slices, MVP shaping, feature-pack scope, module-boundary fit, or when a proposal risks becoming an event pool, detached subsystem, rigid route tree, locked timeline, spreadsheet, tactics game, or god-game instead of an integrated living society.
---

# Zongzu Game Design

## Overview

Use this skill to keep Zongzu design anchored to the repo's actual product:
- rules-driven living world
- world moves before the player
- notifications are projections of state changes
- player acts through bounded leverage
- adults and institutions can resist, delay, reinterpret, or exploit commands
- clan memory, favors, shame, debt, fear, and grudges persist across generations
- shell presentation is spatialized
- conflict and warfare extend the lineage simulation rather than becoming a detached tactics game

Use it to turn broad design prompts into a connected pass across:
- monthly loop and three-xun cadence
- module-owned pressure
- command and autonomy resolution
- DomainEvent receipts and structured metadata
- projection, notice, great hall, desk sandbox, and conflict/campaign surfaces
- historical momentum and Renzong thin-chain topology
- roadmap phase, feature pack, schema/save, and acceptance-test ownership

## Current Repo Anchors

Use current code and `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` to distinguish implemented topology from design target:
- the topology index records live thin-chain claims; the fuller pressure spec is not proof that code is wired
- a chain is only "current" when real `PublishedEvents`, `ConsumedEvents`, scheduler drain behavior, metadata, off-scope tests, and projection/receipt behavior line up
- long-run pressure saturation can be intended stress, missing recovery, missing allocation, or diagnostic debt; do not flatten it into a simple tuning problem
- Unity shell presence does not change product authority: shell surfaces show read models and bounded command affordances, never direct court/world control
- the current mainline is committed through v476: v3-v108 build and audit the thin skeleton; v109-v204 closes the first court-policy rule-density branch without making a full court engine; v213-v292 adds and closes the first social mobility / fidelity-ring substrate; v253-v268 closes Chain 9 regime-legitimacy first readback; v293-v380 closes personnel-flow readiness/gate/future-lane preflight without adding direct movement lanes; v381-v452 closes commoner/social-position readback, source-key visibility, scale-budget readback, regional guard, commoner status owner-lane preflight, and fidelity scale-budget preflight; v453-v460 adds household mobility dynamics explanation over existing pressure signals; v461-v468 closes that explanation as first-layer governance evidence; v469-v476 adds household mobility owner-lane preflight
- current v19-v476 follow-up, owner-lane, court-policy, mobility, regime, personnel-flow, social-position, and household-mobility readbacks/preflights are playable guidance, not new household omnipotence or player-as-god control: they advise repeat, switch, cool down, return to the proper owner lane, explain docket/receipt anti-misread, explain fidelity/detail scale, show readiness/gate/preflight limits, show structured social-position sources, explain existing household mobility dimensions, require owner/cadence/scope/fanout/schema gates for future household mobility depth, or stop re-pressuring local surfaces from existing structured aftermath
- treat public-life/order/court-policy/regime/social-mobility/personnel-flow/social-position/household-mobility v18-v476 as rule-driven command / aftermath / SocialMemory / home-household / owner-lane / docket / receipt / mobility-scale / future-lane preflight / status-readback / household-dimension explanation / household owner-lane preflight guidance, not an event-chain, event pool, full court engine, full regime engine, full society engine, universal class ladder, direct personnel-control design, or new mobility algorithm
- v32-v34 are evidence-quality passes for the backend event-contract graph, not new playable loops; they classify debt, gate unclassified debt, and show owner/evidence backlinks without adding runtime authority
- v35-v100 are thin playable-world handoffs/readbacks, not thick new systems: canal-window pressure returns to Trade/Order, household burden and relief stay in Family, Office/yamen后手 remains in Office, force/campaign后账 stays in force/campaign lanes, Warfare directives and aftermath stay in `WarfareCampaign`, and court-policy process remains Office/PublicLife readback through structured metadata, query snapshots, projection fields, no-touch tests, and no schema/ledger/UI authority expansion
- v101-v108 is an audit boundary: "thin-chain complete" means topology/readback/ownership evidence; v109-v204 is the first court-policy rule-density branch; v213-v292 is the first mobility/personnel-flow substrate; v253-v268 is first-layer regime readback; v293-v380 is only personnel-flow readiness/gate/future-lane preflight and closeout evidence; v381-v452 is social-position and scale-budget readback/preflight evidence; v453-v460 is household mobility dynamics explanation over existing signals; v461-v468 is closeout governance, not runtime depth; v469-v476 is owner-lane preflight, not movement implementation. Taxes, famine, disaster relief, office factions, thick court process, campaign logistics, full regime recognition, canal politics, deeper durable residue, migration economy, direct personnel commands, Family/Office/Warfare personnel-flow owner lanes, commoner status formulas, social-class mobility engines, household movement rules, route history, and recovery/decay remain future work
- not every future contract should become immediate gameplay; first classify the debt, then graduate one owner lane only when the player-facing or simulation readback can stay bounded and testable
- scale budget is part of design: use `SIMULATION_FIDELITY_MODEL.md` focus rings so full fidelity stays near the player or active pressure, while distant society remains alive through summarized pressure until a planned owner-lane rule promotes more detail
- gameplay depth must pair simulation math with player-facing levers, uncertainty, tradeoff, counterpressure, and aftermath readability; a technically plausible algorithm is not enough if it becomes invisible spreadsheet churn
- future mobility, status, personnel, regime, and court depth should preserve "near detail, far summary": active locality gets richer actors and command texture, distant regions get aggregated pressure, sampled exemplars, and bounded readback
- a mechanic is not ready to implement until it names owner, cadence, state, projection, bounded command, resistance/refusal, test proof, and rough cardinality/fanout risk

## External Calibration Anchors

Use outside engineering and accessibility material as design calibration, not as a replacement for Zongzu's product direction:
- .NET testing guidance reinforces small, behavior-focused design proof: a mechanic should have one clear claim per test lane, not a giant acceptance snapshot as its first proof.
- .NET diagnostics and performance guidance reinforce naming the hot path and expected cardinality before widening simulation fidelity, adding global scans, or proposing caches.
- Unity Profiler, object pooling, UI optimization, and assembly-boundary guidance apply to shell implementation and iteration cost; they do not justify moving command resolution, scheduler cadence, or state ownership into Unity.
- WCAG/Xbox accessibility guidance reinforces that projected consequences must remain readable through contrast, focus order, labels, narration, and non-color cues, even when the surface is spatialized.
- Historical/source calibration should sharpen pressure carriers and confidence bands; it should not force dense named-person simulation everywhere or turn source facts into fixed event triggers.

## Use This Skill When

- designing or reviewing the main loop, monthly loop, xun cadence, or subsystem loop
- deciding whether a feature is rules-driven enough
- shaping pressure chains, broad-to-local allocation, or Renzong thin-chain work
- shaping player commands, influence circles, leverage, refusal, delay, and backlash
- planning a vertical slice, MVP cut, post-MVP pack, or roadmap phase
- checking whether a design fits module ownership, Query / Command / DomainEvent contracts, and save/version rules
- pairing design work with `zongzu-architecture-boundaries` when the task is mainly about code placement, scheduler/event contracts, command seams, save compatibility, or coupling review
- deciding how a mechanic becomes visible in the great hall, lineage surface, desk sandbox, notice tray, conflict vignette, campaign board, or debug surface
- comparing outside games or sources without importing their genre assumptions
- checking whether a proposal drifts into event-pool design, detached tactics design, rigid route trees, spreadsheet-with-flavor design, or player-as-god design

## Fast Lane

For short design checks, answer only the core loop, owner, player leverage, projection surface, and obvious drift risk. Use a full pass when the topic changes cadence, fidelity rings, pressure propagation, command resolution, long-run balance, feature-pack scope, or future roadmap shape.

## Workflow

1. Read project truth first.

   Start with the repo docs that match the task. For broad or ambiguous prompts, read at least:
   - `docs/PRODUCT_SCOPE.md`
   - `docs/MVP_SCOPE.md`
   - `docs/POST_MVP_SCOPE.md`
   - `docs/SIMULATION.md`
   - `docs/PLAYER_SCOPE.md`
   - `docs/RULES_DRIVEN_LIVING_WORLD.md`
   - `docs/LIVING_WORLD_DESIGN.md`
   - `docs/MODULE_BOUNDARIES.md`
   - `docs/MODULE_INTEGRATION_RULES.md`
   - `docs/ACCEPTANCE_TESTS.md`

   When the prompt touches later scale, also read:
   - `docs/GAME_DEVELOPMENT_ROADMAP.md`
   - `docs/HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
   - `docs/RENZONG_PRESSURE_CHAIN_SPEC.md`
   - `docs/RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`
   - `docs/SPATIAL_SKELETON_SPEC.md`
   - `docs/MODERN_GAME_ENGINEERING_STANDARDS.md`
   - `docs/EXTENSIBILITY_MODEL.md`

2. Check current implementation facts before proposing a design as "done".

   Inspect code and tests when the design claims implementation fit:
   - module runners, `ModuleSchemaVersion`, `PublishedEvents`, `ConsumedEvents`, and accepted commands
   - `MonthlyScheduler` cadence and bounded event drain
   - `SimulationBootstrapper` feature-pack module sets
   - module-owned `HandleCommand(...)` implementations and their command resolver/helpers
   - application orchestration code only where it still routes, seeds, or assembles rather than owning rules
   - presentation read-model builders, adapters, and Unity-facing ViewModels
   - relevant tests under `tests/`

3. Identify the design layer.

   Determine whether the task is mainly about:
   - loop structure
   - pressure chain or pressure locus
   - broad-to-local allocation and off-scope boundary
   - player leverage and influence footprint
   - command resolution or adult autonomy
   - actor ladder and institutional incentives
   - feature pack, topology, or roadmap phase
   - shell projection and player comprehension
   - balancing, diagnostics, scale budget, or long-run failure modes
   - schema/save/module contract impact

   Pair with `zongzu-pressure-chain` for cross-module chain implementation, `zongzu-simulation-validation` for replay/health proof, `zongzu-content-authoring` when copy or authored content changes, and `zongzu-unity-shell` when the work touches Unity-facing files.

4. Load only the references you need.

   - Read [references/rules-driven-living-world.md](references/rules-driven-living-world.md) for the design thesis and anti-patterns.
   - Read [references/pressure-chains-and-causality.md](references/pressure-chains-and-causality.md) for how pressure starts, travels, and becomes visible.
   - Read [references/command-resolution-and-bounded-leverage.md](references/command-resolution-and-bounded-leverage.md) for player influence, command windows, and anti-god rules.
   - Read [references/vertical-slice-and-mvp-shaping.md](references/vertical-slice-and-mvp-shaping.md) for slice design, cut discipline, and MVP shaping.
   - Read [references/similar-game-system-calibration.md](references/similar-game-system-calibration.md) only when outside-game comparison should calibrate system structure without importing genre assumptions.

5. Convert design talk into playable structure.

   A useful result answers:
   - what rule owns the change
   - what state and module own the pressure
   - what cadence resolves it
   - what structured event or diff records the result
   - whether the pressure is an edge, ongoing demand, receipt, summary, or projection-only trace
   - what the player can and cannot reach
   - who can resist, delay, reinterpret, or exploit the player's intent
   - what the shell shows first and what stays inspectable in the background
   - what tests or diagnostics prove the chain
   - what docs, schemas, boundaries, or acceptance criteria must change

## Short Prompt Expansion

Treat short prompts as whole-system prompts unless the user explicitly asks for a narrow definition.

For prompts like `rules driven`, `living world`, `main loop`, `MVP`, `vertical slice`, `Renzong chain`, `bounded command`, `office pressure`, `public life`, `warfare`, or `route`, default to:
- identify the underlying loop
- identify where pressure starts and which module owns it
- identify pressure locus, propagation radius, naming threshold, and no-touch boundary
- identify who carries the pressure and who can distort it
- identify player leverage and limits
- identify shell projection and receipt
- identify same-month vs delayed consequence
- identify test and doc impact
- identify fidelity ring, fanout/cardinality, and performance validation need when the feature scales across settlements, households, actors, routes, or notices
- identify whether the proposal needs a scheduler cap, one-pass projection index, recurring-demand model, no-touch test, or explicit deferral before it can scale

## Output Rules

- Do not design core play around random event pools.
- Do not describe Zongzu as an event-driven game. It is rules-driven; DomainEvents record and propagate facts after rules resolve.
- Do not let flavor text become the authoritative driver.
- Do not give the player omnipotent direct control.
- Do not design commands as guaranteed outcomes.
- Do not treat every subsystem as its own game mode.
- Do not clone another game's genre frame just because it solves a neighboring problem.
- Do not detach warfare into a separate tactics game.
- Do not lock history into a scripted timeline the player can only watch.
- Do not call a pressure chain complete unless scheduler-level behavior, structured metadata, off-scope entities, and projection receipts are covered or explicitly marked deferred.
- Do not parse `DomainEvent.Summary` as rule input.
- Do not let local pressure fan out globally by accident.
- Do not let broad pressure become local consequence without an allocation rule.
- Do not add balance knobs without owner, calibration source, expected band, and diagnostics/test.
- Do not treat "simulate everyone in detail" as a design virtue; promote fidelity where pressure, player reach, or readability justifies it.
- Do not add a system whose complexity grows all-to-all unless the player-facing reason and validation budget are explicit.
- Do not make mods or content packs a second event pool.
- Prefer pressure chains over isolated features.
- Prefer edge events for changed conditions and recurring demand models for ongoing burdens.
- Prefer readable cause traces over hidden simulation magic.
- Prefer vertical slices that prove both causality and player response.

## Zongzu-Specific Guidance

- The world advances before the player acts.
- Review and interpretation are part of play, not downtime between actions.
- The player wins through leverage, preparation, timing, judgment, and earned social reach.
- Family, commoner pressure, exams, trade, office, public life, order, conflict, court rhythm, and force should behave like one linked field.
- Full fidelity belongs near the player's current influence footprint; distant regions and later packs can begin as pressure summaries.
- The scale rule is: focus ring decides precision; prosperity and settlement scale decide pool thickness. A poor nearby household can deserve dense simulation; a rich distant city can remain mostly summarized until pressure or player reach promotes it.
- Commoner/social-position design should feel like legible social pressure, household registration, livelihood, service, office contact, debt, memory, and local visibility; it should not become a universal RPG class ladder, global people browser, or promote/demote button set.
- Renzong-era and historical-process work should become pressure carriers and windows of possibility, not untouchable cutscenes.
- A good command asks who executes it, who benefits, who loses face, who remembers, and where backlash can surface.
- Current public-life order commands should continue to ask that question through the existing lane: the owning module resolves the bounded action, SocialMemory remembers durable social residue when in scope, household responses stay household-bounded, Family/Office/Order owner-lane return guidance stays projection-only, and shell readback explains leverage/cost/status without granting god control.
- MVP should feel like sitting in the hall and hearing the world arrive, not sampling disconnected mechanics.
- Later regions, scenarios, wars, courts, and dynasty-cycle systems should extend the same cadence, pressure, projection, and bounded-command spine.
