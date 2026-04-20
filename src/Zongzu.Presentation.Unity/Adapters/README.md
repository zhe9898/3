# Adapter Conventions

Adapters in this folder are thin surface mappers.

## What belongs here

- one surface-facing adapter per shell surface or lane
- small presentation-local helper types
- text glue that preserves existing wording
- command/receipt mapping helpers

## What does not belong here

- authoritative game rules
- save/schema logic
- cross-module state mutation
- large ordering/lookup setup that can be shared cleanly through a projection context

## Naming

- `*ShellAdapter` for surface mappers
- `*TextAdapter` for presentation-local copy glue
- `*CommandSelector` for narrow command-picking helpers

## Expected shape

A healthy adapter usually:
1. consumes a bundle or projection context
2. maps into one surface view model
3. delegates repeated text or selection glue to a local helper

If an adapter starts accumulating:
- repeated sorting
- dictionary/index setup
- lead-item selection reused by multiple methods

move that work into `ProjectionContexts/`.
