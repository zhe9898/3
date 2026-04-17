# ACCEPTANCE_TESTS

## Global acceptance rules
Every release line must pass:
- deterministic replay tests
- save roundtrip tests
- invariant tests
- feature-manifest compatibility tests
- module boundary tests where practical

## Phase M0
- 12-month empty/minimal world replay equality
- save root manifest validation
- module registration order deterministic

## Phase M1
- no impossible family states
- household membership invariants hold
- commoner pressure can rise/fall deterministically
- grudges can persist across multiple years

## Phase M2
- six-module replay remains deterministic across repeated seeds
- 60-month replay remains deterministic across multiple representative seeds
- headless diagnostics harness can run 120 months and report bounded notification growth plus save payload sizes
- diagnostics harness and debug shell expose aligned runtime metrics for diff entries, domain events, notifications, and save payload bytes
- M2 save roundtrip preserves all enabled module namespaces including `NarrativeProjection`
- legacy M0-M1 saves load through the M2 loader when newer modules remain disabled
- explicit root schema mismatches are rejected at load time
- explicit module schema mismatches are rejected at load time
- exam outcomes explainable
- trade outcomes explainable
- disabled M2-lite modules stay absent from save output
- notifications trace back to `WorldDiff` entries
- notification history retention stays bounded and trims oldest notices first
- runtime-only debug traces reset after save/load and do not affect compatibility
- first-pass shell composes from read-model bundles only
- debug panel exposes seed, enabled modules, recent traces, inspectors, and invariant/warning output without authority writes
- presentation projects do not reference authority modules directly
- notifications trace back to diffs
- UI shell can display all required surfaces without authority leakage

## Phase M3
- before active M3 integration, placeholder `OrderAndBanditry` and `ConflictAndForce` modules expose stable no-op query seams and remain absent from active M2 bootstraps
- local conflict resolution deterministic
- conflict aftermath affects owned modules only through events
- no tactical micro inputs exist

## Post-MVP packs
### Office pack
- appointment authority affects only office-owned state directly
- family/trade changes happen through events and handlers

### Order/banditry pack
- outlaw/banditry state can be enabled without schema collisions
- route insecurity affects trade via queries/events, not direct mutation

### Force pack
- force pool math deterministic
- command cap and supply effects reproducible
- local conflict logs readable

### Warfare pack
- campaign outcomes reproducible from seed + inputs
- war overlay remains campaign-level, no unit-micro authority
- campaign aftermath updates other modules only via events

## Boundary tests
At integration level, verify:
- no module writes foreign namespace
- projections remain read-only
- disabled feature packs load clean defaults
