# DOCUMENTATION_MAP

## Purpose

This file is the navigation and authority map for the Zongzu documentation set.
It does not replace the design specs, module specs, schema docs, or execution plans.
It tells readers where each kind of truth lives, how to read the docs without flattening the world, and how to turn a broad design promise into an implemented, tested slice.

## How to read this map

Use this file when you need to answer one of these questions:

- Which document is the authority for this decision?
- Is this document a product thesis, a contract, a design field, a roadmap, or an execution record?
- What should I read before changing a module, save schema, command, projection, Unity shell surface, or historical pressure chain?
- How do I keep post-MVP ambition connected to the current modular-monolith spine?

This map is about alignment, not correction. Existing docs are treated as valid project material unless a future task explicitly deprecates a section.

## Authority tiers

| Tier | Meaning | Examples |
| --- | --- | --- |
| Product authority | Defines the project fantasy, non-negotiable product truths, scope direction, and anti-patterns. | `PRODUCT_SCOPE.md`, `RULES_DRIVEN_LIVING_WORLD.md`, `FULL_SYSTEM_SPEC.md`, `CODEX_MASTER_SPEC.md` |
| Architecture authority | Defines ownership, boundaries, integration seams, save/schema rules, and engineering constraints. | `ARCHITECTURE.md`, `MODULE_BOUNDARIES.md`, `MODULE_INTEGRATION_RULES.md`, `DATA_SCHEMA.md`, `SCHEMA_NAMESPACE_RULES.md` |
| Domain authority | Defines a system field such as lineage, household, social memory, influence, conflict, office, public life, or Renzong pressure. | `PERSON_OWNERSHIP_RULES.md`, `SOCIAL_STRATA_AND_PATHWAYS.md`, `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`, `RENZONG_PRESSURE_CHAIN_SPEC.md`, `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` |
| Presentation authority | Defines spatialized shell grammar, surface read order, player-facing wording, and object anchors. | `VISUAL_FORM_AND_INTERACTION.md`, `UI_AND_PRESENTATION.md`, `SPATIAL_SKELETON_SPEC.md`, `WRITING_AND_COPY_GUIDELINES.md` |
| Delivery authority | Defines phase order, roadmap, acceptance tests, and task execution discipline. | `GAME_DEVELOPMENT_ROADMAP.md`, `IMPLEMENTATION_PHASES.md`, `ACCEPTANCE_TESTS.md`, `PLANS.md`, `docs/exec-plans/README.md` |
| Synthesis / orientation | Compresses the full system for fast re-entry. It should not override a more specific authority doc. | `README.md`, `FULL_SYSTEM_SPEC.md`, `CODEX_MASTER_SPEC.md`, this file |
| Execution record | Captures one bounded task, milestone, or implementation pass. It should not become a hidden spec. | `docs/exec-plans/active/*.md`, `docs/exec-plans/archive/*.md` |

## Source-of-truth matrix

