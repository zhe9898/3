# STATIC_BACKEND_FIRST

This document defines the preferred implementation order for Zongzu's authoritative backend.
It is mainly for Codex and other implementation agents so that new work does not over-couple unfinished rules into the wrong layer.

It complements:
- `ENGINEERING_RULES.md`
- `ARCHITECTURE.md`
- `MODULE_BOUNDARIES.md`
- `SIMULATION.md`
- `SIMULATION_FIDELITY_MODEL.md`
- `MODULE_CADENCE_MATRIX.md`
- `CODEX_MASTER_SPEC.md`

## Core rule

Build the backend as **static structure first, rule density later**.

That means:
- define stable module boundaries before deep simulation formulas
- define stable state shape before rich behavioral tuning
- define stable cadence, command, diff, and projection contracts before filling every pathway with full rules

This does **not** mean an empty database or dead DTO pile.
It means the first backend milestone should be:
- structurally correct
- schedulable
- saveable
- queryable
- projectable

before it becomes behaviorally rich.

## Why this rule exists

Zongzu is a long-lived living-world project.
If rules are written deeply before structure stabilizes, later work tends to:
- spread rule logic across Application, module state, projections, and UI
- hide authority in helper services or strings
- make schema migration expensive
- make day/month/season cadence harder to reason about

The repo already has a strong structural spine:
- module-per-domain projects
- deterministic scheduler
- save root and per-module schema envelopes
- query contracts
- projection/read-model builder

The safest next step is to deepen rules **through** that spine, not around it.

## What should be stabilized first

Codex should treat these as first-pass backend assets that may be designed before heavy rule writing:

### 1. Module boundaries
- which module owns what
- which module may publish which event
- which module may accept which command
- which module may expose which query

### 2. State containers
- IDs
- references
- lists/pools
- aggregate buckets
- authority state versus projection state

### 3. Time contracts
- `day`
- `month`
- `seasonal`
- `xun` only as calendar/projection grouping unless a transitional implementation note says otherwise
- who runs when
- who only consolidates at month end

### 4. Command contracts
- player intent shape
- accepted command names
- target keys
- receipts
- rejection reasons

### 5. Diff and event contracts
- what kind of structured diffs a module emits
- what event names exist
- what downstream modules may react

### 6. Projection contracts
- read-model bundle shape
- shell-facing summaries
- inspector payload shape
- hall / sandbox / docket feeds

### 7. World topology containers
- route graph
- region / prefecture / county / settlement references
- node ownership
- pool ownership

### 8. Summary pools
- `household_band`
- `labor_pool`
- `marriage_pool`
- `rumor_pool`
- `migration_pool`

These can exist before the final social algorithms are rich.

## What gets filled later

After structure stabilizes, deepen:
- pressure formulas
- escalation thresholds
- migration logic
- marriage opportunity logic
- office and petition dynamics
- route risk and market coupling
- disorder and retaliation logic
- dynastic or regional climate effects

These are the parts that should stay replaceable while the structure holds.

## The minimum live-rule requirement

Do **not** interpret "structure first" as "no rules at all".

Every major backend slice should still prove at least one live pressure chain, for example:
- region pressure -> grain price -> household strain -> diff -> notice
- marriage pressure -> family decision -> branch tension -> diff -> hall projection
- route disorder -> trade margin change -> debt pressure -> receipt

The right pattern is:
- stable skeleton first
- one narrow live rule chain next
- broaden rule density later

Not:
- final grand simulation immediately

## Anti-coupling rules

### 1. Application routes intent; modules own consequences

Application services may:
- validate
- route
- assemble context
- return receipts

Application services should **not** become a second rule engine.

If a command permanently changes authoritative state according to domain-specific thresholds, tradeoffs, or pressure math, that logic should migrate into the owning module.

### 2. Authoritative state stores facts and pressure, not shell prose

Authoritative state may keep:
- durable outcomes
- durable counters
- durable pressure levels
- durable references

Avoid growing authoritative state around:
- final UI labels
- decorative explanation strings
- shell-only prose
- long trace text that really belongs in projection

Short-term compatibility fields may exist, but new work should prefer projection-side wording.

### 3. Projection owns wording, not authority

`NarrativeProjection`, read-model builders, and shell-oriented projection layers should own:
- readable summaries
- hall wording
- notice text
- docket phrasing
- inspector phrasing

They must not become a second authority layer.

### 4. Scheduler contracts come before dense day-level logic

It is acceptable to:
- wire day-level cadence into scheduler contracts first
- keep existing `xun` implementation names only as a transitional compatibility layer when code has not migrated yet
- let some modules remain month-only or thin at first

It is **not** acceptable to:
- invent hidden per-module clocks outside the scheduler
- fake cadence in presentation
- bury time advancement in random helpers

## Current repo interpretation

As of the current codebase state:
- the scheduler and cadence contracts exist
- module-per-domain structure exists
- save/migration structure exists
- read-model bundle structure exists
- many modules still use relatively thin month-level heuristics
- current scheduler code may still expose `xun`-named hooks, but the target doctrine is day-level authority with month-end review; dense short-band module behavior is still sparse

That means the project is still in a good position to follow this rule.
It is **not** too late to keep rules decoupled.

## Practical guidance for Codex

When adding a new backend feature, prefer this order:

1. decide the owning module
2. define or extend stable state shape
3. define or extend command/query/event contracts
4. define cadence placement
5. expose projection shape
6. prove one minimal rule chain
7. deepen formulas only after the chain is stable

## Red flags

Stop and reconsider if a change does any of these:
- adds domain-specific pressure math to `Application` routing code
- stores final narrative wording inside authority state
- writes directly into another module's state "just for now"
- invents private clocks outside `SimulationCadenceBand`
- makes shell read models the only place where a concept exists

## One-line summary

Build Zongzu backend as **stable modules, stable state, stable time contracts, and stable projections first; deepen rules through those structures later**.
