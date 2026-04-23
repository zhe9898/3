---
name: zongzu-game-design
description: Use when working on Zongzu's game design, especially rules-driven living-world structure, Northern Song/Renzong pressure chains, monthly and xun cadence, bounded player leverage, actor autonomy, command resolution, explainable causality, vertical slices, MVP shaping, feature-pack scope, module-boundary fit, or when a proposal risks becoming an event pool, detached subsystem, rigid route tree, locked timeline, spreadsheet, tactics game, or god-game instead of an integrated living society.
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

## Use This Skill When

- designing or reviewing the main loop, monthly loop, xun cadence, or subsystem loop
- deciding whether a feature is rules-driven enough
- shaping pressure chains, broad-to-local allocation, or Renzong thin-chain work
- shaping player commands, influence circles, leverage, refusal, delay, and backlash
- planning a vertical slice, MVP cut, post-MVP pack, or roadmap phase
- checking whether a design fits module ownership, Query / Command / DomainEvent contracts, and save/version rules
- deciding how a mechanic becomes visible in the great hall, lineage surface, desk sandbox, notice tray, conflict vignette, campaign board, or debug surface
- comparing outside games or sources without importing their genre assumptions
- checking whether a proposal drifts into event-pool design, detached tactics design, rigid route trees, spreadsheet-with-flavor design, or player-as-god design

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
   - application command services
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

4. Load only the references you need.

   - Read [references/rules-driven-living-world.md](references/rules-driven-living-world.md) for the design thesis and anti-patterns.
   - Read [references/pressure-chains-and-causality.md](references/pressure-chains-and-causality.md) for how pressure starts, travels, and becomes visible.
   - Read [references/command-resolution-and-bounded-leverage.md](references/command-resolution-and-bounded-leverage.md) for player influence, command windows, and anti-god rules.
   - Read [references/vertical-slice-and-mvp-shaping.md](references/vertical-slice-and-mvp-shaping.md) for slice design, cut discipline, and MVP shaping.

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
- Renzong-era and historical-process work should become pressure carriers and windows of possibility, not untouchable cutscenes.
- A good command asks who executes it, who benefits, who loses face, who remembers, and where backlash can surface.
- MVP should feel like sitting in the hall and hearing the world arrive, not sampling disconnected mechanics.
- Later regions, scenarios, wars, courts, and dynasty-cycle systems should extend the same cadence, pressure, projection, and bounded-command spine.