| Question | Start here | Then read | Notes |
| --- | --- | --- | --- |
| What is Zongzu? | `PRODUCT_SCOPE.md` | `FULL_SYSTEM_SPEC.md`, `CODEX_MASTER_SPEC.md`, `RULES_DRIVEN_LIVING_WORLD.md` | Product truth comes before feature appetite. |
| What is the current implementation spine? | `ARCHITECTURE.md` | `MODULE_BOUNDARIES.md`, `MODULE_INTEGRATION_RULES.md`, `STATIC_BACKEND_FIRST.md` | Modular monolith, module-owned state, Query / Command / DomainEvent. |
| Who owns person data? | `PERSON_OWNERSHIP_RULES.md` | `MODULE_BOUNDARIES.md`, `DATA_SCHEMA.md`, `LIVING_WORLD_DESIGN.md` | `PersonRegistry` is identity-only. Domain facts live in modules or read-side projections. |
| How should player-facing person identity be assembled? | `PERSON_OWNERSHIP_RULES.md` | `DATA_SCHEMA.md`, `UI_AND_PRESENTATION.md`, `MODULE_INTEGRATION_RULES.md` | `PersonDossiers` are projection-layer read models over distributed person facts, not an authoritative person table. |
| What is the player's gameplay perspective? | `PLAYER_SCOPE.md` | `PRODUCT_SCOPE.md`, `VISUAL_FORM_AND_INTERACTION.md`, `UI_AND_PRESENTATION.md` | The player continuity is a home-household seat, not a fixed-person RPG identity and not clan-god control. |
| What changes save/schema? | `SCHEMA_NAMESPACE_RULES.md` | `DATA_SCHEMA.md`, `VERSION_ALIGNMENT.md`, `EXTENSIBILITY_MODEL.md` | New authoritative state requires schema/version documentation. Read models alone do not. |
| How does time advance? | `SIMULATION.md` | `MODULE_CADENCE_MATRIX.md`, `SIMULATION_FIDELITY_MODEL.md` | Player review is monthly; day-level inner motion remains scheduler-owned; `xun` is a projection/calendar band rather than the bottom authority grid. |
| How does the living society stay alive? | `RULES_DRIVEN_LIVING_WORLD.md` | `LIVING_WORLD_DESIGN.md`, `SOCIAL_STRATA_AND_PATHWAYS.md`, `MULTI_ROUTE_DESIGN_MATRIX.md` | Pressure chains beat random event pools. |
| What proves the game loop is actually playable? | `RULES_DRIVEN_LIVING_WORLD.md` | `PLAYER_SCOPE.md`, `UI_AND_PRESENTATION.md`, `ACCEPTANCE_TESTS.md` | A slice must connect visible pressure -> readable leverage -> bounded command -> module-owned receipt/refusal/residue -> changed next-month read. |
| How does history enter play? | `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md` | `RENZONG_PRESSURE_CHAIN_SPEC.md`, `SOCIAL_STRATA_AND_PATHWAYS.md`, `INFLUENCE_POWER_AND_FACTIONS.md` | Historical carriers create pressure and windows, not fixed cutscenes. |
| Where does Renzong thin-chain topology live? | `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` | `RENZONG_PRESSURE_CHAIN_SPEC.md`, `MODULE_INTEGRATION_RULES.md`, `ACCEPTANCE_TESTS.md` | The topology index is the current thin-chain ledger; the pressure-chain spec remains the fuller design target. |
| How should the shell look and read? | `VISUAL_FORM_AND_INTERACTION.md` | `UI_AND_PRESENTATION.md`, `SPATIAL_SKELETON_SPEC.md`, `WRITING_AND_COPY_GUIDELINES.md` | Shell surfaces are spatialized projections, not rule owners. |
| What is the roadmap? | `GAME_DEVELOPMENT_ROADMAP.md` | `IMPLEMENTATION_PHASES.md`, `MVP_SCOPE.md`, `POST_MVP_SCOPE.md` | MVP is substrate, not a ceiling. |
| What proves done? | `ACCEPTANCE_TESTS.md` | Relevant ExecPlan, module tests, integration tests, presentation tests | Acceptance should prove causality, boundaries, and shell visibility. |
| How should Codex execute a large task? | `PLANS.md` | `docs/exec-plans/README.md`, `CODEX_TASK_PROMPTS.md`, `CODEX_MASTER_SPEC.md` | Large work gets an ExecPlan before edits. |
| Which Zongzu skill should a Codex pass use? | `CODEX_SKILL_RATIONALIZATION_MATRIX.md` | `AGENTS.md`, relevant skill `SKILL.md`, local references before external sources | The matrix keeps skill usage aligned with current repo facts, external calibration standards, and game direction. |

## Complete document inventory

