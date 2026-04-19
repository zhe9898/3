# Zongzu Copilot Instructions

## General Guidelines
- Read local skill packs first when relevant:
  - `.github/skills/zongzu-ancient-china/SKILL.md`
  - `.github/skills/zongzu-game-design/SKILL.md`
  - `.github/skills/zongzu-ui-shell/SKILL.md`
- Use those skill packs in whole-skill mode, including their bundled `references/` and `agents/` material when relevant.
- After the local skill packs, read:
  - `docs/FULL_SYSTEM_SPEC.md`
  - `docs/ENGINEERING_RULES.md`
  - `docs/ARCHITECTURE.md`
  - `docs/SIMULATION.md`
  - `docs/SIMULATION_FIDELITY_MODEL.md`
- Prefer repository-local skill material over generic advice when the topic is historical grounding, living-world rules, shell/UI direction, map sandbox direction, bounded leverage, cadence, or simulation architecture.

## Project-Specific Rules
- This project is a Northern Song-inspired, multi-route, rules-driven simulation of a living Chinese ancient society. Do not reduce it to a simple lineage manager, event-pool game, or god-game.
- The world self-runs before the player acts. The player has bounded leverage, not direct omnipotent control.
- Backend architecture is a modular monolith: one scheduler, one save root, one world-state container, multiple authoritative modules with owned state and explicit contracts.
- Use `xun / month / seasonal` cadence. The shell is monthly review; the living world may pulse inside the month.
- Modules own their own state. Cross-module interaction should go through Query, Command, and DomainEvent. Do not directly mutate foreign module state.
- Keep Application thin. Do not turn orchestration code into a second rule engine.
- Keep UI, read models, and shell wording downstream of authoritative simulation. UI must not resolve authority outcomes.
- Prefer structure and contracts before deepening formulas. New systems should fit existing ownership, cadence, save, migration, and projection rules.
- Keep code high-cohesion and low-coupling. Split oversized files by ownership or workflow seam. Avoid weak glue layers, deprecated libraries, blocking IO in authority hot paths, and leakage of debug/internal data into player-facing surfaces.
- For historical or social questions, treat society as alive across multiple routes: household survival, lineage management, commerce, office, local order, conflict, and later macro pressure all belong to one linked field.
- For gameplay design, prefer pressure chains over event pools, bounded commands over guaranteed outcomes, and readable cause traces over hidden simulation magic.
- For shell and map work, prefer object-anchored 2.5D hall/desk/sandbox surfaces over poster-like dashboards or flat platform-tool layouts.

## Ownership Model
- The Zongzu project uses the Person ownership model B: Kernel-layer PersonRegistry (identity-only anchor) + per-domain-module person state. FamilyCore kinship is clan-scoped only. See `PERSON_OWNERSHIP_RULES.md`.
