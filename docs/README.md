# Docs README

This directory contains the authoritative specification for the project.

## Read order
1. `PRODUCT_SCOPE.md`
2. `MVP_SCOPE.md`
3. `POST_MVP_SCOPE.md`
4. `VERSION_ALIGNMENT.md`
5. `TECH_STACK.md`
6. `ENGINEERING_RULES.md`
7. `ARCHITECTURE.md`
8. `MODULE_BOUNDARIES.md`
9. `EXTENSIBILITY_MODEL.md`
10. `MODULE_INTEGRATION_RULES.md`
11. `SCHEMA_NAMESPACE_RULES.md`
12. `DATA_SCHEMA.md`
13. `SOCIAL_STRATA_AND_PATHWAYS.md`
14. `SIMULATION.md`
15. `PLAYER_SCOPE.md`
16. `RELATIONSHIPS_AND_GRUDGES.md`
17. `CONFLICT_AND_FORCE.md`
18. `VISUAL_FORM_AND_INTERACTION.md`
19. `UI_AND_PRESENTATION.md`
20. `MVP.md`
21. `IMPLEMENTATION_PHASES.md`
22. `ACCEPTANCE_TESTS.md`
23. `CODEX_TASK_PROMPTS.md`
24. `CODEX_MASTER_SPEC.md`

## Document groups

### Product and scope
- `PRODUCT_SCOPE.md`
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
- `SIMULATION.md`
- `PLAYER_SCOPE.md`
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
