# ENGINEERING_RULES

## 1. Determinism is mandatory
Same seed + same commands + same code version must produce the same authoritative result.

Rules:
- all randomness goes through project RNG interfaces
- iteration order must be stable
- no wall-clock authority
- no unordered collection side effects
- authoritative economy/scheduling avoid float drift

## 2. Time model
- one authoritative tick = one month
- `GameDate { Year, Month }`
- age stored in months
- no subsystem may invent hidden clocks

## 3. State ownership discipline
- every module owns its own state namespace
- only the owning module may mutate that namespace
- other modules may read public projections/queries only
- no direct foreign state mutation, even “just this once”

## 4. Integration discipline
Cross-module integration is limited to:
- **Query**: read-only projection access
- **Command**: player/application intent routed to module owners
- **DomainEvent**: deterministic event emission and handling

Forbidden:
- module A calling module B internals to mutate B state
- UI writing authoritative state
- narrative templates applying gameplay effects

## 5. Structured diffs are mandatory
Every authoritative month must yield a `WorldDiff` or equivalent structured diff set.
Player-facing important notices must trace back to those diffs.

## 6. Explanations are mandatory
Major outcomes must record top causes.
At minimum:
- exam outcomes
- trade gains/losses
- marriage acceptance/refusal
- inheritance changes
- grievance escalations
- conflict outcomes
- campaign outcomes when enabled

## 7. Save and schema rules
- root save must include `RootSchemaVersion`
- each module state must include `ModuleSchemaVersion`
- feature manifest must be persisted
- migrations are mandatory for incompatible changes
- IDs never recycle

## 8. Code style rules
- cohesive files and types
- no hidden mutable global singletons in authority layers
- prefer value objects over primitive obsession
- side effects must be explicit
- do not bury game rules in UI or config loaders

## 9. Testing rules
No authoritative change is done without:
- invariant tests
- deterministic replay tests if simulation changes
- save roundtrip tests if schema changes
- module integration tests if new events/queries are added

## 10. Forbidden shortcuts
- no `UnityEngine.Random` in authority code
- no `DateTime.Now` in simulation
- no raw narrative event pool as gameplay authority
- no god-mode commands that bypass resolution
- no undocumented cross-module writes
