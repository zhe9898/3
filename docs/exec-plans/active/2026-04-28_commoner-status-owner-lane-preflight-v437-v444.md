# Commoner Status Owner-Lane Preflight V437-V444

## Intent

V437-V444 prepares the next possible depth step after the v381-v436 social-position visibility layer.

The preflight does not implement status drift. It records the recommended first owner lane for future commoner status rules: `PopulationAndHouseholds`, because the existing implemented substrate already owns household livelihood, activity, migration pressure, labor pools, distress, debt, land, grain, and settlement pressure carriers.

This keeps future commoner / class-position depth from drifting into `PersonRegistry`, Application, UI, Unity, or a standalone social-class module.

## Baseline

- `main` at `e022280 Close social position scale readback layer`.
- V381-V436 closed first-layer social-position visibility only.
- Current implemented facts: `PopulationAndHouseholds` owns livelihood/activity/pools; `PersonRegistry` owns identity and existing `FidelityRing` only; Application builds projections; Unity copies projected fields.

## Scope

- Add docs and architecture guard evidence for the future owner-lane recommendation.
- State the future rule requirements before implementation: owner state, cadence, target scope, no-touch boundary, hot path, expected cardinality, deterministic cap/order, schema impact, projection fields, and validation.
- Keep code behavior unchanged.

## Non-Goals

- No new production rule.
- No command, monthly rule, resolver, or event path.
- No full class engine.
- No promote/demote command.
- No zhuhu/kehu conversion state.
- No office-service, trade-attachment, clerk, artisan, merchant, tenant, or gentry route.
- No class / social-position / commoner-status / personnel / movement / owner-lane / preflight ledger.
- No `PopulationAndHouseholds` schema change in this pass.
- No `PersonRegistry` expansion beyond identity and existing `FidelityRing`.
- No Application, UI, or Unity authority.
- No new module such as `SocialClass`, `CommonerMobility`, `CommonerStatus`, `SocialPosition`, or `Strata`.
- No parsing of `DomainEvent.Summary`, person dossier prose, social-position text, source-key display, scale-budget text, notification prose, receipt prose, mobility text, public-life lines, or docs text.

## Target Schema / Migration

Target schema/migration impact: none.

This pass is docs/tests preflight only. If future work adds persisted household status drift, route history, conversion state, precision state, durable residue, selection state, or projection caches, stop and write a separate `PopulationAndHouseholds` schema/migration plan first.

## Determinism / Performance

- No runtime behavior, scheduler cadence, event flow, command route, save/load path, or projection algorithm changes.
- Future work must declare hot path and expected cardinality before it scans households, members, pools, or settlements.
- Future selection rules must be deterministic and capped.

## Validation Plan

- Focused architecture guard.
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Rollback

Revert the docs/tests preflight commit. No save migration or production data rollback is required.

## Milestones

- [x] Add ExecPlan.
- [x] Add docs and architecture preflight guard.
- [x] Run final validation lane.

## Completion Evidence

- Docs now record v437-v444 as a commoner status owner-lane preflight in the topology index, design/code audit, module boundaries, integration rules, schema docs, simulation docs, UI/presentation docs, acceptance tests, social strata/pathways, simulation fidelity model, and skill rationalization matrix.
- The preflight recommends `PopulationAndHouseholds` as the future first owner lane because the current code substrate already owns household livelihood, activity, migration pressure, labor pools, debt, land, grain, and settlement pressure carriers.
- `PersonRegistry` remains identity and existing `FidelityRing` only; this pass adds no commoner status, social class, office-service, trade-attachment, or durable social-position residue state there.
- Production rule code is unchanged. The architecture guard forbids new promote/demote commands, status/class ledgers, class engines, new status modules, forbidden manager/controller drift, prose parsing, and Application/UI/Unity authority drift for this preflight.
- Schema/migration impact: none. No persisted fields, module schema versions, root save version, migrations, manifests, or save membership changed.

Validation run:

- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Commoner_status_owner_lane_preflight_v437_v444_must_point_to_population_without_new_status_authority"`
- `dotnet build Zongzu.sln --no-restore`
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`
