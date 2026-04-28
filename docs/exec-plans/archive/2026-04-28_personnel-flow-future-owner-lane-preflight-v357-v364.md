# Personnel Flow Future Owner-Lane Preflight V357-V364

## Intent

V357-V364 is a preflight guard for future personnel-flow owner lanes after the v325-v356 gate/readback layer.

The goal is to make it explicit that `FamilyCore`, `OfficeAndCareer`, and `WarfareCampaign` personnel-flow lanes are not opened by the current gate. Each future lane must bring its own owner module, state/rule boundary, command shape, target scope, cardinality, deterministic order/cap, schema impact, and validation plan before implementation.

## Non-Goals

- No new command.
- No movement resolver.
- No migration economy.
- No office-service, kin-transfer, assignment, return, summon, or campaign-manpower rule.
- No new `PersonnelFlow`, `SocialMobility`, or `Migration` module.
- No command/movement/personnel/assignment/focus/scheduler/future-owner-lane ledger.
- No Application movement authority.
- No UI/Unity authority.
- No prose parsing of `DomainEvent.Summary`, command summaries, receipt prose, notification prose, mobility text, public-life lines, or docs text.

## Required Future Lane Contract

Any later personnel-flow owner lane must declare:

- owner module and accepted command;
- target scope and no-touch boundary;
- hot path and expected cardinality;
- deterministic ordering and cap;
- state/schema impact or explicit no-save rationale;
- same-month vs next-month cadence;
- projection/readback fields;
- focused integration, architecture, and presentation validation.

## Target Schema / Migration

Target schema/migration impact: none.

If implementation requires persisted state, a module schema bump, a save manifest change, a projection cache, or a migration step, stop and write the schema impact before editing code.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- Focused architecture preflight test.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Completion Notes

- Added a future owner-lane preflight guard after the v325-v356 personnel-flow gate layer.
- Documented that Family/Office/Warfare personnel-flow lanes still require separate owner-module command/rule/state/schema/validation plans.
- Added architecture guard proving no direct personnel command, no movement resolver, no future-owner-lane ledger, no schema drift, no UI/Application authority, and no `PersonRegistry` expansion.
- Updated topology, audit, boundary, integration, data schema, schema namespace, simulation, simulation fidelity, UI, acceptance, and player-scope docs.
- Schema/migration impact: none.
- Validation completed:
  - `dotnet build Zongzu.sln --no-restore`
  - `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --filter Personnel_flow_future_owner_lane_preflight_v357_v364`
  - `git diff --check`
  - `dotnet test Zongzu.sln --no-build`