| Document | Role | Primary authority |
| --- | --- | --- |
| `README.md` | Docs entry point and read paths. | Orientation |
| `PRODUCT_SCOPE.md` | Project thesis, scope, product truths, anti-patterns. | Product |
| `FULL_SYSTEM_SPEC.md` | Whole-system synthesis. | Synthesis |
| `CODEX_MASTER_SPEC.md` | Compact implementation shorthand for Codex. | Synthesis / execution |
| `RULES_DRIVEN_LIVING_WORLD.md` | Living-world design contract and anti-event-pool thesis. | Product / domain |
| `LIVING_WORLD_DESIGN.md` | Long-form world structure and implementation order. | Domain / design field |
| `MVP_SCOPE.md` | MVP substrate and explicit cut lines. | Scope |
| `MVP.md` | MVP capsule. | Scope |
| `POST_MVP_SCOPE.md` | Additive post-MVP feature packs and anti-goals. | Scope |
| `VERSION_ALIGNMENT.md` | Version naming and alignment policy. | Delivery / schema support |
| `GAME_DEVELOPMENT_ROADMAP.md` | Phase roadmap and implementation ladder. | Delivery |
| `IMPLEMENTATION_PHASES.md` | Phase details keyed to roadmap. | Delivery |
| `ACCEPTANCE_TESTS.md` | Phase and feature-pack acceptance criteria. | Delivery / validation |
| `TECH_STACK.md` | Runtime and toolchain choices. | Engineering |
| `ENGINEERING_RULES.md` | General engineering constraints. | Engineering |
| `MODERN_GAME_ENGINEERING_STANDARDS.md` | Current code/system/Unity/content engineering bar. | Engineering |
| `STATIC_BACKEND_FIRST.md` | Structure-first implementation rule. | Engineering / delivery |
| `ARCHITECTURE.md` | Top-level layer architecture. | Architecture |
| `MODULE_BOUNDARIES.md` | Module ownership, queries, commands, events, no-touch zones. | Architecture |
| `MODULE_INTEGRATION_RULES.md` | Allowed integration channels and cross-module rules. | Architecture |
| `EXTENSIBILITY_MODEL.md` | Feature-pack and extension model. | Architecture |
| `SCHEMA_NAMESPACE_RULES.md` | Save namespace and migration rules. | Schema |
| `DATA_SCHEMA.md` | Save, state, event, and read-model data shapes. | Schema |
| `SIMULATION.md` | Simulation flow, cadence, determinism, event handling. | Simulation |
| `SIMULATION_FIDELITY_MODEL.md` | Core/local/wider fidelity policy. | Simulation |
| `MODULE_CADENCE_MATRIX.md` | Per-module day/month/seasonal cadence, plus xun as projection/calendar band. | Simulation |
| `PLAYER_SCOPE.md` | Player identity, reach, and bounded leverage. | Product / domain |
| `PERSON_OWNERSHIP_RULES.md` | Person identity/domain/projection ownership. | Domain / architecture |
| `RELATIONSHIPS_AND_GRUDGES.md` | Memory, grudge, and long-horizon relationship design. | Domain |
| `SOCIAL_STRATA_AND_PATHWAYS.md` | Social routes, status drift, office, law, religion, commoner life. | Domain / historical |
| `MULTI_ROUTE_DESIGN_MATRIX.md` | Route-family design matrix. | Domain |
| `INFLUENCE_POWER_AND_FACTIONS.md` | Influence, power, factional pull, and leverage surfaces. | Domain |
| `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md` | Historical trend translation into pressure chains. | Historical / domain |
| `RENZONG_PRESSURE_CHAIN_SPEC.md` | Renzong pressure-chain topology and event contract preflight. | Historical / domain / integration |
| `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md` | Current Renzong thin-chain topology ledger and implementation-status index. | Historical / domain / integration |
| `CONFLICT_AND_FORCE.md` | Conflict ladder, force types, and warfare relation to lineage simulation. | Domain |
| `MAP_AND_SANDBOX_DIRECTION.md` | Map and sandbox direction. | Presentation / domain |
| `SPATIAL_SKELETON_SPEC.md` | Spatial skeleton, topology, overlays, public surfaces, shell anchors. | Presentation / domain |
| `VISUAL_FORM_AND_INTERACTION.md` | Visual form, spatial anchors, interaction grammar. | Presentation |
| `UI_AND_PRESENTATION.md` | UI information hierarchy, surfaces, adapters, projection boundaries. | Presentation |
| `WRITING_AND_COPY_GUIDELINES.md` | Wording lanes and copy ownership. | Presentation / content |
| `ART_AND_AUDIO_ASSET_SOURCING.md` | Asset sourcing and art/audio constraints. | Presentation / content |
| `DESIGN_CODE_ALIGNMENT_AUDIT.md` | Audit record comparing design and implementation. | Review record |
| `STEP2A_LINEAGE_CONTINUITY_PLAN.md` | Specific lineage continuity planning record. | Planning record |
| `CODEX_TASK_PROMPTS.md` | Reusable task prompt patterns. | Execution |
| `CODEX_SKILL_RATIONALIZATION_MATRIX.md` | Codex skill-pack orchestration, external standard calibration, and current Zongzu skill sequencing. | Execution / governance |
| `DOCUMENTATION_MAP.md` | Documentation authority map and reading guide. | Orientation / governance |

