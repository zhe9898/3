# Docs README

This directory contains the authoritative specification for the project.

## Read order
1. `PRODUCT_SCOPE.md`
2. `FULL_SYSTEM_SPEC.md`
3. `RULES_DRIVEN_LIVING_WORLD.md`
4. `MVP_SCOPE.md`
5. `POST_MVP_SCOPE.md`
6. `VERSION_ALIGNMENT.md`
7. `TECH_STACK.md`
8. `ENGINEERING_RULES.md`
9. `STATIC_BACKEND_FIRST.md`
10. `ARCHITECTURE.md`
11. `MODULE_BOUNDARIES.md`
12. `EXTENSIBILITY_MODEL.md`
13. `MODULE_INTEGRATION_RULES.md`
14. `SCHEMA_NAMESPACE_RULES.md`
15. `DATA_SCHEMA.md`
16. `SOCIAL_STRATA_AND_PATHWAYS.md`
17. `SIMULATION_FIDELITY_MODEL.md`
18. `SIMULATION.md`
19. `MODULE_CADENCE_MATRIX.md`
20. `PLAYER_SCOPE.md`
21. `INFLUENCE_POWER_AND_FACTIONS.md`
22. `RELATIONSHIPS_AND_GRUDGES.md`
23. `CONFLICT_AND_FORCE.md`
24. `VISUAL_FORM_AND_INTERACTION.md`
25. `UI_AND_PRESENTATION.md`
26. `MAP_AND_SANDBOX_DIRECTION.md`
27. `MVP.md`
28. `IMPLEMENTATION_PHASES.md`
29. `ACCEPTANCE_TESTS.md`
30. `CODEX_TASK_PROMPTS.md`
31. `CODEX_MASTER_SPEC.md`

## Document groups

### Product and scope
- `PRODUCT_SCOPE.md`
- `FULL_SYSTEM_SPEC.md`
- `RULES_DRIVEN_LIVING_WORLD.md`
- `MVP_SCOPE.md`
- `POST_MVP_SCOPE.md`
- `VERSION_ALIGNMENT.md`
- `MVP.md`

### Engineering and architecture
- `TECH_STACK.md`
- `ENGINEERING_RULES.md`
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
- `RELATIONSHIPS_AND_GRUDGES.md`
- `CONFLICT_AND_FORCE.md`
- `MAP_AND_SANDBOX_DIRECTION.md`

### Presentation
- `VISUAL_FORM_AND_INTERACTION.md`
- `UI_AND_PRESENTATION.md`
- `MAP_AND_SANDBOX_DIRECTION.md`

### Delivery and execution
- `IMPLEMENTATION_PHASES.md`
- `ACCEPTANCE_TESTS.md`
- `CODEX_TASK_PROMPTS.md`
- `CODEX_MASTER_SPEC.md`

## Core concept
The game is a **Northern Song-inspired, multi-route, rules-driven simulation of a living Chinese ancient society**.
The player enters that society through a household or lineage position rather than as an all-powerful controller or a permanently elite manager.
Houses may stabilize, rise, fragment, drift sideways, sink into commoner survival, or fall into gray dependence while the wider society keeps moving.
The architecture is a **modular monolith** whose modules own their own state and integrate through deterministic Query/Command/DomainEvent flows.
The single-page whole-system synthesis for Codex and future implementation passes is captured in `FULL_SYSTEM_SPEC.md`.
That synthesis also keeps pluggability explicit: feature packs and `absent / lite / full` depth bands must remain additive rather than turning the codebase into a hard-wired blob.
The higher-level design manifesto for how that living world should behave is captured in `RULES_DRIVEN_LIVING_WORLD.md`.
The fidelity policy for who becomes a full agent, who remains household- or node-level, and how upper layers stay alive as pressure is captured in `SIMULATION_FIDELITY_MODEL.md`.
The per-module time contract for `xun / month / seasonal` execution is captured in `MODULE_CADENCE_MATRIX.md`.
The backend implementation-order rule for keeping structure ahead of deep rules is captured in `STATIC_BACKEND_FIRST.md`.
