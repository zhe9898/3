# Household Mobility Runtime Rule Health Evidence V549-V556

## Goal

Record diagnostics/readiness evidence for the first household mobility runtime rule after V541-V548 closeout, without changing runtime behavior.

This pass is a health-evidence and next-gate preflight. It does not add a second rule, widen fanout, tune long-run saturation, implement movement, or change save schema.

## Scope

In scope:
- Confirm the V533-V540 monthly owner rule remains the only household mobility runtime rule in this track.
- Record the current health proof boundary:
  - focused owner tests prove capped eligible targets, no-touch behavior, deterministic ordering/cap, same-seed replay, and malformed config fallback;
  - architecture guards prove owner-only consumption, schema neutrality, no `PersonRegistry` expansion, no Application/UI/Unity authority, no prose parsing, and no runtime plugin marketplace;
  - full build/test hygiene is required for this docs/tests-only pass.
- Document the next gate before any future widening:
  - touched household/pool/settlement counts;
  - deterministic cap/order evidence;
  - same-seed replay evidence;
  - no-touch evidence for quiet/off-scope/distant summaries;
  - long-run pressure-band interpretation before changing formulas;
  - hot-path/cardinality note before fanout expansion.

Out of scope:
- No runtime behavior change.
- No second household mobility runtime rule.
- No direct route-history.
- No household movement command.
- No relocation command.
- No migration economy.
- No class/status engine.
- No movement ledger, selector watermark, target-cardinality state, owner-lane ledger, or cooldown ledger.
- No `PersonRegistry` expansion.
- Application/UI/Unity do not calculate household mobility outcomes.
- No parsing of `DomainEvent.Summary`, projection prose, receipt text, public-life lines, or docs text.
- No runtime plugin marketplace, arbitrary script rules, runtime assemblies, or reflection-heavy rule loading.
- No long-run saturation tuning.
- No performance optimization claim.

## Touched Modules

- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this health-evidence pass.

## Schema / Save Impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, serialized diagnostic state, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism

No runtime behavior changes in this pass. Determinism remains the V533-V540 rule evidence:
- active pools ordered by outflow pressure descending, then settlement id;
- household candidates scored from existing numeric state, ordered by score descending, then household id;
- caps/threshold/delta validated with deterministic fallback;
- no random choice, unordered traversal, IO, prose parsing, reflection, runtime assembly loading, or external data reads are introduced by V549-V556.

## Health Gate Notes

The current rule is not yet a movement system. Before a later PR widens it or adds another owner-lane rule, that PR must explicitly answer:
- Which owner state changes, if any, are required?
- What household/pool/settlement counts are touched per monthly pass?
- What deterministic order and cap choose targets before any fanout?
- Which quiet households, off-scope settlements, and distant summaries remain no-touch?
- Whether long-run high migration pressure is intended historical stress, missing recovery, missing allocation, or projection debt.
- Whether the hot path needs performance evidence before widening fanout.

V549-V556 does not answer those by code. It records them as the next gate.

## Milestones

1. Add V549-V556 health-evidence ExecPlan.
2. Add architecture guard proving this is diagnostics/readiness evidence only:
   - no runtime behavior change;
   - no second runtime rule;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift;
   - no long-run saturation tuning;
   - no performance optimization claim.
3. Update required docs and acceptance evidence.
4. Run focused architecture validation plus full build/test hygiene.

## Validation Plan

- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_rule_health_evidence_v549_v556_must_remain_diagnostics_only_without_runtime_or_schema_drift"`
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
- Passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_runtime_rule_health_evidence_v549_v556_must_remain_diagnostics_only_without_runtime_or_schema_drift"`.
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build`.
- Evidence note: the full no-build run produced ten-year diagnostic output with replay hash `F9823C1020EFDE3BF55825CB65D27F161BFE2F40107BC77050311A2E3EA04FD4`; V549-V556 records that as health context only and makes no long-run saturation tuning or performance optimization claim.

PR validation:
- Pending: PR CI.

## Rollback Path

Remove the V549-V556 docs and architecture guard. No production code or save/schema rollback is required.
