# Zongzu.Presentation.Unity

This project is the read-only shell layer for Zongzu's Unity-facing presentation models.

It exists to turn `Zongzu.Contracts` read models into stable surface view models for:
- great hall
- lineage / ancestral hall
- family council
- desk sandbox
- office surface
- warfare / campaign-lite surface
- notification center
- debug panel

## Layering

- root folder:
  - `FirstPassPresentationShell`
  - project entrypoint types
- `ViewModels/`:
  - shell view models grouped by surface or concern
  - shared shell DTOs that multiple surfaces consume
- `Adapters/`:
  - thin surface mappers grouped by surface or concern
  - local text and command helpers near their owning surface
- `ProjectionContexts/`:
  - precomputed ordering, lookups, and shared projection selections

## Composer rule

`FirstPassPresentationShell` is the composer, not a rule host.

It may:
- sort top-level notification input
- create a shared projection context when multiple surfaces consume the same downstream projection
- call surface adapters in a readable order

It must not:
- contain authoritative game rules
- recreate module logic
- become the place where every surface assembles its own local helper graph

## Adapter rule

A shell adapter should:
- consume `PresentationReadModelBundle` and/or a projection context
- produce one surface view model
- keep local presentation glue close to the surface

A shell adapter should not:
- mutate authority state
- own cross-surface product truth
- duplicate large ordering/lookup setup that belongs in a projection context

## Projection context rule

A projection context is for:
- ordering
- lookups
- counters
- lead-item selection
- shared downstream projection preparation

A projection context is not for:
- authoritative rules
- view-model construction
- free-floating copywriting

## Text helper rule

Small text helpers are acceptable when they:
- stay presentation-local
- preserve existing wording
- reduce adapter noise

They should not:
- invent new product semantics
- replace source-of-truth copy decisions that belong upstream

## Dependency direction

The intended direction is:

`Zongzu.Contracts -> ProjectionContexts -> Adapters -> Surface ViewModels -> FirstPassPresentationShell`

Shared contexts may be built in the composer when multiple surfaces consume the same projection output, as with notifications.
