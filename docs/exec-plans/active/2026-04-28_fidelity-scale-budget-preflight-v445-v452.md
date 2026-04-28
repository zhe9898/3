# Fidelity Scale Budget Preflight V445-V452

## Intent

V445-V452 turns the product rule "近处细，远处汇总；玩家身边活，天下不逐人硬算" into an explicit architecture preflight after the v437-v444 commoner status owner-lane pass. The short English engineering shorthand is: near detail, far summary.

This is a performance and authority boundary guard, not a runtime feature. It does not implement commoner status drift, raise fidelity rings, scan the whole world person-by-person, or add a selector. It records the future rule requirement: detailed named-person simulation must be pressure-selected, owner-laned, deterministic, capped, and schema-planned before code lands.

## Baseline

- `main` at `0611509 Preflight commoner status owner lane`.
- V381-V444 closed the first social-position visibility and future `PopulationAndHouseholds` owner-lane preflight.
- Current code already has `PersonRegistry.FidelityRing` as an identity/readability scale marker and keeps household livelihood/activity/pool pressure in `PopulationAndHouseholds`.

## Scope

- Add docs and architecture guard evidence for the near-detail / far-summary scale budget.
- Make future social/commoner/personnel depth declare target scope: named close-orbit actors, local households, active-region pools, or distant summaries.
- Prove this pass is docs/tests only and does not create global per-person simulation authority.

## Non-Goals

- No new production rule.
- No fidelity-ring mutation.
- No target selector, promotion pass, or regional person browser.
- No full society engine, class engine, commoner status engine, movement engine, or personnel engine.
- No global person scan, all-world per-person career simulation, or scheduler sweep.
- No new module such as `SocialClass`, `CommonerStatus`, `SocialPosition`, `PopulationScaleBudget`, or `WorldPopulationManager`.
- No scale-budget, fidelity-budget, commoner-status, social-position, movement, selector, scheduler, or owner-lane ledger.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application, UI, or Unity authority.
- No parsing of `DomainEvent.Summary`, person dossier prose, social-position text, source-key display, scale-budget text, notification prose, receipt prose, mobility text, public-life lines, or docs text.

## Target Schema / Migration

Target schema/migration impact: none.

This pass is docs/tests preflight only. If future work stores status drift, precision bands, selection watermarks, durable residue, pool membership, route history, or projection caches, stop and write a separate schema/migration plan first.

## Determinism / Performance

- No runtime behavior, scheduler cadence, event flow, command route, save/load path, or projection algorithm changes.
- Future work must name the hot path, expected touched counts, deterministic ordering, cap/watermark/cadence, cache invalidation, save/schema impact, and validation lane before changing target cardinality.
- Distant society remains represented through settlement/household pools and pressure carriers until a specific owner lane promotes bounded local detail.

## Validation Plan

- Focused architecture guard.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests preflight commit. No save migration, production data rollback, or schema rollback is required.

## Milestones

- [x] Add ExecPlan.
- [x] Add docs and architecture scale-budget guard.
- [x] Run final validation lane.

## Completion Evidence

- Docs now record v445-v452 in the topology index, design/code audit, module boundaries, integration rules, schema docs, simulation docs, UI/presentation docs, acceptance tests, social strata/pathways, simulation fidelity model, and skill rationalization matrix.
- The preflight names the scale rule directly: near detail, far summary. Future social/commoner/personnel depth must be pressure-selected, owner-laned, deterministic, capped, and schema-planned before it raises fidelity or target cardinality.
- `PersonRegistry` remains identity and existing `FidelityRing` only. This pass adds no precision-band state, selector state, target-cardinality state, global person scanner, regional person browser, class engine, commoner status engine, movement engine, or manager/controller.
- Production rule code is unchanged. The architecture guard proves this preflight adds no runtime selector, no all-world per-person simulation, no new status module, no ledger/schema, no prose parser, and no Application/UI/Unity authority drift.
- Schema/migration impact: none. No persisted fields, module schema versions, root save version, migrations, manifests, or save membership changed.

Validation run:

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Fidelity_scale_budget_preflight_v445_v452_must_keep_near_detail_far_summary_without_global_person_scan"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`
