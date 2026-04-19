# Docs README

This directory contains the authoritative specification for the project.

## Read order
1. `PRODUCT_SCOPE.md`
2. `RULES_DRIVEN_LIVING_WORLD.md`
3. `MVP_SCOPE.md`
4. `POST_MVP_SCOPE.md`
5. `VERSION_ALIGNMENT.md`
6. `TECH_STACK.md`
7. `ENGINEERING_RULES.md`
8. `ARCHITECTURE.md`
9. `MODULE_BOUNDARIES.md`
10. `EXTENSIBILITY_MODEL.md`
11. `MODULE_INTEGRATION_RULES.md`
12. `SCHEMA_NAMESPACE_RULES.md`
13. `DATA_SCHEMA.md`
14. `SOCIAL_STRATA_AND_PATHWAYS.md`
15. `SIMULATION.md`
16. `PLAYER_SCOPE.md`
17. `INFLUENCE_POWER_AND_FACTIONS.md`
18. `RELATIONSHIPS_AND_GRUDGES.md`
19. `CONFLICT_AND_FORCE.md`
20. `VISUAL_FORM_AND_INTERACTION.md`
21. `UI_AND_PRESENTATION.md`
22. `MVP.md`
23. `IMPLEMENTATION_PHASES.md`
24. `ACCEPTANCE_TESTS.md`
25. `CODEX_TASK_PROMPTS.md`
26. `CODEX_MASTER_SPEC.md`

## Document groups

### Product and scope
- `PRODUCT_SCOPE.md`
- `RULES_DRIVEN_LIVING_WORLD.md`
- `MVP_SCOPE.md`
- `POST_MVP_SCOPE.md`
- `VERSION_ALIGNMENT.md`
- `MVP.md`

### Engineering and architecture
- `TECH_STACK.md`
- `ENGINEERING_RULES.md`
- `ARCHITECTURE.md`
- `MODULE_BOUNDARIES.md`
- `EXTENSIBILITY_MODEL.md`
- `MODULE_INTEGRATION_RULES.md`
- `SCHEMA_NAMESPACE_RULES.md`
- `DATA_SCHEMA.md`

### Simulation and feature domains
- `MULTI_ROUTE_DESIGN_MATRIX.md`
- `SIMULATION.md`
- `PLAYER_SCOPE.md`
- `INFLUENCE_POWER_AND_FACTIONS.md`
- `SOCIAL_STRATA_AND_PATHWAYS.md`
- `RELATIONSHIPS_AND_GRUDGES.md`
- `CONFLICT_AND_FORCE.md`

### Presentation
- `VISUAL_FORM_AND_INTERACTION.md`
- `UI_AND_PRESENTATION.md`

### Delivery and execution
- `IMPLEMENTATION_PHASES.md`
- `ACCEPTANCE_TESTS.md`
- `CODEX_TASK_PROMPTS.md`
- `CODEX_MASTER_SPEC.md`

## Core concept
The game is a **spatialized lineage simulation**.
The player is a clan head inside a living world.
The architecture is a **modular monolith** whose modules own their own state and integrate through deterministic Query/Command/DomainEvent flows.
The higher-level design manifesto for how that living world should behave is captured in `RULES_DRIVEN_LIVING_WORLD.md`.
