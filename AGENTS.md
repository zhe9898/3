# AGENTS.md

## Purpose
Build a **Windows single-player lineage simulation game** whose authoritative architecture is a **modular monolith**.

The game must preserve these product truths:
- the world advances before the player acts
- notifications are projections of state changes, not the driver of state changes
- the player influences the world through bounded local leverage, not omnipotent buttons
- adults are partially autonomous
- clans remember favors, shame, debt, fear, and grudges over generations
- presentation is **spatialized** rather than a raw spreadsheet, raw text parser, or card battler
- conflict and warfare are integrated extensions of the lineage simulation, not a separate tactics game

The game must preserve these engineering truths:
- modules own their own state
- cross-module coordination uses **Query / Command / DomainEvent**
- authoritative simulation remains deterministic
- save data is versioned at both root and module levels
- MVP foundations are the substrate for later releases

## Mandatory read order before non-trivial work
1. `docs/README.md`
2. `docs/PRODUCT_SCOPE.md`
3. `docs/MVP_SCOPE.md`
4. `docs/POST_MVP_SCOPE.md`
5. `docs/VERSION_ALIGNMENT.md`
6. `docs/TECH_STACK.md`
7. `docs/ENGINEERING_RULES.md`
8. `docs/ARCHITECTURE.md`
9. `docs/MODULE_BOUNDARIES.md`
10. `docs/EXTENSIBILITY_MODEL.md`
11. `docs/MODULE_INTEGRATION_RULES.md`
12. `docs/SCHEMA_NAMESPACE_RULES.md`
13. `docs/DATA_SCHEMA.md`
14. `docs/SOCIAL_STRATA_AND_PATHWAYS.md`
15. `docs/SIMULATION.md`
16. `docs/PLAYER_SCOPE.md`
17. `docs/RELATIONSHIPS_AND_GRUDGES.md`
18. `docs/CONFLICT_AND_FORCE.md`
19. `docs/VISUAL_FORM_AND_INTERACTION.md`
20. `docs/UI_AND_PRESENTATION.md`
21. `docs/MVP.md`
22. `docs/IMPLEMENTATION_PHASES.md`
23. `docs/ACCEPTANCE_TESTS.md`
24. `docs/CODEX_TASK_PROMPTS.md`
25. `docs/CODEX_MASTER_SPEC.md`

For large tasks also read:
- `PLANS.md`
- `docs/exec-plans/README.md`

## Non-negotiable product constraints
1. **No event-pool core loop**
   - authoritative state changes happen before narrative projection
   - notifications are downstream of structured diffs

2. **No player-as-god design**
   - player influence flows through money, prestige, favors, clan authority, office, and force
   - commands resolve against autonomy, institutions, logistics, and risk

3. **No rigid career rails**
   - scholar, merchant, official, soldier, outlaw, and commoner positions are social states and pathways, not isolated skill trees

4. **Adults are not puppets**
   - adults may resist, reinterpret, delay, evade, or exploit commands

5. **Grudges persist**
   - memory and grudge systems must support multi-generational continuity

6. **Spatialized shell is mandatory**
   - great hall / study, ancestral hall, desk sandbox, and conflict vignettes are the visual shell
   - raw database presentation is insufficient

7. **Conflict remains system-integrated**
   - MVP conflict is abstract
   - post-MVP warfare is campaign-level desk-sandbox projection
   - no detached RTS or unit micro layer

8. **MVP and post-MVP must align structurally**
   - later releases extend the same kernel, commands, events, projections, save IDs, and monthly tick

## Non-negotiable architecture constraints
1. The project is a **modular monolith**, not a giant world manager and not a runtime plugin marketplace.
2. `Zongzu.Kernel` and simulation modules must not reference Unity APIs.
3. Every simulation module owns its own state namespace.
4. Modules may not mutate another module's authoritative state directly.
5. Cross-module interaction is limited to:
   - read-only queries/projections
   - commands routed through application services
   - domain events emitted and handled deterministically
6. New module state requires:
   - schema entry in `docs/DATA_SCHEMA.md`
   - namespace/version rules in `docs/SCHEMA_NAMESPACE_RULES.md`
   - boundary entry in `docs/MODULE_BOUNDARIES.md`
   - integration notes in `docs/MODULE_INTEGRATION_RULES.md`
7. New feature packs require:
   - explicit registration
   - save manifest update
   - acceptance test updates
8. UI code may never contain authoritative game rules.

## Work process
For any task larger than a tiny fix:
1. identify touched modules and docs
2. create or update an ExecPlan under `docs/exec-plans/active/`
3. implement one milestone at a time
4. add or update tests first or alongside code
5. update docs if behavior, schema, boundaries, or scope changed
6. keep save compatibility and determinism notes in the ExecPlan

## Done criteria
A task is done only if:
- code compiles
- tests pass
- docs are updated
- determinism remains valid if simulation changed
- save compatibility impact is documented
- module boundaries remain clean
- the change still matches the world-first design
