---
name: zongzu-simulation-validation
description: "Use when a task explicitly asks to validate Zongzu simulation behavior, determinism, replay hashes, scheduler cadence, event flow, pressure saturation, long-run health, migration behavior, performance diagnostics, or acceptance tests, or when runtime/save/pressure-chain changes need proof. Do not use for doc-only or formatting-only edits unless validation is requested."
---

# Zongzu Simulation Validation

## Overview

Use this skill to prove that Zongzu still behaves like a deterministic living-world simulation after code, schema, pressure-chain, or presentation-adjacent changes.

The goal is not just "tests pass." The goal is to know whether the world cadence, event graph, save compatibility, and long-run pressure bands still make sense.

Use it as a diagnostic or finishing pass, not as a ritual on every task. For documentation-only, skill-only, formatting-only, or comment-only edits, say why runtime validation is unnecessary and use the narrowest relevant check.

## Current Repo Anchors

Useful current validation surfaces include:
- `tests/Zongzu.Integration.Tests/M2LiteIntegrationTests/TenYearSimulationHealthCheckTests.cs` for 120-month campaign sandbox diagnostics
- `_healthcheck.log` when present as the latest captured diagnostic output, but rerun when the task needs fresh evidence
- `MonthlyScheduler` for prepare/xun/month/event-drain/projection ordering
- `ModuleRunner<TState>` metadata for event, cadence, command, schema, and namespace contracts
- `tests/Zongzu.Presentation.Unity.Tests` for presentation adapter/ViewModel work
- public-life/order validation on the current branch should cover Order accepted/partial/refused command tests, response-lane owner tests, SocialMemory next-month residue tests, PopulationAndHouseholds local-response tests, schema/migration lanes for `OrderAndBanditry` schema `9`, `OfficeAndCareer` schema `7`, `FamilyCore` schema `8`, and `PopulationAndHouseholds` schema `3`, architecture guards for module-owned command seams, and Unity projection-copy tests
- current v19-v380 follow-up, owner-lane, Office/Family/Force/Warfare/Court readback, directive/aftermath docket readback, court-policy local-response/SocialMemory/public-reading/public-follow-up/docket/suggested-action/suggested-receipt/receipt-docket/public-life-receipt echo guard, social mobility / fidelity-ring / influence readbacks, regime-legitimacy readback, personnel-flow readiness/gate/future-lane preflight guards, and closeout validation should prove projection-only derivation where claimed, owner-module mutation where claimed, no SocialMemory prose parsing, no receipt or `DomainEvent.Summary` parsing, no schema bump, no command outcome calculation in Application/UI/Unity, no follow-up/owner-lane/Office/Family/Force/Warfare/Court/policy/cooldown/docket/suggested-action/receipt/receipt-docket/public-life-receipt/social-mobility/regime/personnel/future-lane ledger, and focused integration/presentation assertions before any full-solution test
- current v32-v34 event-contract validation should prove classification coverage, no `Unclassified` debt in the ten-year diagnostic, structured `owner=<module>` / `evidence=<doc-or-test>` readback, and no use of diagnostic output as runtime authority
- current v35-v100 validation should prove each owner lane separately: canal-window has Trade/Order authority consumers, household burden reaches only sponsor `FamilyCore` pressure, Family relief mutates only `FamilyCore`, Office implementation/readback stays Office-owned, Office/Family/Force/Warfare/Court lane closure is projection-only unless a named owner command says otherwise, structured metadata/query snapshots beat prose, off-scope entities stay untouched, and no schema/migration changed
- current v101-v108 validation is audit evidence: it must distinguish thin-chain skeleton completion from future full-chain rule-density debt and must not be used as runtime performance or gameplay completion proof
- current v109-v380 validation splits by branch: v109-v204 is narrow Chain 8 court-policy rule-density proof over structured Office/PublicLife/SocialMemory snapshots; v213-v292 is social mobility / fidelity-ring / influence proof over existing PopulationAndHouseholds and PersonRegistry snapshots; v253-v268 is regime-legitimacy readback proof over existing world/office/public-life snapshots; v293-v380 is personnel-flow readiness/gate/future-lane preflight proof that blocks direct move/transfer/summon/assign pressure while keeping future Family/Office/Warfare lanes as separately planned owner lanes. Each branch must prove off-scope non-inheritance, schema neutrality, no summary/prose parsing, and Unity/ViewModel copy-only behavior when presentation is touched
- performance validation is evidence-driven: use focused tests for behavior, long-run diagnostics for pressure health, replay/hash checks for determinism, and `dotnet-counters` or profiler output only when CPU, allocation, GC, memory, exception rate, or frame cost is the actual risk

## External Calibration Anchors

- Apply Microsoft Learn unit-test guidance as a Zongzu rule of thumb: tests should be fast, isolated, repeatable, self-checking, and focused on one behavioral claim.
- Use `dotnet-counters` / .NET diagnostics for first-level runtime evidence when CPU, GC, allocation rate, exception rate, memory, or long-run health is the risk.
- Prefer high-performance logging patterns only for hot diagnostics; source-generated logging does not replace DomainEvent metadata, receipts, or projections.
- Use Unity Profiler / Memory Profiler only for Unity shell implementation claims. Pure ViewModel/adapter changes usually need presentation tests, not editor profiling.
- Accessibility validation should check whether projected meaning survives contrast, focus/read order, semantic labels, and narration, not whether the shell looks like a generic app.

When health diagnostics report events with no authority consumer or declared events that never appear, classify them before changing code: projection-only receipt, dormant/future contract, seeded-path gap, acceptance-test gap, or alignment bug.

