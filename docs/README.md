# Docs README

This directory contains the authoritative specification for the project.

Start with `DOCUMENTATION_MAP.md` when you need to know which document owns a decision.
Start with `CODEX_MASTER_SPEC.md` or `FULL_SYSTEM_SPEC.md` when you need a compact whole-system re-entry.

## Fast paths

### First re-entry

1. `DOCUMENTATION_MAP.md`
2. `CODEX_MASTER_SPEC.md`
3. `FULL_SYSTEM_SPEC.md`
4. `PRODUCT_SCOPE.md`

### Non-trivial implementation work

1. `DOCUMENTATION_MAP.md`
2. `PRODUCT_SCOPE.md`
3. `RULES_DRIVEN_LIVING_WORLD.md`
4. Relevant domain document
5. `ARCHITECTURE.md`
6. `MODULE_BOUNDARIES.md`
7. `MODULE_INTEGRATION_RULES.md`
8. `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md` if state or schema changes
9. `UI_AND_PRESENTATION.md` and `VISUAL_FORM_AND_INTERACTION.md` if player-facing
10. `ACCEPTANCE_TESTS.md`
11. Active ExecPlan

### Full broad read order

1. `DOCUMENTATION_MAP.md`
2. `PRODUCT_SCOPE.md`
3. `FULL_SYSTEM_SPEC.md`
4. `CODEX_MASTER_SPEC.md`
5. `RULES_DRIVEN_LIVING_WORLD.md`
6. `LIVING_WORLD_DESIGN.md`
7. `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
8. `MVP_SCOPE.md`
9. `POST_MVP_SCOPE.md`
10. `GAME_DEVELOPMENT_ROADMAP.md`
11. `VERSION_ALIGNMENT.md`
12. `TECH_STACK.md`
13. `ENGINEERING_RULES.md`
14. `MODERN_GAME_ENGINEERING_STANDARDS.md`
15. `STATIC_BACKEND_FIRST.md`
16. `ARCHITECTURE.md`
17. `MODULE_BOUNDARIES.md`
18. `EXTENSIBILITY_MODEL.md`
19. `MODULE_INTEGRATION_RULES.md`
20. `SCHEMA_NAMESPACE_RULES.md`
21. `DATA_SCHEMA.md`
22. `SIMULATION_FIDELITY_MODEL.md`
23. `SIMULATION.md`
24. `MODULE_CADENCE_MATRIX.md`
25. `PLAYER_SCOPE.md`
26. `PERSON_OWNERSHIP_RULES.md`
27. `SOCIAL_STRATA_AND_PATHWAYS.md`
28. `INFLUENCE_POWER_AND_FACTIONS.md`
29. `MULTI_ROUTE_DESIGN_MATRIX.md`
30. `RELATIONSHIPS_AND_GRUDGES.md`
31. `CONFLICT_AND_FORCE.md`
32. `RENZONG_PRESSURE_CHAIN_SPEC.md`
33. `VISUAL_FORM_AND_INTERACTION.md`
34. `UI_AND_PRESENTATION.md`
35. `SPATIAL_SKELETON_SPEC.md`
36. `MAP_AND_SANDBOX_DIRECTION.md`
37. `WRITING_AND_COPY_GUIDELINES.md`
38. `ART_AND_AUDIO_ASSET_SOURCING.md`
39. `MVP.md`
40. `IMPLEMENTATION_PHASES.md`
41. `ACCEPTANCE_TESTS.md`
42. `CODEX_TASK_PROMPTS.md`

## Document groups

### Product and scope
- `DOCUMENTATION_MAP.md`
- `PRODUCT_SCOPE.md`
- `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
- `FULL_SYSTEM_SPEC.md`
- `GAME_DEVELOPMENT_ROADMAP.md`
- `RULES_DRIVEN_LIVING_WORLD.md`
- `MVP_SCOPE.md`
- `POST_MVP_SCOPE.md`
- `VERSION_ALIGNMENT.md`
- `MVP.md`

### Engineering and architecture
- `TECH_STACK.md`
- `ENGINEERING_RULES.md`
- `MODERN_GAME_ENGINEERING_STANDARDS.md`
- `STATIC_BACKEND_FIRST.md`
- `ARCHITECTURE.md`
- `MODULE_BOUNDARIES.md`
- `EXTENSIBILITY_MODEL.md`
- `MODULE_INTEGRATION_RULES.md`
- `SCHEMA_NAMESPACE_RULES.md`
- `DATA_SCHEMA.md`
- `PERSON_OWNERSHIP_RULES.md`

### Simulation and feature domains
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `SIMULATION.md`
- `MODULE_CADENCE_MATRIX.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `SOCIAL_STRATA_AND_PATHWAYS.md`
- `RENZONG_PRESSURE_CHAIN_SPEC.md`
- `RELATIONSHIPS_AND_GRUDGES.md`
- `CONFLICT_AND_FORCE.md`
- `MAP_AND_SANDBOX_DIRECTION.md`
- `LIVING_WORLD_DESIGN.md`

### Presentation
- `VISUAL_FORM_AND_INTERACTION.md`
- `UI_AND_PRESENTATION.md`
- `MAP_AND_SANDBOX_DIRECTION.md`
- `SPATIAL_SKELETON_SPEC.md`
- `WRITING_AND_COPY_GUIDELINES.md`
- `ART_AND_AUDIO_ASSET_SOURCING.md`

### Delivery and execution
- `GAME_DEVELOPMENT_ROADMAP.md`
- `IMPLEMENTATION_PHASES.md`
- `ACCEPTANCE_TESTS.md`
- `CODEX_TASK_PROMPTS.md`
- `CODEX_MASTER_SPEC.md`
- `DESIGN_CODE_ALIGNMENT_AUDIT.md`
- `STEP2A_LINEAGE_CONTINUITY_PLAN.md`

## Core concept
The game is a **Northern Song-inspired, multi-route, rules-driven simulation of a living Chinese ancient society**.
The player enters that society through a household or lineage position rather than as an all-powerful controller or a permanently elite manager.
Houses may stabilize, rise, fragment, drift sideways, sink into commoner survival, or fall into gray dependence while the wider society keeps moving.
At later scale, the same rules may allow rebellion, polity formation, succession struggle, usurpation, restoration, or dynasty repair; history can bend when earned by pressure, leverage, legitimacy, force, and memory, not by free timeline editing.
The architecture is a **modular monolith** whose modules own their own state and integrate through deterministic Query/Command/DomainEvent flows.
The single-page whole-system synthesis for Codex and future implementation passes is captured in `FULL_SYSTEM_SPEC.md`.
The phase-by-phase implementation index and master roadmap are captured in `GAME_DEVELOPMENT_ROADMAP.md`.
That synthesis also keeps pluggability explicit: feature packs and `absent / lite / full` depth bands must remain additive rather than turning the codebase into a hard-wired blob.
The higher-level design manifesto for how that living world should behave is captured in `RULES_DRIVEN_LIVING_WORLD.md`.
The rule for historical figures, reforms, wars, policies, and great trends entering the simulation as pressure rather than rails is captured in `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`.
The fidelity policy for who becomes a full agent, who remains household- or node-level, and how upper layers stay alive as pressure is captured in `SIMULATION_FIDELITY_MODEL.md`.
The per-module time contract for `xun / month / seasonal` execution is captured in `MODULE_CADENCE_MATRIX.md`.
The code/module/system/Unity engineering standard bridge is captured in `MODERN_GAME_ENGINEERING_STANDARDS.md`.
The backend implementation-order rule for keeping structure ahead of deep rules is captured in `STATIC_BACKEND_FIRST.md`.
