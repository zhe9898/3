# Household Mobility First Runtime Rule Closeout V541-V548

## Goal

Close out the V501-V540 household mobility runtime-rule track after the first small owner-lane monthly rule landed.

This pass is docs/tests closeout. It does not add runtime behavior, a second household mobility runtime rule, migration movement, route history, persisted state, or UI/Application authority.

## Scope

In scope:
- Confirm V533-V540 remains the only first household mobility runtime rule in this track.
- Record the rule as a bounded pressure nudge over existing `PopulationAndHouseholds` state:
  - owner: `PopulationAndHouseholds`;
  - cadence: monthly;
  - deterministic pool and household ordering;
  - default fanout: one active pool and two households;
  - output: existing `MigrationRisk`, `IsMigrating`, `MigrationPools`, and existing `MigrationStarted` threshold receipt.
- Freeze the closeout boundary across docs and architecture tests.
- Record that future work needs a new ExecPlan before any additional runtime rule, projection surface, persisted state, movement command, or route-history model.

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

## Touched Modules

- `tests/Zongzu.Architecture.Tests`
- Documentation listed in the user-required household mobility doc update set.

No production source changes are planned for this closeout pass.

## Schema / Save Impact

Target schema/migration impact: none.

No persisted fields, module schema version bump, root save version change, migration step, save-manifest membership change, route-history state, movement ledger, selector watermark, target-cardinality state, cooldown ledger, owner-lane ledger, durable residue, rules-data file, loader, config namespace, or serialized projection cache is added.

`PopulationAndHouseholds` remains schema `3`.

## Determinism

No runtime behavior changes in this pass. Determinism remains the V533-V540 rule evidence:
- active pools ordered by outflow pressure descending, then settlement id;
- household candidates scored from existing numeric state, ordered by score descending, then household id;
- caps/threshold/delta validated with deterministic fallback;
- no random choice, unordered traversal, IO, prose parsing, reflection, runtime assembly loading, or external data reads.

## Milestones

1. Add V541-V548 closeout ExecPlan.
2. Add architecture guard proving this is closeout only:
   - no second runtime rule;
   - no movement authority;
   - no route-history model;
   - no schema drift;
   - no prose parsing;
   - no `PersonRegistry` expansion;
   - no Application/UI/Unity authority drift.
3. Update required docs and acceptance evidence.
4. Run focused architecture validation plus full build/test hygiene.

## Validation Plan

- Focused architecture test:
  - `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_first_runtime_rule_closeout_v541_v548_must_remain_closeout_only_without_second_mobility_authority"`
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
- Passed: `dotnet test tests/Zongzu.Architecture.Tests/Zongzu.Architecture.Tests.csproj --no-restore --filter "Household_mobility_first_runtime_rule_closeout_v541_v548_must_remain_closeout_only_without_second_mobility_authority"`
- Passed: `dotnet build Zongzu.sln --no-restore`.
- Passed: `git diff --check`.
- Passed: touched-file replacement-character scan.
- Passed: `dotnet test Zongzu.sln --no-build`.

PR validation:
- Pending: PR CI.

## Rollback Path

Remove the V541-V548 docs and architecture guard. No production code or save/schema rollback is required.