## Recommended read paths

### Fast orientation

1. `README.md`
2. `DOCUMENTATION_MAP.md`
3. `CODEX_MASTER_SPEC.md`
4. `FULL_SYSTEM_SPEC.md`

### Non-trivial implementation

1. `README.md`
2. `DOCUMENTATION_MAP.md`
3. `PRODUCT_SCOPE.md`
4. Relevant product/domain doc
5. Relevant architecture/schema doc
6. Relevant presentation doc if player-facing
7. `ACCEPTANCE_TESTS.md`
8. Active ExecPlan

### Module or command work

1. `ARCHITECTURE.md`
2. `MODULE_BOUNDARIES.md`
3. `MODULE_INTEGRATION_RULES.md`
4. Relevant module/domain doc
5. `DATA_SCHEMA.md` and `SCHEMA_NAMESPACE_RULES.md` if authoritative state changes
6. `ACCEPTANCE_TESTS.md`

### Read-model or Unity shell work

1. `UI_AND_PRESENTATION.md`
2. `VISUAL_FORM_AND_INTERACTION.md`
3. `SPATIAL_SKELETON_SPEC.md` when map/desk/topology surfaces are involved
4. `WRITING_AND_COPY_GUIDELINES.md` when wording changes
5. Relevant read-model entries in `DATA_SCHEMA.md`
6. Relevant architecture boundary docs

### Historical or Renzong pressure work

1. `HISTORICAL_PROCESS_AND_GREAT_TRENDS.md`
2. `SOCIAL_STRATA_AND_PATHWAYS.md`
3. `RENZONG_THIN_CHAIN_TOPOLOGY_INDEX.md`
4. `RENZONG_PRESSURE_CHAIN_SPEC.md`
5. `MODULE_INTEGRATION_RULES.md`
6. `ACCEPTANCE_TESTS.md`

### Save/schema work

1. `SCHEMA_NAMESPACE_RULES.md`
2. `DATA_SCHEMA.md`
3. `VERSION_ALIGNMENT.md`
4. `EXTENSIBILITY_MODEL.md`
5. Relevant module boundary docs

## Turning docs into implementation

For a non-trivial feature, do not jump from a design paragraph directly into code.
Use this chain:

1. Identify the authority doc and the specific promise being implemented.
2. Create an ExecPlan under `docs/exec-plans/active/`.
3. Name the owning module, state, query, command, event, projection, and tests.
4. State save/schema and determinism impact.
5. Implement the smallest complete chain that proves the promise.
6. Update docs only where behavior, boundary, schema, or acceptance changes.
7. Run the appropriate tests and record the result.

## Governance rules

- Do not use this map to create new product rules.
- Do not let synthesis docs override more specific authority docs.
- Do not let an ExecPlan become the only place a permanent contract exists.
- Do not add a new authoritative state shape without updating schema docs.
- Do not add a new player-facing surface fact without identifying its read-model or projection source.
- When adding a new major doc, add it to `README.md`, this inventory, and the relevant roadmap job index.

## Current impact

This documentation map has no runtime, save/schema, determinism, or Unity asset impact.
It is a navigation and governance layer for the existing documentation set.
