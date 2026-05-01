# Household Mobility Runtime Widening Gate V557-V564

## Goal

Freeze the widening gate for household mobility runtime work after V549-V556 health evidence.

This pass is a docs/tests preflight for future fanout or formula expansion. It does not widen the current rule, add counters, add diagnostics state, change formulas, or add a performance claim.

## Scope

In scope:
- State the minimum evidence required before a later PR can widen the V533-V540 first runtime rule:
  - target scope and owner state;
  - monthly touched household/pool/settlement counts;
  - deterministic cap/order and tie-break proof;
  - same-seed replay proof;
  - quiet/off-scope/distant-summary no-touch proof;
  - pressure-band interpretation before recovery/decay changes;
  - hot-path/cardinality note before performance work.
- Confirm the current runtime rule remains:
  - owner: `PopulationAndHouseholds`;
  - cadence: monthly;
  - default fanout: one active pool and two households;
  - output: existing `MigrationRisk`, `IsMigrating`, `MigrationPools`, and the existing `MigrationStarted` threshold receipt only.
- Add architecture-test evidence that this pass does not become the widening implementation.

Out of scope:
- No runtime behavior change.
- No fanout widening.
- No second household mobility runtime rule.
- No direct route-history.
- No household movement command.
- No relocation command.
- No migration economy.
- No class/status engine.
- No recovery/decay formula change.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, cooldown ledger, touch-count state, diagnostic state, or performance cache.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes, touched counts, target eligibility, health classification, or performance status.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No runtime plugin marketplace, arbitrary script rules, runtime assemblies, or reflection-heavy rule loading.
- No long-run saturation tuning.
- No performance optimization claim.

## Touched Modules

- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this widening-gate pass.

## Schema / Save Impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, touch-count state, diagnostic state, performance cache, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism

No runtime behavior changes in this pass. The existing V533-V540 deterministic path remains unchanged:
- active pools ordered by outflow pressure descending, then settlement id;
- household candidates scored from existing numeric state, ordered by score descending, then household id;
- caps/threshold/delta validated with deterministic fallback;
- no random choice, unordered traversal, IO, prose parsing, reflection, runtime assembly loading, external data reads, counters, or caches are introduced by V557-V564.

## Widening Gate

A future widening PR must not start from "raise the cap" or "lower the pressure" alone. It must first declare:
- Owner: still `PopulationAndHouseholds`, unless a separate owner-lane plan proves otherwise.
- Target scope: player-near households, pressure-hit local households, active-region pools, or distant summaries.
- Current and proposed fanout: households, pools, and settlements touched per month.
- Deterministic order: pool ordering, household ordering, and all tie-break priorities.
- No-touch proof: quiet households, lower-priority active pools, off-scope settlements, distant pooled society, `PersonRegistry`, Application, UI, and Unity.
- Pressure-band meaning: whether high migration pressure is intended stress, missing recovery, missing allocation, or projection debt.
- Schema decision: whether any proposed state is persisted; if yes, stop and plan schema/migration before implementation.
- Validation lane: focused owner tests first, then architecture/no-touch checks, replay proof, build/diff/encoding/full test hygiene, and performance evidence only if performance is claimed.

## Milestones

1. Add V557-V564 widening-gate ExecPlan.
2. Add architecture guard proving this is docs/tests preflight only:
   - no runtime behavior change;
   - no fanout widening;
   - no second runtime rule;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no diagnostic/touch-count/performance state;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift;
   - no long-run saturation tuning;
   - no performance optimization claim.
3. Update required docs and acceptance evidence.
4. Run focused architecture validation plus full build/test hygiene.

## Validation Plan

- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_widening_gate_v557_v564_must_remain_preflight_only_without_fanout_or_schema_drift"`
- Build:
  - `dotnet build Zongzu.sln --no-restore`
- Diff hygiene:
  - `git diff --check`
- Encoding:
  - touched-file replacement-character scan
- Full no-build solution test:
  - `dotnet test Zongzu.sln --no-build`

## Evidence Log

Local validation:
- Passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_widening_gate_v557_v564_must_remain_preflight_only_without_fanout_or_schema_drift"`.
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build`.
- Evidence note: the full no-build run produced ten-year diagnostic output with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`; V557-V564 records that as context only and makes no fanout widening, long-run saturation tuning, counter/cache addition, or performance optimization claim.

PR validation:
- Pending: PR CI.

## Rollback Path

Remove the V557-V564 docs and architecture guard. No production code or save/schema rollback is required.
