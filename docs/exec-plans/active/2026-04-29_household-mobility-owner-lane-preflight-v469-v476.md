# Household Mobility Owner-Lane Preflight V469-V476

## Intent

V469-V476 records the owner-lane preflight for the next possible household mobility rule-density layer.

This is the step after the v453-v460 explanation and v461-v468 closeout. It does not implement movement, migration economy, route history, class/status drift, durable residue, or new per-person simulation. It pins what a future implementation must name before changing runtime rules: owner state, cadence, target scope, hot path, touched counts, deterministic cap/order, no-touch boundary, schema impact, projection fields, and validation.

## Baseline

- `main` at `1238c7f Align skill pack with v468 codebase`.
- V453-V460 projects `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, and `HouseholdMobilityDynamicsSummary` from existing household social-pressure signals.
- V461-V468 closes that layer as first-layer explanation only.
- Current production code needs no behavior change for this pass.

## Scope

- Document that `PopulationAndHouseholds` remains the first owner lane for future household mobility depth because it already owns household livelihood, activity, distress, debt, labor, grain, land, migration pressure, and pool carriers.
- Preserve the scale rule: near detail, far summary. Player-near and pressure-hit households can become more readable in a future owner-laned rule; distant society remains pool/settlement pressure summary until explicitly promoted by a separate plan.
- Add an architecture guard proving this preflight cannot be reused as a hidden movement command, migration economy, route-history model, selector, durable residue path, class/status engine, `PersonRegistry` expansion, or UI/Unity authority.
- Update topology, social-strata, boundary, integration, schema, simulation, UI, acceptance, fidelity-budget, design-audit, and skill-matrix docs.

## Non-Goals

- No production rule change.
- No migration economy.
- No route-history model.
- No direct movement command.
- No household relocation, move, transfer, summon, assign, or migration command.
- No household-mobility, movement, route-history, selector, precision-band, status, class, owner-lane, or preflight ledger.
- No durable movement residue or same-month SocialMemory write.
- No commoner status route, class ladder, zhuhu/kehu conversion, office-service lane, or trade-attachment lane.
- No selector watermark, target-cardinality state, regional person selector, global person scan, world population manager, mobility engine, movement engine, class engine, or status engine.
- No persisted state, schema bump, migration, save manifest change, module namespace, projection cache, or feature-pack save membership.
- No Application, UI, or Unity authority.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No parsing of `DomainEvent.Summary`, `MobilityDynamicsExplanationSummary`, `MobilityDynamicsDimensionKeys`, `HouseholdMobilityDynamicsSummary`, person dossier prose, social-position text, source-key display, scale-budget text, notification prose, receipt prose, mobility text, public-life lines, or docs text.

## Future Rule Gate

A later implementation may only deepen household mobility when it declares:

- owning module and owned state, with `PopulationAndHouseholds` as the default first lane unless an ExecPlan proves another owner;
- cadence: monthly, xun-band, command-time, or later SocialMemory cadence;
- target scope: named close-orbit actors, local households, active-region pools, or distant summaries;
- hot path and expected cardinality: households, people, settlements, pools, events, notices, or Unity rows touched;
- deterministic order/cap and stable tie-break;
- no-touch boundary for quiet households, off-scope settlements, distant summaries, and unrelated modules;
- schema/migration impact, including the stop point if persisted route history, selector state, residue, or target-cardinality state is needed;
- projection/readback fields and the surface that will copy them;
- validation lane: focused owner test, integration/no-touch proof, architecture guard, presentation copy-only proof, and broader tests only when runtime behavior changes.

## Target Schema / Migration

Target schema/migration impact: none.

This pass is docs/tests preflight only. If future work stores mobility histories, route histories, selector watermarks, target-cardinality state, durable SocialMemory movement residue, commoner status drift, zhuhu/kehu conversion, projection caches, or new module state, stop and write a separate owner-module schema/migration plan first.

## Determinism / Performance

- No runtime behavior, scheduler cadence, event flow, command route, save/load path, projection algorithm, Unity adapter, or content loader changes.
- No new hot path, global scan, cache, selector, or Unity frame work.
- The preflight records future performance requirements: hot path, touched counts, deterministic cap/order, cadence, and no-touch proof must be declared before implementation.

## Validation Plan

- Focused architecture guard.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- scan touched files for replacement characters
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests preflight commit. No save migration or production data rollback is required.

## Milestones

- [x] Add preflight ExecPlan.
- [x] Add docs and architecture guard.
- [x] Run validation lane.

## Completion Evidence

- Added v469-v476 household mobility owner-lane preflight evidence across topology, social strata, design audit, module boundaries, integration rules, schema rules, data schema, simulation, UI/presentation, acceptance, fidelity model, and skill matrix docs.
- Added `Household_mobility_owner_lane_preflight_v469_v476_must_gate_future_rule_depth_without_runtime_authority` to prove the preflight remains docs/tests governance and cannot be used as movement, route-history, selector, status/class, durable-residue, Application/UI/Unity, manager/controller, prose-parser, or `PersonRegistry` authority.
- Production rule code unchanged.
- Schema / migration impact: none. No persisted state, schema version, migration, save manifest, module namespace, projection cache, selector state, route-history state, durable residue, or module payload changed.

Validation completed on 2026-04-29:

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Household_mobility_owner_lane_preflight_v469_v476_must_gate_future_rule_depth_without_runtime_authority"` passed.
- `dotnet build Zongzu.sln --no-restore` passed with 0 warnings and 0 errors.
- `git diff --check` passed.
- Touched-file replacement-character scan passed.
- `dotnet test Zongzu.sln --no-build` passed.