## Use This Skill When

- runtime rules, scheduler cadence, command resolution, or cross-module pressure behavior changed
- save/migration compatibility or replay determinism changed
- a task claims long-run pressure health, performance improvement, or algorithmic scalability
- event-contract diagnostics, no-touch proof, or off-scope entities are part of done
- Unity shell performance is claimed with actual Unity implementation changes
- a docs/skill-only pass needs an explicit "no runtime/performance validation required" note

## Fast Lane

For docs, skills, copy, comments, or projection wording with no runtime behavior change, use `git diff --check` plus targeted text/encoding checks and record no runtime validation. Use focused tests before full solution tests; use counters/profilers only when runtime or frame cost is the real risk.

## Workflow

1. Scope the validation lane first.

   Decide whether the task needs:
   - no runtime validation
   - a targeted module/test-project check
   - save/migration compatibility checks
   - replay/determinism checks
   - long-run health diagnostics
   - performance/counter/profiler diagnostics for hot-path or frame-risk changes
   - full `dotnet test Zongzu.sln --no-restore`

   Prefer the smallest lane that can prove the claim. Escalate only when runtime authority, save shape, scheduler cadence, command resolution, or cross-module pressure changed.

2. Read only the validation baseline needed.

   Start with:
   - the active ExecPlan for the touched work, if present
   - `docs/ACCEPTANCE_TESTS.md` for acceptance expectations
   - `docs/SIMULATION.md` for scheduler/cadence questions
   - `docs/MODULE_INTEGRATION_RULES.md` for event-flow and chain questions
   - `docs/SCHEMA_NAMESPACE_RULES.md` / `docs/DATA_SCHEMA.md` only when save or schema is involved

   For save or schema work, also use `zongzu-save-and-schema`.
   For Renzong pressure chains, also use `zongzu-pressure-chain`.

3. Confirm current implementation facts before judging.

   Inspect the live code and tests relevant to the change:
   - `MonthlyScheduler` for prepare/xun/month/projection ordering and bounded event drain
   - `ModuleRunner<TState>` metadata: phase, execution order, cadence, state namespace, schema version, events, commands
   - `SimulationBootstrapper` feature-pack module sets and manifests
   - `GameSimulation` command routing, save/load, replay hash refresh, and query registry setup
   - relevant module tests and integration tests
   - `tools/Zongzu.MvpPreviewRunner` when a long-run or preview diagnosis is involved

4. Run the selected validation lane.

   Determinism/replay:
   - run the smallest targeted determinism test first if one exists
   - run `dotnet test Zongzu.sln --no-restore` for broad confirmation
   - compare replay hashes only for the same seed, manifest, module set, save input, and command sequence
   - treat changed hashes as expected only when rules, state, migrations, or command resolution intentionally changed

   Event flow/observability:
   - compare `PublishedEvents` and `ConsumedEvents` against emitted events in tests or diagnostics
   - classify emitted-but-unconsumed or declared-but-never-emitted events as projection-only receipt, future contract, dormant source, or alignment bug
   - fail or escalate when the current diagnostic lane reports `Unclassified` event-contract debt
   - do not treat `DomainEvent.Summary` parsing as a valid authority path

   Pressure health:
   - inspect whether pressure reaches stable bands, oscillates, saturates, or dead-ends
   - distinguish intended historical stress from formula debt
   - name the owning module, pressure locus, and no-touch boundary before proposing balance changes
   - for top-band values such as `Security=0`, `BanditThreat=100`, `DisorderPressure=100`, or `MigrationPressure=100`, prefer recovery/dampening/edge/recurring-demand analysis over simply lowering event deltas

   Performance health:
   - name the hot path and expected cardinality before changing algorithms
   - measure baseline before claiming faster, lower-allocation, or lower-frame-cost behavior
   - for .NET runtime checks, prefer `dotnet-counters`/built-in runtime metrics for first-level CPU, GC, allocation rate, memory, and exception signals
   - for Unity shell implementation, prefer Profiler/Memory Profiler/frame evidence when tooling is configured; do not require it for pure ViewModel or doc changes
   - keep replay inputs fixed when comparing performance runs so a changed workload is not mistaken for an optimization
   - for scheduler or projection algorithm changes, record touched counts, sort/cap strategy, deterministic tie-breaks, and why a one-pass index or bounded queue is enough
   - for long-run saturation, distinguish intended historical stress, missing recovery, missing allocation, over-broad fanout, and missing projection before changing formulas

   Save/migration:
   - verify root save version, module schema version, manifest membership, migration path, and round-trip tests
   - do not call a migration safe until legacy and current envelopes are both covered by tests

5. Report the result as evidence, not vibes.

   A useful validation result includes:
   - command(s) run and pass/fail status
   - touched module set and manifest path
   - determinism/save impact
   - event-flow classification
   - any long-run saturation or diagnostic debt
   - whether docs/ExecPlan need an update

## Output Rules

- Do not claim "validated" from a compile alone when scheduler, save, replay, or pressure behavior changed.
- Do not flatten all high pressure into a bug; decide whether it is intended stress, missing recovery, missing allocation, or missing projection.
- Do not change balance knobs without owner, expected band, source pressure, recovery path, and tests.
- Do not ignore unconsumed events; classify them.
- Do not claim a performance improvement without naming baseline, workload, hot path, and counter/test evidence.
- Prefer focused tests first, then the full solution test.
- Preserve deterministic inputs when comparing runs.
- Do not run long health checks just to make a doc or skill edit feel heavier.
- For doc-only, skill-only, and wording-only edits, record `no runtime/performance validation required`.
