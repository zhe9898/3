# UI_AND_PRESENTATION

This document defines information hierarchy, screens, and interaction responsibilities.

## Information hierarchy
### Tier 1: urgent
Needs immediate player attention:
- death, inheritance, core marriage, legal emergency, critical conflict, campaign collapse

### Tier 2: consequential
Important soon:
- branch tension
- exam result
- trade route loss
- rising outlaw pressure
- local security decline

### Tier 3: background
Useful but non-blocking:
- rumors
- local market drift
- distant kin information
- ambient reports

## Core screens / surfaces
1. great hall dashboard
2. lineage surface
3. person inspector
4. household/clan inspector
5. desk sandbox settlement view
6. ledger/report view
7. notification center
8. conflict result surface
9. campaign board surface later

## UI architecture rules
- use view models or read models
- presentation reads projections/query services
- presentation sends commands
- presentation does not hold authoritative logic

## Current first-pass implementation note
- the current repository implements a first-pass shell as view-model composition rather than final Unity scenes
- `Zongzu.Presentation.Unity` consumes a read-model bundle exported by application code
- family, exam, trade, settlement, and notification panels are composed from projections only
- development-facing debug panels are also composed from read-only projections: seed, enabled modules, module inspectors, recent diff traces, recent domain events, warnings, and invariant status
- authoritative simulation state stays inside application/module layers

## Explainability rule
Every major visible outcome needs:
- what happened
- affected actors
- why it happened
- what the player can do next

## Debug UI minimum
Development builds must expose:
- current seed
- current date
- replay hash
- enabled feature packs
- diff traces
- module state inspectors
- warning/invariant list
- campaign traces when warfare pack is enabled

Current repository note:
- the first-pass shell now carries a read-only debug panel for seed, feature packs, module inspectors, recent diff traces, and warnings/invariants
- latest-month debug traces are runtime diagnostics only and are not part of save compatibility
