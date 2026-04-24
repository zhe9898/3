# ENGINEERING_RULES

For the bridge between these project rules and modern C# / Unity / game-engineering practice, see `MODERN_GAME_ENGINEERING_STANDARDS.md`.

## 1. Determinism is mandatory
Same seed + same commands + same code version must produce the same authoritative result.

Rules:
- all randomness goes through project RNG interfaces
- iteration order must be stable
- no wall-clock authority
- no unordered collection side effects
- authoritative economy and scheduling avoid float drift

## 2. Time model
- the player-facing authoritative review shell is monthly
- inside each month, nearby lived pressure targets day-level authority steps, with quiet spans batched or skipped deterministically
- `xun` labels may exist as almanac wording, projection grouping, or loose schedule windows, not as the preferred bottom authority grid
- module cadence must be declared as `day`, `month`, `seasonal`, command-resolution, or an explicit combination
- `GameDate` must preserve deterministic month identity and support deterministic sub-month cadence
- age may remain stored in months even when pressure moves inside the month
- no subsystem may invent hidden clocks or bypass the scheduler cadence contract

## 3. Backend implementation order
- stabilize structure and contracts before deepening formulas
- define module ownership, state shape, cadence bands, command/query/event contracts, and save schema first
- treat read models, receipts, and projections as downstream contracts, not ad hoc UI convenience objects
- introduce dense rules only after the owning module's structure can survive iteration
- every new subsystem should prove at least one minimal live pressure chain before broadening coverage

## 4. State ownership discipline
- every module owns its own state namespace
- only the owning module may mutate that namespace
- other modules may read public projections or queries only
- no direct foreign state mutation, even "just this once"

## 5. Integration discipline
Cross-module integration is limited to:
- **Query**: read-only projection access
- **Command**: player or application intent routed to module owners
- **DomainEvent**: deterministic event emission and handling

Forbidden:
- module A calling module B internals to mutate B state
- UI writing authoritative state
- narrative templates applying gameplay effects

## 6. Application services stay thin
- application services may validate and route intent, but may not become a second rule layer
- command handling in application code must not directly mutate foreign module state in ways that bypass owning module logic
- if a command needs domain consequence logic, move that consequence into the owning module instead of growing orchestration code

## 7. Structured diffs are mandatory
Every authoritative month must yield a `WorldDiff` or equivalent structured diff set.
Sub-month pulses may emit narrower deterministic diffs, but monthly review must consolidate them into a readable authoritative summary.
Player-facing important notices must trace back to those diffs.

## 8. Explanations are mandatory
Major outcomes must record top causes.
At minimum:
- exam outcomes
- trade gains and losses
- marriage acceptance or refusal
- inheritance changes
- grievance escalations
- conflict outcomes
- campaign outcomes when enabled

## 9. Save and schema rules
- root save must include `RootSchemaVersion`
- each module state must include `ModuleSchemaVersion`
- feature manifest must be persisted
- migrations are mandatory for incompatible changes
- IDs never recycle

## 10. Code style rules
- cohesive files and types
- high cohesion, low coupling
- single files and types must stay maintainable in size; when a file starts carrying multiple responsibilities, split by ownership or workflow seam instead of letting it become a maintenance sink
- no hidden mutable global singletons in authority layers
- prefer value objects over primitive obsession
- side effects must be explicit
- do not bury game rules in UI or config loaders
- authoritative state should store facts, pressures, references, and durable outcomes; shell prose and labels belong downstream
- avoid stringly-typed glue, ad hoc key concatenation, and brittle cross-layer payload stitching when typed contracts can carry the same meaning
- do not grow god classes, catch-all managers, or vague utility sinks that blur ownership
- do not normalize giant source files as acceptable architecture; if review, navigation, testing, or ownership becomes muddy, the file is already too large

## 10a. Codex-readable architecture guardrails
Treat the following as explicit implementation rules rather than style advice.

### File size and splitting
- authority, application, persistence, and projection source files should usually stay under roughly `400` logical lines
- once a non-generated source file pushes past roughly `600` logical lines, splitting is the default expectation rather than an optional cleanup
- if one file mixes two or more of these responsibilities, split it:
  - domain rule resolution
  - command routing
  - persistence or migration
  - projection or shell wording
  - diagnostics or debug helpers
  - external IO

### Authority hot path definition
Authority hot paths include:
- module `day`, `month`, and `seasonal` execution
- deterministic domain-event handling
- month-end consolidation
- authority diff generation

Inside authority hot paths, do **not**:
- perform blocking filesystem IO
- perform blocking network IO
- sleep, wait on arbitrary tasks, or poll wall-clock time
- read secrets or environment-dependent machine state

### Boundary clarity
- application layer may validate and route intent, but may not directly mutate foreign module state
- persistence layer may serialize and migrate state, but may not invent domain consequences
- presentation and read-model code may format and arrange data, but may not decide authority outcomes
- each authoritative type should have one obvious owning module; if ownership is arguable in code review, the boundary is not clear enough yet

### Leakage prevention
The following must never appear in committed player-facing projections, saves, or public read models unless explicitly intended:
- secrets, tokens, or credentials
- local absolute machine paths
- stack traces or internal exception dumps
- hidden future information the player should not know yet
- debug-only traces that belong only in diagnostics

### Dependency hygiene
- do not add a new library if it is marked obsolete, deprecated, end-of-life, or already superseded by an existing project abstraction
- do not add a helper library merely to avoid writing a small explicit adapter
- if a dependency introduces hidden clocks, hidden IO, hidden globals, or hidden caching into authority code, it is the wrong dependency by default

## 11. Security, leakage, and boundary safety
- no secrets, tokens, local machine paths, or private diagnostics may leak into committed assets, save payloads, public read models, or player-facing shell text
- internal authority traces may feed debug-only surfaces, but must not leak hidden future information or private state into normal player projections
- module internals stay private; only declared queries, commands, events, and projections cross boundaries
- do not bypass boundaries through reflection, dynamic dictionaries, or convenience backdoors

## 12. IO and hot-path discipline
- authoritative hot paths must not perform ad hoc blocking IO
- save/load, import/export, diagnostics flushes, and other filesystem work belong at explicit boundaries, not inside per-agent or per-node resolution loops
- external reads and writes must be batched, deferred, or staged so they cannot silently stall day-level or month authority passes
- if a feature requires heavy IO, it must declare where the stall is allowed and how authority stays deterministic

## 13. Dependency and library hygiene
- do not introduce deprecated or obsolete libraries into new code without an explicit repo-level exception and migration note
- prefer current first-party platform APIs and existing project abstractions over one-off helper packages
- avoid libraries that push hidden clocks, hidden IO, hidden caching, or implicit global state into authority code
- remove or quarantine weak transitional glue instead of normalizing it as a permanent layer

## 14. Testing rules
No authoritative change is done without:
- invariant tests
- deterministic replay tests if simulation changes
- save roundtrip tests if schema changes
- module integration tests if new events or queries are added

## 15. Forbidden shortcuts
- no `UnityEngine.Random` in authority code
- no `DateTime.Now` in simulation
- no raw narrative event pool as gameplay authority
- no god-mode commands that bypass resolution
- no undocumented cross-module writes
- no blocking file or network IO inside authority hot loops
- no player-facing shell reading raw authority internals directly
- no silent dependency on deprecated libraries for new feature work
