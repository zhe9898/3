# Projection Context Conventions

Projection contexts are precomputed read-only helpers for the shell layer.

## What belongs here

- ordered arrays used by one or more surface mappings
- dictionary / lookup preparation
- counters and summary numbers
- lead-item or lead-surface selection
- shared downstream projection setup used by multiple surfaces

## What does not belong here

- authoritative simulation logic
- direct view-model construction
- UI object grammar
- free-floating wording decisions that are not tied to projection preparation

## Naming

- `*ProjectionContext`

## Expected shape

A projection context should:
- be created from `PresentationReadModelBundle`
- expose already-sorted or already-grouped data
- stay deterministic and side-effect free
- make adapters smaller, not smarter

If a context starts choosing how a surface should look instead of preparing what the surface needs to read, it is probably too high-level and should move back toward an adapter helper.
