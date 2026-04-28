# Commoner Social Position Preflight V381-V388

## Intent

V381-V388 is a preflight guard for future commoner / social-position mobility work.

The goal is to document how Zongzu can deepen commoner social drift without turning the current social-mobility substrate into a full class engine, a direct promotion button, or a global per-person career simulation.

## Current Substrate

- `PopulationAndHouseholds` already owns household livelihood, labor/activity pressure, distress, and migration/mobility pools.
- `PersonRegistry` owns identity and `FidelityRing` only.
- `EducationAndExams`, `TradeAndIndustry`, `OfficeAndCareer`, `FamilyCore`, `PublicLifeAndRumor`, and `SocialMemoryAndRelations` already expose adjacent social-pressure carriers, but they do not yet own a complete commoner class-mobility route.
- Current readbacks prove near detail / far summary, not a complete status ladder.

## Required Future Lane Contract

Any later commoner social-position lane must declare:

- owner module and accepted command or monthly rule;
- social-position pressure carrier, such as livelihood, debt, literacy, trade attachment, yamen document contact, lineage support, public reputation, or durable memory;
- target scope and no-touch boundary;
- hot path and expected cardinality;
- deterministic order/cap;
- schema impact or explicit no-save rationale;
- same-month vs next-month cadence;
- projection fields and readback fields;
- focused integration, architecture, presentation, and save/schema validation.

## Non-Goals

- No complete class engine.
- No direct promote/demote commoner command.
- No zhuhu/kehu conversion state.
- No office-service, clerk, merchant, tenant, artisan, or gentry promotion rule.
- No new `SocialClass`, `CommonerMobility`, `Strata`, or `Migration` module.
- No class/social-position/personnel/movement/focus/scheduler ledger.
- No Application social-class authority.
- No UI/Unity authority.
- No `PersonRegistry` expansion beyond identity and `FidelityRing`.
- No prose parsing of `DomainEvent.Summary`, command summaries, receipt prose, notification prose, person dossier text, mobility text, public-life lines, docs text, or social-position labels.

## Target Schema / Migration

Target schema/migration impact: none.

If implementation requires persisted state, a module schema bump, a save manifest change, a projection cache, or a migration step, stop and write the schema impact before editing code.

## Validation Plan

- `dotnet build Zongzu.sln --no-restore`
- Focused architecture preflight test.
- `git diff --check`
- `dotnet test Zongzu.sln --no-build`

## Milestones

- [x] Create the preflight ExecPlan.
- [x] Document the commoner/social-position future-lane contract.
- [x] Add focused architecture guard.
- [x] Run validation lane.

## Completion Notes

- Schema/migration impact target: none.
- Runtime impact target: none; this pass is docs/tests preflight only.
- `dotnet test tests\Zongzu.Architecture.Tests\Zongzu.Architecture.Tests.csproj --no-restore --filter "Name=Commoner_social_position_preflight_v381_v388_must_document_future_lane_without_class_engine_or_schema_drift"` passed.
- `dotnet build Zongzu.sln --no-restore` passed.
- `git diff --check` passed.
- `dotnet test Zongzu.sln --no-build` passed.
