---
name: zongzu-game-design
description: Use when working on Zongzu's game design, especially rules-driven living-world structure, monthly simulation loop, pressure chains, bounded player leverage, command resolution, explainable causality, vertical slices, MVP shaping, or when a proposal risks becoming an event pool, detached subsystem, or god-game instead of an integrated lineage simulation.
---

# Zongzu Game Design

## Overview

This skill keeps Zongzu game design anchored to the repo's actual product:
- rules-driven
- living-world
- world moves before the player
- player acts through bounded leverage
- text and flavor are downstream of state
- conflict and warfare stay integrated with lineage simulation

Use it to turn broad design prompts into a whole-design pass that connects:
- monthly loop
- module pressure
- player commands
- consequence flow
- shell projection
- MVP cut discipline

## Use This Skill When

- designing or reviewing the main loop, monthly loop, or subsystem loop
- deciding whether a feature is rules-driven enough
- deciding how a mechanic becomes visible in the shell
- shaping player commands, leverage, and limitations
- planning a vertical slice or MVP cut
- checking whether a proposal drifts into event-pool design, detached tactics design, or spreadsheet-with-flavor design
- connecting family, commoners, office, trade, conflict, order, and warfare into one living-world field

## Workflow

1. Read project truth first.

   Start with:
   - `docs/PRODUCT_SCOPE.md`
   - `docs/SIMULATION.md`
   - `docs/PLAYER_SCOPE.md`
   - `docs/MVP.md`

2. Identify the design layer.

   Determine whether the task is mainly about:
   - loop structure
   - pressure chain
   - player leverage
   - command resolution
   - shell projection
   - MVP cut or vertical slice

3. Load only the references you need.

   - Read [references/rules-driven-living-world.md](references/rules-driven-living-world.md) for the design thesis and anti-patterns.
   - Read [references/pressure-chains-and-causality.md](references/pressure-chains-and-causality.md) for how pressure starts, travels, and becomes visible.
   - Read [references/command-resolution-and-bounded-leverage.md](references/command-resolution-and-bounded-leverage.md) for player influence, command windows, and anti-god rules.
   - Read [references/vertical-slice-and-mvp-shaping.md](references/vertical-slice-and-mvp-shaping.md) for slice design, cut discipline, and MVP shaping.

3a. Treat short design prompts as auto-deep-linked.

   If the user says things like:
   - `rules driven`
   - `living world`
   - `main loop`
   - `MVP`
   - `vertical slice`

   do not stop at a shallow design summary.

   Default to:
   - identify the underlying loop
   - identify which module pressure starts the chain
   - identify who carries that pressure
   - identify how the player can intervene
   - identify how the result becomes visible in shell, notices, or vignettes
   - identify what should be cut, deferred, or left as flavor

3b. Treat broad design prompts as whole-system prompts.

   If the user says things like:
   - `game design`
   - `full design pass`
   - `rules-driven living world`
   - `connect the design`

   then connect:
   - world simulation
   - household and lineage pressure
   - economy and mobility
   - office and order
   - force and conflict
   - narrative projection
   - player review and bounded command

4. Convert design talk into playable structure.

   A good result should answer:
   - what rule owns the change
   - what pressure enters the month
   - what module or phase resolves it
   - what the player can actually do
   - what the shell shows
   - what stays out of scope for now

## Output Rules

- Do not design core play around random event pools.
- Do not let flavor text become the authoritative driver.
- Do not give the player omnipotent direct control.
- Do not treat every subsystem as its own game mode.
- Do not solve missing structure with more narrative dressing.
- Do not detach warfare into a separate tactics game.
- Do not design commands as guaranteed outcomes; they are intents resolved against autonomy, institutions, resources, and risk.
- Prefer pressure chains over isolated features.
- Prefer monthly consequences over instant gratification loops when the project fantasy depends on delayed outcomes.
- Prefer readable cause traces over hidden simulation magic.
- Prefer vertical slices that prove the living-world loop over broad but thin content coverage.

## Zongzu-Specific Guidance

- The world advances before the player acts.
- Review and interpretation are part of the game, not downtime between actions.
- The player wins through leverage, preparation, timing, and judgment, not through universal command authority.
- Family, commoner pressure, exams, trade, office, disorder, and force should behave like one linked field, even when some packs are still lite or disabled.
- Conflict belongs to the same world and scheduler as household and office pressure.
- A good Zongzu slice proves that structured diffs, projections, and bounded commands already make the shell feel alive.
- MVP should feel like sitting in the hall and hearing the world arrive, not like sampling disconnected mechanics.
