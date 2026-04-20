# Zongzu.Contracts

This project is the shared contract layer between the deterministic authority modules, the application layer, and downstream presentation.

## Folder map

- `Execution/`
  - scheduler-facing runtime contracts, event envelopes, and module execution scopes
- `Queries/`
  - read-only query interfaces and registries that let modules/projectors inspect state without crossing ownership
- `ReadModels/`
  - shell-facing read-model bundles plus stable player-command and hall/governance projections
- `Observability/`
  - debug, runtime-scale, and observability snapshots that stay outside authoritative mutation
- `Persistence/`
  - save-root, feature-manifest, and module-envelope contracts
- `Spatial/`
  - route, settlement, locus, and conflict-anchor contracts used by desk/macro sandbox consumers
- project root
  - small shared enums and vocabulary types that remain cross-cutting rather than folder-specific

## Guardrails

- keep namespaces as `Zongzu.Contracts` unless there is a deliberate API-shape reason to change them
- treat this project as shared language, not as a place for module logic
- prefer moving contracts into the smallest clear bucket instead of growing another flat root pile
