# Docs README

This directory contains the authoritative specification for the project.

## Read order
1. `PRODUCT_SCOPE.md`
2. `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
3. `FULL_SYSTEM_SPEC.md`
4. `GAME_DEVELOPMENT_ROADMAP.md`
5. `RULES_DRIVEN_LIVING_WORLD.md`
6. `MVP_SCOPE.md`
7. `POST_MVP_SCOPE.md`
8. `VERSION_ALIGNMENT.md`
9. `TECH_STACK.md`
10. `ENGINEERING_RULES.md`
11. `MODERN_GAME_ENGINEERING_STANDARDS.md`
12. `STATIC_BACKEND_FIRST.md`
13. `ARCHITECTURE.md`
14. `MODULE_BOUNDARIES.md`
15. `EXTENSIBILITY_MODEL.md`
16. `MODULE_INTEGRATION_RULES.md`
17. `SCHEMA_NAMESPACE_RULES.md`
18. `DATA_SCHEMA.md`
19. `SOCIAL_STRATA_AND_PATHWAYS.md`
20. `SIMULATION_FIDELITY_MODEL.md`
21. `SIMULATION.md`
22. `MODULE_CADENCE_MATRIX.md`
23. `PLAYER_SCOPE.md`
24. `INFLUENCE_POWER_AND_FACTIONS.md`
25. `RELATIONSHIPS_AND_GRUDGES.md`
26. `CONFLICT_AND_FORCE.md`
27. `VISUAL_FORM_AND_INTERACTION.md`
28. `UI_AND_PRESENTATION.md`
29. `MAP_AND_SANDBOX_DIRECTION.md`
30. `MVP.md`
31. `IMPLEMENTATION_PHASES.md`
32. `ACCEPTANCE_TESTS.md`
33. `CODEX_TASK_PROMPTS.md`
34. `CODEX_MASTER_SPEC.md`
35. `RENZONG_PRESSURE_CHAIN_SPEC.md`
36. `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`

## Document groups

### Product and scope
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

### Simulation and feature domains
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `SIMULATION.md`
- `MODULE_CADENCE_MATRIX.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `SOCIAL_STRATA_AND_PATHWAYS.md`
- `RENZONG_PRESSURE_CHAIN_SPEC.md`
- `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`
- `RELATIONSHIPS_AND_GRUDGES.md`
- `CONFLICT_AND_FORCE.md`
- `MAP_AND_SANDBOX_DIRECTION.md`

### Presentation
- `VISUAL_FORM_AND_INTERACTION.md`
- `UI_AND_PRESENTATION.md`
- `MAP_AND_SANDBOX_DIRECTION.md`

### Delivery and execution
- `GAME_DEVELOPMENT_ROADMAP.md`
- `IMPLEMENTATION_PHASES.md`
- `ACCEPTANCE_TESTS.md`
- `CODEX_TASK_PROMPTS.md`
- `CODEX_MASTER_SPEC.md`

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
The current Renzong thin-chain implementation topology, scope boundaries, watermarks, receipts, proof tests, and full-chain debt are captured in `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`.
The code/module/system/Unity engineering standard bridge is captured in `MODERN_GAME_ENGINEERING_STANDARDS.md`.
The backend implementation-order rule for keeping structure ahead of deep rules is captured in `STATIC_BACKEND_FIRST.md`.
